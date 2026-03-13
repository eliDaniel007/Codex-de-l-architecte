#if UNITY_EDITOR
using Codex.CodeEditor;
using Codex.Console;
using Codex.Core;
using Codex.Course;
using Codex.Puzzle;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Codex.UI
{
    public static class CodexSceneSetup
    {
        static TMP_FontAsset _font;

        [MenuItem("Codex/Setup Codex Panel")]
        public static void SetupCodexPanel()
        {
            _font = TMP_Settings.defaultFontAsset;
            if (_font == null)
            {
                var guids = AssetDatabase.FindAssets("t:TMP_FontAsset");
                if (guids.Length > 0)
                    _font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                        AssetDatabase.GUIDToAssetPath(guids[0]));
            }
            if (_font == null)
            {
                EditorUtility.DisplayDialog("Codex Setup",
                    "Police TMP introuvable ! Importez TextMeshPro d'abord.", "OK");
                return;
            }

            // --- CLEANUP ---
            DestroyIfExists("CodexCanvas");
            DestroyIfExists("--- Codex Managers ---");
            DestroyIfExists("PlayerProgress");
            DestroyIfExists("EventSystem");

            // === EVENT SYSTEM (CRITICAL for all UI interaction) ===
            var esGO = new GameObject("EventSystem");
            Undo.RegisterCreatedObjectUndo(esGO, "Create EventSystem");
            esGO.AddComponent<EventSystem>();
            esGO.AddComponent<InputSystemUIInputModule>();

            // === CANVAS ===
            var canvasGO = new GameObject("CodexCanvas");
            Undo.RegisterCreatedObjectUndo(canvasGO, "Create Codex");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGO.AddComponent<GraphicRaycaster>();

            // === PANEL ROOT (right half of screen) ===
            var panelRoot = Bg(canvasGO.transform, "CodexPanel", new Color(0.10f, 0.11f, 0.14f));
            var panelRect = panelRoot.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0f);
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            var cg = panelRoot.AddComponent<CanvasGroup>();
            cg.interactable = true;
            cg.blocksRaycasts = true;
            cg.alpha = 1f;

            var vlg = panelRoot.AddComponent<VerticalLayoutGroup>();
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlHeight = true;
            vlg.childControlWidth = true;
            vlg.spacing = 2;
            vlg.padding = new RectOffset(4, 4, 4, 4);

            // ============ 1. HEADER ============
            var header = Bg(panelRoot.transform, "Header", new Color(0.14f, 0.15f, 0.20f));
            header.AddComponent<LayoutElement>().minHeight = 50;
            var hHLG = header.AddComponent<HorizontalLayoutGroup>();
            hHLG.childForceExpandWidth = false;
            hHLG.childForceExpandHeight = false;
            hHLG.childControlWidth = true;
            hHLG.childControlHeight = true;
            hHLG.childAlignment = TextAnchor.MiddleLeft;
            hHLG.spacing = 10;
            hHLG.padding = new RectOffset(15, 15, 5, 5);

            var headerTitle = Txt(header.transform, "HeaderTitle", "", 22, Color.white, FontStyles.Bold);
            headerTitle.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;

            var runBtn = Btn(header.transform, "RunButton", "> Run",
                new Color(0.18f, 0.72f, 0.35f), 90);
            var saveBtn = Btn(header.transform, "SaveButton", "Save",
                new Color(0.30f, 0.42f, 0.65f), 90);

            // ============ 2. TAB BAR ============
            var tabBar = Bg(panelRoot.transform, "TabBar", new Color(0.12f, 0.13f, 0.17f));
            tabBar.GetComponent<Image>().raycastTarget = false;
            tabBar.AddComponent<LayoutElement>().minHeight = 38;
            var tbHLG = tabBar.AddComponent<HorizontalLayoutGroup>();
            tbHLG.childForceExpandWidth = false;
            tbHLG.childForceExpandHeight = true;
            tbHLG.childControlWidth = true;
            tbHLG.childControlHeight = true;
            tbHLG.spacing = 2;
            tbHLG.padding = new RectOffset(5, 5, 2, 2);

            // ============ 3. CODE EDITOR ============
            var editorArea = Bg(panelRoot.transform, "CodeEditorArea", new Color(0.12f, 0.14f, 0.18f));
            editorArea.GetComponent<Image>().raycastTarget = false;
            var eaLE = editorArea.AddComponent<LayoutElement>();
            eaLE.flexibleHeight = 3;
            eaLE.minHeight = 150;
            var eaHLG = editorArea.AddComponent<HorizontalLayoutGroup>();
            eaHLG.childForceExpandHeight = true;
            eaHLG.childControlWidth = true;
            eaHLG.childControlHeight = true;
            eaHLG.spacing = 0;
            eaHLG.padding = new RectOffset(0, 0, 5, 5);

            // Line numbers
            var lineCol = Bg(editorArea.transform, "LineNumbers", new Color(0.10f, 0.11f, 0.14f));
            lineCol.GetComponent<Image>().raycastTarget = false;
            var lcLE = lineCol.AddComponent<LayoutElement>();
            lcLE.preferredWidth = 55;
            lcLE.minWidth = 55;
            var lineNumText = Txt(lineCol.transform, "LineNumText", "", 18, new Color(0.55f, 0.55f, 0.55f));
            lineNumText.alignment = TextAlignmentOptions.TopRight;
            Stretch(lineNumText, 2, 2, -6, -2);

            // Code area
            var codeArea = Bg(editorArea.transform, "CodeArea", new Color(0.12f, 0.14f, 0.18f));
            codeArea.GetComponent<Image>().raycastTarget = false;
            codeArea.AddComponent<LayoutElement>().flexibleWidth = 1;

            // Highlighted overlay (display only — NO raycast)
            var hlText = Txt(codeArea.transform, "HighlightedText", "", 18, new Color(0.83f, 0.83f, 0.83f));
            hlText.richText = true;
            hlText.alignment = TextAlignmentOptions.TopLeft;
            hlText.raycastTarget = false;
            Stretch(hlText, 10, 2, -10, -2);

            // TMP_InputField (invisible, for editing interaction)
            var inputGO = new GameObject("CodeInput");
            inputGO.transform.SetParent(codeArea.transform, false);
            var inputRect = inputGO.AddComponent<RectTransform>();
            inputRect.anchorMin = Vector2.zero;
            inputRect.anchorMax = Vector2.one;
            inputRect.offsetMin = new Vector2(10, 2);
            inputRect.offsetMax = new Vector2(-10, -2);
            inputGO.AddComponent<Image>().color = new Color(0, 0, 0, 0.01f);

            var textArea = new GameObject("Text Area");
            textArea.transform.SetParent(inputGO.transform, false);
            var taRect = textArea.AddComponent<RectTransform>();
            taRect.anchorMin = Vector2.zero;
            taRect.anchorMax = Vector2.one;
            taRect.offsetMin = Vector2.zero;
            taRect.offsetMax = Vector2.zero;

            var phTxt = Txt(textArea.transform, "Placeholder", "// Code...", 18,
                new Color(0.5f, 0.5f, 0.5f, 0.5f), FontStyles.Italic);
            Stretch(phTxt, 0, 0, 0, 0);

            var inTxt = Txt(textArea.transform, "Text", "", 18, new Color(1, 1, 1, 0.02f));
            Stretch(inTxt, 0, 0, 0, 0);

            var tmpInput = inputGO.AddComponent<TMP_InputField>();
            tmpInput.textComponent = inTxt;
            tmpInput.textViewport = taRect;
            tmpInput.placeholder = phTxt;
            tmpInput.lineType = TMP_InputField.LineType.MultiLineNewline;
            tmpInput.richText = false;

            // ============ 4. GAP INPUT AREA ============
            var gapArea = Bg(panelRoot.transform, "GapInputArea", new Color(0.13f, 0.14f, 0.18f));
            gapArea.GetComponent<Image>().raycastTarget = false;
            var gaLE = gapArea.AddComponent<LayoutElement>();
            gaLE.flexibleHeight = 2;
            gaLE.minHeight = 100;

            var gapTitle = Txt(gapArea.transform, "GapTitle", "  Champs a remplir :", 16,
                new Color(0.7f, 0.8f, 0.9f), FontStyles.Bold);
            gapTitle.raycastTarget = false;
            var gtRect = gapTitle.GetComponent<RectTransform>();
            gtRect.anchorMin = new Vector2(0, 1);
            gtRect.anchorMax = Vector2.one;
            gtRect.pivot = new Vector2(0.5f, 1f);
            gtRect.sizeDelta = new Vector2(0, 25);

            // Scroll for gap inputs
            var gapScrollGO = new GameObject("GapScroll");
            gapScrollGO.transform.SetParent(gapArea.transform, false);
            var gsRect = gapScrollGO.AddComponent<RectTransform>();
            gsRect.anchorMin = Vector2.zero;
            gsRect.anchorMax = Vector2.one;
            gsRect.offsetMin = new Vector2(0, 0);
            gsRect.offsetMax = new Vector2(0, -28);
            var gapScroll = gapScrollGO.AddComponent<ScrollRect>();
            gapScroll.horizontal = false;
            gapScroll.vertical = true;

            var gapVP = Bg(gapScrollGO.transform, "Viewport", Color.clear);
            gapVP.GetComponent<Image>().raycastTarget = false;
            var vpRect = gapVP.GetComponent<RectTransform>();
            vpRect.anchorMin = Vector2.zero;
            vpRect.anchorMax = Vector2.one;
            vpRect.offsetMin = Vector2.zero;
            vpRect.offsetMax = Vector2.zero;
            gapVP.AddComponent<Mask>().showMaskGraphic = false;
            gapScroll.viewport = vpRect;

            var gapContent = new GameObject("GapContent");
            gapContent.transform.SetParent(gapVP.transform, false);
            var gcRect = gapContent.AddComponent<RectTransform>();
            gcRect.anchorMin = new Vector2(0, 1);
            gcRect.anchorMax = Vector2.one;
            gcRect.pivot = new Vector2(0.5f, 1f);
            gcRect.sizeDelta = Vector2.zero;
            gapContent.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var grid = gapContent.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(220, 75);
            grid.spacing = new Vector2(10, 8);
            grid.padding = new RectOffset(10, 10, 5, 5);
            grid.constraint = GridLayoutGroup.Constraint.Flexible;
            gapScroll.content = gcRect;

            // ============ 5. COURSE SECTION ============
            var courseSection = Bg(panelRoot.transform, "CourseSection", new Color(0.11f, 0.12f, 0.16f));
            courseSection.GetComponent<Image>().raycastTarget = false;
            var csLE = courseSection.AddComponent<LayoutElement>();
            csLE.flexibleHeight = 1.5f;
            csLE.minHeight = 100;
            var csVLG = courseSection.AddComponent<VerticalLayoutGroup>();
            csVLG.childForceExpandWidth = true;
            csVLG.childForceExpandHeight = false;
            csVLG.childControlHeight = true;
            csVLG.childControlWidth = true;
            csVLG.spacing = 5;
            csVLG.padding = new RectOffset(10, 10, 8, 8);

            var csHeader = new GameObject("CourseHeader");
            csHeader.transform.SetParent(courseSection.transform, false);
            csHeader.AddComponent<RectTransform>();
            var chHLG = csHeader.AddComponent<HorizontalLayoutGroup>();
            chHLG.childForceExpandWidth = false;
            chHLG.childControlWidth = true;
            chHLG.childControlHeight = true;
            chHLG.spacing = 10;
            csHeader.AddComponent<LayoutElement>().minHeight = 30;

            var courseTitleTxt = Txt(csHeader.transform, "CourseTitle", "# Cours C#", 22, Color.white, FontStyles.Bold);
            courseTitleTxt.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;

            var lessonList = Bg(courseSection.transform, "LessonList", Color.clear);
            lessonList.GetComponent<Image>().raycastTarget = false;
            lessonList.AddComponent<LayoutElement>().flexibleHeight = 1;
            var llVLG = lessonList.AddComponent<VerticalLayoutGroup>();
            llVLG.childForceExpandWidth = true;
            llVLG.childForceExpandHeight = false;
            llVLG.childControlHeight = true;
            llVLG.childControlWidth = true;
            llVLG.spacing = 4;

            // ============ 6. CONSOLE ============
            var consoleSection = Bg(panelRoot.transform, "ConsoleSection", new Color(0.08f, 0.09f, 0.11f));
            consoleSection.GetComponent<Image>().raycastTarget = false;
            var conLE = consoleSection.AddComponent<LayoutElement>();
            conLE.flexibleHeight = 1;
            conLE.minHeight = 80;
            var conVLG = consoleSection.AddComponent<VerticalLayoutGroup>();
            conVLG.childForceExpandWidth = true;
            conVLG.childControlHeight = true;
            conVLG.childControlWidth = true;
            conVLG.spacing = 2;
            conVLG.padding = new RectOffset(8, 8, 4, 4);

            var conTitle = Txt(consoleSection.transform, "ConsoleTitle", "Console", 18,
                new Color(0.7f, 0.7f, 0.7f), FontStyles.Bold);
            conTitle.raycastTarget = false;
            conTitle.gameObject.AddComponent<LayoutElement>().minHeight = 28;

            Bg(consoleSection.transform, "Separator", new Color(0.25f, 0.25f, 0.3f))
                .AddComponent<LayoutElement>().minHeight = 1;

            // Console text (empty — filled at runtime)
            var consoleText = Txt(consoleSection.transform, "ConsoleText", "", 16, new Color(0.83f, 0.83f, 0.83f));
            consoleText.richText = true;
            consoleText.alignment = TextAlignmentOptions.TopLeft;
            consoleText.raycastTarget = false;
            consoleText.gameObject.AddComponent<LayoutElement>().flexibleHeight = 1;

            // ============ WIRE COMPONENTS ============
            var editorCtrl = editorArea.AddComponent<CodeEditorController>();
            SetField(editorCtrl, "codeInput", tmpInput);
            SetField(editorCtrl, "highlightedOverlay", hlText);
            SetField(editorCtrl, "runButton", runBtn);
            SetField(editorCtrl, "saveButton", saveBtn);

            var lineRend = lineCol.AddComponent<LineNumberRenderer>();
            SetField(lineRend, "lineNumberText", lineNumText);
            SetField(editorCtrl, "lineNumbers", lineRend);

            var tabMgr = tabBar.AddComponent<TabManager>();
            SetField(tabMgr, "tabContainer", tabBar.transform);
            SetField(tabMgr, "editorController", editorCtrl);

            var gapMgr = gapArea.AddComponent<GapInputManager>();
            SetField(gapMgr, "gapContainer", gapContent.transform);

            var codeCon = consoleSection.AddComponent<CodeConsole>();
            SetField(codeCon, "consoleText", consoleText);

            var puzzleMgr = panelRoot.AddComponent<PuzzleManager>();
            SetField(puzzleMgr, "codeEditor", editorCtrl);
            SetField(puzzleMgr, "tabManager", tabMgr);
            SetField(puzzleMgr, "console", codeCon);
            SetField(puzzleMgr, "gapInputManager", gapMgr);

            var courseUI = courseSection.AddComponent<CourseProgressUI>();
            SetField(courseUI, "lessonListContainer", lessonList.transform);
            SetField(courseUI, "courseTitleText", courseTitleTxt);

            var codexMgr = panelRoot.AddComponent<CodexPanelManager>();
            SetField(codexMgr, "panelRoot", panelRect);
            SetField(codexMgr, "canvasGroup", cg);
            SetField(codexMgr, "codeEditorPanel", editorArea);
            SetField(codexMgr, "coursePanel", courseSection);
            SetField(codexMgr, "consolePanel", consoleSection);
            SetField(codexMgr, "codeEditor", editorCtrl);
            SetField(codexMgr, "tabManager", tabMgr);
            SetField(codexMgr, "console", codeCon);
            SetField(codexMgr, "puzzleManager", puzzleMgr);
            SetField(codexMgr, "courseProgressUI", courseUI);
            SetField(codexMgr, "headerTitle", headerTitle);

            // ============ MANAGERS ============
            var managersGO = new GameObject("--- Codex Managers ---");
            Undo.RegisterCreatedObjectUndo(managersGO, "Create Managers");

            var progressGO = new GameObject("PlayerProgress");
            Undo.RegisterCreatedObjectUndo(progressGO, "Create PlayerProgress");
            progressGO.AddComponent<PlayerProgress>();

            var cmGO = new GameObject("CourseManager");
            cmGO.transform.SetParent(managersGO.transform);
            var courseMgr = cmGO.AddComponent<CourseManager>();
            SetField(courseMgr, "puzzleManager", puzzleMgr);
            SetField(courseMgr, "progressUI", courseUI);
            SetField(codexMgr, "courseManager", courseMgr);

            var gmGO = new GameObject("GameManager");
            gmGO.transform.SetParent(managersGO.transform);
            var gameMgr = gmGO.AddComponent<GameManager>();
            SetField(gameMgr, "codexPanel", codexMgr);
            SetField(gameMgr, "courseManager", courseMgr);
            SetField(gameMgr, "puzzleManager", puzzleMgr);

            EditorUtility.SetDirty(canvasGO);
            Selection.activeGameObject = canvasGO;
            Debug.Log("<color=green>[Codex] Setup OK</color>");
        }

        // ---- Helpers ----

        static void DestroyIfExists(string name)
        {
            var go = GameObject.Find(name);
            if (go != null) Undo.DestroyObjectImmediate(go);
        }

        static GameObject Bg(Transform parent, string name, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var r = go.AddComponent<RectTransform>();
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.offsetMin = Vector2.zero;
            r.offsetMax = Vector2.zero;
            go.AddComponent<Image>().color = color;
            return go;
        }

        static TMP_Text Txt(Transform parent, string name, string text, int size,
            Color color, FontStyles style = FontStyles.Normal)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var r = go.AddComponent<RectTransform>();
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.offsetMin = Vector2.zero;
            r.offsetMax = Vector2.zero;
            var t = go.AddComponent<TextMeshProUGUI>();
            t.font = _font;
            t.text = text;
            t.fontSize = size;
            t.fontStyle = style;
            t.color = color;
            t.enableWordWrapping = true;
            t.overflowMode = TextOverflowModes.Overflow;
            t.raycastTarget = false; // non-interactive by default
            return t;
        }

        static Button Btn(Transform parent, string name, string label, Color bg, float w)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            var img = go.AddComponent<Image>();
            img.color = bg;
            img.raycastTarget = true; // buttons MUST receive clicks
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            go.AddComponent<LayoutElement>().preferredWidth = w;
            var t = Txt(go.transform, "Label", label, 16, Color.white, FontStyles.Bold);
            t.alignment = TextAlignmentOptions.Center;
            Stretch(t, 0, 0, 0, 0);
            return btn;
        }

        static void Stretch(TMP_Text t, float l, float b, float r, float top)
        {
            var rt = t.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(l, b);
            rt.offsetMax = new Vector2(r, top);
        }

        static void SetField(object target, string field, object value)
        {
            var f = target.GetType().GetField(field,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (f != null) f.SetValue(target, value);
            else Debug.LogWarning($"[Codex] Field '{field}' not found on {target.GetType().Name}");
        }
    }
}
#endif
