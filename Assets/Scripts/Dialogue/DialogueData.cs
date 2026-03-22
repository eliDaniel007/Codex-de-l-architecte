using System;
using System.Collections.Generic;
using UnityEngine;

namespace Codex.Dialogue
{
    [CreateAssetMenu(fileName = "NewDialogue", menuName = "Codex/Dialogue Data")]
    public class DialogueData : ScriptableObject
    {
        public string dialogueId;
        public List<DialogueLine> lines = new List<DialogueLine>();
    }

    [Serializable]
    public class DialogueLine
    {
        public string speakerName;
        public Sprite portrait;
        [TextArea(2, 5)]
        public string text;
        [Range(0.01f, 0.1f)]
        public float letterDelay = 0.03f;
        public AudioClip voiceClip;
        public bool waitForInput = true;
    }
}
