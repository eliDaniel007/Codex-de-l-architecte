using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Minimap façon circuit imprimé (coin bas-droit de la MainScene) :
///   • fond sombre à fine bordure cyan
///   • points colorés : CPU (cyan), RAM (violet), Écran (vert)
///   • flèche blanche = le joueur (orientée selon son regard)
///   • anneau doré clignotant = l'objectif actuel
/// Nord fixe (haut = +Z). Singleton créé par GameState.
/// </summary>
public class MiniCarte : MonoBehaviour
{
    public static MiniCarte Instance { get; private set; }

    const float TAILLE = 240f;  // côté du panneau (px de référence)
    const float MARGE  = 14f;   // marge intérieure pour les points

    private Canvas        _canvas;
    private RectTransform _panneau;
    private RectTransform _joueurUI, _cpuUI, _ramUI, _ecranUI, _objectifUI;

    private Transform _joueur, _cpu, _ram, _ecran;
    private readonly System.Collections.Generic.List<(RectTransform ui, Transform monde)> _claviers
        = new System.Collections.Generic.List<(RectTransform, Transform)>();
    private Vector2   _mondeMin, _mondeTaille; // bornes X/Z de la carte mère

    public static void Ensure()
    {
        if (Instance == null)
        {
            var go = new GameObject("[MiniCarte]");
            go.AddComponent<MiniCarte>();
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
        bool main = scene.name == GameState.I.mainSceneName;
        _canvas.enabled = main;
        if (main) TrouverReperes();
    }

    void TrouverReperes()
    {
        _joueur = _cpu = _ram = _ecran = null;

        var pg = GameObject.FindGameObjectWithTag("Player");
        if (pg != null) _joueur = pg.transform;
        var cpuGO = GameObject.Find("Processeur");
        if (cpuGO != null) _cpu = cpuGO.transform;
        var ram = FindFirstObjectByType<LoadSceneOnPlayerEnter>();
        if (ram != null) _ram = ram.transform;
        var ecran = FindFirstObjectByType<ConsoleScreen>();
        if (ecran != null) _ecran = ecran.transform;

        // Claviers : un point jaune par terminal
        foreach (var (ui, _) in _claviers) if (ui != null) Destroy(ui.gameObject);
        _claviers.Clear();
        foreach (var k in FindObjectsByType<KeyboardTerminal>(FindObjectsSortMode.None))
            _claviers.Add((Point(new Color(1f, 0.82f, 0.3f), 11f, "CLAVIER"), k.transform));

        // La flèche du joueur et l'objectif restent au-dessus des autres points.
        if (_objectifUI != null) _objectifUI.SetAsLastSibling();
        if (_joueurUI   != null) _joueurUI.SetAsLastSibling();

        // Bornes de la carte mère (bounds du sol)
        _mondeMin = new Vector2(-40f, -40f); _mondeTaille = new Vector2(80f, 80f);
        var sol = GameObject.Find("Ground");
        if (sol != null)
        {
            var rend = sol.GetComponentInChildren<Renderer>();
            if (rend != null)
            {
                var b = rend.bounds;
                _mondeMin    = new Vector2(b.min.x, b.min.z);
                _mondeTaille = new Vector2(Mathf.Max(1f, b.size.x), Mathf.Max(1f, b.size.z));
            }
        }
    }

    void Update()
    {
        if (!_canvas.enabled) return;

        Placer(_cpuUI,   _cpu);
        Placer(_ramUI,   _ram);
        Placer(_ecranUI, _ecran);
        foreach (var (ui, monde) in _claviers) Placer(ui, monde);

        // Joueur : position + orientation du regard
        if (_joueur != null)
        {
            Placer(_joueurUI, _joueur);
            _joueurUI.localRotation = Quaternion.Euler(0f, 0f, -_joueur.eulerAngles.y);
        }

        // Objectif : anneau doré clignotant sur la cible active
        var cible = ObjectiveMarker.CibleActuelle;
        bool visible = cible != null;
        _objectifUI.gameObject.SetActive(visible);
        if (visible)
        {
            Placer(_objectifUI, cible);
            float k = 0.8f + 0.35f * Mathf.Abs(Mathf.Sin(Time.time * 3.5f));
            _objectifUI.localScale = Vector3.one * k;
        }
    }

    /// <summary>Positionne un point UI à partir d'une position monde (X/Z).</summary>
    void Placer(RectTransform ui, Transform monde)
    {
        if (ui == null) return;
        if (monde == null) { ui.gameObject.SetActive(false); return; }
        ui.gameObject.SetActive(true);

        float nx = Mathf.Clamp01((monde.position.x - _mondeMin.x) / _mondeTaille.x);
        float nz = Mathf.Clamp01((monde.position.z - _mondeMin.y) / _mondeTaille.y);
        float demi = TAILLE * 0.5f - MARGE;
        ui.anchoredPosition = new Vector2((nx - 0.5f) * 2f * demi, (nz - 0.5f) * 2f * demi);
    }

    // ── UI ────────────────────────────────────────────────────────────────

    void BuildUI()
    {
        _canvas = gameObject.AddComponent<Canvas>();
        _canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 95;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // Cadre extérieur (bordure cyan) + fond sombre
        var cadreGO = new GameObject("Cadre");
        cadreGO.transform.SetParent(transform, false);
        var cadre = cadreGO.AddComponent<Image>();
        cadre.color = new Color(0f, 0.85f, 1f, 0.55f);
        cadre.raycastTarget = false;
        var cr = cadreGO.GetComponent<RectTransform>();
        cr.anchorMin = new Vector2(1f, 0f); cr.anchorMax = new Vector2(1f, 0f);
        cr.pivot = new Vector2(1f, 0f);
        cr.anchoredPosition = new Vector2(-18f, 18f);
        cr.sizeDelta = new Vector2(TAILLE + 4f, TAILLE + 4f);

        var fondGO = new GameObject("Fond");
        fondGO.transform.SetParent(cadreGO.transform, false);
        var fond = fondGO.AddComponent<Image>();
        fond.color = new Color(0.01f, 0.03f, 0.07f, 0.88f);
        fond.raycastTarget = false;
        _panneau = fondGO.GetComponent<RectTransform>();
        _panneau.anchorMin = Vector2.zero; _panneau.anchorMax = Vector2.one;
        _panneau.offsetMin = new Vector2(2f, 2f); _panneau.offsetMax = new Vector2(-2f, -2f);

        // Lignes de circuit décoratives (2 fines pistes dans le fond)
        LigneDecor(new Vector2(0.15f, 0.25f), new Vector2(0.65f, 0.255f));
        LigneDecor(new Vector2(0.4f, 0.7f),  new Vector2(0.85f, 0.705f));

        // Points des stations
        _cpuUI   = Point(new Color(0f, 0.85f, 1f),   11f, "CPU");
        _ramUI   = Point(new Color(0.78f, 0.43f, 1f), 11f, "RAM");
        _ecranUI = Point(new Color(0f, 1f, 0.55f),   11f, "ÉCRAN");

        // Anneau de l'objectif (doré, creux : 4 petits côtés)
        _objectifUI = Anneau(new Color(1f, 0.82f, 0.25f), 26f);

        // Flèche du joueur (chevron blanc : 2 barres en V)
        _joueurUI = Chevron(Color.white);
    }

    void LigneDecor(Vector2 min, Vector2 max)
    {
        var go = new GameObject("Piste");
        go.transform.SetParent(_panneau, false);
        var img = go.AddComponent<Image>();
        img.color = new Color(0f, 0.6f, 0.8f, 0.14f);
        img.raycastTarget = false;
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = min; r.anchorMax = max;
        r.offsetMin = r.offsetMax = Vector2.zero;
    }

    RectTransform Point(Color c, float taille, string label)
    {
        var go = new GameObject("Pt_" + label);
        go.transform.SetParent(_panneau, false);
        var img = go.AddComponent<Image>();
        img.color = c;
        img.raycastTarget = false;
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
        r.sizeDelta = new Vector2(taille, taille);
        go.transform.localRotation = Quaternion.Euler(0f, 0f, 45f); // losange

        // Étiquette sous le point
        var txtGO = new GameObject("Label");
        txtGO.transform.SetParent(go.transform, false);
        txtGO.transform.localRotation = Quaternion.Euler(0f, 0f, -45f);
        var tmp = txtGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = 12f; tmp.fontStyle = FontStyles.Bold;
        tmp.color = new Color(c.r, c.g, c.b, 0.95f);
        tmp.alignment = TextAlignmentOptions.Top;
        tmp.raycastTarget = false;
        tmp.enableWordWrapping = false;
        var tr = txtGO.GetComponent<RectTransform>();
        tr.anchorMin = tr.anchorMax = new Vector2(0.5f, 0f);
        tr.pivot = new Vector2(0.5f, 1f);
        tr.anchoredPosition = new Vector2(0f, -4f);
        tr.sizeDelta = new Vector2(60f, 16f);
        return r;
    }

    RectTransform Anneau(Color c, float taille)
    {
        var go = new GameObject("Objectif");
        go.transform.SetParent(_panneau, false);
        var r = go.AddComponent<RectTransform>();
        r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
        r.sizeDelta = new Vector2(taille, taille);

        // 4 côtés fins = un carré creux (lisible à cette taille)
        foreach (var (min, max) in new[]
        {
            (new Vector2(0f, 0f), new Vector2(1f, 0.12f)),
            (new Vector2(0f, 0.88f), new Vector2(1f, 1f)),
            (new Vector2(0f, 0f), new Vector2(0.12f, 1f)),
            (new Vector2(0.88f, 0f), new Vector2(1f, 1f)),
        })
        {
            var cote = new GameObject("Cote");
            cote.transform.SetParent(go.transform, false);
            var img = cote.AddComponent<Image>();
            img.color = c;
            img.raycastTarget = false;
            var cr = cote.GetComponent<RectTransform>();
            cr.anchorMin = min; cr.anchorMax = max;
            cr.offsetMin = cr.offsetMax = Vector2.zero;
        }
        return r;
    }

    RectTransform Chevron(Color c)
    {
        // Vraie flèche : une tige verticale + deux barres de pointe en haut.
        var go = new GameObject("Joueur");
        go.transform.SetParent(_panneau, false);
        var r = go.AddComponent<RectTransform>();
        r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
        r.sizeDelta = new Vector2(18f, 18f);

        // Tige
        var tige = new GameObject("Tige");
        tige.transform.SetParent(go.transform, false);
        var tImg = tige.AddComponent<Image>();
        tImg.color = c;
        tImg.raycastTarget = false;
        var tr = tige.GetComponent<RectTransform>();
        tr.anchorMin = tr.anchorMax = new Vector2(0.5f, 0.5f);
        tr.sizeDelta = new Vector2(3.5f, 13f);
        tr.anchoredPosition = new Vector2(0f, -2.5f);

        // Pointe (deux barres qui se rejoignent au sommet)
        foreach (float signe in new[] { -1f, 1f })
        {
            var barre = new GameObject("Pointe");
            barre.transform.SetParent(go.transform, false);
            var img = barre.AddComponent<Image>();
            img.color = c;
            img.raycastTarget = false;
            var br = barre.GetComponent<RectTransform>();
            br.anchorMin = br.anchorMax = new Vector2(0.5f, 0.5f);
            br.sizeDelta = new Vector2(3.5f, 9f);
            br.anchoredPosition = new Vector2(signe * 2.8f, 3.2f);
            barre.transform.localRotation = Quaternion.Euler(0f, 0f, signe * 42f);
        }
        return r;
    }
}
