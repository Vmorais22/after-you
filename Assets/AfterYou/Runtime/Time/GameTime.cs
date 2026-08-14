using System;

namespace AfterYou.TimeSystem
{
    public enum DaySegment
    {
        Morning,
        Afternoon,
        Night
    }

    [Serializable]
    public struct GameTime
    {
        public int Day;
        public DaySegment Segment;
        [UnityEngine.Range(0, 23)] public int Hour;

        public GameTime(int day, DaySegment segment, int hour)
        {
            Day = day;
            Segment = segment;
            Hour = hour;
        }

        public override string ToString()
        {
            return $"Day {Day} - {Segment} ({Hour:00}:00)";
        }
    }
}
