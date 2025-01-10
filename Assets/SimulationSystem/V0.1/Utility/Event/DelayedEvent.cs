using System;
using System.Threading.Tasks;
using UnityEngine.Events;

namespace SimulationSystem.V0._1.Utility.Event
{
    [Serializable]
    public class DelayedEvent
    {
        public UnityEvent delayedEvent;
        public float waitTimeInSeconds;
    
        public async void InvokeDelayedEvent()
        {
            await Task.Delay(TimeSpan.FromSeconds(waitTimeInSeconds));
            delayedEvent.Invoke();
        }
    }
}