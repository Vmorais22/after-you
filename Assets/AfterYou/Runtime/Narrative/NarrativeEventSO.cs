using System.Collections.Generic;
using AfterYou.Dialogue;
using UnityEngine;

namespace AfterYou.Narrative
{
    [CreateAssetMenu(menuName = "After You/Narrative/Narrative Event")]
    public sealed class NarrativeEventSO : ScriptableObject
    {
        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField, TextArea] public string DesignerNotes { get; private set; }
        [field: SerializeField] public bool Repeatable { get; private set; }
        [field: SerializeField] public DialogueDefinitionSO Dialogue { get; private set; }
        [field: SerializeField] public List<NarrativeRequirement> Requirements { get; private set; } = new();
        [field: SerializeField] public List<NarrativeConsequence> Consequences { get; private set; } = new();
    }
}
