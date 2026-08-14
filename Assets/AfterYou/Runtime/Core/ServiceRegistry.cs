using System;
using System.Collections.Generic;

namespace AfterYou.Core
{
    public sealed class ServiceRegistry
    {
        private readonly Dictionary<Type, object> services = new();
        private readonly List<ISaveParticipant> saveParticipants = new();

        public void Register(object service)
        {
            if (service is ISaveParticipant saveParticipant)
            {
                saveParticipants.Add(saveParticipant);
            }

            foreach (var contract in service.GetType().GetInterfaces())
            {
                if (contract == typeof(IGameService) || contract == typeof(ISaveParticipant))
                {
                    continue;
                }

                services.TryAdd(contract, service);
            }

            services[service.GetType()] = service;
        }

        public T Get<T>() where T : class
        {
            if (services.TryGetValue(typeof(T), out var service))
            {
                return (T)service;
            }

            throw new InvalidOperationException($"Service {typeof(T).Name} is not registered.");
        }

        public bool TryGet<T>(out T service) where T : class
        {
            if (services.TryGetValue(typeof(T), out var value))
            {
                service = (T)value;
                return true;
            }

            service = null;
            return false;
        }

        public IReadOnlyList<ISaveParticipant> GetSaveParticipants()
        {
            return saveParticipants;
        }
    }
}
