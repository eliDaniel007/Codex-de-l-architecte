using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Portail d'entrée de la RAM. L'ACCÈS EST CONTRÔLÉ : si la mission n'attend
/// pas le joueur dans la RAM, l'entrée est refusée avec un message qui indique
/// où aller. (Campagne finie = accès libre.)
/// </summary>
public class LoadSceneOnPlayerEnter : MonoBehaviour
{
    private float _prochainMessage; // anti-spam du message de refus

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var gs = GameState.I;
        string attendue = gs.StationAttendue();

        // Mauvaise station → accès refusé + redirection.
        if (attendue != "ram" && attendue != "")
        {
            if (Time.time >= _prochainMessage)
            {
                _prochainMessage = Time.time + 3f;
                AudioFX.Erreur();
                PromptUI.Show(gs.BriefingEnAttente()
                    ? "<color=#FF6B6B>RAM verrouillée !</color>  Va d'abord au <b>CPU</b> lire la prochaine ligne du programme."
                    : $"<color=#FF6B6B>RAM verrouillée !</color>  Tu dois aller vers <b>{GameState.NomStation(attendue)}</b>.");
            }
            return;
        }

        Debug.Log("Trigger avec : " + other.name);
        SceneManager.LoadScene("RAM");
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) PromptUI.Hide();
    }
}
