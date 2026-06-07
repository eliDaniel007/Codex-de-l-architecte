using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Contrôleur de la scène RAM (multi-cellules).
///   • Clic sur une case VIDE en portant une box → on y dépose la variable.
///   • Clic sur une case PLEINE → on reprend la variable (retour à MainScene).
///   • [Échap] → retour sans rien faire.
/// Le contenu des cases est persistant via GameState.ramSlots.
/// </summary>
public class RAMSceneController : MonoBehaviour
{
    public static RAMSceneController Instance { get; private set; }

    private RAMBoxSelector[]  _cells;
    private TextMeshProUGUI   _handLabel;
    private TextMeshProUGUI   _feedback;
    private bool              _busy;
    private int               _lastClickFrame = -1;

    void Awake() { Instance = this; }
    void OnDestroy() { if (Instance == this) Instance = null; }

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        EnsureEventSystem();
        EnsurePhysicsRaycaster();

        CollectCells();
        GameState.I.EnsureRamSlots(_cells.Length);
        for (int i = 0; i < _cells.Length; i++) ApplyCellVisual(i);

        BuildUI();
        RefreshHandUI();
    }

    void Update()
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb != null && kb.escapeKey.wasPressedThisFrame) Retour();
#else
        if (Input.GetKeyDown(KeyCode.Escape)) Retour();
#endif
    }

    // ── cellules ──────────────────────────────────────────────────────────

    void CollectCells()
    {
        var found = FindObjectsByType<RAMBoxSelector>(FindObjectsSortMode.None);
        // Tri déterministe par position → index stable d'un chargement à l'autre.
        System.Array.Sort(found, (a, b) =>
        {
            Vector3 pa = a.transform.position, pb = b.transform.position;
            if (Mathf.Abs(pa.x - pb.x) > 0.01f) return pa.x.CompareTo(pb.x);
            if (Mathf.Abs(pa.z - pb.z) > 0.01f) return pa.z.CompareTo(pb.z);
            return pa.y.CompareTo(pb.y);
        });
        _cells = found;
        for (int i = 0; i < _cells.Length; i++) _cells[i].cellIndex = i;

        Debug.Log($"[RAMSceneController] {_cells.Length} cases détectées.");
    }

    public void OnCellClicked(RAMBoxSelector cell)
    {
        if (_busy) return;
        if (Time.frameCount == _lastClickFrame) return; // anti double-déclenchement
        _lastClickFrame = Time.frameCount;

        var gs = GameState.I;
        int i = cell.cellIndex;
        if (i < 0 || i >= gs.ramSlots.Count) return;
        var slot = gs.ramSlots[i];

        if (slot.filled)
        {
            // Reprendre cette variable → retour à MainScene avec la box en main.
            gs.PrendreDeCase(i);
            gs.ramJustVisited = true;
            _busy = true;
            Debug.Log($"[RAMSceneController] Reprise case {i + 1}.");
            SceneManager.LoadScene(gs.mainSceneName);
        }
        else if (gs.boxExists)
        {
            // Déposer la box portée dans cette case vide.
            gs.DeposerDansCase(i);
            gs.CompleterSiKind(QuestKind.Rangement); // valide la quête « stocker en RAM »
            ApplyCellVisual(i);
            RefreshHandUI();
            Feedback($"Déposé en case {i + 1}.");
            Debug.Log($"[RAMSceneController] Dépôt en case {i + 1}.");
        }
        else
        {
            Feedback("Case vide. Apporte une variable depuis le clavier pour la déposer.");
        }
    }

    void ApplyCellVisual(int i)
    {
        var cell = _cells[i];
        var slot = GameState.I.ramSlots[i];

        cell.variableName  = slot.filled ? slot.variable : "";
        cell.variableValue = slot.filled ? slot.value    : "";
        cell.typeName      = slot.filled ? slot.type     : "int";

        var rend = cell.GetComponent<Renderer>();
        if (rend != null)
            rend.material.color = slot.filled ? slot.color : new Color(0.18f, 0.20f, 0.26f, 1f);

        string label = slot.filled
            ? $"<color=#569CD6>{slot.type}</color> {slot.variable}\n<size=120%>{slot.value}</size>\n<size=55%>[case {i + 1}]</size>"
            : $"<color=#5A6473>vide</color>\n<size=60%>[case {i + 1}]</size>";
        SetLabel(cell.transform, label);
    }

    void SetLabel(Transform cellT, string texte)
    {
        var t = cellT.Find("RAM_Label");
        GameObject go;
        if (t == null)
        {
            go = new GameObject("RAM_Label");
            go.transform.SetParent(cellT, false);
            go.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            var tmp = go.AddComponent<TextMeshPro>();
            tmp.fontSize  = 4f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color     = Color.white;
            tmp.richText  = true;
            go.AddComponent<LookAtCamera>();
        }
        else go = t.gameObject;

        go.GetComponent<TextMeshPro>().text = texte;
    }

    // ── retour ────────────────────────────────────────────────────────────

    void Retour()
    {
        if (_busy) return;
        _busy = true;
        GameState.I.RetourSansDepot();      // si on tient encore une box, on la régénère en main
        GameState.I.ramJustVisited = true;
        SceneManager.LoadScene(GameState.I.mainSceneName);
    }

    // ── UI ────────────────────────────────────────────────────────────────

    void BuildUI()
    {
        var canvasGO = new GameObject("RAM_UI");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        Txt(canvasGO.transform, "MÉMOIRE RAM",
            new Vector2(0.1f, 0.9f), new Vector2(0.9f, 0.98f), 44, Color.white, FontStyles.Bold);

        Txt(canvasGO.transform,
            "Clique une case VIDE pour déposer la variable que tu portes  •  Clique une case PLEINE pour la reprendre  •  [Échap] retour",
            new Vector2(0.04f, 0.83f), new Vector2(0.96f, 0.89f), 22, new Color(0.7f, 0.75f, 0.85f), FontStyles.Normal);

        _feedback  = Txt(canvasGO.transform, "",
            new Vector2(0.04f, 0.11f), new Vector2(0.96f, 0.16f), 24, new Color(0.5f, 0.9f, 0.6f), FontStyles.Italic);

        _handLabel = Txt(canvasGO.transform, "",
            new Vector2(0.04f, 0.04f), new Vector2(0.96f, 0.1f), 26, new Color(1f, 0.85f, 0.4f), FontStyles.Bold);
    }

    void RefreshHandUI()
    {
        if (_handLabel == null) return;
        var gs = GameState.I;
        _handLabel.text = gs.boxExists
            ? $"En main : <color=#FFD27F>{gs.boxType} {gs.boxVariable} = {gs.boxValue}</color>  →  clique une case vide pour déposer"
            : "Mains vides  →  clique une case pleine pour reprendre une variable";
    }

    void Feedback(string msg)
    {
        if (_feedback == null) return;
        _feedback.text = msg;
        CancelInvoke(nameof(ClearFeedback));
        Invoke(nameof(ClearFeedback), 2.5f);
    }

    void ClearFeedback() { if (_feedback != null) _feedback.text = ""; }

    // ── helpers ───────────────────────────────────────────────────────────

    TextMeshProUGUI Txt(Transform parent, string texte, Vector2 ancMin, Vector2 ancMax,
                        float taille, Color couleur, FontStyles style)
    {
        var go = new GameObject("Txt");
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text          = texte;
        tmp.fontSize      = taille;
        tmp.color         = couleur;
        tmp.alignment     = TextAlignmentOptions.Center;
        tmp.richText      = true;
        tmp.fontStyle     = style;
        tmp.raycastTarget = false; // n'intercepte pas les clics destinés aux cases 3D
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = ancMin; r.anchorMax = ancMax;
        r.offsetMin = r.offsetMax = Vector2.zero;
        return tmp;
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

    static void EnsurePhysicsRaycaster()
    {
        var cam = Camera.main;
        if (cam == null) cam = FindFirstObjectByType<Camera>();
        if (cam != null && cam.GetComponent<PhysicsRaycaster>() == null)
            cam.gameObject.AddComponent<PhysicsRaycaster>();
    }
}

/// <summary>
/// Petit utilitaire : le texte regarde toujours la caméra.
/// </summary>
public class LookAtCamera : MonoBehaviour
{
    void LateUpdate()
    {
        if (Camera.main != null)
            transform.rotation = Camera.main.transform.rotation;
    }
}
