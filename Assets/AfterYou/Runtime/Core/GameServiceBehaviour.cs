using UnityEngine;

namespace AfterYou.Core
{
    public abstract class GameServiceBehaviour : MonoBehaviour, IGameService
    {
        protected ServiceRegistry Services { get; private set; }

        public virtual void Initialize(ServiceRegistry services)
        {
            Services = services;
        }

        public virtual void Shutdown()
        {
        }
    }
}
