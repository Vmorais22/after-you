using System;
using System.Collections.Generic;
using UnityEngine;

namespace AfterYou.Dialogue
{
    [Serializable]
    public sealed class DialogueLine
    {
        public string SpeakerId;
        [TextArea(2, 5)] public string Text;
        public string EmotionId;
    }

    [CreateAssetMenu(menuName = "After You/Dialogue/Dialogue")]
    public sealed class DialogueDefinitionSO : ScriptableObject
    {
        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField] public List<DialogueLine> Lines { get; private set; } = new();
    }
}
