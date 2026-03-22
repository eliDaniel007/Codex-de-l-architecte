using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

namespace Codex.Core
{
    [RequireComponent(typeof(Collider))]
    public class PuzzleTerminal : MonoBehaviour
    {
        [Header("Puzzle")]
        [Tooltip("ID du puzzle a lancer (ex: temple_01, pont_01)")]
        [SerializeField] string puzzleId;

        [Header("Interaction")]
        [SerializeField] Key interactKey = Key.E;

        [Header("Visual")]
        [SerializeField] Color glowColor = new Color(0.3f, 1f, 0.5f);
        [SerializeField] Color completedColor = new Color(0.2f, 0.5f, 1f);

        bool _playerInRange;
        bool _isCompleted;
        GameObject _promptUI;
        Renderer _renderer;
        Color _originalColor;
        static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

        void Start()
        {
            _renderer = GetComponentInChildren<Renderer>();
            if (_renderer != null)
                _originalColor = _renderer.material.color;

            var col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;

            CreatePromptUI();
        }

        void Update()
        {
            if (!_playerInRange || _isCompleted) return;
            if (WorldCodexBridge.Instance != null && WorldCodexBridge.Instance.IsCodexOpen) return;
            if (Keyboard.current != null && Keyboard.current[interactKey].wasPressedThisFrame)
                Interact();
        }

        void Interact()
        {
            if (WorldCodexBridge.Instance == null) return;
            WorldCodexBridge.Instance.OpenCodex(puzzleId);
            if (_promptUI != null) _promptUI.SetActive(false);
        }

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            _playerInRange = true;
            if (!_isCompleted && _promptUI != null)
                _promptUI.SetActive(true);
            if (!_isCompleted)
                SetGlow(true);
        }

        void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            _playerInRange = false;
            if (_promptUI != null)
                _promptUI.SetActive(false);
            SetGlow(false);
        }

        void SetGlow(bool on)
        {
            if (_renderer == null) return;
            if (on)
            {
                _renderer.material.EnableKeyword("_EMISSION");
                _renderer.material.SetColor(EmissionColor, glowColor * 0.5f);
            }
            else
            {
                _renderer.material.SetColor(EmissionColor, Color.black);
            }
        }

        public void MarkCompleted()
        {
            _isCompleted = true;
            if (_promptUI != null) _promptUI.SetActive(false);
            if (_renderer != null)
            {
                _renderer.material.EnableKeyword("_EMISSION");
                _renderer.material.SetColor(EmissionColor, completedColor * 0.3f);
            }
        }

        void CreatePromptUI()
        {
            var canvas = new GameObject("PromptCanvas");
            canvas.transform.SetParent(transform, false);
            canvas.transform.localPosition = Vector3.up * 2.5f;

            var c = canvas.AddComponent<Canvas>();
            c.renderMode = RenderMode.WorldSpace;
            c.GetComponent<RectTransform>().sizeDelta = new Vector2(2f, 0.5f);
            canvas.transform.localScale = Vector3.one * 0.01f;
            canvas.AddComponent<UnityEngine.UI.CanvasScaler>();

            var billboard = canvas.AddComponent<BillboardUI>();

            var textGO = new GameObject("Text");
            textGO.transform.SetParent(canvas.transform, false);
            var rt = textGO.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var bg = textGO.AddComponent<UnityEngine.UI.Image>();
            bg.color = new Color(0f, 0f, 0f, 0.7f);

            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(textGO.transform, false);
            var lrt = labelGO.AddComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = Vector2.zero;
            lrt.offsetMax = Vector2.zero;

            var tmp = labelGO.AddComponent<TextMeshProUGUI>();
            tmp.text = "[E] Interagir";
            tmp.fontSize = 28;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;
            tmp.raycastTarget = false;

            _promptUI = canvas;
            _promptUI.SetActive(false);
        }

        public string PuzzleId => puzzleId;
    }
}
