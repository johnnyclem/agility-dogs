#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using AgilityDogs.Gameplay.Replay;
using AgilityDogs.Core;

namespace AgilityDogs.Editor
{
    /// <summary>
    /// Editor window for diagnosing replay data and playback issues.
    /// Provides detailed analysis of recorded replays and troubleshooting tools.
    /// </summary>
    public class ReplayDiagnosticsWindow : EditorWindow
    {
        private ReplayData selectedReplay;
        private ReplayManager replayManager;
        
        // Analysis data
        private ReplayAnalysis analysis;
        private Vector2 scrollPos;
        private int selectedFrameIndex = -1;
        
        // Display settings
        private bool showFrameDetails = false;
        private bool showEventDetails = true;
        private bool showPathAnalysis = true;
        private bool showPerformanceMetrics = true;
        
        [MenuItem("Agility Dogs/Replay Diagnostics")]
        public static void ShowWindow()
        {
            GetWindow<ReplayDiagnosticsWindow>("Replay Diagnostics");
        }

        private void OnEnable()
        {
            FindReplayManager();
        }

        private void FindReplayManager()
        {
            if (Application.isPlaying)
            {
                replayManager = FindObjectOfType<ReplayManager>();
            }
        }

        private void OnGUI()
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play mode to analyze replays in real-time.", MessageType.Info);
                return;
            }

            FindReplayManager();
            
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            DrawToolbar();
            GUILayout.Space(10);

            DrawReplaySelector();
            GUILayout.Space(10);

            if (selectedReplay != null || (replayManager != null && replayManager.CurrentReplayData != null))
            {
                DrawAnalysis();
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                RefreshAnalysis();
            }

            if (GUILayout.Button("Load Replay", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                LoadReplayFromFile();
            }

            if (GUILayout.Button("Export Report", EditorStyles.toolbarButton, GUILayout.Width(90)))
            {
                ExportDiagnosticsReport();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawReplaySelector()
        {
            EditorGUILayout.LabelField("Replay Source", EditorStyles.boldLabel);

            if (replayManager != null)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                EditorGUILayout.LabelField("Current Replay Manager", EditorStyles.miniBoldLabel);
                
                if (replayManager.CurrentReplayData != null)
                {
                    if (GUILayout.Button("Use Current Replay"))
                    {
                        selectedReplay = replayManager.CurrentReplayData;
                        AnalyzeReplay(selectedReplay);
                    }
                    
                    EditorGUILayout.LabelField($"Frames: {replayManager.CurrentReplayData.frames.Count}");
                    EditorGUILayout.LabelField($"Events: {replayManager.CurrentReplayData.events.Count}");
                    EditorGUILayout.LabelField($"Duration: {replayManager.CurrentReplayData.GetDuration():F2}s");
                }
                else
                {
                    EditorGUILayout.LabelField("No replay data in manager");
                }
                
                EditorGUILayout.EndVertical();
            }

            // Selected replay field
            EditorGUILayout.BeginHorizontal();
            selectedReplay = (ReplayData)EditorGUILayout.ObjectField("Selected Replay", selectedReplay, typeof(ReplayData), false);
            if (selectedReplay != null && GUILayout.Button("Analyze", GUILayout.Width(60)))
            {
                AnalyzeReplay(selectedReplay);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawAnalysis()
        {
            var replay = selectedReplay ?? replayManager?.CurrentReplayData;
            if (replay == null) return;

            if (analysis == null)
            {
                AnalyzeReplay(replay);
            }

            EditorGUILayout.LabelField("Replay Analysis", EditorStyles.boldLabel);
            GUILayout.Space(5);

            // Overview
            DrawOverview(replay);
            GUILayout.Space(10);

            // Performance metrics
            if (showPerformanceMetrics)
            {
                DrawPerformanceMetrics();
                GUILayout.Space(10);
            }

            // Event analysis
            if (showEventDetails)
            {
                DrawEventAnalysis();
                GUILayout.Space(10);
            }

            // Path analysis
            if (showPathAnalysis)
            {
                DrawPathAnalysis();
                GUILayout.Space(10);
            }

            // Frame details
            if (showFrameDetails)
            {
                DrawFrameDetails(replay);
            }
        }

        private void DrawOverview(ReplayData replay)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Overview", EditorStyles.miniBoldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Duration:", GUILayout.Width(80));
            EditorGUILayout.LabelField($"{replay.GetDuration():F2}s");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Frames:", GUILayout.Width(80));
            EditorGUILayout.LabelField($"{replay.frames.Count}");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Events:", GUILayout.Width(80));
            EditorGUILayout.LabelField($"{replay.events.Count}");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Run Result:", GUILayout.Width(80));
            EditorGUILayout.LabelField($"{replay.runResult}");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Faults:", GUILayout.Width(80));
            EditorGUILayout.LabelField($"{replay.faultCount}");
            EditorGUILayout.EndHorizontal();

            if (analysis != null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Avg Frame Delta:", GUILayout.Width(80));
                EditorGUILayout.LabelField($"{analysis.averageFrameDelta:F4}s");
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Frame Rate:", GUILayout.Width(80));
                EditorGUILayout.LabelField($"{1f / analysis.averageFrameDelta:F1} fps");
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawPerformanceMetrics()
        {
            if (analysis == null) return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            showPerformanceMetrics = EditorGUILayout.Foldout(showPerformanceMetrics, "Performance Metrics", true);
            
            if (!showPerformanceMetrics) { EditorGUILayout.EndVertical(); return; }

            // Frame timing distribution
            EditorGUILayout.LabelField("Frame Timing", EditorStyles.miniBoldLabel);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Min Delta:", GUILayout.Width(100));
            EditorGUILayout.LabelField($"{analysis.minFrameDelta:F4}s");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Max Delta:", GUILayout.Width(100));
            EditorGUILayout.LabelField($"{analysis.maxFrameDelta:F4}s");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Std Dev:", GUILayout.Width(100));
            EditorGUILayout.LabelField($"{analysis.frameDeltaStdDev:F4}s");
            EditorGUILayout.EndHorizontal();

            // Path metrics
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Path Metrics", EditorStyles.miniBoldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Total Distance:", GUILayout.Width(100));
            EditorGUILayout.LabelField($"{analysis.totalPathDistance:F2}m");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Avg Speed:", GUILayout.Width(100));
            EditorGUILayout.LabelField($"{analysis.averageSpeed:F2} m/s");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Max Speed:", GUILayout.Width(100));
            EditorGUILayout.LabelField($"{analysis.maxSpeed:F2} m/s");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawEventAnalysis()
        {
            var replay = selectedReplay ?? replayManager?.CurrentReplayData;
            if (replay == null || replay.events.Count == 0) return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            showEventDetails = EditorGUILayout.Foldout(showEventDetails, $"Event Analysis ({replay.events.Count} events)", true);
            
            if (!showEventDetails) { EditorGUILayout.EndVertical(); return; }

            // Event type counts
            var eventCounts = new Dictionary<ReplayEventType, int>();
            foreach (var evt in replay.events)
            {
                if (!eventCounts.ContainsKey(evt.eventType))
                    eventCounts[evt.eventType] = 0;
                eventCounts[evt.eventType]++;
            }

            EditorGUILayout.LabelField("Event Distribution", EditorStyles.miniBoldLabel);
            foreach (var kvp in eventCounts)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"{kvp.Key}:", GUILayout.Width(150));
                EditorGUILayout.LabelField($"{kvp.Value}");
                EditorGUILayout.EndHorizontal();
            }

            // Event list
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Event Timeline", EditorStyles.miniBoldLabel);

            for (int i = 0; i < Mathf.Min(replay.events.Count, 50); i++)
            {
                var evt = replay.events[i];
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"[{evt.timestamp:F2}s]", GUILayout.Width(60));
                EditorGUILayout.LabelField($"{evt.eventType}", GUILayout.Width(120));
                EditorGUILayout.LabelField(evt.data, EditorStyles.miniLabel);
                EditorGUILayout.EndHorizontal();
            }

            if (replay.events.Count > 50)
            {
                EditorGUILayout.LabelField($"... and {replay.events.Count - 50} more events", EditorStyles.miniLabel);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawPathAnalysis()
        {
            if (analysis == null) return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            showPathAnalysis = EditorGUILayout.Foldout(showPathAnalysis, "Path Analysis", true);
            
            if (!showPathAnalysis) { EditorGUILayout.EndVertical(); return; }

            // Path quality metrics
            EditorGUILayout.LabelField("Path Quality", EditorStyles.miniBoldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Path Efficiency:", GUILayout.Width(120));
            EditorGUILayout.LabelField($"{analysis.pathEfficiency * 100:F1}%");
            EditorGUILayout.EndHorizontal();

            float efficiency = analysis.pathEfficiency;
            Rect rect = EditorGUILayout.GetControlRect(false, 20);
            EditorGUI.ProgressBar(rect, efficiency, $"Efficiency: {efficiency * 100:F1}%");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Sharp Turns:", GUILayout.Width(120));
            EditorGUILayout.LabelField($"{analysis.sharpTurnCount}");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Stops/Pauses:", GUILayout.Width(120));
            EditorGUILayout.LabelField($"{analysis.pauseCount}");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Max Velocity Change:", GUILayout.Width(120));
            EditorGUILayout.LabelField($"{analysis.maxVelocityChange:F2} m/s");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawFrameDetails(ReplayData replay)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            showFrameDetails = EditorGUILayout.Foldout(showFrameDetails, "Frame Details", true);
            
            if (!showFrameDetails) { EditorGUILayout.EndVertical(); return; }

            // Frame selector
            EditorGUILayout.BeginHorizontal();
            selectedFrameIndex = EditorGUILayout.IntSlider("Frame", selectedFrameIndex, 0, replay.frames.Count - 1);
            if (GUILayout.Button("Go", GUILayout.Width(40)))
            {
                // Jump to frame in replay
                if (replayManager != null)
                {
                    replayManager.SeekToTime(replay.frames[selectedFrameIndex].timestamp);
                }
            }
            EditorGUILayout.EndHorizontal();

            if (selectedFrameIndex >= 0 && selectedFrameIndex < replay.frames.Count)
            {
                var frame = replay.frames[selectedFrameIndex];
                
                EditorGUILayout.LabelField($"Frame {selectedFrameIndex} at {frame.timestamp:F3}s", EditorStyles.miniBoldLabel);
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Dog Pos:", GUILayout.Width(70));
                EditorGUILayout.LabelField($"{frame.dogPosition}");
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Handler Pos:", GUILayout.Width(70));
                EditorGUILayout.LabelField($"{frame.handlerPosition}");
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Dog State:", GUILayout.Width(70));
                EditorGUILayout.LabelField($"{frame.dogState}");
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }

        private void AnalyzeReplay(ReplayData replay)
        {
            if (replay == null || replay.frames.Count < 2) return;

            analysis = new ReplayAnalysis();
            
            // Frame timing analysis
            float minDelta = float.MaxValue;
            float maxDelta = float.MinValue;
            float sumDelta = 0f;
            float sumDeltaSquared = 0f;
            int deltaCount = 0;

            for (int i = 1; i < replay.frames.Count; i++)
            {
                float delta = replay.frames[i].timestamp - replay.frames[i - 1].timestamp;
                if (delta > 0 && delta < 1f) // Filter out anomalies
                {
                    minDelta = Mathf.Min(minDelta, delta);
                    maxDelta = Mathf.Max(maxDelta, delta);
                    sumDelta += delta;
                    sumDeltaSquared += delta * delta;
                    deltaCount++;
                }
            }

            if (deltaCount > 0)
            {
                analysis.averageFrameDelta = sumDelta / deltaCount;
                analysis.minFrameDelta = minDelta == float.MaxValue ? 0 : minDelta;
                analysis.maxFrameDelta = maxDelta == float.MinValue ? 0 : maxDelta;
                
                float mean = analysis.averageFrameDelta;
                analysis.frameDeltaStdDev = Mathf.Sqrt((sumDeltaSquared / deltaCount) - (mean * mean));
            }

            // Path analysis
            float totalDistance = 0f;
            float maxSpeed = 0f;
            float sumSpeed = 0f;
            int sharpTurns = 0;
            int pauses = 0;
            float maxVelocityChange = 0f;

            for (int i = 1; i < replay.frames.Count; i++)
            {
                float dist = Vector3.Distance(replay.frames[i].dogPosition, replay.frames[i - 1].dogPosition);
                totalDistance += dist;

                // Calculate speed
                float delta = replay.frames[i].timestamp - replay.frames[i - 1].timestamp;
                if (delta > 0)
                {
                    float speed = dist / delta;
                    maxSpeed = Mathf.Max(maxSpeed, speed);
                    sumSpeed += speed;

                    // Detect pauses
                    if (speed < 0.5f) pauses++;

                    // Detect sharp turns
                    if (i > 1)
                    {
                        Vector3 prevDir = (replay.frames[i - 1].dogPosition - replay.frames[i - 2].dogPosition).normalized;
                        Vector3 currDir = (replay.frames[i].dogPosition - replay.frames[i - 1].dogPosition).normalized;
                        float angle = Vector3.Angle(prevDir, currDir);
                        if (angle > 45f) sharpTurns++;
                    }
                }
            }

            analysis.totalPathDistance = totalDistance;
            analysis.averageSpeed = sumSpeed / Mathf.Max(1, replay.frames.Count - 1);
            analysis.maxSpeed = maxSpeed;
            analysis.sharpTurnCount = sharpTurns;
            analysis.pauseCount = pauses;

            // Calculate efficiency (straight line vs actual path)
            if (replay.frames.Count > 1)
            {
                float straightDistance = Vector3.Distance(replay.frames[0].dogPosition, replay.frames[replay.frames.Count - 1].dogPosition);
                analysis.pathEfficiency = straightDistance / Mathf.Max(0.1f, totalDistance);
            }

            Debug.Log($"[ReplayDiagnostics] Analyzed replay: {analysis}");
        }

        private void RefreshAnalysis()
        {
            var replay = selectedReplay ?? replayManager?.CurrentReplayData;
            if (replay != null)
            {
                AnalyzeReplay(replay);
            }
        }

        private void LoadReplayFromFile()
        {
            string path = EditorUtility.OpenFilePanel("Load Replay Data", "", "asset");
            if (!string.IsNullOrEmpty(path))
            {
                if (path.StartsWith(Application.dataPath))
                {
                    path = "Assets" + path.Substring(Application.dataPath.Length);
                }

                selectedReplay = AssetDatabase.LoadAssetAtPath<ReplayData>(path);
                if (selectedReplay != null)
                {
                    AnalyzeReplay(selectedReplay);
                }
            }
        }

        private void ExportDiagnosticsReport()
        {
            var replay = selectedReplay ?? replayManager?.CurrentReplayData;
            if (replay == null || analysis == null)
            {
                EditorUtility.DisplayDialog("No Data", "No replay data to export.", "OK");
                return;
            }

            string path = EditorUtility.SaveFilePanel("Export Diagnostics Report", "", "replay_diagnostics", "txt");
            if (string.IsNullOrEmpty(path)) return;

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("=== Replay Diagnostics Report ===");
            sb.AppendLine($"Generated: {System.DateTime.Now}");
            sb.AppendLine();
            
            sb.AppendLine("--- Overview ---");
            sb.AppendLine($"Duration: {replay.GetDuration():F2}s");
            sb.AppendLine($"Frames: {replay.frames.Count}");
            sb.AppendLine($"Events: {replay.events.Count}");
            sb.AppendLine($"Run Result: {replay.runResult}");
            sb.AppendLine($"Faults: {replay.faultCount}");
            sb.AppendLine();
            
            sb.AppendLine("--- Performance Metrics ---");
            sb.AppendLine($"Avg Frame Delta: {analysis.averageFrameDelta:F4}s");
            sb.AppendLine($"Frame Rate: {1f / analysis.averageFrameDelta:F1} fps");
            sb.AppendLine($"Min Frame Delta: {analysis.minFrameDelta:F4}s");
            sb.AppendLine($"Max Frame Delta: {analysis.maxFrameDelta:F4}s");
            sb.AppendLine($"Frame Delta StdDev: {analysis.frameDeltaStdDev:F4}s");
            sb.AppendLine();
            
            sb.AppendLine("--- Path Metrics ---");
            sb.AppendLine($"Total Distance: {analysis.totalPathDistance:F2}m");
            sb.AppendLine($"Avg Speed: {analysis.averageSpeed:F2} m/s");
            sb.AppendLine($"Max Speed: {analysis.maxSpeed:F2} m/s");
            sb.AppendLine($"Path Efficiency: {analysis.pathEfficiency * 100:F1}%");
            sb.AppendLine($"Sharp Turns: {analysis.sharpTurnCount}");
            sb.AppendLine($"Pauses: {analysis.pauseCount}");
            sb.AppendLine();

            sb.AppendLine("--- Events ---");
            foreach (var evt in replay.events)
            {
                sb.AppendLine($"[{evt.timestamp:F3}s] {evt.eventType}: {evt.data}");
            }

            System.IO.File.WriteAllText(path, sb.ToString());
            Debug.Log($"Diagnostics report exported to: {path}");
            EditorUtility.RevealInFinder(path);
        }
    }

    [System.Serializable]
    public class ReplayAnalysis
    {
        // Frame timing
        public float averageFrameDelta;
        public float minFrameDelta;
        public float maxFrameDelta;
        public float frameDeltaStdDev;

        // Path metrics
        public float totalPathDistance;
        public float averageSpeed;
        public float maxSpeed;
        public float pathEfficiency;
        public int sharpTurnCount;
        public int pauseCount;
        public float maxVelocityChange;

        public override string ToString()
        {
            return $"ReplayAnalysis: {totalPathDistance:F2}m, {averageSpeed:F2}m/s avg, {pathEfficiency * 100:F1}% efficient";
        }
    }
}
#endif