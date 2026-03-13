using System.Collections.Generic;
using UnityEngine;

namespace Codex.Course
{
    /// <summary>
    /// Top-level data asset that holds the entire course structure.
    /// Contains all lessons in order.
    /// </summary>
    [CreateAssetMenu(fileName = "CourseData", menuName = "Codex/Course Data")]
    public class CourseData : ScriptableObject
    {
        public string courseName = "Cours C#";
        public string courseDescription = "Apprends les fondamentaux du C# en sauvant Arcadia !";
        public List<LessonData> lessons = new List<LessonData>();
    }
}
