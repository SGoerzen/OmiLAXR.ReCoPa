using OmiLAXR.Listeners;
using UnityEngine;

namespace OmiLAXR.Adapters.YOUR_ADAPTER_NAME
{
    [AddComponentMenu("OmiLAXR / 1) Listeners / Example Listener (Adapter.YOUR_ADAPTER_NAME)")]
    public sealed class ExampleListener : Listener
    {
        public override void StartListening()
        {
            var exampleComponents = FindObjectsOfType<ExampleComponent>();
            Found(exampleComponents);
        }
    }
}
