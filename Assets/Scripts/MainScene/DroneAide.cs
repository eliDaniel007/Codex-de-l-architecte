using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// LE DRONE D'AIDE : un petit drone lumineux flotte près du point de départ.
/// S'approcher + [E] → panneau d'aide contextuel :
///   • l'explication PÉDAGOGIQUE de la ligne en cours (le concept),
///   • quoi faire maintenant (la consigne exacte de l'étape),
///   • un rappel des règles du monde (variables en RAM, valeurs sur la tête).
/// Auto-créé au chargement de la MainScene.
/// </summary>
public class DroneAide : MonoBehaviour
{
    const float DIST_AIDE = 3.5f;

    /// <summary>Vrai quand le panneau d'aide est ouvert (bloque le menu pause).</summary>
    public static bool Ouvert { get; private set; }

    private Transform  _drone;
    private Transform  _joueur;
    private Vector3    _base;
    private GameObject _panneau;
    private TextMeshProUGUI _contenu;
    private bool       _promptAffiche; // le « [E] AIDE » est à l'écran

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        SceneManager.sceneLoaded += (s, m) => CreerSiBesoin();
        CreerSiBesoin();
    }

    static void CreerSiBesoin()
    {
        if (SceneManager.GetActiveScene().name != GameState.I.mainSceneName) return;
        if (FindFirstObjectByType<DroneAide>() != null) return;

        var go = new GameObject("[DroneAide]");
        go.AddComponent<DroneAide>();
    }

    void Start()
    {
        var pg = GameObject.FindGameObjectWithTag("Player");
        if (pg != null) _joueur = pg.transform;

        ConstruireDrone();
        ConstruirePanneau();

        // Posé un peu à l'écart du point de départ (visible, mais pas dans les pattes).
        Vector3 pos = _joueur != null
            ? _joueur.position + _joueur.right * 8f + _joueur.forward * 5f
            : Vector3.zero;
        if (Physics.Raycast(pos + Vector3.up * 5f, Vector3.down, out var hit, 20f))
            pos = hit.point;
        _base = pos + Vector3.up * 1.9f;
        _drone.position = _base;
    }

    void OnDestroy() { Ouvert = false; }

    void Update()
    {
        if (_drone == null) return;

        // Flottement + rotation lente
        _drone.position = _base + Vector3.up * (Mathf.Sin(Time.time * 1.6f) * 0.18f);
        _drone.Rotate(Vector3.up, 35f * Time.deltaTime, Space.World);

        if (_joueur == null || EcranTitre.Visible || BriefingCinematic.EnCours) return;

        float dist = Vector3.Distance(_joueur.position, _drone.position);

        if (Ouvert)
        {
            if (AppuyeE() || AppuyeEchap() || dist > DIST_AIDE * 2f) Fermer();
            return;
        }

        if (dist < DIST_AIDE)
        {
            PromptUI.Show("[E]  <color=#00D9FF>AIDE</color>  —  demander conseil au drone");
            _promptAffiche = true;
            if (AppuyeE()) Ouvrir();
        }
        else if (_promptAffiche)
        {
            // On s'éloigne : le « [E] AIDE » disparaît (il restait affiché avant).
            PromptUI.Hide();
            _promptAffiche = false;
        }
    }

    // ── contenu de l'aide ─────────────────────────────────────────────────

    void Ouvrir()
    {
        PromptUI.Hide();
        var gs = GameState.I;
        var q  = gs.QueteActuelle();

        string titre, concept;
        if (q == null || gs.ToutesQuetesTerminees())
        {
            titre   = "Programme terminé !";
            concept = "Tu as exécuté tout le chapitre 1. Recommence pour améliorer ton score, " +
                      "ou explore la carte (un insecte se cache quelque part...).";
        }
        else if (gs.BriefingEnAttente())
        {
            titre   = "Briefing requis";
            concept = "Chaque ligne du programme t'est révélée par l'UNITÉ DE CONTRÔLE du CPU. " +
                      "Rends-toi au CPU pour lire la prochaine ligne.";
        }
        else
        {
            titre   = q.titre;
            concept = ExplicationDe(q.kind);
        }

        _contenu.text =
            $"<size=130%><color=#00D9FF>{titre}</color></size>\n\n" +
            $"<color=#FFD24F>LE CONCEPT</color>\n{concept}\n\n" +
            $"<color=#FFD24F>QUE FAIRE MAINTENANT ?</color>\n{gs.IndicationActuelle()}\n\n" +
            "<size=80%><color=#7A8699>Rappels : les VARIABLES (boîtes) vivent dans la RAM — " +
            "seules les VALEURS voyagent sur ta tête, colorées selon leur type. " +
            "Cliquer une variable mains vides = copier sa valeur. " +
            "Cliquer une variable avec une valeur en main = la ranger dedans (affectation). " +
            "Le CPU a deux moitiés : l'unité de CONTRÔLE (cyan) lit le programme, " +
            "l'unité ARITHMÉTIQUE (orange) calcule.</color></size>";

        _panneau.SetActive(true);
        Ouvert = true;
    }

    void Fermer()
    {
        _panneau.SetActive(false);
        Ouvert = false;
    }

    static string ExplicationDe(QuestKind kind)
    {
        switch (kind)
        {
            case QuestKind.DeclarationRam:
                return "DÉCLARER une variable, c'est réserver une case de mémoire avec un NOM, un TYPE et une VALEUR. " +
                       "int x = 4; crée la case « x », de type entier, contenant 4. Dans la RAM, clique la boîte du type voulu.";
            case QuestKind.LectureRam:
                return "Console.WriteLine(x) AFFICHE la valeur de x à l'écran. Lire une variable ne la détruit pas : " +
                       "tu emportes une COPIE de sa valeur, l'originale reste en mémoire.";
            case QuestKind.SaisieEcran:
                return "Console.ReadLine() attend que l'utilisateur tape quelque chose AU CLAVIER. " +
                       "Ce qui arrive est du TEXTE (string) SANS NOM : c'est en le rangeant dans y qu'il devient la valeur de y.";
            case QuestKind.Parse:
                return "Int32.Parse(y) CONVERTIT le texte de y en nombre entier : \"42\" (des caractères) devient 42 " +
                       "(un int avec lequel on peut calculer). C'est l'unité ARITHMÉTIQUE du CPU qui fait la conversion.";
            case QuestKind.Calcul:
                return "somme = x + z : l'unité arithmétique additionne les VALEURS qu'on lui apporte — elle affiche " +
                       "... + ... car elle ne sait pas d'où elles viennent ni où ira le résultat. C'est TOI qui ranges " +
                       "le résultat dans somme (déclarée d'abord pour réserver sa place).";
            default:
                return "Suis la consigne du HUD en haut à gauche — le losange lumineux te montre où aller.";
        }
    }

    // ── visuel du drone ───────────────────────────────────────────────────

    void ConstruireDrone()
    {
        var racine = new GameObject("DroneAide");
        _drone = racine.transform;

        var sh = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        Material Mat(Color c, float emission)
        {
            var m = new Material(sh);
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
            else m.color = c;
            if (emission > 0f && m.HasProperty("_EmissionColor"))
            {
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", c * emission);
            }
            return m;
        }

        // Corps
        var corps = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        corps.transform.SetParent(_drone, false);
        corps.transform.localScale = Vector3.one * 0.5f;
        Destroy(corps.GetComponent<Collider>());
        corps.GetComponent<Renderer>().material = Mat(new Color(0.85f, 0.88f, 0.95f), 0f);

        // Œil lumineux
        var oeil = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        oeil.transform.SetParent(_drone, false);
        oeil.transform.localPosition = new Vector3(0f, 0.02f, 0.21f);
        oeil.transform.localScale    = Vector3.one * 0.16f;
        Destroy(oeil.GetComponent<Collider>());
        oeil.GetComponent<Renderer>().material = Mat(new Color(0f, 0.85f, 1f), 2f);

        // Anneau
        var anneau = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        anneau.transform.SetParent(_drone, false);
        anneau.transform.localScale = new Vector3(0.75f, 0.02f, 0.75f);
        Destroy(anneau.GetComponent<Collider>());
        anneau.GetComponent<Renderer>().material = Mat(new Color(0f, 0.85f, 1f), 1.2f);

        // Petite lampe
        var lampeGO = new GameObject("Lampe");
        lampeGO.transform.SetParent(_drone, false);
        var lampe = lampeGO.AddComponent<Light>();
        lampe.type = LightType.Point;
        lampe.color = new Color(0f, 0.85f, 1f);
        lampe.range = 5f; lampe.intensity = 1.6f;

        // Étiquette « AIDE »
        var txtGO = new GameObject("Etiquette");
        txtGO.transform.SetParent(_drone, false);
        txtGO.transform.localPosition = Vector3.up * 0.65f;
        var tmp = txtGO.AddComponent<TextMeshPro>();
        tmp.text = "AIDE";
        tmp.fontSize = 3.2f; tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0f, 0.9f, 1f);
        tmp.outlineWidth = 0.22f; tmp.outlineColor = new Color32(0, 0, 0, 220);
        tmp.rectTransform.sizeDelta = new Vector2(4f, 1f);
        txtGO.AddComponent<LookAtCamera>();
    }

    // ── panneau UI ────────────────────────────────────────────────────────

    void ConstruirePanneau()
    {
        var canvasGO = new GameObject("[AideUI]");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 105;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        _panneau = new GameObject("Panneau");
        _panneau.transform.SetParent(canvasGO.transform, false);
        var fond = _panneau.AddComponent<Image>();
        fond.color = new Color(0f, 0.02f, 0.07f, 0.95f);
        fond.raycastTarget = false;
        var pr = _panneau.GetComponent<RectTransform>();
        pr.anchorMin = new Vector2(0.24f, 0.14f); pr.anchorMax = new Vector2(0.76f, 0.86f);
        pr.offsetMin = pr.offsetMax = Vector2.zero;

        // Liseré cyan à gauche
        var lis = new GameObject("Lisere");
        lis.transform.SetParent(_panneau.transform, false);
        var lImg = lis.AddComponent<Image>();
        lImg.color = new Color(0f, 0.85f, 1f);
        lImg.raycastTarget = false;
        var lr = lis.GetComponent<RectTransform>();
        lr.anchorMin = new Vector2(0f, 0f); lr.anchorMax = new Vector2(0.008f, 1f);
        lr.offsetMin = lr.offsetMax = Vector2.zero;

        var txtGO = new GameObject("Contenu");
        txtGO.transform.SetParent(_panneau.transform, false);
        _contenu = txtGO.AddComponent<TextMeshProUGUI>();
        _contenu.color = Color.white;
        _contenu.richText = true;
        _contenu.alignment = TextAlignmentOptions.TopLeft;
        _contenu.enableAutoSizing = true;
        _contenu.fontSizeMin = 14f; _contenu.fontSizeMax = 27f;
        _contenu.raycastTarget = false;
        var tr = txtGO.GetComponent<RectTransform>();
        tr.anchorMin = new Vector2(0.04f, 0.1f); tr.anchorMax = new Vector2(0.96f, 0.96f);
        tr.offsetMin = tr.offsetMax = Vector2.zero;

        var piedGO = new GameObject("Pied");
        piedGO.transform.SetParent(_panneau.transform, false);
        var pied = piedGO.AddComponent<TextMeshProUGUI>();
        pied.text = "<color=#7A8699>[E] ou [Échap] pour fermer</color>";
        pied.fontSize = 20f; pied.alignment = TextAlignmentOptions.Center;
        pied.raycastTarget = false;
        var fr = piedGO.GetComponent<RectTransform>();
        fr.anchorMin = new Vector2(0.1f, 0.015f); fr.anchorMax = new Vector2(0.9f, 0.09f);
        fr.offsetMin = fr.offsetMax = Vector2.zero;

        _panneau.SetActive(false);
    }

    static bool AppuyeE()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.E);
#endif
    }

    static bool AppuyeEchap()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.Escape);
#endif
    }
}
