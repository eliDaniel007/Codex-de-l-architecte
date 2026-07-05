using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// EASTER EGG PÉDAGOGIQUE : un petit insecte sombre est CACHÉ quelque part
/// sur la carte mère (immobile, endroit aléatoire à chaque partie — il faut
/// vraiment chercher). Si le joueur le trouve et l'examine ([E]), il découvre
/// l'histoire du PREMIER BUG informatique : le papillon coincé dans le
/// Harvard Mark II, trouvé par l'équipe de Grace Hopper en 1947.
/// Badge « Chasseur de bugs » à la clé. Auto-créé dans la MainScene.
/// </summary>
public class BugDeHopper : MonoBehaviour
{
    const float DIST_EXAMEN = 2.5f;

    private Transform _corps;
    private Transform _joueur;
    private bool      _trouve;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        SceneManager.sceneLoaded += (s, m) => CreerSiBesoin();
        CreerSiBesoin();
    }

    static void CreerSiBesoin()
    {
        if (SceneManager.GetActiveScene().name != GameState.I.mainSceneName) return;
        if (FindFirstObjectByType<BugDeHopper>() != null) return;

        var go = new GameObject("[BugDeHopper]");
        go.AddComponent<BugDeHopper>();
    }

    void Start()
    {
        var joueurGO = GameObject.FindGameObjectWithTag("Player");
        if (joueurGO != null) _joueur = joueurGO.transform;

        ConstruireInsecte();
        _corps.position = PoserAuSol(TrouverCachette());
        _corps.rotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);
    }

    void Update()
    {
        if (_corps == null || _trouve || _joueur == null) return;

        // L'insecte est immobile : il faut le TROUVER.
        if (Vector3.Distance(_joueur.position, _corps.position) < DIST_EXAMEN)
        {
            PromptUI.Show("[E]  Examiner l'insecte...");
            if (AppuyeE()) Examiner();
        }
    }

    void Examiner()
    {
        _trouve = true;
        PromptUI.Hide();
        Badges.ChasseurDeBug();
        NotificationsUI.Afficher(
            "LE PREMIER BUG (1947)",
            "Un papillon coincé dans le Harvard Mark II — trouvé par l'équipe de Grace Hopper. " +
            "Depuis, on « débogue » les programmes !",
            new Color(0.6f, 0.9f, 0.4f));

        // L'insecte s'enfuit et disparaît.
        Destroy(_corps.gameObject, 1.2f);
        Destroy(gameObject, 1.5f);
    }

    // ── cachette ──────────────────────────────────────────────────────────

    Vector3 PoserAuSol(Vector3 p)
    {
        if (Physics.Raycast(p + Vector3.up * 3f, Vector3.down, out var hit, 8f))
            return hit.point + Vector3.up * 0.08f;
        return p;
    }

    /// <summary>Cachette aléatoire — différente à CHAQUE partie (graine liée à
    /// l'horloge) : parmi 14 candidats sur la carte, le plus loin du joueur.</summary>
    Vector3 TrouverCachette()
    {
        Bounds b = new Bounds(Vector3.zero, new Vector3(60f, 1f, 60f));
        var sol = GameObject.Find("Ground");
        if (sol != null)
        {
            var rend = sol.GetComponentInChildren<Renderer>();
            if (rend != null) b = rend.bounds;
        }

        var rnd = new System.Random(System.Environment.TickCount); // vraie graine aléatoire
        Vector3 meilleure = b.center;
        float meilleureDist = -1f;
        for (int i = 0; i < 14; i++)
        {
            float x = Mathf.Lerp(b.min.x + 4f, b.max.x - 4f, (float)rnd.NextDouble());
            float z = Mathf.Lerp(b.min.z + 4f, b.max.z - 4f, (float)rnd.NextDouble());
            var candidat = new Vector3(x, b.max.y, z);

            float d = _joueur != null ? Vector3.Distance(candidat, _joueur.position) : 0f;
            if (d > meilleureDist) { meilleureDist = d; meilleure = candidat; }
        }
        return meilleure;
    }

    // ── apparence : petit insecte sombre à pattes ─────────────────────────

    void ConstruireInsecte()
    {
        var racine = new GameObject("Insecte");
        _corps = racine.transform;
        _corps.localScale = Vector3.one * 0.65f; // petit : il faut ouvrir l'œil

        var sh  = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var mat = new Material(sh);
        var sombre = new Color(0.09f, 0.07f, 0.06f);
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", sombre);
        else mat.color = sombre;

        // Abdomen + tête (deux sphères écrasées)
        var abdomen = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        abdomen.transform.SetParent(_corps, false);
        abdomen.transform.localPosition = new Vector3(0f, 0.05f, -0.1f);
        abdomen.transform.localScale    = new Vector3(0.28f, 0.16f, 0.42f);
        Destroy(abdomen.GetComponent<Collider>());
        abdomen.GetComponent<Renderer>().sharedMaterial = mat;

        var tete = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        tete.transform.SetParent(_corps, false);
        tete.transform.localPosition = new Vector3(0f, 0.06f, 0.18f);
        tete.transform.localScale    = new Vector3(0.16f, 0.13f, 0.16f);
        Destroy(tete.GetComponent<Collider>());
        tete.GetComponent<Renderer>().sharedMaterial = mat;

        // 6 pattes (petits bâtonnets)
        for (int i = 0; i < 3; i++)
        {
            foreach (float signe in new[] { -1f, 1f })
            {
                var patte = GameObject.CreatePrimitive(PrimitiveType.Cube);
                patte.transform.SetParent(_corps, false);
                patte.transform.localPosition = new Vector3(signe * 0.18f, 0.03f, -0.15f + i * 0.14f);
                patte.transform.localRotation = Quaternion.Euler(0f, 0f, signe * 40f);
                patte.transform.localScale    = new Vector3(0.16f, 0.02f, 0.02f);
                Destroy(patte.GetComponent<Collider>());
                patte.GetComponent<Renderer>().sharedMaterial = mat;
            }
        }
    }

    static bool AppuyeE()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.E);
#endif
    }
}
