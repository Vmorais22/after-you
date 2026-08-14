using System;
using AfterYou.Core;
using AfterYou.Events;
using AfterYou.Game;
using UnityEngine;

namespace AfterYou.TimeSystem
{
    public interface ITimeService
    {
        GameTime Current { get; }
        void Advance();
    }

    public sealed class TimeManager : GameServiceBehaviour, ITimeService, ISaveParticipant
    {
        [SerializeField] private TimeConfigSO configuration;
        [SerializeField] private TimeChangedChannelSO timeChanged;
        [SerializeField] private DayEndedChannelSO dayEnded;

        public GameTime Current { get; private set; }
        public string SaveKey => "time";

        public override void Initialize(ServiceRegistry services)
        {
            base.Initialize(services);
            if (configuration == null)
            {
                throw new InvalidOperationException("TimeManager requires a TimeConfigSO.");
            }

            Current = new GameTime(
                1,
                configuration.StartingSegment,
                configuration.GetStartHour(configuration.StartingSegment));
            timeChanged?.Raise(Current);
        }

        public void Advance()
        {
            if (Current.Segment != DaySegment.Night)
            {
                var nextSegment = (DaySegment)((int)Current.Segment + 1);
                Current = new GameTime(
                    Current.Day,
                    nextSegment,
                    configuration.GetStartHour(nextSegment));
                timeChanged?.Raise(Current);
                return;
            }

            var completedDay = Current.Day;
            if (Current.Day >= configuration.TotalDays)
            {
                dayEnded?.Raise(completedDay);
                if (Services.TryGet<GameManager>(out var gameManager))
                {
                    gameManager.CompleteGame();
                }

                return;
            }

            Current = new GameTime(
                Current.Day + 1,
                DaySegment.Morning,
                configuration.GetStartHour(DaySegment.Morning));
            timeChanged?.Raise(Current);
            dayEnded?.Raise(completedDay);
        }

        public string CaptureJson()
        {
            return JsonUtility.ToJson(Current);
        }

        public void RestoreJson(string json)
        {
            Current = JsonUtility.FromJson<GameTime>(json);
            timeChanged?.Raise(Current);
        }
    }
}
