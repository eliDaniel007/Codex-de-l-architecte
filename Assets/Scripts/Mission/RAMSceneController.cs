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
    }

    int CompterPleines()
    {
        int n = 0;
        foreach (var s in GameState.I.ramSlots) if (s.filled) n++;
        return n;
    }

    void Update()
    {
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

        // Largeur de la boîte (monde) pour l'espacement.
        float largeur = 1f;
        var rend = _templateRoot.GetComponentInChildren<Renderer>();
        if (rend != null) largeur = Mathf.Max(0.1f, rend.bounds.size.x);

        float dx   = largeur * facteurEspacement;
        float trou = largeur * facteurTrou;

        // Décalages de colonnes : 2 boîtes, espace, 2 boîtes.
        var cols = new List<float>();
        for (int c = 0; c < boitesParTablette; c++)
        {
            float x = c * dx;
            if (c >= boitesParTablette / 2) x += trou;
            cols.Add(x);
        }

        // Tablettes : la rangée du modèle d'abord, puis les autres (Wooden Shelf*).
        var deltas = new List<Vector3> { Vector3.zero };
        var shelves = new List<Transform>();
        foreach (var t in FindObjectsByType<Transform>(FindObjectsSortMode.None))
            if (t.parent == null && t.name.StartsWith("Wooden Shelf"))
                shelves.Add(t);

        if (shelves.Count > 1)
        {
            // Tablette la plus proche du modèle = sa rangée de référence.
            Transform refShelf = shelves[0];
            float best = float.MaxValue;
            foreach (var s in shelves)
            {
                float d = Vector3.Distance(s.position, origine);
                if (d < best) { best = d; refShelf = s; }
            }
            // Les autres tablettes donnent les décalages de rangée.
            var autres = new List<Transform>();
            foreach (var s in shelves) if (s != refShelf) autres.Add(s);
            autres.Sort((a, b) => b.position.z.CompareTo(a.position.z));
            foreach (var s in autres) deltas.Add(s.position - refShelf.position);
        }

        // Cases en ordre : rangée du modèle d'abord, colonne par colonne.
        foreach (var delta in deltas)
            foreach (var x in cols)
                _slotPos.Add(origine + delta + new Vector3(x, 0f, 0f));
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

        // Mêmes textes que la boîte d'origine : 'nom' et 'valeur'.
        foreach (var tmp in box.GetComponentsInChildren<TMP_Text>(true))
        {
            string n = tmp.gameObject.name.Trim().ToLowerInvariant();
            if      (n == "nom")    tmp.text = slot.variable;
            else if (n == "valeur") tmp.text = slot.value;
        }
    }
}
