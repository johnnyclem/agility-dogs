using System;
using System.Collections.Generic;
using UnityEngine;
using AgilityDogs.Core;
using AgilityDogs.Events;

namespace AgilityDogs.Services
{
    /// <summary>
    /// Voice Command Service - Stub Implementation for v2
    /// This service provides a framework for voice commands that will be implemented in a future update.
    /// </summary>
    public class VoiceCommandService : MonoBehaviour
    {
        [Header("Voice Command Settings")]
        [SerializeField] private bool enableVoiceCommands = false;
        [SerializeField] private float confidenceThreshold = 0.75f;
        [SerializeField] private float commandCooldown = 0.5f;
        
        [Header("Voice Recognition Settings")]
        [SerializeField] private string preferredSTTService = "Vosk";
        [SerializeField] private int sampleRate = 16000;
        [SerializeField] private float maxRecordingTime = 5.0f;
        
        [Header("Command Mapping")]
        [SerializeField] private List<VoiceCommandMapping> commandMappings = new List<VoiceCommandMapping>();
        
        // Service state
        private bool isInitialized = false;
        private bool isListening = false;
        private float lastCommandTime;
        private string lastRecognizedText = "";
        
        // STT service placeholder
        private object sttService = null;
        
        // Events
        public event Action<HandlerCommand, float> OnVoiceCommandDetected;
        public event Action<string> OnVoiceTextRecognized;
        public event Action OnVoiceServiceReady;
        public event Action<string> OnVoiceServiceError;
        
        // Properties
        public bool IsEnabled => enableVoiceCommands;
        public bool IsListening => isListening;
        public bool IsInitialized => isInitialized;
        public float ConfidenceThreshold => confidenceThreshold;
        
        // Command mapping data structure
        [System.Serializable]
        public class VoiceCommandMapping
        {
            public string voicePhrase;
            public HandlerCommand command;
            [Range(0f, 1f)]
            public float confidenceWeight = 1.0f;
            public bool caseSensitive = false;
        }
        
        private void Awake()
        {
            // Initialize command mappings if empty
            if (commandMappings.Count == 0)
            {
                InitializeDefaultMappings();
            }
        }
        
        private void Start()
        {
            if (enableVoiceCommands)
            {
                InitializeVoiceService();
            }
            else
            {
                Debug.Log("[VoiceCommandService] Voice commands disabled. Enable in inspector for v2.");
            }
        }
        
        private void OnDestroy()
        {
            ShutdownVoiceService();
        }
        
        /// <summary>
        /// Initialize default voice command mappings
        /// </summary>
        private void InitializeDefaultMappings()
        {
            commandMappings.Add(new VoiceCommandMapping 
            { 
                voicePhrase = "jump", 
                command = HandlerCommand.Jump,
                confidenceWeight = 1.0f
            });
            
            commandMappings.Add(new VoiceCommandMapping 
            { 
                voicePhrase = "tunnel", 
                command = HandlerCommand.Tunnel,
                confidenceWeight = 1.0f
            });
            
            commandMappings.Add(new VoiceCommandMapping 
            { 
                voicePhrase = "weave", 
                command = HandlerCommand.Weave,
                confidenceWeight = 1.0f
            });
            
            commandMappings.Add(new VoiceCommandMapping 
            { 
                voicePhrase = "table", 
                command = HandlerCommand.Table,
                confidenceWeight = 1.0f
            });
            
            commandMappings.Add(new VoiceCommandMapping 
            { 
                voicePhrase = "come by", 
                command = HandlerCommand.ComeBye,
                confidenceWeight = 1.0f
            });
            
            commandMappings.Add(new VoiceCommandMapping 
            { 
                voicePhrase = "away", 
                command = HandlerCommand.Away,
                confidenceWeight = 1.0f
            });
            
            commandMappings.Add(new VoiceCommandMapping 
            { 
                voicePhrase = "here", 
                command = HandlerCommand.Here,
                confidenceWeight = 1.0f
            });
            
            commandMappings.Add(new VoiceCommandMapping 
            { 
                voicePhrase = "out", 
                command = HandlerCommand.Out,
                confidenceWeight = 1.0f
            });
            
            commandMappings.Add(new VoiceCommandMapping 
            { 
                voicePhrase = "go", 
                command = HandlerCommand.Go,
                confidenceWeight = 1.0f
            });
            
            commandMappings.Add(new VoiceCommandMapping 
            { 
                voicePhrase = "left", 
                command = HandlerCommand.Left,
                confidenceWeight = 1.0f
            });
            
            commandMappings.Add(new VoiceCommandMapping 
            { 
                voicePhrase = "right", 
                command = HandlerCommand.Right,
                confidenceWeight = 1.0f
            });
            
            Debug.Log($"[VoiceCommandService] Initialized {commandMappings.Count} default command mappings");
        }
        
        /// <summary>
        /// Initialize the voice service (stub implementation)
        /// </summary>
        private void InitializeVoiceService()
        {
            if (isInitialized)
            {
                Debug.LogWarning("[VoiceCommandService] Already initialized");
                return;
            }
            
            try
            {
                // Stub: In v2, this would initialize the actual STT service
                // For now, we just set up the framework
                
                Debug.Log($"[VoiceCommandService] Initializing voice service with {preferredSTTService}");
                Debug.Log($"[VoiceCommandService] Sample rate: {sampleRate}Hz");
                Debug.Log($"[VoiceCommandService] Confidence threshold: {confidenceThreshold}");
                
                // Simulate initialization
                isInitialized = true;
                
                // Notify listeners
                OnVoiceServiceReady?.Invoke();
                
                Debug.Log("[VoiceCommandService] Voice service initialized (stub mode)");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[VoiceCommandService] Failed to initialize: {ex.Message}");
                OnVoiceServiceError?.Invoke(ex.Message);
            }
        }
        
        /// <summary>
        /// Shutdown the voice service
        /// </summary>
        private void ShutdownVoiceService()
        {
            if (!isInitialized) return;
            
            isListening = false;
            isInitialized = false;
            
            Debug.Log("[VoiceCommandService] Voice service shutdown");
        }
        
        /// <summary>
        /// Start listening for voice commands
        /// </summary>
        public void StartListening()
        {
            if (!isInitialized)
            {
                Debug.LogWarning("[VoiceCommandService] Cannot start listening - service not initialized");
                return;
            }
            
            if (isListening)
            {
                Debug.LogWarning("[VoiceCommandService] Already listening");
                return;
            }
            
            isListening = true;
            Debug.Log("[VoiceCommandService] Started listening for voice commands");
            
            // Stub: In v2, this would start the actual audio capture
        }
        
        /// <summary>
        /// Stop listening for voice commands
        /// </summary>
        public void StopListening()
        {
            if (!isListening)
            {
                Debug.LogWarning("[VoiceCommandService] Not currently listening");
                return;
            }
            
            isListening = false;
            Debug.Log("[VoiceCommandService] Stopped listening for voice commands");
            
            // Stub: In v2, this would stop the actual audio capture
        }
        
        /// <summary>
        /// Toggle voice command listening
        /// </summary>
        public void ToggleListening()
        {
            if (isListening)
            {
                StopListening();
            }
            else
            {
                StartListening();
            }
        }
        
        /// <summary>
        /// Process recognized text from STT service
        /// This is a stub method - in v2, this would be called by the actual STT service
        /// </summary>
        /// <param name="recognizedText">The text recognized by STT</param>
        /// <param name="confidence">Confidence score (0-1)</param>
        public void ProcessRecognizedText(string recognizedText, float confidence)
        {
            if (!isListening || !isInitialized)
                return;
            
            lastRecognizedText = recognizedText;
            
            // Fire text recognized event
            OnVoiceTextRecognized?.Invoke(recognizedText);
            
            // Check if confidence meets threshold
            if (confidence < confidenceThreshold)
            {
                Debug.Log($"[VoiceCommandService] Low confidence ({confidence:F2}): '{recognizedText}'");
                
                // Fire "misunderstood" event for low confidence
                GameEvents.RaiseVoiceCommandMisunderstood(recognizedText, confidence);
                return;
            }
            
            // Check cooldown
            if (Time.time - lastCommandTime < commandCooldown)
            {
                Debug.Log($"[VoiceCommandService] Command cooldown active");
                return;
            }
            
            // Try to match recognized text to a command
            HandlerCommand matchedCommand = MatchTextToCommand(recognizedText);
            
            if (matchedCommand != HandlerCommand.None)
            {
                Debug.Log($"[VoiceCommandService] Command detected: {matchedCommand} (confidence: {confidence:F2})");
                
                // Update last command time
                lastCommandTime = Time.time;
                
                // Fire command detected event
                OnVoiceCommandDetected?.Invoke(matchedCommand, confidence);
                
                // Raise game event
                GameEvents.RaiseVoiceCommandDetected(matchedCommand, confidence, recognizedText);
            }
            else
            {
                Debug.Log($"[VoiceCommandService] No command match for: '{recognizedText}'");
            }
        }
        
        /// <summary>
        /// Match recognized text to a handler command
        /// </summary>
        private HandlerCommand MatchTextToCommand(string text)
        {
            if (string.IsNullOrEmpty(text))
                return HandlerCommand.None;
            
            string searchText = text.ToLower().Trim();
            
            foreach (var mapping in commandMappings)
            {
                string phrase = mapping.caseSensitive ? mapping.voicePhrase : mapping.voicePhrase.ToLower();
                
                // Check for exact match or contains match
                if (searchText.Contains(phrase) || searchText.Equals(phrase))
                {
                    return mapping.command;
                }
            }
            
            return HandlerCommand.None;
        }
        
        /// <summary>
        /// Simulate a voice command for testing purposes
        /// </summary>
        public void SimulateVoiceCommand(string text, float confidence = 0.9f)
        {
            Debug.Log($"[VoiceCommandService] Simulating voice command: '{text}' (confidence: {confidence})");
            ProcessRecognizedText(text, confidence);
        }
        
        /// <summary>
        /// Get current voice command status
        /// </summary>
        public VoiceCommandStatus GetStatus()
        {
            return new VoiceCommandStatus
            {
                isEnabled = enableVoiceCommands,
                isInitialized = isInitialized,
                isListening = isListening,
                confidenceThreshold = confidenceThreshold,
                commandCooldown = commandCooldown,
                lastRecognizedText = lastRecognizedText,
                mappedCommandsCount = commandMappings.Count
            };
        }
        
        /// <summary>
        /// Add or update a voice command mapping
        /// </summary>
        public void AddCommandMapping(string phrase, HandlerCommand command, float confidenceWeight = 1.0f)
        {
            // Check if mapping already exists
            var existing = commandMappings.Find(m => 
                m.voicePhrase.Equals(phrase, StringComparison.OrdinalIgnoreCase));
            
            if (existing != null)
            {
                existing.command = command;
                existing.confidenceWeight = confidenceWeight;
                Debug.Log($"[VoiceCommandService] Updated mapping: '{phrase}' -> {command}");
            }
            else
            {
                commandMappings.Add(new VoiceCommandMapping
                {
                    voicePhrase = phrase,
                    command = command,
                    confidenceWeight = confidenceWeight
                });
                Debug.Log($"[VoiceCommandService] Added mapping: '{phrase}' -> {command}");
            }
        }
        
        /// <summary>
        /// Remove a voice command mapping
        /// </summary>
        public bool RemoveCommandMapping(string phrase)
        {
            var removed = commandMappings.RemoveAll(m => 
                m.voicePhrase.Equals(phrase, StringComparison.OrdinalIgnoreCase));
            
            if (removed > 0)
            {
                Debug.Log($"[VoiceCommandService] Removed mapping for: '{phrase}'");
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Clear all command mappings
        /// </summary>
        public void ClearCommandMappings()
        {
            commandMappings.Clear();
            Debug.Log("[VoiceCommandService] Cleared all command mappings");
        }
        
        /// <summary>
        /// Reset to default command mappings
        /// </summary>
        public void ResetToDefaults()
        {
            ClearCommandMappings();
            InitializeDefaultMappings();
            Debug.Log("[VoiceCommandService] Reset to default command mappings");
        }
        
        #if UNITY_EDITOR
        /// <summary>
        /// Editor-only test method to verify voice command mappings
        /// </summary>
        [ContextMenu("Test Voice Command Mappings")]
        private void TestVoiceCommandMappings()
        {
            Debug.Log("=== Voice Command Mapping Test ===");
            foreach (var mapping in commandMappings)
            {
                Debug.Log($"'{mapping.voicePhrase}' -> {mapping.command} (weight: {mapping.confidenceWeight})");
            }
            Debug.Log($"Total mappings: {commandMappings.Count}");
        }
        #endif
    }
    
    /// <summary>
    /// Voice command status data structure
    /// </summary>
    [System.Serializable]
    public struct VoiceCommandStatus
    {
        public bool isEnabled;
        public bool isInitialized;
        public bool isListening;
        public float confidenceThreshold;
        public float commandCooldown;
        public string lastRecognizedText;
        public int mappedCommandsCount;
    }
}