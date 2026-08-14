using System;
using AfterYou.Core;
using AfterYou.Events;
using UnityEngine;

namespace AfterYou.Dialogue
{
    public interface IDialogueService
    {
        DialogueDefinitionSO ActiveDialogue { get; }
        void StartDialogue(DialogueDefinitionSO dialogue);
        void EndDialogue();
    }

    public sealed class DialogueManager : GameServiceBehaviour, IDialogueService
    {
        [SerializeField] private StringEventChannelSO dialogueStarted;
        [SerializeField] private StringEventChannelSO dialogueEnded;

        public DialogueDefinitionSO ActiveDialogue { get; private set; }

        public void StartDialogue(DialogueDefinitionSO dialogue)
        {
            ActiveDialogue = dialogue != null
                ? dialogue
                : throw new ArgumentNullException(nameof(dialogue));
            dialogueStarted?.Raise(dialogue.Id);
        }

        public void EndDialogue()
        {
            if (ActiveDialogue == null)
            {
                return;
            }

            var id = ActiveDialogue.Id;
            ActiveDialogue = null;
            dialogueEnded?.Raise(id);
        }
    }
}
