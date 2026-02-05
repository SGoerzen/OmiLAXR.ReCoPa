/*
* SPDX-License-Identifier: AGPL-3.0-or-later
* Copyright (C) 2025 Sergej Görzen <sergej.goerzen@gmail.com>
* This file is part of OmiLAXR.
*/
using System;

namespace OmiLAXR.ReCoPa
{
    /// <summary>
    /// Snapshot of tracking state and runtime metadata for ReCoPa.
    /// </summary>
    [Serializable]
    public struct TrackingMeta
    {
        /// <summary>
        /// True when tracking is currently active.
        /// </summary>
        public bool isTracking;

        /// <summary>
        /// True when tracking is paused.
        /// </summary>
        public bool isTrackingPaused;

        /// <summary>
        /// True when calibration has been completed.
        /// </summary>
        public bool isCalibrated;

        /// <summary>
        /// Host machine name.
        /// </summary>
        public string computerName;

        /// <summary>
        /// Primary actor name.
        /// </summary>
        public string actorName;

        /// <summary>
        /// Primary actor email.
        /// </summary>
        public string actorEmail;

        /// <summary>
        /// Active actor name (if different from primary).
        /// </summary>
        public string activeActorName;

        /// <summary>
        /// Active actor email (if different from primary).
        /// </summary>
        public string activeActorEmail;

        /// <summary>
        /// LRS registration id for the session.
        /// </summary>
        public string registrationId;

        /// <summary>
        /// Active endpoint identifiers.
        /// </summary>
        public string[] endpoints;

        /// <summary>
        /// Active filter identifiers.
        /// </summary>
        public string[] filters;

        /// <summary>
        /// Action identifiers available during tracking.
        /// </summary>
        public string[] actions;

        /// <summary>
        /// Gesture identifiers available during tracking.
        /// </summary>
        public string[] gestures;

        /// <summary>
        /// Latest heart rate reading.
        /// </summary>
        public float? heartRate;

        /// <summary>
        /// Latest frames-per-second reading.
        /// </summary>
        public float? fps;

        /// <summary>
        /// Additional metadata context serialized as string.
        /// </summary>
        public string metaContext; 

        /// <summary>
        /// Empty/default metadata instance.
        /// </summary>
        public static readonly TrackingMeta Empty = new TrackingMeta();
    }
}
