using UnityEngine;

/// <summary>
/// L'humour de l'OS : quand le joueur fait une erreur, le système d'exploitation
/// commente avec une pointe d'ironie bienveillante (voix neuronale, clips
/// Resources/Voix/err1..err3). Anti-spam : au plus une réplique toutes les 25 s.
/// Singleton créé par GameState.
/// </summary>
public class HumourOS : MonoBehaviour
{
    public static HumourOS Instance { get; private set; }

    const float COOLDOWN = 25f;
    const int   NB_REPLIQUES_ERREUR = 3;

    private AudioSource _src;
    private float       _prochainOk;
    private int         _derniere = -1;

    public static void Ensure()
    {
        if (Instance == null)
        {
            var go = new GameObject("[HumourOS]");
            go.AddComponent<HumourOS>();
        }
    }

    /// <summary>Appelé quand le joueur commet une erreur : réplique ironique (parfois).</summary>
    public static void Erreur()
    {
        if (Instance == null) return;
        if (Time.unscaledTime < Instance._prochainOk) return;
        Instance._prochainOk = Time.unscaledTime + COOLDOWN;

        // Réplique différente de la précédente (variété)
        int choix;
        do { choix = Random.Range(1, NB_REPLIQUES_ERREUR + 1); }
        while (choix == Instance._derniere && NB_REPLIQUES_ERREUR > 1);
        Instance._derniere = choix;

        var clip = Resources.Load<AudioClip>("Voix/err" + choix);
        if (clip == null) return;
        Instance._src.PlayOneShot(clip, 0.9f);
    }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _src = gameObject.AddComponent<AudioSource>();
        _src.spatialBlend = 0f; // voix « dans l'oreille »
        _src.volume       = 0.9f;
    }
}
