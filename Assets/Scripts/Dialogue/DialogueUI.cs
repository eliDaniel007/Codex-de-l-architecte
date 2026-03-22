using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using Codex.Core;

namespace Codex.Dialogue
{
    public class DialogueUI : MonoBehaviour
    {
        public static DialogueUI Instance { get; private set; }

        GameObject _canvasGO;
        GameObject _panel;
        Image _portrait;
        TMP_Text _nameText;
        TMP_Text _dialogueText;
        TMP_Text _continueHint;

        bool _isPlaying;
        bool _isTyping;
        bool _skipRequested;
        Coroutine _typeRoutine;

        DialogueData _currentDialogue;
        int _lineIndex;
        System.Action _onComplete;

        static readonly Color C_BG = new Color(0f, 0f, 0f, 0.85f);
        static readonly Color C_NAME = new Color(0.3f, 0.8f, 1f);
        static readonly Color C_TEXT = Color.white;
        static readonly Color C_HINT = new Color(0.6f, 0.6f, 0.6f);

        public bool IsPlaying => _isPlaying;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            BuildUI();
        }

        void Update()
        {
            if (!_isPlaying) return;

            bool pressed = (Keyboard.current != null &&
                (Keyboard.current.spaceKey.wasPressedThisFrame ||
                 Keyboard.current.enterKey.wasPressedThisFrame)) ||
                (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame);

            if (pressed)
            {
                if (_isTyping)
                    _skipRequested = true;
                else
                    NextLine();
            }
        }

        public void Play(DialogueData dialogue, System.Action onComplete = null)
        {
            if (dialogue == null || dialogue.lines.Count == 0) return;

            _currentDialogue = dialogue;
            _lineIndex = 0;
            _onComplete = onComplete;
            _isPlaying = true;
            _canvasGO.SetActive(true);

            if (WorldCodexBridge.Instance != null && !WorldCodexBridge.Instance.IsCodexOpen)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            ShowLine(_currentDialogue.lines[0]);
        }

        public void PlayLines(List<DialogueLine> lines, System.Action onComplete = null)
        {
            var data = ScriptableObject.CreateInstance<DialogueData>();
            data.lines = lines;
            Play(data, onComplete);
        }

        void NextLine()
        {
            _lineIndex++;
            if (_lineIndex >= _currentDialogue.lines.Count)
            {
                EndDialogue();
                return;
            }
            ShowLine(_currentDialogue.lines[_lineIndex]);
        }

        void ShowLine(DialogueLine line)
        {
            _nameText.text = line.speakerName;
            _nameText.color = C_NAME;

            if (line.portrait != null)
            {
                _portrait.sprite = line.portrait;
                _portrait.gameObject.SetActive(true);
            }
            else
            {
                _portrait.gameObject.SetActive(false);
            }

            _continueHint.gameObject.SetActive(false);

            if (_typeRoutine != null)
                StopCoroutine(_typeRoutine);
            _typeRoutine = StartCoroutine(TypeText(line));
        }

        IEnumerator TypeText(DialogueLine line)
        {
            _isTyping = true;
            _skipRequested = false;
            _dialogueText.text = "";

            foreach (char c in line.text)
            {
                if (_skipRequested)
                {
                    _dialogueText.text = line.text;
                    break;
                }
                _dialogueText.text += c;
                yield return new WaitForSeconds(line.letterDelay);
            }

            _isTyping = false;
            _dialogueText.text = line.text;

            if (line.waitForInput)
                _continueHint.gameObject.SetActive(true);
            else
            {
                yield return new WaitForSeconds(1.5f);
                NextLine();
            }
        }

        void EndDialogue()
        {
            _isPlaying = false;
            _canvasGO.SetActive(false);

            if (WorldCodexBridge.Instance != null && !WorldCodexBridge.Instance.IsCodexOpen)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            _onComplete?.Invoke();
        }

        void BuildUI()
        {
            _canvasGO = new GameObject("DialogueCanvas");
            _canvasGO.transform.SetParent(transform, false);
            var canvas = _canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;
            var scaler = _canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.matchWidthOrHeight = 0.5f;
            _canvasGO.AddComponent<GraphicRaycaster>();

            _panel = new GameObject("DialoguePanel");
            _panel.transform.SetParent(_canvasGO.transform, false);
            var panelRT = _panel.AddComponent<RectTransform>();
            panelRT.anchorMin = new Vector2(0.05f, 0.02f);
            panelRT.anchorMax = new Vector2(0.95f, 0.28f);
            panelRT.offsetMin = Vector2.zero;
            panelRT.offsetMax = Vector2.zero;

            var panelImg = _panel.AddComponent<Image>();
            panelImg.color = C_BG;
            panelImg.raycastTarget = true;

            var hlg = _panel.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(16, 16, 12, 12);
            hlg.spacing = 16;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            var portraitGO = new GameObject("Portrait");
            portraitGO.transform.SetParent(_panel.transform, false);
            portraitGO.AddComponent<RectTransform>();
            var portraitLE = portraitGO.AddComponent<LayoutElement>();
            portraitLE.minWidth = 100;
            portraitLE.preferredWidth = 100;
            portraitLE.minHeight = 100;
            _portrait = portraitGO.AddComponent<Image>();
            _portrait.color = new Color(0.2f, 0.2f, 0.2f);
            _portrait.preserveAspect = true;

            var textArea = new GameObject("TextArea");
            textArea.transform.SetParent(_panel.transform, false);
            textArea.AddComponent<RectTransform>();
            var textLE = textArea.AddComponent<LayoutElement>();
            textLE.flexibleWidth = 1;
            var vlg = textArea.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 6;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;

            var nameGO = new GameObject("Name");
            nameGO.transform.SetParent(textArea.transform, false);
            nameGO.AddComponent<RectTransform>();
            var nameLE = nameGO.AddComponent<LayoutElement>();
            nameLE.minHeight = 28;
            _nameText = nameGO.AddComponent<TextMeshProUGUI>();
            _nameText.fontSize = 20;
            _nameText.fontStyle = FontStyles.Bold;
            _nameText.color = C_NAME;
            _nameText.raycastTarget = false;

            var textGO = new GameObject("Text");
            textGO.transform.SetParent(textArea.transform, false);
            textGO.AddComponent<RectTransform>();
            var textTLE = textGO.AddComponent<LayoutElement>();
            textTLE.flexibleHeight = 1;
            _dialogueText = textGO.AddComponent<TextMeshProUGUI>();
            _dialogueText.fontSize = 17;
            _dialogueText.color = C_TEXT;
            _dialogueText.raycastTarget = false;
            _dialogueText.enableWordWrapping = true;

            var hintGO = new GameObject("ContinueHint");
            hintGO.transform.SetParent(textArea.transform, false);
            hintGO.AddComponent<RectTransform>();
            var hintLE = hintGO.AddComponent<LayoutElement>();
            hintLE.minHeight = 20;
            _continueHint = hintGO.AddComponent<TextMeshProUGUI>();
            _continueHint.text = "[Espace / Clic] Continuer...";
            _continueHint.fontSize = 13;
            _continueHint.fontStyle = FontStyles.Italic;
            _continueHint.color = C_HINT;
            _continueHint.alignment = TextAlignmentOptions.Right;
            _continueHint.raycastTarget = false;

            _canvasGO.SetActive(false);
        }
    }
}
