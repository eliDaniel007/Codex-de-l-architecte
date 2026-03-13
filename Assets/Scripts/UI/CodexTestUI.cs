using System;
using System.Collections.Generic;
using Codex.CodeEditor;
using Codex.Puzzle;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Codex.UI
{
    public class CodexTestUI : MonoBehaviour
    {
        PuzzleDefinition _puzzle;
        readonly Dictionary<int, string> _answers = new Dictionary<int, string>();
        readonly HashSet<string> _completed = new HashSet<string>();
        string _consoleLog = "";
        int _hintIndex;
        int _activeLessonIdx;
        int _activeTab;

        readonly List<TabInfo> _tabs = new List<TabInfo>();
        readonly List<LessonEntry> _lessons = new List<LessonEntry>();

        TMP_Text _titleText;
        TMP_Text _codeDisplay;
        TMP_Text _consoleText;
        Transform _tabBar;
        Transform _gapContent;
        GameObject _codeSection;
        GameObject _courseSection;
        Transform _courseContent;
        GameObject _gapLabel;
        GameObject _gapScroll;

        static readonly Color C_BG      = new Color(0.10f, 0.11f, 0.14f);
        static readonly Color C_CODE    = new Color(0.06f, 0.07f, 0.09f);
        static readonly Color C_INPUT   = new Color(0.15f, 0.16f, 0.20f);
        static readonly Color C_GAP_BG  = new Color(0.12f, 0.13f, 0.16f);
        static readonly Color C_GAP_ROW = new Color(0.14f, 0.15f, 0.18f);
        static readonly Color C_GREEN   = new Color(0.18f, 0.72f, 0.35f);
        static readonly Color C_BLUE    = new Color(0.30f, 0.50f, 0.85f);
        static readonly Color C_PURPLE  = new Color(0.55f, 0.30f, 0.80f);
        static readonly Color C_GOLD    = new Color(1f, 0.84f, 0f);
        static readonly Color C_WHITE   = Color.white;
        static readonly Color C_GRAY    = new Color(0.5f, 0.5f, 0.5f);
        static readonly Color C_CON     = new Color(0.7f, 0.9f, 0.7f);
        static readonly Color C_SEL     = new Color(0.18f, 0.55f, 0.35f);
        static readonly Color C_UNSEL   = new Color(0.20f, 0.21f, 0.25f);
        static readonly Color C_TAB_ON  = new Color(0.16f, 0.17f, 0.22f);
        static readonly Color C_TAB_OFF = new Color(0.08f, 0.09f, 0.11f);
        static readonly Color C_LOCKED  = new Color(0.25f, 0.25f, 0.25f);

        void Start()
        {
            DefineLessons();

            if (FindFirstObjectByType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<InputSystemUIInputModule>();
            }

            var canvasGO = new GameObject("CodexCanvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGO.AddComponent<GraphicRaycaster>();

            var panel = R(canvasGO.transform, "Panel");
            panel.anchorMin = new Vector2(0.5f, 0f);
            panel.anchorMax = Vector2.one;
            panel.offsetMin = new Vector2(8, 8);
            panel.offsetMax = new Vector2(-8, -8);
            Bg(panel, C_BG);
            VLG(panel.gameObject, 6, new RectOffset(12, 12, 10, 10));

            _titleText = T(panel, "Title", "", 18, C_WHITE, FontStyles.Bold);
            _titleText.alignment = TextAlignmentOptions.Center;
            _titleText.enableWordWrapping = true;
            L(_titleText.gameObject, minH: 28);

            // CODE SECTION
            _codeSection = new GameObject("CodeSection");
            _codeSection.transform.SetParent(panel, false);
            _codeSection.AddComponent<RectTransform>();
            L(_codeSection, flexH: 7);
            VLG(_codeSection, 4);

            var tabBarRT = R(_codeSection.transform, "TabBar");
            L(tabBarRT.gameObject, minH: 26);
            Bg(tabBarRT, C_TAB_OFF);
            var tabHlg = tabBarRT.gameObject.AddComponent<HorizontalLayoutGroup>();
            tabHlg.childForceExpandWidth = false;
            tabHlg.childForceExpandHeight = true;
            tabHlg.childControlWidth = true;
            tabHlg.childControlHeight = true;
            tabHlg.spacing = 2;
            tabHlg.padding = new RectOffset(4, 4, 0, 0);
            _tabBar = tabBarRT;

            var codeScroll = Scroll(_codeSection.transform, "CodeScroll", C_CODE, 100, 3);
            AddContentLayout(codeScroll.content, 10);
            _codeDisplay = T(codeScroll.content, "Code", "", 14, new Color(0.83f, 0.83f, 0.83f));
            _codeDisplay.richText = true;
            _codeDisplay.alignment = TextAlignmentOptions.TopLeft;
            _codeDisplay.enableWordWrapping = false;
            _codeDisplay.overflowMode = TextOverflowModes.Overflow;

            _gapLabel = new GameObject("GapLbl");
            _gapLabel.transform.SetParent(_codeSection.transform, false);
            _gapLabel.AddComponent<RectTransform>();
            L(_gapLabel, minH: 22);
            var gl = _gapLabel.AddComponent<TextMeshProUGUI>();
            gl.text = "REMPLIS LES GAPS :";
            gl.fontSize = 14; gl.color = C_GOLD;
            gl.fontStyle = FontStyles.Bold;
            gl.alignment = TextAlignmentOptions.Left;
            gl.raycastTarget = false;

            var gapScrollSR = Scroll(_codeSection.transform, "GapScroll", C_GAP_BG, 80, 2);
            _gapScroll = gapScrollSR.gameObject;
            _gapContent = gapScrollSR.content;
            VLG(_gapContent.gameObject, 6, new RectOffset(6, 6, 6, 6));
            _gapContent.gameObject.AddComponent<ContentSizeFitter>().verticalFit =
                ContentSizeFitter.FitMode.PreferredSize;

            // COURSE SECTION
            _courseSection = new GameObject("CourseSection");
            _courseSection.transform.SetParent(panel, false);
            _courseSection.AddComponent<RectTransform>();
            L(_courseSection, flexH: 7);
            VLG(_courseSection, 6, new RectOffset(4, 4, 4, 4));
            _courseSection.SetActive(false);

            var chdr = T(_courseSection.transform, "CourseHdr",
                "COURS C# -- Apprends les fondamentaux en sauvant Arcadia !",
                15, C_GOLD, FontStyles.Bold);
            chdr.alignment = TextAlignmentOptions.Center;
            L(chdr.gameObject, minH: 25);

            var courseScroll = Scroll(_courseSection.transform, "CourseScroll",
                new Color(0.08f, 0.09f, 0.11f), 100, 5);
            _courseContent = courseScroll.content;
            VLG(_courseContent.gameObject, 8, new RectOffset(8, 8, 8, 8));
            _courseContent.gameObject.AddComponent<ContentSizeFitter>().verticalFit =
                ContentSizeFitter.FitMode.PreferredSize;

            // BUTTONS
            var bar = R(panel, "Buttons");
            L(bar.gameObject, minH: 38);
            var bhlg = bar.gameObject.AddComponent<HorizontalLayoutGroup>();
            bhlg.childForceExpandWidth = true;
            bhlg.childForceExpandHeight = true;
            bhlg.childControlWidth = true;
            bhlg.childControlHeight = true;
            bhlg.spacing = 8;

            Btn(bar, "Run",  "> RUN",    C_GREEN,  OnRun);
            Btn(bar, "Hint", "? INDICE", C_BLUE,   OnHint);
            Btn(bar, "Cours","# COURS",  C_PURPLE, OnCours);

            // CONSOLE
            var conScroll = Scroll(panel, "ConScroll", C_CODE, 60, 2);
            AddContentLayout(conScroll.content, 8);
            _consoleText = T(conScroll.content, "ConText", "", 14, C_CON);
            _consoleText.richText = true;
            _consoleText.alignment = TextAlignmentOptions.TopLeft;

            _activeLessonIdx = 0;
            _puzzle = _lessons[0].create();
            ShowCode();
            LoadPuzzle();
        }

        void DefineLessons()
        {
            _lessons.Add(new LessonEntry {
                id = "temple_01", name = "Niv.1 -- Variables",
                desc = "Declare les bonnes variables pour activer le drone.",
                concept = "Variables",
                create = ExamplePuzzles.CreateVariablesPuzzle
            });
            _lessons.Add(new LessonEntry {
                id = "pont_01", name = "Niv.2 -- Conditions",
                desc = "Programme la condition du Golem pour passer.",
                concept = "if / else",
                create = ExamplePuzzles.CreateConditionsPuzzle,
                requiresId = "temple_01"
            });
            _lessons.Add(new LessonEntry {
                id = "vallee_01", name = "Niv.3 -- Boucles",
                desc = "Construis un pont de 10 blocs avec une boucle for.",
                concept = "for / while",
                create = ExamplePuzzles.CreateLoopPuzzle,
                requiresId = "pont_01"
            });
        }

        // === TABS ===

        void BuildTabs()
        {
            ClearChildren(_tabBar);
            _tabs.Clear();

            string mainName = _puzzle.puzzleName.Replace(" ", "") + ".cs";
            _tabs.Add(new TabInfo { name = mainName,
                code = SyntaxHighlighter.HighlightWithGaps(_puzzle.templateCode),
                isMain = true });

            if (_puzzle.additionalFiles != null)
            {
                for (int i = 0; i < _puzzle.additionalFiles.Count; i++)
                {
                    var af = _puzzle.additionalFiles[i];
                    _tabs.Add(new TabInfo { name = af.fileName,
                        code = SyntaxHighlighter.Highlight(af.code),
                        isMain = false });
                }
            }

            for (int i = 0; i < _tabs.Count; i++)
            {
                int idx = i;
                var tabGO = new GameObject("Tab" + i);
                tabGO.transform.SetParent(_tabBar, false);
                tabGO.AddComponent<RectTransform>();
                var tabLE = L(tabGO, minH: 24);
                tabLE.minWidth = 80;

                var img = tabGO.AddComponent<Image>();
                img.raycastTarget = true;
                var btn = tabGO.AddComponent<Button>();
                btn.targetGraphic = img;
                btn.onClick.AddListener(delegate { SwitchTab(idx); });

                var lbl = T(tabGO.transform, "L", _tabs[i].name, 12, C_WHITE);
                lbl.alignment = TextAlignmentOptions.Center;
                Stretch(lbl.rectTransform);
            }

            _activeTab = 0;
            RefreshTabs();
        }

        void SwitchTab(int idx)
        {
            if (idx < 0 || idx >= _tabs.Count) return;
            _activeTab = idx;
            _codeDisplay.text = _tabs[idx].code;
            bool main = _tabs[idx].isMain;
            _gapLabel.SetActive(main);
            _gapScroll.SetActive(main);
            RefreshTabs();
        }

        void RefreshTabs()
        {
            for (int i = 0; i < _tabBar.childCount; i++)
            {
                var img = _tabBar.GetChild(i).GetComponent<Image>();
                if (img != null) img.color = (i == _activeTab) ? C_TAB_ON : C_TAB_OFF;
                var txt = _tabBar.GetChild(i).GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null) txt.color = (i == _activeTab) ? C_WHITE : C_GRAY;
            }
        }

        // === PUZZLE ===

        void LoadPuzzle()
        {
            if (_puzzle == null) return;
            _titleText.text = _puzzle.puzzleName + " -- " + _puzzle.description;
            _answers.Clear();
            _hintIndex = 0;
            _consoleLog = "";
            _consoleText.text = "";
            BuildTabs();
            ClearChildren(_gapContent);
            foreach (var gap in _puzzle.gaps) BuildGap(gap);
            _codeDisplay.text = _tabs[0].code;
            _gapLabel.SetActive(true);
            _gapScroll.SetActive(true);
            Log("Puzzle charge ! Remplis les gaps et clique RUN.");
        }

        void BuildGap(GapDefinition gap)
        {
            var row = R(_gapContent, "Gap" + gap.gapIndex);
            Bg(row, C_GAP_ROW);
            L(row.gameObject, minH: 55);
            VLG(row.gameObject, 2, new RectOffset(8, 8, 4, 4));

            var lbl = T(row, "Lbl", "[" + gap.gapIndex + "] " + gap.label,
                13, C_GOLD, FontStyles.Bold);
            lbl.alignment = TextAlignmentOptions.Left;
            L(lbl.gameObject, minH: 18);

            if (gap.inputType == GapInputType.TextInput)
                BuildTextGap(row, gap);
            else
                BuildChoiceGap(row, gap);
        }

        void BuildTextGap(RectTransform parent, GapDefinition gap)
        {
            var inputGO = new GameObject("In" + gap.gapIndex);
            inputGO.transform.SetParent(parent, false);
            inputGO.AddComponent<RectTransform>();
            L(inputGO, minH: 30);
            inputGO.AddComponent<Image>().color = C_INPUT;

            var area = new GameObject("Text Area");
            area.transform.SetParent(inputGO.transform, false);
            var areaR = area.AddComponent<RectTransform>();
            areaR.anchorMin = Vector2.zero;
            areaR.anchorMax = Vector2.one;
            areaR.offsetMin = new Vector2(8, 2);
            areaR.offsetMax = new Vector2(-8, -2);
            area.AddComponent<RectMask2D>();

            var ph = new GameObject("Placeholder");
            ph.transform.SetParent(area.transform, false);
            Stretch(ph.AddComponent<RectTransform>());
            var phT = ph.AddComponent<TextMeshProUGUI>();
            phT.text = gap.placeholder != null ? gap.placeholder : "...";
            phT.fontSize = 15;
            phT.fontStyle = FontStyles.Italic;
            phT.color = C_GRAY;
            phT.raycastTarget = false;

            var txt = new GameObject("Text");
            txt.transform.SetParent(area.transform, false);
            Stretch(txt.AddComponent<RectTransform>());
            var txtT = txt.AddComponent<TextMeshProUGUI>();
            txtT.fontSize = 15;
            txtT.color = C_WHITE;
            txtT.raycastTarget = false;

            var field = inputGO.AddComponent<TMP_InputField>();
            field.textComponent = txtT;
            field.textViewport = areaR;
            field.placeholder = phT;
            field.pointSize = 15;
            field.lineType = TMP_InputField.LineType.SingleLine;

            int idx = gap.gapIndex;
            field.onValueChanged.AddListener(delegate(string v) { _answers[idx] = v; });
        }

        void BuildChoiceGap(RectTransform parent, GapDefinition gap)
        {
            List<string> options;
            if (gap.inputType == GapInputType.Toggle)
                options = new List<string> { "true", "false" };
            else
                options = gap.options;

            var choiceRow = R(parent, "Choices" + gap.gapIndex);
            L(choiceRow.gameObject, minH: 30);
            var hlg = choiceRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.spacing = 4;

            var images = new List<Image>();
            int idx = gap.gapIndex;

            for (int i = 0; i < options.Count; i++)
            {
                string val = options[i];
                var optGO = new GameObject("Opt_" + val);
                optGO.transform.SetParent(choiceRow, false);
                optGO.AddComponent<RectTransform>();

                var img = optGO.AddComponent<Image>();
                img.color = C_UNSEL;
                img.raycastTarget = true;
                images.Add(img);

                var btn = optGO.AddComponent<Button>();
                btn.targetGraphic = img;

                var label = new GameObject("L");
                label.transform.SetParent(optGO.transform, false);
                Stretch(label.AddComponent<RectTransform>());
                var lTmp = label.AddComponent<TextMeshProUGUI>();
                lTmp.text = val;
                lTmp.fontSize = 14;
                lTmp.color = C_WHITE;
                lTmp.alignment = TextAlignmentOptions.Center;
                lTmp.raycastTarget = false;

                Image capturedImg = img;
                List<Image> capturedList = images;
                btn.onClick.AddListener(delegate
                {
                    _answers[idx] = val;
                    for (int j = 0; j < capturedList.Count; j++)
                        capturedList[j].color = C_UNSEL;
                    capturedImg.color = C_SEL;
                });
            }
        }

        // === COURSE VIEW ===

        void ShowCode()
        {
            _codeSection.SetActive(true);
            _courseSection.SetActive(false);
        }

        void ShowCourse()
        {
            _codeSection.SetActive(false);
            _courseSection.SetActive(true);
            _titleText.text = "COURS C# -- Progression";
            BuildCourseList();
        }

        void BuildCourseList()
        {
            ClearChildren(_courseContent);
            for (int i = 0; i < _lessons.Count; i++)
            {
                var lesson = _lessons[i];
                bool done = _completed.Contains(lesson.id);
                bool locked = lesson.requiresId != null
                    && !_completed.Contains(lesson.requiresId);
                int idx = i;

                var row = R(_courseContent, "Lesson" + i);
                Bg(row, locked ? C_LOCKED : (done ? C_SEL : C_GAP_ROW));
                L(row.gameObject, minH: 70);
                VLG(row.gameObject, 2, new RectOffset(12, 12, 6, 6));

                string status = done ? "<color=#4EC9B0>[COMPLETE]</color>"
                    : (locked ? "<color=#F44747>[VERROUILLE]</color>"
                    : "<color=#FFD700>[DISPONIBLE]</color>");

                var nameT = T(row, "Name", status + "  " + lesson.name,
                    16, C_WHITE, FontStyles.Bold);
                nameT.richText = true;
                nameT.alignment = TextAlignmentOptions.Left;
                L(nameT.gameObject, minH: 22);

                var descT = T(row, "Desc", lesson.desc + "   (" + lesson.concept + ")",
                    13, C_GRAY);
                descT.alignment = TextAlignmentOptions.Left;
                L(descT.gameObject, minH: 18);

                if (!locked)
                {
                    var rowImg = row.GetComponent<Image>();
                    if (rowImg != null) rowImg.raycastTarget = true;
                    var btn = row.gameObject.AddComponent<Button>();
                    btn.targetGraphic = rowImg;
                    btn.onClick.AddListener(delegate { SelectLesson(idx); });
                }
            }
        }

        void SelectLesson(int idx)
        {
            _activeLessonIdx = idx;
            _puzzle = _lessons[idx].create();
            ShowCode();
            LoadPuzzle();
        }

        // === CALLBACKS ===

        void OnRun()
        {
            if (_puzzle == null || _courseSection.activeSelf) return;

            var result = PuzzleValidator.Validate(_puzzle, _answers);
            Log("--- Validation ---");

            for (int i = 0; i < result.GapResults.Count; i++)
            {
                var gr = result.GapResults[i];
                string c = gr.IsCorrect ? "#4EC9B0" : "#F44747";
                string icon = gr.IsCorrect ? "[OK]" : "[X]";
                Log("<color=" + c + ">" + icon + " [" + gr.GapIndex + "] "
                    + gr.Label + " : " + gr.Message + "</color>");
            }

            Log("Score : " + result.CorrectCount + "/" + result.TotalCount);

            if (result.IsSuccess)
            {
                Log("<color=#4EC9B0>" + result.Message + "</color>");
                _completed.Add(_puzzle.puzzleId);

                bool allDone = true;
                for (int i = 0; i < _lessons.Count; i++)
                    if (!_completed.Contains(_lessons[i].id)) { allDone = false; break; }

                if (allDone)
                    Log("<color=#FFD700>Felicitations ! Tu as termine tous les niveaux !</color>");
                else if (_activeLessonIdx < _lessons.Count - 1)
                    Log("<color=#FFD700>Niveau suivant debloque ! Clique COURS pour continuer.</color>");
            }
            else
                Log("<color=#F44747>" + result.Message + "</color>");
        }

        void OnHint()
        {
            if (_puzzle == null || _courseSection.activeSelf) return;
            if (_puzzle.hints == null || _puzzle.hints.Count == 0)
            {
                Log("<color=#FFD700>Pas d indice disponible.</color>");
                return;
            }
            if (_hintIndex < _puzzle.hints.Count)
            {
                Log("<color=#FFD700>[?] " + _puzzle.hints[_hintIndex] + "</color>");
                _hintIndex++;
            }
            else
                Log("<color=#FFD700>Tu as utilise tous les indices !</color>");
        }

        void OnCours()
        {
            if (_courseSection.activeSelf) ShowCode();
            else ShowCourse();
        }

        void Log(string msg)
        {
            _consoleLog += msg + "\n";
            if (_consoleText != null) _consoleText.text = _consoleLog;
        }

        // === UI HELPERS ===

        RectTransform R(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.AddComponent<RectTransform>();
        }

        Image Bg(RectTransform rt, Color c)
        {
            var img = rt.gameObject.AddComponent<Image>();
            img.color = c;
            img.raycastTarget = false;
            return img;
        }

        TextMeshProUGUI T(Transform parent, string name, string text,
            float size, Color color, FontStyles style = FontStyles.Normal)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.fontStyle = style;
            tmp.raycastTarget = false;
            return tmp;
        }

        LayoutElement L(GameObject go, float minH = -1, float flexH = -1)
        {
            var le = go.GetComponent<LayoutElement>();
            if (le == null) le = go.AddComponent<LayoutElement>();
            if (minH >= 0) le.minHeight = minH;
            if (flexH >= 0) le.flexibleHeight = flexH;
            return le;
        }

        VerticalLayoutGroup VLG(GameObject go, float spacing, RectOffset padding = null)
        {
            var v = go.AddComponent<VerticalLayoutGroup>();
            v.childForceExpandWidth = true;
            v.childForceExpandHeight = false;
            v.childControlWidth = true;
            v.childControlHeight = true;
            v.spacing = spacing;
            if (padding != null) v.padding = padding;
            return v;
        }

        void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        ScrollRect Scroll(Transform parent, string name, Color bg, float minH, float flexH)
        {
            var root = R(parent, name);
            L(root.gameObject, minH, flexH);
            Bg(root, bg);

            var viewport = R(root, "Viewport");
            Stretch(viewport);
            viewport.gameObject.AddComponent<RectMask2D>();
            viewport.gameObject.AddComponent<Image>().color = Color.clear;

            var content = R(viewport, "Content");
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = new Vector2(1, 1);
            content.pivot = new Vector2(0, 1);
            content.sizeDelta = Vector2.zero;

            var sr = root.gameObject.AddComponent<ScrollRect>();
            sr.viewport = viewport;
            sr.content = content;
            sr.horizontal = false;
            sr.vertical = true;
            sr.movementType = ScrollRect.MovementType.Clamped;
            sr.scrollSensitivity = 25;

            return sr;
        }

        void AddContentLayout(Transform content, float pad)
        {
            VLG(content.gameObject, 0, new RectOffset((int)pad, (int)pad, (int)pad, (int)pad));
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit =
                ContentSizeFitter.FitMode.PreferredSize;
        }

        void Btn(Transform parent, string name, string label, Color color,
            UnityEngine.Events.UnityAction action)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            var img = go.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = true;
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(action);

            var lbl = new GameObject("L");
            lbl.transform.SetParent(go.transform, false);
            Stretch(lbl.AddComponent<RectTransform>());
            var tmp = lbl.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 17;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = C_WHITE;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
        }

        void ClearChildren(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
                Destroy(parent.GetChild(i).gameObject);
        }

        struct TabInfo
        {
            public string name;
            public string code;
            public bool isMain;
        }

        struct LessonEntry
        {
            public string id;
            public string name;
            public string desc;
            public string concept;
            public string requiresId;
            public Func<PuzzleDefinition> create;
        }
    }
}
