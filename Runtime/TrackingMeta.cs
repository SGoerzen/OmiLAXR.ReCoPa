using System;
using System.Collections.Generic;

namespace OmiLAXR.Modules.ReCoPa
{
    [Serializable]
    public class TrackingMeta : Dictionary<string, object>
    {
        public static readonly TrackingMeta Empty = new TrackingMeta();
    }
}