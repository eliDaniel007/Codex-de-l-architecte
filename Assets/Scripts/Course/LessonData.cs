using System.Collections.Generic;
using Codex.Puzzle;
using UnityEngine;

namespace Codex.Course
{
    /// <summary>
    /// Defines a lesson (chapter) within the course.
    /// Each lesson covers one C# concept and contains multiple puzzles of increasing difficulty.
    /// </summary>
    [CreateAssetMenu(fileName = "NewLesson", menuName = "Codex/Lesson Data")]
    public class LessonData : ScriptableObject
    {
        [Header("Identity")]
        public string lessonId;
        public string lessonName;
        [TextArea(2, 4)]
        public string description;
        public Sprite icon;

        [Header("Concept")]
        public ConceptType concept;

        [Header("Puzzles")]
        [Tooltip("Puzzles dans l'ordre de progression")]
        public List<PuzzleDefinition> puzzles = new List<PuzzleDefinition>();

        [Header("Unlock")]
        [Tooltip("Leçon requise pour débloquer celle-ci (null = débloquée par défaut)")]
        public LessonData prerequisite;

        [Header("Rewards")]
        public int xpReward = 100;
        public string unlockMessage = "Nouveau concept débloqué !";
    }
}
