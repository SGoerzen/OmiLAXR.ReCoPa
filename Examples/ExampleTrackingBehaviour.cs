using OmiLAXR.TrackingBehaviours;
using UnityEngine;

namespace OmiLAXR.Adapters.YOUR_ADAPTER_NAME
{
    [AddComponentMenu("OmiLAXR / 3) Tracking Behaviours / Example Tracking Behaviour (Adapter.YOUR_ADAPTER_NAME)")]
    public sealed class ExampleTrackingBehaviour : TrackingBehaviour
    {
        public event TrackingBehaviourAction<ExampleComponent, int> OnExampleEvent; 
        protected override void AfterFilteredObjects(Object[] objects)
        {
            var exampleComponents = Select<ExampleComponent>(objects);
            foreach (var e in exampleComponents)
            {
                e.exampleEvent += (randomValue) =>
                {
                    OnExampleEvent?.Invoke(this, e, randomValue);
                };
            }
        }
    }
}
