using OmiLAXR.Pipelines;
using OmiLAXR.ReCoPa.Filters;
using UnityEngine;

namespace OmiLAXR.ReCoPa
{
    [AddComponentMenu("OmiLAXR / Modules / ReCoPa / Learner Pipeline Extension")]
    public class LearnerPipelineExtension : PipelineExtension<LearnerPipeline>
    {
        protected override PipelineComponent[] OnExtend()
        {
            return new PipelineComponent[]
            {
                gameObject.AddComponent<ReCoPaFilter>()
            };
        }
    }
}