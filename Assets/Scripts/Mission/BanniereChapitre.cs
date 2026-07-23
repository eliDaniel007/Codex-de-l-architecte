using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Bannière plein écran de FIN DE CHAPITRE (façon « MISSION COMPLETE ») :
/// bande sombre horizontale qui s'ouvre au centre, gros titre doré, sous-titre,
/// puis fondu. ~3 secondes, déclenchée par GameState aux jalons du programme.
/// Singleton créé par GameState.
/// </summary>
public class BanniereChapitre : MonoBehaviour
{
    public static BanniereChapitre Instance { get; private set; }

    private CanvasGroup     _groupe;
    private RectTransform   _bande;
    private TextMeshProUGUI _titre, _sousTitre;

    public static void Ensure()
    {
        if (Instance == null)
        {
            var go = new GameObject("[BanniereChapitre]");
            go.AddComponent<BanniereChapitre>();
        }
    }

    /// <summary>Affiche la bannière (titre doré + sous-titre) pendant ~3 s,
    /// avec une voix off optionnelle (clip Resources/Voix).</summary>
    public static void Afficher(string titre, string sousTitre, string voix = null)
    {
        if (Instance == null) return;

        // Voix off via la FILE globale : elle passera après les autres voix.
        if (!string.IsNullOrEmpty(voix)) FileVoix.Jouer(voix);

        Instance.StopAllCoroutines();
        Instance.StartCoroutine(Instance.Jouer(titre, sousTitre));
    }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildUI();
    }

    IEnumerator Jouer(string titre, string sousTitre)
    {
        _titre.text     = titre;
        _sousTitre.text = sousTitre;

        // La bande s'ouvre verticalement au centre de l'écran.
        _groupe.alpha = 1f;
        for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / 0.3f)
        {
            float k = Mathf.SmoothStep(0f, 1f, t);
            _bande.anchorMin = new Vector2(0f, Mathf.Lerp(0.5f, 0.38f, k));
            _bande.anchorMax = new Vector2(1f, Mathf.Lerp(0.5f, 0.62f, k));
            yield return null;
        }
        _bande.anchorMin = new Vector2(0f, 0.38f);
        _bande.anchorMax = new Vector2(1f, 0.62f);

        yield return new WaitForSecondsRealtime(2.2f);

        // Fondu de sortie.
        for (float t = 1f; t > 0f; t -= Time.unscaledDeltaTime / 0.4f)
        {
            _groupe.alpha = t;
            yield return null;
        }
        _groupe.alpha = 0f;
    }

    void BuildUI()
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 145; // sous l'écran de rating, au-dessus du HUD
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // Bande sombre pleine largeur
        var bandeGO = new GameObject("Bande");
        bandeGO.transform.SetParent(transform, false);
        var fond = bandeGO.AddComponent<Image>();
        fond.color = new Color(0f, 0.015f, 0.06f, 0.93f);
        fond.raycastTarget = false;
        _bande = bandeGO.GetComponent<RectTransform>();
        _bande.anchorMin = new Vector2(0f, 0.5f);
        _bande.anchorMax = new Vector2(1f, 0.5f);
        _bande.offsetMin = _bande.offsetMax = Vector2.zero;

        // Liserés dorés haut et bas
        foreach (var (min, max) in new[]
        {
            (new Vector2(0f, 0.97f), new Vector2(1f, 1f)),
            (new Vector2(0f, 0f),    new Vector2(1f, 0.03f)),
        })
        {
            var lis = new GameObject("Lisere");
            lis.transform.SetParent(bandeGO.transform, false);
            var img = lis.AddComponent<Image>();
            img.color = new Color(1f, 0.82f, 0.25f, 0.9f);
            img.raycastTarget = false;
            var r = lis.GetComponent<RectTransform>();
            r.anchorMin = min; r.anchorMax = max;
            r.offsetMin = r.offsetMax = Vector2.zero;
        }

        // Titre doré
        var titreGO = new GameObject("Titre");
        titreGO.transform.SetParent(bandeGO.transform, false);
        _titre = titreGO.AddComponent<TextMeshProUGUI>();
        _titre.fontSize  = 62f;
        _titre.fontStyle = FontStyles.Bold;
        _titre.color     = new Color(1f, 0.85f, 0.35f);
        _titre.alignment = TextAlignmentOptions.Center;
        _titre.raycastTarget = false;
        var tr = titreGO.GetComponent<RectTransform>();
        tr.anchorMin = new Vector2(0.05f, 0.42f); tr.anchorMax = new Vector2(0.95f, 0.92f);
        tr.offsetMin = tr.offsetMax = Vector2.zero;

        // Sous-titre
        var sousGO = new GameObject("SousTitre");
        sousGO.transform.SetParent(bandeGO.transform, false);
        _sousTitre = sousGO.AddComponent<TextMeshProUGUI>();
        _sousTitre.fontSize  = 28f;
        _sousTitre.color     = new Color(0.75f, 0.8f, 0.9f);
        _sousTitre.alignment = TextAlignmentOptions.Center;
        _sousTitre.raycastTarget = false;
        var sr = sousGO.GetComponent<RectTransform>();
        sr.anchorMin = new Vector2(0.05f, 0.1f); sr.anchorMax = new Vector2(0.95f, 0.42f);
        sr.offsetMin = sr.offsetMax = Vector2.zero;

        _groupe = bandeGO.AddComponent<CanvasGroup>();
        _groupe.alpha = 0f;
        _groupe.blocksRaycasts = false;
    }
}
