using UnityEngine;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Pose ce script sur chaque cube ramassable (DataBox).
/// Le joueur doit avoir le tag "Player" et le composant PlayerHolder.
/// </summary>
public class PickupItem : MonoBehaviour
{
    [Header("Identité")]
    public string itemId      = "data_box";
    public string displayText = "42";
    public string codeLabel   = "int nombre = 42;";

    [Header("Portée")]
    public float interactRange = 3f;
    public float heldScaleFactor = 0.55f;

    // ── état ───────────────────────────────────────────────────────────────
    private PlayerHolder _holder;
    private Transform    _player;
    private Vector3      _originalScale;
    private bool         _isPicked;

    private TextMeshPro  _label;   // label flottant "[E] Ramasser"

    // ──────────────────────────────────────────────────────────────────────
    private bool _initialized;

    void Start()
    {
        EnsureInitialized();
    }

    private void EnsureInitialized()
    {
        if (_initialized) return;

        _originalScale = transform.localScale;

        var pg = GameObject.FindGameObjectWithTag("Player");
        if (pg != null)
        {
            _player = pg.transform;
            _holder = pg.GetComponent<PlayerHolder>();
            if (_holder == null) _holder = pg.GetComponentInChildren<PlayerHolder>();
        }

        // Label flottant au-dessus du cube
        if (_label == null)
        {
            _label = CreerLabel($"[E]  {codeLabel}");
            _label.enabled = false;
        }
        
        _initialized = true;
    }

    // ──────────────────────────────────────────────────────────────────────
    void Update()
    {
        if (_isPicked || _player == null) return;
        if (_label == null) return;

        float dist   = Vector3.Distance(transform.position, _player.position);
        bool proche  = dist <= interactRange;
        bool porteQc = _holder != null && _holder.HeldItem != null && _holder.HeldItem != this;

        _label.enabled = proche && !porteQc;

        if (_label.enabled && Camera.main != null)
            _label.transform.rotation = Camera.main.transform.rotation;

        if (proche && !porteQc && AppuyeE())
        {
            if (_holder != null) _holder.TryPickup(this);
        }

        // Poser l'objet qu'on porte
        if (_holder != null && _holder.HeldItem == this)
        {
            _label.enabled = false;
            if (AppuyeE()) _holder.Drop();
        }
    }

    // ── Appelé par PlayerHolder ────────────────────────────────────────────

    public void OnPicked()
    {
        EnsureInitialized();
        _isPicked = true;
        if (_label != null) _label.enabled = false;
        transform.localScale = _originalScale * heldScaleFactor;
        foreach (var c in GetComponentsInChildren<Collider>()) c.enabled = false;
    }

    public void OnDropped()
    {
        _isPicked = false;
        transform.localScale = _originalScale;
        foreach (var c in GetComponentsInChildren<Collider>()) c.enabled = true;
    }

    // ── Helper ─────────────────────────────────────────────────────────────

    TextMeshPro CreerLabel(string texte)
    {
        var go  = new GameObject("PickupLabel");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = Vector3.up * 1.5f;
        var tmp = go.AddComponent<TextMeshPro>();
        tmp.text      = texte;
        tmp.fontSize  = 2.5f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = Color.white;
        return tmp;
    }

    static bool AppuyeE()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.E);
#endif
    }
}
