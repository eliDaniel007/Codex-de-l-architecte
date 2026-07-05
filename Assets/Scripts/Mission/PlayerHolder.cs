using UnityEngine;

/// <summary>
/// Attach to the Player GameObject.
/// Gère l'unique objet que le joueur transporte — posé sur sa TÊTE comme
/// enfant (position locale 0,0,0 du point de portage).
/// Le point de portage 'holdPoint' est placé au-dessus de la tête ; tu peux
/// créer un enfant et l'assigner pour ajuster précisément la position.
/// </summary>
public class PlayerHolder : MonoBehaviour
{
    [Tooltip("Point de portage sur la tête. Laissé vide → créé automatiquement.")]
    public Transform holdPoint;

    [Tooltip("Hauteur du point de portage au-dessus du pivot du joueur.")]
    public float hauteurTete = 1.95f;

    public PickupItem HeldItem { get; private set; }

    // ── public API ─────────────────────────────────────────────────────────

    public bool TryPickup(PickupItem item)
    {
        if (HeldItem != null) return false;
        EnsureHoldPoint();
        HeldItem = item;
        item.OnPicked();

        // Enfant du point de portage (sur la tête), position locale 0,0,0.
        item.transform.SetParent(holdPoint, false);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;
        return true;
    }

    private void EnsureHoldPoint()
    {
        if (holdPoint != null) return;

        var hp = new GameObject("HoldPoint");
        hp.transform.SetParent(transform);
        hp.transform.localPosition = new Vector3(0f, hauteurTete, 0f); // sur la tête
        hp.transform.localRotation = Quaternion.identity;
        holdPoint = hp.transform;
    }

    /// <summary>Lâche l'objet porté à sa position actuelle dans le monde.</summary>
    public void Drop()
    {
        if (HeldItem == null) return;
        HeldItem.transform.SetParent(null, true);
        HeldItem.OnDropped();
        HeldItem = null;
    }

    /// <summary>Retire l'objet porté sans le restituer (il a été consommé).</summary>
    public void ConsumeHeld()
    {
        if (HeldItem != null) HeldItem.transform.SetParent(null, true);
        HeldItem = null;
    }

    // ── Unity ──────────────────────────────────────────────────────────────

    private void Start()
    {
        PromptUI.EnsureInstance();
        EnsureHoldPoint();
    }
}
