using UnityEngine;
using UnityEngine.InputSystem;

namespace Codex.Dialogue
{
    [RequireComponent(typeof(Collider))]
    public class DialogueTrigger : MonoBehaviour
    {
        [SerializeField] DialogueData dialogue;
        [SerializeField] Key interactKey = Key.E;
        [SerializeField] bool playOnce = true;
        [SerializeField] bool autoPlay;

        bool _playerInRange;
        bool _hasPlayed;

        void Start()
        {
            var col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }

        void Update()
        {
            if (!_playerInRange || _hasPlayed && playOnce) return;
            if (DialogueUI.Instance != null && DialogueUI.Instance.IsPlaying) return;
            if (Keyboard.current != null && Keyboard.current[interactKey].wasPressedThisFrame)
                TriggerDialogue();
        }

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            _playerInRange = true;
            if (autoPlay && (!_hasPlayed || !playOnce))
                TriggerDialogue();
        }

        void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            _playerInRange = false;
        }

        void TriggerDialogue()
        {
            if (dialogue == null || DialogueUI.Instance == null) return;
            _hasPlayed = true;
            DialogueUI.Instance.Play(dialogue);
        }

        public void SetDialogue(DialogueData data) { dialogue = data; }
    }
}
