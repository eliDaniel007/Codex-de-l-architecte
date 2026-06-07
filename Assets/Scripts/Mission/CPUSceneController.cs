using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Contrôleur de la scène CPU.
/// Affiche la liste des objectifs (quêtes) et met en avant la quête active.
/// Le joueur lit ses objectifs puis revient à la MainScene (Échap ou bouton Retour).
/// Toute l'UI est construite par code (comme ClavierSceneController).
/// </summary>
public class CPUSceneController : MonoBehaviour
{
    [Header("Couleurs")]
    public Color colorActive   = new Color(0f, 0.85f, 1f);       // Cyan : quête active
    public Color colorDone      = new Color(0.45f, 0.85f, 0.45f); // Vert : terminée
    public Color colorPending   = new Color(0.55f, 0.6f, 0.7f);   // Gris : à venir

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        ConstruireUI();
    }

    void Update()
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb != null && (kb.escapeKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame))
            Retour();
#else
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return))
            Retour();
#endif
    }

    void Retour()
    {
        GameState.I.cpuJustVisited = true;
        SceneManager.LoadScene(GameState.I.mainSceneName);
    }

    // ── UI ────────────────────────────────────────────────────────────────

    void ConstruireUI()
    {
        var gs = GameState.I;
        gs.InitQuests();

        var canvasGO = new GameObject("CPU_UI");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        EnsureEventSystem();

        // Fond
        var fondGO = new GameObject("Fond");
        fondGO.transform.SetParent(canvasGO.transform, false);
        var fondImg = fondGO.AddComponent<Image>();
        fondImg.color = new Color(0.01f, 0.03f, 0.07f, 0.98f);
        var fr = fondGO.GetComponent<RectTransform>();
        fr.anchorMin = Vector2.zero; fr.anchorMax = Vector2.one;
        fr.offsetMin = fr.offsetMax = Vector2.zero;

        // Titre
        AjouterTexte(canvasGO.transform, "<color=#00D9FF>CPU</color>  —  OBJECTIFS",
            new Vector2(0.1f, 0.85f), new Vector2(0.9f, 0.95f), 54f, Color.white,
            TextAlignmentOptions.Center, FontStyles.Bold);

        // Sous-titre : quête active
        var active = gs.QueteActuelle();
        string indice = (active != null && active.kind == QuestKind.Question)
            ? "  <size=70%><color=#FFD27F>(réponds au clavier)</color></size>"
            : "";
        string sousTitre = active != null
            ? $"Objectif actuel : <color=#00D9FF>{active.description}</color>{indice}"
            : "<color=#73D973>Tous les objectifs sont terminés !</color>";
        AjouterTexte(canvasGO.transform, sousTitre,
            new Vector2(0.1f, 0.75f), new Vector2(0.9f, 0.83f), 28f, new Color(0.85f, 0.9f, 1f),
            TextAlignmentOptions.Center, FontStyles.Normal);

        // Liste des quêtes
        ConstruireListe(canvasGO.transform, gs);

        // Bouton retour + aide
        AjouterTexte(canvasGO.transform, "Appuie sur [Échap] ou [Entrée] pour revenir",
            new Vector2(0.1f, 0.03f), new Vector2(0.9f, 0.09f), 22f, new Color(0.6f, 0.65f, 0.75f),
            TextAlignmentOptions.Center, FontStyles.Italic);

        CreerBouton(canvasGO.transform, "RETOUR", new Vector2(0.42f, 0.1f), new Vector2(0.58f, 0.17f),
            new Color(0.1f, 0.3f, 0.4f), Retour);
    }

    void ConstruireListe(Transform parent, GameState gs)
    {
        // Conteneur vertical centré
        var listGO = new GameObject("ListeQuetes");
        listGO.transform.SetParent(parent, false);
        var lr = listGO.AddComponent<RectTransform>();
        lr.anchorMin = new Vector2(0.18f, 0.22f);
        lr.anchorMax = new Vector2(0.82f, 0.72f);
        lr.offsetMin = lr.offsetMax = Vector2.zero;

        var vlg = listGO.AddComponent<VerticalLayoutGroup>();
        vlg.spacing            = 14f;
        vlg.childAlignment     = TextAnchor.UpperLeft;
        vlg.childControlHeight  = true;
        vlg.childControlWidth   = true;
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth  = true;

        for (int i = 0; i < gs.quests.Count; i++)
        {
            var q = gs.quests[i];
            bool estActive = (i == gs.questIndex) && !q.complete;

            string puce;
            Color  couleur;
            if (q.complete)      { puce = "<color=#73D973>[x]</color>"; couleur = colorDone; }
            else if (estActive)  { puce = "<color=#00D9FF>></color>";   couleur = colorActive; }
            else                 { puce = "<color=#8C99B3>-</color>";   couleur = colorPending; }

            string ligne = $"{puce}  <b>{q.titre}</b>\n<size=80%>     {q.description}</size>";
            AjouterLigneQuete(listGO.transform, ligne, couleur, estActive);
        }
    }

    void AjouterLigneQuete(Transform parent, string texte, Color couleur, bool surligne)
    {
        var go = new GameObject("Quete");
        go.transform.SetParent(parent, false);

        var bg = go.AddComponent<Image>();
        bg.color = surligne ? new Color(0f, 0.85f, 1f, 0.12f) : new Color(1f, 1f, 1f, 0.03f);

        var le = go.AddComponent<LayoutElement>();
        le.minHeight = 64f;

        var txtGO = new GameObject("Txt");
        txtGO.transform.SetParent(go.transform, false);
        var tmp = txtGO.AddComponent<TextMeshProUGUI>();
        tmp.text      = texte;
        tmp.fontSize  = 26f;
        tmp.color     = couleur;
        tmp.richText  = true;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        var tr = txtGO.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
        tr.offsetMin = new Vector2(20f, 6f); tr.offsetMax = new Vector2(-20f, -6f);
    }

    // ── helpers UI ────────────────────────────────────────────────────────

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

    void AjouterTexte(Transform parent, string texte, Vector2 ancMin, Vector2 ancMax, float taille,
                      Color couleur, TextAlignmentOptions align, FontStyles style)
    {
        var go = new GameObject("Txt");
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = texte; tmp.fontSize = taille; tmp.color = couleur;
        tmp.alignment = align; tmp.richText = true; tmp.fontStyle = style;
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = ancMin; r.anchorMax = ancMax;
        r.offsetMin = r.offsetMax = Vector2.zero;
    }

    void CreerBouton(Transform parent, string label, Vector2 ancMin, Vector2 ancMax, Color couleur, System.Action onClick)
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
}
