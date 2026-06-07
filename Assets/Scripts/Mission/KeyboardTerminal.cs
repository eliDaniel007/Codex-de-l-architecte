using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Pose ce script sur le modèle du clavier dans MainScene.
/// Le PÉRIMÈTRE est défini par le Collider (trigger) attaché au clavier :
/// dès que le joueur entre dans ce collider, la scène Clavier se charge.
/// Plus de bouton [E]. Pour ajuster la zone, redimensionne le Box Collider
/// du clavier dans l'éditeur.
/// </summary>
[RequireComponent(typeof(Collider))]
public class KeyboardTerminal : MonoBehaviour
{
    [Header("Scène")]
    [Tooltip("Nom de la scène Clavier à charger.")]
    public string clavierSceneName = "Clavier";
    [Tooltip("Tag du joueur (StarterAssets : 'Player').")]
    public string playerTag = "Player";
    [Tooltip("Force le Collider attaché à devenir trigger au démarrage.")]
    public bool   forceTrigger = true;

    private Collider _col;
    private bool     _armed = true;

    void Start()
    {
        _col = GetComponent<Collider>();
        if (forceTrigger)
            foreach (var c in GetComponents<Collider>()) c.isTrigger = true;

        // Si le joueur est DÉJÀ dans le périmètre au chargement (ex: on revient
        // du clavier), on désarme jusqu'à ce qu'il en sorte → pas de re-entrée
        // immédiate en boucle.
        var pg = GameObject.FindGameObjectWithTag(playerTag);
        if (pg != null && _col != null && _col.bounds.Contains(pg.transform.position))
            _armed = false;

        Debug.Log($"[KeyboardTerminal] Prêt sur '{name}'. Périmètre = collider. Cible : '{clavierSceneName}'.");
    }

    void OnTriggerEnter(Collider other)
    {
        if (!_armed || !other.CompareTag(playerTag)) return;
        EntrerClavier();
    }

    void OnTriggerExit(Collider other)
    {
        // Le joueur quitte le périmètre → on ré-arme.
        if (other.CompareTag(playerTag)) _armed = true;
    }

    void EntrerClavier()
    {
        _armed = false;
        GameState.I.clavierJustVisited = true;
        Debug.Log($"[KeyboardTerminal] Joueur dans le périmètre → chargement de '{clavierSceneName}'");
        SceneManager.LoadScene(clavierSceneName);
    }

    void OnDrawGizmos()
    {
        var col = GetComponent<Collider>();
        if (col == null) return;
        Gizmos.color = new Color(0f, 1f, 1f, 0.18f);
        Gizmos.DrawCube(col.bounds.center, col.bounds.size);
        Gizmos.color = new Color(0f, 1f, 1f, 0.9f);
        Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
    }
}
