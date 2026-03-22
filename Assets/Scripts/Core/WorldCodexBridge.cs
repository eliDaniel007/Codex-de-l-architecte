using UnityEngine;
using UnityEngine.InputSystem;
using Codex.UI;
using StarterAssets;

namespace Codex.Core
{
    public class WorldCodexBridge : MonoBehaviour
    {
        public static WorldCodexBridge Instance { get; private set; }

        [Header("References")]
        [SerializeField] CodexTestUI codexUI;
        [SerializeField] GameObject player;

        [Header("Settings")]
        [SerializeField] Key closeKey = Key.Escape;

        bool _codexOpen;
        ThirdPersonController _tpc;
        StarterAssetsInputs _inputs;
        PlayerInput _playerInput;

        public bool IsCodexOpen => _codexOpen;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void Start()
        {
            if (player != null)
            {
                _tpc = player.GetComponent<ThirdPersonController>();
                _inputs = player.GetComponent<StarterAssetsInputs>();
                _playerInput = player.GetComponent<PlayerInput>();
            }

            if (codexUI == null)
                codexUI = FindFirstObjectByType<CodexTestUI>();
        }

        void Update()
        {
            if (!_codexOpen) return;
            if (Keyboard.current != null && Keyboard.current[closeKey].wasPressedThisFrame)
                CloseCodex();
        }

        public void OpenCodex(string puzzleId = null)
        {
            if (_codexOpen) return;
            _codexOpen = true;

            FreezePlayer();

            if (codexUI != null)
                codexUI.Show(puzzleId);
        }

        public void CloseCodex()
        {
            if (!_codexOpen) return;
            _codexOpen = false;

            if (codexUI != null)
                codexUI.Hide();

            UnfreezePlayer();
        }

        void FreezePlayer()
        {
            if (_inputs != null)
            {
                _inputs.move = Vector2.zero;
                _inputs.look = Vector2.zero;
                _inputs.jump = false;
                _inputs.sprint = false;
                _inputs.cursorInputForLook = false;
            }

            if (_tpc != null)
                _tpc.enabled = false;

            if (_playerInput != null)
                _playerInput.enabled = false;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        void UnfreezePlayer()
        {
            if (_playerInput != null)
                _playerInput.enabled = true;

            if (_tpc != null)
                _tpc.enabled = true;

            if (_inputs != null)
            {
                _inputs.cursorInputForLook = true;
                _inputs.cursorLocked = true;
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
