using System.Collections.Generic;
using AfterYou.Dialogue;
using AfterYou.Narrative;
using UnityEngine;

namespace AfterYou.Dreams
{
    [CreateAssetMenu(menuName = "After You/Dreams/Dream")]
    public sealed class DreamDefinitionSO : ScriptableObject
    {
        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField] public DialogueDefinitionSO Dialogue { get; private set; }
        [field: SerializeField] public List<NarrativeRequirement> Requirements { get; private set; } = new();
    }
}
