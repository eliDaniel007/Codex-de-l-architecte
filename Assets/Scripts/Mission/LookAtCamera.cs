using UnityEngine;

/// <summary>
/// Petit utilitaire : l'objet (texte, panneau...) fait toujours face à la caméra.
/// </summary>
public class LookAtCamera : MonoBehaviour
{
    void LateUpdate()
    {
        if (Camera.main != null)
            transform.rotation = Camera.main.transform.rotation;
    }
}
