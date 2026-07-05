using System.Collections;
using UnityEngine;

/// <summary>
/// Voix radio du « centre de contrôle » façon film d'espionnage :
/// grésillement (shrrr) → réplique vocale (fichiers Resources/Voix/m1..m7, fin)
/// → grésillement de fin. Annonce automatiquement chaque nouvelle mission.
/// Singleton créé par GameState ; survit aux changements de scène.
/// </summary>
public class VoiceOver : MonoBehaviour
{
    public static VoiceOver Instance { get; private set; }

    private AudioSource _src;
    private int         _derniereAnnonce = int.MinValue;

    public static void Ensure()
    {
        if (Instance == null)
        {
            var go = new GameObject("[VoiceOver]");
            go.AddComponent<VoiceOver>();
        }
    }

    /// <summary>Annonce la mission active (une seule fois par mission).</summary>
    public static void AnnoncerMission(float delai = 0.9f)
    {
        if (Instance != null) Instance.Annoncer(delai);
    }

    /// <summary>Réinitialise la radio (nouvelle campagne) et rejoue l'intro.</summary>
    public static void Reinitialiser()
    {
        if (Instance == null) return;
        Instance._derniereAnnonce = int.MinValue;
        Instance.JouerNomme("intro", 1.5f);
    }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _src = gameObject.AddComponent<AudioSource>();
        _src.spatialBlend = 0f; // radio dans l'oreille, pas dans le monde
        _src.volume       = 0.95f;
    }

    void Start()
    {
        if (GameState.I.BriefingEnAttente())
            JouerNomme("intro", 1.2f);     // « rends-toi au CPU pour tes objectifs »
        else
            Annoncer(1.2f);                // reprend l'annonce de la mission en cours
    }

    void Annoncer(float delai)
    {
        var gs = GameState.I;

        string clipName;
        int    idx;
        if (gs.ToutesQuetesTerminees()) { clipName = "fin"; idx = int.MaxValue; }
        else                            { idx = gs.questIndex; clipName = "m" + (idx + 1); }

        if (idx == _derniereAnnonce) return; // déjà annoncée
        _derniereAnnonce = idx;

        JouerNomme(clipName, delai);
    }

    void JouerNomme(string clipName, float delai)
    {
        var clip = Resources.Load<AudioClip>("Voix/" + clipName);
        if (clip == null)
        {
            Debug.LogWarning($"[VoiceOver] Clip 'Voix/{clipName}' introuvable.");
            return;
        }
        StopAllCoroutines();
        StartCoroutine(JouerRadio(clip, delai));
    }

    IEnumerator JouerRadio(AudioClip voix, float delai)
    {
        yield return new WaitForSeconds(delai);

        _src.Stop();
        _src.PlayOneShot(Gresillement(0.35f), 0.5f);  // shrrr d'ouverture
        yield return new WaitForSeconds(0.42f);

        _src.PlayOneShot(voix, 1f);
        yield return new WaitForSeconds(voix.length + 0.05f);

        _src.PlayOneShot(Gresillement(0.18f), 0.35f); // shrr de fermeture
    }

    /// <summary>Bruit blanc filtré : le grésillement radio.</summary>
    static AudioClip Gresillement(float duree)
    {
        const int sr = 44100;
        int n = (int)(duree * sr);
        var data = new float[n];
        var rnd  = new System.Random();
        float prev = 0f;

        for (int i = 0; i < n; i++)
        {
            float env = Mathf.Min(1f, i / (0.01f * sr)) *
                        Mathf.Min(1f, (n - i) / (0.05f * sr));
            float blanc = (float)(rnd.NextDouble() * 2.0 - 1.0);
            prev = 0.6f * prev + 0.4f * blanc;      // filtre passe-bas léger
            data[i] = prev * 0.55f * env;
        }

        var clip = AudioClip.Create("gresillement", n, 1, sr, false);
        clip.SetData(data, 0);
        return clip;
    }
}
