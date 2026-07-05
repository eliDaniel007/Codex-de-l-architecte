using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Menu pause de la MainScene : petit bouton ☰ en haut à droite (et touche
/// Échap) qui met le jeu en pause et ouvre un menu Reprendre / Recommencer /
/// Quitter. Singleton créé par GameState ; visible uniquement dans la MainScene.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    public static PauseMenu Instance { get; private set; }

    private Canvas     _canvas;
    private GameObject _bouton;     // petit bouton ☰
    private GameObject _overlay;    // menu pause (caché par défaut)
    private bool       _enPause;
    private readonly List<MonoBehaviour> _gelees = new List<MonoBehaviour>();

    public static void Ensure()
    {
        if (Instance == null)
        {
            var go = new GameObject("[PauseMenu]");
            go.AddComponent<PauseMenu>();
        }
    }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildUI();
        SceneManager.sceneLoaded += OnSceneLoaded;
        AppliquerVisibilite(SceneManager.GetActiveScene().name);
    }

    void OnDestroy()
    {
        if (Instance == this) SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Changement de scène = on sort de pause (sécurité) et on ajuste la visibilité.
        if (_enPause) Reprendre();
        AppliquerVisibilite(scene.name);
    }

    void AppliquerVisibilite(string sceneName)
    {
        bool main = (sceneName == GameState.I.mainSceneName);
        _canvas.enabled = main;
        if (!main && _enPause) Reprendre();
    }

    void Update()
    {
        // La pause n'existe que dans la MainScene (les autres scènes ont [Échap] = Retour).
        if (SceneManager.GetActiveScene().name != GameState.I.mainSceneName) return;
        // Pas pendant la cinématique, le rating ou le journal (eux gèrent leurs touches).
        if (!_enPause && (BriefingCinematic.EnCours || RatingScreen.Visible || JournalMissions.EstOuvert)) return;

#if ENABLE_INPUT_SYSTEM
        bool echap = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#else
        bool echap = Input.GetKeyDown(KeyCode.Escape);
#endif
        if (echap) Basculer();
    }

    // ── pause ──────────────────────────────────────────────────────────────

    public void Basculer() { if (_enPause) Reprendre(); else Pause(); }

    public void Pause()
    {
        if (_enPause) return;
        _enPause = true;
        _overlay.SetActive(true);
        _bouton.SetActive(false);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
        GelerJoueur(true);
    }

    public void Reprendre()
    {
        _enPause = false;
        if (_overlay != null) _overlay.SetActive(false);
        if (_bouton  != null) _bouton.SetActive(true);
        Time.timeScale = 1f;
        if (SceneManager.GetActiveScene().name == GameState.I.mainSceneName)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
        }
        GelerJoueur(false);
    }

    void Recommencer()
    {
        Reprendre();
        GameState.I.ReinitialiserCampagne();
    }

    void Quitter()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>Active/désactive les contrôleurs du joueur (déplacement + caméra).</summary>
    void GelerJoueur(bool geler)
    {
        if (geler)
        {
            _gelees.Clear();
            var pg = GameObject.FindGameObjectWithTag("Player");
            if (pg == null) return;
            foreach (var mb in pg.GetComponentsInChildren<MonoBehaviour>())
            {
                if (mb == null) continue;
                string n = mb.GetType().Name;
                if (n.Contains("ThirdPersonController") || n.Contains("StarterAssetsInputs") ||
                    n.Contains("PlayerInput"))
                {
                    if (mb.enabled) { mb.enabled = false; _gelees.Add(mb); }
                }
            }
        }
        else
        {
            foreach (var mb in _gelees) if (mb != null) mb.enabled = true;
            _gelees.Clear();
        }
    }

    // ── UI ────────────────────────────────────────────────────────────────

    void BuildUI()
    {
        _canvas = gameObject.AddComponent<Canvas>();
        _canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 120;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        gameObject.AddComponent<GraphicRaycaster>();
        EnsureEventSystem();

        // Petit bouton ☰ en haut à droite
        _bouton = new GameObject("BoutonMenu");
        _bouton.transform.SetParent(transform, false);
        var bImg = _bouton.AddComponent<Image>();
        bImg.color = new Color(0f, 0.04f, 0.12f, 0.85f);
        _bouton.AddComponent<Button>().onClick.AddListener(Pause);
        var br = _bouton.GetComponent<RectTransform>();
        br.anchorMin = new Vector2(1f, 1f); br.anchorMax = new Vector2(1f, 1f);
        br.pivot     = new Vector2(1f, 1f);
        br.anchoredPosition = new Vector2(-18f, -18f);
        br.sizeDelta = new Vector2(150f, 56f);
        AjouterLabel(_bouton.transform, "☰  Menu", 26f, new Color(0f, 0.85f, 1f));

        // Overlay du menu pause (caché)
        _overlay = new GameObject("Overlay");
        _overlay.transform.SetParent(transform, false);
        var oImg = _overlay.AddComponent<Image>();
        oImg.color = new Color(0f, 0.01f, 0.05f, 0.92f);
        var or = _overlay.GetComponent<RectTransform>();
        or.anchorMin = Vector2.zero; or.anchorMax = Vector2.one;
        or.offsetMin = or.offsetMax = Vector2.zero;

        AjouterTitre(_overlay.transform, "PAUSE",
            new Vector2(0.2f, 0.66f), new Vector2(0.8f, 0.8f), 80f);

        Bouton(_overlay.transform, "REPRENDRE",   new Vector2(0.38f, 0.52f), new Vector2(0.62f, 0.60f),
            new Color(0.1f, 0.35f, 0.18f), Reprendre);
        Bouton(_overlay.transform, "RECOMMENCER", new Vector2(0.38f, 0.42f), new Vector2(0.62f, 0.50f),
            new Color(0.30f, 0.22f, 0.08f), Recommencer);
        Bouton(_overlay.transform, "QUITTER",     new Vector2(0.38f, 0.32f), new Vector2(0.62f, 0.40f),
            new Color(0.35f, 0.12f, 0.12f), Quitter);

        AjouterTitre(_overlay.transform, "<size=60%>[Échap] pour reprendre</size>",
            new Vector2(0.2f, 0.22f), new Vector2(0.8f, 0.28f), 28f);

        _overlay.SetActive(false);
    }

    void AjouterLabel(Transform parent, string texte, float taille, Color couleur)
    {
        var go = new GameObject("Label");
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = texte; tmp.fontSize = taille; tmp.color = couleur;
        tmp.alignment = TextAlignmentOptions.Center; tmp.fontStyle = FontStyles.Bold;
        tmp.raycastTarget = false;
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
        r.offsetMin = r.offsetMax = Vector2.zero;
    }

    void AjouterTitre(Transform parent, string texte, Vector2 ancMin, Vector2 ancMax, float taille)
    {
        var go = new GameObject("Titre");
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = texte; tmp.fontSize = taille; tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center; tmp.fontStyle = FontStyles.Bold;
        tmp.richText = true; tmp.raycastTarget = false;
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = ancMin; r.anchorMax = ancMax;
        r.offsetMin = r.offsetMax = Vector2.zero;
    }

    void Bouton(Transform parent, string label, Vector2 ancMin, Vector2 ancMax, Color couleur, System.Action onClick)
    {
        var go = new GameObject("Btn_" + label);
        go.transform.SetParent(parent, false);
        go.AddComponent<Image>().color = couleur;
        go.AddComponent<Button>().onClick.AddListener(() => onClick());
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = ancMin; r.anchorMax = ancMax;
        r.offsetMin = r.offsetMax = Vector2.zero;
        AjouterLabel(go.transform, label, 28f, Color.white);
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
