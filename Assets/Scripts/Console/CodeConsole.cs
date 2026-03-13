using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Codex.Console
{
    /// <summary>
    /// Manages the console output panel at the bottom of the Codex interface.
    /// Displays compilation messages, errors, hints from drone C-Sharp.
    /// Supports color-coded log levels and auto-scroll.
    /// </summary>
    public class CodeConsole : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] TMP_Text consoleText;
        [SerializeField] ScrollRect scrollRect;
        [SerializeField] Button clearButton;

        [Header("Settings")]
        [SerializeField] int maxLines = 100;
        [SerializeField] bool autoScroll = true;

        [Header("Colors")]
        [SerializeField] string infoColor = "#D4D4D4";
        [SerializeField] string successColor = "#4EC9B0";
        [SerializeField] string errorColor = "#F44747";
        [SerializeField] string warningColor = "#CCA700";
        [SerializeField] string hintColor = "#569CD6";

        readonly StringBuilder _logBuffer = new StringBuilder();
        int _lineCount;

        void Awake()
        {
            if (clearButton != null)
                clearButton.onClick.AddListener(Clear);
        }

        public void LogInfo(string message)
        {
            AppendLine(message, infoColor, ">");
        }

        public void LogSuccess(string message)
        {
            AppendLine(message, successColor, "[OK]");
        }

        public void LogError(string message)
        {
            AppendLine(message, errorColor, "[ERR]");
        }

        public void LogWarning(string message)
        {
            AppendLine(message, warningColor, "[!]");
        }

        public void LogHint(string message)
        {
            AppendLine(message, hintColor, "[?]");
        }

        void AppendLine(string message, string color, string prefix)
        {
            _lineCount++;

            if (_lineCount > maxLines)
            {
                TrimOldLines();
            }

            string timestamp = System.DateTime.Now.ToString("HH:mm:ss");
            _logBuffer.Append($"<color={color}>[{timestamp}] {prefix} {EscapeRichText(message)}</color>\n");

            RefreshDisplay();
        }

        void TrimOldLines()
        {
            string content = _logBuffer.ToString();
            int cutIndex = 0;
            int linesToRemove = _lineCount - maxLines + 10; // Remove in batches
            for (int i = 0; i < linesToRemove && cutIndex < content.Length; i++)
            {
                int next = content.IndexOf('\n', cutIndex);
                if (next < 0) break;
                cutIndex = next + 1;
            }

            _logBuffer.Remove(0, cutIndex);
            _lineCount -= linesToRemove;
        }

        void RefreshDisplay()
        {
            if (consoleText != null)
                consoleText.text = _logBuffer.ToString();

            if (autoScroll && scrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                scrollRect.verticalNormalizedPosition = 0f;
            }
        }

        static string EscapeRichText(string text)
        {
            return text.Replace("<", "<\u200B").Replace(">", "\u200B>");
        }

        public void Clear()
        {
            _logBuffer.Clear();
            _lineCount = 0;
            if (consoleText != null)
                consoleText.text = "";
        }

        public void LogCompilationStart()
        {
            LogInfo("Compilation en cours...");
        }

        public void LogCompilationSuccess()
        {
            LogSuccess("Compilation reussie !");
        }

        public void LogCompilationError(string details)
        {
            LogError("Erreur de compilation !");
            if (!string.IsNullOrEmpty(details))
                LogError($"  Détails : {details}");
        }
    }
}
