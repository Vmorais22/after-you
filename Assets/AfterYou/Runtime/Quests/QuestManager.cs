using System;
using System.Collections.Generic;
using AfterYou.Core;
using UnityEngine;

namespace AfterYou.Quests
{
    public interface IQuestService
    {
        QuestState GetState(string questId);
        void SetState(string questId, QuestState state);
    }

    public sealed class QuestManager : GameServiceBehaviour, IQuestService, ISaveParticipant
    {
        private readonly Dictionary<string, QuestState> states = new();

        public string SaveKey => "quests";

        public QuestState GetState(string questId)
        {
            return states.TryGetValue(questId, out var state) ? state : QuestState.Unavailable;
        }

        public void SetState(string questId, QuestState state)
        {
            if (!string.IsNullOrWhiteSpace(questId))
            {
                states[questId] = state;
            }
        }

        public string CaptureJson()
        {
            var entries = new List<QuestSaveEntry>();
            foreach (var pair in states)
            {
                entries.Add(new QuestSaveEntry { Id = pair.Key, State = pair.Value });
            }

            return JsonUtility.ToJson(new QuestSaveData { Entries = entries });
        }

        public void RestoreJson(string json)
        {
            states.Clear();
            var data = JsonUtility.FromJson<QuestSaveData>(json);
            if (data?.Entries == null)
            {
                return;
            }

            foreach (var entry in data.Entries)
            {
                states[entry.Id] = entry.State;
            }
        }

        [Serializable]
        private sealed class QuestSaveData
        {
            public List<QuestSaveEntry> Entries = new();
        }

        [Serializable]
        private sealed class QuestSaveEntry
        {
            public string Id;
            public QuestState State;
        }
    }
}
