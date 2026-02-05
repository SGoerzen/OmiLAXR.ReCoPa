/*
* SPDX-License-Identifier: AGPL-3.0-or-later
* Copyright (C) 2025 Sergej Görzen <sergej.goerzen@gmail.com>
* This file is part of OmiLAXR.
*/
using System;
using System.ComponentModel;
using OmiLAXR.Composers;
using OmiLAXR.Endpoints;
using UnityEngine;

namespace OmiLAXR.ReCoPa.Endpoints
{
    /// <summary>
    /// ReCoPa endpoint stub for statement delivery.
    /// Integrates into the endpoint pipeline while ReCoPa handles transport elsewhere.
    /// </summary>
    [AddComponentMenu("OmiLAXR / 6) Endpoints / ReCoPa Endpoint"),
     Description("Send statements to ReCoPa.")]
    public class ReCoPaEndpoint : Endpoint
    {
        /// <summary>
        /// Handles outgoing statements and reports the transfer result.
        /// </summary>
        /// <param name="statement">Statement to send</param>
        /// <returns>Transfer code for the pipeline</returns>
        protected override TransferCode HandleSending(IStatement statement)
        {
            return TransferCode.Success;
        }
    }
}
