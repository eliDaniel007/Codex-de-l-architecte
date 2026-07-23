using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Pose ce script sur le modèle du clavier dans MainScene.
/// Détection par DISTANCE HORIZONTALE (la hauteur Y est ignorée), car le
/// clavier est posé en hauteur : un collider physique flotterait au-dessus
/// du sol et le joueur n'entrerait jamais dedans.
///
/// Le périmètre = un cercle de rayon 'triggerRadius' autour du clavier,
/// visible via le gizmo cyan. Ajuste 'triggerRadius' pour le coller au clavier.
/// </summary>
public class KeyboardTerminal : MonoBehaviour
{
    [Header("Scène")]
    [Tooltip("Nom de la scène Clavier à charger.")]
    public string clavierSceneName = "Clavier";
    [Tooltip("Tag du joueur (StarterAssets : 'Player').")]
    public string playerTag = "Player";

    [Header("Périmètre")]
    [Tooltip("Rayon (en mètres) autour du clavier pour entrer. Règle-le avec le gizmo.")]
    public float triggerRadius = 2.5f;
    [Tooltip("Ignore la différence de hauteur (Y) joueur/clavier. À garder coché ici.")]
    public bool  ignorerHauteur = true;
    [Tooltip("Logs de distance dans la Console (pour calibrer le rayon).")]
    public bool  debug = false;

    private Transform _player;
    private bool      _estPrincipal = true; // le clavier LOIN de la RAM (celui des missions)
    private bool      _promptAffiche;       // un message est affiché par CE clavier
    private float     _nextLog;

    void Start()
    {
        TrouverJoueur();
        _estPrincipal = CalculerPrincipal();
        Debug.Log($"[KeyboardTerminal] Prêt sur '{name}' (principal={_estPrincipal}). Rayon {triggerRadius} m.");
    }

    void TrouverJoueur()
    {
        var pg = GameObject.FindGameObjectWithTag(playerTag);
        if (pg != null) _player = pg.transform;
    }

    void Update()
    {
        if (_player == null) { TrouverJoueur(); if (_player == null) return; }

        float dist = DistanceAuJoueur();

        if (debug && Time.time >= _nextLog)
        {
            _nextLog = Time.time + 1f;
            Debug.Log($"[KeyboardTerminal] distance joueur = {dist:0.0} m (rayon {triggerRadius} m)");
        }

        // ── Ligne 3 : Console.ReadLine() se fait ICI — au clavier PRINCIPAL
        //    (celui laissé sur la carte, loin de la RAM), pas au doublon.
        var gs = GameState.I;
        var q  = gs.QueteActuelle();
        bool readLine = _estPrincipal && q != null && !q.complete &&
                        q.kind == QuestKind.SaisieEcran && gs.missionEtape == 1 && !gs.boxExists;
        bool proche = dist <= triggerRadius * 1.4f;

        if (readLine)
        {
            if (proche)
            {
                PromptUI.Show("[E]  <color=#00D9FF>Console.ReadLine()</color>  —  récupérer la valeur tapée par l'utilisateur");
                _promptAffiche = true;
                if (AppuyeE()) RecevoirReadLine();
            }
            else if (_promptAffiche) { PromptUI.Hide(); _promptAffiche = false; }
        }
        else if (_estPrincipal)
        {
            // MAUVAISE STATION : le joueur vient au clavier alors que la mission
            // l'attend ailleurs → message d'orientation. PAS pendant la période
            // de grâce (il vient peut-être de réussir une interaction ici même).
            string attendue = gs.StationAttendue();
            if (proche && !gs.EnGrace && attendue != "clavier" && attendue != "")
            {
                PromptUI.Show($"<color=#FF6B6B>Rien à faire au clavier !</color>  Va plutôt vers <b>{GameState.NomStation(attendue)}</b>.");
                _promptAffiche = true;
            }
            else if (!proche && _promptAffiche) { PromptUI.Hide(); _promptAffiche = false; }
        }

        // Le TERMINAL DE DÉCLARATION est définitivement retiré : le clavier ne
        // charge plus la scène Clavier. Les déclarations se font dans la RAM ;
        // le clavier ne sert plus qu'au Console.ReadLine() de la ligne 3.
    }

    /// <summary>L'utilisateur a « tapé » une suite de chiffres : elle arrive en main
    /// SANS nom (on ne sait pas encore que c'est y) — juste la valeur.</summary>
    void RecevoirReadLine()
    {
        PromptUI.Hide();
        string val = Random.Range(10, 100).ToString();
        GameState.I.SaisirLigne(val);
        Debug.Log($"[KeyboardTerminal] Console.ReadLine() → \"{val}\" (valeur sans nom en main).");
    }

    static bool AppuyeE()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.E);
#endif
    }

    /// <summary>Vrai si CE clavier est le principal : le plus ÉLOIGNÉ du portail RAM.</summary>
    bool CalculerPrincipal()
    {
        var tous = FindObjectsByType<KeyboardTerminal>(FindObjectsSortMode.None);
        if (tous.Length <= 1) return true;

        var ram = FindFirstObjectByType<LoadSceneOnPlayerEnter>();
        if (ram == null) return true;

        KeyboardTerminal meilleur = null;
        float best = -1f;
        foreach (var k in tous)
        {
            Vector3 a = k.transform.position, b = ram.transform.position;
            a.y = 0f; b.y = 0f;
            float d = Vector3.Distance(a, b);
            if (d > best) { best = d; meilleur = k; }
        }
        return meilleur == this;
    }

    float DistanceAuJoueur()
    {
        Vector3 a = _player.position;
        Vector3 b = transform.position;
        if (ignorerHauteur) { a.y = 0f; b.y = 0f; }
        return Vector3.Distance(a, b);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 0.9f);
        Gizmos.DrawWireSphere(transform.position, triggerRadius);
        // disque au sol pour visualiser le périmètre horizontal réel
        Gizmos.color = new Color(0f, 1f, 1f, 0.25f);
        Vector3 sol = new Vector3(transform.position.x, 0f, transform.position.z);
        Gizmos.DrawWireSphere(sol, triggerRadius);
    }
}
