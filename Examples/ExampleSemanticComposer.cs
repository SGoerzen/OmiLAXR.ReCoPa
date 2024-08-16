using System.Collections.Generic;
using System.Linq;
using OmiLAXR.Composers;
using OmiLAXR.Pipelines;
using OmiLAXR.xAPI.Composers;
using OmiLAXR.xAPI.Composers.HigherComposers;
using UnityEngine;

namespace OmiLAXR.Adapters.YOUR_ADAPTER_NAME
{
    [AddComponentMenu("OmiLAXR / 4.1) Higher Composers / Example Semantic Composer (Adapter.YOUR_ADAPTER_NAME)")]
    public sealed class ExampleSemanticComposer : xApiStatementSemanticComposer<LearnerPipeline>
    {
        protected override Author GetAuthor()
            => new Author("Sergej Görzen", "goerzen@cs.rwth-aachen.de");
        protected override Dictionary<string, MatchCondition<xApiStatement>> Conditions()
            => new Dictionary<string, MatchCondition<xApiStatement>>
            {
                { "clicked", new MatchCondition<xApiStatement>(1, stmt => stmt.MatchKeys("clicked", "mouse")) },
                { "moved", new MatchCondition<xApiStatement>(10, stmt => stmt.MatchKeys("moved", "mouse")) }
            };
        protected override void OnMatchAllConditions(Dictionary<string, IEnumerable<xApiStatement>> matchingStatements)
        {
            // Get clicked statement
            var clickedStatement = matchingStatements["clicked"].First();
            // Get first and last move statement
            var movedMatch = matchingStatements["moved"];
            var firstMoveStatement = movedMatch.First();
            var lastMoveStatement = movedMatch.Last();
            // Compose new statement
            actor.Does(xapi.generic.verbs.achieved)
                .Activity(xapi.generic.activities.goal)
                .WithExtension(xapi.generic.extensions.activity.name("dragged"));
        }
    }
}