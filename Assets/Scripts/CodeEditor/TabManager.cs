using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Codex.CodeEditor
{
    /// <summary>
    /// Manages the file tab bar at the top of the code editor.
    /// Creates tabs dynamically at runtime — no prefab needed.
    /// </summary>
    public class TabManager : MonoBehaviour
    {
        [SerializeField] Transform tabContainer;
        [SerializeField] CodeEditorController editorController;

        [Header("Tab Colors")]
        [SerializeField] Color activeTabColor = new Color(0.18f, 0.20f, 0.25f, 1f);
        [SerializeField] Color inactiveTabColor = new Color(0.12f, 0.13f, 0.17f, 1f);

        readonly Dictionary<string, TabData> _tabs = new Dictionary<string, TabData>();
        readonly List<string> _tabOrder = new List<string>();
        string _activeTabId;

        public string ActiveTabId => _activeTabId;

        class TabData
        {
            public string FileName;
            public string Code;
            public GameObject TabGO;
            public Image Background;
            public TMP_Text Label;
            public bool IsReadOnly;
        }

        public void OpenTab(string tabId, string fileName, string initialCode, bool isReadOnly = false)
        {
            if (_tabs.ContainsKey(tabId))
            {
                SelectTab(tabId);
                return;
            }

            // Build tab UI dynamically
            GameObject tabGO = null;
            Image bg = null;
            TMP_Text label = null;

            if (tabContainer != null)
            {
                tabGO = new GameObject($"Tab_{tabId}");
                tabGO.transform.SetParent(tabContainer, false);
                tabGO.AddComponent<RectTransform>();

                bg = tabGO.AddComponent<Image>();
                bg.color = inactiveTabColor;

                var le = tabGO.AddComponent<LayoutElement>();
                le.preferredWidth = 200;
                le.minWidth = 120;

                var btn = tabGO.AddComponent<Button>();
                btn.targetGraphic = bg;
                string capturedId = tabId;
                btn.onClick.AddListener(() => SelectTab(capturedId));

                // File icon + name
                var labelGO = new GameObject("Label");
                labelGO.transform.SetParent(tabGO.transform, false);
                var labelRect = labelGO.AddComponent<RectTransform>();
                labelRect.anchorMin = Vector2.zero;
                labelRect.anchorMax = Vector2.one;
                labelRect.offsetMin = new Vector2(10, 0);
                labelRect.offsetMax = new Vector2(-10, 0);

                label = labelGO.AddComponent<TextMeshProUGUI>();
                label.text = $"\u25a0 {fileName}";
                label.fontSize = 15;
                label.color = Color.white;
                label.alignment = TextAlignmentOptions.MidlineLeft;
            }

            var data = new TabData
            {
                FileName = fileName,
                Code = initialCode,
                TabGO = tabGO,
                Background = bg,
                Label = label,
                IsReadOnly = isReadOnly
            };

            _tabs[tabId] = data;
            _tabOrder.Add(tabId);

            // Remove the static default tab on first real tab
            if (_tabOrder.Count == 1 && tabContainer != null)
            {
                var defaultTab = tabContainer.Find("DefaultTab");
                if (defaultTab != null) Destroy(defaultTab.gameObject);
            }

            SelectTab(tabId);
        }

        public void SelectTab(string tabId)
        {
            if (!_tabs.ContainsKey(tabId)) return;

            // Save current tab
            if (_activeTabId != null && _tabs.ContainsKey(_activeTabId))
            {
                var current = _tabs[_activeTabId];
                current.Code = editorController?.GetCurrentCode() ?? current.Code;
                SetTabVisual(current, false);
            }

            _activeTabId = tabId;
            var tab = _tabs[tabId];
            SetTabVisual(tab, true);
            editorController?.LoadCode(tab.Code, tab.IsReadOnly);
        }

        void SetTabVisual(TabData tab, bool active)
        {
            if (tab.Background != null)
                tab.Background.color = active ? activeTabColor : inactiveTabColor;
            if (tab.Label != null)
                tab.Label.color = active ? Color.white : new Color(0.55f, 0.55f, 0.55f);
        }

        public void CloseTab(string tabId)
        {
            if (!_tabs.ContainsKey(tabId)) return;
            var tab = _tabs[tabId];
            if (tab.TabGO != null) Destroy(tab.TabGO);
            _tabs.Remove(tabId);
            _tabOrder.Remove(tabId);

            if (_activeTabId == tabId)
            {
                _activeTabId = null;
                if (_tabOrder.Count > 0)
                    SelectTab(_tabOrder[_tabOrder.Count - 1]);
                else
                    editorController?.Clear();
            }
        }

        public void UpdateActiveTabCode(string code)
        {
            if (_activeTabId == null || !_tabs.ContainsKey(_activeTabId)) return;
            _tabs[_activeTabId].Code = code;
        }

        public string GetTabCode(string tabId)
        {
            return _tabs.ContainsKey(tabId) ? _tabs[tabId].Code : null;
        }

        public void ClearAllTabs()
        {
            foreach (var tab in _tabs.Values)
                if (tab.TabGO != null) Destroy(tab.TabGO);
            _tabs.Clear();
            _tabOrder.Clear();
            _activeTabId = null;
        }
    }
}
