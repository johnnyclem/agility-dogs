using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace AgilityDogs.Services
{
    public class VOAssetManager : MonoBehaviour
    {
        public static VOAssetManager Instance { get; private set; }

        [Header("Asset Storage")]
        [SerializeField] private string assetFolder = "VoiceOver";
        [SerializeField] private bool cacheAudioFiles = true;
        [SerializeField] private int maxCacheSizeMB = 100;

        [Header("Audio Settings")]
        [SerializeField] private AudioType audioFormat = AudioType.MPEG;
        [SerializeField] private int sampleRate = 44100;

        [Header("Generation Queue")]
        [SerializeField] private int maxConcurrentGenerations = 2;
        [SerializeField] private float requestTimeout = 30f;

        // Cache
        private Dictionary<string, AudioClip> audioCache = new Dictionary<string, AudioClip>();
        private Dictionary<string, string> filePathCache = new Dictionary<string, string>();
        private Queue<VOGenerationRequest> generationQueue = new Queue<VOGenerationRequest>();
        private int activeGenerations = 0;

        // Events
        public event Action<string, AudioClip> OnVOLoaded;
        public event Action<string> OnVOFailed;
        public event Action<string, float> OnVOGenerationProgress;

        // Properties
        public int CachedAudioCount => audioCache.Count;
        public int QueuedRequests => generationQueue.Count;

        private string assetPath;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeAssetPath();
            LoadCachedFileIndex();
        }

        private void InitializeAssetPath()
        {
            assetPath = Path.Combine(Application.persistentDataPath, assetFolder);
            if (!Directory.Exists(assetPath))
            {
                Directory.CreateDirectory(assetPath);
            }
            Debug.Log($"[VOAssetManager] Asset path: {assetPath}");
        }

        private void LoadCachedFileIndex()
        {
            if (!cacheAudioFiles) return;

            string indexPath = Path.Combine(assetPath, "index.json");
            if (File.Exists(indexPath))
            {
                try
                {
                    string json = File.ReadAllText(indexPath);
                    var index = JsonUtility.FromJson<VOAssetIndex>(json);
                    
                    foreach (var entry in index.entries)
                    {
                        filePathCache[entry.key] = entry.filePath;
                    }
                    
                    Debug.Log($"[VOAssetManager] Loaded {filePathCache.Count} cached files");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[VOAssetManager] Failed to load index: {e.Message}");
                }
            }
        }

        private void SaveFileIndex()
        {
            if (!cacheAudioFiles) return;

            var index = new VOAssetIndex();
            foreach (var kvp in filePathCache)
            {
                index.entries.Add(new VOAssetEntry
                {
                    key = kvp.Key,
                    filePath = kvp.Value
                });
            }

            string indexPath = Path.Combine(assetPath, "index.json");
            string json = JsonUtility.ToJson(index, true);
            File.WriteAllText(indexPath, json);
        }

        #region Public API

        /// <summary>
        /// Load or generate voice-over audio for the given text
        /// </summary>
        public void RequestVO(string key, string text, string voiceId, Action<AudioClip> onSuccess, Action<string> onError = null)
        {
            // Check cache first
            if (audioCache.TryGetValue(key, out AudioClip cachedClip))
            {
                onSuccess?.Invoke(cachedClip);
                OnVOLoaded?.Invoke(key, cachedClip);
                return;
            }

            // Check if file exists on disk
            if (filePathCache.TryGetValue(key, out string filePath) && File.Exists(filePath))
            {
                StartCoroutine(LoadAudioFromDisk(key, filePath, onSuccess, onError));
                return;
            }

            // Queue for generation
            var request = new VOGenerationRequest
            {
                key = key,
                text = text,
                voiceId = voiceId,
                onSuccess = onSuccess,
                onError = onError
            };

            generationQueue.Enqueue(request);
            ProcessGenerationQueue();
        }

        /// <summary>
        /// Load a pre-recorded audio clip from the Assets folder
        /// </summary>
        public void LoadPreRecordedClip(string clipName, Action<AudioClip> onSuccess, Action<string> onError = null)
        {
            StartCoroutine(LoadPreRecordedCoroutine(clipName, onSuccess, onError));
        }

        private IEnumerator LoadPreRecordedCoroutine(string clipName, Action<AudioClip> onSuccess, Action<string> onError)
        {
            // Try Resources folder first
            AudioClip clip = Resources.Load<AudioClip>($"VoiceOver/{clipName}");
            if (clip != null)
            {
                onSuccess?.Invoke(clip);
                yield break;
            }

            // Try streaming assets
            string streamingPath = Path.Combine(Application.streamingAssetsPath, "VoiceOver", $"{clipName}.wav");
            if (File.Exists(streamingPath))
            {
                yield return LoadAudioFromPath(streamingPath, onSuccess, onError);
            }
            else
            {
                onError?.Invoke($"Pre-recorded clip not found: {clipName}");
            }
        }

        /// <summary>
        /// Pre-load a batch of voice-over assets
        /// </summary>
        public void PreloadBatch(List<VOBatchItem> items)
        {
            StartCoroutine(PreloadBatchCoroutine(items));
        }

        private IEnumerator PreloadBatchCoroutine(List<VOBatchItem> items)
        {
            foreach (var item in items)
            {
                // Check if already cached
                if (audioCache.ContainsKey(item.key)) continue;

                // Check disk
                if (filePathCache.TryGetValue(item.key, out string filePath) && File.Exists(filePath))
                {
                    yield return LoadAudioFromDisk(item.key, filePath, null, null);
                }
                // Would need to generate if not found
            }
        }

        /// <summary>
        /// Clear the audio cache to free memory
        /// </summary>
        public void ClearCache()
        {
            audioCache.Clear();
            Resources.UnloadUnusedAssets();
            GC.Collect();
            Debug.Log("[VOAssetManager] Cache cleared");
        }

        /// <summary>
        /// Delete all cached files from disk
        /// </summary>
        public void ClearDiskCache()
        {
            if (Directory.Exists(assetPath))
            {
                Directory.Delete(assetPath, true);
                Directory.CreateDirectory(assetPath);
            }

            filePathCache.Clear();
            audioCache.Clear();
            SaveFileIndex();
            Debug.Log("[VOAssetManager] Disk cache cleared");
        }

        /// <summary>
        /// Get the current disk cache size in MB
        /// </summary>
        public float GetDiskCacheSizeMB()
        {
            if (!Directory.Exists(assetPath)) return 0f;

            long totalSize = 0;
            var dirInfo = new DirectoryInfo(assetPath);
            foreach (var file in dirInfo.GetFiles("*", SearchOption.AllDirectories))
            {
                totalSize += file.Length;
            }

            return totalSize / (1024f * 1024f);
        }

        #endregion

        #region Generation

        private void ProcessGenerationQueue()
        {
            if (activeGenerations >= maxConcurrentGenerations) return;
            if (generationQueue.Count == 0) return;

            var request = generationQueue.Dequeue();
            StartCoroutine(GenerateVOCoroutine(request));
        }

        private IEnumerator GenerateVOCoroutine(VOGenerationRequest request)
        {
            activeGenerations++;
            OnVOGenerationProgress?.Invoke(request.key, 0f);

            // Use ElevenLabsService to generate
            var elevenLabs = FindObjectOfType<ElevenLabsService>();
            if (elevenLabs == null)
            {
                request.onError?.Invoke("ElevenLabsService not found");
                activeGenerations--;
                ProcessGenerationQueue();
                yield break;
            }

            bool completed = false;
            AudioClip generatedClip = null;
            string error = null;

            yield return elevenLabs.GenerateSpeech(request.text, request.voiceId, clip =>
            {
                generatedClip = clip;
                completed = true;
            });

            if (generatedClip != null)
            {
                // Cache in memory
                audioCache[request.key] = generatedClip;

                // Save to disk if caching enabled
                if (cacheAudioFiles)
                {
                    yield return SaveAudioToDisk(request.key, generatedClip);
                }

                request.onSuccess?.Invoke(generatedClip);
                OnVOLoaded?.Invoke(request.key, generatedClip);
            }
            else
            {
                request.onError?.Invoke("Generation failed");
                OnVOFailed?.Invoke(request.key);
            }

            activeGenerations--;
            OnVOGenerationProgress?.Invoke(request.key, 1f);
            ProcessGenerationQueue();
        }

        #endregion

        #region Disk Operations

        private IEnumerator LoadAudioFromDisk(string key, string filePath, Action<AudioClip> onSuccess, Action<string> onError)
        {
            string url = "file://" + filePath;
            UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(url, audioFormat);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke($"Failed to load audio: {request.error}");
                yield break;
            }

            AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
            clip.name = key;

            audioCache[key] = clip;
            onSuccess?.Invoke(clip);
            OnVOLoaded?.Invoke(key, clip);
        }

        private IEnumerator LoadAudioFromPath(string path, Action<AudioClip> onSuccess, Action<string> onError)
        {
            string url = path.Contains("://") ? path : "file://" + path;
            UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(url, audioFormat);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke($"Failed to load audio: {request.error}");
                yield break;
            }

            AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
            onSuccess?.Invoke(clip);
        }

        private IEnumerator SaveAudioToDisk(string key, AudioClip clip)
        {
            // Convert AudioClip to WAV bytes
            float[] samples = new float[clip.samples * clip.channels];
            clip.GetData(samples, 0);

            byte[] wavBytes = EncodeToWAV(samples, clip.channels, clip.sampleRate);

            // Generate file path
            string fileName = SanitizeFileName(key) + ".wav";
            string filePath = Path.Combine(assetPath, fileName);

            // Write file
            yield return null; // Yield for frame
            File.WriteAllBytes(filePath, wavBytes);

            // Update cache
            filePathCache[key] = filePath;
            SaveFileIndex();

            Debug.Log($"[VOAssetManager] Saved audio to disk: {fileName}");
        }

        private byte[] EncodeToWAV(float[] samples, int channels, int sampleRate)
        {
            int sampleCount = samples.Length;
            int byteCount = sampleCount * 2; // 16-bit audio

            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                // RIFF header
                writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
                writer.Write(36 + byteCount);
                writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));

                // fmt chunk
                writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
                writer.Write(16);
                writer.Write((short)1); // PCM
                writer.Write((short)channels);
                writer.Write(sampleRate);
                writer.Write(sampleRate * channels * 2);
                writer.Write((short)(channels * 2));
                writer.Write((short)16);

                // data chunk
                writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
                writer.Write(byteCount);

                // Write samples
                foreach (float sample in samples)
                {
                    short value = (short)(Mathf.Clamp(sample, -1f, 1f) * short.MaxValue);
                    writer.Write(value);
                }

                return stream.ToArray();
            }
        }

        private string SanitizeFileName(string input)
        {
            char[] invalidChars = Path.GetInvalidFileNameChars();
            string sanitized = input;
            foreach (char c in invalidChars)
            {
                sanitized = sanitized.Replace(c, '_');
            }
            return sanitized;
        }

        #endregion
    }

    [Serializable]
    public class VOGenerationRequest
    {
        public string key;
        public string text;
        public string voiceId;
        public Action<AudioClip> onSuccess;
        public Action<string> onError;
    }

    [Serializable]
    public class VOBatchItem
    {
        public string key;
        public string text;
        public string voiceId;
    }

    [Serializable]
    public class VOAssetIndex
    {
        public List<VOAssetEntry> entries = new List<VOAssetEntry>();
    }

    [Serializable]
    public class VOAssetEntry
    {
        public string key;
        public string filePath;
    }
}
