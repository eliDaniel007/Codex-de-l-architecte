using System.Collections.Generic;
using Codex.Core;
using Codex.Puzzle;
using UnityEngine;
using UnityEngine.Events;

namespace Codex.Course
{
    /// <summary>
    /// Manages course progression, lesson unlocking, and current lesson/puzzle tracking.
    /// Works with PlayerProgress for persistence.
    /// </summary>
    public class CourseManager : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] CourseData courseData;

        [Header("References")]
        [SerializeField] PuzzleManager puzzleManager;
        [SerializeField] CourseProgressUI progressUI;

        [Header("Events")]
        public UnityEvent<LessonData> OnLessonStarted;
        public UnityEvent<LessonData> OnLessonCompleted;
        public UnityEvent<float> OnProgressUpdated; // 0-1

        LessonData _currentLesson;
        int _currentPuzzleIndex;

        public CourseData Course => courseData;
        public LessonData CurrentLesson => _currentLesson;
        public float GlobalProgress => CalculateGlobalProgress();

        void Start()
        {
            if (progressUI != null && courseData != null)
                progressUI.Initialize(courseData, this);

            if (puzzleManager != null)
                puzzleManager.OnPuzzleCompleted.AddListener(OnCurrentPuzzleCompleted);
        }

        void OnDestroy()
        {
            if (puzzleManager != null)
                puzzleManager.OnPuzzleCompleted.RemoveListener(OnCurrentPuzzleCompleted);
        }

        public void StartLesson(LessonData lesson)
        {
            if (lesson == null || !IsLessonUnlocked(lesson)) return;

            _currentLesson = lesson;
            _currentPuzzleIndex = GetFirstUncompletedPuzzleIndex(lesson);

            if (_currentPuzzleIndex < lesson.puzzles.Count)
            {
                puzzleManager?.LoadPuzzle(lesson.puzzles[_currentPuzzleIndex]);
            }

            OnLessonStarted?.Invoke(lesson);
        }

        public void StartLesson(int lessonIndex)
        {
            if (courseData == null || lessonIndex < 0 || lessonIndex >= courseData.lessons.Count)
                return;
            StartLesson(courseData.lessons[lessonIndex]);
        }

        public bool IsLessonUnlocked(LessonData lesson)
        {
            if (lesson.prerequisite == null) return true;
            return PlayerProgress.Instance != null &&
                   PlayerProgress.Instance.IsLessonCompleted(lesson.prerequisite.lessonId);
        }

        public bool IsLessonCompleted(LessonData lesson)
        {
            return PlayerProgress.Instance != null &&
                   PlayerProgress.Instance.IsLessonCompleted(lesson.lessonId);
        }

        public LessonState GetLessonState(LessonData lesson)
        {
            if (IsLessonCompleted(lesson)) return LessonState.Completed;
            if (IsLessonUnlocked(lesson)) return LessonState.Available;
            return LessonState.Locked;
        }

        void OnCurrentPuzzleCompleted()
        {
            if (_currentLesson == null) return;

            // Mark puzzle as completed
            var puzzle = _currentLesson.puzzles[_currentPuzzleIndex];
            PlayerProgress.Instance?.CompletePuzzle(_currentLesson.lessonId, puzzle.puzzleId);

            _currentPuzzleIndex++;

            if (_currentPuzzleIndex >= _currentLesson.puzzles.Count)
            {
                CompletCurrentLesson();
            }
            else
            {
                // Auto-load next puzzle after a delay
                Invoke(nameof(LoadNextPuzzle), 2f);
            }

            OnProgressUpdated?.Invoke(GlobalProgress);
            progressUI?.RefreshAll();
        }

        void LoadNextPuzzle()
        {
            if (_currentLesson != null && _currentPuzzleIndex < _currentLesson.puzzles.Count)
            {
                puzzleManager?.LoadPuzzle(_currentLesson.puzzles[_currentPuzzleIndex]);
            }
        }

        void CompletCurrentLesson()
        {
            PlayerProgress.Instance?.CompleteLesson(_currentLesson.lessonId);
            PlayerProgress.Instance?.AddXP(_currentLesson.xpReward);

            OnLessonCompleted?.Invoke(_currentLesson);
        }

        int GetFirstUncompletedPuzzleIndex(LessonData lesson)
        {
            for (int i = 0; i < lesson.puzzles.Count; i++)
            {
                bool completed = PlayerProgress.Instance != null &&
                    PlayerProgress.Instance.IsPuzzleCompleted(lesson.lessonId, lesson.puzzles[i].puzzleId);
                if (!completed) return i;
            }
            return 0; // All completed, restart from beginning
        }

        float CalculateGlobalProgress()
        {
            if (courseData == null || courseData.lessons.Count == 0) return 0f;

            int total = 0;
            int completed = 0;

            foreach (var lesson in courseData.lessons)
            {
                total += lesson.puzzles.Count;
                foreach (var puzzle in lesson.puzzles)
                {
                    if (PlayerProgress.Instance != null &&
                        PlayerProgress.Instance.IsPuzzleCompleted(lesson.lessonId, puzzle.puzzleId))
                    {
                        completed++;
                    }
                }
            }

            return total > 0 ? (float)completed / total : 0f;
        }
    }

    public enum LessonState
    {
        Locked,
        Available,
        Completed
    }
}
