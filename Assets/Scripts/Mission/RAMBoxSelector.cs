using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Une case de la RAM, cliquable. Les données et le visuel sont pilotés par
/// RAMSceneController ; ce script ne fait que relayer les clics au contrôleur.
///   • Clic sur une case PLEINE  → reprendre la variable.
///   • Clic sur une case VIDE (en portant une box) → y déposer la box.
/// </summary>
public class RAMBoxSelector : MonoBehaviour, IPointerClickHandler
{
    [Header("Données de la variable (pilotées par le contrôleur)")]
    public string variableName  = "";
    public string variableValue = "";
    public string typeName      = "int";

    [HideInInspector] public int cellIndex = -1;

    // Clic via EventSystem + PhysicsRaycaster (New Input System).
    public void OnPointerClick(PointerEventData eventData) => Notifier();

    // Filet de sécurité si l'ancien Input Manager est actif.
    private void OnMouseDown() => Notifier();

    private void Notifier()
    {
        if (RAMSceneController.Instance != null)
            RAMSceneController.Instance.OnCellClicked(this);
    }
}
