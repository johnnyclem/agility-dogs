#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using AgilityDogs.Core;

namespace AgilityDogs.Editor
{
    /// <summary>
    /// Editor window for visualizing command timing for agility courses.
    /// Shows a timeline view of optimal command points for each obstacle.
    /// </summary>
    public class CourseTimingWindow : EditorWindow
    {
        private List<ObstacleTimingData> timingData;
        private List<ObstaclePlacement> obstacles;
        private Vector2 scrollPos;
        
        // Display settings
        private float timelineWidth = 600f;
        private float rowHeight = 40f;
        private float headerHeight = 50f;
        private float timeScale = 20f; // Pixels per second
        
        // Colors
        private Color earlyColor = new Color(1f, 0.3f, 0.3f, 0.7f);
        private Color optimalColor = new Color(0.3f, 1f, 0.3f, 0.7f);
        private Color lateColor = new Color(1f, 0.6f, 0.2f, 0.7f);
        private Color backgroundEven = new Color(0.9f, 0.9f, 0.9f, 1f);
        private Color backgroundOdd = new Color(0.85f, 0.85f, 0.85f, 1f);
        private Color timelineGrid = new Color(0.7f, 0.7f, 0.7f, 0.5f);
        
        private GUIStyle headerStyle;
        private GUIStyle rowStyle;
        private GUIStyle timeLabelStyle;

        public static void ShowTiming(List<ObstacleTimingData> timingData, List<ObstaclePlacement> obstacles)
        {
            var window = GetWindow<CourseTimingWindow>("Course Timing");
            window.SetData(timingData, obstacles);
            window.Show();
        }

        public void SetData(List<ObstacleTimingData> timingData, List<ObstaclePlacement> obstacles)
        {
            this.timingData = timingData;
            this.obstacles = obstacles;
            InitializeStyles();
        }

        private void InitializeStyles()
        {
            headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.black }
            };

            rowStyle = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(5, 5, 0, 0)
            };

            timeLabelStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.gray }
            };
        }

        private void OnGUI()
        {
            if (timingData == null || timingData.Count == 0)
            {
                EditorGUILayout.HelpBox("No timing data available. Generate timing chart from Course Editor.", MessageType.Info);
                return;
            }

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUILayout.LabelField($"Total Estimated Time: {GetTotalTime():F2}s", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                Repaint();
            }
            
            if (GUILayout.Button("Export CSV", EditorStyles.toolbarButton, GUILayout.Width(70)))
            {
                ExportToCSV();
            }
            
            EditorGUILayout.EndHorizontal();

            DrawSettings();
            GUILayout.Space(10);
            DrawTimeline();
        }

        private void DrawSettings()
        {
            EditorGUILayout.BeginHorizontal();
            timeScale = EditorGUILayout.Slider("Zoom", timeScale, 5f, 50f);
            rowHeight = EditorGUILayout.Slider("Row Height", rowHeight, 20f, 60f);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawTimeline()
        {
            float totalTime = GetTotalTime();
            float requiredWidth = Mathf.Max(timelineWidth, totalTime * timeScale + 100f);
            float requiredHeight = headerHeight + (timingData.Count * rowHeight) + 20f;

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.ExpandHeight(true));

            Rect viewRect = new Rect(0, 0, requiredWidth, requiredHeight);
            GUILayout.BeginArea(viewRect);

            // Draw background
            EditorGUI.DrawRect(viewRect, Color.white);

            // Draw time header
            DrawTimeHeader(totalTime);

            // Draw obstacle rows
            for (int i = 0; i < timingData.Count; i++)
            {
                DrawObstacleRow(i, timingData[i]);
            }

            // Draw vertical time grid lines
            DrawTimeGrid(totalTime);

            GUILayout.EndArea();
            EditorGUILayout.EndScrollView();
        }

        private void DrawTimeHeader(float totalTime)
        {
            Rect headerRect = new Rect(0, 0, timelineWidth, headerHeight);
            EditorGUI.DrawRect(headerRect, new Color(0.95f, 0.95f, 0.95f, 1f));

            // Draw time markers
            int maxSeconds = Mathf.CeilToInt(totalTime);
            for (int i = 0; i <= maxSeconds; i++)
            {
                float x = 50 + (i * timeScale);
                if (x > timelineWidth) break;

                Rect labelRect = new Rect(x - 15, 20, 30, 20);
                GUI.Label(labelRect, $"{i}s", timeLabelStyle);
            }

            // Header labels
            Rect obstacleLabel = new Rect(5, 5, 45, 15);
            GUI.Label(obstacleLabel, "Obstacle", EditorStyles.miniBoldLabel);

            Rect typeLabel = new Rect(5, 22, 45, 15);
            GUI.Label(typeLabel, "Type", EditorStyles.miniBoldLabel);
        }

        private void DrawObstacleRow(int index, ObstacleTimingData data)
        {
            float y = headerHeight + (index * rowHeight);
            bool isEven = index % 2 == 0;
            
            // Row background
            Rect rowRect = new Rect(0, y, timelineWidth, rowHeight);
            EditorGUI.DrawRect(rowRect, isEven ? backgroundEven : backgroundOdd);

            // Obstacle number and type
            Rect numberRect = new Rect(5, y + 2, 25, 18);
            GUI.Label(numberRect, $"{index + 1}", rowStyle);

            Rect typeRect = new Rect(5, y + 20, 45, 18);
            string typeShort = data.obstacleType.ToString().Replace("Jump", "").Replace("Poles", "P");
            GUI.Label(typeRect, typeShort, EditorStyles.miniLabel);

            // Draw timing windows
            float baseX = 50;

            // Early window (red)
            float earlyStart = baseX + (data.EarlyCommandTime * timeScale);
            float earlyWidth = data.earlyCommandWindow * timeScale;
            DrawTimingWindow(earlyStart, y + 5, earlyWidth, rowHeight - 10, earlyColor, "Early");

            // Optimal window (green)
            float optimalStart = baseX + (data.OptimalCommandTime * timeScale);
            float optimalWidth = data.optimalCommandWindow * timeScale;
            DrawTimingWindow(optimalStart, y + 5, optimalWidth, rowHeight - 10, optimalColor, "Optimal");

            // Late window (orange)
            float lateStart = baseX + (data.LateCommandTime * timeScale);
            float lateWidth = data.lateCommandWindow * timeScale;
            DrawTimingWindow(lateStart, y + 5, lateWidth, rowHeight - 10, lateColor, "Late");

            // Command point marker
            float cmdPointX = baseX + (data.OptimalCommandTime * timeScale);
            Rect markerRect = new Rect(cmdPointX - 2, y, 4, rowHeight);
            EditorGUI.DrawRect(markerRect, Color.black);

            // Duration bar
            float durationStart = baseX + (data.estimatedArrivalTime * timeScale);
            float durationWidth = data.expectedDuration * timeScale;
            Rect durationRect = new Rect(durationStart, y + rowHeight - 8, durationWidth, 6);
            EditorGUI.DrawRect(durationRect, new Color(0.5f, 0.5f, 0.8f, 0.7f));
        }

        private void DrawTimingWindow(float x, float y, float width, float height, Color color, string tooltip)
        {
            if (width < 2) width = 2; // Minimum width for visibility
            
            Rect rect = new Rect(x, y, width, height);
            EditorGUI.DrawRect(rect, color);

            // Border
            Handles.color = color * 0.7f;
            Handles.DrawLine(new Vector3(rect.x, rect.y), new Vector3(rect.x + rect.width, rect.y));
            Handles.DrawLine(new Vector3(rect.x, rect.y + rect.height), new Vector3(rect.x + rect.width, rect.y + rect.height));
            Handles.DrawLine(new Vector3(rect.x, rect.y), new Vector3(rect.x, rect.y + rect.height));
            Handles.DrawLine(new Vector3(rect.x + rect.width, rect.y), new Vector3(rect.x + rect.width, rect.y + rect.height));
        }

        private void DrawTimeGrid(float totalTime)
        {
            int maxSeconds = Mathf.CeilToInt(totalTime);
            
            for (int i = 0; i <= maxSeconds; i++)
            {
                float x = 50 + (i * timeScale);
                if (x > timelineWidth) break;

                Handles.color = timelineGrid;
                Handles.DrawLine(new Vector3(x, headerHeight), new Vector3(x, headerHeight + (timingData.Count * rowHeight)));
            }
        }

        private float GetTotalTime()
        {
            if (timingData == null || timingData.Count == 0) return 10f;
            
            var lastObstacle = timingData[timingData.Count - 1];
            return lastObstacle.LateCommandTime + lastObstacle.expectedDuration + 1f;
        }

        private void ExportToCSV()
        {
            string path = EditorUtility.SaveFilePanel("Export Timing Data", "", "course_timing", "csv");
            if (string.IsNullOrEmpty(path)) return;

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Obstacle,Type,Arrival Time,Early Window,Optimal Window,Late Window,Duration");

            for (int i = 0; i < timingData.Count; i++)
            {
                var d = timingData[i];
                sb.AppendLine($"{i + 1},{d.obstacleType},{d.estimatedArrivalTime:F3},{d.earlyCommandWindow:F3},{d.optimalCommandWindow:F3},{d.lateCommandWindow:F3},{d.expectedDuration:F3}");
            }

            System.IO.File.WriteAllText(path, sb.ToString());
            Debug.Log($"Timing data exported to: {path}");
            EditorUtility.RevealInFinder(path);
        }
    }
}
#endif