using System;
using System.Collections.Generic;
using UnityEngine;

namespace Codex.Core
{
    /// <summary>
    /// Singleton that tracks all player progress.
    /// Handles lesson completion, puzzle completion, XP, and persistence via PlayerPrefs.
    /// </summary>
    public class PlayerProgress : MonoBehaviour
    {
        public static PlayerProgress Instance { get; private set; }

        [Header("Debug")]
        [SerializeField] bool resetOnStart;

        ProgressData _data;

        public int TotalXP => _data.totalXP;
        public int Level => _data.totalXP / 500 + 1; // 500 XP per level

        public event Action<int> OnXPChanged;
        public event Action<string> OnLessonCompleted;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);

            if (resetOnStart)
                ResetAll();
            else
                Load();
        }

        // --- Lesson Progress ---

        public void CompleteLesson(string lessonId)
        {
            if (!_data.completedLessons.Contains(lessonId))
            {
                _data.completedLessons.Add(lessonId);
                Save();
                OnLessonCompleted?.Invoke(lessonId);
            }
        }

        public bool IsLessonCompleted(string lessonId)
        {
            return _data.completedLessons.Contains(lessonId);
        }

        // --- Puzzle Progress ---

        public void CompletePuzzle(string lessonId, string puzzleId)
        {
            string key = $"{lessonId}:{puzzleId}";
            if (!_data.completedPuzzles.Contains(key))
            {
                _data.completedPuzzles.Add(key);
                Save();
            }
        }

        public bool IsPuzzleCompleted(string lessonId, string puzzleId)
        {
            return _data.completedPuzzles.Contains($"{lessonId}:{puzzleId}");
        }

        // --- XP ---

        public void AddXP(int amount)
        {
            _data.totalXP += amount;
            Save();
            OnXPChanged?.Invoke(_data.totalXP);
        }

        // --- Stats ---

        public void RecordAttempt(string puzzleId, bool success)
        {
            _data.totalAttempts++;
            if (success) _data.successfulAttempts++;
            Save();
        }

        public float GetSuccessRate()
        {
            if (_data.totalAttempts == 0) return 0f;
            return (float)_data.successfulAttempts / _data.totalAttempts;
        }

        // --- Persistence ---

        void Save()
        {
            string json = JsonUtility.ToJson(_data);
            PlayerPrefs.SetString("CodexProgress", json);
            PlayerPrefs.Save();
        }

        void Load()
        {
            string json = PlayerPrefs.GetString("CodexProgress", "");
            if (string.IsNullOrEmpty(json))
            {
                _data = new ProgressData();
            }
            else
            {
                _data = JsonUtility.FromJson<ProgressData>(json);
                if (_data == null) _data = new ProgressData();
            }
        }

        public void ResetAll()
        {
            _data = new ProgressData();
            PlayerPrefs.DeleteKey("CodexProgress");
            PlayerPrefs.Save();
        }

        [Serializable]
        class ProgressData
        {
            public List<string> completedLessons = new List<string>();
            public List<string> completedPuzzles = new List<string>();
            public int totalXP;
            public int totalAttempts;
            public int successfulAttempts;
        }
    }
}
