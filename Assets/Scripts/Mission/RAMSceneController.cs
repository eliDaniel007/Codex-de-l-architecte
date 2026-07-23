using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Contrôleur de la scène RAM.
///
/// Principe : la boîte « variable » placée à la main dans la scène sert de
/// MODÈLE. Pour chaque variable stockée, on affiche un DUPLICATA de cette
/// boîte (même apparence, mêmes textes « nom » / « valeur ») posé sur les
/// tablettes (Wooden Shelf), 4 boîtes par tablette : 2, espace, 2.
///
///   • Dépôt automatique : on entre en portant une box → rangée dans la 1ère case libre.
///   • Sélection : clic sur une boîte → on reprend la variable (RAMBoxSelector).
///   • Case vide → pas de boîte (juste l'espace).
///   • Les boîtes décor (int box, float box, ...) ne sont pas touchées.
///   • [Échap] → retour au monde.
/// </summary>
public class RAMSceneController : MonoBehaviour
{
    [Header("Disposition (multiples de la largeur de la boîte)")]
    [Tooltip("Espacement entre deux boîtes d'une même paire.")]
    public float facteurEspacement = 1.4f;
    [Tooltip("Espace supplémentaire au milieu de la tablette (entre les deux paires).")]
    public float facteurTrou = 1.2f;
    [Tooltip("Nombre de boîtes par tablette (2 + espace + 2).")]
    public int boitesParTablette = 4;

    private Transform        _templateRoot; // racine complète de la boîte « variable »
    private List<Transform>  _boxes;        // boîte affichée par case (racine + clones)
    private List<Vector3>    _slotPos;      // position monde de chaque case
    private float            _tailleValeurOrig = -1f; // taille d'origine du texte 'valeur'
    private float            _tailleNomOrig    = -1f; // taille d'origine du texte 'nom'
    // Couleur réelle des boîtes de TYPE de la scène (int box, float box, ...)

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false; // On utilise la main de robot (comportement d'origine)

        AdapterCameraALEcran(); // plein écran : ne pas zoomer sur les tablettes

        var gs = GameState.I;
        Debug.Log(gs.boxExists
            ? $"[RAM] Entrée AVEC box en main : {gs.boxType} {gs.boxVariable} = {gs.boxValue}"
            : "[RAM] Entrée mains vides.");

        TrouverTemplate();
        Debug.Log(_templateRoot != null
            ? $"[RAM] Boîte modèle : racine '{_templateRoot.name}' (toute la boîte est gérée d'un bloc)"
            : "[RAM] ATTENTION : boîte modèle 'variable' introuvable — dépôt quand même, affichage impossible.");

        ScannerCouleursDecor(); // couleurs réelles des boîtes de type (int box, ...)

        if (_templateRoot != null) CalculerSlots();
        else _slotPos = new List<Vector3>();

        // Le STOCKAGE ne doit jamais échouer : au moins 12 cases logiques.
        gs.EnsureRamSlots(Mathf.Max(12, _slotPos.Count));

        if (gs.boxExists && gs.boxEstValeur)
        {
            // Une VALEUR nue : pas de dépôt automatique — le joueur CHOISIT la
            // variable en cliquant sa boîte (c'est l'AFFECTATION). Le bandeau
            // reste affiché tant que la valeur est en main.
            Message($"AFFECTATION — tu portes la valeur  \"{gs.boxValue}\"  :  clique la variable où la ranger.", true, 600f);
        }
        else if (gs.BriefingEnAttente() || (gs.StationAttendue() != "ram" && gs.StationAttendue() != ""))
        {
            // Mauvaise station ou briefing à lire — mais PAS à la seconde même où
            // le joueur vient de réussir quelque chose ici (période de grâce) :
            // le rappel s'affichera un peu plus tard s'il traîne encore là.
            _rappelDiffere = true;
            if (!gs.EnGrace) AfficherRappelStation();
        }
        else
        {
            // Une boîte-variable : dépôt automatique (comportement d'origine).
            int depose = gs.DeposerAuto();
            if (depose >= 0)
                Debug.Log($"[RAM] Dépôt automatique en case {depose + 1}. ({CompterPleines()} case(s) occupée(s))");
            else if (gs.boxExists)
                Debug.LogWarning("[RAM] RAM pleine : la box reste en main.");
        }

        if (_templateRoot != null) ConstruireBoites();

        // Formulaire « déclarer une variable » (mission 1 + usage libre).
        var ui = new GameObject("[RamDeclarationUI]");
        ui.AddComponent<RamDeclarationUI>();
    }

    int CompterPleines()
    {
        int n = 0;
        foreach (var s in GameState.I.ramSlots) if (s.filled) n++;
        return n;
    }

    /// <summary>
    /// Cadre la caméra de la RAM pour l'écran réel :
    ///  1. DÉZOOM global (le cadrage d'origine est trop serré : on élargit le
    ///     champ de vision de 30 %) ;
    ///  2. compensation d'aspect : le champ HORIZONTAL reste identique quel
    ///     que soit le format (plein écran, fenêtré...) — tablettes et
    ///     variables toujours visibles.
    /// </summary>
    static void AdapterCameraALEcran()
    {
        var cam = Camera.main;
        if (cam == null || cam.orthographic) return;

        const float ASPECT_REFERENCE = 16f / 9f;
        const float DEZOOM           = 1.1f; // 1 = cadrage d'origine ; plus grand = plus large

        // Champ horizontal voulu : celui réglé pour du 16:9, élargi du dézoom.
        float hFov = 2f * Mathf.Atan(Mathf.Tan(cam.fieldOfView * Mathf.Deg2Rad * 0.5f) * ASPECT_REFERENCE);
        hFov *= DEZOOM;

        // FOV vertical équivalent pour l'écran réel.
        cam.fieldOfView = Mathf.Clamp(
            2f * Mathf.Atan(Mathf.Tan(hFov * 0.5f) / cam.aspect) * Mathf.Rad2Deg, 20f, 110f);

        Debug.Log($"[RAM] Caméra adaptée (aspect {cam.aspect:0.00}, dézoom ×{DEZOOM}) : FOV {cam.fieldOfView:0.0}°.");
    }

    // ── message flottant de la scène RAM ──────────────────────────────────

    static TMPro.TextMeshProUGUI _msgTexte;
    static float _msgFin;

    /// <summary>Message en haut de l'écran (vert = succès, rouge = erreur).</summary>
    public static void Message(string texte, bool succes, float duree = 3.5f)
    {
        if (_msgTexte == null)
        {
            var go = new GameObject("[MessageRAM]");
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 85;
            var scaler = go.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            var fondGO = new GameObject("Fond");
            fondGO.transform.SetParent(go.transform, false);
            var img = fondGO.AddComponent<UnityEngine.UI.Image>();
            img.color = new Color(0f, 0.02f, 0.08f, 0.85f);
            img.raycastTarget = false;
            var fr = fondGO.GetComponent<RectTransform>();
            fr.anchorMin = new Vector2(0.16f, 0.87f); fr.anchorMax = new Vector2(0.84f, 0.96f);
            fr.offsetMin = fr.offsetMax = Vector2.zero;

            var txtGO = new GameObject("Texte");
            txtGO.transform.SetParent(fondGO.transform, false);
            _msgTexte = txtGO.AddComponent<TMPro.TextMeshProUGUI>();
            _msgTexte.fontSize = 28f; _msgTexte.fontStyle = TMPro.FontStyles.Bold;
            _msgTexte.alignment = TMPro.TextAlignmentOptions.Center;
            _msgTexte.raycastTarget = false;
            var tr = txtGO.GetComponent<RectTransform>();
            tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
            tr.offsetMin = new Vector2(14f, 4f); tr.offsetMax = new Vector2(-14f, -4f);
        }
        _msgTexte.text  = texte;
        _msgTexte.color = succes ? new Color(0.5f, 1f, 0.7f) : new Color(1f, 0.5f, 0.5f);
        _msgTexte.transform.parent.gameObject.SetActive(true);
        _msgFin = Time.unscaledTime + duree;
    }

    void MajMessage()
    {
        if (_msgTexte != null && _msgTexte.transform.parent.gameObject.activeSelf &&
            Time.unscaledTime > _msgFin)
            _msgTexte.transform.parent.gameObject.SetActive(false);
    }

    // ── dialogue de confirmation (affectation) ────────────────────────────

    static GameObject            _confirmPanneau;
    static TMPro.TextMeshProUGUI _confirmTexte;
    static System.Action         _confirmAction;

    /// <summary>Vrai quand le dialogue OUI/NON est affiché (bloque les clics boîtes).</summary>
    public static bool ConfirmationOuverte =>
        _confirmPanneau != null && _confirmPanneau.activeSelf;

    /// <summary>Pose une question OUI/NON ; 'surOui' est exécuté si le joueur confirme.</summary>
    public static void DemanderConfirmation(string question, System.Action surOui)
    {
        if (_confirmPanneau == null) ConstruireConfirmation();
        _confirmTexte.text = question;
        _confirmAction     = surOui;
        _confirmPanneau.SetActive(true);
    }

    static void ConstruireConfirmation()
    {
        var go = new GameObject("[ConfirmationRAM]");
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 95;
        var scaler = go.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        go.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        _confirmPanneau = new GameObject("Panneau");
        _confirmPanneau.transform.SetParent(go.transform, false);
        var fond = _confirmPanneau.AddComponent<UnityEngine.UI.Image>();
        fond.color = new Color(0f, 0.02f, 0.08f, 0.96f);
        var pr = _confirmPanneau.GetComponent<RectTransform>();
        pr.anchorMin = new Vector2(0.3f, 0.38f); pr.anchorMax = new Vector2(0.7f, 0.62f);
        pr.offsetMin = pr.offsetMax = Vector2.zero;

        // Question
        var txtGO = new GameObject("Question");
        txtGO.transform.SetParent(_confirmPanneau.transform, false);
        _confirmTexte = txtGO.AddComponent<TMPro.TextMeshProUGUI>();
        _confirmTexte.fontSize = 28f; _confirmTexte.fontStyle = TMPro.FontStyles.Bold;
        _confirmTexte.alignment = TMPro.TextAlignmentOptions.Center;
        _confirmTexte.color = Color.white; _confirmTexte.richText = true;
        _confirmTexte.raycastTarget = false;
        var tr = txtGO.GetComponent<RectTransform>();
        tr.anchorMin = new Vector2(0.05f, 0.45f); tr.anchorMax = new Vector2(0.95f, 0.95f);
        tr.offsetMin = tr.offsetMax = Vector2.zero;

        // Boutons OUI / NON
        BoutonConfirm("OUI", new Vector2(0.12f, 0.08f), new Vector2(0.46f, 0.38f),
            new Color(0.1f, 0.4f, 0.2f), () =>
            {
                _confirmPanneau.SetActive(false);
                _confirmAction?.Invoke();
                _confirmAction = null;
            });
        BoutonConfirm("NON", new Vector2(0.54f, 0.08f), new Vector2(0.88f, 0.38f),
            new Color(0.35f, 0.12f, 0.12f), () =>
            {
                _confirmPanneau.SetActive(false);
                _confirmAction = null;
            });

        _confirmPanneau.SetActive(false);
    }

    static void BoutonConfirm(string label, Vector2 min, Vector2 max, Color couleur, System.Action onClick)
    {
        var go = new GameObject("Btn_" + label);
        go.transform.SetParent(_confirmPanneau.transform, false);
        go.AddComponent<UnityEngine.UI.Image>().color = couleur;
        go.AddComponent<UnityEngine.UI.Button>().onClick.AddListener(() => onClick());
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = min; r.anchorMax = max;
        r.offsetMin = r.offsetMax = Vector2.zero;

        var txtGO = new GameObject("Label");
        txtGO.transform.SetParent(go.transform, false);
        var tmp = txtGO.AddComponent<TMPro.TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = 30f; tmp.fontStyle = TMPro.FontStyles.Bold;
        tmp.color = Color.white; tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        var lr = txtGO.GetComponent<RectTransform>();
        lr.anchorMin = Vector2.zero; lr.anchorMax = Vector2.one;
        lr.offsetMin = lr.offsetMax = Vector2.zero;
    }

    bool _rappelDiffere; // un rappel « mauvaise station » attend la fin de la grâce

    /// <summary>Affiche le rappel adapté (briefing à lire ou mauvaise station).</summary>
    void AfficherRappelStation()
    {
        _rappelDiffere = false;
        var gs = GameState.I;
        if (gs.BriefingEnAttente())
            Message("Retourne au CPU (unité de contrôle) lire la prochaine ligne", false, 600f);
        else if (gs.StationAttendue() != "ram" && gs.StationAttendue() != "")
            Message($"Rien à faire dans la RAM pour l'instant — va plutôt vers {GameState.NomStation(gs.StationAttendue())}.",
                    false, 8f);
    }

    void Update()
    {
        MajMessage(); // expiration du message flottant

        // La grâce vient de se terminer et le joueur est toujours là → rappel.
        if (_rappelDiffere && !GameState.I.EnGrace) AfficherRappelStation();

        // Dialogue OUI/NON ouvert : [Échap] le ferme (annule), rien d'autre.
        if (ConfirmationOuverte)
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            { _confirmPanneau.SetActive(false); _confirmAction = null; }
#else
            if (Input.GetKeyDown(KeyCode.Escape))
            { _confirmPanneau.SetActive(false); _confirmAction = null; }
#endif
            return;
        }

        if (RamDeclarationUI.PanneauOuvert) return; // le formulaire gère ses touches

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) Retour();
#else
        if (Input.GetKeyDown(KeyCode.Escape)) Retour();
#endif
    }

    void Retour()
    {
        GameState.I.RetourSansDepot(); // si on tient encore une box (RAM pleine), on la régénère
        SceneManager.LoadScene(GameState.I.mainSceneName);
    }

    // ── couleurs des types : lues sur les TEXTES des boîtes de type ──────

    /// <summary>
    /// Échantillonne la couleur du TEXTE de chaque boîte de type (bool, char,
    /// int, float, string) et la mémorise dans GameState : les variables et les
    /// valeurs utiliseront EXACTEMENT ces couleurs (mêmes RGB).
    /// </summary>
    void ScannerCouleursDecor()
    {
        var selectors = FindObjectsByType<RAMBoxSelector>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        var racines = new HashSet<Transform>();
        foreach (var sel in selectors)
        {
            var root = sel.transform.root;
            if (_templateRoot != null && root == _templateRoot) continue; // pas la boîte variable
            racines.Add(root);
        }

        int n = 0;
        foreach (var root in racines)
        {
            // Le texte qui affiche le nom du type (ex : « int ») donne sa couleur.
            foreach (var tmp in root.GetComponentsInChildren<TMP_Text>(true))
            {
                string txt = tmp.text.Trim().ToLowerInvariant();
                if (txt == "int" || txt == "float" || txt == "string" || txt == "bool" || txt == "char")
                {
                    GameState.DefinirCouleurType(txt, tmp.color);
                    n++;
                    break;
                }
            }
        }
        Debug.Log($"[RAM] {n} couleur(s) de type échantillonnée(s) sur les boîtes.");
    }

    // ── détection du modèle ───────────────────────────────────────────────

    /// <summary>La boîte modèle est celle qui porte les textes 'nom' et 'valeur'
    /// (la boîte « variable » placée à la main). Les boîtes décor n'ont que 'Type'.
    /// Fallback : la boîte dont la racine s'appelle 'variable'.</summary>
    void TrouverTemplate()
    {
        var selectors = FindObjectsByType<RAMBoxSelector>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        // La boîte « variable » est celle qui contient les textes 'nom' et 'valeur'.
        // On prend sa RACINE ENTIÈRE : la boîte est composée de plusieurs morceaux
        // (carton, textes...) chacun pouvant porter son propre sélecteur.
        foreach (var sel in selectors)
        {
            bool aNom = false, aValeur = false;
            foreach (var tmp in sel.transform.root.GetComponentsInChildren<TMP_Text>(true))
            {
                string n = tmp.gameObject.name.Trim().ToLowerInvariant();
                if (n == "nom")    aNom    = true;
                if (n == "valeur") aValeur = true;
            }
            if (aNom && aValeur) { _templateRoot = sel.transform.root; return; }
        }

        // Fallback : par le nom de la racine de la boîte.
        foreach (var sel in selectors)
        {
            string nom = sel.transform.root.name.Trim().ToLowerInvariant();
            if (nom.Contains("variable")) { _templateRoot = sel.transform.root; return; }
        }
    }

    // ── calcul des positions de cases ─────────────────────────────────────

    void CalculerSlots()
    {
        _slotPos = new List<Vector3>();
        Vector3 origine = _templateRoot.position;

        // ── Tablettes : 'Wooden Shelf*' à N'IMPORTE quelle profondeur ──
        // (on ne garde que la racine de chaque étagère, pas ses sous-objets)
        var shelves = new List<Transform>();
        foreach (var t in FindObjectsByType<Transform>(FindObjectsSortMode.None))
        {
            if (!t.name.StartsWith("Wooden Shelf")) continue;
            bool sousObjet = false;
            for (var p = t.parent; p != null; p = p.parent)
                if (p.name.StartsWith("Wooden Shelf")) { sousObjet = true; break; }
            if (!sousObjet) shelves.Add(t);
        }

        // Pas d'étagère trouvée : simple rangée de 4 alignée sur la boîte modèle.
        if (shelves.Count == 0)
        {
            float dx = LargeurBoite() * 1.12f;
            for (int c = 0; c < 4; c++)
                _slotPos.Add(origine + new Vector3(c * dx, 0f, 0f));
            Debug.LogWarning("[RAM] Aucune 'Wooden Shelf' trouvée : rangée simple autour du modèle.");
            return;
        }

        // Étagère de référence = la plus proche de la boîte modèle.
        Transform refShelf = shelves[0];
        float best = float.MaxValue;
        foreach (var s in shelves)
        {
            float d = Vector3.Distance(s.position, origine);
            if (d < best) { best = d; refShelf = s; }
        }

        // Rangées : la référence d'abord (hauteur/profondeur du modèle), puis les autres.
        var deltas = new List<Vector3> { Vector3.zero };
        var autres = new List<Transform>();
        foreach (var s in shelves) if (s != refShelf) autres.Add(s);
        autres.Sort((a, b) => b.position.z.CompareTo(a.position.z));
        foreach (var s in autres) deltas.Add(s.position - refShelf.position);

        // ── Colonnes : 4 variables par tablette, COLLÉES VERS LA GAUCHE ──
        //     _1_2_3_4........   (bien visibles, serrées côté gauche du meuble)
        Bounds b = BoundsDe(refShelf);
        float boite = LargeurBoite();
        float x0    = b.min.x + boite * 0.55f;   // départ : bord gauche + demi-boîte
        float pas   = boite * 1.12f;             // espacement serré
        float xMax  = b.max.x - boite * 0.5f;

        var cols = new List<float>();
        for (int c = 0; c < 4; c++)
            cols.Add(Mathf.Min(x0 + c * pas, xMax)); // jamais hors du meuble

        // Cases en ordre : rangée du modèle d'abord, colonne par colonne.
        // Hauteur/profondeur = celles de la boîte modèle (posée sur sa tablette).
        foreach (var delta in deltas)
            foreach (var x in cols)
                _slotPos.Add(new Vector3(x, origine.y, origine.z) + delta);

        Debug.Log($"[RAM] {shelves.Count} étagère(s), {_slotPos.Count} case(s), colonnes de {x0:0.0} à {xMax:0.0}.");
    }

    float LargeurBoite()
    {
        var rend = _templateRoot != null ? _templateRoot.GetComponentInChildren<Renderer>() : null;
        return rend != null ? Mathf.Max(0.1f, rend.bounds.size.x) : 1f;
    }

    static Bounds BoundsDe(Transform t)
    {
        var rends = t.GetComponentsInChildren<Renderer>();
        if (rends.Length == 0) return new Bounds(t.position, Vector3.one);
        Bounds b = rends[0].bounds;
        for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
        return b;
    }

    // ── affichage ─────────────────────────────────────────────────────────

    void ConstruireBoites()
    {
        var gs = GameState.I;
        _boxes = new List<Transform>();

        for (int i = 0; i < _slotPos.Count; i++)
        {
            Transform box;
            if (i == 0)
            {
                box = _templateRoot; // la boîte d'origine occupe sa propre place
            }
            else if (gs.ramSlots[i].filled)
            {
                // Duplication de la boîte modèle COMPLÈTE, placée telle quelle.
                var go = Instantiate(_templateRoot.gameObject, _slotPos[i], _templateRoot.rotation);
                go.name = _templateRoot.name + $"_case{i + 1}";
                box = go.transform;
            }
            else
            {
                _boxes.Add(null); // case vide → pas de boîte du tout
                continue;
            }

            _boxes.Add(box);
            AppliquerCase(box, i);
        }
    }

    void AppliquerCase(Transform box, int i)
    {
        var slot = GameState.I.ramSlots[i];

        if (!slot.filled)
        {
            // Case vide → on laisse juste l'espace (la boîte entière est masquée).
            box.gameObject.SetActive(false);
            return;
        }

        box.gameObject.SetActive(true);

        // La boîte est composée de plusieurs morceaux (carton, textes...) qui
        // peuvent chacun porter un sélecteur : on les configure TOUS pour que
        // n'importe quel clic sur la boîte prenne la bonne case.
        foreach (var s in box.GetComponentsInChildren<RAMBoxSelector>(true))
        {
            s.cellIndex     = i;
            s.variableName  = slot.variable;
            s.variableValue = slot.value;
            s.typeName      = slot.type;
        }

        // Couleur du type : int = rouge, float = rose, string = violet, bool = vert.
        // La couleur du type = celle du TEXTE de sa boîte de type (échantillonnée).
        // Le carton, lui, reste en carton — comme les boîtes de type.
        Color couleurType = GameState.CouleurType(slot.type);
        slot.color = couleurType;

        // Mêmes textes que la boîte d'origine : 'nom', 'valeur' et 'Type'.
        foreach (var tmp in box.GetComponentsInChildren<TMP_Text>(true))
        {
            string n = tmp.gameObject.name.Trim().ToLowerInvariant();
            if      (n == "nom")
            {
                tmp.text  = slot.variable;
                tmp.color = couleurType;
                // Le NOM de la variable en PLUS GROS (135 % de l'origine) :
                // c'est l'information principale de la boîte.
                if (_tailleNomOrig < 0f) _tailleNomOrig = tmp.fontSize;
                tmp.fontSize = _tailleNomOrig * 1.35f;
            }
            else if (n == "valeur")
            {
                tmp.text  = slot.value;
                tmp.color = couleurType;
                // Valeur plus PETITE (60 % de la taille d'origine du modèle) et
                // tronquée avec « … » si elle est trop longue pour la boîte.
                if (_tailleValeurOrig < 0f) _tailleValeurOrig = tmp.fontSize;
                tmp.fontSize     = _tailleValeurOrig * 0.6f;
                tmp.overflowMode = TextOverflowModes.Ellipsis;
            }
            else if (n == "type")   { tmp.text = slot.type;     tmp.color = couleurType; }
        }
    }
}
