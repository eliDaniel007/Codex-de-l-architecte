using UnityEngine;

/// <summary>
/// Attach to the Player GameObject.
/// Manages the single item the player can carry at a time.
/// Set HoldPoint to a child Transform positioned ~1 m in front of the camera.
/// </summary>
public class PlayerHolder : MonoBehaviour
{
    [Tooltip("Empty child GameObject positioned in front of the player where the held item floats.")]
    public Transform holdPoint;

    [Tooltip("Smooth-follow speed while carrying an item.")]
    public float followSpeed = 20f;

    public PickupItem HeldItem { get; private set; }

    // ── public API ─────────────────────────────────────────────────────────

    public bool TryPickup(PickupItem item)
    {
        if (HeldItem != null) return false;
        EnsureHoldPoint();
        HeldItem = item;
        item.OnPicked();
        return true;
    }

    private void EnsureHoldPoint()
    {
        if (holdPoint != null) return;
        
        var hp = new GameObject("HoldPoint");
        hp.transform.SetParent(transform);
        hp.transform.localPosition = new Vector3(0f, 1.2f, 1.5f);
        holdPoint = hp.transform;
    }

    /// <summary>Drop the held item at its current world position.</summary>
    public void Drop()
    {
        if (HeldItem == null) return;
        HeldItem.OnDropped();
        HeldItem = null;
    }

    /// <summary>Remove the held item without restoring it (it was consumed).</summary>
    public void ConsumeHeld()
    {
        HeldItem = null;
    }

    // ── Unity ──────────────────────────────────────────────────────────────

    private void Start()
    {
        PromptUI.EnsureInstance();
        EnsureHoldPoint();
    }

    private void LateUpdate()
    {
        if (HeldItem == null) return;

        // Animation de flottement (bobbing)
        float bob = Mathf.Sin(Time.time * 3f) * 0.05f;
        Vector3 targetPos = holdPoint.position + Vector3.up * bob;

        HeldItem.transform.position = Vector3.Lerp(
            HeldItem.transform.position,
            targetPos,
            Time.deltaTime * followSpeed);

        HeldItem.transform.rotation = Quaternion.Lerp(
            HeldItem.transform.rotation,
            holdPoint.rotation,
            Time.deltaTime * followSpeed);
    }
}
