using AfterYou.Narrative;
using AfterYou.TimeSystem;
using UnityEngine;

namespace AfterYou.Events
{
    [CreateAssetMenu(menuName = "After You/Events/Time Changed Channel")]
    public sealed class TimeChangedChannelSO : GameEventChannelSO<GameTime>
    {
    }

    [CreateAssetMenu(menuName = "After You/Events/Day Ended Channel")]
    public sealed class DayEndedChannelSO : GameEventChannelSO<int>
    {
    }

    [CreateAssetMenu(menuName = "After You/Events/Narrative Event Channel")]
    public sealed class NarrativeEventChannelSO : GameEventChannelSO<NarrativeEventSO>
    {
    }

    [CreateAssetMenu(menuName = "After You/Events/String Event Channel")]
    public sealed class StringEventChannelSO : GameEventChannelSO<string>
    {
    }
}
