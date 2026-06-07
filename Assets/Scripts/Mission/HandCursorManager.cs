using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gère l'affichage d'une main de robot qui suit le curseur.
/// </summary>
public class HandCursorManager : MonoBehaviour
{
    public Sprite handSprite;
    private GameObject _handGO;

    void Start()
    {
        CreateHandUI();
    }

    void CreateHandUI()
    {
        // Création du Canvas s'il n'existe pas
        var canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            var canvasGO = new GameObject("HandCanvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999; // Toujours au-dessus
            canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        _handGO = new GameObject("RobotHandCursor");
        _handGO.transform.SetParent(canvas.transform, false);
        
        var img = _handGO.AddComponent<Image>();
        img.sprite = handSprite;
        img.raycastTarget = false;

        var rt = _handGO.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(120, 120);
        
        // Ajout du script de mouvement
        _handGO.AddComponent<HandCursor>();
    }
}
