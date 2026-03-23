using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        Color,
        PA
    }

    [System.Serializable]
    public class CommentaryLine
    {
        public string text;
        public float weight = 1f;
        public string[] tags;
        public AnnouncerType announcerType;
    }

    [System.Serializable]
    public class CommentaryCategory
    {
        public string name;
        public CommentaryLine[] lines;
        public float totalWeight;
    }

    public class CommentaryManager : MonoBehaviour
    {
        [Header("Eastworld Configuration")]
        [SerializeField] private string gameUuid = "agility-dogs-game";
        [SerializeField] private string mainAnnouncerAgentUuid = "main-announcer-agent";
        [SerializeField] private string colorCommentatorAgentUuid = "color-commentator-agent";
        [SerializeField] private string paAnnouncerAgentUuid = "pa-announcer-agent";
        
        [Header("Eastworld Agent Info")]
        [SerializeField] private string mainAnnouncerName = "Barkley Stevens";
        [SerializeField] private string colorCommentatorName = "Dana Woofington";
        [SerializeField] private string paAnnouncerName = "Patricia Woof";
        
        [Header("References")]
        [SerializeField] private ElevenLabsService elevenLabsService;
        [SerializeField] private EastworldClient eastworldClient;
        [SerializeField] private AudioSource mainAnnouncerSource;
        [SerializeField] private AudioSource colorCommentatorSource;
        [SerializeField] private AudioSource paAnnouncerSource;
        
        [Header("Settings")]
        [SerializeField] private bool enableCommentary = true;
        [SerializeField] private float commentaryCooldown = 2f;
        [SerializeField] private bool useAuthoredLines = true;
        [SerializeField] private float eastworldFallbackChance = 0.3f;
        
        [Header("Breed Callouts")]
        [SerializeField] private bool enableBreedCallouts = true;
        
        [Header("Pressure Escalation")]
        [SerializeField] private float championshipPressureThreshold = 0.8f;
        [SerializeField] private float pressureEscalationRate = 0.1f;
        
        [Header("Near Miss Settings")]
        [SerializeField] private float nearMissThreshold = 0.3f;
        [SerializeField] private float nearMissDistance = 0.5f;
        
        private string sessionUuid;
        private bool isSessionActive = false;
        private Queue<CommentaryTask> commentaryQueue = new Queue<CommentaryTask>();
        private bool isProcessingQueue = false;
        private float lastCommentaryTime;
        private DogAgentController dog;
        private Queue<string> recentCommentary = new Queue<string>();
        private const int MaxRecentCommentary = 5;
        
        // Pressure tracking
        private float currentPressure = 0f;
        private bool isChampionship = false;
        
        // Authored lines database
        private Dictionary<string, CommentaryCategory> authoredLines = new Dictionary<string, CommentaryCategory>();
        
        private class CommentaryTask
        {
            public AnnouncerType announcerType;
            public string eventDescription;
            public string context;
            public float timestamp;
            public string category;
            public string[] tags;
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
            if (mainAnnouncerSource == null || colorCommentatorSource == null || paAnnouncerSource == null)
            {
                AudioSource[] sources = GetComponents<AudioSource>();
                if (sources.Length >= 3)
                {
                    mainAnnouncerSource = sources[0];
                    colorCommentatorSource = sources[1];
                    paAnnouncerSource = sources[2];
                }
                else if (sources.Length >= 2)
                {
                    mainAnnouncerSource = sources[0];
                    colorCommentatorSource = sources[1];
                    // Create PA source if missing
                    if (paAnnouncerSource == null)
                    {
                        paAnnouncerSource = gameObject.AddComponent<AudioSource>();
                        paAnnouncerSource.spatialBlend = 1f; // 3D audio for PA
                    }
                }
                else
                {
                    Debug.LogError("CommentaryManager requires at least two AudioSource components.");
                }
            }
            
            // Find dog in scene
            dog = FindObjectOfType<DogAgentController>();
            if (dog == null)
                Debug.LogWarning("DogAgentController not found for breed callouts.");

            // Initialize authored lines database
            InitializeAuthoredLines();
            
            // Subscribe to game events
            SubscribeToEvents();
            
            // Start Eastworld session
            if (enableCommentary)
                StartCoroutine(StartEastworldSession());
        }
        
        private void InitializeAuthoredLines()
        {
            // Run start lines
            AddAuthoredLine("run_start", "The agility run has begun!", AnnouncerType.Main, 1f, new string[] { "start", "main" });
            AddAuthoredLine("run_start", "And they're off! Let's see what this team can do.", AnnouncerType.Main, 0.8f, new string[] { "start", "main" });
            AddAuthoredLine("run_start", "Here we go! The clock starts now.", AnnouncerType.Color, 0.9f, new string[] { "start", "color" });
            
            // Run completion lines
            AddAuthoredLine("run_complete_qualified", "What a run! They've qualified!", AnnouncerType.Main, 1f, new string[] { "complete", "qualified", "main" });
            AddAuthoredLine("run_complete_qualified", "Incredible performance! That's a qualifying run!", AnnouncerType.Color, 0.9f, new string[] { "complete", "qualified", "color" });
            AddAuthoredLine("run_complete_fault", "Too bad about those faults. The time was good though.", AnnouncerType.Color, 0.8f, new string[] { "complete", "fault", "color" });
            
            // Fault lines
            AddAuthoredLine("fault_jump", "Oh! A knocked bar on that jump.", AnnouncerType.Main, 0.9f, new string[] { "fault", "jump" });
            AddAuthoredLine("fault_weave", "Missed a weave pole entry there.", AnnouncerType.Color, 0.8f, new string[] { "fault", "weave" });
            AddAuthoredLine("fault_contact", "They didn't hit that contact zone properly.", AnnouncerType.Main, 0.7f, new string[] { "fault", "contact" });
            
            // Clean obstacle lines
            AddAuthoredLine("clean_jump", "Beautiful jump! Perfect form.", AnnouncerType.Color, 0.8f, new string[] { "clean", "jump" });
            AddAuthoredLine("clean_weave", "Look at those weave poles! Textbook execution.", AnnouncerType.Main, 0.9f, new string[] { "clean", "weave" });
            AddAuthoredLine("clean_tunnel", "Smooth through that tunnel.", AnnouncerType.Color, 0.7f, new string[] { "clean", "tunnel" });
            
            // Split time lines
            AddAuthoredLine("fast_split", "Incredible speed through that section!", AnnouncerType.Color, 0.9f, new string[] { "split", "fast" });
            AddAuthoredLine("fast_split", "They're really moving now!", AnnouncerType.Main, 0.8f, new string[] { "split", "fast" });
            AddAuthoredLine("slow_split", "Lost a bit of momentum there.", AnnouncerType.Color, 0.7f, new string[] { "split", "slow" });
            
            // Championship pressure lines
            AddAuthoredLine("pressure_high", "This is it! Championship on the line!", AnnouncerType.Main, 1f, new string[] { "pressure", "championship" });
            AddAuthoredLine("pressure_high", "The crowd is on their feet! This could be historic!", AnnouncerType.Color, 0.9f, new string[] { "pressure", "championship" });
            AddAuthoredLine("pressure_medium", "They need to keep this pace up.", AnnouncerType.Color, 0.8f, new string[] { "pressure", "medium" });
            
            // Near miss lines
            AddAuthoredLine("near_miss", "So close! Just barely missed that.", AnnouncerType.Color, 0.9f, new string[] { "near_miss" });
            AddAuthoredLine("near_miss", "Whew! That was a close one.", AnnouncerType.Main, 0.8f, new string[] { "near_miss" });
            
            // PA announcer lines
            AddAuthoredLine("pa_announcement", "Ladies and gentlemen, please welcome our next competitor!", AnnouncerType.PA, 1f, new string[] { "pa", "intro" });
            AddAuthoredLine("pa_announcement", "The course is now clear for the next run.", AnnouncerType.PA, 0.9f, new string[] { "pa", "clear" });
            AddAuthoredLine("pa_announcement", "Please give a round of applause for our competitor!", AnnouncerType.PA, 0.8f, new string[] { "pa", "applause" });
            
            // Calculate total weights for each category
            foreach (var category in authoredLines.Values)
            {
                category.totalWeight = category.lines.Sum(l => l.weight);
            }
        }
        
        private void AddAuthoredLine(string category, string text, AnnouncerType announcerType, float weight, string[] tags)
        {
            if (!authoredLines.ContainsKey(category))
            {
                authoredLines[category] = new CommentaryCategory { name = category, lines = new List<CommentaryLine>().ToArray() };
            }
            
            var categoryData = authoredLines[category];
            var linesList = categoryData.lines.ToList();
            linesList.Add(new CommentaryLine 
            { 
                text = text, 
                weight = weight, 
                tags = tags, 
                announcerType = announcerType 
            });
            categoryData.lines = linesList.ToArray();
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
                
                // Start chats with all announcers
                StartCoroutine(eastworldClient.StartChat(sessionUuid, mainAnnouncerAgentUuid, mainAnnouncerName, null, error => Debug.LogError(error)));
                StartCoroutine(eastworldClient.StartChat(sessionUuid, colorCommentatorAgentUuid, colorCommentatorName, null, error => Debug.LogError(error)));
                StartCoroutine(eastworldClient.StartChat(sessionUuid, paAnnouncerAgentUuid, paAnnouncerName, null, error => Debug.LogError(error)));
            }, error =>
            {
                Debug.LogError($"Failed to start Eastworld session: {error}");
            });
        }
        
        private CommentaryLine SelectWeightedRandomLine(string category, string[] requiredTags = null)
        {
            if (!authoredLines.ContainsKey(category)) return null;
            
            var categoryData = authoredLines[category];
            List<CommentaryLine> validLines = new List<CommentaryLine>();
            
            foreach (var line in categoryData.lines)
            {
                // Check if line is not in recent commentary
                if (recentCommentary.Contains(line.text)) continue;
                
                // Check if line has required tags (if specified)
                if (requiredTags != null && requiredTags.Length > 0)
                {
                    bool hasAllTags = requiredTags.All(tag => line.tags.Contains(tag));
                    if (!hasAllTags) continue;
                }
                
                validLines.Add(line);
            }
            
            if (validLines.Count == 0) return null;
            
            // Weighted random selection
            float totalWeight = validLines.Sum(l => l.weight);
            float randomValue = Random.Range(0f, totalWeight);
            float currentWeight = 0f;
            
            foreach (var line in validLines)
            {
                currentWeight += line.weight;
                if (randomValue <= currentWeight)
                {
                    return line;
                }
            }
            
            return validLines.Last(); // Fallback
        }
        
        private void AddToRecentCommentary(string text)
        {
            recentCommentary.Enqueue(text);
            if (recentCommentary.Count > MaxRecentCommentary)
            {
                recentCommentary.Dequeue();
            }
        }
        
        private void OnRunStarted()
        {
            // Reset pressure for new run
            currentPressure = 0f;
            
            // Try to use authored line first
            if (useAuthoredLines)
            {
                var line = SelectWeightedRandomLine("run_start");
                if (line != null)
                {
                    QueueCommentary(line.announcerType, "Run started", line.text, "run_start");
                    AddToRecentCommentary(line.text);
                    return;
                }
            }
            
            // Fallback to dynamic generation
            string context = "The agility run has begun!";
            
            // Add breed callout if enabled
            if (enableBreedCallouts && dog != null && dog.Breed != null)
            {
                context += $" We have a {dog.Breed} on the course!";
            }
            
            QueueCommentary(AnnouncerType.Main, "Run started", context);
        }
        
        private void OnRunCompleted(RunResult result, float time, int faults)
        {
            // Update pressure based on result
            if (result == RunResult.Qualified && faults == 0)
            {
                currentPressure += pressureEscalationRate;
                isChampionship = currentPressure >= championshipPressureThreshold;
            }
            else if (faults > 2)
            {
                currentPressure = Mathf.Max(0, currentPressure - pressureEscalationRate * 2);
            }
            
            // Select appropriate line based on result
            string category = result == RunResult.Qualified ? "run_complete_qualified" : "run_complete_fault";
            
            if (useAuthoredLines)
            {
                var line = SelectWeightedRandomLine(category);
                if (line != null)
                {
                    QueueCommentary(line.announcerType, "Run completed", line.text, category);
                    AddToRecentCommentary(line.text);
                }
                else
                {
                    // Fallback to dynamic
                    GenerateDynamicRunCompleted(result, time, faults);
                }
            }
            else
            {
                GenerateDynamicRunCompleted(result, time, faults);
            }
            
            // Add pressure commentary if championship
            if (isChampionship && result == RunResult.Qualified)
            {
                QueueCommentary(AnnouncerType.Main, "Championship pressure", 
                    "This could be a championship performance!", "pressure_high");
            }
        }
        
        private void GenerateDynamicRunCompleted(RunResult result, float time, int faults)
        {
            string resultText = result == RunResult.Qualified ? "qualified" : "did not qualify";
            string context = $"The run is complete. The team {resultText} with a time of {time:F2} seconds and {faults} faults.";
            
            // Add breed callout
            if (enableBreedCallouts && dog != null && dog.Breed != null)
            {
                context += $" That {dog.Breed} gave it their all!";
            }
            
            QueueCommentary(AnnouncerType.Main, "Run completed", context);
            
            if (faults > 0)
            {
                QueueCommentary(AnnouncerType.Color, "Faults analysis", 
                    $"That's {faults} fault(s) on the course. The handler will need to work on precision.");
            }
        }
        
        private void OnObstacleCompleted(ObstacleType type, bool clean)
        {
            string obstacleName = type.ToString();
            string category = clean ? "clean_" + obstacleName.ToLower() : "fault_" + obstacleName.ToLower();
            
            // Try authored lines first
            if (useAuthoredLines)
            {
                var line = SelectWeightedRandomLine(category);
                if (line != null)
                {
                    QueueCommentary(line.announcerType, clean ? "Clean obstacle" : "Obstacle fault", line.text, category);
                    AddToRecentCommentary(line.text);
                    return;
                }
                
                // Try generic clean/fault lines
                category = clean ? "clean_jump" : "fault_jump";
                line = SelectWeightedRandomLine(category);
                if (line != null)
                {
                    QueueCommentary(line.announcerType, clean ? "Clean obstacle" : "Obstacle fault", line.text, category);
                    AddToRecentCommentary(line.text);
                    return;
                }
            }
            
            // Fallback to dynamic generation
            if (clean)
            {
                string context = $"Beautifully executed {obstacleName}! The dog made that look easy.";
                if (enableBreedCallouts && dog != null && dog.Breed != null)
                {
                    context += $" That's the {dog.Breed} for you!";
                }
                QueueCommentary(AnnouncerType.Color, "Clean obstacle", context);
            }
            else
            {
                QueueCommentary(AnnouncerType.Main, "Obstacle fault", 
                    $"A fault on the {obstacleName}. The dog knocked the bar or missed the contact zone.");
            }
        }
        
        private void CheckForNearMiss(Vector3 obstaclePosition, Vector3 dogPosition, Vector3 dogVelocity)
        {
            float distanceToObstacle = Vector3.Distance(obstaclePosition, dogPosition);
            
            // Check if dog narrowly avoided a fault
            if (distanceToObstacle < nearMissDistance && distanceToObstacle > 0.1f)
            {
                // Dog was very close to obstacle but didn't hit it
                Vector3 toObstacle = obstaclePosition - dogPosition;
                float dotProduct = Vector3.Dot(dogVelocity.normalized, toObstacle.normalized);
                
                // Dog was moving away from obstacle (avoided it)
                if (dotProduct < -0.5f)
                {
                    QueueCommentary(AnnouncerType.Color, "Near miss", 
                        "So close! Just barely missed that.", "near_miss");
                }
            }
        }
        
        private void OnFaultCommitted(FaultType fault, string obstacleName)
        {
            string category = "fault_" + obstacleName.ToLower();
            
            // Try authored lines first
            if (useAuthoredLines)
            {
                var line = SelectWeightedRandomLine(category);
                if (line != null)
                {
                    QueueCommentary(line.announcerType, "Fault committed", line.text, category);
                    AddToRecentCommentary(line.text);
                    return;
                }
                
                // Try generic fault line
                line = SelectWeightedRandomLine("fault_jump");
                if (line != null)
                {
                    QueueCommentary(line.announcerType, "Fault committed", line.text, "fault_jump");
                    AddToRecentCommentary(line.text);
                    return;
                }
            }
            
            // Fallback to dynamic generation
            QueueCommentary(AnnouncerType.Main, "Fault committed", 
                $"Fault: {fault} on {obstacleName}. That's a costly mistake.");
        }
        
        private void OnSplitTimeRecorded(float time)
        {
            // Only comment on particularly fast or slow splits
            if (time < 2.0f)
            {
                string category = "fast_split";
                
                if (useAuthoredLines)
                {
                    var line = SelectWeightedRandomLine(category);
                    if (line != null)
                    {
                        QueueCommentary(line.announcerType, "Fast split", line.text, category);
                        AddToRecentCommentary(line.text);
                        return;
                    }
                }
                
                // Fallback to dynamic
                QueueCommentary(AnnouncerType.Color, "Fast split", 
                    $"Incredible speed through that section! Split time of {time:F2} seconds.");
            }
            else if (time > 5.0f)
            {
                string category = "slow_split";
                
                if (useAuthoredLines)
                {
                    var line = SelectWeightedRandomLine(category);
                    if (line != null)
                    {
                        QueueCommentary(line.announcerType, "Slow split", line.text, category);
                        AddToRecentCommentary(line.text);
                        return;
                    }
                }
                
                // Fallback to dynamic
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
        
        private void QueueCommentary(AnnouncerType announcerType, string eventDescription, string context, string category = null, string[] tags = null)
        {
            if (!enableCommentary || !isSessionActive) return;
            if (Time.time - lastCommentaryTime < commentaryCooldown) return;
            
            CommentaryTask task = new CommentaryTask
            {
                announcerType = announcerType,
                eventDescription = eventDescription,
                context = context,
                timestamp = Time.time,
                category = category,
                tags = tags
            };
            
            commentaryQueue.Enqueue(task);
            ProcessQueue();
        }
        
        private void ProcessQueue()
        {
            if (isProcessingQueue || commentaryQueue.Count == 0) 
            {
                // Stop ducking if queue is empty and commentary is not playing
                if (AudioDuckingService.Instance != null && commentaryQueue.Count == 0)
                {
                    AudioDuckingService.Instance.StopDucking();
                }
                return;
            }
            
            isProcessingQueue = true;
            CommentaryTask task = commentaryQueue.Dequeue();
            
            StartCoroutine(GenerateAndPlayCommentary(task));
        }
        
        private IEnumerator GenerateAndPlayCommentary(CommentaryTask task)
        {
            // Determine if we should use Eastworld or authored lines
            bool useEastworld = !useAuthoredLines || Random.value < eastworldFallbackChance;
            string commentaryText = null;
            
            if (useEastworld && isSessionActive)
            {
                // Get commentary from Eastworld agent
                string agentUuid = GetAgentUuid(task.announcerType);
                string prompt = $"You are an announcer at an agility competition. {task.context}. Provide brief, enthusiastic commentary (1-2 sentences).";
                
                bool requestCompleted = false;
                
                yield return eastworldClient.Interact(sessionUuid, agentUuid, prompt, response =>
                {
                    commentaryText = response.content;
                    requestCompleted = true;
                }, error =>
                {
                    Debug.LogError($"Eastworld error: {error}");
                    requestCompleted = true;
                    commentaryText = task.context;
                });
                
                // Wait for request to complete
                while (!requestCompleted) yield return null;
            }
            
            // Use provided context if Eastworld failed or not used
            if (string.IsNullOrEmpty(commentaryText))
            {
                commentaryText = task.context;
            }
            
            // Add to recent commentary for anti-repetition
            AddToRecentCommentary(commentaryText);
            
            // Generate speech with ElevenLabs
            AudioClip audioClip = null;
            bool audioLoaded = false;
            
            switch (task.announcerType)
            {
                case AnnouncerType.Main:
                    yield return elevenLabsService.SpeakAsMainAnnouncer(commentaryText, clip =>
                    {
                        audioClip = clip;
                        audioLoaded = true;
                    });
                    break;
                    
                case AnnouncerType.Color:
                    yield return elevenLabsService.SpeakAsColorCommentator(commentaryText, clip =>
                    {
                        audioClip = clip;
                        audioLoaded = true;
                    });
                    break;
                    
                case AnnouncerType.PA:
                    // For PA announcer, we might use a different voice or the same as main
                    yield return elevenLabsService.SpeakAsMainAnnouncer(commentaryText, clip =>
                    {
                        audioClip = clip;
                        audioLoaded = true;
                    });
                    break;
            }
            
            // Wait for audio to load
            while (!audioLoaded) yield return null;
            
            // Play the audio
            if (audioClip != null)
            {
                AudioSource source = GetAnnouncerSource(task.announcerType);
                elevenLabsService.PlayAudioClip(audioClip, source);
                lastCommentaryTime = Time.time;
                
                // Trigger audio ducking when commentary plays
                if (AudioDuckingService.Instance != null)
                {
                    AudioDuckingService.Instance.StartDucking();
                }
            }
            
            // Process next in queue after a short delay
            yield return new WaitForSeconds(0.5f);
            
            // Stop ducking after commentary finishes
            if (AudioDuckingService.Instance != null && !isProcessingQueue)
            {
                AudioDuckingService.Instance.StopDucking();
            }
            
            isProcessingQueue = false;
            ProcessQueue();
        }
        
        private string GetAgentUuid(AnnouncerType announcerType)
        {
            switch (announcerType)
            {
                case AnnouncerType.Main: return mainAnnouncerAgentUuid;
                case AnnouncerType.Color: return colorCommentatorAgentUuid;
                case AnnouncerType.PA: return paAnnouncerAgentUuid;
                default: return mainAnnouncerAgentUuid;
            }
        }
        
        private AudioSource GetAnnouncerSource(AnnouncerType announcerType)
        {
            switch (announcerType)
            {
                case AnnouncerType.Main: return mainAnnouncerSource;
                case AnnouncerType.Color: return colorCommentatorSource;
                case AnnouncerType.PA: return paAnnouncerSource;
                default: return mainAnnouncerSource;
            }
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
        
        public void TriggerPAAnnouncerCommentary(string context)
        {
            QueueCommentary(AnnouncerType.PA, "Manual trigger", context);
        }
        
        // Championship pressure methods
        public void SetChampionshipMode(bool isChampionship)
        {
            this.isChampionship = isChampionship;
            if (isChampionship)
            {
                currentPressure = championshipPressureThreshold;
            }
        }
        
        public void AddPressure(float amount)
        {
            currentPressure = Mathf.Clamp01(currentPressure + amount);
            if (currentPressure >= championshipPressureThreshold)
            {
                isChampionship = true;
            }
        }
        
        // Test method
        public void TestCommentary()
        {
            TriggerMainAnnouncerCommentary("Welcome to the agility competition!");
            StartCoroutine(WaitAndTestColor());
            StartCoroutine(WaitAndTestPA());
        }
        
        private IEnumerator WaitAndTestColor()
        {
            yield return new WaitForSeconds(3f);
            TriggerColorCommentatorCommentary("The dog looks ready to run!");
        }
        
        private IEnumerator WaitAndTestPA()
        {
            yield return new WaitForSeconds(6f);
            TriggerPAAnnouncerCommentary("Ladies and gentlemen, please welcome our next competitor!");
        }
    }
}