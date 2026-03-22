using UnityEngine;
using AgilityDogs.Core;
using AgilityDogs.Events;
using AgilityDogs.Services;
using AgilityDogs.Gameplay;
using AgilityDogs.Gameplay.Dog;
using AgilityDogs.Gameplay.Handler;
using AgilityDogs.Gameplay.Scoring;

namespace AgilityDogs.Services
{
    /// <summary>
    /// Dev-only component that auto-starts gameplay and shows a minimal HUD.
    /// Attach to any GameObject in the test scene.
    /// </summary>
    public class DevTestRunner : MonoBehaviour
    {
        [Header("Auto Start")]
        [SerializeField] private bool autoStartGameplay = true;
        [SerializeField] private float startDelay = 0.5f;
        [SerializeField] private bool skipCountdown = true;

        [Header("Debug Display")]
        [SerializeField] private bool showHUD = true;

        private AgilityScoringService scoringService;
        private DogAgentController dog;
        private CourseRunner courseRunner;
        private float startTimer;
        private bool hasStarted;
        private string lastEvent = "";
        private float lastEventTime;

        private void OnEnable()
        {
            GameEvents.OnGameStateChanged += OnStateChanged;
            GameEvents.OnCommandIssued += OnCommand;
            GameEvents.OnFaultCommitted += OnFault;
            GameEvents.OnObstacleCompleted += OnObstacle;
            GameEvents.OnRunCompleted += OnRunCompleted;
        }

        private void OnDisable()
        {
            GameEvents.OnGameStateChanged -= OnStateChanged;
            GameEvents.OnCommandIssued -= OnCommand;
            GameEvents.OnFaultCommitted -= OnFault;
            GameEvents.OnObstacleCompleted -= OnObstacle;
            GameEvents.OnRunCompleted -= OnRunCompleted;
        }

        private void Start()
        {
            scoringService = FindObjectOfType<AgilityScoringService>();
            dog = FindObjectOfType<DogAgentController>();
            courseRunner = FindObjectOfType<CourseRunner>();
            startTimer = startDelay;
        }

        private void Update()
        {
            if (autoStartGameplay && !hasStarted)
            {
                startTimer -= Time.deltaTime;
                if (startTimer <= 0f)
                {
                    hasStarted = true;
                    StartGameplay();
                }
            }

            // Keyboard shortcuts for testing
            if (Input.GetKeyDown(KeyCode.F5))
            {
                RestartRun();
            }
            if (Input.GetKeyDown(KeyCode.F1))
            {
                ToggleCameraMode();
            }
        }

        private void StartGameplay()
        {
            if (GameManager.Instance == null)
            {
                Debug.LogError("GameManager not found in scene!");
                return;
            }

            if (skipCountdown)
            {
                GameManager.Instance.BeginRun();
            }
            else
            {
                GameManager.Instance.StartCountdown();
                if (courseRunner != null)
                {
                    courseRunner.StartCountdown();
                }
            }

            Debug.Log("[DevTestRunner] Gameplay started. WASD=move, Shift=sprint, F5=restart, F1=camera");
        }

        private void RestartRun()
        {
            hasStarted = false;
            autoStartGameplay = true;
            startTimer = 0.2f;

            if (courseRunner != null)
            {
                courseRunner.RestartCourse();
            }

            // Reset positions
            var handler = FindObjectOfType<HandlerController>();
            if (handler != null)
            {
                handler.transform.position = new Vector3(0, 0, -5);
                handler.transform.rotation = Quaternion.identity;
            }
            if (dog != null)
            {
                dog.transform.position = new Vector3(1.5f, 0, -5);
                dog.transform.rotation = Quaternion.identity;
            }

            Debug.Log("[DevTestRunner] Run restarted");
        }

        private void ToggleCameraMode()
        {
            var cam = FindObjectOfType<Presentation.Camera.AgilityCameraController>();
            if (cam != null)
            {
                cam.ToggleMode();
                Debug.Log($"[DevTestRunner] Camera mode: {cam.CurrentMode}");
            }
        }

        private void OnStateChanged(GameState from, GameState to)
        {
            lastEvent = $"State: {from} -> {to}";
            lastEventTime = Time.time;
        }

        private void OnCommand(HandlerCommand cmd)
        {
            lastEvent = $"Command: {cmd}";
            lastEventTime = Time.time;
        }

        private void OnFault(FaultType fault, string obstacle)
        {
            lastEvent = $"FAULT: {fault} at {obstacle}";
            lastEventTime = Time.time;
        }

        private void OnObstacle(ObstacleType type, bool clean)
        {
            lastEvent = $"Obstacle: {type} (clean={clean})";
            lastEventTime = Time.time;
        }

        private void OnRunCompleted(RunResult result, float time, int faults)
        {
            lastEvent = $"RUN COMPLETE: {result} | {time:F2}s | {faults} faults";
            lastEventTime = Time.time;
        }

        private void OnGUI()
        {
            if (!showHUD) return;

            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = 14;

            GUILayout.BeginArea(new Rect(10, 10, 320, 200), boxStyle);

            // Game state
            string state = GameManager.Instance != null
                ? GameManager.Instance.CurrentState.ToString()
                : "No GameManager";
            GUILayout.Label($"State: {state}", labelStyle);

            // Timer and faults
            if (scoringService != null)
            {
                GUILayout.Label($"Time: {scoringService.CurrentTime:F2}s", labelStyle);
                GUILayout.Label($"Faults: {scoringService.FaultCount}", labelStyle);
            }

            // Dog state
            if (dog != null)
            {
                GUILayout.Label($"Dog: {dog.CurrentState} | Speed: {dog.Speed:F1}", labelStyle);
            }

            // Last event
            if (Time.time - lastEventTime < 3f && !string.IsNullOrEmpty(lastEvent))
            {
                GUILayout.Label(lastEvent, labelStyle);
            }

            GUILayout.Space(5);
            GUILayout.Label("F5=Restart  F1=Camera", labelStyle);

            GUILayout.EndArea();
        }
    }
}
