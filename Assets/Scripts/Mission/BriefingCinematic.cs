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
    private readonly System.Collections.Generic.List<MonoBehaviour> _geles
        = new System.Collections.Generic.List<MonoBehaviour>();

    /// <summary>Vrai pendant la cinématique (pour bloquer le menu pause).</summary>
    public static bool EnCours { get; private set; }

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
        if (scene.name != GameState.I.mainSceneName)
        {
            // Une autre scène se charge pendant la cinématique → on l'interrompt
            // proprement (les objets de la MainScene sont détruits).
            if (EnCours) Interrompre();
            return;
        }

        if (_dejaJouee) return;
        if (EcranTitre.Visible) return; // le titre d'abord : il nous lancera à sa fermeture
        if (CampagneDejaCommencee) { _dejaJouee = true; return; } // REPRENDRE ≠ recommencer

        // Automatique au lancement d'une NOUVELLE campagne (une fois par session).
        _dejaJouee = true;
        StartCoroutine(Jouer());
    }

    /// <summary>Vrai si le joueur a déjà commencé la campagne (sauvegarde en cours) :
    /// en REPRENANT sa partie, il ne doit pas revoir la cinématique d'intro.</summary>
    static bool CampagneDejaCommencee =>
        GameState.I.missionRevelee >= 0 || GameState.I.questIndex > 0;

    /// <summary>Lance la cinématique après la fermeture de l'écran titre.</summary>
    public static void LancerApresTitre()
    {
        if (Instance == null || _dejaJouee) return;
        if (SceneManager.GetActiveScene().name != GameState.I.mainSceneName) return;
        if (CampagneDejaCommencee) { _dejaJouee = true; return; } // reprise : pas d'intro
        _dejaJouee = true;
        Instance.StartCoroutine(Instance.Jouer());
    }

    /// <summary>Arrête la cinématique et nettoie (changement de scène inattendu).</summary>
    void Interrompre()
    {
        StopAllCoroutines();
        if (_overlay != null) Destroy(_overlay);
        _overlay = null;
        _cam     = null;
        _geles.Clear(); // les composants gelés appartenaient à la scène détruite
        MusiqueOuverture.ArreterEnFondu(1f);
        EnCours  = false;
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

    /// <summary>Le clavier « principal » : le plus éloigné du portail RAM.</summary>
    static Transform TrouverClavierPrincipal()
    {
        var claviers = FindObjectsByType<KeyboardTerminal>(FindObjectsSortMode.None);
        if (claviers.Length == 0) return null;

        var ram = FindFirstObjectByType<LoadSceneOnPlayerEnter>();
        if (ram == null) return claviers[0].transform;

        Transform meilleur = claviers[0].transform;
        float best = -1f;
        foreach (var k in claviers)
        {
            float d = Vector3.Distance(k.transform.position, ram.transform.position);
            if (d > best) { best = d; meilleur = k.transform; }
        }
        return meilleur;
    }

    IEnumerator Jouer()
    {
        EnCours = true;

        // Bande-son : la musique d'ouverture continue (ou démarre), et la radio
        // lance son briefing PENDANT le survol.
        MusiqueOuverture.Jouer();
        VoiceOver.AnnoncerOuverture();

        yield return null; // laisse la scène s'initialiser

        var mainCam = Camera.main;
        if (mainCam == null) yield break;

        var joueur = GameObject.FindGameObjectWithTag("Player");
        Vector3 posJoueur = joueur != null ? joueur.transform.position : mainCam.transform.position;

        // Le joueur est GELÉ pendant la cinématique : sinon il peut marcher dans
        // une zone (CPU, RAM...) qui change de scène et détruit nos caméras.
        GelerJoueur(joueur, true);

        // Étapes du survol : (nom affiché, cible)
        var etapes = new System.Collections.Generic.List<(string nom, Transform cible)>();
        var cpu     = GameObject.Find("Processeur");
        var clavier = TrouverClavierPrincipal();
        var portail = FindFirstObjectByType<LoadSceneOnPlayerEnter>();
        var ecran   = FindFirstObjectByType<ConsoleScreen>();
        if (cpu     != null) etapes.Add(("CPU — lit le programme et calcule",     cpu.transform));
        if (clavier != null) etapes.Add(("CLAVIER — le terminal de saisie",       clavier));
        if (portail != null) etapes.Add(("RAM — déclare et range tes variables",  portail.transform));
        if (ecran   != null) etapes.Add(("ÉCRAN — Console : affichage et saisie", ecran.transform));
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
            if (camGO == null || cible == null) break; // scène changée / objet détruit
            if (_labelStation != null) _labelStation.text = nom;

            Vector3 sommet = SommetDe(cible);
            Vector3 recul  = (posJoueur - sommet); recul.y = 0f;
            recul = recul.sqrMagnitude < 0.01f ? Vector3.back : recul.normalized;
            Vector3 destination = sommet + recul * 7f + Vector3.up * 3f;

            Vector3    dep = camGO.transform.position;
            Quaternion rot = camGO.transform.rotation;
            float t = 0f;
            while (t < 1f && !_skip)
            {
                if (camGO == null) break; // détruit par un chargement de scène
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

        // Retour vers la vue du joueur (si tout existe encore).
        if (!_skip && camGO != null && mainCam != null)
        {
            Vector3 dep = camGO.transform.position;
            Quaternion rotDep = camGO.transform.rotation;
            if (_labelStation != null) _labelStation.text = "À toi de jouer, agent.";
            float t = 0f;
            while (t < 1f && !_skip)
            {
                if (camGO == null || mainCam == null) break; // scène changée en plein vol
                t += Time.deltaTime / 1.4f;
                float k = Mathf.SmoothStep(0f, 1f, t);
                camGO.transform.position = Vector3.Lerp(dep, mainCam.transform.position, k);
                camGO.transform.rotation = Quaternion.Slerp(rotDep, mainCam.transform.rotation, k);
                yield return null;
            }
        }

        if (camGO    != null) Destroy(camGO);
        if (_overlay != null) Destroy(_overlay);
        _cam = null;
        GelerJoueur(null, false); // rend les commandes au joueur
        MusiqueOuverture.ArreterEnFondu(2.5f); // la musique s'éteint, place au jeu
        EnCours = false;
    }

    /// <summary>Active/désactive les contrôleurs du joueur pendant la cinématique.</summary>
    void GelerJoueur(GameObject joueur, bool geler)
    {
        if (geler)
        {
            _geles.Clear();
            if (joueur == null) return;
            foreach (var mb in joueur.GetComponentsInChildren<MonoBehaviour>())
            {
                if (mb == null) continue;
                string n = mb.GetType().Name;
                if (n.Contains("ThirdPersonController") || n.Contains("StarterAssetsInputs") ||
                    n.Contains("PlayerInput"))
                {
                    if (mb.enabled) { mb.enabled = false; _geles.Add(mb); }
                }
            }
        }
        else
        {
            foreach (var mb in _geles) if (mb != null) mb.enabled = true;
            _geles.Clear();
        }
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
