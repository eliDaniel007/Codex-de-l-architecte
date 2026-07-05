using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Scène « Calculateur » : on y arrive en apportant une variable (x puis y) au
/// CPU pendant la mission Calcul. Le CPU enregistre la box, affiche l'opération
/// (x, y, x + y = résultat) puis renvoie au monde.
/// </summary>
public class CalculateurController : MonoBehaviour
{
    private string _message;
    private bool   _busy;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        // Le CPU reçoit la box portée (y → Parse, ou x/z → somme).
        _message = GameState.I.CpuRecevoir();

        ConstruireUI();
        Invoke(nameof(Retour), 4.5f); // retour auto au monde
    }

    void Update()
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb != null && (kb.escapeKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame ||
                           kb.spaceKey.wasPressedThisFrame))
            Retour();
#else
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            Retour();
#endif
    }

    void Retour()
    {
        if (_busy) return;
        _busy = true;
        CancelInvoke();
        GameState.I.cpuJustVisited = true; // anti re-entrée immédiate côté CPUZone
        SceneManager.LoadScene(GameState.I.mainSceneName);
    }

    // ── UI ────────────────────────────────────────────────────────────────

    void ConstruireUI()
    {
        var gs = GameState.I;

        var canvasGO = new GameObject("CalcUI");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();
        EnsureEventSystem();

        var fond = new GameObject("Fond");
        fond.transform.SetParent(canvasGO.transform, false);
        var img = fond.AddComponent<Image>();
        img.color = new Color(0.01f, 0.03f, 0.07f, 0.98f);
        var fr = fond.GetComponent<RectTransform>();
        fr.anchorMin = Vector2.zero; fr.anchorMax = Vector2.one;
        fr.offsetMin = fr.offsetMax = Vector2.zero;

        Txt("<color=#00D9FF>CALCULATEUR</color>  —  CPU",
            new Vector2(0.1f, 0.82f), new Vector2(0.9f, 0.93f), 58f, Color.white, FontStyles.Bold, canvasGO.transform);

        // Opération affichée selon la mission en cours.
        string op = "";
        var q = gs.QueteActuelle();
        if (q != null && q.kind == QuestKind.Parse)
        {
            // z = Int32.Parse(y)
            op = $"y = \"<color=#C86EFF>{gs.cpuY}</color>\"\n\n" +
                 $"z = Int32.Parse(y)\n\n" +
                 $"<color=#FFD27F>z = {gs.cpuZ}</color>";
        }
        else if (q != null && q.kind == QuestKind.Calcul)
        {
            // somme = x + z
            string xStr = $"x = <color=#00D9FF>{gs.cpuX}</color>";
            string zStr = (gs.missionEtape >= 2) ? $"z = <color=#00D9FF>{gs.cpuZ}</color>"
                                                 : "z = <color=#5A6473>?</color>";
            op = (gs.missionEtape >= 2)
                ? $"{xStr}\n{zStr}\n\n<color=#FFD27F>somme = x + z = {gs.cpuX} + {gs.cpuZ} = {gs.cpuSomme}</color>"
                : $"{xStr}\n{zStr}";
        }

        Txt(op, new Vector2(0.1f, 0.42f), new Vector2(0.9f, 0.78f), 60f, Color.white, FontStyles.Bold, canvasGO.transform);

        if (!string.IsNullOrEmpty(_message))
            Txt(_message, new Vector2(0.08f, 0.26f), new Vector2(0.92f, 0.4f), 30f,
                new Color(0.85f, 0.9f, 1f), FontStyles.Italic, canvasGO.transform);

        Txt("[Échap / Espace] revenir au monde",
            new Vector2(0.1f, 0.05f), new Vector2(0.9f, 0.12f), 24f,
            new Color(0.6f, 0.65f, 0.75f), FontStyles.Normal, canvasGO.transform);
    }

    void Txt(string texte, Vector2 ancMin, Vector2 ancMax, float taille, Color couleur, FontStyles style, Transform parent)
    {
        var go = new GameObject("Txt");
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = texte; tmp.fontSize = taille; tmp.color = couleur;
        tmp.alignment = TextAlignmentOptions.Center; tmp.richText = true; tmp.fontStyle = style;
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = ancMin; r.anchorMax = ancMax;
        r.offsetMin = r.offsetMax = Vector2.zero;
    }

    static void EnsureEventSystem()
    {
        if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var go = new GameObject("EventSystem");
            go.AddComponent<UnityEngine.EventSystems.EventSystem>();
            var uiType = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (uiType != null) go.AddComponent(uiType);
            else go.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
    }
}
