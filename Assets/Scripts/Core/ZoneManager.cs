using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Codex.Core
{
    public class ZoneManager : MonoBehaviour
    {
        public static ZoneManager Instance { get; private set; }

        [Serializable]
        public class Zone
        {
            public string zoneId;
            public string zoneName;
            public List<string> puzzleIds = new List<string>();
            public Codex.Dialogue.DialogueData introDialogue;
            public Codex.Dialogue.DialogueData completionDialogue;
            public GameObject[] activateOnComplete;
            public UnityEvent onZoneComplete;
        }

        [SerializeField] List<Zone> zones = new List<Zone>();

        readonly HashSet<string> _completedPuzzles = new HashSet<string>();
        readonly HashSet<string> _completedZones = new HashSet<string>();
        readonly HashSet<string> _introPlayed = new HashSet<string>();

        public event Action<string> OnZoneCompleted;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void RegisterPuzzleComplete(string puzzleId)
        {
            if (string.IsNullOrEmpty(puzzleId)) return;
            _completedPuzzles.Add(puzzleId);
            CheckZoneCompletion();
        }

        public void TryPlayZoneIntro(string zoneId)
        {
            if (_introPlayed.Contains(zoneId)) return;
            var zone = zones.Find(z => z.zoneId == zoneId);
            if (zone == null || zone.introDialogue == null) return;
            if (Dialogue.DialogueUI.Instance == null) return;

            _introPlayed.Add(zoneId);
            Dialogue.DialogueUI.Instance.Play(zone.introDialogue);
        }

        void CheckZoneCompletion()
        {
            foreach (var zone in zones)
            {
                if (_completedZones.Contains(zone.zoneId)) continue;

                bool allDone = true;
                foreach (var pid in zone.puzzleIds)
                {
                    if (!_completedPuzzles.Contains(pid))
                    {
                        allDone = false;
                        break;
                    }
                }

                if (allDone)
                {
                    _completedZones.Add(zone.zoneId);
                    Debug.Log("[ZoneManager] Zone complete : " + zone.zoneName);

                    if (zone.activateOnComplete != null)
                    {
                        foreach (var go in zone.activateOnComplete)
                        {
                            if (go != null) go.SetActive(true);
                        }
                    }

                    zone.onZoneComplete?.Invoke();

                    if (zone.completionDialogue != null && Dialogue.DialogueUI.Instance != null)
                        Dialogue.DialogueUI.Instance.Play(zone.completionDialogue);

                    OnZoneCompleted?.Invoke(zone.zoneId);
                }
            }
        }

        public bool IsZoneComplete(string zoneId) => _completedZones.Contains(zoneId);
        public bool IsPuzzleComplete(string puzzleId) => _completedPuzzles.Contains(puzzleId);
        public int CompletedZoneCount => _completedZones.Count;
        public int TotalZoneCount => zones.Count;
    }
}
