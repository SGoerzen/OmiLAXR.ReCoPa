using System;
using System.Linq;
using OmiLAXR.Filters;
using UnityEngine;
using Object = UnityEngine.Object;

namespace OmiLAXR.ReCoPa.Filters
{
    [AddComponentMenu("OmiLAXR / 2) Filters / ReCoPa Filter")]
    public sealed class ReCoPaFilter : Filter
    {
        public string[] gameObjects = Array.Empty<string>();
        public override Object[] Pass(Object[] gos)
        {
            return gos.Where(go => gameObjects.Contains(go.GetTrackingName())).ToArray();
        }
    }
}