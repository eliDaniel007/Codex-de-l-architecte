using UnityEngine;

/// <summary>
/// Pose ce composant sur un GameObject de la MainScene (ex : un objet vide
/// "MissionConfig"). Glisse le prefab de la boîte (celui de la RAM) dans
/// 'boxPrefab' : c'est cette boîte que le joueur portera sur la tête.
/// </summary>
public class MissionConfig : MonoBehaviour
{
    [Header("Boîte portée")]
    [Tooltip("Prefab de la boîte (le modèle de la RAM). Glisse-le ici.")]
    public GameObject boxPrefab;

    [Tooltip("Échelle de la boîte portée (à ajuster selon le prefab).")]
    public float boxScale = 0.6f;

    void Awake()
    {
        var gs = GameState.I;
        gs.boxPrefab = boxPrefab;
        gs.boxScale  = boxScale;
    }
}
