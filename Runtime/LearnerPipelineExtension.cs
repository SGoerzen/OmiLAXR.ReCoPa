using System.Linq;
using OmiLAXR.ReCoPa.TrackingBehaviours;
using OmiLAXR.Pipelines;
using UnityEngine;

namespace OmiLAXR.ReCoPa
{
    [AddComponentMenu("OmiLAXR / Modules / ReCoPa / Learner Pipeline Extension")]
    public class LearnerPipelineExtension : PipelineExtension<LearnerPipeline>
    {
        protected override void Extend(LearnerPipeline pipeline)
        {
            Add(gameObject.AddComponent<ReCoPaTrackingBehaviour>());
        }
    }
}