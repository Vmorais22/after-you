using System;
using System.Collections.Generic;
using AfterYou.TimeSystem;
using UnityEngine;

namespace AfterYou.Characters
{
    [Serializable]
    public sealed class RoutineSlot
    {
        [Min(1)] public int Day = 1;
        public DaySegment Segment;
        public string LocationId;
        public string ActivityId;
        public string RequiredFlag;
    }

    [CreateAssetMenu(menuName = "After You/Characters/Routine Schedule")]
    public sealed class RoutineScheduleSO : ScriptableObject
    {
        [SerializeField] private List<RoutineSlot> slots = new();

        public bool TryGetSlot(GameTime time, Func<string, bool> flagResolver, out RoutineSlot result)
        {
            foreach (var slot in slots)
            {
                if (slot.Day != time.Day || slot.Segment != time.Segment)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(slot.RequiredFlag) && !flagResolver(slot.RequiredFlag))
                {
                    continue;
                }

                result = slot;
                return true;
            }

            result = null;
            return false;
        }
    }
}
