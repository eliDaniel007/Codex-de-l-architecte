using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Pose ce script sur le modèle du clavier dans MainScene.
/// Dès que le joueur s'approche (distance horizontale), on charge automatiquement
/// la scène Clavier. Plus de bouton [E].
///
/// Détection par DISTANCE (pas besoin de Collider). La hauteur (Y) est ignorée
/// par défaut, donc ça marche même si le clavier est posé en hauteur.
/// </summary>
public class KeyboardTerminal : MonoBehaviour
{
    [Header("Scène")]
    [Tooltip("Nom de la scène Clavier à charger automatiquement.")]
    public string clavierSceneName = "Clavier";
    [Tooltip("Tag du joueur (StarterAssets : 'Player').")]
    public string playerTag = "Player";

    [Header("Détection de proximité")]
    [Tooltip("Distance (en mètres) à laquelle on entre dans le clavier.")]
    public float triggerRadius = 3.5f;
    [Tooltip("Si vrai, ignore la différence de hauteur (Y) entre joueur et clavier.")]
    public bool  ignorerHauteur = true;

    private Transform _player;
    private bool      _armed = true;

    void Start()
    {
        TrouverJoueur();

        // En revenant juste de la scène Clavier, on désarme : le joueur devra
        // d'abord s'éloigner avant de pouvoir re-déclencher (anti-boucle).
        if (GameState.I.clavierJustVisited)
        {
            _armed = false;
            GameState.I.clavierJustVisited = false;
        }

        Debug.Log($"[KeyboardTerminal] Prêt sur '{name}' (rayon {triggerRadius} m). Cible : '{clavierSceneName}'.");
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

        if (!_armed)
        {
            // Hystérésis : on ne ré-arme qu'une fois clairement éloigné.
            if (dist > triggerRadius * 1.5f) _armed = true;
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

    // Bonus : si un Collider trigger est présent, on accepte aussi cette voie.
    void OnTriggerEnter(Collider other)
    {
        if (!_armed || !other.CompareTag(playerTag)) return;
        EntrerClavier();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, triggerRadius);
    }
}
