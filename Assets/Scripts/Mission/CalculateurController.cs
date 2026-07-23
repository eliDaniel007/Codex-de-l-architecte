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

        // Thème ORANGE/AMBRE : l'UAL se distingue au premier coup d'œil de
        // l'unité de contrôle (thème cyan).
        var fond = new GameObject("Fond");
        fond.transform.SetParent(canvasGO.transform, false);
        var img = fond.AddComponent<Image>();
        img.color = new Color(0.09f, 0.05f, 0.01f, 0.98f); // brun sombre chaud
        var fr = fond.GetComponent<RectTransform>();
        fr.anchorMin = Vector2.zero; fr.anchorMax = Vector2.one;
        fr.offsetMin = fr.offsetMax = Vector2.zero;

        // Liseré orange en haut
        var lisere = new GameObject("Lisere");
        lisere.transform.SetParent(canvasGO.transform, false);
        var lImg = lisere.AddComponent<Image>();
        lImg.color = new Color(1f, 0.65f, 0.2f, 0.95f);
        var lr = lisere.GetComponent<RectTransform>();
        lr.anchorMin = new Vector2(0f, 0.965f); lr.anchorMax = new Vector2(1f, 1f);
        lr.offsetMin = lr.offsetMax = Vector2.zero;

        // GRANDE BANDE ORANGE à gauche avec « UAL » en vertical : impossible de
        // confondre avec l'unité de contrôle (bande cyan « CONTRÔLE »).
        var bande = new GameObject("BandeUnite");
        bande.transform.SetParent(canvasGO.transform, false);
        var bImg = bande.AddComponent<Image>();
        bImg.color = new Color(1f, 0.65f, 0.2f, 0.9f);
        var brr = bande.GetComponent<RectTransform>();
        brr.anchorMin = new Vector2(0f, 0f); brr.anchorMax = new Vector2(0.055f, 0.965f);
        brr.offsetMin = brr.offsetMax = Vector2.zero;

        var bandeTxtGO = new GameObject("BandeLabel");
        bandeTxtGO.transform.SetParent(bande.transform, false);
        var bandeTxt = bandeTxtGO.AddComponent<TextMeshProUGUI>();
        bandeTxt.text = "U A L";
        bandeTxt.fontSize = 70f; bandeTxt.fontStyle = FontStyles.Bold;
        bandeTxt.color = new Color(0.15f, 0.07f, 0f);
        bandeTxt.alignment = TextAlignmentOptions.Center;
        bandeTxt.raycastTarget = false;
        var btr = bandeTxtGO.GetComponent<RectTransform>();
        btr.anchorMin = new Vector2(0.5f, 0.5f); btr.anchorMax = new Vector2(0.5f, 0.5f);
        btr.sizeDelta = new Vector2(800f, 100f);
        bandeTxtGO.transform.localRotation = Quaternion.Euler(0f, 0f, 90f); // vertical

        // Filigrane « + » géant en fond (la signature du calcul)
        var filiGO = new GameObject("Filigrane");
        filiGO.transform.SetParent(canvasGO.transform, false);
        var fili = filiGO.AddComponent<TextMeshProUGUI>();
        fili.text = "+";
        fili.fontSize = 700f; fili.fontStyle = FontStyles.Bold;
        fili.color = new Color(1f, 0.65f, 0.2f, 0.05f);
        fili.alignment = TextAlignmentOptions.Center;
        fili.raycastTarget = false;
        var ftr = filiGO.GetComponent<RectTransform>();
        ftr.anchorMin = new Vector2(0.2f, 0.05f); ftr.anchorMax = new Vector2(1f, 0.9f);
        ftr.offsetMin = ftr.offsetMax = Vector2.zero;

        Txt("<color=#FFB84D>CPU — UNITÉ ARITHMÉTIQUE ET LOGIQUE</color>",
            new Vector2(0.05f, 0.84f), new Vector2(0.95f, 0.945f), 52f, Color.white, FontStyles.Bold, canvasGO.transform);
        Txt("<color=#8A7355>Je calcule. C'est l'unité de contrôle qui lit le programme.</color>",
            new Vector2(0.1f, 0.79f), new Vector2(0.9f, 0.845f), 24f, Color.white, FontStyles.Italic, canvasGO.transform);

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
            // L'UAL ne sait PAS où ira le résultat : elle affiche juste  ... + ...
            // (les pointillés se remplissent quand les valeurs arrivent).
            string vx = gs.missionEtape >= 2 ? $"<color=#00D9FF>{gs.cpuX}</color>" : "<color=#5A6473>...</color>";
            string vz = gs.missionEtape >= 3 ? $"<color=#00D9FF>{gs.cpuZ}</color>" : "<color=#5A6473>...</color>";
            op = $"{vx} + {vz}";
            if (gs.missionEtape >= 3)
                op += $"\n\n<color=#FFD27F>= {gs.cpuSomme}</color>";
        }
        else if (q != null && q.kind == QuestKind.ConditionIf)
        {
            // Le TEST : l'UAL évalue et rend un BOOLÉEN — c'est lui qui choisira la porte.
            op = $"<color=#00D9FF>{gs.cpuSomme}</color> > {GameState.SEUIL_IF}  →  " +
                 (gs.cpuIfVrai ? "<color=#59C96A>VRAI</color>" : "<color=#FF6B6B>FAUX</color>") + "\n\n" +
                 $"<color=#FFD27F>résultat :  {(gs.cpuIfVrai ? "true" : "false")}   (bool)</color>";
        }
        else if (q != null && q.kind == QuestKind.TantQue)
        {
            // while (somme >= 20) somme -= 20 — un tour par visite
            op = $"somme = <color=#00D9FF>{gs.cpuAvant}</color>\n\n" +
                 $"while (somme >= 20)  →  " +
                 (gs.cpuWhileVrai ? "<color=#59C96A>VRAI</color>" : "<color=#FF6B6B>FAUX</color>") + "\n\n" +
                 (gs.cpuWhileVrai
                    ? $"<color=#FFD27F>somme = {gs.cpuAvant} - 20 = {gs.cpuSomme}</color>"
                    : $"<color=#FFD27F>La boucle s'arrête ({gs.cpuToursWhile} tour{(gs.cpuToursWhile > 1 ? "s" : "")})</color>");
        }

        // Zones SÉPARÉES (le test en haut, le message en bas) + auto-dimensionnement :
        // les textes longs rétrécissent au lieu de déborder l'un sur l'autre.
        TxtAuto(op, new Vector2(0.1f, 0.46f), new Vector2(0.9f, 0.78f), 60f, 24f,
            Color.white, FontStyles.Bold, canvasGO.transform);

        if (!string.IsNullOrEmpty(_message))
            TxtAuto(_message, new Vector2(0.08f, 0.24f), new Vector2(0.92f, 0.44f), 30f, 16f,
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

    /// <summary>Texte à taille AUTO : il rétrécit pour tenir dans sa zone
    /// (plus de chevauchement entre le test et le message, ligne 7).</summary>
    void TxtAuto(string texte, Vector2 ancMin, Vector2 ancMax, float tailleMax, float tailleMin,
                 Color couleur, FontStyles style, Transform parent)
    {
        var go = new GameObject("TxtAuto");
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = texte; tmp.color = couleur;
        tmp.alignment = TextAlignmentOptions.Center; tmp.richText = true; tmp.fontStyle = style;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMax = tailleMax;
        tmp.fontSizeMin = tailleMin;
        tmp.overflowMode = TextOverflowModes.Truncate;
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
