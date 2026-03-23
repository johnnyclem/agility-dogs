using System;
using System.Collections.Generic;
using UnityEngine;

namespace AgilityDogs.Services
{
    /// <summary>
    /// Manages platform-specific features and performance budgets.
    /// Handles WebGL constraints, LOD management, and telemetry.
    /// </summary>
    public class PlatformManager : MonoBehaviour
    {
        public static PlatformManager Instance { get; private set; }

        [Header("Platform Detection")]
        [SerializeField] private bool autoDetectPlatform = true;
        [SerializeField] private PlatformProfile platformProfile;

        [Header("Performance Budgets")]
        [SerializeField] private int targetFrameRate = 60;
        [SerializeField] private float maxMemoryMB = 512f;
        [SerializeField] private int maxDrawCalls = 1000;
        [SerializeField] private int maxTriangles = 500000;

        [Header("Quality Settings")]
        [SerializeField] private QualityLevel currentQuality = QualityLevel.Medium;
        [SerializeField] private bool dynamicResolution = true;

        [Header("WebGL Specific")]
        [SerializeField] private bool webglMemoryWarningShown = false;
        [SerializeField] private int webglMaxTextureSize = 2048;
        [SerializeField] private bool webglCompressTextures = true;

        // Performance monitoring
        private float[] frameTimes = new float[60];
        private int frameTimeIndex;
        private float currentFPS;
        private float currentMemory;

        // Telemetry
        private bool telemetryEnabled = false;
        private Dictionary<string, object> telemetryData = new Dictionary<string, object>();

        // Events
        public event Action<PlatformProfile> OnPlatformDetected;
        public event Action<QualityLevel> OnQualityChanged;
        public event Action<float> OnPerformanceWarning;
        public event Action<Dictionary<string, object>> OnTelemetryEvent;

        // Properties
        public PlatformProfile PlatformProfile => platformProfile;
        public QualityLevel CurrentQuality => currentQuality;
        public float CurrentFPS => currentFPS;
        public bool IsWebGL => Application.platform == RuntimePlatform.WebGLPlayer;
        public bool IsMobile =>
            Application.platform == RuntimePlatform.Android ||
            Application.platform == RuntimePlatform.IPhonePlayer;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (autoDetectPlatform)
            {
                DetectPlatform();
            }

            ApplyPlatformOptimizations();
            LoadQualitySettings();

            // Check WebGL memory
            if (IsWebGL)
            {
                InvokeRepeating(nameof(CheckWebGLMemory), 10f, 10f);
            }
        }

        private void Update()
        {
            UpdatePerformanceMetrics();
            CheckPerformanceBudgets();

            if (dynamicResolution)
            {
                AdjustDynamicResolution();
            }
        }

        #region Platform Detection

        private void DetectPlatform()
        {
            platformProfile = new PlatformProfile
            {
                platform = Application.platform,
                isWebGL = IsWebGL,
                isMobile = IsMobile,
                systemMemory = SystemInfo.systemMemorySize,
                graphicsMemory = SystemInfo.graphicsMemorySize,
                processorCount = SystemInfo.processorCount,
                supportsInstancing = SystemInfo.supportsInstancing,
                maxTextureSize = SystemInfo.maxTextureSize,
                supportsMotionVectors = SystemInfo.supportsMotionVectors
            };

            // Set recommended quality based on platform
            if (IsWebGL)
            {
                platformProfile.recommendedQuality = QualityLevel.Low;
                platformProfile.maxCrowdSize = 50;
                platformProfile.enableLOD = false;
            }
            else if (IsMobile)
            {
                platformProfile.recommendedQuality = QualityLevel.Low;
                platformProfile.maxCrowdSize = 100;
                platformProfile.enableLOD = true;
            }
            else
            {
                platformProfile.recommendedQuality = QualityLevel.High;
                platformProfile.maxCrowdSize = 250;
                platformProfile.enableLOD = true;
            }

            OnPlatformDetected?.Invoke(platformProfile);
            Debug.Log($"[Platform] Detected: {platformProfile.platform}, Quality: {platformProfile.recommendedQuality}");
        }

        #endregion

        #region Quality Management

        public void SetQualityLevel(QualityLevel level)
        {
            currentQuality = level;
            PlayerPrefs.SetInt("QualityLevel", (int)level);

            ApplyQualitySettings(level);
            OnQualityChanged?.Invoke(level);

            TrackTelemetry("quality_changed", new Dictionary<string, object>
            {
                { "quality", level.ToString() },
                { "platform", Application.platform.ToString() }
            });
        }

        private void ApplyQualitySettings(QualityLevel level)
        {
            var settings = GetQualitySettings(level);

            // Apply to Unity QualitySettings
            QualitySettings.shadowDistance = settings.shadowDistance;
            QualitySettings.antiAliasing = settings.antiAliasing;
            QualitySettings.vSyncCount = settings.vSyncCount;
            QualitySettings.particleRaycastBudget = settings.particleRaycastBudget;

            // Set target frame rate
            Application.targetFrameRate = targetFrameRate;

            // Apply texture quality
            QualitySettings.globalTextureMipmapLimit = settings.textureQuality;

            Debug.Log($"[Platform] Applied quality settings: {level}");
        }

        private QualitySettingsData GetQualitySettings(QualityLevel level)
        {
            return level switch
            {
                QualityLevel.Low => new QualitySettingsData
                {
                    shadowDistance = 20f,
                    antiAliasing = 0,
                    vSyncCount = 0,
                    particleRaycastBudget = 4,
                    textureQuality = 2 // Half res
                },
                QualityLevel.Medium => new QualitySettingsData
                {
                    shadowDistance = 40f,
                    antiAliasing = 2,
                    vSyncCount = 1,
                    particleRaycastBudget = 16,
                    textureQuality = 1 // Quarter res
                },
                QualityLevel.High => new QualitySettingsData
                {
                    shadowDistance = 80f,
                    antiAliasing = 4,
                    vSyncCount = 1,
                    particleRaycastBudget = 64,
                    textureQuality = 0 // Full res
                },
                QualityLevel.Ultra => new QualitySettingsData
                {
                    shadowDistance = 120f,
                    antiAliasing = 8,
                    vSyncCount = 2,
                    particleRaycastBudget = 256,
                    textureQuality = 0
                },
                _ => GetQualitySettings(QualityLevel.Medium)
            };
        }

        private void LoadQualitySettings()
        {
            int savedQuality = PlayerPrefs.GetInt("QualityLevel", -1);
            if (savedQuality >= 0)
            {
                SetQualityLevel((QualityLevel)savedQuality);
            }
            else if (platformProfile != null)
            {
                SetQualityLevel(platformProfile.recommendedQuality);
            }
        }

        #endregion

        #region Performance Monitoring

        private void UpdatePerformanceMetrics()
        {
            // Track frame times
            frameTimes[frameTimeIndex] = Time.unscaledDeltaTime;
            frameTimeIndex = (frameTimeIndex + 1) % frameTimes.Length;

            // Calculate FPS
            float totalTime = 0f;
            for (int i = 0; i < frameTimes.Length; i++)
            {
                totalTime += frameTimes[i];
            }
            currentFPS = frameTimes.Length / totalTime;

            // Track memory
            currentMemory = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() / (1024f * 1024f);
        }

        private void CheckPerformanceBudgets()
        {
            // Check FPS
            if (currentFPS < targetFrameRate * 0.8f)
            {
                OnPerformanceWarning?.Invoke(currentFPS);
            }

            // Check memory
            if (currentMemory > maxMemoryMB * 0.9f)
            {
                Debug.LogWarning($"[Platform] Memory warning: {currentMemory:F1}MB / {maxMemoryMB}MB");
                OnPerformanceWarning?.Invoke(currentMemory);
            }
        }

        private void AdjustDynamicResolution()
        {
            // Reduce quality if performance is poor
            if (currentFPS < targetFrameRate * 0.7f)
            {
                // Could reduce resolution scale here
            }
        }

        private void CheckWebGLMemory()
        {
            if (!IsWebGL) return;

            // WebGL has limited memory
            if (currentMemory > 256f && !webglMemoryWarningShown)
            {
                webglMemoryWarningShown = true;
                Debug.LogWarning("[Platform] WebGL memory usage is high. Consider reducing quality settings.");
                OnPerformanceWarning?.Invoke(currentMemory);
            }
        }

        #endregion

        #region Platform Optimizations

        private void ApplyPlatformOptimizations()
        {
            if (IsWebGL)
            {
                // WebGL-specific optimizations
                Application.targetFrameRate = 30;
                webglMemoryWarningShown = false;
            }
            else if (IsMobile)
            {
                // Mobile-specific optimizations
                Application.targetFrameRate = 60;
                Screen.sleepTimeout = SleepTimeout.NeverSleep;
            }
        }

        #endregion

        #region Telemetry

        public void EnableTelemetry(bool enabled)
        {
            telemetryEnabled = enabled;
            Debug.Log($"[Platform] Telemetry {(enabled ? "enabled" : "disabled")}");
        }

        public void TrackEvent(string eventName, Dictionary<string, object> data = null)
        {
            if (!telemetryEnabled) return;

            var eventData = data ?? new Dictionary<string, object>();
            eventData["timestamp"] = DateTime.Now.ToString("o");
            eventData["platform"] = Application.platform.ToString();
            eventData["fps"] = currentFPS;
            eventData["memory"] = currentMemory;

            OnTelemetryEvent?.Invoke(eventData);
        }

        public void TrackTelemetry(string eventName, Dictionary<string, object> data = null)
        {
            TrackEvent(eventName, data);
        }

        #endregion

        #region Public API

        public bool IsFeatureSupported(string feature)
        {
            return feature switch
            {
                "hd_textures" => currentQuality >= QualityLevel.High,
                "shadows" => currentQuality >= QualityLevel.Medium,
                "antialiasing" => true,
                "postprocessing" => currentQuality >= QualityLevel.Medium,
                "crowd_250" => !IsWebGL && !IsMobile,
                "crowd_100" => !IsMobile,
                _ => true
            };
        }

        public int GetRecommendedCrowdSize()
        {
            return platformProfile?.maxCrowdSize ?? 100;
        }

        public void ReportCrash(Exception exception)
        {
            Debug.LogError($"[Platform] Crash reported: {exception.Message}");
            TrackEvent("crash", new Dictionary<string, object>
            {
                { "message", exception.Message },
                { "stackTrace", exception.StackTrace }
            });
        }

        #endregion
    }

    [Serializable]
    public class PlatformProfile
    {
        public RuntimePlatform platform;
        public bool isWebGL;
        public bool isMobile;
        public int systemMemory;
        public int graphicsMemory;
        public int processorCount;
        public bool supportsInstancing;
        public int maxTextureSize;
        public bool supportsMotionVectors;

        // Recommendations
        public QualityLevel recommendedQuality;
        public int maxCrowdSize;
        public bool enableLOD;
    }

    [Serializable]
    public class QualitySettingsData
    {
        public float shadowDistance;
        public int antiAliasing;
        public int vSyncCount;
        public int particleRaycastBudget;
        public int textureQuality;
    }

    public enum QualityLevel
    {
        Low,
        Medium,
        High,
        Ultra
    }
}
