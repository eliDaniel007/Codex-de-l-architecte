using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Journal de mission (touche [J] dans la MainScene) : les 6 lignes du
/// programme avec leur état ([OK] / > en cours / [verrouillée]) et la
/// collection de badges (débloqués en doré, restants en gris).
/// Singleton créé par GameState.
/// </summary>
public class JournalMissions : MonoBehaviour
{
    public static JournalMissions Instance { get; private set; }

    /// <summary>Vrai quand le journal est affiché (le menu pause ignore alors Échap).</summary>
    public static bool EstOuvert => Instance != null && Instance._ouvert;

    private Canvas          _canvas;
    private GameObject      _panneau;
    private TextMeshProUGUI _colMissions, _colBadges;
    private bool            _ouvert;

    public static void Ensure()
    {
        if (Instance == null)
        {
            var go = new GameObject("[JournalMissions]");
            go.AddComponent<JournalMissions>();
        }
    }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildUI();
        SceneManager.sceneLoaded += (s, m) => { if (_ouvert) Fermer(); };
    }

    void Update()
    {
        // Uniquement dans la MainScene, hors pause / cinématique / rating.
        if (SceneManager.GetActiveScene().name != GameState.I.mainSceneName) return;
        if (EcranTitre.Visible || BriefingCinematic.EnCours || RatingScreen.Visible) return;

#if ENABLE_INPUT_SYSTEM
        bool j = Keyboard.current != null && Keyboard.current.jKey.wasPressedThisFrame;
        bool echap = _ouvert && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#else
        bool j = Input.GetKeyDown(KeyCode.J);
        bool echap = _ouvert && Input.GetKeyDown(KeyCode.Escape);
#endif
        if (j) { if (_ouvert) Fermer(); else Ouvrir(); }
        else if (echap) Fermer();
    }

    void Ouvrir()
    {
        Rafraichir();
        _panneau.SetActive(true);
        _ouvert = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
    }

    void Fermer()
    {
        _panneau.SetActive(false);
        _ouvert = false;
        if (SceneManager.GetActiveScene().name == GameState.I.mainSceneName)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
        }
    }

    void ExporterRapport()
    {
        string chemin = RapportEleve.Exporter();
        if (chemin != null)
        {
            AudioFX.Succes();
            // Chemin raccourci pour le toast (dossier parent + nom de fichier).
            string court = System.IO.Path.Combine(
                System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(chemin)) ?? "",
                System.IO.Path.GetFileName(chemin));
            NotificationsUI.Afficher("RAPPORT EXPORTÉ",
                $"Fichier créé : {court}", new Color(0.3f, 0.8f, 1f));
        }
        else
        {
            AudioFX.Erreur();
            NotificationsUI.Afficher("EXPORT IMPOSSIBLE",
                "Voir la console pour le détail.", new Color(1f, 0.45f, 0.45f));
        }
    }

    // ── contenu ───────────────────────────────────────────────────────────

    void Rafraichir()
    {
        var gs = GameState.I;

        // Colonne missions (+ meilleur score de chaque ligne terminée)
        var sb = new System.Text.StringBuilder();
        int mn = Mathf.FloorToInt(gs.TempsCampagne / 60f);
        int sec = Mathf.FloorToInt(gs.TempsCampagne % 60f);
        sb.AppendLine($"<size=130%><color=#00D9FF>LE PROGRAMME</color></size>  " +
                      $"<size=75%><color=#7A8699>{mn:0}:{sec:00} — {gs.nbErreurs} erreur{(gs.nbErreurs > 1 ? "s" : "")}" +
                      $"{(gs.modeZen ? " — mode Zen" : "")}</color></size>\n");
        for (int i = 0; i < gs.quests.Count; i++)
        {
            var q = gs.quests[i];
            if (q.complete)
            {
                sb.Append($"<color=#59C96A>[OK]</color>  <color=#AEB9CC>{q.titre}</color>");
                var stat = gs.StatLigne(i);
                if (stat.HasValue)
                    sb.Append($"   <size=75%><color=#FFD24F>{stat.Value.etoiles}/3</color>" +
                              $"<color=#7A8699> — {stat.Value.duree:0} s, {stat.Value.erreurs} err.</color></size>");
                sb.AppendLine();
            }
            else if (i == gs.questIndex && i <= gs.missionRevelee)
                sb.AppendLine($"<color=#00D9FF>></color>  <b>{q.titre}</b>\n      <size=80%><color=#6BBF59><i>{q.description}</i></color></size>");
            else
                sb.AppendLine("<color=#5A6473>[verrouillée]</color>");
            sb.AppendLine();
        }
        _colMissions.text = sb.ToString();

        // Colonne badges
        var sbB = new System.Text.StringBuilder();
        int obtenus = 0;
        foreach (var (id, _, _) in Badges.Tous) if (Badges.EstDebloque(id)) obtenus++;
        sbB.AppendLine($"<size=130%><color=#FFD24F>BADGES</color></size>  <size=90%>{obtenus}/{Badges.Tous.Length}</size>\n");
        foreach (var (id, titre, description) in Badges.Tous)
        {
            if (Badges.EstDebloque(id))
                sbB.AppendLine($"<color=#FFD24F><b>{titre}</b></color>\n<size=78%><color=#AEB9CC>{description}</color></size>\n");
            else
                sbB.AppendLine($"<color=#4A5261>???</color>\n<size=78%><color=#4A5261>{description}</color></size>\n");
        }
        _colBadges.text = sbB.ToString();
    }

    // ── UI ────────────────────────────────────────────────────────────────

    void BuildUI()
    {
        _canvas = gameObject.AddComponent<Canvas>();
        _canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 110;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        gameObject.AddComponent<GraphicRaycaster>();

        _panneau = new GameObject("Panneau");
        _panneau.transform.SetParent(transform, false);
        var fond = _panneau.AddComponent<Image>();
        fond.color = new Color(0f, 0.015f, 0.06f, 0.94f);
        var pr = _panneau.GetComponent<RectTransform>();
        pr.anchorMin = new Vector2(0.14f, 0.1f); pr.anchorMax = new Vector2(0.86f, 0.9f);
        pr.offsetMin = pr.offsetMax = Vector2.zero;

        Titre(_panneau.transform, "JOURNAL DE MISSION", new Vector2(0.05f, 0.9f), new Vector2(0.95f, 0.985f), 42f);
        Titre(_panneau.transform, "<size=55%><color=#7A8699>[J] ou [Échap] pour fermer</color></size>",
              new Vector2(0.04f, 0.015f), new Vector2(0.5f, 0.075f), 30f);

        // Bouton d'export du rapport (fichier texte pour le professeur)
        var btnGO = new GameObject("BtnExport");
        btnGO.transform.SetParent(_panneau.transform, false);
        btnGO.AddComponent<Image>().color = new Color(0.08f, 0.3f, 0.42f);
        btnGO.AddComponent<Button>().onClick.AddListener(ExporterRapport);
        var br = btnGO.GetComponent<RectTransform>();
        br.anchorMin = new Vector2(0.55f, 0.018f); br.anchorMax = new Vector2(0.95f, 0.078f);
        br.offsetMin = br.offsetMax = Vector2.zero;
        var btnTxt = new GameObject("Label");
        btnTxt.transform.SetParent(btnGO.transform, false);
        var tmpB = btnTxt.AddComponent<TextMeshProUGUI>();
        tmpB.text = "EXPORTER LE RAPPORT"; tmpB.fontSize = 24f; tmpB.fontStyle = FontStyles.Bold;
        tmpB.color = Color.white; tmpB.alignment = TextAlignmentOptions.Center;
        tmpB.raycastTarget = false;
        var brt = btnTxt.GetComponent<RectTransform>();
        brt.anchorMin = Vector2.zero; brt.anchorMax = Vector2.one;
        brt.offsetMin = brt.offsetMax = Vector2.zero;

        _colMissions = Colonne(new Vector2(0.045f, 0.09f), new Vector2(0.52f, 0.88f));
        _colBadges   = Colonne(new Vector2(0.55f,  0.09f), new Vector2(0.955f, 0.88f));

        _panneau.SetActive(false);
    }

    void Titre(Transform parent, string texte, Vector2 min, Vector2 max, float taille)
    {
        var go = new GameObject("Titre");
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = texte; tmp.fontSize = taille;
        tmp.color = Color.white; tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center; tmp.richText = true;
        tmp.raycastTarget = false;
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = min; r.anchorMax = max;
        r.offsetMin = r.offsetMax = Vector2.zero;
    }

    TextMeshProUGUI Colonne(Vector2 min, Vector2 max)
    {
        var go = new GameObject("Colonne");
        go.transform.SetParent(_panneau.transform, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.TopLeft; tmp.richText = true;
        tmp.raycastTarget = false;
        // Auto-dimensionnement : le texte rétrécit pour TOUJOURS tenir dans la colonne.
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = 10f;
        tmp.fontSizeMax = 24f;
        tmp.overflowMode = TextOverflowModes.Truncate;
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = min; r.anchorMax = max;
        r.offsetMin = r.offsetMax = Vector2.zero;
        return tmp;
    }
}
