using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace OmiLAXR.Adapters.YOUR_ADAPTER_NAME
{
    public class ExampleComponent : MonoBehaviour
    {
        public event Action<int> exampleEvent;
        private void Start()
        {
            // Something happen
            exampleEvent?.Invoke(Random.Range(0, 100));
        }
    }
}