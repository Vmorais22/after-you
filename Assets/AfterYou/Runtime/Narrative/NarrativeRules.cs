using System;
using AfterYou.TimeSystem;

namespace AfterYou.Narrative
{
    public enum RequirementKind
    {
        DayAtLeast,
        DayAtMost,
        HourIs,
        HourAtLeast,
        HourAtMost,
        TimeSegmentIs,
        FlagIs,
        AbilityUnlocked,
        CharacterAvailable,
        RelationshipAtLeast,
        QuestStateIs
    }

    public enum ConsequenceKind
    {
        SetFlag,
        UnlockAbility,
        SetCharacterAvailability,
        ChangeRelationship,
        StartQuest,
        CompleteQuest,
        FailQuest
    }

    [Serializable]
    public sealed class NarrativeRequirement
    {
        public RequirementKind Kind;
        public string Key;
        public string TargetId;
        public int IntValue;
        public bool BoolValue = true;
        public DaySegment Segment;
    }

    [Serializable]
    public sealed class NarrativeConsequence
    {
        public ConsequenceKind Kind;
        public string Key;
        public string TargetId;
        public int IntValue;
        public bool BoolValue = true;
    }
}
