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
        public string activeActorName;
        public string activeActorEmail;
        public string registrationId;
        public string[] endpoints;
        public string[] filters;
        public string[] actions;
        public string[] gestures;
        public float? heartRate;
        public float? fps;
        public string metaContext; 
        public static readonly TrackingMeta Empty = new TrackingMeta();
    }
}
