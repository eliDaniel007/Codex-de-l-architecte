using System.Collections;
using UnityEngine;

/// <summary>
/// Bande-son d'ouverture : nappe de synthé douce (progression Am – F – C – G)
/// composée par code — accords tenus + arpège léger + basse ronde.
/// Joue pendant l'écran titre ET la cinématique de briefing, puis s'éteint
/// en fondu quand le gameplay commence.
/// </summary>
public class MusiqueOuverture : MonoBehaviour
{
    static MusiqueOuverture _i;

    const int   SR     = 44100;
    const float VOLUME = 0.3f;

    AudioSource _src;
    Coroutine   _fondu;

    /// <summary>Lance (ou relance) la musique d'ouverture.</summary>
    public static void Jouer()
    {
        if (_i == null)
        {
            var go = new GameObject("[MusiqueOuverture]");
            _i = go.AddComponent<MusiqueOuverture>();
            DontDestroyOnLoad(go);
            _i._src = go.AddComponent<AudioSource>();
            _i._src.clip         = Composer();
            _i._src.loop         = true;
            _i._src.spatialBlend = 0f;
        }
        if (_i._fondu != null) { _i.StopCoroutine(_i._fondu); _i._fondu = null; }
        _i._src.volume = VOLUME;
        if (!_i._src.isPlaying) _i._src.Play();
    }

    /// <summary>Éteint la musique en fondu (le jeu commence).</summary>
    public static void ArreterEnFondu(float duree = 2.5f)
    {
        if (_i == null || _i._src == null || !_i._src.isPlaying) return;
        if (_i._fondu != null) _i.StopCoroutine(_i._fondu);
        _i._fondu = _i.StartCoroutine(_i.Fondu(duree));
    }

    IEnumerator Fondu(float duree)
    {
        float v0 = _src.volume;
        for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / duree)
        {
            _src.volume = Mathf.Lerp(v0, 0f, t);
            yield return null;
        }
        _src.Stop();
        _fondu = null;
    }

    // ── composition ───────────────────────────────────────────────────────

    /// <summary>Boucle de 4 mesures (8 s) : Am, F, C, G.</summary>
    static AudioClip Composer()
    {
        const float MESURE = 2f;                     // 2 s par accord
        int nMesure = (int)(MESURE * SR);
        int nTotal  = nMesure * 4;
        var data = new float[nTotal];

        // (basse, [nappe], [arpège]) par accord — fréquences en Hz
        var accords = new (float basse, float[] nappe, float[] arpege)[]
        {
            (110.00f, new[] { 220.00f, 261.63f, 329.63f }, new[] { 440.00f, 523.25f, 659.26f, 523.25f }), // Am
            ( 87.31f, new[] { 174.61f, 220.00f, 261.63f }, new[] { 349.23f, 440.00f, 523.25f, 440.00f }), // F
            (130.81f, new[] { 261.63f, 329.63f, 392.00f }, new[] { 523.25f, 659.26f, 783.99f, 659.26f }), // C
            ( 98.00f, new[] { 246.94f, 293.66f, 392.00f }, new[] { 493.88f, 587.33f, 783.99f, 587.33f }), // G
        };

        for (int m = 0; m < 4; m++)
        {
            var (basse, nappe, arpege) = accords[m];
            int debut = m * nMesure;

            for (int i = 0; i < nMesure; i++)
            {
                float t = i / (float)SR;          // temps dans la mesure
                float e = Enveloppe(t, MESURE);   // fondu doux d'accord

                // Nappe : 3 notes tenues, très douces
                float s = 0f;
                foreach (var f in nappe)
                    s += Mathf.Sin(2f * Mathf.PI * f * t) * 0.16f;

                // Basse ronde (fondamentale + octave discrète)
                s += Mathf.Sin(2f * Mathf.PI * basse * t) * 0.22f;
                s += Mathf.Sin(2f * Mathf.PI * basse * 2f * t) * 0.05f;
                s *= e;

                // Arpège : 8 croches par mesure, notes piquées qui s'éteignent
                int   pas    = (int)(t / 0.25f);
                float tNote  = t - pas * 0.25f;
                float fArp   = arpege[pas % arpege.Length];
                float eNote  = Mathf.Exp(-tNote * 7f) * Mathf.Min(1f, tNote / 0.01f);
                s += Mathf.Sin(2f * Mathf.PI * fArp * tNote) * 0.10f * eNote;

                data[debut + i] = s;
            }
        }

        var clip = AudioClip.Create("musique_ouverture", nTotal, 1, SR, false);
        clip.SetData(data, 0);
        return clip;
    }

    /// <summary>Attaque et retombée douces pour chaque accord (pas de clic).</summary>
    static float Enveloppe(float t, float duree)
    {
        float attaque = Mathf.Min(1f, t / 0.25f);
        float sortie  = Mathf.Min(1f, (duree - t) / 0.35f);
        return attaque * sortie;
    }
}
