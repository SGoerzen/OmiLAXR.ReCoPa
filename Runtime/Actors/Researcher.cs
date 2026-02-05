/*
* SPDX-License-Identifier: AGPL-3.0-or-later
* Copyright (C) 2025 Sergej Görzen <sergej.goerzen@gmail.com>
* This file is part of OmiLAXR.
*/
using UnityEngine;

namespace OmiLAXR.ReCoPa
{
    /// <summary>
    /// Default actor configuration for a researcher persona.
    /// </summary>
    [AddComponentMenu("OmiLAXR / Actors / Researcher")]
    public class Researcher : Actor
    {
        /// <summary>
        /// Unity reset callback used to populate default actor values.
        /// </summary>
        private void OnReset()
        {
            actorName = "Researcher";
            actorEmail = "anonymous@omilaxr.dev";
        }
    }
}
