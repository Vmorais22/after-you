using System.Collections.Generic;
using UnityEngine;

namespace AfterYou.Characters
{
    [CreateAssetMenu(menuName = "After You/Characters/Character")]
    public sealed class CharacterDefinitionSO : ScriptableObject
    {
        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField] public string DisplayName { get; private set; }
        [field: SerializeField, TextArea] public string Description { get; private set; }
        [field: SerializeField] public Sprite Portrait { get; private set; }
        [field: SerializeField] public RoutineScheduleSO Routine { get; private set; }
        [field: SerializeField] public List<string> InitialEmotionTags { get; private set; } = new();
    }
}
