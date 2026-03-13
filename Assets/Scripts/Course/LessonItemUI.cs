using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Codex.Course
{
    /// <summary>
    /// Individual lesson item in the course progress list.
    /// Shows lesson name, icon, and lock/progress state.
    /// </summary>
    public class LessonItemUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] TMP_Text lessonNameText;
        [SerializeField] Image iconImage;
        [SerializeField] Image lockIcon;
        [SerializeField] Image checkmarkIcon;
        [SerializeField] Button button;
        [SerializeField] Image progressIndicator;

        [Header("State Colors")]
        [SerializeField] Color lockedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        [SerializeField] Color availableColor = new Color(0.2f, 0.6f, 0.9f, 1f);
        [SerializeField] Color completedColor = new Color(0.2f, 0.8f, 0.4f, 1f);

        LessonData _lesson;
        CourseManager _manager;

        public void Initialize(LessonData lesson, CourseManager manager)
        {
            _lesson = lesson;
            _manager = manager;

            if (lessonNameText != null)
                lessonNameText.text = lesson.lessonName;

            if (iconImage != null && lesson.icon != null)
                iconImage.sprite = lesson.icon;

            if (button != null)
                button.onClick.AddListener(OnClick);

            Refresh();
        }

        public void Refresh()
        {
            if (_lesson == null || _manager == null) return;

            LessonState state = _manager.GetLessonState(_lesson);

            bool isLocked = state == LessonState.Locked;
            bool isCompleted = state == LessonState.Completed;

            if (button != null) button.interactable = !isLocked;
            if (lockIcon != null) lockIcon.gameObject.SetActive(isLocked);
            if (checkmarkIcon != null) checkmarkIcon.gameObject.SetActive(isCompleted);

            if (progressIndicator != null)
            {
                switch (state)
                {
                    case LessonState.Locked:
                        progressIndicator.color = lockedColor;
                        break;
                    case LessonState.Available:
                        progressIndicator.color = availableColor;
                        break;
                    case LessonState.Completed:
                        progressIndicator.color = completedColor;
                        break;
                }
            }

            if (lessonNameText != null)
                lessonNameText.color = isLocked ? lockedColor : Color.white;
        }

        void OnClick()
        {
            _manager?.StartLesson(_lesson);
        }
    }
}
