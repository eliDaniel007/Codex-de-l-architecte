using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Formulaire de déclaration de variable DANS la scène RAM :
/// choisir le type (int / float / string / bool), taper le nom et la valeur,
/// puis SAUVEGARDER → la variable occupe une case (une boîte apparaît).
/// Créé par RAMSceneController. Bouton « + DÉCLARER » ou touche [N].
/// </summary>
public class RamDeclarationUI : MonoBehaviour
{
    /// <summary>Vrai quand le panneau est ouvert (bloque [Échap] retour de la scène).</summary>
    public static bool PanneauOuvert { get; private set; }

    public static RamDeclarationUI Instance { get; private set; }

    /// <summary>Ouvre le formulaire avec un type présélectionné (clic sur une boîte de type).</summary>
    public static void OuvrirPourType(string type)
    {
        if (Instance == null) return;
        if (GameState.I.BriefingEnAttente()) return; // briefing d'abord (message géré au clic)
        Instance.ChoisirType(type);
        Instance.Ouvrir();
    }

    private Canvas          _canvas;
    private GameObject      _bouton;    // bouton « + DÉCLARER »
    private GameObject      _panneau;   // formulaire
    private TMP_InputField  _nom;
    private TMP_InputField  _valeur;
    private TextMeshProUGUI _message;
    private string          _type = "int";
    private readonly System.Collections.Generic.Dictionary<string, Image> _typeBtns
        = new System.Collections.Generic.Dictionary<string, Image>();

    void Awake()  { Instance = this; }

    void Start()  { BuildUI(); }

    void OnDestroy()
    {
        PanneauOuvert = false;
        if (Instance == this) Instance = null;
    }

    void Update()
    {
        // Briefing obligatoire : pas de déclaration tant que la ligne n'est pas lue.
        if (!PanneauOuvert && GameState.I.BriefingEnAttente()) return;

#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb == null) return;
        if (!PanneauOuvert && kb.nKey.wasPressedThisFrame) Ouvrir();
        else if (PanneauOuvert && kb.escapeKey.wasPressedThisFrame) Fermer();
        else if (PanneauOuvert && (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame)) Sauvegarder();
#else
        if (!PanneauOuvert && Input.GetKeyDown(KeyCode.N)) Ouvrir();
        else if (PanneauOuvert && Input.GetKeyDown(KeyCode.Escape)) Fermer();
        else if (PanneauOuvert && Input.GetKeyDown(KeyCode.Return)) Sauvegarder();
#endif
    }

    void Ouvrir()
    {
        PanneauOuvert = true;
        _panneau.SetActive(true);
        _bouton.SetActive(false);
        _message.text = "";
        Cursor.visible = true;
        if (_nom != null) _nom.ActivateInputField();
    }

    void Fermer()
    {
        PanneauOuvert = false;
        _panneau.SetActive(false);
        _bouton.SetActive(true);
        Cursor.visible = false; // on rend la main-curseur de la RAM
    }

    void Sauvegarder()
    {
        string err = GameState.I.DeclarerEnRam(_type, _nom.text, _valeur.text);
        if (err == null)
        {
            // Succès : on recharge la scène RAM pour voir la nouvelle boîte.
            PanneauOuvert = false;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            AudioFX.Erreur();
            _message.text = err;
        }
    }

    void ChoisirType(string type)
    {
        _type = type;
        foreach (var kv in _typeBtns)
            kv.Value.color = kv.Key == type
                ? new Color(0f, 0.55f, 0.75f)
                : new Color(0.08f, 0.14f, 0.25f);
    }

    // ── UI ────────────────────────────────────────────────────────────────

    void BuildUI()
    {
        _canvas = gameObject.AddComponent<Canvas>();
        _canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 90;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        gameObject.AddComponent<GraphicRaycaster>();
        EnsureEventSystem();

        // Bouton « + DÉCLARER » (en bas au centre)
        _bouton = new GameObject("BtnDeclarer");
        _bouton.transform.SetParent(transform, false);
        var bImg = _bouton.AddComponent<Image>();
        bImg.color = new Color(0f, 0.35f, 0.5f, 0.95f);
        _bouton.AddComponent<Button>().onClick.AddListener(Ouvrir);
        var br = _bouton.GetComponent<RectTransform>();
        br.anchorMin = new Vector2(0.5f, 0f); br.anchorMax = new Vector2(0.5f, 0f);
        br.pivot = new Vector2(0.5f, 0f);
        br.anchoredPosition = new Vector2(0f, 24f);
        br.sizeDelta = new Vector2(430f, 62f);
        Label(_bouton.transform, "Clique une boîte de type pour déclarer   (ou [N])", 22f, Color.white);

        // Panneau du formulaire
        _panneau = new GameObject("Panneau");
        _panneau.transform.SetParent(transform, false);
        var pImg = _panneau.AddComponent<Image>();
        pImg.color = new Color(0.01f, 0.04f, 0.1f, 0.97f);
        var pr = _panneau.GetComponent<RectTransform>();
        pr.anchorMin = new Vector2(0.28f, 0.16f); pr.anchorMax = new Vector2(0.72f, 0.86f);
        pr.offsetMin = pr.offsetMax = Vector2.zero;

        Titre(_panneau.transform, "<color=#00D9FF>DÉCLARER UNE VARIABLE</color>",
            new Vector2(0.05f, 0.88f), new Vector2(0.95f, 0.98f), 36f);

        // Rangée de types
        Titre(_panneau.transform, "Type :", new Vector2(0.07f, 0.78f), new Vector2(0.93f, 0.85f), 24f,
            TextAlignmentOptions.MidlineLeft);
        string[] types = { "int", "float", "string", "bool" };
        for (int i = 0; i < types.Length; i++)
        {
            string t = types[i];
            float x0 = 0.07f + i * 0.22f;
            var go = new GameObject("Type_" + t);
            go.transform.SetParent(_panneau.transform, false);
            var img = go.AddComponent<Image>();
            go.AddComponent<Button>().onClick.AddListener(() => ChoisirType(t));
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = new Vector2(x0, 0.68f); r.anchorMax = new Vector2(x0 + 0.20f, 0.77f);
            r.offsetMin = r.offsetMax = Vector2.zero;
            var c = GameState.CouleurType(t);
            Label(go.transform, $"<color=#{ColorUtility.ToHtmlStringRGB(c)}>{t}</color>", 26f, Color.white);
            _typeBtns[t] = img;
        }
        ChoisirType("int");

        // Champs nom / valeur
        Titre(_panneau.transform, "Nom :", new Vector2(0.07f, 0.57f), new Vector2(0.93f, 0.64f), 24f,
            TextAlignmentOptions.MidlineLeft);
        _nom = Champ(_panneau.transform, "ex : x", new Vector2(0.07f, 0.47f), new Vector2(0.93f, 0.56f));

        Titre(_panneau.transform, "Valeur :", new Vector2(0.07f, 0.37f), new Vector2(0.93f, 0.44f), 24f,
            TextAlignmentOptions.MidlineLeft);
        _valeur = Champ(_panneau.transform, "ex : 4", new Vector2(0.07f, 0.27f), new Vector2(0.93f, 0.36f));

        // Message d'erreur
        var msgGO = new GameObject("Message");
        msgGO.transform.SetParent(_panneau.transform, false);
        _message = msgGO.AddComponent<TextMeshProUGUI>();
        _message.fontSize = 22f; _message.color = new Color(1f, 0.45f, 0.45f);
        _message.alignment = TextAlignmentOptions.Center; _message.richText = true;
        var mr = msgGO.GetComponent<RectTransform>();
        mr.anchorMin = new Vector2(0.05f, 0.19f); mr.anchorMax = new Vector2(0.95f, 0.26f);
        mr.offsetMin = mr.offsetMax = Vector2.zero;

        // Boutons Sauvegarder / Annuler
        Bouton(_panneau.transform, "SAUVEGARDER  [Entrée]", new Vector2(0.12f, 0.06f), new Vector2(0.58f, 0.16f),
            new Color(0.1f, 0.38f, 0.2f), Sauvegarder);
        Bouton(_panneau.transform, "ANNULER", new Vector2(0.62f, 0.06f), new Vector2(0.88f, 0.16f),
            new Color(0.32f, 0.13f, 0.13f), Fermer);

        _panneau.SetActive(false);
    }

    void Label(Transform parent, string texte, float taille, Color couleur)
    {
        var go = new GameObject("Label");
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = texte; tmp.fontSize = taille; tmp.color = couleur;
        tmp.alignment = TextAlignmentOptions.Center; tmp.fontStyle = FontStyles.Bold;
        tmp.richText = true; tmp.raycastTarget = false;
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
        r.offsetMin = r.offsetMax = Vector2.zero;
    }

    void Titre(Transform parent, string texte, Vector2 min, Vector2 max, float taille,
               TextAlignmentOptions align = TextAlignmentOptions.Center)
    {
        var go = new GameObject("Titre");
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = texte; tmp.fontSize = taille; tmp.color = Color.white;
        tmp.alignment = align; tmp.fontStyle = FontStyles.Bold; tmp.richText = true;
        tmp.raycastTarget = false;
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = min; r.anchorMax = max;
        r.offsetMin = r.offsetMax = Vector2.zero;
    }

    TMP_InputField Champ(Transform parent, string placeholder, Vector2 min, Vector2 max)
    {
        var go = new GameObject("Champ");
        go.transform.SetParent(parent, false);
        go.AddComponent<Image>().color = new Color(0.05f, 0.11f, 0.22f, 1f);
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = min; r.anchorMax = max;
        r.offsetMin = r.offsetMax = Vector2.zero;
        var input = go.AddComponent<TMP_InputField>();

        var area = new GameObject("TextArea", typeof(RectTransform));
        area.transform.SetParent(go.transform, false);
        var art = area.GetComponent<RectTransform>();
        art.anchorMin = Vector2.zero; art.anchorMax = Vector2.one;
        art.offsetMin = new Vector2(14f, 6f); art.offsetMax = new Vector2(-14f, -6f);
        area.AddComponent<RectMask2D>();

        var ph = new GameObject("Placeholder");
        ph.transform.SetParent(area.transform, false);
        var phT = ph.AddComponent<TextMeshProUGUI>();
        phT.text = placeholder; phT.fontSize = 27f; phT.fontStyle = FontStyles.Italic;
        phT.color = new Color(1f, 1f, 1f, 0.3f); phT.alignment = TextAlignmentOptions.MidlineLeft;
        phT.rectTransform.anchorMin = Vector2.zero; phT.rectTransform.anchorMax = Vector2.one;
        phT.rectTransform.offsetMin = phT.rectTransform.offsetMax = Vector2.zero;

        var tx = new GameObject("Text");
        tx.transform.SetParent(area.transform, false);
        var txT = tx.AddComponent<TextMeshProUGUI>();
        txT.fontSize = 27f; txT.color = Color.white;
        txT.alignment = TextAlignmentOptions.MidlineLeft;
        txT.rectTransform.anchorMin = Vector2.zero; txT.rectTransform.anchorMax = Vector2.one;
        txT.rectTransform.offsetMin = txT.rectTransform.offsetMax = Vector2.zero;

        input.textViewport  = art;
        input.textComponent = txT;
        input.placeholder   = phT;
        return input;
    }

    void Bouton(Transform parent, string label, Vector2 min, Vector2 max, Color couleur, System.Action onClick)
    {
        var go = new GameObject("Btn_" + label);
        go.transform.SetParent(parent, false);
        go.AddComponent<Image>().color = couleur;
        go.AddComponent<Button>().onClick.AddListener(() => onClick());
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = min; r.anchorMax = max;
        r.offsetMin = r.offsetMax = Vector2.zero;
        Label(go.transform, label, 24f, Color.white);
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
