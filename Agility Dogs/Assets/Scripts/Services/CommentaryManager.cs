using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AgilityDogs.Core;
using AgilityDogs.Events;
using AgilityDogs.Gameplay.Dog;
using AgilityDogs.Gameplay.Obstacles;

namespace AgilityDogs.Services
{
    public enum AnnouncerType
    {
        Main,
        Color
    }

    public class CommentaryManager : MonoBehaviour
    {
        [Header("Eastworld Configuration")]
        [SerializeField] private string gameUuid = "agility-dogs-game";
        [SerializeField] private string mainAnnouncerAgentUuid = "main-announcer-agent";
        [SerializeField] private string colorCommentatorAgentUuid = "color-commentator-agent";
        
        [Header("References")]
        [SerializeField] private ElevenLabsService elevenLabsService;
        [SerializeField] private EastworldClient eastworldClient;
        [SerializeField] private AudioSource mainAnnouncerSource;
        [SerializeField] private AudioSource colorCommentatorSource;
        
        [Header("Settings")]
        [SerializeField] private bool enableCommentary = true;
        [SerializeField] private float commentaryCooldown = 2f;
        
        private string sessionUuid;
        private bool isSessionActive = false;
        private Queue<CommentaryTask> commentaryQueue = new Queue<CommentaryTask>();
        private bool isProcessingQueue = false;
        private float lastCommentaryTime;
        
        private class CommentaryTask
        {
            public AnnouncerType announcerType;
            public string eventDescription;
            public string context;
            public float timestamp;
        }
        
        private void Start()
        {
            // Find services on same GameObject first, then in scene
            if (elevenLabsService == null)
                elevenLabsService = GetComponent<ElevenLabsService>();
            if (elevenLabsService == null)
                elevenLabsService = FindObjectOfType<ElevenLabsService>();
            
            if (eastworldClient == null)
                eastworldClient = GetComponent<EastworldClient>();
            if (eastworldClient == null)
                eastworldClient = FindObjectOfType<EastworldClient>();
            
            // Ensure we have AudioSources
            if (mainAnnouncerSource == null || colorCommentatorSource == null)
            {
                AudioSource[] sources = GetComponents<AudioSource>();
                if (sources.Length >= 2)
                {
                    mainAnnouncerSource = sources[0];
                    colorCommentatorSource = sources[1];
                }
                else
                {
                    Debug.LogError("CommentaryManager requires two AudioSource components.");
                }
            }
            
            // Subscribe to game events
            SubscribeToEvents();
            
            // Start Eastworld session
            if (enableCommentary)
                StartCoroutine(StartEastworldSession());
        }
        
        private void SubscribeToEvents()
        {
            GameEvents.OnRunStarted += OnRunStarted;
            GameEvents.OnRunCompleted += OnRunCompleted;
            GameEvents.OnObstacleCompleted += OnObstacleCompleted;
            GameEvents.OnFaultCommitted += OnFaultCommitted;
            GameEvents.OnSplitTimeRecorded += OnSplitTimeRecorded;
            GameEvents.OnCountdownTick += OnCountdownTick;
            GameEvents.OnCourseLoaded += OnCourseLoaded;
        }
        
        private void OnDestroy()
        {
            GameEvents.OnRunStarted -= OnRunStarted;
            GameEvents.OnRunCompleted -= OnRunCompleted;
            GameEvents.OnObstacleCompleted -= OnObstacleCompleted;
            GameEvents.OnFaultCommitted -= OnFaultCommitted;
            GameEvents.OnSplitTimeRecorded -= OnSplitTimeRecorded;
            GameEvents.OnCountdownTick -= OnCountdownTick;
            GameEvents.OnCourseLoaded -= OnCourseLoaded;
        }
        
        private IEnumerator StartEastworldSession()
        {
            if (!enableCommentary) yield break;
            
            yield return eastworldClient.CreateSession(gameUuid, sessionJson =>
            {
                // Parse session UUID from response
                EastworldResponse response = JsonUtility.FromJson<EastworldResponse>(sessionJson);
                sessionUuid = response.session_uuid;
                isSessionActive = true;
                Debug.Log($"Eastworld session started: {sessionUuid}");
                
                // Start chats with both announcers
                StartCoroutine(eastworldClient.StartChat(sessionUuid, mainAnnouncerAgentUuid, "Main Announcer", null, error => Debug.LogError(error)));
                StartCoroutine(eastworldClient.StartChat(sessionUuid, colorCommentatorAgentUuid, "Color Commentator", null, error => Debug.LogError(error)));
            }, error =>
            {
                Debug.LogError($"Failed to start Eastworld session: {error}");
            });
        }
        
        private void OnRunStarted()
        {
            QueueCommentary(AnnouncerType.Main, "Run started", "The agility run has begun!");
        }
        
        private void OnRunCompleted(RunResult result, float time, int faults)
        {
            string resultText = result == RunResult.Qualified ? "qualified" : "did not qualify";
            QueueCommentary(AnnouncerType.Main, "Run completed", 
                $"The run is complete. The team {resultText} with a time of {time:F2} seconds and {faults} faults.");
            
            if (faults > 0)
            {
                QueueCommentary(AnnouncerType.Color, "Faults analysis", 
                    $"That's {faults} fault(s) on the course. The handler will need to work on precision.");
            }
        }
        
        private void OnObstacleCompleted(ObstacleType type, bool clean)
        {
            string obstacleName = type.ToString();
            if (clean)
            {
                QueueCommentary(AnnouncerType.Color, "Clean obstacle", 
                    $"Beautifully executed {obstacleName}! The dog made that look easy.");
            }
            else
            {
                QueueCommentary(AnnouncerType.Main, "Obstacle fault", 
                    $"A fault on the {obstacleName}. The dog knocked the bar or missed the contact zone.");
            }
        }
        
        private void OnFaultCommitted(FaultType fault, string obstacleName)
        {
            QueueCommentary(AnnouncerType.Main, "Fault committed", 
                $"Fault: {fault} on {obstacleName}. That's a costly mistake.");
        }
        
        private void OnSplitTimeRecorded(float time)
        {
            // Only comment on particularly fast or slow splits
            if (time < 2.0f)
            {
                QueueCommentary(AnnouncerType.Color, "Fast split", 
                    $"Incredible speed through that section! Split time of {time:F2} seconds.");
            }
            else if (time > 5.0f)
            {
                QueueCommentary(AnnouncerType.Color, "Slow split", 
                    $"They lost some momentum there. Split time of {time:F2} seconds.");
            }
        }
        
        private void OnCountdownTick()
        {
            // Optional: countdown commentary
        }
        
        private void OnCourseLoaded()
        {
            QueueCommentary(AnnouncerType.Main, "Course loaded", 
                "The course is set. We have a challenging sequence of obstacles ahead.");
        }
        
        private void QueueCommentary(AnnouncerType announcerType, string eventDescription, string context)
        {
            if (!enableCommentary || !isSessionActive) return;
            if (Time.time - lastCommentaryTime < commentaryCooldown) return;
            
            CommentaryTask task = new CommentaryTask
            {
                announcerType = announcerType,
                eventDescription = eventDescription,
                context = context,
                timestamp = Time.time
            };
            
            commentaryQueue.Enqueue(task);
            ProcessQueue();
        }
        
        private void ProcessQueue()
        {
            if (isProcessingQueue || commentaryQueue.Count == 0) return;
            
            isProcessingQueue = true;
            CommentaryTask task = commentaryQueue.Dequeue();
            
            StartCoroutine(GenerateAndPlayCommentary(task));
        }
        
        private IEnumerator GenerateAndPlayCommentary(CommentaryTask task)
        {
            // First, get commentary from Eastworld agent
            string agentUuid = task.announcerType == AnnouncerType.Main ? mainAnnouncerAgentUuid : colorCommentatorAgentUuid;
            string prompt = $"You are an announcer at an agility competition. {task.context}. Provide brief, enthusiastic commentary (1-2 sentences).";
            
            bool requestCompleted = false;
            string commentaryText = null;
            
            yield return eastworldClient.Interact(sessionUuid, agentUuid, prompt, response =>
            {
                commentaryText = response.content;
                requestCompleted = true;
            }, error =>
            {
                Debug.LogError($"Eastworld error: {error}");
                requestCompleted = true;
                // Fallback to context text
                commentaryText = task.context;
            });
            
            // Wait for request to complete
            while (!requestCompleted) yield return null;
            
            if (string.IsNullOrEmpty(commentaryText))
            {
                commentaryText = task.context;
            }
            
            // Generate speech with ElevenLabs
            AudioClip audioClip = null;
            bool audioLoaded = false;
            
            if (task.announcerType == AnnouncerType.Main)
            {
                yield return elevenLabsService.SpeakAsMainAnnouncer(commentaryText, clip =>
                {
                    audioClip = clip;
                    audioLoaded = true;
                });
            }
            else
            {
                yield return elevenLabsService.SpeakAsColorCommentator(commentaryText, clip =>
                {
                    audioClip = clip;
                    audioLoaded = true;
                });
            }
            
            // Wait for audio to load
            while (!audioLoaded) yield return null;
            
            // Play the audio
            if (audioClip != null)
            {
                AudioSource source = task.announcerType == AnnouncerType.Main ? mainAnnouncerSource : colorCommentatorSource;
                elevenLabsService.PlayAudioClip(audioClip, source);
                lastCommentaryTime = Time.time;
            }
            
            // Process next in queue after a short delay
            yield return new WaitForSeconds(0.5f);
            isProcessingQueue = false;
            ProcessQueue();
        }
        
        // Public methods to trigger commentary manually
        public void TriggerMainAnnouncerCommentary(string context)
        {
            QueueCommentary(AnnouncerType.Main, "Manual trigger", context);
        }
        
        public void TriggerColorCommentatorCommentary(string context)
        {
            QueueCommentary(AnnouncerType.Color, "Manual trigger", context);
        }
        
        // Test method
        public void TestCommentary()
        {
            TriggerMainAnnouncerCommentary("Welcome to the agility competition!");
            StartCoroutine(WaitAndTestColor());
        }
        
        private IEnumerator WaitAndTestColor()
        {
            yield return new WaitForSeconds(3f);
            TriggerColorCommentatorCommentary("The dog looks ready to run!");
        }
    }
}