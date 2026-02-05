/*
* SPDX-License-Identifier: AGPL-3.0-or-later
* Copyright (C) 2025 Sergej Görzen <sergej.goerzen@gmail.com>
* This file is part of OmiLAXR.
*/
using System;
using System.Linq;
using OmiLAXR.Components;
using OmiLAXR.Filters;
using UnityEngine;
using Object = UnityEngine.Object;

namespace OmiLAXR.ReCoPa.Filters
{
    /// <summary>
    /// Filter component for the ReCoPa pipeline.
    /// Restricts tracked objects to a configured list of tracking names.
    /// </summary>
    [AddComponentMenu("OmiLAXR / 2) Filters / ReCoPa Filter")]
    public sealed class ReCoPaFilter : Filter
    {
        /// <summary>
        /// List of GameObject tracking names that are allowed to pass.
        /// Names must match <see cref="OmiLAXR.Components.TrackingNameExtensions.GetTrackingName"/>.
        /// </summary>
        public string[] gameObjects = Array.Empty<string>();

        /// <summary>
        /// Filters the incoming objects by tracking name.
        /// </summary>
        /// <param name="gos">Incoming Unity Objects from the pipeline</param>
        /// <returns>Objects whose tracking name is in <see cref="gameObjects"/></returns>
        public override Object[] Pass(Object[] gos)
        {
            return gos.Where(go => gameObjects.Contains(go.GetTrackingName())).ToArray();
        }
    }
}
