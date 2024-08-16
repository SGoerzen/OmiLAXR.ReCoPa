using System.Linq;
using OmiLAXR.Pipelines.Filters;
using UnityEngine;

namespace OmiLAXR.Adapters.YOUR_ADAPTER_NAME
{
    [AddComponentMenu("OmiLAXR / 2) Filters / Example Filter (Adapter.YOUR_ADAPTER_NAME)")]
    public sealed class ExampleFilter : Filter
    {
        public override Object[] Pass(Object[] gos)
        {
            // pass all objects - you can filter here
            return gos.Select(go => go).ToArray();
        }
    }
}
