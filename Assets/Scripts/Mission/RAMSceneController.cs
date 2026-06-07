using UnityEngine;
using TMPro;

/// <summary>
/// Contrôleur pour la scène RAM.
/// Gère l'affichage des variables stockées dans les cases de la RAM.
/// </summary>
public class RAMSceneController : MonoBehaviour
{
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false; // On utilise la main de robot

        VisualiserContenuRAM();
    }

    void VisualiserContenuRAM()
    {
        var gs = GameState.I;
        if (!gs.ramFilled) return;

        // On cherche toutes les cases (boîtes) de la RAM
        var selectors = FindObjectsByType<RAMBoxSelector>(FindObjectsSortMode.None);
        
        if (selectors.Length > 0)
        {
            // On cherche une boîte qui n'est pas encore "marquée" ou on prend la première
            // Pour faire simple, on va marquer les boîtes utilisées pour ne pas les écraser
            RAMBoxSelector target = selectors[0];
            
            // On peut essayer de trouver une boîte qui a encore ses valeurs par défaut
            foreach(var s in selectors)
            {
                if (s.variableName == "nombre" && s.variableValue == "25")
                {
                    target = s;
                    break;
                }
            }
            
            target.variableName  = gs.ramVariable;
            target.variableValue = gs.ramValue;
            target.typeName      = gs.ramType;

            // On applique le matériau/couleur d'origine capturé lors du dépôt
            var rend = target.GetComponent<Renderer>();
            if (rend != null)
            {
                if (gs.ramMaterial != null)
                {
                    rend.sharedMaterial = gs.ramMaterial;
                }
                else
                {
                    // Fallback sur la couleur par type
                    rend.material.color = gs.ramColor;
                }
            }

            // On ajoute un petit texte au-dessus pour voir le contenu (plus discret)
            AjouterLabel(target.transform, $"<color=#569CD6>{gs.ramType}</color> {gs.ramVariable}\n<size=120%>{gs.ramValue}</size>");
            
            Debug.Log($"[RAMSceneController] Variable '{gs.ramVariable}' insérée dans la table RAM.");
        }
    }

    void AjouterLabel(Transform parent, string texte)
    {
        var go = new GameObject("RAM_Label");
        go.transform.SetParent(parent);
        go.transform.localPosition = new Vector3(0, 1.2f, 0);
        
        var tmp = go.AddComponent<TextMeshPro>();
        tmp.text = texte;
        tmp.fontSize = 4;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        
        // Face à la caméra
        go.AddComponent<LookAtCamera>();
    }
}

/// <summary>
/// Petit script utilitaire pour que le texte regarde la caméra.
/// </summary>
public class LookAtCamera : MonoBehaviour
{
    void LateUpdate()
    {
        if (Camera.main != null)
            transform.rotation = Camera.main.transform.rotation;
    }
}
