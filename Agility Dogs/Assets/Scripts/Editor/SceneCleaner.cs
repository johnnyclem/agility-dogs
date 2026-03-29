using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.AI;

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

        [MenuItem("Tools/Bake NavMesh Surfaces")]
        public static void BakeNavMeshSurfaces()
        {
            var surfaces = Object.FindObjectsOfType<NavMeshSurface>();
            if (surfaces == null || surfaces.Length == 0)
            {
                Debug.LogWarning("No NavMeshSurface found in scene.");
                return;
            }

            int baked = 0;
            foreach (var surface in surfaces)
            {
                surface.BuildNavMesh();
                baked++;
                Debug.Log($"Baked NavMeshSurface on {surface.gameObject.name}");
            }

            Debug.Log($"NavMesh bake complete. Baked {baked} surfaces.");
        }

        [MenuItem("Tools/Wire Scene References")]
        public static void WireSceneReferences()
        {
            WireObstacleNavigationPoints();
            WireWeavePolePositions();
        }

        private static void WireObstacleNavigationPoints()
        {
            var obstacles = Object.FindObjectsOfType<AgilityDogs.Gameplay.Obstacles.ObstacleBase>();
            int wired = 0;
            foreach (var obs in obstacles)
            {
                var entry = obs.transform.Find("EntryPoint");
                var commit = obs.transform.Find("CommitPoint");
                var exit = obs.transform.Find("ExitPoint");

                var serializedObj = new SerializedObject(obs);
                if (entry != null)
                {
                    serializedObj.FindProperty("entryPoint").objectReferenceValue = entry;
                    wired++;
                }
                if (commit != null)
                {
                    serializedObj.FindProperty("commitPoint").objectReferenceValue = commit;
                    wired++;
                }
                if (exit != null)
                {
                    serializedObj.FindProperty("exitPoint").objectReferenceValue = exit;
                    wired++;
                }
                serializedObj.ApplyModifiedProperties();
            }
            Debug.Log($"Wired {wired} navigation point references across {obstacles.Length} obstacles.");
        }

        private static void WireWeavePolePositions()
        {
            var weaves = Object.FindObjectsOfType<AgilityDogs.Gameplay.Obstacles.WeavePolesObstacle>();
            foreach (var weave in weaves)
            {
                var poles = new System.Collections.Generic.List<Transform>();
                for (int i = 0; i < weave.transform.childCount; i++)
                {
                    var child = weave.transform.GetChild(i);
                    if (child.name.StartsWith("Pole"))
                    {
                        poles.Add(child);
                    }
                }
                if (poles.Count > 0)
                {
                    var serializedObj = new SerializedObject(weave);
                    var prop = serializedObj.FindProperty("polePositions");
                    prop.arraySize = poles.Count;
                    for (int i = 0; i < poles.Count; i++)
                    {
                        prop.GetArrayElementAtIndex(i).objectReferenceValue = poles[i];
                    }
                    serializedObj.ApplyModifiedProperties();
                    Debug.Log($"Wired {poles.Count} poles on {weave.gameObject.name}");
                }
            }
        }
    }
}
