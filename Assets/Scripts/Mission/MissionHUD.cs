using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// HUD de mission façon Hitman : bandeau en haut à gauche affichant la mission
/// en cours (n°, titre) et l'indication (où aller / quoi faire).
/// Visible uniquement dans la MainScene (les autres scènes ont leur propre UI).
/// Singleton créé par GameState ; survit aux changements de scène.
/// </summary>
public class MissionHUD : MonoBehaviour
{
    public static MissionHUD Instance { get; private set; }

    private Canvas          _canvas;
    private TextMeshProUGUI _numero;
    private TextMeshProUGUI _titre;
    private TextMeshProUGUI _indication;

    public static void Ensure()
    {
        if (Instance == null)
        {
            var go = new GameObject("[MissionHUD]");
            go.AddComponent<MissionHUD>();
        }
    }

    public static void Refresh()
    {
        if (Instance != null) Instance.Actualiser();
    }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildUI();
        SceneManager.sceneLoaded += OnSceneLoaded;
        AppliquerVisibilite(SceneManager.GetActiveScene().name);
        Actualiser();
    }

    void OnDestroy()
    {
        if (Instance == this) SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AppliquerVisibilite(scene.name);
        Actualiser();
    }

    void AppliquerVisibilite(string sceneName)
    {
        _canvas.enabled = (sceneName == GameState.I.mainSceneName);
    }

    void Actualiser()
    {
        var gs = GameState.I;
        var q  = gs.QueteActuelle();

        if (q == null || gs.ToutesQuetesTerminees())
        {
            _numero.text     = "MISSIONS";
            _titre.text      = "Toutes les missions sont terminées !";
            _indication.text = "Bravo, architecte.";
            return;
        }

        _numero.text     = $"MISSION {gs.questIndex + 1}/{gs.quests.Count}";
        _titre.text      = q.titre;
        _indication.text = string.IsNullOrEmpty(q.indication) ? q.description : q.indication;
    }

    // ── UI ────────────────────────────────────────────────────────────────

    void BuildUI()
    {
        _canvas = gameObject.AddComponent<Canvas>();
        _canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 80;

        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // Panneau haut-gauche
        var panel = new GameObject("Panel");
        panel.transform.SetParent(transform, false);
        var bg = panel.AddComponent<Image>();
        bg.color = new Color(0f, 0.02f, 0.08f, 0.72f);
        bg.raycastTarget = false;

        var pr = panel.GetComponent<RectTransform>();
        pr.anchorMin = new Vector2(0f, 1f);
        pr.anchorMax = new Vector2(0f, 1f);
        pr.pivot     = new Vector2(0f, 1f);
        pr.anchoredPosition = new Vector2(18f, -18f);
        pr.sizeDelta = new Vector2(600f, 140f);

        // Liseré cyan à gauche (style Hitman)
        var bar = new GameObject("Bar");
        bar.transform.SetParent(panel.transform, false);
        var barImg = bar.AddComponent<Image>();
        barImg.color = new Color(0f, 0.85f, 1f, 0.95f);
        barImg.raycastTarget = false;
        var br = bar.GetComponent<RectTransform>();
        br.anchorMin = new Vector2(0f, 0f);
        br.anchorMax = new Vector2(0f, 1f);
        br.pivot     = new Vector2(0f, 0.5f);
        br.anchoredPosition = Vector2.zero;
        br.sizeDelta = new Vector2(5f, 0f);

        _numero     = CreerTexte(panel.transform, 18f, new Color(0f, 0.85f, 1f), FontStyles.Bold,
                                 new Vector2(18f, -10f), new Vector2(560f, 26f));
        _titre      = CreerTexte(panel.transform, 28f, Color.white, FontStyles.Bold,
                                 new Vector2(18f, -38f), new Vector2(560f, 36f));
        _indication = CreerTexte(panel.transform, 20f, new Color(0.78f, 0.84f, 0.95f), FontStyles.Italic,
                                 new Vector2(18f, -80f), new Vector2(560f, 52f));
    }

    TextMeshProUGUI CreerTexte(Transform parent, float taille, Color couleur, FontStyles style,
                               Vector2 pos, Vector2 size)
    {
        var go = new GameObject("Txt");
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.fontSize      = taille;
        tmp.color         = couleur;
        tmp.fontStyle     = style;
        tmp.alignment     = TextAlignmentOptions.TopLeft;
        tmp.richText      = true;
        tmp.raycastTarget = false;
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(0f, 1f);
        r.anchorMax = new Vector2(0f, 1f);
        r.pivot     = new Vector2(0f, 1f);
        r.anchoredPosition = pos;
        r.sizeDelta = size;
        return tmp;
    }
}
