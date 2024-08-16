using OmiLAXR.Composers;
using OmiLAXR.xAPI.Composers;
using UnityEngine;

namespace OmiLAXR.Adapters.YOUR_ADAPTER_NAME
{
    [AddComponentMenu("OmiLAXR / 4) Composers / Example Composer (Adapter.YOUR_ADAPTER_NAME)")]
    public sealed class ExampleComposer : xApiStatementComposer<ExampleTrackingBehaviour>
    {
        protected override Author GetAuthor()
            => new Author("YOUR_NAME", "your@email.com");

        protected override void Compose(ExampleTrackingBehaviour tb)
        {
            tb.OnExampleEvent += (sender, targetComponent, randomValue) =>
            {
                var statement = actor.Does(xapi.generic.verbs.received)
                    .Activity(xapi.generic.activities.number)
                    .WithExtension(xapi.generic.extensions.activity.name(targetComponent.name))
                    .WithResult(xapi.generic.extensions.result.value(randomValue));
                SendStatement(statement);
            };
        }
    }
}
