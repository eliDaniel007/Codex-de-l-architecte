using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Décor de CARTE MÈRE : peuple automatiquement le plateau (« Ground ») avec
/// les composants qu'on trouve sur une vraie carte mère :
///
///   • PUCES : gros circuits intégrés noirs avec broches dorées et marquage (U7, K2...)
///   • CONDENSATEURS : cylindres verticaux à capuchon argenté
///   • RÉSISTANCES SMD : petites plaquettes sombres aux extrémités argentées
///   • PISTES : chemins de cuivre plats en L qui serpentent sur la carte
///   • VIAS : petits plots dorés
///   • CÂBLES : fils souples rouge/noir/jaune qui font des arcs sur la carte
///
/// Tout est généré au chargement de la MainScene, en évitant les zones de
/// jeu (CPU, RAM, écran, claviers, spawn du joueur). Position déterministe
/// (même disposition à chaque lancement).
/// </summary>
public class DecorCarteMere : MonoBehaviour
{
    [Header("Sol de référence")]
    public string nomSol = "Ground";
    [Tooltip("Marge depuis les bords de la carte (m).")]
    public float margeBord = 3f;
    [Tooltip("Rayon libre autour des stations de jeu (m).")]
    public float rayonExclusion = 8f;

    [Header("Quantités")]
    public int nbPuces         = 18;
    public int nbCondensateurs = 26;
    public int nbResistances   = 34;
    public int nbPistes        = 22;
    public int nbVias          = 80;
    public int nbCables        = 12;
    public int nbSlots         = 6;   // connecteurs longs (type barrette RAM / PCIe)
    public int nbPiles         = 2;   // piles bouton CR2032
    public int nbQuartz        = 10;  // oscillateurs à quartz (capsules métalliques)

    private Transform _racine;
    private Vector3[] _coins;              // 4 coins du plateau (dessus)
    private Vector3   _origine, _axeU, _axeV; // repère de la carte (pour rester dessus)
    private float     _lonU, _lonV;
    private readonly List<Vector3> _exclusions = new List<Vector3>();
    private readonly List<(Vector3 pos, float rayon)> _occupes = new List<(Vector3, float)>();

    // matériaux partagés
    private Material _matNoir, _matOr, _matArgent, _matCuivre;
    private static readonly Color[] CouleursCondo = {
        new Color(0.10f, 0.15f, 0.35f),   // bleu nuit
        new Color(0.12f, 0.12f, 0.14f),   // noir
        new Color(0.35f, 0.18f, 0.10f),   // brun
    };
    private static readonly Color[] CouleursCable = {
        new Color(0.75f, 0.10f, 0.10f),   // rouge
        new Color(0.10f, 0.10f, 0.12f),   // noir
        new Color(0.85f, 0.70f, 0.10f),   // jaune
    };

    // ── AUTO-CRÉATION dans la MainScene ───────────────────────────────────

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        SceneManager.sceneLoaded += (s, m) => CreerSiBesoin();
        CreerSiBesoin();
    }

    static void CreerSiBesoin()
    {
        if (SceneManager.GetActiveScene().name != GameState.I.mainSceneName) return;
        if (FindFirstObjectByType<DecorCarteMere>() != null) return;

        var go = new GameObject("[DecorCarteMere]");
        go.AddComponent<DecorCarteMere>();
    }

    void Start()
    {
        Construire();
    }

    // ══════════════════════════════════════════════════════════════════════
    //  CONSTRUCTION
    // ══════════════════════════════════════════════════════════════════════

    public void Construire()
    {
        var plateau = TrouverPlateau();
        if (plateau == null) { Debug.LogWarning("[DecorCM] Plateau introuvable."); return; }

        _coins = CoinsSuperieurs(plateau, margeBord);

        // Repère 2D de la carte : permet de vérifier qu'un point reste dessus.
        _origine = _coins[0];
        _axeU = _coins[1] - _coins[0]; _lonU = _axeU.magnitude; _axeU /= _lonU;
        _axeV = _coins[3] - _coins[0]; _lonV = _axeV.magnitude; _axeV /= _lonV;

        TrouverExclusions();
        ConstruireMateriaux();

        if (_racine != null) Destroy(_racine.gameObject);
        _racine = new GameObject("Composants_CarteMere").transform;
        _racine.SetParent(transform, false);

        Random.InitState(20260705); // disposition stable d'une session à l'autre

        for (int i = 0; i < nbPuces; i++)         Puce(i);
        for (int i = 0; i < nbSlots; i++)         Slot(i);
        for (int i = 0; i < nbCondensateurs; i++) Condensateur();
        for (int i = 0; i < nbResistances; i++)   Resistance();
        for (int i = 0; i < nbQuartz; i++)        Quartz();
        for (int i = 0; i < nbPiles; i++)         PileBouton();
        for (int i = 0; i < nbPistes; i++)        Piste();
        for (int i = 0; i < nbVias; i++)          Via();
        for (int i = 0; i < nbCables; i++)        Cable();

        Debug.Log($"[DecorCM] Carte mère peuplée : {nbPuces} puces, {nbSlots} slots, {nbCondensateurs} condos, " +
                  $"{nbResistances} résistances, {nbQuartz} quartz, {nbPiles} piles, " +
                  $"{nbPistes} pistes, {nbVias} vias, {nbCables} câbles.");
    }

    // ── composants ────────────────────────────────────────────────────────

    /// <summary>Circuit intégré : corps noir plat, broches dorées, marquage TMP.</summary>
    void Puce(int index)
    {
        float larg = Random.Range(2.2f, 4.5f);
        float prof = Random.Range(2.2f, 4.5f);
        float haut = Random.Range(0.5f, 0.8f);
        if (!TrouverPlace(Mathf.Max(larg, prof) * 0.75f, out Vector3 pos)) return;

        float angle = Random.Range(0, 4) * 90f;
        var parent = new GameObject("Puce_U" + (index + 1));
        parent.transform.SetParent(_racine, false);
        parent.transform.position = pos;
        parent.transform.rotation = Quaternion.Euler(0f, angle, 0f);

        // Corps
        var corps = GameObject.CreatePrimitive(PrimitiveType.Cube);
        corps.name = "Corps";
        corps.transform.SetParent(parent.transform, false);
        corps.transform.localPosition = new Vector3(0f, haut * 0.5f, 0f);
        corps.transform.localScale    = new Vector3(larg, haut, prof);
        corps.GetComponent<Renderer>().sharedMaterial = _matNoir; // collider conservé : on peut monter dessus

        // Rangées de broches dorées (2 côtés)
        foreach (float signe in new[] { -1f, 1f })
        {
            var broches = GameObject.CreatePrimitive(PrimitiveType.Cube);
            broches.name = "Broches";
            broches.transform.SetParent(parent.transform, false);
            broches.transform.localPosition = new Vector3(signe * (larg * 0.5f + 0.15f), haut * 0.25f, 0f);
            broches.transform.localScale    = new Vector3(0.3f, haut * 0.5f, prof * 0.9f);
            Destroy(broches.GetComponent<Collider>());
            broches.GetComponent<Renderer>().sharedMaterial = _matOr;
        }

        // Marquage sur le dessus (U7, K2...)
        var txtGO = new GameObject("Marquage");
        txtGO.transform.SetParent(parent.transform, false);
        txtGO.transform.localPosition = new Vector3(0f, haut + 0.02f, 0f);
        txtGO.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        var tmp = txtGO.AddComponent<TextMeshPro>();
        string[] prefixes = { "U", "K", "IC", "Q" };
        tmp.text      = $"{prefixes[Random.Range(0, prefixes.Length)]}{Random.Range(1, 99)}\n<size=55%>CDX-{Random.Range(100, 999)}</size>";
        tmp.fontSize  = 7f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = new Color(0.85f, 0.87f, 0.9f);
        tmp.rectTransform.sizeDelta = new Vector2(larg, prof);
    }

    /// <summary>Connecteur long (barrette RAM / PCIe) : socle sombre + fente claire.</summary>
    void Slot(int index)
    {
        float longu = Random.Range(6f, 11f);
        if (!TrouverPlace(longu * 0.55f, out Vector3 pos)) return;

        float angle = Random.Range(0, 4) * 90f;
        var parent = new GameObject("Slot_" + (index + 1));
        parent.transform.SetParent(_racine, false);
        parent.transform.position = pos;
        parent.transform.rotation = Quaternion.Euler(0f, angle, 0f);

        // Socle
        var socle = GameObject.CreatePrimitive(PrimitiveType.Cube);
        socle.name = "Socle";
        socle.transform.SetParent(parent.transform, false);
        socle.transform.localPosition = new Vector3(0f, 0.45f, 0f);
        socle.transform.localScale    = new Vector3(longu, 0.9f, 1.1f);
        socle.GetComponent<Renderer>().sharedMaterial =
            Mat(new Color(0.12f, 0.13f, 0.16f), 0.2f, 0.45f); // collider conservé (obstacle)

        // Fente centrale claire (le connecteur)
        var fente = GameObject.CreatePrimitive(PrimitiveType.Cube);
        fente.name = "Fente";
        fente.transform.SetParent(parent.transform, false);
        fente.transform.localPosition = new Vector3(0f, 0.92f, 0f);
        fente.transform.localScale    = new Vector3(longu * 0.94f, 0.08f, 0.3f);
        Destroy(fente.GetComponent<Collider>());
        fente.GetComponent<Renderer>().sharedMaterial = _matOr;

        // Ergots de verrouillage aux extrémités
        foreach (float signe in new[] { -1f, 1f })
        {
            var ergot = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ergot.name = "Ergot";
            ergot.transform.SetParent(parent.transform, false);
            ergot.transform.localPosition = new Vector3(signe * (longu * 0.5f + 0.25f), 0.65f, 0f);
            ergot.transform.localScale    = new Vector3(0.5f, 1.3f, 0.9f);
            Destroy(ergot.GetComponent<Collider>());
            ergot.GetComponent<Renderer>().sharedMaterial =
                Mat(new Color(0.75f, 0.72f, 0.65f), 0.1f, 0.4f);
        }
    }

    /// <summary>Oscillateur à quartz : petite capsule métallique couchée.</summary>
    void Quartz()
    {
        if (!TrouverPlace(0.8f, out Vector3 pos)) return;

        var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go.name = "Quartz";
        go.transform.SetParent(_racine, false);
        go.transform.position   = pos + Vector3.up * 0.22f;
        go.transform.rotation   = Quaternion.Euler(90f, Random.Range(0, 4) * 90f, 0f);
        go.transform.localScale = new Vector3(0.45f, 0.55f, 0.45f);
        Destroy(go.GetComponent<Collider>());
        go.GetComponent<Renderer>().sharedMaterial = _matArgent;
    }

    /// <summary>Pile bouton CR2032 : disque argenté plat avec gravure.</summary>
    void PileBouton()
    {
        if (!TrouverPlace(1.6f, out Vector3 pos)) return;

        var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = "PileBouton";
        go.transform.SetParent(_racine, false);
        go.transform.position   = pos + Vector3.up * 0.12f;
        go.transform.localScale = new Vector3(2.6f, 0.12f, 2.6f);
        go.GetComponent<Renderer>().sharedMaterial = _matArgent;

        // Gravure « CR2032 + » sur le dessus
        var txtGO = new GameObject("Gravure");
        txtGO.transform.SetParent(go.transform, false);
        txtGO.transform.localPosition = new Vector3(0f, 1.1f, 0f);
        txtGO.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        txtGO.transform.localScale    = new Vector3(1f, 8.3f, 1f); // compense l'écrasement du cylindre
        var tmp = txtGO.AddComponent<TextMeshPro>();
        tmp.text      = "CR2032\n<size=70%>+</size>";
        tmp.fontSize  = 4f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = new Color(0.35f, 0.36f, 0.4f);
        tmp.rectTransform.sizeDelta = new Vector2(2.4f, 2.4f);
    }

    /// <summary>Condensateur électrolytique : cylindre + capuchon argenté.</summary>
    void Condensateur()
    {
        float rayon = Random.Range(0.35f, 0.7f);
        float haut  = Random.Range(1.1f, 2.2f);
        if (!TrouverPlace(rayon + 0.4f, out Vector3 pos)) return;

        var corps = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        corps.name = "Condensateur";
        corps.transform.SetParent(_racine, false);
        corps.transform.position   = pos + Vector3.up * (haut * 0.5f);
        corps.transform.localScale = new Vector3(rayon * 2f, haut * 0.5f, rayon * 2f);
        corps.GetComponent<Renderer>().sharedMaterial =
            Mat(CouleursCondo[Random.Range(0, CouleursCondo.Length)], 0.3f, 0.5f);

        var capot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        capot.name = "Capot";
        capot.transform.SetParent(_racine, false);
        capot.transform.position   = pos + Vector3.up * (haut + 0.03f);
        capot.transform.localScale = new Vector3(rayon * 2.02f, 0.05f, rayon * 2.02f);
        Destroy(capot.GetComponent<Collider>());
        capot.GetComponent<Renderer>().sharedMaterial = _matArgent;
    }

    /// <summary>Résistance SMD : plaquette sombre, extrémités argentées.</summary>
    void Resistance()
    {
        float longu = Random.Range(0.8f, 1.4f);
        if (!TrouverPlace(longu, out Vector3 pos)) return;

        float angle = Random.Range(0, 4) * 90f;
        var parent = new GameObject("Resistance");
        parent.transform.SetParent(_racine, false);
        parent.transform.position = pos;
        parent.transform.rotation = Quaternion.Euler(0f, angle, 0f);

        var corps = GameObject.CreatePrimitive(PrimitiveType.Cube);
        corps.transform.SetParent(parent.transform, false);
        corps.transform.localPosition = new Vector3(0f, 0.12f, 0f);
        corps.transform.localScale    = new Vector3(longu, 0.24f, longu * 0.45f);
        Destroy(corps.GetComponent<Collider>());
        corps.GetComponent<Renderer>().sharedMaterial = Mat(new Color(0.16f, 0.17f, 0.2f), 0.2f, 0.4f);

        foreach (float signe in new[] { -1f, 1f })
        {
            var bout = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bout.transform.SetParent(parent.transform, false);
            bout.transform.localPosition = new Vector3(signe * longu * 0.5f, 0.12f, 0f);
            bout.transform.localScale    = new Vector3(0.18f, 0.26f, longu * 0.47f);
            Destroy(bout.GetComponent<Collider>());
            bout.GetComponent<Renderer>().sharedMaterial = _matArgent;
        }
    }

    /// <summary>Piste de cuivre : chemin plat en L (2 à 4 segments) posé sur la carte.</summary>
    void Piste()
    {
        if (!TrouverPlace(2f, out Vector3 pos)) return;

        Vector3 u = (_coins[1] - _coins[0]).normalized; // axes de la carte
        Vector3 v = (_coins[3] - _coins[0]).normalized;
        Vector3 dir = Random.value < 0.5f ? u : v;

        int nbSegments = Random.Range(2, 5);
        Vector3 courant = pos;
        for (int s = 0; s < nbSegments; s++)
        {
            float longu = Random.Range(3f, 9f);
            Vector3 fin = courant + dir * longu;

            // La piste ne doit ni sortir de la carte, ni traverser une station.
            if (!DansLaCarte(fin, 1f) || ProcheExclusion(fin, 0.5f) ||
                ProcheExclusion((courant + fin) * 0.5f, 0.5f))
                break;

            var seg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            seg.name = "Piste";
            seg.transform.SetParent(_racine, false);
            seg.transform.position   = (courant + fin) * 0.5f + Vector3.up * 0.015f;
            seg.transform.rotation   = Quaternion.LookRotation(dir);
            seg.transform.localScale = new Vector3(0.28f, 0.03f, longu + 0.28f);
            Destroy(seg.GetComponent<Collider>());
            seg.GetComponent<Renderer>().sharedMaterial = _matCuivre;

            // Via doré au coude
            var coude = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            coude.name = "Coude";
            coude.transform.SetParent(_racine, false);
            coude.transform.position   = fin + Vector3.up * 0.02f;
            coude.transform.localScale = new Vector3(0.45f, 0.02f, 0.45f);
            Destroy(coude.GetComponent<Collider>());
            coude.GetComponent<Renderer>().sharedMaterial = _matOr;

            courant = fin;
            dir = (dir == u || dir == -u) ? (Random.value < 0.5f ? v : -v)
                                          : (Random.value < 0.5f ? u : -u);
        }
    }

    /// <summary>Via : petit plot doré.</summary>
    void Via()
    {
        if (!TrouverPlace(0.5f, out Vector3 pos)) return;

        var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = "Via";
        go.transform.SetParent(_racine, false);
        go.transform.position   = pos + Vector3.up * 0.03f;
        go.transform.localScale = new Vector3(0.28f, 0.03f, 0.28f);
        Destroy(go.GetComponent<Collider>());
        go.GetComponent<Renderer>().sharedMaterial = _matOr;
    }

    /// <summary>Câble souple : arc entre deux points de la carte (LineRenderer).
    /// Le trajet complet est vérifié : jamais au-dessus d'une station.</summary>
    void Cable()
    {
        Vector3 a = Vector3.zero, b = Vector3.zero;
        bool ok = false;
        for (int essai = 0; essai < 10 && !ok; essai++)
        {
            if (!TrouverPlace(1f, out a)) return;
            if (!TrouverPlace(1f, out b)) return;

            // Vérifie 7 points le long du trajet : tous loin des stations et sur la carte.
            ok = true;
            for (int i = 1; i < 7; i++)
            {
                Vector3 pt = Vector3.Lerp(a, b, i / 7f);
                if (ProcheExclusion(pt, 0.5f) || !DansLaCarte(pt, 0.5f)) { ok = false; break; }
            }
        }
        if (!ok) return;

        Color c = CouleursCable[Random.Range(0, CouleursCable.Length)];

        var go = new GameObject("Cable");
        go.transform.SetParent(_racine, false);
        var lr = go.AddComponent<LineRenderer>();

        // Le câble est POSÉ AU SOL : il serpente légèrement (ondulation
        // latérale) et chaque point est plaqué sur la surface de la carte.
        Vector3 lateral = Vector3.Cross((b - a).normalized, Vector3.up);
        float amplitude = Random.Range(0.6f, 1.6f);
        float frequence = Random.Range(2f, 4f);
        float phase     = Random.Range(0f, 6.28f);

        const int PTS = 26;
        lr.positionCount = PTS;
        for (int i = 0; i < PTS; i++)
        {
            float t = i / (PTS - 1f);
            // fondu de l'ondulation aux extrémités (le câble part et arrive droit)
            float fondu = Mathf.Sin(t * Mathf.PI);
            Vector3 p = Vector3.Lerp(a, b, t) + lateral * (Mathf.Sin(t * frequence * 6.28f + phase) * amplitude * fondu);

            // plaqué sur la surface réelle de la carte
            if (Physics.Raycast(p + Vector3.up * 3f, Vector3.down, out var hit, 6f))
                p = hit.point;
            p += Vector3.up * 0.08f; // demi-épaisseur du câble

            lr.SetPosition(i, p);
        }
        lr.widthMultiplier = 0.16f;
        lr.material = Mat(c, 0.1f, 0.35f);
        lr.material.color = c;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
    }

    // ── placement ─────────────────────────────────────────────────────────

    /// <summary>Point libre aléatoire : SUR la carte (avec sa taille), loin des
    /// 4 stations de jeu, sans chevaucher un autre composant.</summary>
    bool TrouverPlace(float rayon, out Vector3 pos)
    {
        for (int essai = 0; essai < 60; essai++)
        {
            Vector3 p = Vector3.Lerp(
                Vector3.Lerp(_coins[0], _coins[1], Random.value),
                Vector3.Lerp(_coins[3], _coins[2], Random.value),
                Random.value);

            // L'objet ENTIER doit rester sur la carte (marge = son rayon).
            if (!DansLaCarte(p, rayon)) continue;

            // Zones de jeu interdites : distance = rayon d'exclusion + taille de l'objet.
            if (ProcheExclusion(p, rayon)) continue;

            // Chevauchement avec un composant déjà posé
            bool interdit = false;
            foreach (var (o, r) in _occupes)
                if (HorizDist(p, o) < rayon + r) { interdit = true; break; }
            if (interdit) continue;

            // Pose sur la surface réelle
            if (Physics.Raycast(p + Vector3.up * 3f, Vector3.down, out var hit, 6f))
                p = hit.point;

            _occupes.Add((p, rayon));
            pos = p;
            return true;
        }
        pos = Vector3.zero;
        return false;
    }

    /// <summary>Vrai si le point (avec sa marge) est à l'intérieur du plateau.</summary>
    bool DansLaCarte(Vector3 p, float marge)
    {
        Vector3 d = p - _origine; d.y = 0f;
        float a = Vector3.Dot(d, _axeU);
        float b = Vector3.Dot(d, _axeV);
        return a >= marge && a <= _lonU - marge &&
               b >= marge && b <= _lonV - marge;
    }

    /// <summary>Vrai si le point est trop proche d'une station de jeu.</summary>
    bool ProcheExclusion(Vector3 p, float rayonObjet)
    {
        foreach (var e in _exclusions)
            if (HorizDist(p, e) < rayonExclusion + rayonObjet) return true;
        return false;
    }

    static float HorizDist(Vector3 a, Vector3 b)
    {
        a.y = 0f; b.y = 0f;
        return Vector3.Distance(a, b);
    }

    void TrouverExclusions()
    {
        _exclusions.Clear();

        var cpu = GameObject.Find("Processeur");
        if (cpu != null) _exclusions.Add(cpu.transform.position);
        var zone = FindFirstObjectByType<CPUZone>();
        if (zone != null) _exclusions.Add(zone.transform.position);
        var ram = FindFirstObjectByType<LoadSceneOnPlayerEnter>();
        if (ram != null) _exclusions.Add(ram.transform.position);
        var ecran = FindFirstObjectByType<ConsoleScreen>();
        if (ecran != null) _exclusions.Add(ecran.transform.position);
        foreach (var k in FindObjectsByType<KeyboardTerminal>(FindObjectsSortMode.None))
            _exclusions.Add(k.transform.position);
        var joueur = GameObject.FindGameObjectWithTag("Player");
        if (joueur != null) _exclusions.Add(joueur.transform.position);
        var spawn = GameObject.Find("BoxSpawnPoint");
        if (spawn != null) _exclusions.Add(spawn.transform.position);
    }

    // ── plateau (mêmes règles que la clôture) ─────────────────────────────

    Renderer TrouverPlateau()
    {
        GameObject sol = GameObject.Find(nomSol);
        if (sol == null)
        {
            foreach (var t in FindObjectsByType<Transform>(FindObjectsSortMode.None))
            {
                string n = t.name.ToLowerInvariant();
                if (n.Contains("ground") || n.Contains("carte") || n.Contains("mother") || n == "sol")
                { sol = t.gameObject; break; }
            }
        }
        if (sol == null) return null;

        Renderer meilleur = null;
        float aireMax = 0f;
        foreach (var r in sol.GetComponentsInChildren<Renderer>())
        {
            float aire = r.bounds.size.x * r.bounds.size.z;
            if (aire > aireMax) { aireMax = aire; meilleur = r; }
        }
        return meilleur;
    }

    static Vector3[] CoinsSuperieurs(Renderer r, float recul)
    {
        Bounds lb = r.localBounds;
        Transform t = r.transform;

        var coins = new[]
        {
            t.TransformPoint(new Vector3(lb.min.x, lb.max.y, lb.min.z)),
            t.TransformPoint(new Vector3(lb.max.x, lb.max.y, lb.min.z)),
            t.TransformPoint(new Vector3(lb.max.x, lb.max.y, lb.max.z)),
            t.TransformPoint(new Vector3(lb.min.x, lb.max.y, lb.max.z)),
        };

        Vector3 centre = (coins[0] + coins[1] + coins[2] + coins[3]) * 0.25f;
        for (int i = 0; i < 4; i++)
        {
            Vector3 versCentre = centre - coins[i];
            versCentre.y = 0f;
            if (versCentre.sqrMagnitude > 0.001f)
                coins[i] += versCentre.normalized * recul;
        }
        return coins;
    }

    // ── matériaux ─────────────────────────────────────────────────────────

    void ConstruireMateriaux()
    {
        _matNoir   = Mat(new Color(0.08f, 0.08f, 0.1f),  0.25f, 0.55f);
        _matOr     = Mat(new Color(0.85f, 0.68f, 0.25f), 0.9f,  0.75f);
        _matArgent = Mat(new Color(0.75f, 0.77f, 0.8f),  0.95f, 0.8f);
        _matCuivre = Mat(new Color(0.8f,  0.5f,  0.25f), 0.85f, 0.6f);
    }

    static Material Mat(Color c, float metallic, float smoothness)
    {
        var sh = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var m = new Material(sh);
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        else m.color = c;
        if (m.HasProperty("_Metallic"))   m.SetFloat("_Metallic", metallic);
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smoothness);
        return m;
    }
}
