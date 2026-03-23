#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using AgilityDogs.Core;
using AgilityDogs.Data;

namespace AgilityDogs.Editor
{
    /// <summary>
    /// Editor utility for generating game data assets.
    /// Creates venues, handlers, and courses for the game.
    /// </summary>
    public class DataPipelineEditor : EditorWindow
    {
        // Venue settings
        private string venueName = "New Venue";
        private string venueCity = "City";
        private string venueState = "State";
        private WeatherType defaultWeather = WeatherType.Clear;
        private bool isChampionshipVenue = false;
        private int venueDifficulty = 5;
        private int venuePrestige = 5;

        // Handler settings
        private string handlerName = "New Handler";
        private int handlerSpeed = 5;
        private int handlerPrecision = 5;
        private int handlerCourseReading = 5;
        private int handlerPressure = 5;

        // Course settings
        private string courseName = "New Course";
        private CourseType courseType = CourseType.Standard;
        private float standardTime = 45f;
        private int courseDifficulty = 5;
        private int obstacleCount = 12;

        // Output folder
        private string outputFolder = "Assets/Data";

        [MenuItem("Agility Dogs/Data Pipeline")]
        public static void ShowWindow()
        {
            GetWindow<DataPipelineEditor>("Data Pipeline");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Data Pipeline Editor", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            outputFolder = EditorGUILayout.TextField("Output Folder", outputFolder);

            if (GUILayout.Button("Create Output Folder"))
            {
                CreateOutputFolder();
            }

            EditorGUILayout.Space(10);
            DrawVenueSection();
            EditorGUILayout.Space(10);
            DrawHandlerSection();
            EditorGUILayout.Space(10);
            DrawCourseSection();
            EditorGUILayout.Space(10);
            DrawBatchOperations();
        }

        #region Venue Section

        private void DrawVenueSection()
        {
            EditorGUILayout.LabelField("Venue Generator", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            venueName = EditorGUILayout.TextField("Venue Name", venueName);
            venueCity = EditorGUILayout.TextField("City", venueCity);
            venueState = EditorGUILayout.TextField("State", venueState);
            defaultWeather = (WeatherType)EditorGUILayout.EnumPopup("Default Weather", defaultWeather);
            isChampionshipVenue = EditorGUILayout.Toggle("Championship Venue", isChampionshipVenue);
            venueDifficulty = EditorGUILayout.IntSlider("Difficulty", venueDifficulty, 1, 10);
            venuePrestige = EditorGUILayout.IntSlider("Prestige", venuePrestige, 1, 10);

            if (GUILayout.Button("Create Venue"))
            {
                CreateVenue();
            }

            EditorGUILayout.EndVertical();
        }

        private void CreateVenue()
        {
            if (string.IsNullOrEmpty(venueName))
            {
                EditorUtility.DisplayDialog("Error", "Please enter a venue name.", "OK");
                return;
            }

            var venue = ScriptableObject.CreateInstance<VenueData>();
            venue.venueId = venueName.ToLower().Replace(" ", "_");
            venue.venueName = venueName;
            venue.displayName = venueName;
            venue.city = venueCity;
            venue.state = venueState;
            venue.country = "USA";
            venue.defaultWeather = defaultWeather;
            venue.isChampionshipVenue = isChampionshipVenue;
            venue.difficultyRating = venueDifficulty;
            venue.prestigeLevel = venuePrestige;
            venue.description = $"A {(isChampionshipVenue ? "championship-level" : "competitive")} agility venue in {venueCity}, {venueState}.";

            string path = Path.Combine(outputFolder, "Venues");
            EnsureDirectoryExists(path);
            
            string assetPath = Path.Combine(path, $"{venueName}_VenueData.asset");
            AssetDatabase.CreateAsset(venue, assetPath);
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = venue;
            
            Debug.Log($"Created venue: {assetPath}");
        }

        #endregion

        #region Handler Section

        private void DrawHandlerSection()
        {
            EditorGUILayout.LabelField("Handler Generator", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            handlerName = EditorGUILayout.TextField("Handler Name", handlerName);
            handlerSpeed = EditorGUILayout.IntSlider("Speed Stat", handlerSpeed, 1, 10);
            handlerPrecision = EditorGUILayout.IntSlider("Precision Stat", handlerPrecision, 1, 10);
            handlerCourseReading = EditorGUILayout.IntSlider("Course Reading", handlerCourseReading, 1, 10);
            handlerPressure = EditorGUILayout.IntSlider("Pressure Handling", handlerPressure, 1, 10);

            if (GUILayout.Button("Create Handler"))
            {
                CreateHandler();
            }

            EditorGUILayout.EndVertical();
        }

        private void CreateHandler()
        {
            if (string.IsNullOrEmpty(handlerName))
            {
                EditorUtility.DisplayDialog("Error", "Please enter a handler name.", "OK");
                return;
            }

            var handler = ScriptableObject.CreateInstance<HandlerData>();
            handler.handlerName = handlerName.ToLower().Replace(" ", "_");
            handler.displayName = handlerName;
            handler.speedStat = handlerSpeed;
            handler.commandPrecisionStat = handlerPrecision;
            handler.courseReadingStat = handlerCourseReading;
            handler.pressureHandlingStat = handlerPressure;
            handler.backstory = $"{handlerName} is an experienced agility handler known for their {(handlerSpeed >= 7 ? "speed" : handlerPrecision >= 7 ? "precision" : "balanced")} approach.";
            handler.handlerSpeed = 4f + (handlerSpeed * 0.5f);
            handler.handlerSprintSpeed = 6f + (handlerSpeed * 0.6f);

            string path = Path.Combine(outputFolder, "Handlers");
            EnsureDirectoryExists(path);
            
            string assetPath = Path.Combine(path, $"{handlerName}_HandlerData.asset");
            AssetDatabase.CreateAsset(handler, assetPath);
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = handler;
            
            Debug.Log($"Created handler: {assetPath}");
        }

        #endregion

        #region Course Section

        private void DrawCourseSection()
        {
            EditorGUILayout.LabelField("Course Generator", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            courseName = EditorGUILayout.TextField("Course Name", courseName);
            courseType = (CourseType)EditorGUILayout.EnumPopup("Course Type", courseType);
            standardTime = EditorGUILayout.FloatField("Standard Time (s)", standardTime);
            courseDifficulty = EditorGUILayout.IntSlider("Difficulty", courseDifficulty, 1, 10);
            obstacleCount = EditorGUILayout.IntSlider("Obstacle Count", obstacleCount, 8, 20);

            if (GUILayout.Button("Create Course"))
            {
                CreateCourse();
            }

            if (GUILayout.Button("Generate Random Course"))
            {
                GenerateRandomCourse();
            }

            EditorGUILayout.EndVertical();
        }

        private void CreateCourse()
        {
            if (string.IsNullOrEmpty(courseName))
            {
                EditorUtility.DisplayDialog("Error", "Please enter a course name.", "OK");
                return;
            }

            var course = ScriptableObject.CreateInstance<CourseDefinition>();
            course.courseName = courseName;
            course.courseType = courseType;
            course.standardTime = standardTime;
            course.maximumTime = standardTime + 15f;
            course.difficultyRating = courseDifficulty;
            course.venueName = "Any";

            // Generate obstacles based on course type
            course.obstacleSequence = GenerateObstacleSequence(courseType, obstacleCount);

            string path = Path.Combine(outputFolder, "Courses");
            EnsureDirectoryExists(path);
            
            string assetPath = Path.Combine(path, $"{courseName}_CourseDefinition.asset");
            AssetDatabase.CreateAsset(course, assetPath);
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = course;
            
            Debug.Log($"Created course: {assetPath} with {course.obstacleSequence.Length} obstacles");
        }

        private void GenerateRandomCourse()
        {
            courseName = $"Random_{courseType}_{System.DateTime.Now:HHmmss}";
            CreateCourse();
        }

        private ObstacleData[] GenerateObstacleSequence(CourseType type, int count)
        {
            var obstacles = new List<ObstacleData>();

            // Define available obstacles based on course type
            List<ObstacleType> availableObstacles = new List<ObstacleType>();
            
            // Always include jumps
            availableObstacles.Add(ObstacleType.BarJump);
            availableObstacles.Add(ObstacleType.TireJump);
            availableObstacles.Add(ObstacleType.BroadJump);
            availableObstacles.Add(ObstacleType.WallJump);
            availableObstacles.Add(ObstacleType.WeavePoles);

            if (type == CourseType.Standard)
            {
                // Standard courses include contact obstacles
                availableObstacles.Add(ObstacleType.AFrame);
                availableObstacles.Add(ObstacleType.DogWalk);
                availableObstacles.Add(ObstacleType.Teeter);
                availableObstacles.Add(ObstacleType.PauseTable);
            }

            // Generate sequence with some variety
            int jumpCount = 0;
            for (int i = 0; i < count; i++)
            {
                ObstacleType type2;
                
                // Limit consecutive jumps
                if (jumpCount >= 3)
                {
                    // Force non-jump obstacle
                    var nonJumps = availableObstacles.FindAll(o => !IsJump(o));
                    type2 = nonJumps[Random.Range(0, nonJumps.Count)];
                    jumpCount = 0;
                }
                else
                {
                    type2 = availableObstacles[Random.Range(0, availableObstacles.Count)];
                    if (IsJump(type2))
                        jumpCount++;
                    else
                        jumpCount = 0;
                }

                var obstacle = ScriptableObject.CreateInstance<ObstacleData>();
                obstacle.obstacleType = type2;
                obstacle.sequenceOrder = i + 1;
                obstacles.Add(obstacle);
            }

            return obstacles.ToArray();
        }

        private bool IsJump(ObstacleType type)
        {
            return type == ObstacleType.BarJump || 
                   type == ObstacleType.TireJump || 
                   type == ObstacleType.BroadJump || 
                   type == ObstacleType.WallJump;
        }

        #endregion

        #region Batch Operations

        private void DrawBatchOperations()
        {
            EditorGUILayout.LabelField("Batch Operations", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            if (GUILayout.Button("Create Sample Venues"))
            {
                CreateSampleVenues();
            }

            if (GUILayout.Button("Create Sample Handlers"))
            {
                CreateSampleHandlers();
            }

            if (GUILayout.Button("Create Sample Courses"))
            {
                CreateSampleCourses();
            }

            if (GUILayout.Button("Create All Sample Data"))
            {
                CreateSampleVenues();
                CreateSampleHandlers();
                CreateSampleCourses();
            }

            EditorGUILayout.EndVertical();
        }

        private void CreateSampleVenues()
        {
            var sampleVenues = new[]
            {
                new { Name = "Westminster Agility Arena", City = "New York", State = "NY", Championship = true, Prestige = 10 },
                new { Name = "Pacific Agility Center", City = "San Diego", State = "CA", Championship = false, Prestige = 7 },
                new { Name = "Heartland Agility Park", City = "Omaha", State = "NE", Championship = false, Prestige = 5 },
                new { Name = "Southern Agility Complex", City = "Atlanta", State = "GA", Championship = true, Prestige = 8 },
                new { Name = "Mountain View Agility Field", City = "Denver", State = "CO", Championship = false, Prestige = 6 }
            };

            foreach (var v in sampleVenues)
            {
                venueName = v.Name;
                venueCity = v.City;
                venueState = v.State;
                isChampionshipVenue = v.Championship;
                venuePrestige = v.Prestige;
                CreateVenue();
            }
        }

        private void CreateSampleHandlers()
        {
            var sampleHandlers = new[]
            {
                new { Name = "Alex Thompson", Speed = 7, Precision = 6, CourseReading = 7, Pressure = 5 },
                new { Name = "Jordan Williams", Speed = 5, Precision = 8, CourseReading = 6, Pressure = 7 },
                new { Name = "Casey Rodriguez", Speed = 6, Precision = 7, CourseReading = 8, Pressure = 6 },
                new { Name = "Morgan Chen", Speed = 8, Precision = 5, CourseReading = 5, Pressure = 8 },
                new { Name = "Riley Johnson", Speed = 6, Precision = 6, CourseReading = 7, Pressure = 7 }
            };

            foreach (var h in sampleHandlers)
            {
                handlerName = h.Name;
                handlerSpeed = h.Speed;
                handlerPrecision = h.Precision;
                handlerCourseReading = h.CourseReading;
                handlerPressure = h.Pressure;
                CreateHandler();
            }
        }

        private void CreateSampleCourses()
        {
            var sampleCourses = new[]
            {
                new { Name = "Beginner Standard", Type = CourseType.Standard, Time = 60f, Difficulty = 3, Obstacles = 10 },
                new { Name = "Intermediate Jumpers", Type = CourseType.JumpersWithWeaves, Time = 45f, Difficulty = 5, Obstacles = 12 },
                new { Name = "Advanced Standard", Type = CourseType.Standard, Time = 40f, Difficulty = 8, Obstacles = 16 },
                new { Name = "Championship Course", Type = CourseType.Championship, Time = 35f, Difficulty = 10, Obstacles = 18 }
            };

            foreach (var c in sampleCourses)
            {
                courseName = c.Name;
                courseType = c.Type;
                standardTime = c.Time;
                courseDifficulty = c.Difficulty;
                obstacleCount = c.Obstacles;
                CreateCourse();
            }
        }

        #endregion

        #region Helpers

        private void CreateOutputFolder()
        {
            EnsureDirectoryExists(outputFolder);
            Debug.Log($"Output folder ready: {outputFolder}");
        }

        private void EnsureDirectoryExists(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string[] parts = path.Split('/');
                string currentPath = parts[0];
                
                for (int i = 1; i < parts.Length; i++)
                {
                    string nextPath = currentPath + "/" + parts[i];
                    if (!AssetDatabase.IsValidFolder(nextPath))
                    {
                        AssetDatabase.CreateFolder(currentPath, parts[i]);
                    }
                    currentPath = nextPath;
                }
            }
        }

        #endregion
    }
}
#endif