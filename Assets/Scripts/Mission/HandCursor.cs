using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Fait en sorte qu'une image UI (la main) suive le curseur de la souris.
/// </summary>
public class HandCursor : MonoBehaviour
{
    private RectTransform _rectTransform;
    private Canvas _canvas;

    void Start()
    {
        // Sécurité : On n'active la main que dans la scène RAM
        if (!UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.ToUpper().Contains("RAM"))
        {
            Cursor.visible = true;
            Destroy(gameObject);
            return;
        }

        _rectTransform = GetComponent<RectTransform>();
_canvas = GetComponentInParent<Canvas>();
        
        // Cache le curseur système
        Cursor.visible = false;
        
        // S'assure que l'image ne bloque pas les clics (Raycast Target = false)
        var img = GetComponent<Image>();
        if (img != null) img.raycastTarget = false;
        
        // La nouvelle main pointe vers le haut/milieu. 
        // On place le pivot sur le bout de l'index pour que le clic soit précis.
        _rectTransform.pivot = new Vector2(0.72f, 0.95f);
    }

    void Update()
    {
        Vector2 mousePos;
#if ENABLE_INPUT_SYSTEM
        if (UnityEngine.InputSystem.Mouse.current == null) return;
        mousePos = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
#else
        mousePos = Input.mousePosition;
#endif

        Vector2 movePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvas.transform as RectTransform,
            mousePos,
            _canvas.worldCamera,
            out movePos);

        transform.position = _canvas.transform.TransformPoint(movePos);
    }

    void OnDestroy()
    {
        // Réaffiche le curseur quand on quitte la scène
        Cursor.visible = true;
    }
}
