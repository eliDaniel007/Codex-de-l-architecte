using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneOnPlayerEnter : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger avec : " + other.name);

        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene("RAM");
        }
    }
}