/*
* SPDX-License-Identifier: AGPL-3.0-or-later
* Copyright (C) 2025 Sergej Görzen <sergej.goerzen@gmail.com>
* This file is part of OmiLAXR.
*/
using OmiLAXR.Pipelines;
using UnityEngine;

namespace OmiLAXR.ReCoPa
{
    /// <summary>
    /// Pipeline extension entry point for the ReCoPa learner pipeline.
    /// Registers module-specific behaviour on top of <see cref="LearnerPipeline"/>.
    /// </summary>
    [AddComponentMenu("OmiLAXR / 0) Pipeline Extensions / Learner Pipeline Extension (ReCoPa)")]
    public class LearnerPipelineExtension : PipelineExtension<LearnerPipeline>
    {
    }
}
