using OmiLAXR.Modules.ReCoPa.Filters;
using OmiLAXR.Modules.ReCoPa.TrackingBehaviours;
using OmiLAXR.Pipelines;
using UnityEngine;

namespace OmiLAXR.Modules.ReCoPa
{
    [AddComponentMenu("OmiLAXR / Modules / ReCoPa / Learner Pipeline Extension")]
    public class LearnerPipelineExtension : PipelineExtension<LearnerPipeline>
    {
        protected override void Extend(LearnerPipeline pipeline)
        {
            Add(gameObject.AddComponent<ReCoPaFilter>());
            Add(gameObject.AddComponent<ReCoPaTrackingBehaviour>());
        }
    }
}