using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Codex.Puzzle
{
    /// <summary>
    /// Manages the UI input fields for puzzle gaps.
    /// Creates input fields (text, dropdown, toggle) dynamically at runtime.
    /// No prefabs needed — everything is built from code.
    /// </summary>
    public class GapInputManager : MonoBehaviour
    {
        [Header("Container")]
        [SerializeField] Transform gapContainer;

        [Header("Colors")]
        [SerializeField] Color normalColor = new Color(0.2f, 0.22f, 0.28f, 1f);
        [SerializeField] Color errorColor = new Color(0.6f, 0.15f, 0.15f, 1f);
        [SerializeField] Color correctColor = new Color(0.15f, 0.5f, 0.25f, 1f);

        readonly Dictionary<int, GapInputEntry> _entries = new Dictionary<int, GapInputEntry>();

        struct GapInputEntry
        {
            public int GapIndex;
            public GapInputType Type;
            public GameObject Root;
            public TMP_InputField TextInput;
            public TMP_Dropdown Dropdown;
            public Toggle Toggle;
            public Image Background;
        }

        public void SetupGaps(List<GapDefinition> gaps)
        {
            ClearGaps();
            if (gaps == null) return;

            foreach (var gap in gaps)
                CreateGapInput(gap);
        }

        void CreateGapInput(GapDefinition gap)
        {
            if (gapContainer == null) return;

            // Root card
            var root = new GameObject($"Gap_{gap.gapIndex}");
            root.transform.SetParent(gapContainer, false);

            var rootRect = root.AddComponent<RectTransform>();
            var rootImg = root.AddComponent<Image>();
            rootImg.color = normalColor;
            var rootLE = root.AddComponent<LayoutElement>();
            rootLE.minWidth = 180;
            rootLE.preferredWidth = 220;
            rootLE.minHeight = 55;

            var rootVLG = root.AddComponent<VerticalLayoutGroup>();
            rootVLG.childForceExpandWidth = true;
            rootVLG.childForceExpandHeight = false;
            rootVLG.childControlWidth = true;
            rootVLG.childControlHeight = true;
            rootVLG.spacing = 4;
            rootVLG.padding = new RectOffset(8, 8, 4, 4);

            // Label
            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(root.transform, false);
            labelGO.AddComponent<RectTransform>();
            var labelTMP = labelGO.AddComponent<TextMeshProUGUI>();
            labelTMP.text = $"[{gap.gapIndex}] {gap.label}";
            labelTMP.fontSize = 13;
            labelTMP.color = new Color(0.7f, 0.8f, 0.9f);
            labelTMP.fontStyle = FontStyles.Bold;
            labelGO.AddComponent<LayoutElement>().minHeight = 18;

            var entry = new GapInputEntry
            {
                GapIndex = gap.gapIndex,
                Type = gap.inputType,
                Root = root,
                Background = rootImg
            };

            switch (gap.inputType)
            {
                case GapInputType.TextInput:
                case GapInputType.DragDrop:
                    entry.TextInput = BuildTextInput(root.transform, gap.placeholder);
                    break;
                case GapInputType.Dropdown:
                    entry.Dropdown = BuildDropdown(root.transform, gap.options);
                    break;
                case GapInputType.Toggle:
                    entry.Toggle = BuildToggle(root.transform, gap.label);
                    break;
            }

            _entries[gap.gapIndex] = entry;
        }

        TMP_InputField BuildTextInput(Transform parent, string placeholder)
        {
            var go = new GameObject("TextInput");
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();

            var bg = go.AddComponent<Image>();
            bg.color = new Color(0.12f, 0.13f, 0.17f, 1f);

            go.AddComponent<LayoutElement>().minHeight = 30;

            // Text area
            var textArea = new GameObject("TextArea");
            textArea.transform.SetParent(go.transform, false);
            var taRect = textArea.AddComponent<RectTransform>();
            taRect.anchorMin = Vector2.zero;
            taRect.anchorMax = Vector2.one;
            taRect.offsetMin = new Vector2(8, 2);
            taRect.offsetMax = new Vector2(-8, -2);

            // Placeholder
            var phGO = new GameObject("Placeholder");
            phGO.transform.SetParent(textArea.transform, false);
            var phRect = phGO.AddComponent<RectTransform>();
            phRect.anchorMin = Vector2.zero;
            phRect.anchorMax = Vector2.one;
            phRect.offsetMin = Vector2.zero;
            phRect.offsetMax = Vector2.zero;
            var phTMP = phGO.AddComponent<TextMeshProUGUI>();
            phTMP.text = placeholder;
            phTMP.fontSize = 16;
            phTMP.fontStyle = FontStyles.Italic;
            phTMP.color = new Color(0.5f, 0.5f, 0.5f, 0.6f);

            // Input text
            var txtGO = new GameObject("Text");
            txtGO.transform.SetParent(textArea.transform, false);
            var txtRect = txtGO.AddComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.offsetMin = Vector2.zero;
            txtRect.offsetMax = Vector2.zero;
            var txtTMP = txtGO.AddComponent<TextMeshProUGUI>();
            txtTMP.fontSize = 16;
            txtTMP.color = Color.white;

            var input = go.AddComponent<TMP_InputField>();
            input.textComponent = txtTMP;
            input.textViewport = taRect;
            input.placeholder = phTMP;
            input.fontAsset = txtTMP.font;
            input.pointSize = 16;

            return input;
        }

        TMP_Dropdown BuildDropdown(Transform parent, List<string> options)
        {
            var go = new GameObject("Dropdown");
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();

            var bg = go.AddComponent<Image>();
            bg.color = new Color(0.12f, 0.13f, 0.17f, 1f);

            go.AddComponent<LayoutElement>().minHeight = 30;

            // Label for selected value
            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(go.transform, false);
            var labelRect = labelGO.AddComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(8, 2);
            labelRect.offsetMax = new Vector2(-30, -2);
            var labelTMP = labelGO.AddComponent<TextMeshProUGUI>();
            labelTMP.fontSize = 16;
            labelTMP.color = Color.white;

            // Arrow indicator
            var arrowGO = new GameObject("Arrow");
            arrowGO.transform.SetParent(go.transform, false);
            var arrowRect = arrowGO.AddComponent<RectTransform>();
            arrowRect.anchorMin = new Vector2(1, 0);
            arrowRect.anchorMax = Vector2.one;
            arrowRect.offsetMin = new Vector2(-25, 2);
            arrowRect.offsetMax = new Vector2(-5, -2);
            var arrowTMP = arrowGO.AddComponent<TextMeshProUGUI>();
            arrowTMP.text = "▼";
            arrowTMP.fontSize = 14;
            arrowTMP.color = new Color(0.6f, 0.6f, 0.6f);
            arrowTMP.alignment = TextAlignmentOptions.Center;

            // Template for dropdown items
            var template = new GameObject("Template");
            template.transform.SetParent(go.transform, false);
            template.SetActive(false);
            var templateRect = template.AddComponent<RectTransform>();
            templateRect.anchorMin = new Vector2(0, 0);
            templateRect.anchorMax = new Vector2(1, 0);
            templateRect.pivot = new Vector2(0.5f, 1f);
            templateRect.sizeDelta = new Vector2(0, 150);
            var templateImg = template.AddComponent<Image>();
            templateImg.color = new Color(0.15f, 0.16f, 0.20f, 1f);
            var templateScroll = template.AddComponent<ScrollRect>();

            // Viewport
            var viewport = new GameObject("Viewport");
            viewport.transform.SetParent(template.transform, false);
            var vpRect = viewport.AddComponent<RectTransform>();
            vpRect.anchorMin = Vector2.zero;
            vpRect.anchorMax = Vector2.one;
            vpRect.offsetMin = Vector2.zero;
            vpRect.offsetMax = Vector2.zero;
            viewport.AddComponent<Image>().color = new Color(0.15f, 0.16f, 0.20f, 1f);
            viewport.AddComponent<Mask>().showMaskGraphic = true;
            templateScroll.viewport = vpRect;

            // Content
            var content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            var contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = Vector2.one;
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.sizeDelta = new Vector2(0, 30);
            templateScroll.content = contentRect;

            // Item
            var item = new GameObject("Item");
            item.transform.SetParent(content.transform, false);
            var itemRect = item.AddComponent<RectTransform>();
            itemRect.anchorMin = new Vector2(0, 0.5f);
            itemRect.anchorMax = new Vector2(1, 0.5f);
            itemRect.sizeDelta = new Vector2(0, 30);
            var itemToggle = item.AddComponent<Toggle>();

            var itemBg = new GameObject("Item Background");
            itemBg.transform.SetParent(item.transform, false);
            var itemBgRect = itemBg.AddComponent<RectTransform>();
            itemBgRect.anchorMin = Vector2.zero;
            itemBgRect.anchorMax = Vector2.one;
            itemBgRect.offsetMin = Vector2.zero;
            itemBgRect.offsetMax = Vector2.zero;
            itemBg.AddComponent<Image>().color = new Color(0.2f, 0.22f, 0.28f, 1f);

            var itemCheck = new GameObject("Item Checkmark");
            itemCheck.transform.SetParent(item.transform, false);
            var checkRect = itemCheck.AddComponent<RectTransform>();
            checkRect.anchorMin = new Vector2(0, 0);
            checkRect.anchorMax = new Vector2(0, 1);
            checkRect.offsetMin = new Vector2(4, 4);
            checkRect.offsetMax = new Vector2(24, -4);
            var checkTMP = itemCheck.AddComponent<TextMeshProUGUI>();
            checkTMP.text = "V";
            checkTMP.fontSize = 14;
            checkTMP.color = new Color(0.3f, 0.9f, 0.5f);
            checkTMP.alignment = TextAlignmentOptions.Center;

            itemToggle.targetGraphic = itemBg.GetComponent<Image>();
            itemToggle.graphic = checkTMP;

            var itemLabel = new GameObject("Item Label");
            itemLabel.transform.SetParent(item.transform, false);
            var ilRect = itemLabel.AddComponent<RectTransform>();
            ilRect.anchorMin = Vector2.zero;
            ilRect.anchorMax = Vector2.one;
            ilRect.offsetMin = new Vector2(28, 0);
            ilRect.offsetMax = new Vector2(-5, 0);
            var ilTMP = itemLabel.AddComponent<TextMeshProUGUI>();
            ilTMP.fontSize = 15;
            ilTMP.color = Color.white;

            // Build dropdown
            var dropdown = go.AddComponent<TMP_Dropdown>();
            dropdown.template = templateRect;
            dropdown.captionText = labelTMP;
            dropdown.itemText = ilTMP;
            dropdown.targetGraphic = bg;

            dropdown.ClearOptions();
            var optionList = new List<TMP_Dropdown.OptionData>
            {
                new TMP_Dropdown.OptionData("-- Choisir --")
            };
            foreach (var opt in options)
                optionList.Add(new TMP_Dropdown.OptionData(opt));
            dropdown.AddOptions(optionList);
            dropdown.value = 0;
            dropdown.RefreshShownValue();

            return dropdown;
        }

        Toggle BuildToggle(Transform parent, string label)
        {
            var go = new GameObject("Toggle");
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            go.AddComponent<LayoutElement>().minHeight = 30;

            var hlg = go.AddComponent<HorizontalLayoutGroup>();
            hlg.childForceExpandWidth = false;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.spacing = 10;
            hlg.padding = new RectOffset(4, 4, 2, 2);

            // Toggle background
            var bgGO = new GameObject("Background");
            bgGO.transform.SetParent(go.transform, false);
            bgGO.AddComponent<RectTransform>();
            var bgImg = bgGO.AddComponent<Image>();
            bgImg.color = new Color(0.3f, 0.15f, 0.15f, 1f);
            bgGO.AddComponent<LayoutElement>().preferredWidth = 50;

            // Checkmark
            var checkGO = new GameObject("Checkmark");
            checkGO.transform.SetParent(bgGO.transform, false);
            var checkRect = checkGO.AddComponent<RectTransform>();
            checkRect.anchorMin = Vector2.zero;
            checkRect.anchorMax = Vector2.one;
            checkRect.offsetMin = Vector2.zero;
            checkRect.offsetMax = Vector2.zero;
            var checkTMP = checkGO.AddComponent<TextMeshProUGUI>();
            checkTMP.text = "true";
            checkTMP.fontSize = 14;
            checkTMP.color = new Color(0.3f, 0.9f, 0.5f);
            checkTMP.alignment = TextAlignmentOptions.Center;
            checkTMP.fontStyle = FontStyles.Bold;

            // "false" label
            var falseGO = new GameObject("FalseLabel");
            falseGO.transform.SetParent(go.transform, false);
            falseGO.AddComponent<RectTransform>();
            var falseTMP = falseGO.AddComponent<TextMeshProUGUI>();
            falseTMP.text = "false";
            falseTMP.fontSize = 14;
            falseTMP.color = new Color(0.6f, 0.3f, 0.3f);
            falseGO.AddComponent<LayoutElement>().preferredWidth = 50;

            var toggle = go.AddComponent<Toggle>();
            toggle.targetGraphic = bgImg;
            toggle.graphic = checkTMP;
            toggle.isOn = false;

            // Visual feedback on toggle change
            toggle.onValueChanged.AddListener(isOn =>
            {
                bgImg.color = isOn
                    ? new Color(0.15f, 0.35f, 0.2f, 1f)
                    : new Color(0.3f, 0.15f, 0.15f, 1f);
                falseTMP.color = isOn
                    ? new Color(0.4f, 0.4f, 0.4f)
                    : new Color(0.6f, 0.3f, 0.3f);
            });

            return toggle;
        }

        // --- Data Collection ---

        public Dictionary<int, string> CollectAnswers()
        {
            var answers = new Dictionary<int, string>();
            foreach (var kvp in _entries)
            {
                var entry = kvp.Value;
                string answer = "";
                switch (entry.Type)
                {
                    case GapInputType.TextInput:
                    case GapInputType.DragDrop:
                        answer = entry.TextInput != null ? entry.TextInput.text : "";
                        break;
                    case GapInputType.Dropdown:
                        if (entry.Dropdown != null && entry.Dropdown.value > 0)
                            answer = entry.Dropdown.options[entry.Dropdown.value].text;
                        break;
                    case GapInputType.Toggle:
                        answer = entry.Toggle != null
                            ? (entry.Toggle.isOn ? "true" : "false")
                            : "false";
                        break;
                }
                answers[kvp.Key] = answer;
            }
            return answers;
        }

        // --- Visual Feedback ---

        public void HighlightErrors(List<GapResult> gapResults)
        {
            ResetColors();
            if (gapResults == null) return;
            foreach (var result in gapResults)
            {
                if (!_entries.ContainsKey(result.GapIndex)) continue;
                var entry = _entries[result.GapIndex];
                if (entry.Background != null)
                    entry.Background.color = result.IsCorrect ? correctColor : errorColor;
            }
        }

        public void ShowAllCorrect()
        {
            foreach (var entry in _entries.Values)
                if (entry.Background != null)
                    entry.Background.color = correctColor;
        }

        public void ResetColors()
        {
            foreach (var entry in _entries.Values)
                if (entry.Background != null)
                    entry.Background.color = normalColor;
        }

        void ClearGaps()
        {
            foreach (var entry in _entries.Values)
                if (entry.Root != null) Destroy(entry.Root);
            _entries.Clear();
        }
    }
}
