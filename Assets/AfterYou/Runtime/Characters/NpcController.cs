using UnityEngine;

namespace AfterYou.Characters
{
    public sealed class NpcController : MonoBehaviour
    {
        [field: SerializeField] public CharacterDefinitionSO Definition { get; private set; }
        [field: SerializeField] public float MovementSpeed { get; private set; } = 3f;

        public string CurrentActivityId { get; private set; }
        public bool IsAvailable { get; private set; } = true;

        private Vector3 targetPosition;
        private bool hasTarget;

        public void ApplyRoutine(RoutineSlot slot, Transform destination)
        {
            CurrentActivityId = slot.ActivityId;
            if (destination == null)
            {
                hasTarget = false;
                return;
            }

            targetPosition = destination.position;
            hasTarget = true;
        }

        public void SetAvailable(bool value)
        {
            IsAvailable = value;
            gameObject.SetActive(value);
        }

        private void Update()
        {
            if (!hasTarget)
            {
                return;
            }

            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                MovementSpeed * Time.deltaTime);

            if ((transform.position - targetPosition).sqrMagnitude < 0.001f)
            {
                hasTarget = false;
            }
        }
    }
}
