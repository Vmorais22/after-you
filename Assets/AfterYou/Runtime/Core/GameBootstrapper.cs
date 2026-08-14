using System;
using System.Collections.Generic;
using UnityEngine;

namespace AfterYou.Core
{
    [DefaultExecutionOrder(-1000)]
    public sealed class GameBootstrapper : MonoBehaviour
    {
        [SerializeField] private bool persistAcrossScenes = true;

        private readonly List<IGameService> initializedServices = new();
        private ServiceRegistry services;

        private void Awake()
        {
            if (persistAcrossScenes)
            {
                DontDestroyOnLoad(gameObject);
            }

            services = new ServiceRegistry();
            var behaviours = GetComponentsInChildren<MonoBehaviour>(true);

            foreach (var behaviour in behaviours)
            {
                if (behaviour is IGameService service)
                {
                    services.Register(service);
                }
            }

            try
            {
                foreach (var behaviour in behaviours)
                {
                    if (behaviour is not IGameService service)
                    {
                        continue;
                    }

                    service.Initialize(services);
                    initializedServices.Add(service);
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
                enabled = false;
            }
        }

        private void OnDestroy()
        {
            for (var index = initializedServices.Count - 1; index >= 0; index--)
            {
                initializedServices[index].Shutdown();
            }

            initializedServices.Clear();
        }
    }
}
