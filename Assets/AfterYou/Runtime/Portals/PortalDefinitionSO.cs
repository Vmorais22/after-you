using UnityEngine;

namespace AfterYou.Portals
{
    [CreateAssetMenu(menuName = "After You/World/Portal")]
    public sealed class PortalDefinitionSO : ScriptableObject
    {
        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField] public string DisplayName { get; private set; }
        [field: SerializeField] public bool RequiresInteraction { get; private set; }
        [field: SerializeField] public Vector2 ArrivalOffset { get; private set; } = new(0f, -1f);
    }
}
