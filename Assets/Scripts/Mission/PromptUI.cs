using UnityEngine;
using TMPro;

/// <summary>
/// Singleton Screen-Space canvas that displays one context-sensitive prompt
/// at the bottom of the screen (e.g. "[E] Utiliser le clavier").
/// Any script can call PromptUI.Show(...) / PromptUI.Hide().
/// </summary>
public class PromptUI : MonoBehaviour
{
    public static PromptUI Instance { get; private set; }

    private Canvas        _canvas;
    private TextMeshProUGUI _label;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildUI();
        Hide();
    }

    private void BuildUI()
    {
        _canvas = gameObject.AddComponent<Canvas>();
        _canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 100;

        var scaler = gameObject.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode         = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // Background pill
        var bg = new GameObject("Bg");
        bg.transform.SetParent(transform, false);
        var bgImg = bg.AddComponent<UnityEngine.UI.Image>();
        bgImg.color = new Color(0f, 0f, 0f, 0.65f);

        var bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0.5f, 0f);
        bgRect.anchorMax = new Vector2(0.5f, 0f);
        bgRect.pivot     = new Vector2(0.5f, 0f);
        bgRect.anchoredPosition = new Vector2(0f, 30f);
        bgRect.sizeDelta = new Vector2(800f, 60f);

        // Label
        var labelGO = new GameObject("Label");
        labelGO.transform.SetParent(bg.transform, false);
        _label = labelGO.AddComponent<TextMeshProUGUI>();
        _label.alignment  = TextAlignmentOptions.Center;
        _label.fontSize   = 22f;
        _label.color      = Color.white;

        var lr = labelGO.GetComponent<RectTransform>();
        lr.anchorMin = Vector2.zero;
        lr.anchorMax = Vector2.one;
        lr.offsetMin = Vector2.zero;
        lr.offsetMax = Vector2.zero;
    }

    public static void Show(string text)
    {
        if (Instance == null) CreateInstance();
        if (Instance._label.text != text || !Instance._canvas.enabled)
        {
            Debug.Log($"[PromptUI] Affichage du message : {text}");
        }
        Instance._label.text = text;
        Instance._canvas.enabled = true;
    }

    public static void Hide()
    {
        if (Instance == null) return;
        Instance._canvas.enabled = false;
    }

    private static void CreateInstance()
    {
        var go = new GameObject("PromptUI");
        go.AddComponent<PromptUI>();
    }

    /// <summary>Call once at game start to make sure the singleton exists.</summary>
    public static void EnsureInstance()
    {
        if (Instance == null) CreateInstance();
    }
}
