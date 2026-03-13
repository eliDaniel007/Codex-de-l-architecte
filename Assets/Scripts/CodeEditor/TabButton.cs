using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Codex.CodeEditor
{
    /// <summary>
    /// Represents a single file tab in the code editor.
    /// Handles visual state (active/inactive) and click events.
    /// </summary>
    public class TabButton : MonoBehaviour
    {
        [SerializeField] TMP_Text fileNameText;
        [SerializeField] Image background;
        [SerializeField] Button button;
        [SerializeField] Button closeButton;

        [Header("Colors")]
        [SerializeField] Color activeColor = new Color(0.18f, 0.20f, 0.25f, 1f);
        [SerializeField] Color inactiveColor = new Color(0.12f, 0.13f, 0.17f, 1f);
        [SerializeField] Color activeTextColor = Color.white;
        [SerializeField] Color inactiveTextColor = new Color(0.6f, 0.6f, 0.6f, 1f);

        string _tabId;
        TabManager _manager;

        public string TabId => _tabId;

        public void Initialize(string tabId, string fileName, TabManager manager)
        {
            _tabId = tabId;
            _manager = manager;

            if (fileNameText != null)
                fileNameText.text = fileName;

            if (button != null)
                button.onClick.AddListener(OnClick);

            if (closeButton != null)
                closeButton.onClick.AddListener(OnClose);
        }

        void OnClick()
        {
            _manager?.SelectTab(_tabId);
        }

        void OnClose()
        {
            _manager?.CloseTab(_tabId);
        }

        public void SetActive(bool isActive)
        {
            if (background != null)
                background.color = isActive ? activeColor : inactiveColor;

            if (fileNameText != null)
                fileNameText.color = isActive ? activeTextColor : inactiveTextColor;
        }
    }
}
