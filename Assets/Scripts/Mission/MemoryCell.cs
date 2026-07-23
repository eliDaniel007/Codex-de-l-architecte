using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Pose ce script sur le modèle de la RAM dans MainScene.
/// Quand le joueur entre dans le trigger, un prompt "[E] Entrer" s'affiche.
/// Le joueur choisit d'appuyer sur E pour charger la scène RAM.
/// </summary>
[RequireComponent(typeof(Collider))]
public class MemoryCell : MonoBehaviour
{
    [Tooltip("Nom de la scène à charger sur appui [E].")]
    public string ramSceneName = "RAM";

    [Tooltip("Tag du joueur (StarterAssets : 'Player').")]
    public string playerTag = "Player";

    [Tooltip("Si vrai, force le Collider attaché à devenir trigger au démarrage.")]
    public bool forceTrigger = true;

    [Tooltip("Si vrai, n'ouvre la RAM que si le joueur tient une box.")]
    public bool exigerBox = false;

    // Compat
    public bool    IsFilled => GameState.I.ramFilled;
    public DataBox Stored   => null;

    private bool _inRange;

    void Reset()
    {
        if (GetComponent<Collider>() == null)
        {
            var bc = gameObject.AddComponent<BoxCollider>();
            bc.isTrigger = true;
            bc.size = new Vector3(3f, 2f, 3f);
        }
    }

    void Start()
    {
        if (forceTrigger)
            foreach (var c in GetComponents<Collider>()) c.isTrigger = true;

        Debug.Log($"[MemoryCell] Prêt sur '{name}'. Cible scène : '{ramSceneName}'.");
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            _inRange = true;
            Debug.Log($"[MemoryCell] Joueur entré dans la zone.");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            _inRange = false;
            PromptUI.Hide();
        }
    }

    void Update()
    {
        if (!_inRange) return;

        var gs = GameState.I;
        string msg;

        if (gs.boxExists)
            msg = $"[E]  Entrer dans la RAM  (déposer  <color=#FFD700>{gs.boxType} {gs.boxVariable} = {gs.boxValue}</color>)";
        else if (gs.ramFilled)
            msg = $"[E]  Entrer dans la RAM  (contenu : <color=#00FF88>{gs.ramVariable} = {gs.ramValue}</color>)";
        else
            msg = "[E]  Entrer dans la RAM  (vide)";

        PromptUI.Show(msg);

        if (AppuyeE())
        {
            // ACCÈS CONTRÔLÉ : la RAM n'est accessible que si la mission t'y attend.
            string attendue = gs.StationAttendue();
            if (attendue != "ram" && attendue != "")
            {
                AudioFX.Erreur();
                PromptUI.Show(gs.BriefingEnAttente()
                    ? "<color=#FF6B6B>RAM verrouillée !</color>  Va d'abord au <b>CPU</b> lire la prochaine ligne."
                    : $"<color=#FF6B6B>RAM verrouillée !</color>  Tu dois aller vers <b>{GameState.NomStation(attendue)}</b>.");
                return;
            }

            if (exigerBox && !gs.boxExists)
            {
                Debug.Log("[MemoryCell] Pas de box → entrée refusée.");
                return;
            }

            // On garde la box en main : la scène RAM la range elle-même dans les
            // cases multiples (DeposerAuto) — ne pas la consommer ici.
            PromptUI.Hide();
            Debug.Log($"[MemoryCell] [E] pressé → chargement de '{ramSceneName}'");
            SceneManager.LoadScene(ramSceneName);
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

    void OnDrawGizmos()
    {
        var col = GetComponent<Collider>();
        if (col == null) return;
        Gizmos.color = new Color(0f, 1f, 0.3f, 0.25f);
        var b = col.bounds;
        Gizmos.DrawCube(b.center, b.size);
        Gizmos.color = new Color(0f, 1f, 0.3f);
        Gizmos.DrawWireCube(b.center, b.size);
    }
}
