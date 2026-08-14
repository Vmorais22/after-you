using System.Collections.Generic;
using AfterYou.Narrative;
using UnityEngine;

namespace AfterYou.Quests
{
    public enum QuestState
    {
        Unavailable,
        Active,
        Completed,
        Failed
    }

    [CreateAssetMenu(menuName = "After You/Quests/Quest")]
    public sealed class QuestDefinitionSO : ScriptableObject
    {
        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField] public string DisplayName { get; private set; }
        [field: SerializeField, TextArea] public string Description { get; private set; }
        [field: SerializeField] public List<NarrativeRequirement> Requirements { get; private set; } = new();
    }
}
