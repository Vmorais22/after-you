using System;
using System.Collections.Generic;
using AfterYou.Core;
using UnityEngine;

namespace AfterYou.Relationships
{
    public interface IRelationshipService
    {
        int GetValue(string sourceId, string targetId);
        void Change(string sourceId, string targetId, int delta);
    }

    public sealed class RelationshipManager : GameServiceBehaviour, IRelationshipService, ISaveParticipant
    {
        [SerializeField] private int minimumValue = -100;
        [SerializeField] private int maximumValue = 100;

        private readonly Dictionary<string, int> values = new();

        public string SaveKey => "relationships";

        public int GetValue(string sourceId, string targetId)
        {
            return values.TryGetValue(BuildKey(sourceId, targetId), out var value) ? value : 0;
        }

        public void Change(string sourceId, string targetId, int delta)
        {
            var key = BuildKey(sourceId, targetId);
            values[key] = Mathf.Clamp(GetValue(sourceId, targetId) + delta, minimumValue, maximumValue);
        }

        public string CaptureJson()
        {
            var entries = new List<RelationshipEntry>();
            foreach (var pair in values)
            {
                entries.Add(new RelationshipEntry { Key = pair.Key, Value = pair.Value });
            }

            return JsonUtility.ToJson(new RelationshipSaveData { Entries = entries });
        }

        public void RestoreJson(string json)
        {
            values.Clear();
            var data = JsonUtility.FromJson<RelationshipSaveData>(json);
            if (data?.Entries == null)
            {
                return;
            }

            foreach (var entry in data.Entries)
            {
                values[entry.Key] = entry.Value;
            }
        }

        private static string BuildKey(string sourceId, string targetId)
        {
            if (string.IsNullOrWhiteSpace(sourceId) || string.IsNullOrWhiteSpace(targetId))
            {
                throw new ArgumentException("Relationship identifiers cannot be empty.");
            }

            return $"{sourceId}::{targetId}";
        }

        [Serializable]
        private sealed class RelationshipSaveData
        {
            public List<RelationshipEntry> Entries = new();
        }

        [Serializable]
        private sealed class RelationshipEntry
        {
            public string Key;
            public int Value;
        }
    }
}
