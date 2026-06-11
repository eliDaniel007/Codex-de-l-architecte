using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Écran de fin de campagne façon Hitman : rang, temps total, erreurs.
///   [Entrée] / CONTINUER : ferme l'écran (libre de se balader)
///   [R] / RECOMMENCER    : efface la sauvegarde et relance la campagne
/// Apparaît automatiquement quand la dernière mission est validée.
/// </summary>
public class RatingScreen : MonoBehaviour
{
    public static RatingScreen Instance { get; private set; }

    private Canvas _canvas;
    private bool   _visible;

    public static void Afficher()
    {
        if (Instance == null)
        {
            var go = new GameObject("[RatingScreen]");
            go.AddComponent<RatingScreen>();
        }
        Instance.StartCoroutine(Instance.AfficherApresDelai(3.2f)); // laisse la voix « fin » démarrer
    }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    IEnumerator AfficherApresDelai(float delai)
    {
        yield return new WaitForSeconds(delai);
        ConstruireUI();
        _visible = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
    }

    void Update()
    {
        if (!_visible) return;
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb == null) return;
        if (kb.enterKey.wasPressedThisFrame || kb.escapeKey.wasPressedThisFrame) Fermer();
        if (kb.rKey.wasPressedThisFrame) Recommencer();
#else
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Escape)) Fermer();
        if (Input.GetKeyDown(KeyCode.R)) Recommencer();
#endif
    }

    void Fermer()
    {
        _visible = false;
        if (_canvas != null) Destroy(_canvas.gameObject);
        _canvas = null;
    }

    void Recommencer()
    {
        Fermer();
        GameState.I.ReinitialiserCampagne();
    }

    // ── rang ──────────────────────────────────────────────────────────────

    static (string titre, string sousTitre, Color couleur) CalculerRang(int erreurs, float temps)
    {
        if (erreurs == 0 && temps < 10f * 60f)
            return ("ARCHITECTE D'ÉLITE", "Sans faute. Le CPU s'incline.", new Color(1f, 0.84f, 0.2f));
        if (erreurs <= 2)
            return ("AGENT CONFIRMÉ", "Du travail de professionnel.", new Color(0f, 0.85f, 1f));
        if (erreurs <= 5)
            return ("APPRENTI PROMETTEUR", "Encore quelques bugs à corriger.", new Color(0.55f, 0.85f, 0.55f));
        return ("STAGIAIRE DU CODE", "Le compilateur se souviendra de toi.", new Color(0.8f, 0.6f, 0.45f));
    }

    // ── UI ────────────────────────────────────────────────────────────────

    void ConstruireUI()
    {
        if (_canvas != null) Destroy(_canvas.gameObject);

        var gs = GameState.I;
        float temps   = gs.TempsCampagne;
        int   erreurs = gs.nbErreurs;
        var (rang, sousRang, couleurRang) = CalculerRang(erreurs, temps);

        var canvasGO = new GameObject("RatingUI");
        canvasGO.transform.SetParent(transform, false);
        _canvas = canvasGO.AddComponent<Canvas>();
        _canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 200;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();
        EnsureEventSystem();

        // Fond sombre
        var fond = new GameObject("Fond");
        fond.transform.SetParent(canvasGO.transform, false);
        var fondImg = fond.AddComponent<Image>();
        fondImg.color = new Color(0f, 0.01f, 0.05f, 0.93f);
        Etirer(fond);

        Txt(canvasGO.transform, "CAMPAGNE TERMINÉE",
            new Vector2(0.1f, 0.80f), new Vector2(0.9f, 0.88f), 40f, new Color(0.7f, 0.78f, 0.9f), FontStyles.Bold);

        Txt(canvasGO.transform, rang,
            new Vector2(0.05f, 0.62f), new Vector2(0.95f, 0.78f), 84f, couleurRang, FontStyles.Bold);

        Txt(canvasGO.transform, sousRang,
            new Vector2(0.1f, 0.555f), new Vector2(0.9f, 0.62f), 30f, new Color(0.8f, 0.85f, 0.95f), FontStyles.Italic);

        int min = (int)(temps / 60f), sec = (int)(temps % 60f);
        Txt(canvasGO.transform,
            $"Temps de campagne : <color=#00D9FF>{min:00}:{sec:00}</color>      Erreurs : <color=#FF8A8A>{erreurs}</color>      Missions : <color=#73D973>{gs.quests.Count}/{gs.quests.Count}</color>",
            new Vector2(0.05f, 0.42f), new Vector2(0.95f, 0.52f), 32f, Color.white, FontStyles.Normal);

        Bouton(canvasGO.transform, "CONTINUER  [Entrée]", new Vector2(0.30f, 0.22f), new Vector2(0.48f, 0.30f),
            new Color(0.1f, 0.35f, 0.15f), Fermer);
        Bouton(canvasGO.transform, "RECOMMENCER  [R]", new Vector2(0.52f, 0.22f), new Vector2(0.70f, 0.30f),
            new Color(0.35f, 0.2f, 0.08f), Recommencer);
    }

    static void Etirer(GameObject go)
    {
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
        r.offsetMin = r.offsetMax = Vector2.zero;
    }

    void Txt(Transform parent, string texte, Vector2 ancMin, Vector2 ancMax, float taille, Color couleur, FontStyles style)
    {
        var go = new GameObject("Txt");
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = texte; tmp.fontSize = taille; tmp.color = couleur;
        tmp.alignment = TextAlignmentOptions.Center; tmp.richText = true; tmp.fontStyle = style;
        tmp.raycastTarget = false;
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
        var txtGO = new GameObject("Label");
        txtGO.transform.SetParent(go.transform, false);
        var tmp = txtGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = 26f; tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        var tr = txtGO.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
        tr.offsetMin = tr.offsetMax = Vector2.zero;
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
