using System;
using System.Collections.Generic;
using Codex.CodeEditor;
using Codex.Core;
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
        bool _debugUnlockAll = true;

        [Header("World Integration")]
        [SerializeField] bool _startHidden = false;
        GameObject _canvasRoot;

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
        GameObject _lessonSection;
        TMP_Text _lessonDisplay;
        readonly HashSet<string> _lessonSeen = new HashSet<string>();
        readonly Dictionary<string, int> _attempts = new Dictionary<string, int>();
        readonly Dictionary<string, int> _stars = new Dictionary<string, int>();
        GameObject _freeWriteSection;
        TMP_InputField _freeWriteInput;
        TMP_Text _freeWriteInstructions;
        string _freeWriteCode = "";
        readonly HashSet<string> _trophies = new HashSet<string>();
        readonly HashSet<string> _hintsUsedFor = new HashSet<string>();
        int _streak;

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

            // LESSON SECTION (mini-cours avant le puzzle)
            _lessonSection = new GameObject("LessonSection");
            _lessonSection.transform.SetParent(panel, false);
            _lessonSection.AddComponent<RectTransform>();
            L(_lessonSection, flexH: 7);
            VLG(_lessonSection, 6, new RectOffset(4, 4, 4, 4));
            _lessonSection.SetActive(false);

            var lessonScroll = Scroll(_lessonSection.transform, "LessonScroll",
                new Color(0.08f, 0.09f, 0.11f), 100, 6);
            var lessonCnt = lessonScroll.content;
            VLG(lessonCnt.gameObject, 0, new RectOffset(16, 16, 12, 12));
            lessonCnt.gameObject.AddComponent<ContentSizeFitter>().verticalFit =
                ContentSizeFitter.FitMode.PreferredSize;
            _lessonDisplay = T(lessonCnt, "LessonText", "", 15, C_WHITE);
            _lessonDisplay.richText = true;
            _lessonDisplay.alignment = TextAlignmentOptions.TopLeft;
            _lessonDisplay.enableWordWrapping = true;

            var startBtnRT = R(_lessonSection.transform, "StartPuzzle");
            L(startBtnRT.gameObject, minH: 42);
            var startImg = Bg(startBtnRT, C_GREEN);
            startImg.raycastTarget = true;
            var startBtnC = startBtnRT.gameObject.AddComponent<Button>();
            startBtnC.targetGraphic = startImg;
            startBtnC.onClick.AddListener(OnStartPuzzle);
            var startLbl = T(startBtnRT, "L", ">> COMMENCER LE PUZZLE <<", 17, C_WHITE, FontStyles.Bold);
            startLbl.alignment = TextAlignmentOptions.Center;
            Stretch(startLbl.rectTransform);

            // FREE WRITE SECTION
            _freeWriteSection = new GameObject("FreeWriteSection");
            _freeWriteSection.transform.SetParent(panel, false);
            _freeWriteSection.AddComponent<RectTransform>();
            L(_freeWriteSection, flexH: 7);
            VLG(_freeWriteSection, 6, new RectOffset(4, 4, 4, 4));
            _freeWriteSection.SetActive(false);

            var fwInstrScroll = Scroll(_freeWriteSection.transform, "FWInstrScroll",
                new Color(0.08f, 0.09f, 0.11f), 60, 2);
            var fwInstrCnt = fwInstrScroll.content;
            VLG(fwInstrCnt.gameObject, 0, new RectOffset(12, 12, 8, 8));
            fwInstrCnt.gameObject.AddComponent<ContentSizeFitter>().verticalFit =
                ContentSizeFitter.FitMode.PreferredSize;
            _freeWriteInstructions = T(fwInstrCnt, "FWInstr", "", 14, C_GOLD);
            _freeWriteInstructions.richText = true;
            _freeWriteInstructions.enableWordWrapping = true;

            var fwInputScroll = Scroll(_freeWriteSection.transform, "FWInputScroll",
                C_CODE, 100, 5);
            var fwInputArea = fwInputScroll.content;

            var fwInputGO = new GameObject("FWInput");
            fwInputGO.transform.SetParent(fwInputArea, false);
            var fwInputRT = fwInputGO.AddComponent<RectTransform>();
            fwInputRT.anchorMin = Vector2.zero;
            fwInputRT.anchorMax = Vector2.one;
            fwInputRT.offsetMin = Vector2.zero;
            fwInputRT.offsetMax = Vector2.zero;

            var fwArea = new GameObject("Text Area");
            fwArea.transform.SetParent(fwInputGO.transform, false);
            var fwAreaR = fwArea.AddComponent<RectTransform>();
            fwAreaR.anchorMin = Vector2.zero;
            fwAreaR.anchorMax = Vector2.one;
            fwAreaR.offsetMin = new Vector2(10, 4);
            fwAreaR.offsetMax = new Vector2(-10, -4);
            fwArea.AddComponent<RectMask2D>();

            var fwPh = new GameObject("Placeholder");
            fwPh.transform.SetParent(fwArea.transform, false);
            Stretch(fwPh.AddComponent<RectTransform>());
            var fwPhT = fwPh.AddComponent<TextMeshProUGUI>();
            fwPhT.text = "Ecris ton code ici...";
            fwPhT.fontSize = 14;
            fwPhT.fontStyle = FontStyles.Italic;
            fwPhT.color = C_GRAY;
            fwPhT.raycastTarget = false;
            fwPhT.alignment = TextAlignmentOptions.TopLeft;

            var fwTxt = new GameObject("Text");
            fwTxt.transform.SetParent(fwArea.transform, false);
            Stretch(fwTxt.AddComponent<RectTransform>());
            var fwTxtT = fwTxt.AddComponent<TextMeshProUGUI>();
            fwTxtT.fontSize = 14;
            fwTxtT.color = C_WHITE;
            fwTxtT.raycastTarget = false;
            fwTxtT.alignment = TextAlignmentOptions.TopLeft;

            _freeWriteInput = fwInputGO.AddComponent<TMP_InputField>();
            _freeWriteInput.textComponent = fwTxtT;
            _freeWriteInput.textViewport = fwAreaR;
            _freeWriteInput.placeholder = fwPhT;
            _freeWriteInput.pointSize = 14;
            _freeWriteInput.lineType = TMP_InputField.LineType.MultiLineNewline;
            _freeWriteInput.characterLimit = 2000;
            _freeWriteInput.onValueChanged.AddListener(delegate(string v) { _freeWriteCode = v; });

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

            _canvasRoot = canvasGO;

            if (_startHidden)
            {
                _canvasRoot.SetActive(false);
                return;
            }

            _activeLessonIdx = 0;
            _puzzle = _lessons[0].create();
            ShowCode();
            LoadPuzzle();
            CompanionSay("Bienvenue, apprenti ! Je suis C-Sharp, ton drone guide. Ensemble, on va sauver Arcadia !");
        }

        void DefineLessons()
        {
            _lessons.Add(new LessonEntry {
                id = "temple_01", name = "Niv.1 -- Variables",
                desc = "Declare les bonnes variables pour activer le drone.",
                concept = "Variables", isFirstOfConcept = true,
                lessonContent = "<color=#FFD700><size=20>== LES VARIABLES ==</size></color>\n\nEn C#, une <color=#569CD6>variable</color> est une boite qui stocke une valeur.\n\n<color=#4EC9B0>Les types principaux :</color>\n  <color=#569CD6>int</color>      nombre entier (ex: 42, -7, 100)\n  <color=#569CD6>string</color>   texte (ex: \"Bonjour\", \"C-Sharp\")\n  <color=#569CD6>bool</color>     vrai ou faux (true / false)\n  <color=#569CD6>float</color>    nombre decimal (ex: 3.14f)\n\n<color=#4EC9B0>Declaration :</color>\n  <color=#569CD6>type</color> nomVariable = valeur;\n\n<color=#4EC9B0>Exemples :</color>\n  <color=#569CD6>int</color> age = 25;\n  <color=#569CD6>string</color> nom = <color=#CE9178>\"Aria\"</color>;\n  <color=#569CD6>bool</color> estActif = <color=#569CD6>true</color>;\n\n<color=#608B4E>Les guillemets sont obligatoires pour les strings !</color>",
                create = ExamplePuzzles.CreateVariablesPuzzle
            });
            _lessons.Add(new LessonEntry {
                id = "temple_02", name = "Niv.1b -- Variables (Pratique)",
                desc = "Remplis le registre avec int, float, string, bool.",
                concept = "Variables",
                create = ExamplePuzzles.CreateVariablesPracticePuzzle,
                requiresId = "temple_01"
            });
            _lessons.Add(new LessonEntry {
                id = "temple_03", name = "Niv.1c -- Variables (Maitrise)",
                desc = "Ecris toutes les variables du code d'activation.",
                concept = "Variables",
                create = ExamplePuzzles.CreateVariablesMasteryPuzzle,
                requiresId = "temple_02"
            });
            _lessons.Add(new LessonEntry {
                id = "temple_creer", name = "Niv.1d -- Variables (Creation)",
                desc = "Ecris tes propres declarations de variables !",
                concept = "Variables (CREER)",
                create = ExamplePuzzles.CreateVariablesCreerPuzzle,
                requiresId = "temple_03"
            });
            _lessons.Add(new LessonEntry {
                id = "pont_01", name = "Niv.2 -- Conditions",
                desc = "Programme la condition du Golem pour passer.",
                concept = "if / else", isFirstOfConcept = true,
                lessonContent = "<color=#FFD700><size=20>== LES CONDITIONS (if / else) ==</size></color>\n\nLe <color=#569CD6>if</color> execute du code seulement si une condition est vraie.\n\n<color=#4EC9B0>Structure :</color>\n  <color=#569CD6>if</color> (condition)\n  {\n      <color=#608B4E>// code si vrai</color>\n  }\n  <color=#569CD6>else</color>\n  {\n      <color=#608B4E>// code si faux</color>\n  }\n\n<color=#4EC9B0>Operateurs de comparaison :</color>\n  ==  egal a          !=  different de\n  >   superieur       <   inferieur\n  >=  sup. ou egal    <=  inf. ou egal\n\n<color=#4EC9B0>Operateurs logiques :</color>\n  &&  ET (les deux vraies)\n  ||  OU (au moins une vraie)\n  !   NON (inverse)\n\n<color=#4EC9B0>Exemple :</color>\n  <color=#569CD6>if</color> (level >= 5 && hasKey)\n      OpenDoor();\n  <color=#569CD6>else</color>\n      ShowMessage(<color=#CE9178>\"Acces refuse\"</color>);",
                create = ExamplePuzzles.CreateConditionsPuzzle,
                requiresId = "temple_creer"
            });
            _lessons.Add(new LessonEntry {
                id = "pont_02", name = "Niv.2b -- Conditions (Pratique)",
                desc = "Programme le systeme d'alarme avec if/else if/else.",
                concept = "if / else if",
                create = ExamplePuzzles.CreateConditionsPracticePuzzle,
                requiresId = "pont_01"
            });
            _lessons.Add(new LessonEntry {
                id = "pont_03", name = "Niv.2c -- Conditions (Maitrise)",
                desc = "Ecris les conditions du portail magique.",
                concept = "if / else",
                create = ExamplePuzzles.CreateConditionsMasteryPuzzle,
                requiresId = "pont_02"
            });
            _lessons.Add(new LessonEntry {
                id = "pont_creer", name = "Niv.2d -- Conditions (Creation)",
                desc = "Ecris tes propres conditions if/else !",
                concept = "Conditions (CREER)",
                create = ExamplePuzzles.CreateConditionsCreerPuzzle,
                requiresId = "pont_03"
            });
            _lessons.Add(new LessonEntry {
                id = "tour_01", name = "Niv.3a -- Switch (Decouverte)",
                desc = "Decouvre le switch avec le panneau de controle.",
                concept = "switch / case", isFirstOfConcept = true,
                lessonContent = "<color=#FFD700><size=20>== LE SWITCH / CASE ==</size></color>\n\nLe <color=#569CD6>switch</color> teste une variable contre plusieurs valeurs.\nC'est plus lisible qu'une longue chaine de if/else.\n\n<color=#4EC9B0>Structure :</color>\n  <color=#569CD6>switch</color> (variable)\n  {\n      <color=#569CD6>case</color> valeur1:\n          <color=#608B4E>// code pour valeur1</color>\n          <color=#569CD6>break</color>;\n      <color=#569CD6>case</color> valeur2:\n          <color=#608B4E>// code pour valeur2</color>\n          <color=#569CD6>break</color>;\n      <color=#569CD6>default</color>:\n          <color=#608B4E>// si aucun case ne correspond</color>\n          <color=#569CD6>break</color>;\n  }\n\n<color=#4EC9B0>Points cles :</color>\n  - Chaque <color=#569CD6>case</color> se termine par <color=#569CD6>break</color>\n  - <color=#569CD6>default</color> = le cas \"sinon\" (optionnel mais recommande)\n  - Fonctionne avec int, string, char, enum\n\n<color=#4EC9B0>Exemple :</color>\n  <color=#569CD6>switch</color> (direction)\n  {\n      <color=#569CD6>case</color> <color=#CE9178>\"nord\"</color>: Avancer(); <color=#569CD6>break</color>;\n      <color=#569CD6>case</color> <color=#CE9178>\"sud\"</color>:  Reculer(); <color=#569CD6>break</color>;\n      <color=#569CD6>default</color>: Attendre(); <color=#569CD6>break</color>;\n  }",
                create = ExamplePuzzles.CreateSwitchDiscoveryPuzzle,
                requiresId = "pont_creer"
            });
            _lessons.Add(new LessonEntry {
                id = "tour_02", name = "Niv.3b -- Switch (Pratique)",
                desc = "Traduis les runes avec un switch sur des strings.",
                concept = "switch / case",
                create = ExamplePuzzles.CreateSwitchPracticePuzzle,
                requiresId = "tour_01"
            });
            _lessons.Add(new LessonEntry {
                id = "tour_03", name = "Niv.3c -- Switch (Maitrise)",
                desc = "Ecris un switch complet pour commander le drone.",
                concept = "switch / case",
                create = ExamplePuzzles.CreateSwitchMasteryPuzzle,
                requiresId = "tour_02"
            });
            _lessons.Add(new LessonEntry {
                id = "tour_creer", name = "Niv.3d -- Switch (Creation)",
                desc = "Ecris un switch complet depuis zero !",
                concept = "Switch (CREER)",
                create = ExamplePuzzles.CreateSwitchCreerPuzzle,
                requiresId = "tour_03"
            });
            _lessons.Add(new LessonEntry {
                id = "vallee_01", name = "Niv.4a -- Boucle For",
                desc = "Construis un pont de 10 blocs avec une boucle for.",
                concept = "for", isFirstOfConcept = true,
                lessonContent = "<color=#FFD700><size=20>== LA BOUCLE FOR ==</size></color>\n\nQuand on connait le nombre de repetitions.\n\n<color=#4EC9B0>Structure :</color>\n  <color=#569CD6>for</color> (<color=#569CD6>int</color> i = debut; condition; increment)\n  {\n      <color=#608B4E>// code repete</color>\n  }\n\n<color=#4EC9B0>Les 3 parties :</color>\n  1. <color=#569CD6>int</color> i = 0   ->  initialisation du compteur\n  2. i < 10        ->  condition de continuation\n  3. i++           ->  incrementation (+1 a chaque tour)\n\n<color=#4EC9B0>Exemple (afficher 0 a 9) :</color>\n  <color=#569CD6>for</color> (<color=#569CD6>int</color> i = 0; i < 10; i++)\n  {\n      Debug.Log(i);  <color=#608B4E>// 0, 1, 2, ... 9</color>\n  }\n\n<color=#4EC9B0>Astuce :</color>\n  i < 10  avec i=0  ->  10 iterations (0 a 9)\n  i <= 10 avec i=0  ->  11 iterations (0 a 10)",
                create = ExamplePuzzles.CreateLoopPuzzle,
                requiresId = "tour_creer"
            });
            _lessons.Add(new LessonEntry {
                id = "vallee_02", name = "Niv.4b -- Boucle While",
                desc = "Le garde patrouille tant qu'il a de l'energie.",
                concept = "while", isFirstOfConcept = true,
                lessonContent = "<color=#FFD700><size=20>== LA BOUCLE WHILE ==</size></color>\n\nQuand on ne sait pas combien de fois repeter.\nLa condition est verifiee AVANT chaque tour.\n\n<color=#4EC9B0>Structure :</color>\n  <color=#569CD6>while</color> (condition)\n  {\n      <color=#608B4E>// code tant que condition est vraie</color>\n  }\n\n<color=#4EC9B0>Exemple :</color>\n  <color=#569CD6>int</color> vie = 100;\n  <color=#569CD6>while</color> (vie > 0)\n  {\n      vie -= 10;\n      Debug.Log(<color=#CE9178>\"Vie : \"</color> + vie);\n  }\n\n<color=#4EC9B0>Difference avec for :</color>\n  <color=#569CD6>for</color>   ->  nombre de tours connu (compteur)\n  <color=#569CD6>while</color> ->  on boucle selon une condition\n\n<color=#608B4E>Attention : si la condition reste toujours vraie,\nla boucle ne s'arrete jamais (boucle infinie) !</color>",
                create = ExamplePuzzles.CreateWhileLoopPuzzle,
                requiresId = "vallee_01"
            });
            _lessons.Add(new LessonEntry {
                id = "vallee_03", name = "Niv.4c -- While (Pratique)",
                desc = "Cherche le tresor en creusant avec un while.",
                concept = "while",
                create = ExamplePuzzles.CreateWhilePracticePuzzle,
                requiresId = "vallee_02"
            });
            _lessons.Add(new LessonEntry {
                id = "vallee_04", name = "Niv.4d -- Do...While",
                desc = "Le scanner doit analyser au moins une fois.",
                concept = "do...while", isFirstOfConcept = true,
                lessonContent = "<color=#FFD700><size=20>== LA BOUCLE DO...WHILE ==</size></color>\n\nComme while, mais le code s'execute AU MOINS une fois.\nLa condition est verifiee APRES chaque tour.\n\n<color=#4EC9B0>Structure :</color>\n  <color=#569CD6>do</color>\n  {\n      <color=#608B4E>// code execute au moins 1 fois</color>\n  }\n  <color=#569CD6>while</color> (condition);  <color=#608B4E>// point-virgule !</color>\n\n<color=#4EC9B0>Exemple :</color>\n  <color=#569CD6>int</color> essais = 0;\n  <color=#569CD6>do</color>\n  {\n      essais++;\n      Debug.Log(<color=#CE9178>\"Tentative \"</color> + essais);\n  }\n  <color=#569CD6>while</color> (essais < 3);\n\n<color=#4EC9B0>Quand utiliser do...while ?</color>\n  Quand il faut executer le code au moins\n  une fois avant de verifier la condition.\n  Ex: demander un mot de passe, scanner...\n\n<color=#4EC9B0>Resume des 3 boucles :</color>\n  <color=#569CD6>for</color>        ->  tours connus (compteur)\n  <color=#569CD6>while</color>      ->  condition d'abord\n  <color=#569CD6>do..while</color>  ->  au moins 1 execution",
                create = ExamplePuzzles.CreateDoWhileLoopPuzzle,
                requiresId = "vallee_03"
            });
            _lessons.Add(new LessonEntry {
                id = "vallee_creer", name = "Niv.4e -- Boucles (Creation)",
                desc = "Ecris for, while et do...while depuis zero !",
                concept = "Boucles (CREER)",
                create = ExamplePuzzles.CreateLoopsCreerPuzzle,
                requiresId = "vallee_04"
            });
            _lessons.Add(new LessonEntry {
                id = "forge_01", name = "Niv.5a -- Fonctions (Decouverte)",
                desc = "Cree et appelle ta premiere fonction void.",
                concept = "fonctions", isFirstOfConcept = true,
                lessonContent = "<color=#FFD700><size=20>== LES FONCTIONS ==</size></color>\n\nUne fonction est un bloc de code reutilisable avec un nom.\n\n<color=#4EC9B0>Declaration :</color>\n  typeRetour NomFonction(type param)\n  {\n      <color=#608B4E>// code</color>\n      <color=#569CD6>return</color> resultat;\n  }\n\n<color=#4EC9B0>Types de retour :</color>\n  <color=#569CD6>void</color>    ne retourne rien\n  <color=#569CD6>int</color>     retourne un nombre\n  <color=#569CD6>string</color>  retourne un texte\n  <color=#569CD6>bool</color>    retourne vrai/faux\n\n<color=#4EC9B0>Exemple sans retour :</color>\n  <color=#569CD6>void</color> Saluer()\n  {\n      Debug.Log(<color=#CE9178>\"Bonjour !\"</color>);\n  }\n  Saluer();  <color=#608B4E>// appel</color>\n\n<color=#4EC9B0>Exemple avec retour :</color>\n  <color=#569CD6>int</color> Doubler(<color=#569CD6>int</color> n)\n  {\n      <color=#569CD6>return</color> n * 2;\n  }\n  <color=#569CD6>int</color> r = Doubler(5);  <color=#608B4E>// r = 10</color>\n\n<color=#4EC9B0>Points cles :</color>\n  - Les parentheses () sont obligatoires\n  - <color=#569CD6>return</color> renvoie la valeur ET quitte la fonction",
                create = ExamplePuzzles.CreateFunctionDiscoveryPuzzle,
                requiresId = "vallee_creer"
            });
            _lessons.Add(new LessonEntry {
                id = "forge_02", name = "Niv.5b -- Fonctions (Parametres)",
                desc = "Passe des arguments a une fonction.",
                concept = "fonctions",
                create = ExamplePuzzles.CreateFunctionParamsPuzzle,
                requiresId = "forge_01"
            });
            _lessons.Add(new LessonEntry {
                id = "forge_03", name = "Niv.5c -- Fonctions (Retour)",
                desc = "Utilise return pour renvoyer une valeur.",
                concept = "fonctions",
                create = ExamplePuzzles.CreateFunctionReturnPuzzle,
                requiresId = "forge_02"
            });
            _lessons.Add(new LessonEntry {
                id = "forge_04", name = "Niv.5d -- Fonctions (Maitrise)",
                desc = "Ecris une fonction complete avec parametres et retour.",
                concept = "fonctions",
                create = ExamplePuzzles.CreateFunctionMasteryPuzzle,
                requiresId = "forge_03"
            });
            _lessons.Add(new LessonEntry {
                id = "forge_creer", name = "Niv.5e -- Fonctions (Creation)",
                desc = "Ecris une fonction complete depuis zero !",
                concept = "Fonctions (CREER)",
                create = ExamplePuzzles.CreateFunctionsCreerPuzzle,
                requiresId = "forge_04"
            });

            // === DEFIS BONUS ===
            _lessons.Add(new LessonEntry {
                id = "defi_bug_01", name = "Defi -- Trouve le Bug (Variables)",
                desc = "Repere les erreurs de type dans le code !",
                concept = "Bug Detective",
                create = ExamplePuzzles.CreateFindBugVariablesPuzzle,
                requiresId = "temple_creer"
            });
            _lessons.Add(new LessonEntry {
                id = "defi_bug_02", name = "Defi -- Trouve le Bug (Conditions)",
                desc = "La logique des conditions est cassee !",
                concept = "Bug Detective",
                create = ExamplePuzzles.CreateFindBugConditionsPuzzle,
                requiresId = "pont_creer"
            });
            _lessons.Add(new LessonEntry {
                id = "defi_predict_01", name = "Defi -- Predis la Sortie (Boucles)",
                desc = "Que va afficher ce code ? Tape la reponse !",
                concept = "Prediction",
                create = ExamplePuzzles.CreatePredictOutputLoopsPuzzle,
                requiresId = "vallee_creer"
            });
            _lessons.Add(new LessonEntry {
                id = "defi_predict_02", name = "Defi -- Predis la Sortie (Conditions)",
                desc = "Lis le code et devine le resultat !",
                concept = "Prediction",
                create = ExamplePuzzles.CreatePredictOutputConditionsPuzzle,
                requiresId = "pont_creer"
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
            if (_puzzle.isFreeWrite)
            {
                ShowFreeWrite();
                CompanionSay(_puzzle.puzzleName + " -- Mode creation ! Ecris ton code depuis zero !");
                return;
            }
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
            CompanionSay(_puzzle.puzzleName + " -- " + _puzzle.description);
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
            _lessonSection.SetActive(false);
            _freeWriteSection.SetActive(false);
        }

        void ShowCourse()
        {
            _codeSection.SetActive(false);
            _courseSection.SetActive(true);
            _lessonSection.SetActive(false);
            _freeWriteSection.SetActive(false);
            _titleText.text = "COURS C# -- Progression";
            BuildCourseList();
        }

        void ShowLesson(int idx)
        {
            var lesson = _lessons[idx];
            _codeSection.SetActive(false);
            _courseSection.SetActive(false);
            _lessonSection.SetActive(true);
            _freeWriteSection.SetActive(false);
            _titleText.text = "COURS : " + lesson.concept;
            _lessonDisplay.text = lesson.lessonContent;
            _lessonSeen.Add(lesson.id);
            CompanionSay("Lis bien cette lecon avant de te lancer dans le puzzle !");
        }

void ShowFreeWrite()
        {
            _codeSection.SetActive(false);
            _courseSection.SetActive(false);
            _lessonSection.SetActive(false);
            _freeWriteSection.SetActive(true);
            _titleText.text = _puzzle.puzzleName + " -- MODE CREATION";
            _freeWriteInstructions.text = "<color=#FFD700><size=18>MISSION : ECRIS TON CODE</size></color>\n\n"
                + _puzzle.instructions;
            _freeWriteInput.text = "";
            _freeWriteCode = "";
        }

        void OnStartPuzzle()
        {
            ShowCode();
            LoadPuzzle();
        }

        void BuildCourseList()
        {
            ClearChildren(_courseContent);

            int totalStars = 0;
            int maxStars = _lessons.Count * 3;
            foreach (var kv in _stars) totalStars += kv.Value;

            var summary = T(_courseContent, "Summary",
                "<color=#FFD700>Etoiles : " + totalStars + " / " + maxStars + "</color>  |  "
                + "Completes : " + _completed.Count + " / " + _lessons.Count,
                14, C_GOLD, FontStyles.Bold);
            summary.richText = true;
            summary.alignment = TextAlignmentOptions.Center;
            L(summary.gameObject, minH: 24);

            if (_trophies.Count > 0)
            {
                string tList = "<color=#FFD700>Trophees (" + _trophies.Count + "/10) :</color> ";
                foreach (var t in _trophies)
                {
                    string tName = t;
                    if (t == "first_compile") tName = "Premier Compile";
                    else if (t == "perfectionist") tName = "Perfectionniste";
                    else if (t == "no_hints") tName = "Sans Indice";
                    else if (t == "streak3") tName = "Enchainement x3";
                    else if (t == "creator") tName = "Createur";
                    else if (t == "detective") tName = "Detective";
                    else if (t == "oracle") tName = "Devin";
                    else if (t == "halfway") tName = "Demi-Chemin";
                    else if (t == "architect") tName = "Architecte";
                    else if (t == "master") tName = "Maitre Absolu";
                    tList += "[" + tName + "] ";
                }
                var trophyT = T(_courseContent, "Trophies", tList, 12, C_GOLD);
                trophyT.richText = true;
                trophyT.alignment = TextAlignmentOptions.Center;
                trophyT.enableWordWrapping = true;
                L(trophyT.gameObject, minH: 22);
            }

            for (int i = 0; i < _lessons.Count; i++)
            {
                var lesson = _lessons[i];
                bool done = _completed.Contains(lesson.id);
                bool locked = !_debugUnlockAll && lesson.requiresId != null
                    && !_completed.Contains(lesson.requiresId);
                int idx = i;

                var row = R(_courseContent, "Lesson" + i);
                Bg(row, locked ? C_LOCKED : (done ? C_SEL : C_GAP_ROW));
                L(row.gameObject, minH: 70);
                VLG(row.gameObject, 2, new RectOffset(12, 12, 6, 6));

                string starStr = "";
                if (done && _stars.ContainsKey(lesson.id))
                {
                    int s = _stars[lesson.id];
                    starStr = " <color=#FFD700>" + new string('*', s) + new string('-', 3 - s) + "</color>";
                }

                string status = done ? "<color=#4EC9B0>[COMPLETE]</color>" + starStr
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
            var lesson = _lessons[idx];
            if (lesson.isFirstOfConcept && lesson.lessonContent != null
                && !_lessonSeen.Contains(lesson.id))
            {
                ShowLesson(idx);
                return;
            }
            ShowCode();
            LoadPuzzle();
        }

        // === CALLBACKS ===

        void OnRun()
        {
            if (_puzzle == null || _courseSection.activeSelf || _lessonSection.activeSelf) return;

            if (_puzzle.isFreeWrite && _freeWriteSection.activeSelf)
            {
                RunFreeWriteValidation();
                return;
            }

            string pid = _puzzle.puzzleId;
            if (!_attempts.ContainsKey(pid)) _attempts[pid] = 0;
            _attempts[pid]++;

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
                _completed.Add(pid);
                if (ZoneManager.Instance != null) ZoneManager.Instance.RegisterPuzzleComplete(pid);

                int tries = _attempts[pid];
                int earnedStars = tries <= 1 ? 3 : (tries <= 3 ? 2 : 1);
                if (!_stars.ContainsKey(pid) || _stars[pid] < earnedStars)
                    _stars[pid] = earnedStars;
                string starDisplay = new string('*', earnedStars) + new string('-', 3 - earnedStars);
                Log("<color=#FFD700>[" + starDisplay + "] " + earnedStars + " etoile(s) !</color>");
                _streak++;
                CheckTrophies();

                int doneCount = _completed.Count;
                int total = _lessons.Count;
                float pct = (float)doneCount / total * 100f;

                if (doneCount == total)
                    CompanionSay("INCROYABLE ! Tu as termine tous les niveaux ! Tu es un vrai Architecte d'Arcadia !");
                else if (pct >= 75f)
                    CompanionSay("Tu y es presque ! Plus que quelques puzzles et Arcadia sera sauvee !");
                else if (pct >= 50f)
                    CompanionSay("La moitie du chemin est faite ! Tu progresses vite, apprenti !");
                else if (pct >= 25f)
                    CompanionSay("Bon debut ! Continue comme ca, tu maitrises de mieux en mieux le C# !");
                else if (_activeLessonIdx < _lessons.Count - 1)
                    CompanionSay("Bravo ! Niveau suivant debloque. Clique COURS pour continuer.");
            }
            else
            {
                Log("<color=#F44747>" + result.Message + "</color>");
                _streak = 0;
                int tries = _attempts[pid];
                if (tries == 1)
                    CompanionSay("Pas grave, relis le code et reessaie ! Tu peux utiliser INDICE.");
                else if (tries == 3)
                    CompanionSay("Courage ! Regarde bien les indices, la solution est proche.");
                else if (tries >= 5)
                    CompanionSay("N'abandonne pas ! Chaque erreur est une lecon. Relis le cours si besoin.");
            }
        }

        void RunFreeWriteValidation()
        {
            string pid = _puzzle.puzzleId;
            if (!_attempts.ContainsKey(pid)) _attempts[pid] = 0;
            _attempts[pid]++;

            var result = PuzzleValidator.ValidateFreeWrite(_puzzle, _freeWriteCode);
            Log("--- Validation (Mode Creation) ---");

            for (int i = 0; i < result.GapResults.Count; i++)
            {
                var gr = result.GapResults[i];
                string c = gr.IsCorrect ? "#4EC9B0" : "#F44747";
                string icon = gr.IsCorrect ? "[OK]" : "[X]";
                Log("<color=" + c + ">" + icon + " " + gr.Label + " : " + gr.Message + "</color>");
            }

            Log("Criteres : " + result.CorrectCount + "/" + result.TotalCount);

            if (result.IsSuccess)
            {
                Log("<color=#4EC9B0>" + result.Message + "</color>");
                _completed.Add(pid);
                if (ZoneManager.Instance != null) ZoneManager.Instance.RegisterPuzzleComplete(pid);

                int tries = _attempts[pid];
                int earnedStars = tries <= 1 ? 3 : (tries <= 3 ? 2 : 1);
                if (!_stars.ContainsKey(pid) || _stars[pid] < earnedStars)
                    _stars[pid] = earnedStars;
                string starDisplay = new string('*', earnedStars) + new string('-', 3 - earnedStars);
                Log("<color=#FFD700>[" + starDisplay + "] " + earnedStars + " etoile(s) !</color>");
                _streak++;
                CheckTrophies();
                CompanionSay("Magnifique ! Tu as ecrit du code depuis zero ! Tu deviens un vrai developpeur !");
            }
            else
            {
                Log("<color=#F44747>" + result.Message + "</color>");
                _streak = 0;
                int tries = _attempts[pid];
                if (tries == 1)
                    CompanionSay("Pas mal pour un premier essai ! Regarde les criteres manquants.");
                else if (tries >= 3)
                    CompanionSay("Utilise les indices ! Chaque critere te dit exactement ce qu'il faut.");
            }
        }

                void OnHint()
        {
            if (_puzzle == null || _courseSection.activeSelf || (_lessonSection != null && _lessonSection.activeSelf)) return;
            if (_puzzle.hints == null || _puzzle.hints.Count == 0)
            {
                Log("<color=#FFD700>Pas d indice disponible.</color>");
                return;
            }
            if (_hintIndex < _puzzle.hints.Count)
            {
                Log("<color=#FFD700>[?] " + _puzzle.hints[_hintIndex] + "</color>");
                _hintsUsedFor.Add(_puzzle.puzzleId);
                _hintIndex++;
            }
            else
                Log("<color=#FFD700>Tu as utilise tous les indices !</color>");
        }

        void OnCours()
        {
            if (_courseSection.activeSelf) ShowCode();
            else if (_lessonSection.activeSelf) ShowCode();
            else ShowCourse();
        }

        void Log(string msg)
        {
            _consoleLog += msg + "\n";
            if (_consoleText != null) _consoleText.text = _consoleLog;
        }

        void CheckTrophies()
        {
            int doneCount = _completed.Count;
            int total = _lessons.Count;
            float pct = (float)doneCount / total * 100f;
            string pid = _puzzle != null ? _puzzle.puzzleId : "";

            if (doneCount >= 1)
                UnlockTrophy("first_compile", "Premier Compile -- Tu as reussi ton premier puzzle !");

            if (_stars.ContainsKey(pid) && _stars[pid] == 3)
                UnlockTrophy("perfectionist", "Perfectionniste -- 3 etoiles du premier coup !");

            if (doneCount >= 1 && !_hintsUsedFor.Contains(pid))
                UnlockTrophy("no_hints", "Sans Indice -- Reussi sans aucun indice !");

            if (_streak >= 3)
                UnlockTrophy("streak3", "Enchainement x3 -- 3 succes d'affilee !");

            if (pid.Contains("creer"))
                UnlockTrophy("creator", "Createur -- Tu as ecrit du code depuis zero !");

            if (pid.StartsWith("defi_bug"))
                UnlockTrophy("detective", "Detective -- Tu as trouve les bugs !");

            if (pid.StartsWith("defi_predict"))
                UnlockTrophy("oracle", "Devin -- Tu as predit la sortie correctement !");

            if (pct >= 50f)
                UnlockTrophy("halfway", "Demi-Chemin -- 50%% du parcours complete !");

            if (doneCount == total)
                UnlockTrophy("architect", "Architecte d'Arcadia -- TOUT est complete !");

            int totalStars = 0;
            foreach (var kv in _stars) totalStars += kv.Value;
            if (totalStars == total * 3)
                UnlockTrophy("master", "Maitre Absolu -- 3 etoiles sur TOUS les puzzles !");
        }

        void UnlockTrophy(string id, string message)
        {
            if (_trophies.Contains(id)) return;
            _trophies.Add(id);
            Log("<color=#FFD700>=============================</color>");
            Log("<color=#FFD700>  TROPHEE DEBLOQUE !</color>");
            Log("<color=#FFD700>  " + message + "</color>");
            Log("<color=#FFD700>=============================</color>");
            CompanionSay("Felicitations ! Nouveau trophee : " + message.Split(new string[]{" -- "}, System.StringSplitOptions.None)[0] + " !");
        }

        // === WORLD INTEGRATION API ===

        public void Show(string puzzleId = null)
        {
            if (_canvasRoot != null)
                _canvasRoot.SetActive(true);

            if (puzzleId != null)
            {
                for (int i = 0; i < _lessons.Count; i++)
                {
                    if (_lessons[i].id == puzzleId)
                    {
                        SelectLesson(i);
                        return;
                    }
                }
            }

            if (_puzzle == null && _lessons.Count > 0)
            {
                _activeLessonIdx = 0;
                _puzzle = _lessons[0].create();
                ShowCode();
                LoadPuzzle();
                CompanionSay("Bienvenue, apprenti ! Je suis C-Sharp, ton drone guide. Ensemble, on va sauver Arcadia !");
            }
            else
            {
                ShowCourse();
            }
        }

        public void Hide()
        {
            if (_canvasRoot != null)
                _canvasRoot.SetActive(false);
        }

        public bool IsVisible()
        {
            return _canvasRoot != null && _canvasRoot.activeSelf;
        }

                void CompanionSay(string msg)
        {
            Log("<color=#4EC9B0>[C-Sharp]</color> " + msg);
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
            public string lessonContent;
            public bool isFirstOfConcept;
            public Func<PuzzleDefinition> create;
        }
    }
}
