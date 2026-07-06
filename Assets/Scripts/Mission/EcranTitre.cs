using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Écran titre du jeu, affiché au lancement (une fois par session) par-dessus
/// le monde 3D assombri :
///
///     LE CODEX DE L'ARCHITECTE_
///     Apprends la programmation de l'intérieur.
///
///     [ CONTINUER / JOUER ]  [ RECOMMENCER ]  [ QUITTER ]
///
/// Le joueur est figé tant que le menu est ouvert ; la cinématique de briefing
/// et la voix radio attendent la fermeture. Singleton créé par GameState.
/// </summary>
public class EcranTitre : MonoBehaviour
{
    public static EcranTitre Instance { get; private set; }

    /// <summary>Vrai tant que l'écran titre est affiché (fige le reste du jeu).</summary>
    public static bool Visible { get; private set; }

    private static bool _dejaMontre; // une seule fois par session de jeu

    private GameObject      _panneau;
    private TextMeshProUGUI _titre;
    private readonly List<MonoBehaviour> _geles = new List<MonoBehaviour>();

    // Caméra aérienne du menu : orbite lentement au-dessus de la carte mère
    // (même esprit que la cinématique de briefing).
    private Camera  _camTitre;
    private Vector3 _centreCarte;
    private float   _rayonOrbite = 30f;
    private float   _hauteurOrbite = 24f;
    private float   _angleOrbite;

    public static void Ensure()
    {
        if (Instance == null)
        {
            var go = new GameObject("[EcranTitre]");
            go.AddComponent<EcranTitre>();
        }
    }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildUI();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        if (Instance == this) SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (_dejaMontre || Visible) return;
        if (scene.name != GameState.I.mainSceneName) return;

        // IMPORTANT : Visible passe à true IMMÉDIATEMENT (même frame), pour que
        // la cinématique de briefing et la radio — qui testent ce drapeau dans
        // la même frame — attendent bien la fermeture du titre.
        Visible = true;
        _panneau.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
        CreerCameraTitre();      // vue aérienne de la carte pendant le menu
        MusiqueOuverture.Jouer(); // la bande-son d'ouverture démarre avec le titre
        StartCoroutine(GelerDiffere());
        StartCoroutine(ClignoterCurseur());
    }

    IEnumerator GelerDiffere()
    {
        yield return null; // le joueur n'existe qu'après l'initialisation de la scène
        if (Visible) GelerJoueur(true);
    }

    // ── caméra aérienne du menu ───────────────────────────────────────────

    void CreerCameraTitre()
    {
        // Centre et taille de la carte mère → orbite adaptée.
        _centreCarte = Vector3.zero;
        var sol = GameObject.Find("Ground");
        if (sol != null)
        {
            var rend = sol.GetComponentInChildren<Renderer>();
            if (rend != null)
            {
                var b = rend.bounds;
                _centreCarte   = b.center;
                _rayonOrbite   = Mathf.Max(b.size.x, b.size.z) * 0.42f;
                _hauteurOrbite = _rayonOrbite * 0.75f;
            }
        }

        var go = new GameObject("TitreCam");
        _camTitre = go.AddComponent<Camera>();
        var mainCam = Camera.main;
        _camTitre.depth = mainCam != null ? mainCam.depth + 10 : 50;

        _angleOrbite = Random.Range(0f, 360f); // angle de départ varié
        PlacerCameraTitre();
    }

    void PlacerCameraTitre()
    {
        if (_camTitre == null) return;
        float rad = _angleOrbite * Mathf.Deg2Rad;
        Vector3 pos = _centreCarte +
                      new Vector3(Mathf.Cos(rad) * _rayonOrbite, _hauteurOrbite, Mathf.Sin(rad) * _rayonOrbite);
        _camTitre.transform.position = pos;
        _camTitre.transform.LookAt(_centreCarte + Vector3.up * 1.5f);
    }

    void Update()
    {
        if (!Visible || _camTitre == null) return;
        _angleOrbite += 3.5f * Time.unscaledDeltaTime; // orbite lente et continue
        PlacerCameraTitre();
    }

    void DetruireCameraTitre()
    {
        if (_camTitre != null) Destroy(_camTitre.gameObject);
        _camTitre = null;
    }

    void Jouer()
    {
        _dejaMontre = true;
        Visible = false;
        _panneau.SetActive(false);
        DetruireCameraTitre();
        GelerJoueur(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;

        BriefingCinematic.LancerApresTitre(); // le survol des stations démarre
    }

    void Recommencer()
    {
        _dejaMontre = true;
        Visible = false;
        _panneau.SetActive(false);
        DetruireCameraTitre();
        GelerJoueur(false);
        GameState.I.ReinitialiserCampagne(); // recharge la MainScene proprement
    }

    void Quitter()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    IEnumerator ClignoterCurseur()
    {
        const string BASE = "LE CODEX DE <color=#00D9FF>L'ARCHITECTE</color>";
        while (Visible)
        {
            _titre.text = BASE + "<color=#00FF88>_</color>";
            yield return new WaitForSecondsRealtime(0.55f);
            if (!Visible) break;
            _titre.text = BASE + "<color=#00000000>_</color>";
            yield return new WaitForSecondsRealtime(0.45f);
        }
    }

    /// <summary>Fige/libère les contrôleurs du joueur.</summary>
    void GelerJoueur(bool geler)
    {
        if (geler)
        {
            _geles.Clear();
            var pg = GameObject.FindGameObjectWithTag("Player");
            if (pg == null) return;
            foreach (var mb in pg.GetComponentsInChildren<MonoBehaviour>())
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

    // ── UI ────────────────────────────────────────────────────────────────

    void BuildUI()
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 400; // au-dessus de tout
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        gameObject.AddComponent<GraphicRaycaster>();
        EnsureEventSystem();

        _panneau = new GameObject("Panneau");
        _panneau.transform.SetParent(transform, false);
        var fond = _panneau.AddComponent<Image>();
        fond.color = new Color(0f, 0.01f, 0.045f, 0.45f); // voile léger : la vue aérienne respire
        var pr = _panneau.GetComponent<RectTransform>();
        pr.anchorMin = Vector2.zero; pr.anchorMax = Vector2.one;
        pr.offsetMin = pr.offsetMax = Vector2.zero;

        // Titre
        var titreGO = new GameObject("Titre");
        titreGO.transform.SetParent(_panneau.transform, false);
        _titre = titreGO.AddComponent<TextMeshProUGUI>();
        _titre.text      = "LE CODEX DE <color=#00D9FF>L'ARCHITECTE</color>_";
        _titre.fontSize  = 84f;
        _titre.fontStyle = FontStyles.Bold;
        _titre.color     = Color.white;
        _titre.alignment = TextAlignmentOptions.Center;
        _titre.richText  = true;
        var tr = titreGO.GetComponent<RectTransform>();
        tr.anchorMin = new Vector2(0.05f, 0.62f); tr.anchorMax = new Vector2(0.95f, 0.8f);
        tr.offsetMin = tr.offsetMax = Vector2.zero;

        // Sous-titre
        var sousGO = new GameObject("SousTitre");
        sousGO.transform.SetParent(_panneau.transform, false);
        var sous = sousGO.AddComponent<TextMeshProUGUI>();
        sous.text      = "Apprends la programmation de l'intérieur de la machine.";
        sous.fontSize  = 30f;
        sous.color     = new Color(0.65f, 0.72f, 0.85f);
        sous.alignment = TextAlignmentOptions.Center;
        var sr = sousGO.GetComponent<RectTransform>();
        sr.anchorMin = new Vector2(0.1f, 0.55f); sr.anchorMax = new Vector2(0.9f, 0.62f);
        sr.offsetMin = sr.offsetMax = Vector2.zero;

        // Boutons
        bool aSauvegarde = PlayerPrefs.GetInt("cda_actif", 0) == 1;
        string labelJouer = aSauvegarde ? "CONTINUER" : "JOUER";

        Bouton(labelJouer, new Vector2(0.38f, 0.4f), new Vector2(0.62f, 0.48f),
            new Color(0.08f, 0.38f, 0.2f), Jouer);
        if (aSauvegarde)
            Bouton("RECOMMENCER LA CAMPAGNE", new Vector2(0.38f, 0.3f), new Vector2(0.62f, 0.38f),
                new Color(0.3f, 0.22f, 0.08f), Recommencer);
        Bouton("QUITTER", new Vector2(0.38f, aSauvegarde ? 0.2f : 0.3f),
            new Vector2(0.62f, aSauvegarde ? 0.28f : 0.38f),
            new Color(0.32f, 0.12f, 0.12f), Quitter);

        // Pied de page
        var piedGO = new GameObject("Pied");
        piedGO.transform.SetParent(_panneau.transform, false);
        var pied = piedGO.AddComponent<TextMeshProUGUI>();
        pied.text      = "<color=#5A6473>Un jeu pour apprendre le C# — RAM, CPU, écran : à toi de jouer.</color>";
        pied.fontSize  = 20f;
        pied.alignment = TextAlignmentOptions.Center;
        pied.richText  = true;
        var fr = piedGO.GetComponent<RectTransform>();
        fr.anchorMin = new Vector2(0.1f, 0.05f); fr.anchorMax = new Vector2(0.9f, 0.1f);
        fr.offsetMin = fr.offsetMax = Vector2.zero;

        _panneau.SetActive(false);
    }

    void Bouton(string label, Vector2 min, Vector2 max, Color couleur, System.Action onClick)
    {
        var go = new GameObject("Btn_" + label);
        go.transform.SetParent(_panneau.transform, false);
        go.AddComponent<Image>().color = couleur;
        go.AddComponent<Button>().onClick.AddListener(() => onClick());
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = min; r.anchorMax = max;
        r.offsetMin = r.offsetMax = Vector2.zero;

        var txtGO = new GameObject("Label");
        txtGO.transform.SetParent(go.transform, false);
        var tmp = txtGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = 28f; tmp.fontStyle = FontStyles.Bold;
        tmp.color = Color.white; tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        var lr = txtGO.GetComponent<RectTransform>();
        lr.anchorMin = Vector2.zero; lr.anchorMax = Vector2.one;
        lr.offsetMin = lr.offsetMax = Vector2.zero;
    }

    static void EnsureEventSystem()
    {
        if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var go = new GameObject("EventSystem");
            go.AddComponent<UnityEngine.EventSystems.EventSystem>();
            var uiType = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (uiType != null) go.AddComponent(uiType);
            else go.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
    }
}
