using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Globalization;
using System.Text.RegularExpressions;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Scène Clavier : terminal de saisie complet.
///
/// MODE NORMAL (par défaut) :
///   • Déclaration de tous les types : int, float, string, bool, char
///   • Évaluation d'expressions : int x = 2 + 3 * 4;
///   • Affichage : Console.WriteLine(valeur);
///
/// MODE QUESTION (si la quête active est une Question) :
///   • La question est affichée, le joueur tape sa réponse.
///   • Si correct, la quête se valide automatiquement.
/// </summary>
public class ClavierSceneController : MonoBehaviour
{
    [Header("UI Colors")]
    public Color colorType  = new Color(0.33f, 0.61f, 0.84f);
    public Color colorValue = new Color(0.71f, 0.81f, 0.66f);
    public Color colorError = new Color(1f, 0.4f, 0.4f);
    public Color colorOk    = new Color(0.45f, 0.85f, 0.45f);

    // ── état ──────────────────────────────────────────────────────────────
    private string          _buffer = "";
    private TextMeshProUGUI _ligneSaisie;
    private TextMeshProUGUI _feedback;
    private Quest           _question;   // quête-question active, sinon null
    private bool            _verrouille; // bloque la saisie pendant la transition

    void Start()
    {
        var q = GameState.I.QueteActuelle();
        _question = (q != null && q.kind == QuestKind.Question && !q.complete) ? q : null;

        ConstruireUI();
        ActualiserLigne();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null) Keyboard.current.onTextInput += OnCaractere;
#endif
    }

    void OnDestroy()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null) Keyboard.current.onTextInput -= OnCaractere;
#endif
    }

    void Update()
    {
        if (_verrouille) return;
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.backspaceKey.wasPressedThisFrame && _buffer.Length > 0)
        {
            _buffer = _buffer[..^1];
            ActualiserLigne();
        }
        if (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame)
            TenterValidation();
        if (kb.escapeKey.wasPressedThisFrame)
            Retour();
#else
        foreach (char c in Input.inputString) OnCaractere(c);
        if (Input.GetKeyDown(KeyCode.Backspace) && _buffer.Length > 0)
        { _buffer = _buffer[..^1]; ActualiserLigne(); }
        if (Input.GetKeyDown(KeyCode.Return)) TenterValidation();
        if (Input.GetKeyDown(KeyCode.Escape)) Retour();
#endif
    }

    void OnCaractere(char c)
    {
        if (_verrouille) return;
        if (char.IsControl(c)) return;
        _buffer += c;
        ActualiserLigne();
    }

    // ── validation ──────────────────────────────────────────────────────────

    void TenterValidation()
    {
        if (_verrouille) return;
        if (string.IsNullOrWhiteSpace(_buffer)) return;

        string input = _buffer.Trim();

        // MODE QUESTION : on valide la réponse à la quête active.
        if (_question != null)
        {
            if (ReponseCorrecte(input, _question))
            {
                GameState.I.CompleterQueteActuelle();
                Succes("Bonne réponse ! Objectif validé.");
                TerminerEtRevenir();
            }
            else
            {
                Erreur("Mauvaise réponse, réessaie.");
            }
            return;
        }

        // MODE NORMAL — 1) Déclaration : type nom = valeur;
        var mDecl = Regex.Match(input, @"^(int|float|string|bool|char)\s+([a-zA-Z_]\w*)\s*=\s*(.+?)\s*;?$");
        if (mDecl.Success)
        {
            ValiderDeclaration(mDecl.Groups[1].Value, mDecl.Groups[2].Value, mDecl.Groups[3].Value.Trim());
            return;
        }

        // 2) Affichage : Console.WriteLine(valeur);
        var mWL = Regex.Match(input, @"^Console\.WriteLine\((.+)\)\s*;?$");
        if (mWL.Success)
        {
            TraiterWriteLine(mWL.Groups[1].Value.Trim());
            return;
        }

        Erreur("Syntaxe : <type> nom = valeur;   ou   Console.WriteLine(valeur);");
    }

    /// <summary>Valide une déclaration de variable (tous types + expressions).</summary>
    void ValiderDeclaration(string type, string name, string rawVal)
    {
        string stored;
        Color  boxColor;

        switch (type)
        {
            case "int":
                if (!ExpressionEval.TryEval(rawVal, out double iv) || !EstEntier(iv))
                { Erreur("Valeur 'int' invalide (entier attendu)."); return; }
                stored   = ((long)System.Math.Round(iv)).ToString(CultureInfo.InvariantCulture);
                boxColor = new Color(0f, 0.5f, 1f);          // bleu
                break;

            case "float":
                if (!ExpressionEval.TryEval(rawVal, out double fv))
                { Erreur("Valeur 'float' invalide."); return; }
                stored   = fv.ToString(CultureInfo.InvariantCulture);
                boxColor = new Color(1f, 0.8f, 0f);          // jaune
                break;

            case "bool":
                string b = rawVal.Trim().ToLowerInvariant();
                if (b != "true" && b != "false")
                { Erreur("Valeur 'bool' : true ou false."); return; }
                stored   = b;
                boxColor = new Color(0.2f, 0.8f, 0.45f);     // vert
                break;

            case "char":
                string ch = rawVal.Trim().Trim('\'', '"');
                if (ch.Length != 1)
                { Erreur("Valeur 'char' : un seul caractère, ex 'A'."); return; }
                stored   = ch;
                boxColor = new Color(0.95f, 0.5f, 0.2f);     // orange
                break;

            default: // string
                stored   = rawVal.Trim().Trim('"');
                boxColor = new Color(0.8f, 0.2f, 0.8f);      // violet
                break;
        }

        GameState.I.EnregistrerBox(name, stored, type, boxColor);
        GameState.I.spawnDansLaMain   = true;   // boîte directement en main
        GameState.I.clavierJustVisited = true;
        GameState.I.CompleterSiKind(QuestKind.Declaration); // valide la quête si besoin

        Succes($"{type} {name} = {stored} ;");
        TerminerEtRevenir();
    }

    /// <summary>Console.WriteLine : évalue puis renvoie une box "sortie" à afficher.</summary>
    void TraiterWriteLine(string expr)
    {
        string val = expr.Trim();
        string stored;
        string type;

        if (ExpressionEval.TryEval(val, out double n))
        {
            if (EstEntier(n)) { stored = ((long)System.Math.Round(n)).ToString(CultureInfo.InvariantCulture); type = "int"; }
            else              { stored = n.ToString(CultureInfo.InvariantCulture);                            type = "float"; }
        }
        else
        {
            stored = val.Trim('"');
            type   = "string";
        }

        GameState.I.EnregistrerBox("sortie", stored, type, new Color(0f, 0.85f, 1f));
        GameState.I.spawnDansLaMain   = true;
        GameState.I.clavierJustVisited = true;

        Succes($"Console.WriteLine → {stored}");
        TerminerEtRevenir();
    }

    static bool EstEntier(double d) => System.Math.Abs(d - System.Math.Round(d)) < 1e-9;

    bool ReponseCorrecte(string input, Quest q)
    {
        string a = Normaliser(input, q.reponseInsensibleCasse);
        string b = Normaliser(q.reponseAttendue, q.reponseInsensibleCasse);
        if (a == b) return true;

        // tolérance numérique : "14" == "14.0" == "14,0"
        if (ExpressionEval.TryEval(input, out double na) &&
            ExpressionEval.TryEval(q.reponseAttendue, out double nb))
            return System.Math.Abs(na - nb) < 1e-9;

        return false;
    }

    static string Normaliser(string s, bool insensibleCasse)
    {
        if (s == null) return "";
        s = s.Trim().TrimEnd(';').Trim();
        s = Regex.Replace(s, @"\s+", " ");
        return insensibleCasse ? s.ToLowerInvariant() : s;
    }

    // ── transitions ───────────────────────────────────────────────────────

    void TerminerEtRevenir()
    {
        GameState.I.clavierJustVisited = true; // anti re-entrée immédiate au retour
        _verrouille = true;
        Invoke(nameof(RetourMain), 1.1f);
    }

    void RetourMain() => SceneManager.LoadScene(GameState.I.mainSceneName);

    void Retour()
    {
        GameState.I.RetourSansDepot();
        GameState.I.clavierJustVisited = true;
        SceneManager.LoadScene(GameState.I.mainSceneName);
    }

    // ── UI ────────────────────────────────────────────────────────────────

    void ConstruireUI()
    {
        var canvasGO = new GameObject("ClavierUI");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        EnsureEventSystem();

        var fondGO = new GameObject("Fond");
        fondGO.transform.SetParent(canvasGO.transform, false);
        var fondImg = fondGO.AddComponent<Image>();
        fondImg.color = new Color(0.02f, 0.02f, 0.05f, 0.98f);
        var fr = fondGO.GetComponent<RectTransform>();
        fr.anchorMin = Vector2.zero; fr.anchorMax = Vector2.one;
        fr.offsetMin = fr.offsetMax = Vector2.zero;

        // Titre
        string titre = _question != null ? "QUESTION DU CPU" : "TERMINAL DE DÉCLARATION";
        AjouterTexte(canvasGO.transform, titre,
            new Vector2(0.1f, 0.82f), new Vector2(0.9f, 0.92f), 42f, Color.white,
            TextAlignmentOptions.Center, FontStyles.Bold);

        // Bandeau question (mode question uniquement)
        if (_question != null)
        {
            AjouterTexte(canvasGO.transform, $"<color=#00D9FF>{_question.description}</color>",
                new Vector2(0.1f, 0.62f), new Vector2(0.9f, 0.78f), 34f, Color.white,
                TextAlignmentOptions.Center, FontStyles.Normal);
        }

        // Zone de saisie
        var zoneGO = new GameObject("ZoneSaisie");
        zoneGO.transform.SetParent(canvasGO.transform, false);
        var zr = zoneGO.AddComponent<RectTransform>();
        zr.anchorMin = new Vector2(0.15f, 0.45f); zr.anchorMax = new Vector2(0.85f, 0.55f);
        zr.offsetMin = zr.offsetMax = Vector2.zero;
        var zImg = zoneGO.AddComponent<Image>();
        zImg.color = new Color(1, 1, 1, 0.05f);

        var ligneGO = new GameObject("LigneSaisie");
        ligneGO.transform.SetParent(zoneGO.transform, false);
        _ligneSaisie = ligneGO.AddComponent<TextMeshProUGUI>();
        _ligneSaisie.fontSize  = 48f;
        _ligneSaisie.alignment = TextAlignmentOptions.Left;
        _ligneSaisie.fontStyle = FontStyles.Bold;
        _ligneSaisie.richText  = true;
        var lr = ligneGO.GetComponent<RectTransform>();
        lr.anchorMin = Vector2.zero; lr.anchorMax = Vector2.one;
        lr.offsetMin = new Vector2(20, 0); lr.offsetMax = new Vector2(-20, 0);

        // Feedback (erreur / succès)
        var fbGO = new GameObject("Feedback");
        fbGO.transform.SetParent(canvasGO.transform, false);
        _feedback = fbGO.AddComponent<TextMeshProUGUI>();
        _feedback.fontSize  = 26f;
        _feedback.alignment = TextAlignmentOptions.Center;
        _feedback.richText  = true;
        _feedback.enabled   = false;
        var fbr = fbGO.GetComponent<RectTransform>();
        fbr.anchorMin = new Vector2(0.1f, 0.37f); fbr.anchorMax = new Vector2(0.9f, 0.43f);
        fbr.offsetMin = fbr.offsetMax = Vector2.zero;

        // Aide
        string aide = _question != null
            ? "Tape ta réponse puis [Entrée]"
            : "Types : <color=#569CD6>int float string bool char</color>   •   ex : int x = 2 + 3 * 4;   •   Console.WriteLine(x);";
        AjouterTexte(canvasGO.transform, aide,
            new Vector2(0.08f, 0.25f), new Vector2(0.92f, 0.33f), 24f, new Color(0.7f, 0.7f, 0.7f),
            TextAlignmentOptions.Center, FontStyles.Normal);

        CreerBouton(canvasGO.transform, "VALIDER", new Vector2(0.39f, 0.1f), new Vector2(0.49f, 0.18f), new Color(0.1f, 0.4f, 0.1f), TenterValidation);
        CreerBouton(canvasGO.transform, "ANNULER", new Vector2(0.51f, 0.1f), new Vector2(0.61f, 0.18f), new Color(0.3f, 0.1f, 0.1f), Retour);
    }

    void ActualiserLigne()
    {
        if (_ligneSaisie == null) return;

        string display = _buffer;
        // Colorisation syntaxique légère (mode normal surtout)
        display = Regex.Replace(display, @"\b(int|float|string|bool|char)\b", "<color=#569CD6>$1</color>");
        display = Regex.Replace(display, @"\b(true|false)\b", "<color=#569CD6>$1</color>");
        display = Regex.Replace(display, @"\b(Console\.WriteLine)\b", "<color=#DCDCAA>$1</color>");
        display = Regex.Replace(display, @"([=;\(\)\+\-\*/])", "<color=#888>$1</color>");

        _ligneSaisie.text = display + "<color=#569CD6>|</color>";
    }

    void Erreur(string msg)
    {
        if (_feedback == null) return;
        _feedback.text    = $"<color=#FF6464>{msg}</color>";
        _feedback.enabled = true;
        CancelInvoke(nameof(CacherFeedback));
        Invoke(nameof(CacherFeedback), 3f);
    }

    void Succes(string msg)
    {
        if (_feedback == null) return;
        _feedback.text    = $"<color=#73D973>{msg}</color>";
        _feedback.enabled = true;
        CancelInvoke(nameof(CacherFeedback));
    }

    void CacherFeedback() { if (_feedback != null) _feedback.enabled = false; }

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

    void AjouterTexte(Transform parent, string texte, Vector2 ancMin, Vector2 ancMax, float taille,
                      Color couleur, TextAlignmentOptions align = TextAlignmentOptions.Center,
                      FontStyles style = FontStyles.Normal)
    {
        var go = new GameObject("Txt");
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = texte; tmp.fontSize = taille; tmp.color = couleur;
        tmp.alignment = align; tmp.richText = true; tmp.fontStyle = style;
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = ancMin; r.anchorMax = ancMax;
        r.offsetMin = r.offsetMax = Vector2.zero;
    }

    void CreerBouton(Transform parent, string label, Vector2 ancMin, Vector2 ancMax, Color couleur, System.Action onClick)
    {
        var go = new GameObject("Btn_" + label);
        go.transform.SetParent(parent, false);
        go.AddComponent<Image>().color = couleur;
        go.AddComponent<Button>().onClick.AddListener(() => onClick());
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = ancMin; r.anchorMax = ancMax;
        r.offsetMin = r.offsetMax = Vector2.zero;
        var txtGO = new GameObject("Label");
        txtGO.transform.SetParent(go.transform, false);
        var tmp = txtGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = 24f; tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        var tr = txtGO.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
        tr.offsetMin = tr.offsetMax = Vector2.zero;
    }
}

/// <summary>
/// Évaluateur d'expressions arithmétiques (+ - * / %, parenthèses, décimaux).
/// Descente récursive, sans dépendance externe.
/// </summary>
public static class ExpressionEval
{
    public static bool TryEval(string expr, out double result)
    {
        result = 0;
        if (string.IsNullOrWhiteSpace(expr)) return false;
        try
        {
            string s = expr.Replace(',', '.');
            int pos = 0;
            result = ParseExpr(s, ref pos);
            SkipSpaces(s, ref pos);
            return pos >= s.Length;            // toute la chaîne consommée
        }
        catch { return false; }
    }

    static double ParseExpr(string s, ref int pos)   // + -
    {
        double v = ParseTerm(s, ref pos);
        while (true)
        {
            SkipSpaces(s, ref pos);
            if (pos < s.Length && (s[pos] == '+' || s[pos] == '-'))
            {
                char op = s[pos++];
                double r = ParseTerm(s, ref pos);
                v = (op == '+') ? v + r : v - r;
            }
            else break;
        }
        return v;
    }

    static double ParseTerm(string s, ref int pos)   // * / %
    {
        double v = ParseFactor(s, ref pos);
        while (true)
        {
            SkipSpaces(s, ref pos);
            if (pos < s.Length && (s[pos] == '*' || s[pos] == '/' || s[pos] == '%'))
            {
                char op = s[pos++];
                double r = ParseFactor(s, ref pos);
                if      (op == '*') v *= r;
                else if (op == '/') v /= r;
                else                v %= r;
            }
            else break;
        }
        return v;
    }

    static double ParseFactor(string s, ref int pos)
    {
        SkipSpaces(s, ref pos);
        if (pos >= s.Length) throw new System.Exception("fin inattendue");

        if (s[pos] == '(')
        {
            pos++;
            double v = ParseExpr(s, ref pos);
            SkipSpaces(s, ref pos);
            if (pos >= s.Length || s[pos] != ')') throw new System.Exception("parenthèse");
            pos++;
            return v;
        }

        if (s[pos] == '+' || s[pos] == '-')          // unaire
        {
            char op = s[pos++];
            double v = ParseFactor(s, ref pos);
            return (op == '-') ? -v : v;
        }

        int start = pos;
        while (pos < s.Length && (char.IsDigit(s[pos]) || s[pos] == '.')) pos++;
        if (pos == start) throw new System.Exception("nombre attendu");
        return double.Parse(s.Substring(start, pos - start), CultureInfo.InvariantCulture);
    }

    static void SkipSpaces(string s, ref int pos)
    {
        while (pos < s.Length && char.IsWhiteSpace(s[pos])) pos++;
    }
}
