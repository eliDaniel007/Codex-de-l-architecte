using Codex.CodeEditor;
using Codex.Console;
using Codex.Course;
using Codex.Puzzle;
using UnityEngine;
using UnityEngine.UI;

namespace Codex.UI
{
    /// <summary>
    /// Master controller for the entire right-side Codex panel.
    /// Manages the split layout between code editor, course progress, and console.
    /// Handles panel open/close animations and state transitions.
    /// </summary>
    public class CodexPanelManager : MonoBehaviour
    {
        [Header("Panel Root")]
        [SerializeField] RectTransform panelRoot;
        [SerializeField] CanvasGroup canvasGroup;

        [Header("Sub-Panels")]
        [SerializeField] GameObject codeEditorPanel;
        [SerializeField] GameObject coursePanel;
        [SerializeField] GameObject consolePanel;

        [Header("Components")]
        [SerializeField] CodeEditorController codeEditor;
        [SerializeField] TabManager tabManager;
        [SerializeField] CodeConsole console;
        [SerializeField] CourseManager courseManager;
        [SerializeField] PuzzleManager puzzleManager;
        [SerializeField] CourseProgressUI courseProgressUI;

        [Header("Header")]
        [SerializeField] TMPro.TMP_Text headerTitle;
        [SerializeField] TMPro.TMP_Text headerSubtitle;

        [Header("Animation")]
        [SerializeField] float slideSpeed = 8f;

        bool _isOpen;
        bool _isAnimating;
        Vector2 _closedPosition;
        Vector2 _openPosition;

        public bool IsOpen => _isOpen;

        void Awake()
        {
            if (panelRoot != null)
            {
                _openPosition = panelRoot.anchoredPosition;
                _closedPosition = new Vector2(panelRoot.rect.width + 50f, _openPosition.y);
            }

            if (puzzleManager != null)
            {
                puzzleManager.OnPuzzleCompleted.AddListener(OnPuzzleCompleted);
                puzzleManager.OnPuzzleFailed.AddListener(OnPuzzleFailed);
            }
        }

        void OnDestroy()
        {
            if (puzzleManager != null)
            {
                puzzleManager.OnPuzzleCompleted.RemoveListener(OnPuzzleCompleted);
                puzzleManager.OnPuzzleFailed.RemoveListener(OnPuzzleFailed);
            }
        }

        // --- Public API ---

        public void OpenPanel()
        {
            gameObject.SetActive(true);
            _isOpen = true;
            _isAnimating = true;
        }

        public void ClosePanel()
        {
            _isOpen = false;
            _isAnimating = true;
        }

        public void TogglePanel()
        {
            if (_isOpen) ClosePanel();
            else OpenPanel();
        }

        public void OpenWithPuzzle(PuzzleDefinition puzzle)
        {
            OpenPanel();
            ShowBoth();
            puzzleManager?.LoadPuzzle(puzzle);
            UpdateHeader($"Apprendre C# - {puzzle.puzzleName}", puzzle.description);
        }

        public void ShowCodeEditor()
        {
            SetActivePanel(codeEditorPanel: true, coursePanel: false);
        }

        public void ShowCourse()
        {
            SetActivePanel(codeEditorPanel: false, coursePanel: true);
        }

        public void ShowBoth()
        {
            SetActivePanel(codeEditorPanel: true, coursePanel: true);
        }

        void SetActivePanel(bool codeEditorPanel, bool coursePanel)
        {
            if (this.codeEditorPanel != null) this.codeEditorPanel.SetActive(codeEditorPanel);
            if (this.coursePanel != null) this.coursePanel.SetActive(coursePanel);
            if (consolePanel != null) consolePanel.SetActive(true); // Console always visible
        }

        void UpdateHeader(string title, string subtitle = "")
        {
            if (headerTitle != null) headerTitle.text = title;
            if (headerSubtitle != null) headerSubtitle.text = subtitle;
        }

        void Update()
        {
            if (!_isAnimating || panelRoot == null) return;

            // Instant show/hide (no animation for now to keep things simple)
            if (canvasGroup != null)
            {
                canvasGroup.alpha = _isOpen ? 1f : 0f;
                canvasGroup.interactable = _isOpen;
                canvasGroup.blocksRaycasts = _isOpen;
            }

            _isAnimating = false;

            if (!_isOpen)
                gameObject.SetActive(false);
        }

        // --- Events ---

        void OnPuzzleCompleted()
        {
            UpdateHeader("Succès !", "Le code a été compilé avec succès !");
        }

        void OnPuzzleFailed(string message)
        {
            UpdateHeader("Erreur", "Corrige les erreurs et réessaie.");
        }
    }
}
