using System.Collections.Generic;
using Codex.CodeEditor;
using Codex.Console;
using UnityEngine;
using UnityEngine.Events;

namespace Codex.Puzzle
{
    /// <summary>
    /// Orchestrates puzzle lifecycle: loading, user input collection,
    /// validation, and result feedback.
    /// Bridges the PuzzleDefinition data with the UI components.
    /// </summary>
    public class PuzzleManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] CodeEditorController codeEditor;
        [SerializeField] TabManager tabManager;
        [SerializeField] CodeConsole console;
        [SerializeField] GapInputManager gapInputManager;

        [Header("Events")]
        public UnityEvent<ValidationResult> OnPuzzleValidated;
        public UnityEvent OnPuzzleCompleted;
        public UnityEvent<string> OnPuzzleFailed;

        PuzzleDefinition _currentPuzzle;
        int _currentHintIndex;
        int _attemptCount;

        public PuzzleDefinition CurrentPuzzle => _currentPuzzle;
        public int AttemptCount => _attemptCount;

        void Awake()
        {
            if (codeEditor != null)
                codeEditor.OnRunClicked += OnRunCode;
        }

        void OnDestroy()
        {
            if (codeEditor != null)
                codeEditor.OnRunClicked -= OnRunCode;
        }

        public void LoadPuzzle(PuzzleDefinition puzzle)
        {
            if (puzzle == null)
            {
                Debug.LogError("[PuzzleManager] Puzzle is null!");
                return;
            }

            _currentPuzzle = puzzle;
            _currentHintIndex = 0;
            _attemptCount = 0;

            tabManager?.ClearAllTabs();

            // Display the template code with highlighted gap markers (read-only)
            string displayCode = CodeEditor.SyntaxHighlighter.HighlightWithGaps(puzzle.templateCode);
            tabManager?.OpenTab(
                "main",
                GetFileNameForConcept(puzzle.concept),
                puzzle.templateCode,
                true // read-only: player fills gaps via input fields below
            );

            // Open additional read-only tabs
            if (puzzle.additionalFiles != null)
            {
                foreach (var file in puzzle.additionalFiles)
                    tabManager?.OpenTab(file.fileName, file.fileName, file.code, true);
            }

            tabManager?.SelectTab("main");

            // Setup gap input fields
            gapInputManager?.SetupGaps(puzzle.gaps);

            // Console welcome messages
            console?.Clear();
            console?.LogInfo($"Puzzle : {puzzle.puzzleName}");
            console?.LogInfo(puzzle.description);
            console?.LogInfo("Remplis les champs ci-dessous et clique sur > Run !");
        }

        void OnRunCode(string code)
        {
            if (_currentPuzzle == null)
            {
                console?.LogError("Aucun puzzle chargé !");
                return;
            }

            _attemptCount++;

            // Collect answers from gap inputs
            Dictionary<int, string> answers = gapInputManager?.CollectAnswers()
                ?? new Dictionary<int, string>();

            console?.LogInfo($"Compilation en cours...");

            // Validate
            ValidationResult result = PuzzleValidator.Validate(_currentPuzzle, answers);

            OnPuzzleValidated?.Invoke(result);

            if (result.IsSuccess)
            {
                HandleSuccess(result);
            }
            else
            {
                HandleFailure(result);
            }
        }

        void HandleSuccess(ValidationResult result)
        {
            console?.LogSuccess(result.Message);
            console?.LogSuccess($"Score : {result.CorrectCount}/{result.TotalCount} ({result.Score:P0})");

            // Show the final completed code
            string finalCode = PuzzleValidator.BuildFinalCode(
                _currentPuzzle,
                gapInputManager?.CollectAnswers() ?? new Dictionary<int, string>()
            );
            codeEditor?.LoadCode(finalCode, true);

            gapInputManager?.ShowAllCorrect();
            OnPuzzleCompleted?.Invoke();
        }

        void HandleFailure(ValidationResult result)
        {
            console?.LogError(result.Message);

            foreach (var gapResult in result.GapResults)
            {
                if (!gapResult.IsCorrect)
                {
                    console?.LogWarning($"  Champ [{gapResult.GapIndex}] : {gapResult.Message}");
                }
            }

            // Show progressive hint from drone C-Sharp
            if (_currentPuzzle.hints != null && _currentHintIndex < _currentPuzzle.hints.Count)
            {
                string hint = _currentPuzzle.hints[_currentHintIndex];
                console?.LogHint($"C-Sharp : \"{hint}\"");
                _currentHintIndex++;
            }
            else if (_attemptCount >= 3)
            {
                console?.LogHint("C-Sharp : \"Tu bloques ? Regarde bien le type attendu pour chaque champ.\"");
            }

            gapInputManager?.HighlightErrors(result.GapResults);
            OnPuzzleFailed?.Invoke(result.Message);
        }

        string GetFileNameForConcept(ConceptType concept)
        {
            switch (concept)
            {
                case ConceptType.Variables:  return "TempleInit.cs";
                case ConceptType.Conditions: return "GolemAI.cs";
                case ConceptType.SwitchCase: return "ElementalPedestal.cs";
                case ConceptType.Loops:      return "BridgeBuilder.cs";
                case ConceptType.Functions:  return "GeneratorSync.cs";
                default:                     return "Script.cs";
            }
        }

        public void ResetPuzzle()
        {
            if (_currentPuzzle != null)
                LoadPuzzle(_currentPuzzle);
        }

        public void RequestHint()
        {
            if (_currentPuzzle?.hints == null || _currentHintIndex >= _currentPuzzle.hints.Count)
            {
                console?.LogHint("C-Sharp : \"Je n'ai plus d'indices... Tu peux y arriver !\"");
                return;
            }

            string hint = _currentPuzzle.hints[_currentHintIndex];
            console?.LogHint($"C-Sharp : \"{hint}\"");
            _currentHintIndex++;
        }
    }
}
