using System;
using System.Collections.Generic;
using AfterYou.Core;
using AfterYou.Events;
using AfterYou.Narrative;
using AfterYou.TimeSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AfterYou.Characters
{
    public interface ICharacterService
    {
        bool IsAvailable(string characterId);
        void SetAvailable(string characterId, bool available);
        bool HasEmotion(string characterId, string emotionId);
        void SetEmotion(string characterId, string emotionId, bool active);
    }

    public sealed class CharacterManager : GameServiceBehaviour, ICharacterService, ISaveParticipant
    {
        [SerializeField] private TimeChangedChannelSO timeChanged;

        private readonly Dictionary<string, NpcController> npcs = new();
        private readonly Dictionary<string, Transform> locations = new();
        private readonly Dictionary<string, bool> availability = new();
        private readonly Dictionary<string, HashSet<string>> emotions = new();

        public string SaveKey => "characters";

        public override void Initialize(ServiceRegistry services)
        {
            base.Initialize(services);
            SceneManager.sceneLoaded += OnSceneLoaded;
            if (timeChanged != null)
            {
                timeChanged.Raised += ApplyRoutines;
            }

            RebuildSceneRegistry();
            if (Services.TryGet<ITimeService>(out var timeService))
            {
                ApplyRoutines(timeService.Current);
            }
        }

        public override void Shutdown()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (timeChanged != null)
            {
                timeChanged.Raised -= ApplyRoutines;
            }
        }

        public bool IsAvailable(string characterId)
        {
            return !availability.TryGetValue(characterId, out var value) || value;
        }

        public void SetAvailable(string characterId, bool available)
        {
            if (string.IsNullOrWhiteSpace(characterId))
            {
                return;
            }

            availability[characterId] = available;
            if (npcs.TryGetValue(characterId, out var npc))
            {
                npc.SetAvailable(available);
            }
        }

        public bool HasEmotion(string characterId, string emotionId)
        {
            return emotions.TryGetValue(characterId, out var tags) && tags.Contains(emotionId);
        }

        public void SetEmotion(string characterId, string emotionId, bool active)
        {
            if (string.IsNullOrWhiteSpace(characterId) || string.IsNullOrWhiteSpace(emotionId))
            {
                return;
            }

            if (!emotions.TryGetValue(characterId, out var tags))
            {
                tags = new HashSet<string>();
                emotions[characterId] = tags;
            }

            if (active)
            {
                tags.Add(emotionId);
            }
            else
            {
                tags.Remove(emotionId);
            }
        }

        public string CaptureJson()
        {
            var entries = new List<CharacterStateEntry>();
            foreach (var pair in availability)
            {
                entries.Add(new CharacterStateEntry
                {
                    Id = pair.Key,
                    Available = pair.Value,
                    EmotionTags = emotions.TryGetValue(pair.Key, out var tags)
                        ? new List<string>(tags)
                        : new List<string>()
                });
            }

            return JsonUtility.ToJson(new CharacterSaveData { Entries = entries });
        }

        public void RestoreJson(string json)
        {
            availability.Clear();
            emotions.Clear();
            var data = JsonUtility.FromJson<CharacterSaveData>(json);
            if (data?.Entries == null)
            {
                return;
            }

            foreach (var entry in data.Entries)
            {
                SetAvailable(entry.Id, entry.Available);
                emotions[entry.Id] = new HashSet<string>(entry.EmotionTags ?? new List<string>());
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            RebuildSceneRegistry();
            if (Services.TryGet<ITimeService>(out var timeService))
            {
                ApplyRoutines(timeService.Current);
            }
        }

        private void RebuildSceneRegistry()
        {
            npcs.Clear();
            locations.Clear();

            foreach (var npc in FindObjectsByType<NpcController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (npc.Definition != null && !string.IsNullOrWhiteSpace(npc.Definition.Id))
                {
                    npcs[npc.Definition.Id] = npc;
                    availability.TryAdd(npc.Definition.Id, true);
                    npc.SetAvailable(IsAvailable(npc.Definition.Id));
                    if (!emotions.ContainsKey(npc.Definition.Id))
                    {
                        emotions[npc.Definition.Id] =
                            new HashSet<string>(npc.Definition.InitialEmotionTags);
                    }
                }
            }

            foreach (var anchor in FindObjectsByType<LocationAnchor>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (!string.IsNullOrWhiteSpace(anchor.Id))
                {
                    locations[anchor.Id] = anchor.transform;
                }
            }
        }

        private void ApplyRoutines(GameTime time)
        {
            var story = Services.Get<IStoryState>();
            foreach (var npc in npcs.Values)
            {
                var routine = npc.Definition.Routine;
                if (routine != null &&
                    routine.TryGetSlot(time, story.GetFlag, out var slot) &&
                    locations.TryGetValue(slot.LocationId, out var destination))
                {
                    npc.ApplyRoutine(slot, destination);
                }
            }
        }

        [Serializable]
        private sealed class CharacterSaveData
        {
            public List<CharacterStateEntry> Entries = new();
        }

        [Serializable]
        private sealed class CharacterStateEntry
        {
            public string Id;
            public bool Available;
            public List<string> EmotionTags = new();
        }
    }
}
