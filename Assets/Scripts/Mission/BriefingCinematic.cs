using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Cinématique de briefing : à chaque lancement du jeu, la caméra survole les
/// 4 stations (CPU, Clavier, RAM, Écran) avec bandes cinéma et nom de la
/// station, pendant que la radio briefe. [Espace] pour passer.
/// Jouée une seule fois par session de jeu.
/// </summary>
public class BriefingCinematic : MonoBehaviour
{
    public static BriefingCinematic Instance { get; private set; }

    [Tooltip("Durée du déplacement vers chaque station (s).")]
    public float dureeDeplacement = 2.2f;
    [Tooltip("Pause sur chaque station (s).")]
    public float dureePause = 1.0f;

    private static bool _dejaJouee;
    private Camera      _cam;
    private GameObject  _overlay;
    private TextMeshProUGUI _labelStation;
    private bool        _skip;

    public static void Ensure()
    {
        if (Instance == null)
        {
            var go = new GameObject("[BriefingCinematic]");
            go.AddComponent<BriefingCinematic>();
        }
    }

    /// <summary>Autorise la cinématique à rejouer (nouvelle campagne).</summary>
    public static void Reinitialiser()
    {
        _dejaJouee = false;
    }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    void OnDestroy()
    {
        if (Instance == this) SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (_dejaJouee) return;
        if (scene.name != GameState.I.mainSceneName) return;

        // Automatique à chaque lancement du jeu (une fois par session).
        _dejaJouee = true;
        StartCoroutine(Jouer());
    }

    void Update()
    {
        if (_cam == null) return;
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) _skip = true;
#else
        if (Input.GetKeyDown(KeyCode.Space)) _skip = true;
#endif
    }

    IEnumerator Jouer()
    {
        yield return null; // laisse la scène s'initialiser

        var mainCam = Camera.main;
        if (mainCam == null) yield break;

        var joueur = GameObject.FindGameObjectWithTag("Player");
        Vector3 posJoueur = joueur != null ? joueur.transform.position : mainCam.transform.position;

        // Étapes du survol : (nom affiché, cible)
        var etapes = new System.Collections.Generic.List<(string nom, Transform cible)>();
        var cpu     = GameObject.Find("Processeur");
        var clavier = FindFirstObjectByType<KeyboardTerminal>();
        var portail = FindFirstObjectByType<LoadSceneOnPlayerEnter>();
        var ecran   = FindFirstObjectByType<ConsoleScreen>();
        if (cpu     != null) etapes.Add(("CPU — le centre des objectifs",  cpu.transform));
        if (clavier != null) etapes.Add(("CLAVIER — déclare tes variables", clavier.transform));
        if (portail != null) etapes.Add(("RAM — la mémoire de stockage",   portail.transform));
        if (ecran   != null) etapes.Add(("ÉCRAN — affiche tes résultats",  ecran.transform));
        if (etapes.Count == 0) yield break;

        // Caméra de cinématique (au-dessus de la caméra joueur).
        var camGO = new GameObject("CineCam");
        _cam = camGO.AddComponent<Camera>();
        _cam.depth = mainCam.depth + 10;
        camGO.transform.SetPositionAndRotation(mainCam.transform.position, mainCam.transform.rotation);

        ConstruireOverlay();

        foreach (var (nom, cible) in etapes)
        {
            if (_skip) break;
            _labelStation.text = nom;

            Vector3 sommet = SommetDe(cible);
            Vector3 recul  = (posJoueur - sommet); recul.y = 0f;
            recul = recul.sqrMagnitude < 0.01f ? Vector3.back : recul.normalized;
            Vector3 destination = sommet + recul * 7f + Vector3.up * 3f;

            Vector3    dep = camGO.transform.position;
            Quaternion rot = camGO.transform.rotation;
            float t = 0f;
            while (t < 1f && !_skip)
            {
                t += Time.deltaTime / dureeDeplacement;
                float k = Mathf.SmoothStep(0f, 1f, t);
                camGO.transform.position = Vector3.Lerp(dep, destination, k);
                var versCible = Quaternion.LookRotation((sommet - camGO.transform.position).normalized);
                camGO.transform.rotation = Quaternion.Slerp(rot, versCible, k);
                yield return null;
            }

            float pause = 0f;
            while (pause < dureePause && !_skip) { pause += Time.deltaTime; yield return null; }
        }

        // Retour vers la vue du joueur.
        if (!_skip)
        {
            Vector3 dep = camGO.transform.position;
            Quaternion rotDep = camGO.transform.rotation;
            _labelStation.text = "À toi de jouer, agent.";
            float t = 0f;
            while (t < 1f && !_skip)
            {
                t += Time.deltaTime / 1.4f;
                float k = Mathf.SmoothStep(0f, 1f, t);
                camGO.transform.position = Vector3.Lerp(dep, mainCam.transform.position, k);
                camGO.transform.rotation = Quaternion.Slerp(rotDep, mainCam.transform.rotation, k);
                yield return null;
            }
        }

        Destroy(camGO);
        Destroy(_overlay);
        _cam = null;
    }

    // ── overlay cinéma ────────────────────────────────────────────────────

    void ConstruireOverlay()
    {
        _overlay = new GameObject("CineOverlay");
        _overlay.transform.SetParent(transform, false);
        var canvas = _overlay.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 150;
        var scaler = _overlay.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        Bande(new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f));   // haut
        Bande(new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f));   // bas

        var labelGO = new GameObject("Station");
        labelGO.transform.SetParent(_overlay.transform, false);
        _labelStation = labelGO.AddComponent<TextMeshProUGUI>();
        _labelStation.fontSize  = 40f;
        _labelStation.color     = new Color(0f, 0.85f, 1f);
        _labelStation.fontStyle = FontStyles.Bold;
        _labelStation.alignment = TextAlignmentOptions.Center;
        var lr = labelGO.GetComponent<RectTransform>();
        lr.anchorMin = new Vector2(0.1f, 0.045f);
        lr.anchorMax = new Vector2(0.9f, 0.125f);
        lr.offsetMin = lr.offsetMax = Vector2.zero;

        var skipGO = new GameObject("Skip");
        skipGO.transform.SetParent(_overlay.transform, false);
        var skip = skipGO.AddComponent<TextMeshProUGUI>();
        skip.text      = "[ESPACE] passer";
        skip.fontSize  = 22f;
        skip.color     = new Color(0.7f, 0.75f, 0.85f);
        skip.alignment = TextAlignmentOptions.Right;
        var sr = skipGO.GetComponent<RectTransform>();
        sr.anchorMin = new Vector2(0.6f, 0.9f);
        sr.anchorMax = new Vector2(0.97f, 0.97f);
        sr.offsetMin = sr.offsetMax = Vector2.zero;
    }

    void Bande(Vector2 ancMin, Vector2 ancMax, Vector2 pivot)
    {
        var go = new GameObject("Bande");
        go.transform.SetParent(_overlay.transform, false);
        var img = go.AddComponent<Image>();
        img.color = Color.black;
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = ancMin; r.anchorMax = ancMax; r.pivot = pivot;
        r.anchoredPosition = Vector2.zero;
        r.sizeDelta = new Vector2(0f, 120f);
    }

    static Vector3 SommetDe(Transform t)
    {
        var rends = t.GetComponentsInChildren<Renderer>();
        if (rends.Length == 0) return t.position;

        Bounds b = rends[0].bounds;
        for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
        return new Vector3(b.center.x, b.max.y, b.center.z);
    }
}
