using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// FILE D'ATTENTE VOCALE globale : toutes les petites voix off (notifications,
/// bannières, humour de l'OS) passent par ici et sont lues UNE PAR UNE —
/// plus jamais deux voix en même temps. La radio des briefings (VoiceOver)
/// garde la priorité : la file attend qu'elle ait fini de parler.
/// </summary>
public class FileVoix : MonoBehaviour
{
    static FileVoix _i;

    readonly Queue<string> _file = new Queue<string>();
    AudioSource _src;
    bool _lecture;

    /// <summary>Vrai pendant qu'un clip de la file est en cours de lecture.</summary>
    public static bool EnLecture => _i != null && _i._lecture;

    /// <summary>Ajoute un clip (nom dans Resources/Voix) à la file de lecture.</summary>
    public static void Jouer(string nomClip)
    {
        if (string.IsNullOrEmpty(nomClip)) return;
        Ensure();
        _i._file.Enqueue(nomClip);
    }

    static void Ensure()
    {
        if (_i != null) return;
        var go = new GameObject("[FileVoix]");
        _i = go.AddComponent<FileVoix>();
        DontDestroyOnLoad(go);
        _i._src = go.AddComponent<AudioSource>();
        _i._src.spatialBlend = 0f;
        _i.StartCoroutine(_i.Derouler());
    }

    IEnumerator Derouler()
    {
        while (true)
        {
            // Rien à lire, ou la radio des briefings parle → on attend.
            if (_file.Count == 0 || VoiceOver.EnTrainDeParler)
            {
                yield return null;
                continue;
            }

            var clip = Resources.Load<AudioClip>("Voix/" + _file.Dequeue());
            if (clip == null) continue;

            _lecture = true;
            _src.PlayOneShot(clip, 0.9f);
            yield return new WaitForSecondsRealtime(clip.length + 0.25f);
            _lecture = false;
        }
    }
}
