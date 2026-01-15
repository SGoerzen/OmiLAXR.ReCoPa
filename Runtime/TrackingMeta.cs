using System;

namespace OmiLAXR.ReCoPa
{
    [Serializable]
    public struct TrackingMeta
    {
        public bool isTracking;
        public bool isTrackingPaused;
        public bool isCalibrated;
        public string computerName;
        public string actorName;
        public string actorEmail;
        public string metaContext;       
        
        public static readonly TrackingMeta Empty = new TrackingMeta();
    }
}