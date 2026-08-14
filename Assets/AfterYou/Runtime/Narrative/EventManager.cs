using AfterYou.Characters;
using AfterYou.Core;
using AfterYou.Dialogue;
using AfterYou.Events;
using AfterYou.Quests;
using AfterYou.Relationships;
using AfterYou.TimeSystem;
using UnityEngine;

namespace AfterYou.Narrative
{
    public sealed class EventManager : GameServiceBehaviour
    {
        [SerializeField] private NarrativeEventChannelSO eventCompleted;

        public bool CanTrigger(NarrativeEventSO narrativeEvent)
        {
            if (narrativeEvent == null || string.IsNullOrWhiteSpace(narrativeEvent.Id))
            {
                return false;
            }

            var story = Services.Get<IStoryState>();
            if (!narrativeEvent.Repeatable && story.WasEventCompleted(narrativeEvent.Id))
            {
                return false;
            }

            foreach (var requirement in narrativeEvent.Requirements)
            {
                if (!Evaluate(requirement))
                {
                    return false;
                }
            }

            return true;
        }

        public bool TryTrigger(NarrativeEventSO narrativeEvent)
        {
            if (!CanTrigger(narrativeEvent))
            {
                return false;
            }

            foreach (var consequence in narrativeEvent.Consequences)
            {
                Apply(consequence);
            }

            Services.Get<StoryManager>().MarkEventCompleted(narrativeEvent.Id);
            if (narrativeEvent.Dialogue != null)
            {
                Services.Get<IDialogueService>().StartDialogue(narrativeEvent.Dialogue);
            }

            eventCompleted?.Raise(narrativeEvent);
            return true;
        }

        private bool Evaluate(NarrativeRequirement requirement)
        {
            var time = Services.Get<ITimeService>().Current;
            var story = Services.Get<IStoryState>();

            return requirement.Kind switch
            {
                RequirementKind.DayAtLeast => time.Day >= requirement.IntValue,
                RequirementKind.DayAtMost => time.Day <= requirement.IntValue,
                RequirementKind.HourIs => time.Hour == requirement.IntValue,
                RequirementKind.HourAtLeast => time.Hour >= requirement.IntValue,
                RequirementKind.HourAtMost => time.Hour <= requirement.IntValue,
                RequirementKind.TimeSegmentIs => time.Segment == requirement.Segment,
                RequirementKind.FlagIs => story.GetFlag(requirement.Key) == requirement.BoolValue,
                RequirementKind.AbilityUnlocked => story.HasAbility(requirement.Key),
                RequirementKind.CharacterAvailable =>
                    Services.Get<ICharacterService>().IsAvailable(requirement.Key) == requirement.BoolValue,
                RequirementKind.RelationshipAtLeast =>
                    Services.Get<IRelationshipService>().GetValue(requirement.Key, requirement.TargetId) >=
                    requirement.IntValue,
                RequirementKind.QuestStateIs =>
                    Services.Get<IQuestService>().GetState(requirement.Key) == (QuestState)requirement.IntValue,
                _ => false
            };
        }

        private void Apply(NarrativeConsequence consequence)
        {
            switch (consequence.Kind)
            {
                case ConsequenceKind.SetFlag:
                    Services.Get<StoryManager>().SetFlag(consequence.Key, consequence.BoolValue);
                    break;
                case ConsequenceKind.UnlockAbility:
                    Services.Get<StoryManager>().SetAbility(consequence.Key, true);
                    break;
                case ConsequenceKind.SetCharacterAvailability:
                    Services.Get<ICharacterService>().SetAvailable(consequence.Key, consequence.BoolValue);
                    break;
                case ConsequenceKind.ChangeRelationship:
                    Services.Get<IRelationshipService>().Change(
                        consequence.Key,
                        consequence.TargetId,
                        consequence.IntValue);
                    break;
                case ConsequenceKind.StartQuest:
                    Services.Get<IQuestService>().SetState(consequence.Key, QuestState.Active);
                    break;
                case ConsequenceKind.CompleteQuest:
                    Services.Get<IQuestService>().SetState(consequence.Key, QuestState.Completed);
                    break;
                case ConsequenceKind.FailQuest:
                    Services.Get<IQuestService>().SetState(consequence.Key, QuestState.Failed);
                    break;
            }
        }
    }
}
