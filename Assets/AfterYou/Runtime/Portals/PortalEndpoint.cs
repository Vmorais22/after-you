using UnityEngine;

namespace AfterYou.Portals
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class PortalEndpoint : MonoBehaviour
    {
        [field: SerializeField] public PortalDefinitionSO Definition { get; private set; }
        [field: SerializeField] public string EndpointId { get; private set; }
        [field: SerializeField] public string DestinationEndpointId { get; private set; }
        [field: SerializeField] public Transform ArrivalPoint { get; private set; }

        public Vector3 ArrivalPosition =>
            ArrivalPoint != null ? ArrivalPoint.position : transform.position;
    }
}
