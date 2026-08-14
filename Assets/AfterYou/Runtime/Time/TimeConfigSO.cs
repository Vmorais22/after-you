using UnityEngine;

namespace AfterYou.TimeSystem
{
    [CreateAssetMenu(menuName = "After You/Time/Time Configuration")]
    public sealed class TimeConfigSO : ScriptableObject
    {
        [field: SerializeField, Min(1)] public int TotalDays { get; private set; } = 7;
        [field: SerializeField] public DaySegment StartingSegment { get; private set; } = DaySegment.Morning;
        [field: SerializeField, Range(0, 23)] public int MorningHour { get; private set; } = 8;
        [field: SerializeField, Range(0, 23)] public int AfternoonHour { get; private set; } = 14;
        [field: SerializeField, Range(0, 23)] public int NightHour { get; private set; } = 20;

        public int GetStartHour(DaySegment segment)
        {
            return segment switch
            {
                DaySegment.Morning => MorningHour,
                DaySegment.Afternoon => AfternoonHour,
                DaySegment.Night => NightHour,
                _ => MorningHour
            };
        }
    }
}
