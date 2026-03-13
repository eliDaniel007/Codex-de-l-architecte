using Codex.Course;
using Codex.Puzzle;
using Codex.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Codex.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Managers")]
        [SerializeField] CodexPanelManager codexPanel;
        [SerializeField] CourseManager courseManager;
        [SerializeField] PuzzleManager puzzleManager;

        [Header("Settings")]
        [SerializeField] bool startWithCodexOpen = false; // disabled for now

        [Header("Debug")]
        [SerializeField] PuzzleDefinition testPuzzle;
        [SerializeField] bool loadDemoPuzzle = false; // disabled for now
        [SerializeField] DemoPuzzleType demoPuzzleType = DemoPuzzleType.Variables;

        public CodexPanelManager CodexPanel => codexPanel;
        public CourseManager CourseManager => courseManager;
        public PuzzleManager PuzzleManager => puzzleManager;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void Start()
        {
            // Old Codex system disabled — using CodexTestUI instead
            // Uncomment below when the full system is ready:
            //
            // if (startWithCodexOpen && codexPanel != null)
            // {
            //     codexPanel.ShowBoth();
            //     codexPanel.OpenPanel();
            // }
            // if (loadDemoPuzzle && puzzleManager != null)
            //     LoadDemoPuzzle();
        }

        void LoadDemoPuzzle()
        {
            PuzzleDefinition demo;
            switch (demoPuzzleType)
            {
                case DemoPuzzleType.Variables:
                    demo = ExamplePuzzles.CreateVariablesPuzzle();
                    break;
                case DemoPuzzleType.Conditions:
                    demo = ExamplePuzzles.CreateConditionsPuzzle();
                    break;
                case DemoPuzzleType.Loops:
                    demo = ExamplePuzzles.CreateLoopPuzzle();
                    break;
                default:
                    demo = ExamplePuzzles.CreateVariablesPuzzle();
                    break;
            }
            codexPanel?.OpenWithPuzzle(demo);
        }

        public void TriggerPuzzle(PuzzleDefinition puzzle)
        {
            if (codexPanel == null) return;
            codexPanel.OpenWithPuzzle(puzzle);
        }

        public void CloseCodex()
        {
            codexPanel?.ClosePanel();
        }

        void Update()
        {
            if (Keyboard.current == null) return;

            if (Keyboard.current.escapeKey.wasPressedThisFrame)
                codexPanel?.TogglePanel();
        }
    }

    public enum DemoPuzzleType
    {
        Variables,
        Conditions,
        Loops
    }
}
