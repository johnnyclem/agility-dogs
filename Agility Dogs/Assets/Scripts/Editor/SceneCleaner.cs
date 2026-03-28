using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

namespace AgilityDogs.Editor
{
    public static class SceneCleaner
    {
        [MenuItem("Tools/Cleanup Duplicate Panels")]
        public static void CleanupDuplicatePanels()
        {
            var canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogWarning("No Canvas found in scene.");
                return;
            }

            var seen = new System.Collections.Generic.HashSet<string>();
            int deleted = 0;

            for (int i = canvas.transform.childCount - 1; i >= 0; i--)
            {
                var child = canvas.transform.GetChild(i);
                string name = child.gameObject.name;

                if (seen.Contains(name))
                {
                    Debug.Log($"Deleting duplicate: {name}");
                    Object.DestroyImmediate(child.gameObject);
                    deleted++;
                }
                else
                {
                    seen.Add(name);
                }
            }

            Debug.Log($"Cleanup complete. Deleted {deleted} duplicate panels. Remaining: {canvas.transform.childCount}");
        }
    }
}
