using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Marqueur d'objectif 3D façon Hitman : un losange lumineux qui flotte
/// au-dessus de la station correspondant à la mission active (CPU, clavier,
/// portail RAM, écran), avec la distance affichée en dessous.
/// Visible uniquement dans la MainScene. Singleton créé par GameState.
/// </summary>
public class ObjectiveMarker : MonoBehaviour
{
    public static ObjectiveMarker Instance { get; private set; }

    [Header("Réglages")]
    public float hauteur         = 2.2f;   // hauteur au-dessus de la cible
    public float amplitude       = 0.25f;  // flottement vertical
    public float vitesseRotation = 80f;    // °/s

    private GameObject  _marker;
    private Transform   _diamant;
    private TextMeshPro _label;
    private Transform   _cible;
    private Transform   _joueur;
    private Vector3     _base;
    private float       _prochaineMajDistance;

    public static void Ensure()
    {
        if (Instance == null)
        {
            var go = new GameObject("[ObjectiveMarker]");
            go.AddComponent<ObjectiveMarker>();
        }
    }

    public static void Refresh()
    {
        if (Instance != null) Instance.Retrouver();
    }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    void OnDestroy()
    {
        if (Instance == this) SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == GameState.I.mainSceneName)
        {
            ConstruireMarker();
            _joueur = null; // re-trouvé à la demande (nouvelle scène)
            Retrouver();
        }
        else if (_marker != null)
        {
            _marker.SetActive(false);
        }
    }

    // ── ciblage ───────────────────────────────────────────────────────────

    /// <summary>Pointe le marqueur vers la station de la mission active.</summary>
    void Retrouver()
    {
        if (_marker == null) return;

        _cible = null;
        var gs = GameState.I;
        var q  = gs.QueteActuelle();

        if (q != null && !q.complete && !gs.ToutesQuetesTerminees())
        {
            switch (q.kind)
            {
                case QuestKind.Visite:
                    // Vise la tour du Processeur (modèle 3D), sinon la zone.
                    var tour = GameObject.Find("Processeur");
                    if (tour != null) _cible = tour.transform;
                    else
                    {
                        var cpu = FindFirstObjectByType<CPUZone>();
                        if (cpu != null) _cible = cpu.transform;
                    }
                    break;

                case QuestKind.Declaration:
                case QuestKind.Question:
                case QuestKind.Saisie:
                    _cible = ClavierLePlusProche();
                    break;

                case QuestKind.Rangement:
                    var portail = FindFirstObjectByType<LoadSceneOnPlayerEnter>();
                    if (portail != null) _cible = portail.transform;
                    break;

                case QuestKind.Affichage:
                    var ecran = FindFirstObjectByType<ConsoleScreen>();
                    if (ecran != null) _cible = ecran.transform;
                    break;
            }
        }

        bool actif = _cible != null;
        _marker.SetActive(actif);
        if (actif)
        {
            _base = SommetDe(_cible);
            _marker.transform.position = _base;
        }
    }

    /// <summary>
    /// Point au-dessus du SOMMET RÉEL du modèle 3D (bounds de tous ses
    /// renderers), centré sur lui — et non sur le pivot de l'objet, qui peut
    /// être n'importe où dans le modèle.
    /// </summary>
    Vector3 SommetDe(Transform t)
    {
        var rends = t.GetComponentsInChildren<Renderer>();
        if (rends.Length == 0)
            return t.position + Vector3.up * hauteur;

        Bounds b = rends[0].bounds;
        for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
        return new Vector3(b.center.x, b.max.y + hauteur * 0.5f, b.center.z);
    }

    Transform ClavierLePlusProche()
    {
        var claviers = FindObjectsByType<KeyboardTerminal>(FindObjectsSortMode.None);
        if (claviers.Length == 0) return null;

        TrouverJoueur();
        Transform meilleur = claviers[0].transform;
        float meilleureDist = float.MaxValue;
        foreach (var k in claviers)
        {
            float d = _joueur != null ? Vector3.Distance(k.transform.position, _joueur.position) : 0f;
            if (d < meilleureDist) { meilleureDist = d; meilleur = k.transform; }
        }
        return meilleur;
    }

    void TrouverJoueur()
    {
        if (_joueur != null) return;
        var pg = GameObject.FindGameObjectWithTag("Player");
        if (pg != null) _joueur = pg.transform;
    }

    // ── animation ─────────────────────────────────────────────────────────

    void Update()
    {
        if (_marker == null || !_marker.activeSelf || _cible == null) return;

        // Flottement + rotation du losange
        float bob = Mathf.Sin(Time.time * 2.2f) * amplitude;
        _marker.transform.position = _base + Vector3.up * bob;
        if (_diamant != null)
            _diamant.Rotate(Vector3.up, vitesseRotation * Time.deltaTime, Space.World);

        // Distance + orientation du texte vers la caméra
        if (_label != null)
        {
            if (Time.time >= _prochaineMajDistance)
            {
                _prochaineMajDistance = Time.time + 0.25f;
                TrouverJoueur();
                if (_joueur != null)
                {
                    Vector3 a = _joueur.position, b = _cible.position;
                    a.y = 0f; b.y = 0f;
                    _label.text = $"{Vector3.Distance(a, b):0} m";
                }
            }
            if (Camera.main != null)
                _label.transform.rotation = Camera.main.transform.rotation;
        }
    }

    // ── construction ──────────────────────────────────────────────────────

    void ConstruireMarker()
    {
        if (_marker != null) return;

        _marker = new GameObject("Marker");
        _marker.transform.SetParent(transform, false);

        // Losange : cube incliné à 45° sur deux axes, émissif cyan
        var d = GameObject.CreatePrimitive(PrimitiveType.Cube);
        d.name = "Diamant";
        Destroy(d.GetComponent<Collider>()); // ne doit rien bloquer/déclencher
        d.transform.SetParent(_marker.transform, false);
        d.transform.localRotation = Quaternion.Euler(45f, 0f, 45f);
        d.transform.localScale    = new Vector3(0.45f, 0.45f, 0.45f);
        _diamant = d.transform;

        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var mat = new Material(shader);
        var cyan = new Color(0f, 0.85f, 1f);
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", cyan);
        else mat.color = cyan;
        if (mat.HasProperty("_EmissionColor"))
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", cyan * 2.2f);
        }
        d.GetComponent<Renderer>().material = mat;

        // Distance sous le losange
        var txtGO = new GameObject("Distance");
        txtGO.transform.SetParent(_marker.transform, false);
        txtGO.transform.localPosition = new Vector3(0f, -0.85f, 0f);
        _label = txtGO.AddComponent<TextMeshPro>();
        _label.fontSize  = 5f;
        _label.alignment = TextAlignmentOptions.Center;
        _label.color     = Color.white;
        _label.rectTransform.sizeDelta = new Vector2(4f, 1.2f);

        _marker.SetActive(false);
    }
}
