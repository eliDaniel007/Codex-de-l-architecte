using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Codex.Course
{
    /// <summary>
    /// UI component that displays the course list with lesson progress.
    /// Shows lesson items with lock/available/completed states.
    /// Matches the "Cours C#" panel shown in the Codex design.
    /// </summary>
    public class CourseProgressUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] TMP_Text courseTitleText;
        [SerializeField] Slider globalProgressBar;
        [SerializeField] TMP_Text progressPercentText;
        [SerializeField] Transform lessonListContainer;
        [SerializeField] Button addButton; // "+ Nouveau" button

        [Header("Prefab")]
        [SerializeField] GameObject lessonItemPrefab;

        CourseData _courseData;
        CourseManager _courseManager;
        readonly List<LessonItemUI> _lessonItems = new List<LessonItemUI>();

        public void Initialize(CourseData courseData, CourseManager manager)
        {
            _courseData = courseData;
            _courseManager = manager;

            if (courseTitleText != null)
                courseTitleText.text = courseData.courseName;

            BuildLessonList();
            RefreshAll();
        }

        void BuildLessonList()
        {
            // Clear existing
            foreach (var item in _lessonItems)
            {
                if (item != null) Destroy(item.gameObject);
            }
            _lessonItems.Clear();

            if (_courseData == null || lessonItemPrefab == null || lessonListContainer == null)
                return;

            foreach (var lesson in _courseData.lessons)
            {
                var go = Instantiate(lessonItemPrefab, lessonListContainer);
                var itemUI = go.GetComponent<LessonItemUI>();
                if (itemUI != null)
                {
                    itemUI.Initialize(lesson, _courseManager);
                    _lessonItems.Add(itemUI);
                }
            }
        }

        public void RefreshAll()
        {
            float progress = _courseManager?.GlobalProgress ?? 0f;

            if (globalProgressBar != null)
                globalProgressBar.value = progress;

            if (progressPercentText != null)
                progressPercentText.text = $"{progress:P0}";

            foreach (var item in _lessonItems)
            {
                item?.Refresh();
            }
        }
    }
}
