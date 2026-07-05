using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Zone de proximité du Processeur (CPU) dans MainScene.
/// Dès que le joueur entre dans le périmètre, on charge automatiquement
/// la scène "CPU" où s'affichent les objectifs. Aucun bouton requis.
///
/// Détection par DISTANCE (pas besoin de Collider) : on mesure la distance
/// horizontale entre le joueur et cet objet. La hauteur (Y) est ignorée par
/// défaut, donc ça marche même si le Processeur est posé en hauteur.
/// Pose simplement ce script sur un GameObject placé au niveau du Processeur.
/// </summary>
public class CPUZone : MonoBehaviour
{
    [Header("Scène")]
    [Tooltip("Nom de la scène CPU à charger automatiquement.")]
    public string cpuSceneName = "CPU";
    [Tooltip("Tag du joueur (StarterAssets : 'Player').")]
    public string playerTag = "Player";

    [Header("Détection de proximité")]
    [Tooltip("Distance (en mètres) à laquelle on entre dans le CPU.")]
    public float triggerRadius = 4f;
    [Tooltip("Si vrai, ignore la différence de hauteur (Y) entre joueur et CPU.")]
    public bool  ignorerHauteur = true;

    private Transform _player;
    private bool      _armed = true;

    void Start()
    {
        TrouverJoueur();

        // En revenant juste de la scène CPU, on désarme : le joueur devra
        // d'abord s'éloigner avant de pouvoir re-déclencher (anti-boucle).
        if (GameState.I.cpuJustVisited)
        {
            _armed = false;
            GameState.I.cpuJustVisited = false;
        }

        Debug.Log($"[CPUZone] Prêt sur '{name}' (rayon {triggerRadius} m). Cible : '{cpuSceneName}'.");
    }

    void TrouverJoueur()
    {
        var pg = GameObject.FindGameObjectWithTag(playerTag);
        if (pg != null) _player = pg.transform;
    }

    void Update()
    {
        // Pas de changement de scène pendant la cinématique de briefing.
        if (BriefingCinematic.EnCours) return;

        if (_player == null) { TrouverJoueur(); if (_player == null) return; }

        float dist = DistanceAuJoueur();

        if (!_armed)
        {
            // Hystérésis : on ne ré-arme qu'une fois clairement éloigné.
            if (dist > triggerRadius * 1.5f) _armed = true;
            return;
        }

        if (dist <= triggerRadius)
            EntrerCPU();
    }

    float DistanceAuJoueur()
    {
        Vector3 a = _player.position;
        Vector3 b = transform.position;
        if (ignorerHauteur) { a.y = 0f; b.y = 0f; }
        return Vector3.Distance(a, b);
    }

    void EntrerCPU()
    {
        _armed = false;
        GameState.I.cpuJustVisited = true;

        // Missions 4 et 5 : si on apporte la bonne boîte, on va dans la scène
        // CALCULATEUR (le CPU traite la variable) au lieu de l'écran du programme.
        var gs = GameState.I;
        var q  = gs.QueteActuelle();
        bool versCalculateur = q != null && gs.boxExists &&
            ((q.kind == QuestKind.Parse  && gs.missionEtape == 0 && gs.boxVariable == "y") || // z = Int32.Parse(y)
             (q.kind == QuestKind.Calcul && gs.missionEtape == 0 && gs.boxVariable == "x") || // somme = x + z (x)
             (q.kind == QuestKind.Calcul && gs.missionEtape == 1 && gs.boxVariable == "z"));  // somme = x + z (z)

        string scene = versCalculateur ? "Calculateur" : cpuSceneName;
        Debug.Log($"[CPUZone] Joueur à portée → chargement de '{scene}'");
        SceneManager.LoadScene(scene);
    }

    // Bonus : si un Collider trigger est présent, on accepte aussi cette voie.
    void OnTriggerEnter(Collider other)
    {
        if (BriefingCinematic.EnCours) return;
        if (!_armed || !other.CompareTag(playerTag)) return;
        EntrerCPU();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 0.85f, 1f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, triggerRadius);
    }
}
