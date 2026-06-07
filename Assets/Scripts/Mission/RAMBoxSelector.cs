using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

/// <summary>
/// Script à placer sur les boîtes 3D dans la scène RAM.
/// Permet de sélectionner une boîte et de retourner à la scène principale avec la boîte en main.
/// </summary>
public class RAMBoxSelector : MonoBehaviour, IPointerClickHandler
{
    [Header("Données de la variable")]
    public string variableName = "nombre";
    public string variableValue = "25";
    public string typeName = "int";

    [Header("Interaction")]
    [Tooltip("Charger la scène principale immédiatement après le clic ?")]
    public bool autoReturn = true;

    // Support legacy (pour être sûr)
    private void OnMouseDown()
    {
        SelectAndReturn();
    }

    // Support EventSystem (New Input System / UI)
    public void OnPointerClick(PointerEventData eventData)
    {
        SelectAndReturn();
    }

    private void SelectAndReturn()
    {
        StartCoroutine(PickupAnimation());
    }

    private System.Collections.IEnumerator PickupAnimation()
    {
        // 1. Désactiver le collider pour éviter les double-clics
        var col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // 2. Capturer la couleur et le matériau
        Color pickedColor = Color.cyan;
        Material pickedMat = null;
        var rend = GetComponent<Renderer>();
        if (rend != null)
        {
            pickedMat = rend.sharedMaterial;
            if (rend.material.HasProperty("_BaseColor")) pickedColor = rend.material.GetColor("_BaseColor");
            else if (rend.material.HasProperty("_Color")) pickedColor = rend.material.color;
        }

        // 3. Petite animation (Scale Pulse)
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;
        float duration = 0.5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // Courbe : monte puis descend (sinus)
            float curve = Mathf.Sin(t * Mathf.PI);
            transform.localScale = startScale * (1f + curve * 0.4f);
            transform.Rotate(Vector3.up * 360f * Time.deltaTime);
            yield return null;
        }

        // Si on prend la box qui était stockée en RAM, on libère le slot RAM
        if (GameState.I.ramFilled && variableName == GameState.I.ramVariable)
        {
            GameState.I.PrendreDansRam();
        }
        else
        {
            // Sinon (box statique de la scène), on l'enregistre normalement
            GameState.I.EnregistrerBox(variableName, variableValue, typeName, pickedColor, pickedMat);
        }
        
        // Indique qu'on veut que l'objet apparaisse directement dans la main au chargement
        GameState.I.spawnDansLaMain = true;

        Debug.Log($"[RAMBoxSelector] Sélection : {variableName} = {variableValue}. Retour vers {GameState.I.mainSceneName}");

        if (autoReturn)
        {
            SceneManager.LoadScene(GameState.I.mainSceneName);
        }
    }
}
