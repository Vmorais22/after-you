using System;
using System.Collections.Generic;
using System.IO;
using AfterYou.Core;
using AfterYou.Events;
using UnityEngine;

namespace AfterYou.Save
{
    public interface ISaveStorage
    {
        bool Exists(string slot);
        string Read(string slot);
        void Write(string slot, string content);
    }

    public sealed class JsonFileSaveStorage : ISaveStorage
    {
        private readonly string directory;

        public JsonFileSaveStorage(string directory)
        {
            this.directory = directory;
        }

        public bool Exists(string slot)
        {
            return File.Exists(GetPath(slot));
        }

        public string Read(string slot)
        {
            return File.ReadAllText(GetPath(slot));
        }

        public void Write(string slot, string content)
        {
            Directory.CreateDirectory(directory);
            File.WriteAllText(GetPath(slot), content);
        }

        private string GetPath(string slot)
        {
            return Path.Combine(directory, $"{slot}.json");
        }
    }

    public sealed class SaveManager : GameServiceBehaviour
    {
        [SerializeField] private DayEndedChannelSO dayEnded;
        [SerializeField] private string autosaveSlot = "autosave";

        private ISaveStorage storage;

        public override void Initialize(ServiceRegistry services)
        {
            base.Initialize(services);
            storage = new JsonFileSaveStorage(Path.Combine(Application.persistentDataPath, "Saves"));
            if (dayEnded != null)
            {
                dayEnded.Raised += OnDayEnded;
            }
        }

        public override void Shutdown()
        {
            if (dayEnded != null)
            {
                dayEnded.Raised -= OnDayEnded;
            }
        }

        public void Save(string slot)
        {
            var envelope = new SaveEnvelope
            {
                Version = 1,
                SavedAtUtc = DateTime.UtcNow.ToString("O")
            };

            foreach (var participant in Services.GetSaveParticipants())
            {
                envelope.Entries.Add(new SaveEntry
                {
                    Key = participant.SaveKey,
                    Json = participant.CaptureJson()
                });
            }

            storage.Write(slot, JsonUtility.ToJson(envelope, true));
        }

        public bool Load(string slot)
        {
            if (!storage.Exists(slot))
            {
                return false;
            }

            var envelope = JsonUtility.FromJson<SaveEnvelope>(storage.Read(slot));
            var participants = Services.GetSaveParticipants();

            foreach (var entry in envelope.Entries)
            {
                for (var index = 0; index < participants.Count; index++)
                {
                    if (participants[index].SaveKey == entry.Key)
                    {
                        participants[index].RestoreJson(entry.Json);
                        break;
                    }
                }
            }

            return true;
        }

        private void OnDayEnded(int day)
        {
            Save(autosaveSlot);
        }

        [Serializable]
        private sealed class SaveEnvelope
        {
            public int Version;
            public string SavedAtUtc;
            public List<SaveEntry> Entries = new();
        }

        [Serializable]
        private sealed class SaveEntry
        {
            public string Key;
            public string Json;
        }
    }
}
