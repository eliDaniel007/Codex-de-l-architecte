using Codex.Puzzle;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Codex.Core
{
    /// <summary>
    /// Attach to any 3D object in the world that should open a coding puzzle.
    /// When the player interacts (presses E or clicks), the Codex panel opens
    /// with the assigned puzzle.
    /// 
    /// Requires a Collider with isTrigger = true on the object.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class PuzzleTrigger : MonoBehaviour
    {
        [Header("Puzzle")]
        [SerializeField] PuzzleDefinition puzzle;

        [Header("Interaction")]
        [SerializeField] Key interactKey = Key.E;
        [SerializeField] float interactionRange = 3f;

        [Header("Visual Feedback")]
        [SerializeField] GameObject interactPrompt; // UI "Press E" prompt
        [SerializeField] GameObject glowEffect;     // Visual glow when nearby

        bool _playerInRange;
        bool _isCompleted;

        void Start()
        {
            if (interactPrompt != null) interactPrompt.SetActive(false);
            if (glowEffect != null) glowEffect.SetActive(false);
        }

        void Update()
        {
            if (_playerInRange && !_isCompleted &&
                Keyboard.current != null && Keyboard.current[interactKey].wasPressedThisFrame)
            {
                ActivatePuzzle();
            }
        }

        void ActivatePuzzle()
        {
            if (puzzle == null || GameManager.Instance == null) return;
            GameManager.Instance.TriggerPuzzle(puzzle);
        }

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            _playerInRange = true;
            if (interactPrompt != null && !_isCompleted)
                interactPrompt.SetActive(true);
            if (glowEffect != null && !_isCompleted)
                glowEffect.SetActive(true);
        }

        void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            _playerInRange = false;
            if (interactPrompt != null) interactPrompt.SetActive(false);
            if (glowEffect != null) glowEffect.SetActive(false);
        }

        public void MarkCompleted()
        {
            _isCompleted = true;
            if (interactPrompt != null) interactPrompt.SetActive(false);
            if (glowEffect != null) glowEffect.SetActive(false);
        }
    }
}
