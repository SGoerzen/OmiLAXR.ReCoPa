#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace OmiLAXR.Modules.ReCoPa
{
    internal static class CustomMenu
    {
        [MenuItem("GameObject / OmiLAXR / Modules / ReCoPa Connector")]
        private static void AddReCoPaConnector()
        {
            var selectedGameObject = Selection.activeGameObject;
            var prefab = Resources.Load<GameObject>("Prefabs/OmiLAXR.Modules.ReCoPa");
            if (selectedGameObject)
                PrefabUtility.InstantiatePrefab(prefab, selectedGameObject.transform);
            else
                PrefabUtility.InstantiatePrefab(prefab);
        }
    }
}
#endif
