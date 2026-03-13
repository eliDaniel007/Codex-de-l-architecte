using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Codex.CodeEditor
{
    /// <summary>
    /// Main controller for the code editor panel.
    /// Handles code input, syntax highlighting overlay, and line numbers.
    /// 
    /// Architecture: uses an invisible TMP_InputField for actual editing,
    /// and a visible TMP_Text overlay for the highlighted rendering.
    /// </summary>
    public class CodeEditorController : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] TMP_InputField codeInput;

        [Header("Display")]
        [SerializeField] TMP_Text highlightedOverlay;
        [SerializeField] LineNumberRenderer lineNumbers;

        [Header("Scroll")]
        [SerializeField] ScrollRect scrollRect;

        [Header("Buttons")]
        [SerializeField] Button runButton;
        [SerializeField] Button saveButton;

        [Header("Settings")]
        [SerializeField] int tabSize = 4;

        bool _isReadOnly;
        string _currentCode = "";

        public event System.Action<string> OnRunClicked;
        public event System.Action<string> OnSaveClicked;
        public event System.Action<string> OnCodeChanged;

        void Awake()
        {
            if (codeInput != null)
            {
                codeInput.onValueChanged.AddListener(OnInputChanged);
                codeInput.lineType = TMP_InputField.LineType.MultiLineNewline;
                codeInput.richText = false;
            }

            if (runButton != null)
                runButton.onClick.AddListener(HandleRun);

            if (saveButton != null)
                saveButton.onClick.AddListener(HandleSave);
        }

        void OnDestroy()
        {
            if (codeInput != null)
                codeInput.onValueChanged.RemoveListener(OnInputChanged);
        }

        void OnInputChanged(string newText)
        {
            _currentCode = newText;
            RefreshHighlighting();
            lineNumbers?.UpdateLineNumbers(newText);
            OnCodeChanged?.Invoke(newText);
        }

        void RefreshHighlighting()
        {
            if (highlightedOverlay == null) return;

            // Use gap-aware highlighting if the code contains gap markers
            if (_currentCode.Contains("___GAP_"))
                highlightedOverlay.text = SyntaxHighlighter.HighlightWithGaps(_currentCode);
            else
                highlightedOverlay.text = SyntaxHighlighter.Highlight(_currentCode);
        }

        // --- Public API ---

        public void LoadCode(string code, bool isReadOnly = false)
        {
            _isReadOnly = isReadOnly;
            _currentCode = code ?? "";

            if (codeInput != null)
            {
                codeInput.text = _currentCode;
                codeInput.readOnly = isReadOnly;
            }

            RefreshHighlighting();
            lineNumbers?.UpdateLineNumbers(_currentCode);
        }

        public string GetCurrentCode()
        {
            return _currentCode;
        }

        public void Clear()
        {
            LoadCode("", false);
        }

        public void SetReadOnly(bool readOnly)
        {
            _isReadOnly = readOnly;
            if (codeInput != null)
                codeInput.readOnly = readOnly;
        }

        public void InsertTextAtCursor(string text)
        {
            if (codeInput == null || _isReadOnly) return;

            int pos = codeInput.caretPosition;
            string before = _currentCode.Substring(0, Mathf.Min(pos, _currentCode.Length));
            string after = _currentCode.Substring(Mathf.Min(pos, _currentCode.Length));
            string newCode = before + text + after;
            codeInput.text = newCode;
            codeInput.caretPosition = pos + text.Length;
        }

        public void HighlightLine(int lineNumber, Color color)
        {
            // Future: visual indicator for error lines
        }

        void HandleRun()
        {
            OnRunClicked?.Invoke(_currentCode);
        }

        void HandleSave()
        {
            OnSaveClicked?.Invoke(_currentCode);
        }
    }
}
