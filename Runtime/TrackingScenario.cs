/*
* SPDX-License-Identifier: AGPL-3.0-or-later
* Copyright (C) 2025 Sergej Görzen <sergej.goerzen@gmail.com>
* This file is part of OmiLAXR.
*/
using System;

namespace OmiLAXR.ReCoPa
{
    /// <summary>
    /// Describes a tracking scenario for ReCoPa.
    /// Bundles the active game objects, actions, and gestures with a scenario name.
    /// </summary>
    public struct TrackingScenario
    {
        /// <summary>
        /// GameObject tracking names included in the scenario.
        /// </summary>
        public string[] gameObjects;

        /// <summary>
        /// Action identifiers tracked by the scenario.
        /// </summary>
        public string[] actions;

        /// <summary>
        /// Gesture identifiers tracked by the scenario.
        /// </summary>
        public string[] gestures;

        /// <summary>
        /// Human-readable name of the scenario.
        /// </summary>
        public string name;

        /// <summary>
        /// Returns a readable string representation for debugging.
        /// </summary>
        /// <returns>Formatted scenario description</returns>
        public override string ToString()
        {
            return $"[TrackingScenario name={name}, actions={Array(actions)}, gestures={Array(gestures)}, gameObjects={Array(gameObjects)}]";
        }

        /// <summary>
        /// Formats a string array for debug output.
        /// </summary>
        /// <param name="array">Array to format</param>
        /// <returns>Formatted array string</returns>
        private static string Array(string[] array)
        {
            var str = array != null ? string.Join(",", array) : null;
            return $"[Array: [{str}]]";
        }
    }
}
