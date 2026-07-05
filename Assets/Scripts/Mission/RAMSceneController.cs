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
    // Couleur réelle des boîtes de TYPE de la scène (int box, float box, ...)
    private readonly Dictionary<string, Color> _couleursDecor = new Dictionary<string, Color>();

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false; // On utilise la main de robot (comportement d'origine)

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

        // Dépôt automatique : si on entre en portant une box, on la range.
        int depose = gs.DeposerAuto();
        if (depose >= 0)
            Debug.Log($"[RAM] Dépôt automatique en case {depose + 1}. ({CompterPleines()} case(s) occupée(s))");
        else if (gs.boxExists)
            Debug.LogWarning("[RAM] RAM pleine : la box reste en main.");

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

    void Update()
    {
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

    // ── couleurs des boîtes de type (décor) ──────────────────────────────

    /// <summary>
    /// Repère les boîtes de TYPE de la scène (int box, float box, ...) et mémorise
    /// leur couleur de carton : les variables déclarées reprendront EXACTEMENT
    /// cette couleur (int déclaré = même couleur que la boîte int).
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

        foreach (var root in racines)
        {
            string type = TypeDeRacine(root);
            if (type == null || _couleursDecor.ContainsKey(type)) continue;
            var col = CouleurPrincipale(root);
            if (col.HasValue) _couleursDecor[type] = col.Value;
        }

        Debug.Log($"[RAM] Couleurs de type détectées : {string.Join(", ", _couleursDecor.Keys)}");
    }

    static string TypeDeRacine(Transform root)
    {
        foreach (var tmp in root.GetComponentsInChildren<TMP_Text>(true))
        {
            string txt = tmp.text.Trim().ToLowerInvariant();
            if (txt == "int" || txt == "float" || txt == "string" || txt == "bool") return txt;
        }
        string n = root.name.ToLowerInvariant();
        foreach (var k in new[] { "float", "string", "bool", "int" })
            if (n.Contains(k)) return k;
        return null;
    }

    static Color? CouleurPrincipale(Transform root)
    {
        foreach (var rend in root.GetComponentsInChildren<Renderer>(true))
        {
            if (rend.GetComponent<TMP_Text>() != null) continue;
            var mat = rend.sharedMaterial;
            if (mat == null) continue;
            if (mat.HasProperty("_BaseColor")) return mat.GetColor("_BaseColor");
            if (mat.HasProperty("_Color"))     return mat.color;
        }
        return null;
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

        // Pas d'étagère trouvée : simple rangée serrée alignée sur la boîte modèle.
        if (shelves.Count == 0)
        {
            float dx = LargeurBoite() * 1.15f;
            for (int c = 0; c < 3; c++)
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

        // ── Colonnes : 3 boîtes par tablette, resserrées au centre du meuble ──
        // (3 en haut + 3 en bas : au-delà de 3 par ligne, la 4e sortait du meuble)
        Bounds b = BoundsDe(refShelf);
        float marge  = LargeurBoite() * 0.6f;
        float x0 = b.min.x + marge;
        float x1 = b.max.x - marge;
        if (x1 <= x0) { x0 = b.center.x; x1 = b.center.x; }

        float centre = (x0 + x1) * 0.5f;
        float ecart  = Mathf.Min(LargeurBoite() * 1.15f, (x1 - x0) * 0.5f); // boîtes proches

        var cols = new List<float> { centre - ecart, centre, centre + ecart };

        // Cases en ordre : rangée du modèle d'abord, colonne par colonne.
        // Hauteur/profondeur = celles de la boîte modèle (posée sur sa tablette).
        foreach (var delta in deltas)
            foreach (var x in cols)
                _slotPos.Add(new Vector3(x, origine.y, origine.z) + delta);

        Debug.Log($"[RAM] {shelves.Count} étagère(s), {_slotPos.Count} case(s) visibles dans la largeur [{x0:0.0} → {x1:0.0}].");
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
        Color couleurType = GameState.CouleurType(slot.type);

        // Le CARTON reprend la couleur RÉELLE de la boîte de type de la scène
        // (int déclaré = même carton que la boîte « int box »). Palette en secours.
        Color couleurCarton = _couleursDecor.TryGetValue(slot.type, out var cd) ? cd : couleurType;
        GameState.TeinterBoite(box, couleurCarton);
        slot.color = couleurCarton; // la boîte portée gardera la même couleur

        // Mêmes textes que la boîte d'origine : 'nom', 'valeur' et 'Type'.
        foreach (var tmp in box.GetComponentsInChildren<TMP_Text>(true))
        {
            string n = tmp.gameObject.name.Trim().ToLowerInvariant();
            if      (n == "nom")    { tmp.text = slot.variable; tmp.color = couleurType; }
            else if (n == "valeur") { tmp.text = slot.value;    tmp.color = couleurType; }
            else if (n == "type")   { tmp.text = slot.type;     tmp.color = couleurType; }
        }
    }
}
