using System;
using UnityEngine;

namespace AfterYou.Events
{
    public abstract class GameEventChannelSO<T> : ScriptableObject
    {
        public event Action<T> Raised;

        public void Raise(T payload)
        {
            Raised?.Invoke(payload);
        }
    }

    [CreateAssetMenu(menuName = "After You/Events/Void Event Channel")]
    public sealed class VoidEventChannelSO : ScriptableObject
    {
        public event Action Raised;

        public void Raise()
        {
            Raised?.Invoke();
        }
    }
}
