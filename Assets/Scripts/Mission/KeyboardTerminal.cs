using UnityEngine;
using UnityEngine.SceneManagement;

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
    private bool      _armed = true;
    private float     _nextLog;

    void Start()
    {
        TrouverJoueur();

        // Si on revient juste du clavier en étant déjà dans la zone, on désarme
        // jusqu'à ce qu'on s'éloigne (évite la re-entrée en boucle).
        if (_player != null && DistanceAuJoueur() <= triggerRadius)
            _armed = false;

        Debug.Log($"[KeyboardTerminal] Prêt sur '{name}' à {transform.position}. Rayon {triggerRadius} m. Cible : '{clavierSceneName}'.");
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
            Debug.Log($"[KeyboardTerminal] distance joueur = {dist:0.0} m (rayon {triggerRadius} m, armé={_armed})");
        }

        if (!_armed)
        {
            if (dist > triggerRadius * 1.5f) _armed = true;   // hystérésis
            return;
        }

        if (dist <= triggerRadius)
            EntrerClavier();
    }

    float DistanceAuJoueur()
    {
        Vector3 a = _player.position;
        Vector3 b = transform.position;
        if (ignorerHauteur) { a.y = 0f; b.y = 0f; }
        return Vector3.Distance(a, b);
    }

    void EntrerClavier()
    {
        _armed = false;
        GameState.I.clavierJustVisited = true;
        Debug.Log($"[KeyboardTerminal] Joueur à portée → chargement de '{clavierSceneName}'");
        SceneManager.LoadScene(clavierSceneName);
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
