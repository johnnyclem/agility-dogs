using System;
using System.Threading.Tasks;
using UnityEngine;

namespace AgilityDogs.Services
{
    /// <summary>
    /// Platform abstraction layer providing unified access to platform-specific features.
    /// Implements the repository pattern for platform capabilities.
    /// </summary>
    public class PlatformAbstractionLayer : MonoBehaviour
    {
        public static PlatformAbstractionLayer Instance { get; private set; }

        [Header("Platform Services")]
        [SerializeField] private bool autoInitialize = true;

        // Platform implementations
        private IStorageProvider storageProvider;
        private INotificationProvider notificationProvider;
        private IAnalyticsProvider analyticsProvider;
        private IAdvertisingProvider advertisingProvider;
        private ISocialProvider socialProvider;

        // Events
        public event Action OnPlatformInitialized;
        public event Action<string> OnPlatformError;

        // Properties
        public bool IsInitialized { get; private set; }
        public PlatformType CurrentPlatform { get; private set; }
        public IStorageProvider Storage => storageProvider;
        public INotificationProvider Notifications => notificationProvider;
        public IAnalyticsProvider Analytics => analyticsProvider;
        public IAdvertisingProvider Advertising => advertisingProvider;
        public ISocialProvider Social => socialProvider;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (autoInitialize)
            {
                Initialize();
            }
        }

        /// <summary>
        /// Initialize the platform abstraction layer.
        /// </summary>
        public async void Initialize()
        {
            if (IsInitialized) return;

            try
            {
                CurrentPlatform = DetectPlatform();
                Debug.Log($"[PlatformAbstraction] Detected platform: {CurrentPlatform}");

                await InitializePlatformServices();

                IsInitialized = true;
                OnPlatformInitialized?.Invoke();
                Debug.Log("[PlatformAbstraction] Initialization complete");
            }
            catch (Exception e)
            {
                string error = $"Platform initialization failed: {e.Message}";
                Debug.LogError($"[PlatformAbstraction] {error}");
                OnPlatformError?.Invoke(error);
            }
        }

        /// <summary>
        /// Detect the current platform type.
        /// </summary>
        private PlatformType DetectPlatform()
        {
#if UNITY_WEBGL
            return PlatformType.WebGL;
#elif UNITY_IOS
            return PlatformType.iOS;
#elif UNITY_ANDROID
            return PlatformType.Android;
#elif UNITY_STANDALONE_WIN
            return PlatformType.Windows;
#elif UNITY_STANDALONE_OSX
            return PlatformType.MacOS;
#elif UNITY_STANDALONE_LINUX
            return PlatformType.Linux;
#elif UNITY_SWITCH
            return PlatformType.Switch;
#elif UNITY_PS4
            return PlatformType.PS4;
#elif UNITY_XBOXONE
            return PlatformType.XboxOne;
#else
            return PlatformType.Unknown;
#endif
        }

        /// <summary>
        /// Initialize platform-specific services.
        /// </summary>
        private async Task InitializePlatformServices()
        {
            // Initialize storage provider
            storageProvider = CreateStorageProvider();
            if (storageProvider != null)
            {
                await storageProvider.Initialize();
            }

            // Initialize notification provider
            notificationProvider = CreateNotificationProvider();
            if (notificationProvider != null)
            {
                await notificationProvider.Initialize();
            }

            // Initialize analytics provider
            analyticsProvider = CreateAnalyticsProvider();
            if (analyticsProvider != null)
            {
                await analyticsProvider.Initialize();
            }

            // Initialize advertising provider (mobile only)
            if (CurrentPlatform == PlatformType.iOS || CurrentPlatform == PlatformType.Android)
            {
                advertisingProvider = CreateAdvertisingProvider();
                if (advertisingProvider != null)
                {
                    await advertisingProvider.Initialize();
                }
            }

            // Initialize social provider
            socialProvider = CreateSocialProvider();
            if (socialProvider != null)
            {
                await socialProvider.Initialize();
            }
        }

        #region Provider Factories

        private IStorageProvider CreateStorageProvider()
        {
            return CurrentPlatform switch
            {
                PlatformType.WebGL => new WebGLStorageProvider(),
                PlatformType.iOS => new CloudStorageProvider(),
                PlatformType.Android => new CloudStorageProvider(),
                _ => new LocalStorageProvider()
            };
        }

        private INotificationProvider CreateNotificationProvider()
        {
            return CurrentPlatform switch
            {
                PlatformType.iOS => new AppleNotificationProvider(),
                PlatformType.Android => new AndroidNotificationProvider(),
                _ => null // Notifications not supported on other platforms
            };
        }

        private IAnalyticsProvider CreateAnalyticsProvider()
        {
            return new DefaultAnalyticsProvider();
        }

        private IAdvertisingProvider CreateAdvertisingProvider()
        {
            return new DefaultAdvertisingProvider();
        }

        private ISocialProvider CreateSocialProvider()
        {
            return CurrentPlatform switch
            {
                PlatformType.Windows or PlatformType.MacOS or PlatformType.Linux => new SteamSocialProvider(),
                _ => null
            };
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Check if a feature is supported on the current platform.
        /// </summary>
        public bool IsFeatureSupported(PlatformFeature feature)
        {
            return feature switch
            {
                PlatformFeature.CloudSave => CurrentPlatform != PlatformType.WebGL,
                PlatformFeature.Leaderboards => true,
                PlatformFeature.Achievements => CurrentPlatform != PlatformType.WebGL,
                PlatformFeature.Notifications => CurrentPlatform == PlatformType.iOS || CurrentPlatform == PlatformType.Android,
                PlatformFeature.Advertisements => CurrentPlatform == PlatformType.iOS || CurrentPlatform == PlatformType.Android,
                PlatformFeature.SocialFeatures => CurrentPlatform == PlatformType.Windows || CurrentPlatform == PlatformType.MacOS,
                PlatformFeature.HapticFeedback => CurrentPlatform == PlatformType.iOS || CurrentPlatform == PlatformType.Android,
                PlatformFeature.DeepLinking => CurrentPlatform == PlatformType.iOS || CurrentPlatform == PlatformType.Android,
                _ => false
            };
        }

        /// <summary>
        /// Get platform-specific settings.
        /// </summary>
        public PlatformSettings GetPlatformSettings()
        {
            return CurrentPlatform switch
            {
                PlatformType.WebGL => new PlatformSettings
                {
                    MaxMemoryMB = 512,
                    TargetFPS = 30,
                    EnableLOD = false,
                    MaxCrowdSize = 50,
                    TextureQuality = TextureQuality.Half
                },
                PlatformType.iOS or PlatformType.Android => new PlatformSettings
                {
                    MaxMemoryMB = 768,
                    TargetFPS = 60,
                    EnableLOD = true,
                    MaxCrowdSize = 100,
                    TextureQuality = TextureQuality.Half
                },
                _ => new PlatformSettings
                {
                    MaxMemoryMB = 2048,
                    TargetFPS = 60,
                    EnableLOD = true,
                    MaxCrowdSize = 250,
                    TextureQuality = TextureQuality.Full
                }
            };
        }

        /// <summary>
        /// Get the platform name for display.
        /// </summary>
        public string GetPlatformDisplayName()
        {
            return CurrentPlatform switch
            {
                PlatformType.WebGL => "Web Browser",
                PlatformType.iOS => "iOS",
                PlatformType.Android => "Android",
                PlatformType.Windows => "Windows",
                PlatformType.MacOS => "macOS",
                PlatformType.Linux => "Linux",
                PlatformType.Switch => "Nintendo Switch",
                PlatformType.PS4 => "PlayStation 4",
                PlatformType.XboxOne => "Xbox One",
                _ => "Unknown"
            };
        }

        #endregion
    }

    #region Enums

    public enum PlatformType
    {
        Unknown,
        WebGL,
        iOS,
        Android,
        Windows,
        MacOS,
        Linux,
        Switch,
        PS4,
        XboxOne
    }

    public enum PlatformFeature
    {
        CloudSave,
        Leaderboards,
        Achievements,
        Notifications,
        Advertisements,
        SocialFeatures,
        HapticFeedback,
        DeepLinking
    }

    public enum TextureQuality
    {
        Full,
        Half,
        Quarter,
        Eighth
    }

    #endregion

    #region Data Classes

    [Serializable]
    public class PlatformSettings
    {
        public int MaxMemoryMB;
        public int TargetFPS;
        public bool EnableLOD;
        public int MaxCrowdSize;
        public TextureQuality TextureQuality;
    }

    #endregion

    #region Provider Interfaces

    public interface IStorageProvider
    {
        Task Initialize();
        Task Save(string key, string data);
        Task<string> Load(string key);
        Task Delete(string key);
        Task<bool> Exists(string key);
    }

    public interface INotificationProvider
    {
        Task Initialize();
        void ScheduleNotification(string title, string message, DateTime fireTime);
        void CancelAllNotifications();
        void RequestPermission();
        bool HasPermission { get; }
    }

    public interface IAnalyticsProvider
    {
        Task Initialize();
        void TrackEvent(string eventName, Dictionary<string, object> parameters = null);
        void TrackScreenView(string screenName);
        void TrackError(string error, string stackTrace = null);
    }

    public interface IAdvertisingProvider
    {
        Task Initialize();
        void ShowInterstitialAd();
        void ShowRewardedAd(Action<bool> callback);
        bool IsAdReady(AdType type);
    }

    public interface ISocialProvider
    {
        Task Initialize();
        void UnlockAchievement(string achievementId);
        void SubmitScore(string leaderboardId, long score);
        void ShowAchievementsUI();
        void ShowLeaderboardUI(string leaderboardId);
    }

    public enum AdType
    {
        Interstitial,
        Rewarded,
        Banner
    }

    #endregion

    #region Default Implementations

    public class LocalStorageProvider : IStorageProvider
    {
        public Task Initialize() => Task.CompletedTask;

        public Task Save(string key, string data)
        {
            PlayerPrefs.SetString(key, data);
            PlayerPrefs.Save();
            return Task.CompletedTask;
        }

        public Task<string> Load(string key)
        {
            return Task.FromResult(PlayerPrefs.GetString(key, ""));
        }

        public Task Delete(string key)
        {
            PlayerPrefs.DeleteKey(key);
            return Task.CompletedTask;
        }

        public Task<bool> Exists(string key)
        {
            return Task.FromResult(PlayerPrefs.HasKey(key));
        }
    }

    public class WebGLStorageProvider : IStorageProvider
    {
        public Task Initialize()
        {
            Debug.Log("[WebGLStorage] Using browser localStorage");
            return Task.CompletedTask;
        }

        public async Task Save(string key, string data)
        {
            // WebGL uses IndexedDB via JavaScript interop
            // This would use Unity's interop to call JS
            PlayerPrefs.SetString(key, data);
            await Task.CompletedTask;
        }

        public Task<string> Load(string key)
        {
            return Task.FromResult(PlayerPrefs.GetString(key, ""));
        }

        public Task Delete(string key)
        {
            PlayerPrefs.DeleteKey(key);
            return Task.CompletedTask;
        }

        public Task<bool> Exists(string key)
        {
            return Task.FromResult(PlayerPrefs.HasKey(key));
        }
    }

    public class CloudStorageProvider : IStorageProvider
    {
        public Task Initialize()
        {
            Debug.Log("[CloudStorage] Initializing cloud storage");
            return Task.CompletedTask;
        }

        public Task Save(string key, string data)
        {
            // Would integrate with platform cloud save
            PlayerPrefs.SetString(key, data);
            return Task.CompletedTask;
        }

        public Task<string> Load(string key)
        {
            return Task.FromResult(PlayerPrefs.GetString(key, ""));
        }

        public Task Delete(string key)
        {
            PlayerPrefs.DeleteKey(key);
            return Task.CompletedTask;
        }

        public Task<bool> Exists(string key)
        {
            return Task.FromResult(PlayerPrefs.HasKey(key));
        }
    }

    public class DefaultAnalyticsProvider : IAnalyticsProvider
    {
        public Task Initialize()
        {
            Debug.Log("[Analytics] Analytics initialized");
            return Task.CompletedTask;
        }

        public void TrackEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            Debug.Log($"[Analytics] Event: {eventName}");
        }

        public void TrackScreenView(string screenName)
        {
            Debug.Log($"[Analytics] Screen: {screenName}");
        }

        public void TrackError(string error, string stackTrace = null)
        {
            Debug.Log($"[Analytics] Error: {error}");
        }
    }

    public class DefaultAdvertisingProvider : IAdvertisingProvider
    {
        public Task Initialize()
        {
            Debug.Log("[Advertising] Ads initialized");
            return Task.CompletedTask;
        }

        public void ShowInterstitialAd()
        {
            Debug.Log("[Advertising] Showing interstitial ad");
        }

        public void ShowRewardedAd(Action<bool> callback)
        {
            Debug.Log("[Advertising] Showing rewarded ad");
            callback?.Invoke(true);
        }

        public bool IsAdReady(AdType type) => false;
    }

    public class AppleNotificationProvider : INotificationProvider
    {
        public bool HasPermission { get; private set; }

        public Task Initialize()
        {
            Debug.Log("[Notifications] Apple notifications initialized");
            return Task.CompletedTask;
        }

        public void ScheduleNotification(string title, string message, DateTime fireTime)
        {
            Debug.Log($"[Notifications] Scheduled: {title} at {fireTime}");
        }

        public void CancelAllNotifications()
        {
            Debug.Log("[Notifications] Cancelled all");
        }

        public void RequestPermission()
        {
            HasPermission = true;
        }
    }

    public class AndroidNotificationProvider : INotificationProvider
    {
        public bool HasPermission { get; private set; }

        public Task Initialize()
        {
            Debug.Log("[Notifications] Android notifications initialized");
            return Task.CompletedTask;
        }

        public void ScheduleNotification(string title, string message, DateTime fireTime)
        {
            Debug.Log($"[Notifications] Scheduled: {title} at {fireTime}");
        }

        public void CancelAllNotifications()
        {
            Debug.Log("[Notifications] Cancelled all");
        }

        public void RequestPermission()
        {
            HasPermission = true;
        }
    }

    public class SteamSocialProvider : ISocialProvider
    {
        public Task Initialize()
        {
            Debug.Log("[Social] Steam social features initialized");
            return Task.CompletedTask;
        }

        public void UnlockAchievement(string achievementId)
        {
            Debug.Log($"[Social] Achievement unlocked: {achievementId}");
        }

        public void SubmitScore(string leaderboardId, long score)
        {
            Debug.Log($"[Social] Score submitted: {score} to {leaderboardId}");
        }

        public void ShowAchievementsUI()
        {
            Debug.Log("[Social] Showing achievements UI");
        }

        public void ShowLeaderboardUI(string leaderboardId)
        {
            Debug.Log($"[Social] Showing leaderboard: {leaderboardId}");
        }
    }

    #endregion
}