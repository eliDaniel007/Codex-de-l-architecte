using UnityEngine;

/// <summary>
/// Petits effets sonores générés par code (ondes sinus) — aucun asset requis.
///   AudioFX.MissionValidee() : jingle ascendant (mission accomplie)
///   AudioFX.Succes()         : bip positif (action réussie)
///   AudioFX.Depot()          : blip (dépôt RAM / écran)
///   AudioFX.Erreur()         : tonalité grave (erreur)
/// </summary>
public static class AudioFX
{
    private static AudioSource _src;

    private static AudioSource Src
    {
        get
        {
            if (_src == null)
            {
                var go = new GameObject("[AudioFX]");
                Object.DontDestroyOnLoad(go);
                _src = go.AddComponent<AudioSource>();
                _src.spatialBlend = 0f;   // 2D
                _src.volume       = 0.45f;
            }
            return _src;
        }
    }

    public static void MissionValidee() => Jouer(new[] { (659.25f, 0.10f), (830.61f, 0.10f), (987.77f, 0.20f) });
    public static void Succes()         => Jouer(new[] { (783.99f, 0.12f) });
    public static void Depot()          => Jouer(new[] { (523.25f, 0.06f), (659.25f, 0.08f) });
    public static void Erreur()         => Jouer(new[] { (196.00f, 0.22f) });

    private static void Jouer((float freq, float dur)[] notes)
    {
        try { Src.PlayOneShot(Generer(notes)); }
        catch { /* pas d'audio dispo : on ignore */ }
    }

    private static AudioClip Generer((float freq, float dur)[] notes)
    {
        const int sr = 44100;
        int total = 0;
        foreach (var n in notes) total += (int)(n.dur * sr);

        var data = new float[total];
        int pos = 0;
        foreach (var (freq, dur) in notes)
        {
            int len = (int)(dur * sr);
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / sr;
                // enveloppe : attaque 5 ms, relâche 50 ms (évite les clics)
                float env = Mathf.Min(1f, i / (0.005f * sr)) *
                            Mathf.Min(1f, (len - i) / (0.05f * sr));
                data[pos + i] = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.6f * env;
            }
            pos += len;
        }

        var clip = AudioClip.Create("fx", total, 1, sr, false);
        clip.SetData(data, 0);
        return clip;
    }
}
