#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace OmiLAXR.ReCoPa
{
    /// <summary>
    /// Unity Editor menu extension for ReCoPa module creation.
    /// </summary>
    internal static class CustomMenu
    {
        /// <summary>
        /// Adds ReCoPa Connector prefab to the scene via Unity's GameObject menu.
        /// Creates the connector as a child of the selected object or at scene root.
        /// </summary>
        [MenuItem("GameObject / OmiLAXR / Modules / ReCoPa Connector")]
        private static void AddReCoPaConnector()
        {
            // Get currently selected GameObject in hierarchy
            var selectedGameObject = Selection.activeGameObject;
            
            // Load the ReCoPa prefab from Resources folder
            var prefab = Resources.Load<GameObject>("Prefabs/OmiLAXR.ReCoPa");
            
            // Instantiate as child of selected object or at scene root
            if (selectedGameObject)
                PrefabUtility.InstantiatePrefab(prefab, selectedGameObject.transform);
            else
                PrefabUtility.InstantiatePrefab(prefab);
        }
    }
}
#endif