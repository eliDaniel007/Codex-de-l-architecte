using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Pose ce script sur le modèle de la RAM dans MainScene.
/// On entre dans la scène RAM automatiquement en approchant (détection par
/// distance horizontale, hauteur ignorée — la RAM peut être posée en hauteur).
/// Le dépôt/choix se fait À L'INTÉRIEUR de la RAM (clic sur les cases), donc on
/// transporte la box en main dans la scène RAM, on ne dépose plus à l'entrée.
/// </summary>
public class MemoryCell : MonoBehaviour
{
    [Header("Scène")]
    [Tooltip("Nom de la scène RAM à charger.")]
    public string ramSceneName = "RAM";
    [Tooltip("Tag du joueur (StarterAssets : 'Player').")]
    public string playerTag = "Player";

    [Header("Périmètre")]
    [Tooltip("Rayon (en mètres) autour de la RAM pour entrer. Règle-le avec le gizmo.")]
    public float triggerRadius = 3f;
    [Tooltip("Ignore la différence de hauteur (Y) joueur/RAM.")]
    public bool  ignorerHauteur = true;
    [Tooltip("Logs de distance dans la Console (pour calibrer le rayon).")]
    public bool  debug = true;

    private Transform _player;
    private bool      _armed = true;
    private float     _nextLog;

    void Start()
    {
        TrouverJoueur();

        // Anti re-entrée : si on revient juste de la RAM, ou si on est déjà dans
        // la zone au démarrage, on désarme jusqu'à ce que le joueur s'éloigne.
        if (GameState.I.ramJustVisited)
        {
            _armed = false;
            GameState.I.ramJustVisited = false;
        }
        else if (_player != null && DistanceAuJoueur() <= triggerRadius)
        {
            _armed = false;
        }

        Debug.Log($"[MemoryCell] Prêt sur '{name}' à {transform.position}. Rayon {triggerRadius} m. Cible : '{ramSceneName}'.");
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
            Debug.Log($"[MemoryCell] distance joueur = {dist:0.0} m (rayon {triggerRadius} m, armé={_armed})");
        }

        if (!_armed)
        {
            if (dist > triggerRadius * 1.5f) _armed = true; // hystérésis
            return;
        }

        if (dist <= triggerRadius)
            EntrerRAM();
    }

    float DistanceAuJoueur()
    {
        Vector3 a = _player.position;
        Vector3 b = transform.position;
        if (ignorerHauteur) { a.y = 0f; b.y = 0f; }
        return Vector3.Distance(a, b);
    }

    void EntrerRAM()
    {
        _armed = false;
        GameState.I.ramJustVisited = true;
        Debug.Log($"[MemoryCell] Joueur à portée → chargement de '{ramSceneName}'");
        SceneManager.LoadScene(ramSceneName);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 0.3f, 0.9f);
        Gizmos.DrawWireSphere(transform.position, triggerRadius);
        Gizmos.color = new Color(0f, 1f, 0.3f, 0.25f);
        Vector3 sol = new Vector3(transform.position.x, 0f, transform.position.z);
        Gizmos.DrawWireSphere(sol, triggerRadius);
    }
}
