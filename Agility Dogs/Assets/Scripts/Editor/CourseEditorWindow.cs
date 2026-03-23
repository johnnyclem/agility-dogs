#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using AgilityDogs.Core;
using AgilityDogs.Data;

namespace AgilityDogs.Editor
{
    /// <summary>
    /// Custom editor window for creating and editing agility courses.
    /// Provides drag-and-drop obstacle placement, sequence validation, and export functionality.
    /// </summary>
    public class CourseEditorWindow : EditorWindow
    {
        // Course data
        private CourseDefinition currentCourse;
        private List<ObstaclePlacement> obstaclePlacements = new List<ObstaclePlacement>();
        private string courseName = "New Course";
        private CourseType courseType = CourseType.Standard;

        // Editor state
        private Vector2 scrollPos;
        private int selectedObstacleIndex = -1;
        private bool showValidationPanel = true;
        private bool showCourseInfo = true;
        private bool snapToGrid = true;
        private float gridSize = 1f;

        // Validation
        private List<ValidationIssue> validationIssues = new List<ValidationIssue>();

        // Grid
        private Texture2D gridTexture;

        [MenuItem("Agility Dogs/Course Editor")]
        public static void ShowWindow()
        {
            GetWindow<CourseEditorWindow>("Course Editor");
        }

        private void OnEnable()
        {
            CreateGridTexture();
        }

        private void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            DrawToolbar();
            GUILayout.Space(10);

            DrawCourseInfoPanel();
            GUILayout.Space(10);

            DrawObstaclePalette();
            GUILayout.Space(10);

            if (selectedObstacleIndex >= 0 && selectedObstacleIndex < obstaclePlacements.Count)
            {
                DrawSelectedObstaclePanel();
                GUILayout.Space(10);
            }

            DrawCourseSequence();
            GUILayout.Space(10);

            DrawValidationPanel();
            GUILayout.Space(10);

            DrawCommandTimingPanel();
            GUILayout.Space(10);

            DrawActions();

            EditorGUILayout.EndScrollView();
        }

        #region Toolbar

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("New Course", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                CreateNewCourse();
            }

            if (GUILayout.Button("Load Course", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                LoadCourse();
            }

            if (GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                SaveCourse();
            }

            GUILayout.FlexibleSpace();

            snapToGrid = GUILayout.Toggle(snapToGrid, "Snap to Grid", EditorStyles.toolbarButton, GUILayout.Width(80));

            EditorGUILayout.EndHorizontal();
        }

        #endregion

        #region Course Info Panel

        private void DrawCourseInfoPanel()
        {
            showCourseInfo = EditorGUILayout.Foldout(showCourseInfo, "Course Information", true);
            if (!showCourseInfo) return;

            EditorGUI.indentLevel++;

            courseName = EditorGUILayout.TextField("Course Name", courseName);
            courseType = (CourseType)EditorGUILayout.EnumPopup("Course Type", courseType);

            EditorGUILayout.HelpBox($"Obstacles: {obstaclePlacements.Count} | Type: {courseType}", MessageType.Info);

            EditorGUI.indentLevel--;
        }

        #endregion

        #region Obstacle Palette

        private void DrawObstaclePalette()
        {
            EditorGUILayout.LabelField("Obstacle Palette", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();

            // Draw buttons for each obstacle type
            var obstacleTypes = System.Enum.GetValues(typeof(ObstacleType));
            int buttonsPerRow = 4;
            int count = 0;

            foreach (ObstacleType type in obstacleTypes)
            {
                if (type == ObstacleType.None) continue;

                if (GUILayout.Button(type.ToString(), GUILayout.Height(30), GUILayout.ExpandWidth(true)))
                {
                    AddObstacle(type);
                }

                count++;
                if (count % buttonsPerRow == 0)
                {
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private void AddObstacle(ObstacleType type)
        {
            var placement = new ObstaclePlacement
            {
                obstacleType = type,
                position = Vector3.zero,
                rotation = Quaternion.identity,
                sequenceOrder = obstaclePlacements.Count + 1
            };

            obstaclePlacements.Add(placement);
            selectedObstacleIndex = obstaclePlacements.Count - 1;

            ValidateCourse();
        }

        #endregion

        #region Selected Obstacle Panel

        private void DrawSelectedObstaclePanel()
        {
            if (selectedObstacleIndex < 0 || selectedObstacleIndex >= obstaclePlacements.Count) return;

            var obstacle = obstaclePlacements[selectedObstacleIndex];

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField($"Obstacle {selectedObstacleIndex + 1}: {obstacle.obstacleType}", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();

            obstacle.obstacleType = (ObstacleType)EditorGUILayout.EnumPopup("Type", obstacle.obstacleType);

            EditorGUILayout.LabelField("Position");
            obstacle.position = EditorGUILayout.Vector3Field("", obstacle.position);

            EditorGUILayout.LabelField("Rotation (Euler)");
            Vector3 euler = obstacle.rotation.eulerAngles;
            euler = EditorGUILayout.Vector3Field("", euler);
            obstacle.rotation = Quaternion.Euler(euler);

            obstacle.sequenceOrder = EditorGUILayout.IntField("Sequence Order", obstacle.sequenceOrder);

            if (EditorGUI.EndChangeCheck())
            {
                obstaclePlacements[selectedObstacleIndex] = obstacle;
                ValidateCourse();
            }

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Delete", GUILayout.Width(60)))
            {
                obstaclePlacements.RemoveAt(selectedObstacleIndex);
                selectedObstacleIndex = -1;
                ValidateCourse();
            }

            if (GUILayout.Button("Duplicate", GUILayout.Width(80)))
            {
                var duplicate = obstacle.Clone();
                duplicate.sequenceOrder = obstaclePlacements.Count + 1;
                obstaclePlacements.Add(duplicate);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        #endregion

        #region Course Sequence

        private void DrawCourseSequence()
        {
            EditorGUILayout.LabelField("Course Sequence", EditorStyles.boldLabel);

            if (obstaclePlacements.Count == 0)
            {
                EditorGUILayout.HelpBox("No obstacles in course. Add obstacles from the palette above.", MessageType.Info);
                return;
            }

            // Sort by sequence order for display
            var sorted = obstaclePlacements.OrderBy(o => o.sequenceOrder).ToList();

            for (int i = 0; i < sorted.Count; i++)
            {
                var obstacle = sorted[i];
                int originalIndex = obstaclePlacements.IndexOf(obstacle);

                EditorGUILayout.BeginHorizontal();

                // Highlight if selected
                if (originalIndex == selectedObstacleIndex)
                {
                    GUI.backgroundColor = Color.cyan;
                }

                if (GUILayout.Button($"{i + 1}. {obstacle.obstacleType}", EditorStyles.miniButton, GUILayout.Width(200)))
                {
                    selectedObstacleIndex = originalIndex;
                }

                GUI.backgroundColor = Color.white;

                // Move up/down buttons
                if (GUILayout.Button("↑", GUILayout.Width(25)) && i > 0)
                {
                    SwapSequenceOrder(sorted[i], sorted[i - 1]);
                }

                if (GUILayout.Button("↓", GUILayout.Width(25)) && i < sorted.Count - 1)
                {
                    SwapSequenceOrder(sorted[i], sorted[i + 1]);
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        private void SwapSequenceOrder(ObstaclePlacement a, ObstaclePlacement b)
        {
            int temp = a.sequenceOrder;
            a.sequenceOrder = b.sequenceOrder;
            b.sequenceOrder = temp;
        }

        #endregion

        #region Validation

        private void DrawValidationPanel()
        {
            showValidationPanel = EditorGUILayout.Foldout(showValidationPanel, "Validation", true);
            if (!showValidationPanel) return;

            if (GUILayout.Button("Validate Course"))
            {
                ValidateCourse();
            }

            if (validationIssues.Count == 0)
            {
                EditorGUILayout.HelpBox("Course is valid!", MessageType.Info);
            }
            else
            {
                foreach (var issue in validationIssues)
                {
                    MessageType messageType = issue.severity switch
                    {
                        ValidationSeverity.Error => MessageType.Error,
                        ValidationSeverity.Warning => MessageType.Warning,
                        _ => MessageType.Info
                    };

                    EditorGUILayout.HelpBox($"[{issue.severity}] {issue.message}", messageType);
                }
            }
        }

        private void ValidateCourse()
        {
            validationIssues.Clear();

            // Check minimum obstacles
            if (obstaclePlacements.Count < 5)
            {
                validationIssues.Add(new ValidationIssue
                {
                    severity = ValidationSeverity.Warning,
                    message = "Course has fewer than 5 obstacles. Minimum recommended is 5."
                });
            }

            // Check for required obstacles based on course type
            if (courseType == CourseType.Standard)
            {
                bool hasContactObstacle = obstaclePlacements.Any(o => 
                    o.obstacleType == ObstacleType.AFrame || 
                    o.obstacleType == ObstacleType.DogWalk || 
                    o.obstacleType == ObstacleType.Teeter);

                if (!hasContactObstacle)
                {
                    validationIssues.Add(new ValidationIssue
                    {
                        severity = ValidationSeverity.Warning,
                        message = "Standard course should include at least one contact obstacle (A-Frame, Dog Walk, or Teeter)."
                    });
                }

                bool hasWeaves = obstaclePlacements.Any(o => o.obstacleType == ObstacleType.WeavePoles);
                if (!hasWeaves)
                {
                    validationIssues.Add(new ValidationIssue
                    {
                        severity = ValidationSeverity.Info,
                        message = "Consider adding weave poles for variety."
                    });
                }
            }
            else if (courseType == CourseType.JumpersWithWeaves)
            {
                // Check for excluded obstacles
                var excluded = obstaclePlacements.Where(o => 
                    o.obstacleType == ObstacleType.AFrame || 
                    o.obstacleType == ObstacleType.DogWalk || 
                    o.obstacleType == ObstacleType.Teeter || 
                    o.obstacleType == ObstacleType.PauseTable).ToList();

                foreach (var obstacle in excluded)
                {
                    validationIssues.Add(new ValidationIssue
                    {
                        severity = ValidationSeverity.Error,
                        message = $"{obstacle.obstacleType} is not allowed in Jumpers With Weaves courses."
                    });
                }
            }

            // Check sequence order
            var orders = obstaclePlacements.Select(o => o.sequenceOrder).ToList();
            if (orders.Distinct().Count() != orders.Count)
            {
                validationIssues.Add(new ValidationIssue
                {
                    severity = ValidationSeverity.Error,
                    message = "Duplicate sequence orders detected."
                });
            }

            // Check for runs (adjacent jumps)
            int jumpCount = 0;
            var sorted = obstaclePlacements.OrderBy(o => o.sequenceOrder).ToList();
            for (int i = 0; i < sorted.Count; i++)
            {
                if (IsJump(sorted[i].obstacleType))
                {
                    jumpCount++;
                    if (jumpCount >= 3)
                    {
                        validationIssues.Add(new ValidationIssue
                        {
                            severity = ValidationSeverity.Warning,
                            message = $"Jump run of {jumpCount} detected. Consider breaking up with other obstacles."
                        });
                    }
                }
                else
                {
                    jumpCount = 0;
                }
            }
        }

        private bool IsJump(ObstacleType type)
        {
            return type == ObstacleType.BarJump || 
                   type == ObstacleType.TireJump || 
                   type == ObstacleType.BroadJump || 
                   type == ObstacleType.WallJump;
        }

        #endregion

        #region Command Timing Visualization

        private bool showTimingPanel = true;
        private float timingVisualizationScale = 1f;
        private Color earlyCommandColor = Color.red;
        private Color optimalCommandColor = Color.green;
        private Color lateCommandColor = new Color(1f, 0.5f, 0f); // Orange

        private void DrawCommandTimingPanel()
        {
            showTimingPanel = EditorGUILayout.Foldout(showTimingPanel, "Command Timing Visualization", true);
            if (!showTimingPanel) return;

            EditorGUI.indentLevel++;

            timingVisualizationScale = EditorGUILayout.Slider("Scale", timingVisualizationScale, 0.5f, 2f);

            EditorGUILayout.BeginHorizontal();
            earlyCommandColor = EditorGUILayout.ColorField("Early", earlyCommandColor);
            optimalCommandColor = EditorGUILayout.ColorField("Optimal", optimalCommandColor);
            lateCommandColor = EditorGUILayout.ColorField("Late", lateCommandColor);
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Generate Timing Chart"))
            {
                GenerateTimingChart();
            }

            EditorGUILayout.HelpBox("Command timing shows optimal points to issue commands for each obstacle based on dog speed and approach angle.", MessageType.Info);

            EditorGUI.indentLevel--;
        }

        private void GenerateTimingChart()
        {
            if (obstaclePlacements.Count == 0)
            {
                EditorUtility.DisplayDialog("No Obstacles", "Add obstacles to the course first.", "OK");
                return;
            }

            // Calculate timing for each obstacle
            var timingData = new List<ObstacleTimingData>();
            float currentTime = 0f;
            float dogSpeed = 7.5f; // Default speed in m/s

            for (int i = 0; i < obstaclePlacements.Count; i++)
            {
                var obstacle = obstaclePlacements[i];
                
                // Calculate approach distance (distance from previous obstacle or start)
                float approachDistance = 5f; // Default
                if (i > 0)
                {
                    approachDistance = Vector3.Distance(obstacle.position, obstaclePlacements[i - 1].position);
                }

                // Calculate timing windows based on obstacle type
                var timing = CalculateObstacleTiming(obstacle.obstacleType, approachDistance, dogSpeed);
                timing.obstacleIndex = i;
                timing.obstacleType = obstacle.obstacleType;
                timing.position = obstacle.position;
                timing.estimatedArrivalTime = currentTime + (approachDistance / dogSpeed);
                
                timingData.Add(timing);
                
                // Estimate exit time
                currentTime = timing.estimatedArrivalTime + timing.expectedDuration;
            }

            // Display timing visualization in Scene view or create a chart window
            DisplayTimingChart(timingData);

            Debug.Log($"Generated timing chart for {timingData.Count} obstacles. Total estimated time: {currentTime:F2}s");
        }

        private ObstacleTimingData CalculateObstacleTiming(ObstacleType type, float approachDistance, float dogSpeed)
        {
            // Timing windows in seconds
            float earlyWindow = 0f;
            float optimalWindow = 0f;
            float lateWindow = 0f;
            float expectedDuration = 0f;

            switch (type)
            {
                case ObstacleType.BarJump:
                    earlyWindow = 0.3f;
                    optimalWindow = 0.1f;
                    lateWindow = 0.2f;
                    expectedDuration = 0.5f;
                    break;
                case ObstacleType.TireJump:
                    earlyWindow = 0.4f;
                    optimalWindow = 0.15f;
                    lateWindow = 0.25f;
                    expectedDuration = 0.6f;
                    break;
                case ObstacleType.BroadJump:
                    earlyWindow = 0.5f;
                    optimalWindow = 0.2f;
                    lateWindow = 0.3f;
                    expectedDuration = 0.8f;
                    break;
                case ObstacleType.WallJump:
                    earlyWindow = 0.5f;
                    optimalWindow = 0.2f;
                    lateWindow = 0.3f;
                    expectedDuration = 0.7f;
                    break;
                case ObstacleType.AFrame:
                    earlyWindow = 0.6f;
                    optimalWindow = 0.3f;
                    lateWindow = 0.4f;
                    expectedDuration = 1.2f;
                    break;
                case ObstacleType.DogWalk:
                    earlyWindow = 0.8f;
                    optimalWindow = 0.4f;
                    lateWindow = 0.5f;
                    expectedDuration = 2.0f;
                    break;
                case ObstacleType.Teeter:
                    earlyWindow = 0.7f;
                    optimalWindow = 0.3f;
                    lateWindow = 0.4f;
                    expectedDuration = 1.5f;
                    break;
                case ObstacleType.WeavePoles:
                    earlyWindow = 0.4f;
                    optimalWindow = 0.2f;
                    lateWindow = 0.3f;
                    expectedDuration = 3.0f;
                    break;
                case ObstacleType.PauseTable:
                    earlyWindow = 0.3f;
                    optimalWindow = 0.1f;
                    lateWindow = 0.2f;
                    expectedDuration = 5.0f; // Required pause time
                    break;
                default:
                    earlyWindow = 0.3f;
                    optimalWindow = 0.1f;
                    lateWindow = 0.2f;
                    expectedDuration = 0.5f;
                    break;
            }

            // Adjust based on approach distance
            float distanceFactor = Mathf.Clamp01(approachDistance / 10f);
            earlyWindow *= (1f + distanceFactor);
            lateWindow *= (1f + distanceFactor);

            return new ObstacleTimingData
            {
                earlyCommandWindow = earlyWindow,
                optimalCommandWindow = optimalWindow,
                lateCommandWindow = lateWindow,
                expectedDuration = expectedDuration,
                commandPoint = approachDistance / dogSpeed // Time to reach command point
            };
        }

        private void DisplayTimingChart(List<ObstacleTimingData> timingData)
        {
            // Create a simple editor window for timing visualization
            CourseTimingWindow.ShowTiming(timingData, obstaclePlacements);
        }

        #endregion

        #region Actions

        private void DrawActions()
        {
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Clear All Obstacles"))
            {
                if (EditorUtility.DisplayDialog("Clear Obstacles", "Are you sure you want to clear all obstacles?", "Yes", "No"))
                {
                    obstaclePlacements.Clear();
                    selectedObstacleIndex = -1;
                    validationIssues.Clear();
                }
            }

            if (GUILayout.Button("Auto-Layout"))
            {
                AutoLayoutObstacles();
            }

            if (GUILayout.Button("Export to JSON"))
            {
                ExportToJson();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void AutoLayoutObstacles()
        {
            float spacing = 5f;
            for (int i = 0; i < obstaclePlacements.Count; i++)
            {
                var obstacle = obstaclePlacements[i];
                obstacle.position = new Vector3(i * spacing, 0, 0);
                obstaclePlacements[i] = obstacle;
            }
        }

        private void ExportToJson()
        {
            string path = EditorUtility.SaveFilePanel("Export Course", "", courseName, "json");
            if (!string.IsNullOrEmpty(path))
            {
                var exportData = new CourseExportData
                {
                    name = courseName,
                    courseType = courseType,
                    obstacles = obstaclePlacements.ToArray()
                };

                string json = JsonUtility.ToJson(exportData, true);
                System.IO.File.WriteAllText(path, json);
                Debug.Log($"Course exported to: {path}");
            }
        }

        #endregion

        #region Course Management

        private void CreateNewCourse()
        {
            courseName = "New Course";
            courseType = CourseType.Standard;
            obstaclePlacements.Clear();
            selectedObstacleIndex = -1;
            validationIssues.Clear();
        }

        private void LoadCourse()
        {
            string path = EditorUtility.OpenFilePanel("Load Course", "", "asset");
            if (!string.IsNullOrEmpty(path))
            {
                // Convert absolute path to project-relative path
                if (path.StartsWith(Application.dataPath))
                {
                    path = "Assets" + path.Substring(Application.dataPath.Length);
                }

                currentCourse = AssetDatabase.LoadAssetAtPath<CourseDefinition>(path);
                if (currentCourse != null)
                {
                    courseName = currentCourse.courseName;
                    courseType = currentCourse.courseType;
                    // Would load obstacle placements from course definition
                    Debug.Log($"Loaded course: {courseName}");
                }
            }
        }

        private void SaveCourse()
        {
            if (currentCourse == null)
            {
                string path = EditorUtility.SaveFilePanel("Save Course", "Assets/Data/Courses", courseName, "asset");
                if (!string.IsNullOrEmpty(path))
                {
                    if (path.StartsWith(Application.dataPath))
                    {
                        path = "Assets" + path.Substring(Application.dataPath.Length);
                    }

                    currentCourse = ScriptableObject.CreateInstance<CourseDefinition>();
                    currentCourse.courseName = courseName;
                    currentCourse.courseType = courseType;

                    AssetDatabase.CreateAsset(currentCourse, path);
                    AssetDatabase.SaveAssets();
                }
            }

            if (currentCourse != null)
            {
                EditorUtility.SetDirty(currentCourse);
                AssetDatabase.SaveAssets();
                Debug.Log($"Course saved: {courseName}");
            }
        }

        #endregion

        #region Helpers

        private void CreateGridTexture()
        {
            gridTexture = new Texture2D(1, 1);
            gridTexture.SetPixel(0, 0, new Color(0.5f, 0.5f, 0.5f, 0.3f));
            gridTexture.Apply();
        }

        private void SwapObstacles(int index1, int index2)
        {
            if (index1 < 0 || index1 >= obstaclePlacements.Count) return;
            if (index2 < 0 || index2 >= obstaclePlacements.Count) return;

            var temp = obstaclePlacements[index1];
            obstaclePlacements[index1] = obstaclePlacements[index2];
            obstaclePlacements[index2] = temp;
        }

        #endregion
    }

    [System.Serializable]
    public class ObstaclePlacement
    {
        public ObstacleType obstacleType;
        public Vector3 position;
        public Quaternion rotation;
        public int sequenceOrder;

        public ObstaclePlacement Clone()
        {
            return new ObstaclePlacement
            {
                obstacleType = obstacleType,
                position = position,
                rotation = rotation,
                sequenceOrder = sequenceOrder
            };
        }
    }

    [System.Serializable]
    public class CourseExportData
    {
        public string name;
        public CourseType courseType;
        public ObstaclePlacement[] obstacles;
    }

    [System.Serializable]
    public class ValidationIssue
    {
        public ValidationSeverity severity;
        public string message;
    }

    public enum ValidationSeverity
    {
        Info,
        Warning,
        Error
    }

    [System.Serializable]
    public class ObstacleTimingData
    {
        public int obstacleIndex;
        public ObstacleType obstacleType;
        public Vector3 position;
        public float estimatedArrivalTime;
        public float earlyCommandWindow;
        public float optimalCommandWindow;
        public float lateCommandWindow;
        public float expectedDuration;
        public float commandPoint; // Time to issue command

        public float OptimalCommandTime => estimatedArrivalTime - commandPoint;
        public float EarlyCommandTime => OptimalCommandTime - earlyCommandWindow;
        public float LateCommandTime => OptimalCommandTime + lateCommandWindow;
    }
}
#endif
