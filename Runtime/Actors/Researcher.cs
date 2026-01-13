using UnityEngine;

namespace OmiLAXR.ReCoPa
{
    [AddComponentMenu("OmiLAXR / Actors / Researcher")]
    public class Researcher : Actor
    {
        private void OnReset()
        {
            actorName = "Researcher";
            actorEmail = "anonymous@omilaxr.dev";
        }
    }
}