#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using AgilityDogs.Core;

namespace AgilityDogs.Editor
{
    public class DebugVisualizerWindow : EditorWindow
    {
        private bool showDogAIState = true;
        private bool showDogPath = true;
        private bool showObstacleZones = true;
        private bool showCommandHistory = true;
        private bool showHandlerInfluence = true;

        private float visualizationHeight = 0.5f;
        private Color pathColor = Color.green;
        private Color plannedPathColor = Color.cyan;
        private Color errorPathColor = Color.red;
        private float pathWidth = 0.2f;

        private List<DebugPathPoint> pathHistory = new List<DebugPathPoint>();
        private int maxHistoryPoints = 1000;

        [MenuItem("Agility Dogs/Debug Visualizer")]
        public static void ShowWindow()
        {
            GetWindow<DebugVisualizerWindow>("Debug Visualizer");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("AI Debug Visualizer", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Display Options", EditorStyles.miniBoldLabel);
            showDogAIState = EditorGUILayout.Toggle("Dog AI State", showDogAIState);
            showDogPath = EditorGUILayout.Toggle("Dog Path", showDogPath);
            showObstacleZones = EditorGUILayout.Toggle("Obstacle Zones", showObstacleZones);
            showCommandHistory = EditorGUILayout.Toggle("Command History", showCommandHistory);
            showHandlerInfluence = EditorGUILayout.Toggle("Handler Influence", showHandlerInfluence);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Settings", EditorStyles.miniBoldLabel);
            visualizationHeight = EditorGUILayout.Slider("Height Offset", visualizationHeight, 0f, 2f);
            pathWidth = EditorGUILayout.Slider("Path Width", pathWidth, 0.05f, 1f);

            EditorGUILayout.BeginHorizontal();
            pathColor = EditorGUILayout.ColorField("Path Color", pathColor);
            plannedPathColor = EditorGUILayout.ColorField("Planned Path", plannedPathColor);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Actions", EditorStyles.miniBoldLabel);
            
            if (GUILayout.Button("Clear Path History"))
            {
                pathHistory.Clear();
            }

            if (GUILayout.Button("Capture Current State"))
            {
                CaptureDebugState();
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Runtime Info", EditorStyles.miniBoldLabel);
            EditorGUILayout.LabelField($"Path Points: {pathHistory.Count}");
            
            if (Application.isPlaying)
            {
                var dog = FindObjectOfType<Gameplay.Dog.DogAgentController>();
                if (dog != null)
                {
                    EditorGUILayout.LabelField($"Dog State: {dog.CurrentState}");
                    EditorGUILayout.LabelField($"Dog Speed: {dog.CurrentSpeed:F2}");
                    EditorGUILayout.LabelField($"Target Obstacle: {dog.TargetObstacle}");
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Enter Play mode to see runtime data", MessageType.Info);
            }
        }

        private void OnSceneGUI()
        {
            if (!Application.isPlaying) return;

            var dog = FindObjectOfType<Gameplay.Dog.DogAgentController>();
            if (dog == null) return;

            if (showDogPath && pathHistory.Count < maxHistoryPoints)
            {
                pathHistory.Add(new DebugPathPoint
                {
                    position = dog.transform.position,
                    time = Time.time,
                    state = dog.CurrentState
                });

                while (pathHistory.Count > maxHistoryPoints)
                {
                    pathHistory.RemoveAt(0);
                }
            }

            if (showDogPath && pathHistory.Count > 1)
            {
                Handles.color = pathColor;
                Handles.DrawLine(pathHistory[0].position, pathHistory[0].position, pathWidth);

                for (int i = 1; i < pathHistory.Count; i++)
                {
                    float alpha = (float)i / pathHistory.Count;
                    Handles.color = new Color(pathColor.r, pathColor.g, pathColor.b, alpha);
                    Handles.DrawLine(pathHistory[i - 1].position, pathHistory[i].position, pathWidth);

                    if (pathHistory[i].state != pathHistory[i - 1].state)
                    {
                        Handles.color = Color.yellow;
                        Handles.SphereHandleCap(0, pathHistory[i].position, Quaternion.identity, 0.5f, EventType.Repaint);
                    }
                }
            }

            if (showDogAIState)
            {
                DrawAIStateVisualization(dog);
            }

            if (showObstacleZones)
            {
                DrawObstacleZones();
            }

            if (showHandlerInfluence)
            {
                DrawHandlerInfluence(dog);
            }

            SceneView.RepaintAll();
        }

        private void DrawAIStateVisualization(Gameplay.Dog.DogAgentController dog)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold
            };
            style.normal.textColor = GetStateColor(dog.CurrentState);

            Handles.Label(
                dog.transform.position + Vector3.up * 2f,
                $"State: {dog.CurrentState}",
                style
            );

            Handles.color = new Color(0, 1, 0, 0.1f);
            Handles.DrawSolidDisc(
                dog.transform.position,
                Vector3.up,
                10f
            );

            if (dog.TargetObstacle != null)
            {
                Handles.color = Color.magenta;
                Handles.DrawDottedLine(
                    dog.transform.position,
                    dog.TargetObstacle.transform.position,
                    5f
                );

                Handles.Label(
                    dog.TargetObstacle.transform.position + Vector3.up,
                    "Target Obstacle"
                );
            }
        }

        private void DrawObstacleZones()
        {
            var obstacles = FindObjectsOfType<Gameplay.Obstacles.ObstacleBase>();
            foreach (var obstacle in obstacles)
            {
                Handles.color = new Color(0, 1, 0, 0.2f);
                Handles.DrawWireDisc(
                    obstacle.transform.position + obstacle.transform.forward * 2f,
                    Vector3.up,
                    2f
                );

                Handles.color = new Color(1, 1, 0, 0.2f);
                Handles.DrawWireDisc(
                    obstacle.transform.position + obstacle.transform.forward * -2f,
                    Vector3.up,
                    2f
                );

                Handles.Label(
                    obstacle.transform.position + Vector3.up * 0.5f,
                    obstacle.ObstacleType.ToString()
                );
            }
        }

        private void DrawHandlerInfluence(Gameplay.Dog.DogAgentController dog)
        {
            var handler = FindObjectOfType<Gameplay.Handler.HandlerController>();
            if (handler == null) return;

            float influenceRadius = 8f;
            Handles.color = new Color(1, 0.5f, 0, 0.2f);
            Handles.DrawWireDisc(
                handler.transform.position,
                Vector3.up,
                influenceRadius
            );

            Handles.color = Color.orange;
            Handles.DrawDottedLine(
                handler.transform.position,
                dog.transform.position,
                3f
            );

            Handles.color = Color.red;
            Vector3 dir = handler.transform.forward * 3f;
            Handles.DrawLine(handler.transform.position, handler.transform.position + dir, 2f);
        }

        private Color GetStateColor(Core.DogState state)
        {
            return state switch
            {
                Core.DogState.Idle => Color.gray,
                Core.DogState.Running => Color.green,
                Core.DogState.Heeling => Color.blue,
                Core.DogState.CompletingObstacle => Color.cyan,
                Core.DogState.Recovering => Color.red,
                _ => Color.white
            };
        }

        private void CaptureDebugState()
        {
            Debug.Log("=== Debug State Capture ===");
            Debug.Log($"Path History Points: {pathHistory.Count}");

            if (Application.isPlaying)
            {
                var dog = FindObjectOfType<Gameplay.Dog.DogAgentController>();
                if (dog != null)
                {
                    Debug.Log($"Dog State: {dog.CurrentState}");
                    Debug.Log($"Dog Position: {dog.transform.position}");
                    Debug.Log($"Dog Speed: {dog.CurrentSpeed}");
                }
            }

            Debug.Log("=========================");
        }
    }

    [System.Serializable]
    public class DebugPathPoint
    {
        public Vector3 position;
        public float time;
        public Core.DogState state;
    }
}
#endif
