using System.Collections.Generic;
using AfterYou.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AfterYou.Portals
{
    public sealed class PortalManager : GameServiceBehaviour
    {
        [SerializeField, Min(0.05f)] private float reentryCooldown = 0.35f;

        private readonly Dictionary<string, PortalEndpoint> endpoints = new();
        private float nextAllowedTime;

        public override void Initialize(ServiceRegistry services)
        {
            base.Initialize(services);
            SceneManager.sceneLoaded += OnSceneLoaded;
            RebuildRegistry();
        }

        public override void Shutdown()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        public bool TryTraverse(PortalEndpoint source, Transform traveller)
        {
            if (source == null || traveller == null || Time.unscaledTime < nextAllowedTime)
            {
                return false;
            }

            if (!endpoints.TryGetValue(source.DestinationEndpointId, out var destination))
            {
                Debug.LogWarning($"Portal destination '{source.DestinationEndpointId}' was not found.", source);
                return false;
            }

            nextAllowedTime = Time.unscaledTime + reentryCooldown;
            var offset = source.Definition != null ? (Vector3)source.Definition.ArrivalOffset : Vector3.zero;
            traveller.position = destination.ArrivalPosition + offset;
            return true;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            RebuildRegistry();
        }

        private void RebuildRegistry()
        {
            endpoints.Clear();
            foreach (var endpoint in FindObjectsByType<PortalEndpoint>(
                         FindObjectsInactive.Include,
                         FindObjectsSortMode.None))
            {
                if (!string.IsNullOrWhiteSpace(endpoint.EndpointId))
                {
                    endpoints[endpoint.EndpointId] = endpoint;
                }
            }
        }
    }
}
