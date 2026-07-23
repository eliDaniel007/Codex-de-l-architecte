using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Notifications « toast » : petites cartes qui glissent depuis la droite
/// (badge débloqué, mission notée, événement...). File d'attente : les
/// notifications s'affichent l'une après l'autre, ~3,5 s chacune.
/// Singleton créé par GameState ; survit aux changements de scène.
/// </summary>
public class NotificationsUI : MonoBehaviour
{
    public static NotificationsUI Instance { get; private set; }

    private readonly Queue<(string titre, string message, Color accent, string voix)> _attente
        = new Queue<(string, string, Color, string)>();
    private bool _occupe;
    private AudioSource _voix; // voix off des notifications

    private RectTransform   _carte;
    private CanvasGroup     _groupe;
    private Image           _barreAccent;
    private TextMeshProUGUI _titre, _message;

    public static void Ensure()
    {
        if (Instance == null)
        {
            var go = new GameObject("[Notifications]");
            go.AddComponent<NotificationsUI>();
        }
    }

    /// <summary>Empile une notification (titre + message + couleur d'accent).
    /// 'voix' = nom d'un clip Resources/Voix lu en voix off (optionnel).</summary>
    public static void Afficher(string titre, string message, Color? accent = null, string voix = null)
    {
        if (Instance == null) return;
        Instance._attente.Enqueue((titre, message, accent ?? new Color(0f, 0.85f, 1f), voix));
        if (!Instance._occupe) Instance.StartCoroutine(Instance.Derouler());
    }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        _voix = gameObject.AddComponent<AudioSource>();
        _voix.spatialBlend = 0f; // voix « dans l'oreille »
        BuildUI();
    }

    IEnumerator Derouler()
    {
        _occupe = true;
        while (_attente.Count > 0)
        {
            var (titre, message, accent, voix) = _attente.Dequeue();
            _titre.text        = titre;
            _message.text     = message;
            _barreAccent.color = accent;
            AudioFX.Succes();

            // Voix off de la notification : via la FILE globale (jamais deux voix
            // en même temps — elle attend aussi la fin de la radio des briefings).
            if (!string.IsNullOrEmpty(voix)) FileVoix.Jouer(voix);

            // glisse depuis la droite
            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / 0.3f)
            {
                float k = 1f - Mathf.Pow(1f - Mathf.Clamp01(t), 3f); // ease-out
                _carte.anchoredPosition = new Vector2(Mathf.Lerp(440f, -24f, k), _carte.anchoredPosition.y);
                _groupe.alpha = k;
                yield return null;
            }
            _carte.anchoredPosition = new Vector2(-24f, _carte.anchoredPosition.y);
            _groupe.alpha = 1f;

            yield return new WaitForSecondsRealtime(3.2f);

            // fondu de sortie
            for (float t = 1f; t > 0f; t -= Time.unscaledDeltaTime / 0.3f)
            { _groupe.alpha = t; yield return null; }
            _groupe.alpha = 0f;
        }
        _occupe = false;
    }

    void BuildUI()
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 130;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // Carte (ancrée à droite, sous le bouton menu)
        var carteGO = new GameObject("Carte");
        carteGO.transform.SetParent(transform, false);
        var fond = carteGO.AddComponent<Image>();
        fond.color = new Color(0f, 0.03f, 0.1f, 0.94f);
        fond.raycastTarget = false;
        _carte = carteGO.GetComponent<RectTransform>();
        _carte.anchorMin = new Vector2(1f, 1f);
        _carte.anchorMax = new Vector2(1f, 1f);
        _carte.pivot     = new Vector2(1f, 1f);
        _carte.anchoredPosition = new Vector2(440f, -95f); // hors écran au départ
        _carte.sizeDelta = new Vector2(410f, 96f);

        // Barre d'accent colorée à gauche de la carte
        var barreGO = new GameObject("Accent");
        barreGO.transform.SetParent(carteGO.transform, false);
        _barreAccent = barreGO.AddComponent<Image>();
        _barreAccent.raycastTarget = false;
        var ar = barreGO.GetComponent<RectTransform>();
        ar.anchorMin = new Vector2(0f, 0f); ar.anchorMax = new Vector2(0f, 1f);
        ar.pivot = new Vector2(0f, 0.5f);
        ar.anchoredPosition = Vector2.zero;
        ar.sizeDelta = new Vector2(7f, 0f);

        _titre   = Texte(carteGO.transform, new Vector2(0.05f, 0.52f), new Vector2(0.97f, 0.95f), 24f, FontStyles.Bold);
        _message = Texte(carteGO.transform, new Vector2(0.05f, 0.06f), new Vector2(0.97f, 0.5f),  19f, FontStyles.Normal);
        _message.color = new Color(0.75f, 0.8f, 0.9f);

        _groupe = carteGO.AddComponent<CanvasGroup>();
        _groupe.alpha = 0f;
        _groupe.blocksRaycasts = false;
    }

    TextMeshProUGUI Texte(Transform parent, Vector2 min, Vector2 max, float taille, FontStyles style)
    {
        var go = new GameObject("Txt");
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = taille; tmp.fontStyle = style;
        tmp.color = Color.white; tmp.richText = true;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.raycastTarget = false;
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = min; r.anchorMax = max;
        r.offsetMin = r.offsetMax = Vector2.zero;
        return tmp;
    }
}
