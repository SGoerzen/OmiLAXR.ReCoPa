/*
* SPDX-License-Identifier: AGPL-3.0-or-later
* Copyright (C) 2025 Sergej Görzen <sergej.goerzen@gmail.com>
* This file is part of OmiLAXR.
*/
using System.Collections.Generic;
using OmiLAXR.Utils;

namespace OmiLAXR.ReCoPa
{
    /// <summary>
    /// Configuration map for a single endpoint.
    /// Uses <see cref="DataMap"/> for flexible key/value storage.
    /// </summary>
    public class EndpointConfig : DataMap
    {
    }
    
    /// <summary>
    /// Collection of endpoint configurations keyed by endpoint identifier.
    /// </summary>
    public class EndpointConfigs : Dictionary<string, EndpointConfig> {}
}
