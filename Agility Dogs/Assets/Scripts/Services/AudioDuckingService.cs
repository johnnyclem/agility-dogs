using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AgilityDogs.Core;

namespace AgilityDogs.Services
{
    /// <summary>
    /// Audio Ducking Service - Manages audio priority and ducking for commentary
    /// Implements side-chain ducking for crowd ambient when commentary plays
    /// </summary>
    public class AudioDuckingService : MonoBehaviour
    {
        [Header("Ducking Configuration")]
        [SerializeField] private bool enableDucking = true;
        [SerializeField] private float duckingAmount = -4f; // dB reduction when commentary plays
        [SerializeField] private float duckingFadeInTime = 0.1f;
        [SerializeField] private float duckingFadeOutTime = 0.3f;
        
        [Header("Audio Source References")]
        [SerializeField] private AudioSource commentarySource;
        [SerializeField] private AudioSource crowdSource;
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource ambienceSource;
        
        [Header("Priority Channels")]
        [SerializeField] private float commentaryPriority = 1.0f;
        [SerializeField] private float musicPriority = 0.8f;
        [SerializeField] private float crowdPriority = 0.6f;
        [SerializeField] private float ambiencePriority = 0.4f;
        [SerializeField] private float sfxPriority = 0.3f;
        
        [Header("Ducking Targets")]
        [SerializeField] private List<AudioSource> duckingTargets = new List<AudioSource>();
        [SerializeField] private List<float> originalVolumes = new List<float>();
        
        // State
        private bool isDuckingActive = false;
        private Coroutine duckingCoroutine;
        private float currentDuckingLevel = 0f;
        private Dictionary<AudioSource, float> targetDuckingLevels = new Dictionary<AudioSource, float>();
        
        // Singleton
        public static AudioDuckingService Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeDuckingTargets();
        }
        
        private void Start()
        {
            // Subscribe to commentary events if available
            var commentaryManager = FindObjectOfType<CommentaryManager>();
            if (commentaryManager != null)
            {
                // We'll need to add events to CommentaryManager for this to work
                // For now, we'll use a polling approach
            }
        }
        
        private void Update()
        {
            if (!enableDucking) return;
            
            // Check if commentary is playing and duck accordingly
            CheckCommentaryStatus();
        }
        
        private void OnDestroy()
        {
            // Restore all volumes
            RestoreAllVolumes();
        }
        
        /// <summary>
        /// Initialize ducking targets from references
        /// </summary>
        private void InitializeDuckingTargets()
        {
            duckingTargets.Clear();
            originalVolumes.Clear();
            
            // Add configured audio sources
            if (crowdSource != null)
            {
                duckingTargets.Add(crowdSource);
                originalVolumes.Add(crowdSource.volume);
            }
            
            if (musicSource != null)
            {
                duckingTargets.Add(musicSource);
                originalVolumes.Add(musicSource.volume);
            }
            
            if (ambienceSource != null)
            {
                duckingTargets.Add(ambienceSource);
                originalVolumes.Add(ambienceSource.volume);
            }
            
            // Find all AudioSources in scene if none configured
            if (duckingTargets.Count == 0)
            {
                FindAllAudioSources();
            }
        }
        
        /// <summary>
        /// Find all AudioSources in the scene for ducking
        /// </summary>
        private void FindAllAudioSources()
        {
            var allSources = FindObjectsOfType<AudioSource>();
            foreach (var source in allSources)
            {
                if (source != commentarySource && !duckingTargets.Contains(source))
                {
                    duckingTargets.Add(source);
                    originalVolumes.Add(source.volume);
                    targetDuckingLevels[source] = 0f;
                }
            }
        }
        
        /// <summary>
        /// Check if commentary is playing and adjust ducking
        /// </summary>
        private void CheckCommentaryStatus()
        {
            if (commentarySource == null) return;
            
            bool commentaryIsPlaying = commentarySource.isPlaying;
            
            if (commentaryIsPlaying && !isDuckingActive)
            {
                StartDucking();
            }
            else if (!commentaryIsPlaying && isDuckingActive)
            {
                StopDucking();
            }
        }
        
        /// <summary>
        /// Start ducking audio when commentary plays
        /// </summary>
        public void StartDucking()
        {
            if (!enableDucking || isDuckingActive) return;
            
            isDuckingActive = true;
            
            if (duckingCoroutine != null)
            {
                StopCoroutine(duckingCoroutine);
            }
            
            duckingCoroutine = StartCoroutine(DuckingFadeIn());
            
            Debug.Log($"[AudioDuckingService] Started ducking (amount: {duckingAmount}dB)");
        }
        
        /// <summary>
        /// Stop ducking audio when commentary stops
        /// </summary>
        public void StopDucking()
        {
            if (!isDuckingActive) return;
            
            isDuckingActive = false;
            
            if (duckingCoroutine != null)
            {
                StopCoroutine(duckingCoroutine);
            }
            
            duckingCoroutine = StartCoroutine(DuckingFadeOut());
            
            Debug.Log("[AudioDuckingService] Stopped ducking");
        }
        
        /// <summary>
        /// Fade in ducking effect
        /// </summary>
        private IEnumerator DuckingFadeIn()
        {
            float startVolume = currentDuckingLevel;
            float targetVolume = duckingAmount;
            float elapsedTime = 0f;
            
            while (elapsedTime < duckingFadeInTime)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsedTime / duckingFadeInTime);
                
                currentDuckingLevel = Mathf.Lerp(startVolume, targetVolume, t);
                ApplyDuckingToAll();
                
                yield return null;
            }
            
            currentDuckingLevel = targetVolume;
            ApplyDuckingToAll();
        }
        
        /// <summary>
        /// Fade out ducking effect
        /// </summary>
        private IEnumerator DuckingFadeOut()
        {
            float startVolume = currentDuckingLevel;
            float targetVolume = 0f;
            float elapsedTime = 0f;
            
            while (elapsedTime < duckingFadeOutTime)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsedTime / duckingFadeOutTime);
                
                currentDuckingLevel = Mathf.Lerp(startVolume, targetVolume, t);
                ApplyDuckingToAll();
                
                yield return null;
            }
            
            currentDuckingLevel = 0f;
            ApplyDuckingToAll();
            RestoreAllVolumes();
        }
        
        /// <summary>
        /// Apply ducking to all target audio sources
        /// </summary>
        private void ApplyDuckingToAll()
        {
            for (int i = 0; i < duckingTargets.Count; i++)
            {
                if (duckingTargets[i] != null && i < originalVolumes.Count)
                {
                    float duckedVolume = originalVolumes[i] * Mathf.Pow(10f, currentDuckingLevel / 20f);
                    duckingTargets[i].volume = Mathf.Clamp01(duckedVolume);
                }
            }
        }
        
        /// <summary>
        /// Restore all volumes to original levels
        /// </summary>
        private void RestoreAllVolumes()
        {
            for (int i = 0; i < duckingTargets.Count; i++)
            {
                if (duckingTargets[i] != null && i < originalVolumes.Count)
                {
                    duckingTargets[i].volume = originalVolumes[i];
                }
            }
        }
        
        /// <summary>
        /// Register a new audio source for ducking
        /// </summary>
        public void RegisterAudioSource(AudioSource source, float originalVolume, float priority = 0.5f)
        {
            if (source == null || duckingTargets.Contains(source)) return;
            
            duckingTargets.Add(source);
            originalVolumes.Add(originalVolume);
            targetDuckingLevels[source] = 0f;
            
            Debug.Log($"[AudioDuckingService] Registered audio source: {source.name}");
        }
        
        /// <summary>
        /// Unregister an audio source from ducking
        /// </summary>
        public void UnregisterAudioSource(AudioSource source)
        {
            int index = duckingTargets.IndexOf(source);
            if (index >= 0)
            {
                duckingTargets.RemoveAt(index);
                if (index < originalVolumes.Count)
                {
                    originalVolumes.RemoveAt(index);
                }
                targetDuckingLevels.Remove(source);
                
                Debug.Log($"[AudioDuckingService] Unregistered audio source: {source.name}");
            }
        }
        
        /// <summary>
        /// Set ducking amount (in dB)
        /// </summary>
        public void SetDuckingAmount(float amount)
        {
            duckingAmount = Mathf.Clamp(amount, -20f, 0f);
            
            if (isDuckingActive)
            {
                // Update immediately if ducking is active
                currentDuckingLevel = duckingAmount;
                ApplyDuckingToAll();
            }
        }
        
        /// <summary>
        /// Get current ducking amount
        /// </summary>
        public float GetDuckingAmount()
        {
            return duckingAmount;
        }
        
        /// <summary>
        /// Check if ducking is currently active
        /// </summary>
        public bool IsDuckingActive()
        {
            return isDuckingActive;
        }
        
        /// <summary>
        /// Force ducking on/off (for testing)
        /// </summary>
        [ContextMenu("Test Ducking On")]
        public void TestDuckingOn()
        {
            if (commentarySource != null)
            {
                commentarySource.Play();
            }
        }
        
        [ContextMenu("Test Ducking Off")]
        public void TestDuckingOff()
        {
            if (commentarySource != null)
            {
                commentarySource.Stop();
            }
        }
        
        /// <summary>
        /// Set priority for an audio channel
        /// </summary>
        public void SetChannelPriority(string channelName, float priority)
        {
            priority = Mathf.Clamp01(priority);
            
            switch (channelName.ToLower())
            {
                case "commentary":
                    commentaryPriority = priority;
                    break;
                case "music":
                    musicPriority = priority;
                    break;
                case "crowd":
                    crowdPriority = priority;
                    break;
                case "ambience":
                    ambiencePriority = priority;
                    break;
                case "sfx":
                    sfxPriority = priority;
                    break;
            }
        }
        
        /// <summary>
        /// Get priority for an audio channel
        /// </summary>
        public float GetChannelPriority(string channelName)
        {
            switch (channelName.ToLower())
            {
                case "commentary": return commentaryPriority;
                case "music": return musicPriority;
                case "crowd": return crowdPriority;
                case "ambience": return ambiencePriority;
                case "sfx": return sfxPriority;
                default: return 0.5f;
            }
        }
        
        /// <summary>
        /// Get audio mix snapshot based on current state
        /// </summary>
        public AudioMixSnapshot GetCurrentMixSnapshot()
        {
            return new AudioMixSnapshot
            {
                isDuckingActive = isDuckingActive,
                duckingAmount = currentDuckingLevel,
                commentaryPlaying = commentarySource != null && commentarySource.isPlaying,
                duckedSourcesCount = duckingTargets.Count
            };
        }
        
        #if UNITY_EDITOR
        [ContextMenu("Debug Audio Status")]
        private void DebugAudioStatus()
        {
            Debug.Log("=== Audio Ducking Service Status ===");
            Debug.Log($"Enabled: {enableDucking}");
            Debug.Log($"Ducking Active: {isDuckingActive}");
            Debug.Log($"Ducking Amount: {duckingAmount}dB");
            Debug.Log($"Commentary Source: {(commentarySource != null ? commentarySource.name : "NULL")}");
            Debug.Log($"Ducking Targets: {duckingTargets.Count}");
            
            for (int i = 0; i < duckingTargets.Count; i++)
            {
                if (duckingTargets[i] != null)
                {
                    Debug.Log($"  - {duckingTargets[i].name}: Original={originalVolumes[i]}, Current={duckingTargets[i].volume}");
                }
            }
        }
        #endif
    }
    
    /// <summary>
    /// Audio mix snapshot data structure
    /// </summary>
    [System.Serializable]
    public struct AudioMixSnapshot
    {
        public bool isDuckingActive;
        public float duckingAmount;
        public bool commentaryPlaying;
        public int duckedSourcesCount;
    }
}