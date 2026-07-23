using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

/// <summary>
/// Script à placer sur les boîtes 3D dans la scène RAM.
/// Permet de sélectionner une boîte et de retourner à la scène principale avec la boîte en main.
/// Si la boîte représente une case de la RAM (cellIndex >= 0, assigné par
/// RAMSceneController), le clic reprend la variable de cette case.
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

    [HideInInspector] public int cellIndex = -1; // case RAM représentée (-1 = boîte décor)

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
        // BRIEFING OBLIGATOIRE : tant que la prochaine ligne n'a pas été lue au
        // CPU, la RAM refuse toute interaction — même si le joueur connaît la suite.
        if (GameState.I.BriefingEnAttente())
        {
            RAMSceneController.Message(
                "Retourne au CPU (unité de contrôle) lire la prochaine ligne",
                false, 5f);
            AudioFX.Erreur();
            return;
        }

        // Boîte décor (int box, float box, ...) = CHOIX DU TYPE : cliquer ouvre
        // le formulaire de déclaration avec ce type présélectionné.
        if (cellIndex < 0)
        {
            string type = TypeDeDecor();
            if (type != null)
            {
                Debug.Log($"[RAMBoxSelector] Boîte de type '{type}' cliquée → formulaire.");
                RamDeclarationUI.OuvrirPourType(type);
            }
            return;
        }

        var gs = GameState.I;

        // Dialogue de confirmation déjà ouvert → on ignore les clics de boîtes.
        if (RAMSceneController.ConfirmationOuverte) return;

        // On PORTE une valeur nue → cliquer une variable = demander l'AFFECTATION.
        if (gs.boxExists && gs.boxEstValeur)
        {
            string valeur = gs.boxValue;
            RAMSceneController.DemanderConfirmation(
                $"Déposer la valeur  <color=#00D9FF>\"{valeur}\"</color>  dans  <b>{variableName}</b>  ?",
                () =>
                {
                    var (ok, message) = gs.DeposerValeurDansSlot(cellIndex);
                    RAMSceneController.Message(message, ok);
                    if (ok) StartCoroutine(RechargerApresDepot()); // sauvegarde déjà faite
                });
            return;
        }

        StartCoroutine(PickupAnimation());
    }

    System.Collections.IEnumerator RechargerApresDepot()
    {
        // Petit délai (son + message lisible), puis la scène RAM se recharge :
        // la boîte y affiche sa nouvelle valeur.
        yield return new WaitForSeconds(1.1f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>Type représenté par une boîte décor (texte affiché ou nom d'un parent).</summary>
    string TypeDeDecor()
    {
        // 1) Un texte de la boîte elle-même affiche le type (int, float, string, bool).
        foreach (var tmp in GetComponentsInChildren<TMPro.TMP_Text>(true))
        {
            string txt = tmp.text.Trim().ToLowerInvariant();
            if (txt == "int" || txt == "float" || txt == "string" || txt == "bool") return txt;
        }
        // 2) Sinon, un nom dans la chaîne des parents (ex : 'int box').
        for (var t = transform; t != null; t = t.parent)
        {
            string n = t.name.ToLowerInvariant();
            foreach (var k in new[] { "float", "string", "bool", "int" })
                if (n.Contains(k)) return k;
        }
        // 3) Dernier recours : les textes de la racine complète.
        foreach (var tmp in transform.root.GetComponentsInChildren<TMPro.TMP_Text>(true))
        {
            string txt = tmp.text.Trim().ToLowerInvariant();
            if (txt == "int" || txt == "float" || txt == "string" || txt == "bool") return txt;
        }
        return null;
    }

    private System.Collections.IEnumerator PickupAnimation()
    {
        // 1. Désactiver le collider pour éviter les double-clics
        var col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // 2. Petite animation (Scale Pulse)
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

        // On reprend la variable stockée dans cette case.
        GameState.I.PrendreSlot(cellIndex);

        // Indique qu'on veut que l'objet apparaisse directement dans la main au chargement
        GameState.I.spawnDansLaMain = true;

        Debug.Log($"[RAMBoxSelector] Sélection : {variableName} = {variableValue}. Retour vers {GameState.I.mainSceneName}");

        if (autoReturn)
        {
            SceneManager.LoadScene(GameState.I.mainSceneName);
        }
    }
}
