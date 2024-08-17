using UnityEngine;

namespace OmiLAXR.Adapters.YOUR_ADAPTER_NAME
{
    [AddComponentMenu("OmiLAXR / 0) Pipelines / Example Data Provider (Adapter.YOUR_ADAPTER_NAME)")]
    public class ExampleDataProvider : DataProvider
    {
        // Your individual settings
        [Tooltip("Statement Base URL")]
        public string statementIdUri = "https://your_individual_url.com/basepath/";
    }
}