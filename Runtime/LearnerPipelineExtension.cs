using OmiLAXR.Pipelines;
using OmiLAXR.ReCoPa.Filters;
using UnityEngine;

namespace OmiLAXR.ReCoPa
{
    [AddComponentMenu("OmiLAXR / Modules / ReCoPa / Learner Pipeline Extension")]
    public class LearnerPipelineExtension : PipelineExtension<LearnerPipeline>
    {
        protected override void Extend(LearnerPipeline pipeline)
        {
            pipeline.Add(gameObject.AddComponent<ReCoPaFilter>());
        }
    }
}