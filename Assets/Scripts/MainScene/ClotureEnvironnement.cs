using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Clôture automatique autour de la CARTE MÈRE (l'objet « Ground » qui sert
/// de sol au jeu). Aucune mise en place requise : au chargement de la
/// MainScene, la clôture se construit toute seule le long des 4 BORDS RÉELS
/// de la carte (coins du mesh, dans SON repère — même si la carte est
/// tournée ou inclinée, la clôture suit exactement).
///
///   • Poteaux métalliques + 3 lisses segmentées entre chaque paire de poteaux
///   • Mur invisible haut de 6 m : impossible de sortir, même en sautant
/// </summary>
public class ClotureEnvironnement : MonoBehaviour
{
    [Header("Sol de référence (la carte mère)")]
    [Tooltip("Nom de l'objet qui sert de sol. Ses bords = le périmètre de la clôture.")]
    public string nomSol = "Ground";
    [Tooltip("Recul de la clôture vers l'intérieur, depuis le bord de la carte (m).")]
    public float retrait = 0.5f;

    [Header("Apparence")]
    [Tooltip("Hauteur visible de la clôture.")]
    public float hauteur = 12f;
    [Tooltip("Distance entre deux poteaux.")]
    public float espacementPoteaux = 5f;
    [Tooltip("Couleur des poteaux et lisses (métal sombre par défaut).")]
    public Color couleur = new Color(0.22f, 0.24f, 0.28f);

    [Header("Barrière physique")]
    [Tooltip("Hauteur du mur invisible anti-saut.")]
    public float hauteurCollider = 20f;

    private Transform _racine;
    private Material  _matMetal;

    // ── AUTO-CRÉATION : la clôture apparaît toute seule dans la MainScene ──

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        SceneManager.sceneLoaded += (s, m) => CreerSiBesoin();
        CreerSiBesoin(); // scène déjà chargée au lancement
    }

    static void CreerSiBesoin()
    {
        if (SceneManager.GetActiveScene().name != GameState.I.mainSceneName) return;
        if (FindFirstObjectByType<ClotureEnvironnement>() != null) return; // déjà là

        var go = new GameObject("[Cloture]");
        go.AddComponent<ClotureEnvironnement>();
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
        // 1) Le plateau de la carte mère = le renderer avec la plus grande
        //    surface au sol parmi les enfants de l'objet sol.
        var plateau = TrouverPlateau();
        if (plateau == null)
        {
            Debug.LogWarning($"[Cloture] Sol '{nomSol}' introuvable : pas de clôture.");
            return;
        }

        // 2) Les 4 coins SUPÉRIEURS du plateau, dans SON repère (suit la
        //    rotation et l'échelle de la carte), reculés vers l'intérieur.
        Vector3[] coins = CoinsSuperieurs(plateau, retrait);

        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;

        if (_racine != null) Destroy(_racine.gameObject);
        _racine = new GameObject("Cloture_Generee").transform;
        _racine.SetParent(transform, false);

        // 3) Un côté de clôture entre chaque paire de coins consécutifs.
        for (int i = 0; i < 4; i++)
        {
            Vector3 a = coins[i];
            Vector3 b = coins[(i + 1) % 4];
            CoteGenere(a, b);
            MurInvisible(a, b);
        }

        Debug.Log($"[Cloture] Enceinte construite sur les bords de '{plateau.name}' : " +
                  $"{Vector3.Distance(coins[0], coins[1]):0.#} × {Vector3.Distance(coins[1], coins[2]):0.#} m.");
    }

    /// <summary>Renderer du plateau : la plus grande surface horizontale sous l'objet sol.</summary>
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
        float meilleureAire = 0f;
        foreach (var r in sol.GetComponentsInChildren<Renderer>())
        {
            float aire = r.bounds.size.x * r.bounds.size.z;
            if (aire > meilleureAire) { meilleureAire = aire; meilleur = r; }
        }
        return meilleur;
    }

    /// <summary>
    /// Les 4 coins du DESSUS du plateau, calculés dans le repère local du
    /// renderer puis convertis en monde — la clôture épouse donc le plateau
    /// même s'il est tourné. 'recul' les ramène vers l'intérieur.
    /// </summary>
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

        // Recul horizontal vers le centre du plateau.
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

    // ── un côté : poteaux + lisses SEGMENTÉES (suivent la hauteur du bord) ──

    void CoteGenere(Vector3 a, Vector3 b)
    {
        float longueur = Vector3.Distance(a, b);
        int nbPoteaux  = Mathf.Max(2, Mathf.RoundToInt(longueur / espacementPoteaux) + 1);

        // Position (raffinée au sol) de chaque poteau le long du bord.
        var bases = new Vector3[nbPoteaux];
        for (int i = 0; i < nbPoteaux; i++)
        {
            Vector3 p = Vector3.Lerp(a, b, i / (nbPoteaux - 1f));
            bases[i] = AffinerAuSol(p);
            Poteau(bases[i]);
        }

        // Panneaux PLEINS (opaques) entre chaque paire de poteaux.
        for (int i = 0; i < nbPoteaux - 1; i++)
            PanneauPlein(bases[i], bases[i + 1]);
    }

    /// <summary>Mur plein opaque entre deux poteaux, sur toute la hauteur.</summary>
    void PanneauPlein(Vector3 a, Vector3 b)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "Panneau";
        go.transform.SetParent(_racine, false);
        go.transform.position   = (a + b) * 0.5f + Vector3.up * (hauteur * 0.5f);
        go.transform.rotation   = Quaternion.LookRotation(b - a);
        go.transform.localScale = new Vector3(0.25f, hauteur, Vector3.Distance(a, b) + 0.1f);
        Destroy(go.GetComponent<Collider>()); // la physique = mur invisible
        Peindre(go, 0.85f); // légèrement plus sombre que les poteaux
    }

    /// <summary>Petit raycast local : pose le pied du poteau sur la surface réelle
    /// de la carte (max ±2 m par rapport au bord mesuré, pour ne jamais
    /// « tomber » sur un sol plus bas hors de la carte).</summary>
    Vector3 AffinerAuSol(Vector3 p)
    {
        if (Physics.Raycast(p + Vector3.up * 2f, Vector3.down, out var hit, 4f))
            return hit.point;
        return p;
    }

    void Poteau(Vector3 basePos)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "Poteau";
        go.transform.SetParent(_racine, false);
        go.transform.position   = basePos + Vector3.up * (hauteur * 0.5f);
        go.transform.localScale = new Vector3(0.4f, hauteur, 0.4f);
        Destroy(go.GetComponent<Collider>());
        Peindre(go, 1f);

        var cap = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cap.name = "Chapeau";
        cap.transform.SetParent(_racine, false);
        cap.transform.position   = basePos + Vector3.up * (hauteur + 0.1f);
        cap.transform.localScale = new Vector3(0.55f, 0.2f, 0.55f);
        Destroy(cap.GetComponent<Collider>());
        Peindre(cap, 1.6f);
    }

    // ── barrière physique invisible ───────────────────────────────────────

    void MurInvisible(Vector3 a, Vector3 b)
    {
        var go = new GameObject("MurInvisible");
        go.transform.SetParent(_racine, false);

        Vector3 centre = (a + b) * 0.5f;
        go.transform.position = centre + Vector3.up * (hauteurCollider * 0.5f);
        go.transform.rotation = Quaternion.LookRotation(b - a);

        var box = go.AddComponent<BoxCollider>();
        box.size = new Vector3(0.3f, hauteurCollider + 4f, Vector3.Distance(a, b) + 0.3f);
    }

    // ── matériaux ─────────────────────────────────────────────────────────

    void Peindre(GameObject go, float eclat)
    {
        if (_matMetal == null)
        {
            var sh = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            _matMetal = new Material(sh);
            if (_matMetal.HasProperty("_BaseColor")) _matMetal.SetColor("_BaseColor", couleur);
            else _matMetal.color = couleur;
            if (_matMetal.HasProperty("_Metallic"))   _matMetal.SetFloat("_Metallic", 0.6f);
            if (_matMetal.HasProperty("_Smoothness")) _matMetal.SetFloat("_Smoothness", 0.45f);
        }

        var rend = go.GetComponent<Renderer>();
        if (Mathf.Approximately(eclat, 1f)) { rend.sharedMaterial = _matMetal; return; }

        var m = new Material(_matMetal);
        Color c = couleur * eclat; c.a = 1f;
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        else m.color = c;
        rend.sharedMaterial = m;
    }
}
