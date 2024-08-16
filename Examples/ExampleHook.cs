using System;
using OmiLAXR.xAPI.Composers;
using OmiLAXR.xAPI.Hooks;
using UnityEngine;

namespace OmiLAXR.Adapters.YOUR_ADAPTER_NAME
{
    [AddComponentMenu("OmiLAXR / 5) Hooks / Example Hook (Adapter.YOUR_ADAPTER_NAME)")]
    public sealed class ExampleHook : xApiStatementHook
    {
        private void Start()
        {
            
        }

        protected override xApiStatement AfterCompose(xApiStatement statement)
        {
            // change all clicked verbs by interacted
            if (statement.MatchKeys("clicked", "uiElement"))
            {
                DebugLog.xAPI.Print($"Matches keys 'clicked' and 'uiElement': {statement}");
                statement.Verb(xapi.virtualReality.verbs.interacted)
                    .ChangedBy("Sergej G.", "goerzen@informatik.rwth-aachen.de");
                DebugLog.xAPI.Print($"Exchanged 'clicked' by 'interacted': {statement}");
            }

            // Add additional extension
            if (statement.GetActivity().MatchesPath("virtualReality.activities.uiElement"))
            {
                DebugLog.xAPI.Print($"Matches path 'virtualReality.activities.uiElement': {statement}");
                statement.WithExtension(xapi.generic.extensions.activity.color("red"))
                    .ChangedBy("Sergej G.", "goerzen@informatik.rwth-aachen.de");
                DebugLog.xAPI.Print($"Added color 'red': {statement}");
            }
            
            // Exchange extension values
            if (statement.HasExtension("virtualReality.extensions.activity.uiElementValue"))
            {
                DebugLog.xAPI.Print($"Has extension 'virtualReality.extensions.activity.uiElementValue': {statement}");
                statement.WithExtension(xapi.virtualReality.extensions.activity.uiElementValue("exchanged"))
                    .ChangedBy("Sergej G.", "goerzen@informatik.rwth-aachen.de");
                DebugLog.xAPI.Print($"Exchanged 'virtualReality.extensions.activity.uiElementValue' to 'exchanged': {statement}");
            }
            
            // Exchange extension values
            if (statement.HasExtension("virtualReality.extensions.activity.uiElementMinValue"))
            {
                DebugLog.xAPI.Print($"Has extension 'virtualReality.extensions.activity.uiElementMinValue': {statement}");
                statement.DropExtension("virtualReality.extensions.activity.uiElementMinValue")
                    .ChangedBy("Sergej G.", "goerzen@informatik.rwth-aachen.de");
                DebugLog.xAPI.Print($"Dropped extension by path 'virtualReality.extensions.activity.uiElementMinValue': {statement}");
            }
            
            // Discard statement
            if (statement.HasActivity("generic.activities.mouse"))
            {
                DebugLog.xAPI.Print($"Has activity 'generic.activities.mouse': {statement}");
                statement.Discard();
                DebugLog.xAPI.Print($"Discarded statement: {statement}");
            }
            
            return statement;
        }
    }
}