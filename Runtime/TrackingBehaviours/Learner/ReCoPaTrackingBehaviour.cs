/*
* SPDX-License-Identifier: AGPL-3.0-or-later
* Copyright (C) 2025 Sergej Görzen <sergej.goerzen@gmail.com>
* This file is part of OmiLAXR.
*/
using OmiLAXR.TrackingBehaviours;
using UnityEngine;

namespace OmiLAXR.ReCoPa.TrackingBehaviours
{
    /// <summary>
    /// ReCoPa tracking behaviour placeholder for learner pipeline integration.
    /// Extend to bind ReCoPa-specific tracking logic for filtered objects.
    /// </summary>
    [AddComponentMenu("OmiLAXR / 3) Tracking Behaviours / ReCoPa Tracking Behaviour (ReCoPa)")]
    public sealed class ReCoPaTrackingBehaviour : TrackingBehaviour
    {
        /// <summary>
        /// Called after objects have been filtered by the pipeline.
        /// Override or extend to add ReCoPa-specific bindings.
        /// </summary>
        /// <param name="objects">Filtered objects to track</param>
        protected override void AfterFilteredObjects(Object[] objects)
        {
            // Intentionally empty - implement tracking bindings as needed.
        }

        /// <summary>
        /// Cleanup hook when tracking stops.
        /// </summary>
        /// <param name="objects">Objects that were tracked</param>
        protected override void Dispose(Object[] objects)
        {
            // Intentionally empty - implement cleanup as needed.
        }
    }
}
