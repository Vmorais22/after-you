using AfterYou.Core;
using UnityEngine;

namespace AfterYou.Portals
{
    public sealed class PortalTraveller : MonoBehaviour
    {
        private PortalManager portalManager;

        private void Start()
        {
            portalManager = FindFirstObjectByType<PortalManager>();
            if (portalManager == null)
            {
                Debug.LogError("PortalTraveller requires a PortalManager in the scene.", this);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (portalManager != null && other.TryGetComponent<PortalEndpoint>(out var endpoint))
            {
                portalManager.TryTraverse(endpoint, transform);
            }
        }
    }
}
