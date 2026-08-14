using System;
using System.Collections.Generic;
using AfterYou.Core;
using UnityEngine;

namespace AfterYou.Narrative
{
    public interface IStoryState
    {
        bool GetFlag(string id);
        bool HasAbility(string id);
        bool WasEventCompleted(string id);
    }

    public sealed class StoryManager : GameServiceBehaviour, IStoryState, ISaveParticipant
    {
        [SerializeField] private List<string> startingAbilities = new();

        private readonly HashSet<string> flags = new();
        private readonly HashSet<string> abilities = new();
        private readonly HashSet<string> completedEvents = new();

        public string SaveKey => "story";

        public override void Initialize(ServiceRegistry services)
        {
            base.Initialize(services);
            abilities.UnionWith(startingAbilities);
        }

        public bool GetFlag(string id)
        {
            return flags.Contains(id);
        }

        public bool HasAbility(string id)
        {
            return abilities.Contains(id);
        }

        public bool WasEventCompleted(string id)
        {
            return completedEvents.Contains(id);
        }

        public void SetFlag(string id, bool value)
        {
            SetMembership(flags, id, value);
        }

        public void SetAbility(string id, bool value)
        {
            SetMembership(abilities, id, value);
        }

        public void MarkEventCompleted(string id)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                completedEvents.Add(id);
            }
        }

        public string CaptureJson()
        {
            return JsonUtility.ToJson(new StorySaveData(flags, abilities, completedEvents));
        }

        public void RestoreJson(string json)
        {
            var data = JsonUtility.FromJson<StorySaveData>(json);
            Replace(flags, data.Flags);
            Replace(abilities, data.Abilities);
            Replace(completedEvents, data.CompletedEvents);
        }

        private static void SetMembership(HashSet<string> set, string id, bool value)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return;
            }

            if (value)
            {
                set.Add(id);
            }
            else
            {
                set.Remove(id);
            }
        }

        private static void Replace(HashSet<string> target, IEnumerable<string> values)
        {
            target.Clear();
            if (values != null)
            {
                target.UnionWith(values);
            }
        }

        [Serializable]
        private sealed class StorySaveData
        {
            public List<string> Flags;
            public List<string> Abilities;
            public List<string> CompletedEvents;

            public StorySaveData(
                IEnumerable<string> flags,
                IEnumerable<string> abilities,
                IEnumerable<string> completedEvents)
            {
                Flags = new List<string>(flags);
                Abilities = new List<string>(abilities);
                CompletedEvents = new List<string>(completedEvents);
            }
        }
    }
}
