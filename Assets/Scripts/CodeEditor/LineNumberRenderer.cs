using System.Text;
using TMPro;
using UnityEngine;

namespace Codex.CodeEditor
{
    /// <summary>
    /// Renders line numbers alongside the code editor.
    /// Syncs scroll position with the main editor text.
    /// </summary>
    public class LineNumberRenderer : MonoBehaviour
    {
        [SerializeField] TMP_Text lineNumberText;
        [SerializeField] Color lineNumberColor = new Color(0.55f, 0.55f, 0.55f, 1f);

        int _lastLineCount = -1;

        public void UpdateLineNumbers(string codeText)
        {
            if (lineNumberText == null) return;

            int lineCount = 1;
            if (!string.IsNullOrEmpty(codeText))
            {
                for (int i = 0; i < codeText.Length; i++)
                {
                    if (codeText[i] == '\n') lineCount++;
                }
            }

            if (lineCount == _lastLineCount) return;
            _lastLineCount = lineCount;

            string hex = ColorUtility.ToHtmlStringRGBA(lineNumberColor);
            var sb = new StringBuilder();
            for (int i = 1; i <= lineCount; i++)
            {
                sb.Append($"<color=#{hex}>{i}</color>");
                if (i < lineCount) sb.Append('\n');
            }

            lineNumberText.text = sb.ToString();
        }

        public void SyncScrollPosition(float normalizedPosition)
        {
            // Called by the scroll rect to keep line numbers in sync
            if (lineNumberText != null)
            {
                var rt = lineNumberText.rectTransform;
                // Adjust anchoredPosition.y based on scroll
            }
        }
    }
}
