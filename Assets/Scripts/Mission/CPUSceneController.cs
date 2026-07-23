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

    private string _calcMessage; // message du calcul (x reçu, résultat...)

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        // Briefing : la visite au CPU révèle la mission active.
        GameState.I.RevelerMissionActuelle();

        // Box portée traitée (Parse, somme, if) — sinon tour de boucle (ligne 8).
        _calcMessage = GameState.I.CpuRecevoir() ?? GameState.I.BoucleCpu();

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

        // Liseré CYAN en haut : signature visuelle de l'unité de contrôle
        // (l'UAL, elle, est orange).
        var lisere = new GameObject("Lisere");
        lisere.transform.SetParent(canvasGO.transform, false);
        var lImg = lisere.AddComponent<Image>();
        lImg.color = new Color(0f, 0.85f, 1f, 0.95f);
        var lrr = lisere.GetComponent<RectTransform>();
        lrr.anchorMin = new Vector2(0f, 0.965f); lrr.anchorMax = new Vector2(1f, 1f);
        lrr.offsetMin = lrr.offsetMax = Vector2.zero;

        // GRANDE BANDE CYAN à gauche avec « CONTRÔLE » en vertical.
        var bande = new GameObject("BandeUnite");
        bande.transform.SetParent(canvasGO.transform, false);
        var bImg = bande.AddComponent<Image>();
        bImg.color = new Color(0f, 0.85f, 1f, 0.9f);
        var brr = bande.GetComponent<RectTransform>();
        brr.anchorMin = new Vector2(0f, 0f); brr.anchorMax = new Vector2(0.055f, 0.965f);
        brr.offsetMin = brr.offsetMax = Vector2.zero;

        var bandeTxtGO = new GameObject("BandeLabel");
        bandeTxtGO.transform.SetParent(bande.transform, false);
        var bandeTxt = bandeTxtGO.AddComponent<TextMeshProUGUI>();
        bandeTxt.text = "C O N T R Ô L E";
        bandeTxt.fontSize = 52f; bandeTxt.fontStyle = FontStyles.Bold;
        bandeTxt.color = new Color(0f, 0.1f, 0.15f);
        bandeTxt.alignment = TextAlignmentOptions.Center;
        bandeTxt.raycastTarget = false;
        var btr = bandeTxtGO.GetComponent<RectTransform>();
        btr.anchorMin = new Vector2(0.5f, 0.5f); btr.anchorMax = new Vector2(0.5f, 0.5f);
        btr.sizeDelta = new Vector2(800f, 100f);
        bandeTxtGO.transform.localRotation = Quaternion.Euler(0f, 0f, 90f); // vertical

        // Filigrane « { } » géant en fond (la signature du programme)
        var filiGO = new GameObject("Filigrane");
        filiGO.transform.SetParent(canvasGO.transform, false);
        var fili = filiGO.AddComponent<TextMeshProUGUI>();
        fili.text = "{ }";
        fili.fontSize = 550f; fili.fontStyle = FontStyles.Bold;
        fili.color = new Color(0f, 0.85f, 1f, 0.05f);
        fili.alignment = TextAlignmentOptions.Center;
        fili.raycastTarget = false;
        var ftr = filiGO.GetComponent<RectTransform>();
        ftr.anchorMin = new Vector2(0.2f, 0.05f); ftr.anchorMax = new Vector2(1f, 0.9f);
        ftr.offsetMin = ftr.offsetMax = Vector2.zero;

        // Titre : l'unité de CONTRÔLE lit le programme et pilote les missions.
        // (L'autre moitié du CPU, l'unité ARITHMÉTIQUE, fait les calculs.)
        AjouterTexte(canvasGO.transform, "<color=#00D9FF>CPU — UNITÉ DE CONTRÔLE</color>  ·  LE PROGRAMME",
            new Vector2(0.05f, 0.85f), new Vector2(0.95f, 0.95f), 56f, Color.white,
            TextAlignmentOptions.Center, FontStyles.Bold);

        // Sous-titre : ligne de code active
        var active = gs.QueteActuelle();
        string sousTitre = active != null
            ? $"Ligne en cours : <color=#00D9FF>{active.titre}</color>"
            : "<color=#73D973>Programme exécuté avec succès !</color>";
        AjouterTexte(canvasGO.transform, sousTitre,
            new Vector2(0.04f, 0.75f), new Vector2(0.96f, 0.84f), 37f, new Color(0.85f, 0.9f, 1f),
            TextAlignmentOptions.Center, FontStyles.Normal);

        // Message du calcul (x reçu / résultat...)
        if (!string.IsNullOrEmpty(_calcMessage))
            AjouterTexte(canvasGO.transform, $"<color=#00D9FF>{_calcMessage}</color>",
                new Vector2(0.04f, 0.67f), new Vector2(0.96f, 0.745f), 32f, Color.white,
                TextAlignmentOptions.Center, FontStyles.Bold);

        // Liste des quêtes
        ConstruireListe(canvasGO.transform, gs);

        // Bouton retour + aide
        AjouterTexte(canvasGO.transform, "Appuie sur [Échap] ou [Entrée] pour revenir",
            new Vector2(0.1f, 0.025f), new Vector2(0.9f, 0.09f), 25f, new Color(0.6f, 0.65f, 0.75f),
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
        lr.anchorMin = new Vector2(0.10f, 0.175f);
        lr.anchorMax = new Vector2(0.90f, 0.74f);
        lr.offsetMin = lr.offsetMax = Vector2.zero;

        var vlg = listGO.AddComponent<VerticalLayoutGroup>();
        vlg.spacing            = 10f;
        vlg.childAlignment     = TextAnchor.UpperLeft;
        vlg.childControlHeight  = true;
        vlg.childControlWidth   = true;
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth  = true;

        // Seules les missions RÉVÉLÉES sont visibles ; les suivantes sont verrouillées.
        for (int i = 0; i < gs.quests.Count; i++)
        {
            var q = gs.quests[i];

            if (i > gs.missionRevelee)
            {
                // Ligne de code encore verrouillée.
                AjouterLigneQuete(listGO.transform,
                    "<color=#5A6473>[verrouillée]</color>", colorPending, false);
                continue;
            }

            if (q.complete)
            {
                AjouterLigneQuete(listGO.transform,
                    $"<color=#73D973>[x]</color>  <b>{q.titre}</b>", colorDone, false);
            }
            else if (i == gs.questIndex)
            {
                string progression = (q.kind == QuestKind.Compteur)
                    ? $"  <color=#FFD27F>({q.compteur}/{q.objectifCompteur})</color>"
                    : "";
                string ligne = $"<color=#00D9FF>></color>  <b>{q.titre}</b>{progression}   " +
                               $"<size=80%><color=#6BBF59><i>{q.description}</i></color></size>";
                AjouterLigneQuete(listGO.transform, ligne, colorActive, true);
            }
            else
            {
                AjouterLigneQuete(listGO.transform,
                    $"<color=#8C99B3>-</color>  <b>{q.titre}</b>", colorPending, false);
            }
        }
    }

    void AjouterLigneQuete(Transform parent, string texte, Color couleur, bool surligne)
    {
        var go = new GameObject("Quete");
        go.transform.SetParent(parent, false);

        var bg = go.AddComponent<Image>();
        bg.color = surligne ? new Color(0f, 0.85f, 1f, 0.12f) : new Color(1f, 1f, 1f, 0.03f);

        var le = go.AddComponent<LayoutElement>();
        le.minHeight = surligne ? 132f : 56f;

        var txtGO = new GameObject("Txt");
        txtGO.transform.SetParent(go.transform, false);
        var tmp = txtGO.AddComponent<TextMeshProUGUI>();
        tmp.text       = texte;
        tmp.color      = couleur;
        tmp.richText   = true;
        tmp.alignment  = TextAlignmentOptions.TopLeft;
        // Auto-dimensionnement : une ligne longue rétrécit au lieu de déborder
        // sur la ligne suivante (les textes ne se mélangent plus).
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = 15f;
        tmp.fontSizeMax = 27f;
        tmp.overflowMode = TextOverflowModes.Ellipsis;
        var tr = txtGO.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
        tr.offsetMin = new Vector2(20f, 10f); tr.offsetMax = new Vector2(-20f, -10f);
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
