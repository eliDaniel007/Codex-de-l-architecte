using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Codex.CodeEditor
{
    /// <summary>
    /// Transforms raw C# code into TMP rich-text with syntax highlighting.
    /// Colors are tuned for a dark background to match the Codex UI theme.
    /// </summary>
    public static class SyntaxHighlighter
    {
        // --- Codex Dark Theme Palette ---
        static readonly string ColorKeyword   = "#C586C0"; // violet/pink
        static readonly string ColorType      = "#4EC9B0"; // teal
        static readonly string ColorString    = "#CE9178"; // warm orange
        static readonly string ColorNumber    = "#B5CEA8"; // light green
        static readonly string ColorComment   = "#6A9955"; // forest green
        static readonly string ColorMethod    = "#DCDCAA"; // yellow
        static readonly string ColorParam     = "#9CDCFE"; // light blue
        static readonly string ColorDefault   = "#D4D4D4"; // light gray
        static readonly string ColorClass     = "#4EC9B0"; // teal (same as type)
        static readonly string ColorBrace     = "#FFD700"; // gold for { }
        static readonly string ColorOperator  = "#D4D4D4"; // default

        static readonly string[] Keywords = {
            "using", "namespace", "class", "struct", "enum", "interface",
            "public", "private", "protected", "internal", "static", "readonly",
            "const", "abstract", "virtual", "override", "sealed", "partial",
            "void", "return", "new", "this", "base", "null", "true", "false",
            "if", "else", "switch", "case", "default", "break", "continue",
            "for", "foreach", "while", "do", "in",
            "try", "catch", "finally", "throw",
            "var", "get", "set", "value"
        };

        static readonly string[] BuiltInTypes = {
            "int", "float", "double", "string", "bool", "byte", "char",
            "long", "short", "decimal", "object", "dynamic",
            "Vector3", "Vector2", "Quaternion", "Transform", "GameObject",
            "Rigidbody", "Collider", "MonoBehaviour", "ScriptableObject",
            "Color", "Mathf", "Debug", "Input", "Time"
        };

        static readonly Regex RegexComment     = new Regex(@"(//.*?$|/\*[\s\S]*?\*/)", RegexOptions.Multiline | RegexOptions.Compiled);
        static readonly Regex RegexString      = new Regex(@"(""[^""\\]*(?:\\.[^""\\]*)*""|'[^'\\]*(?:\\.[^'\\]*)*'|@""[^""]*(?:""""[^""]*)*"")", RegexOptions.Compiled);
        static readonly Regex RegexNumber      = new Regex(@"\b(\d+\.?\d*f?)\b", RegexOptions.Compiled);
        static readonly Regex RegexMethodCall  = new Regex(@"\b([A-Za-z_]\w*)\s*(?=\()", RegexOptions.Compiled);

        struct Token
        {
            public int Start;
            public int Length;
            public string Color;
            public string Replacement;
        }

        public static string Highlight(string code)
        {
            if (string.IsNullOrEmpty(code)) return "";

            var tokens = new System.Collections.Generic.List<Token>();
            bool[] claimed = new bool[code.Length];

            void Claim(int start, int length, string color, string replacement = null)
            {
                for (int i = start; i < start + length && i < code.Length; i++)
                {
                    if (claimed[i]) return;
                }
                for (int i = start; i < start + length && i < code.Length; i++)
                {
                    claimed[i] = true;
                }
                tokens.Add(new Token { Start = start, Length = length, Color = color, Replacement = replacement });
            }

            // 1. Comments (highest priority)
            foreach (Match m in RegexComment.Matches(code))
                Claim(m.Index, m.Length, ColorComment);

            // 2. Strings
            foreach (Match m in RegexString.Matches(code))
                Claim(m.Index, m.Length, ColorString);

            // 3. Numbers
            foreach (Match m in RegexNumber.Matches(code))
                Claim(m.Groups[1].Index, m.Groups[1].Length, ColorNumber);

            // 4. Method calls
            foreach (Match m in RegexMethodCall.Matches(code))
            {
                string name = m.Groups[1].Value;
                bool isKeyword = System.Array.Exists(Keywords, k => k == name);
                bool isType = System.Array.Exists(BuiltInTypes, t => t == name);
                if (!isKeyword && !isType)
                    Claim(m.Groups[1].Index, m.Groups[1].Length, ColorMethod);
            }

            // 5. Keywords
            foreach (string kw in Keywords)
            {
                int idx = 0;
                while ((idx = code.IndexOf(kw, idx)) >= 0)
                {
                    bool validBefore = idx == 0 || !char.IsLetterOrDigit(code[idx - 1]) && code[idx - 1] != '_';
                    int end = idx + kw.Length;
                    bool validAfter = end >= code.Length || !char.IsLetterOrDigit(code[end]) && code[end] != '_';

                    if (validBefore && validAfter)
                        Claim(idx, kw.Length, ColorKeyword);

                    idx += kw.Length;
                }
            }

            // 6. Built-in types
            foreach (string t in BuiltInTypes)
            {
                int idx = 0;
                while ((idx = code.IndexOf(t, idx)) >= 0)
                {
                    bool validBefore = idx == 0 || !char.IsLetterOrDigit(code[idx - 1]) && code[idx - 1] != '_';
                    int end = idx + t.Length;
                    bool validAfter = end >= code.Length || !char.IsLetterOrDigit(code[end]) && code[end] != '_';

                    if (validBefore && validAfter)
                        Claim(idx, t.Length, ColorType);

                    idx += t.Length;
                }
            }

            // Sort tokens by start position
            tokens.Sort((a, b) => a.Start.CompareTo(b.Start));

            // Build result
            var sb = new StringBuilder(code.Length * 2);
            int pos = 0;

            foreach (var token in tokens)
            {
                if (token.Start > pos)
                {
                    sb.Append(Escape(code.Substring(pos, token.Start - pos)));
                }
                string text = token.Replacement ?? code.Substring(token.Start, token.Length);
                sb.Append($"<color={token.Color}>");
                sb.Append(Escape(text));
                sb.Append("</color>");
                pos = token.Start + token.Length;
            }

            if (pos < code.Length)
                sb.Append(Escape(code.Substring(pos)));

            return sb.ToString();
        }

        static string Escape(string text)
        {
            // TMP uses < > for tags, so we need to escape angle brackets that aren't our tags
            // But we already built our tags, so only escape in raw segments
            return text.Replace("<", "<\u200B").Replace(">", "\u200B>");
        }

        /// <summary>
        /// Returns a highlighted version suitable for display alongside an input field.
        /// Gap placeholders like ___GAP_0___ are replaced with underlined blanks.
        /// </summary>
        public static string HighlightWithGaps(string code, string gapPlaceholderPrefix = "___GAP_")
        {
            string highlighted = Highlight(code);
            // Replace gap markers with styled blanks
            var gapRegex = new Regex(@"___GAP_(\d+)___");
            highlighted = gapRegex.Replace(highlighted, match =>
            {
                string idx = match.Groups[1].Value;
                return $"<color=#FFD700><u>  [{idx}]  </u></color>";
            });
            return highlighted;
        }
    }
}
