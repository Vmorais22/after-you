using System.Reflection;
using AfterYou.Characters;
using AfterYou.Core;
using AfterYou.TimeSystem;
using NUnit.Framework;
using UnityEngine;

namespace AfterYou.Tests
{
    public sealed class FoundationArchitectureTests
    {
        [Test]
        public void ServiceRegistry_ResolvesConcreteAndInterfaceContracts()
        {
            var registry = new ServiceRegistry();
            var service = new TestService();

            registry.Register(service);

            Assert.That(registry.Get<TestService>(), Is.SameAs(service));
            Assert.That(registry.Get<ITestContract>(), Is.SameAs(service));
            Assert.That(registry.GetSaveParticipants(), Has.Count.EqualTo(1));
        }

        [Test]
        public void RoutineSchedule_SelectsDataForCurrentPeriod()
        {
            var schedule = ScriptableObject.CreateInstance<RoutineScheduleSO>();
            var slotsField = typeof(RoutineScheduleSO).GetField(
                "slots",
                BindingFlags.Instance | BindingFlags.NonPublic);
            slotsField.SetValue(schedule, new System.Collections.Generic.List<RoutineSlot>
            {
                new()
                {
                    Day = 2,
                    Segment = DaySegment.Afternoon,
                    LocationId = "library",
                    ActivityId = "reading"
                }
            });

            var found = schedule.TryGetSlot(
                new GameTime(2, DaySegment.Afternoon, 14),
                _ => false,
                out var slot);

            Assert.That(found, Is.True);
            Assert.That(slot.LocationId, Is.EqualTo("library"));
            Object.DestroyImmediate(schedule);
        }

        private interface ITestContract
        {
        }

        private sealed class TestService : ITestContract, ISaveParticipant
        {
            public string SaveKey => "test";

            public string CaptureJson()
            {
                return "{}";
            }

            public void RestoreJson(string json)
            {
            }
        }
    }
}
