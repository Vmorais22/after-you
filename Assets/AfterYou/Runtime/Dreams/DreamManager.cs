using System.Collections.Generic;
using AfterYou.Core;
using AfterYou.Events;
using UnityEngine;

namespace AfterYou.Dreams
{
    public sealed class DreamManager : GameServiceBehaviour
    {
        [SerializeField] private DayEndedChannelSO dayEnded;
        [SerializeField] private List<DreamDefinitionSO> dreams = new();

        public DreamDefinitionSO PendingDream { get; private set; }

        public override void Initialize(ServiceRegistry services)
        {
            base.Initialize(services);
            if (dayEnded != null)
            {
                dayEnded.Raised += SelectDream;
            }
        }

        public override void Shutdown()
        {
            if (dayEnded != null)
            {
                dayEnded.Raised -= SelectDream;
            }
        }

        public void ClearPendingDream()
        {
            PendingDream = null;
        }

        private void SelectDream(int completedDay)
        {
            PendingDream = dreams.Find(dream => dream != null);
        }
    }
}
