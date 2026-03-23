using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_ADDRESSABLES
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
#endif

namespace AgilityDogs.Services
{
    /// <summary>
    /// Service for managing Addressable assets and content streaming.
    /// Handles loading, caching, and unloading of addressables.
    /// Falls back to Resources loading if Addressables package is not installed.
    /// </summary>
    public class AddressableManager : MonoBehaviour
    {
        public static AddressableManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private bool preInitializeOnStart = false;
        [SerializeField] private float downloadTimeoutSeconds = 60f;
        [SerializeField] private bool enableCaching = true;

        [Header("Resources Fallback")]
        [SerializeField] private string resourcesBasePath = "Addressables";

        // Loaded assets tracking
#if UNITY_ADDRESSABLES
        private Dictionary<string, AsyncOperationHandle> loadedHandles = new Dictionary<string, AsyncOperationHandle>();
#endif
        private Dictionary<string, List<string>> groupAssets = new Dictionary<string, List<string>>();
        private Dictionary<string, UnityEngine.Object> loadedResources = new Dictionary<string, UnityEngine.Object>();

        // Events
        public event Action<string> OnAssetLoaded;
        public event Action<string, string> OnAssetLoadFailed;
        public event Action<string, float> OnDownloadProgress;
        public event Action<string> OnCatalogUpdated;

        // Properties
        public bool IsInitialized { get; private set; }
        public long CachedSizeBytes { get; private set; }
        
#if UNITY_ADDRESSABLES
        public bool IsAddressablesAvailable => true;
#else
        public bool IsAddressablesAvailable => false;
#endif

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (preInitializeOnStart)
            {
                Initialize();
            }
        }

        #region Initialization

        /// <summary>
        /// Initialize the asset system.
        /// </summary>
        public void Initialize()
        {
            if (IsInitialized) return;

            try
            {
                Debug.Log($"[AddressableManager] Initializing... (Addressables: {IsAddressablesAvailable})");

#if UNITY_ADDRESSABLES
                // Check for catalog updates
                var checkHandle = Addressables.CheckForCatalogUpdates();
                checkHandle.Completed += (op) =>
                {
                    if (op.Status == AsyncOperationStatus.Succeeded)
                    {
                        var catalogs = op.Result;
                        if (catalogs != null && catalogs.Count > 0)
                        {
                            Debug.Log($"[AddressableManager] Found {catalogs.Count} catalog updates");
                            UpdateCatalogs(catalogs);
                        }
                    }
                    Addressables.Release(checkHandle);
                };
#endif

                IsInitialized = true;
                Debug.Log("[AddressableManager] Initialization complete");
            }
            catch (Exception e)
            {
                Debug.LogError($"[AddressableManager] Initialization failed: {e.Message}");
            }
        }

#if UNITY_ADDRESSABLES
        private void UpdateCatalogs(List<string> catalogs)
        {
            foreach (var catalog in catalogs)
            {
                var updateHandle = Addressables.UpdateCatalogs(catalog);
                updateHandle.Completed += (op) =>
                {
                    if (op.Status == AsyncOperationStatus.Succeeded)
                    {
                        OnCatalogUpdated?.Invoke(catalog);
                        Debug.Log($"[AddressableManager] Catalog updated: {catalog}");
                    }
                    Addressables.Release(updateHandle);
                };
            }
        }
#endif

        #endregion

        #region Asset Loading

        /// <summary>
        /// Load an asset by address or resource path.
        /// </summary>
        public void LoadAsset<T>(string address, Action<T> onSuccess, Action<string> onError = null) where T : UnityEngine.Object
        {
            // Check cache first
            if (loadedResources.TryGetValue(address, out var cached) && cached is T cachedTyped)
            {
                onSuccess?.Invoke(cachedTyped);
                return;
            }

#if UNITY_ADDRESSABLES
            // Try Addressables first
            if (IsAddressablesAvailable)
            {
                var handle = Addressables.LoadAssetAsync<T>(address);
                handle.Completed += (op) =>
                {
                    if (op.Status == AsyncOperationStatus.Succeeded)
                    {
                        loadedHandles[address] = op;
                        loadedResources[address] = op.Result;
                        onSuccess?.Invoke(op.Result);
                        OnAssetLoaded?.Invoke(address);
                    }
                    else
                    {
                        // Fallback to Resources
                        LoadFromResources(address, onSuccess, onError);
                        Addressables.Release(handle);
                    }
                };
                return;
            }
#endif

            // Fallback to Resources
            LoadFromResources(address, onSuccess, onError);
        }

        /// <summary>
        /// Load an asset from Resources folder as fallback.
        /// </summary>
        private void LoadFromResources<T>(string path, Action<T> onSuccess, Action<string> onError = null) where T : UnityEngine.Object
        {
            // Try direct path first
            T asset = Resources.Load<T>(path);
            
            // Try with base path
            if (asset == null && !string.IsNullOrEmpty(resourcesBasePath))
            {
                asset = Resources.Load<T>($"{resourcesBasePath}/{path}");
            }

            if (asset != null)
            {
                loadedResources[path] = asset;
                onSuccess?.Invoke(asset);
                OnAssetLoaded?.Invoke(path);
            }
            else
            {
                string error = $"Failed to load asset: {path}";
                Debug.LogError($"[AddressableManager] {error}");
                onError?.Invoke(error);
                OnAssetLoadFailed?.Invoke(path, error);
            }
        }

        /// <summary>
        /// Load multiple assets by label.
        /// </summary>
        public void LoadAssetsByLabel<T>(string label, Action<IList<T>> onSuccess, Action<string> onError = null) where T : UnityEngine.Object
        {
#if UNITY_ADDRESSABLES
            if (IsAddressablesAvailable)
            {
                var handle = Addressables.LoadAssetsAsync<T>(label, null);
                handle.Completed += (op) =>
                {
                    if (op.Status == AsyncOperationStatus.Succeeded)
                    {
                        onSuccess?.Invoke(op.Result);
                    }
                    else
                    {
                        onError?.Invoke($"Failed to load assets with label: {label}");
                        Addressables.Release(handle);
                    }
                };
                return;
            }
#endif

            // Fallback: Load all from Resources folder with label name as subfolder
            T[] assets = Resources.LoadAll<T>($"{resourcesBasePath}/{label}");
            onSuccess?.Invoke(new List<T>(assets));
        }

        /// <summary>
        /// Instantiate a prefab.
        /// </summary>
        public GameObject Instantiate(string address, Vector3 position, Quaternion rotation, Transform parent = null)
        {
#if UNITY_ADDRESSABLES
            if (IsAddressablesAvailable)
            {
                var handle = Addressables.InstantiateAsync(address, position, rotation, parent);
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    return handle.Result;
                }
                Addressables.Release(handle);
            }
#endif

            // Fallback to Resources
            GameObject prefab = Resources.Load<GameObject>($"{resourcesBasePath}/{address}");
            if (prefab == null)
            {
                prefab = Resources.Load<GameObject>(address);
            }

            if (prefab != null)
            {
                return Instantiate(prefab, position, rotation, parent);
            }

            Debug.LogError($"[AddressableManager] Failed to instantiate: {address}");
            return null;
        }

        /// <summary>
        /// Load a scene.
        /// </summary>
        public void LoadScene(string sceneAddress, LoadSceneMode mode = LoadSceneMode.Single)
        {
#if UNITY_ADDRESSABLES
            if (IsAddressablesAvailable)
            {
                var handle = Addressables.LoadSceneAsync(sceneAddress, mode);
                handle.Completed += (op) =>
                {
                    if (op.Status == AsyncOperationStatus.Succeeded)
                    {
                        Debug.Log($"[AddressableManager] Scene loaded: {sceneAddress}");
                    }
                    else
                    {
                        Debug.LogError($"[AddressableManager] Failed to load scene: {sceneAddress}");
                        Addressables.Release(handle);
                    }
                };
                return;
            }
#endif

            // Fallback to regular scene loading
            SceneManager.LoadScene(sceneAddress, mode);
        }

        #endregion

        #region Asset Management

        /// <summary>
        /// Release a loaded asset.
        /// </summary>
        public void ReleaseAsset(string key)
        {
#if UNITY_ADDRESSABLES
            if (loadedHandles.TryGetValue(key, out var handle))
            {
                Addressables.Release(handle);
                loadedHandles.Remove(key);
            }
#endif
            loadedResources.Remove(key);
            Debug.Log($"[AddressableManager] Released asset: {key}");
        }

        /// <summary>
        /// Clear all loaded assets.
        /// </summary>
        public void ReleaseAll()
        {
#if UNITY_ADDRESSABLES
            foreach (var kvp in loadedHandles)
            {
                Addressables.Release(kvp.Value);
            }
            loadedHandles.Clear();
#endif
            loadedResources.Clear();
            Debug.Log("[AddressableManager] Released all assets");
        }

        /// <summary>
        /// Check if an asset is loaded.
        /// </summary>
        public bool IsAssetLoaded(string key)
        {
            return loadedResources.ContainsKey(key);
        }

        #endregion

        #region Diagnostics

        /// <summary>
        /// Get diagnostic information about loaded assets.
        /// </summary>
        public string GetDiagnosticsInfo()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("=== Addressable Manager Diagnostics ===");
            sb.AppendLine($"Initialized: {IsInitialized}");
            sb.AppendLine($"Addressables Available: {IsAddressablesAvailable}");
            sb.AppendLine($"Loaded Assets: {loadedResources.Count}");
#if UNITY_ADDRESSABLES
            sb.AppendLine($"Addressable Handles: {loadedHandles.Count}");
#endif
            sb.AppendLine();
            sb.AppendLine("Loaded Assets:");
            
            foreach (var kvp in loadedResources)
            {
                sb.AppendLine($"  - {kvp.Key}: {kvp.Value.GetType().Name}");
            }

            return sb.ToString();
        }

        #endregion

        private void OnDestroy()
        {
            ReleaseAll();
        }
    }

    /// <summary>
    /// Helper class for asset references in components.
    /// </summary>
    [Serializable]
    public class AddressableAssetRef
    {
        public string address;
        public string fallbackAddress;
        public string label;
        
        [NonSerialized]
        public bool isLoaded;
        
        [NonSerialized]
        public UnityEngine.Object loadedAsset;
    }

    /// <summary>
    /// Attribute to mark fields for asset conversion.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class AddressableFieldAttribute : Attribute
    {
        public string Label { get; set; }
        
        public AddressableFieldAttribute(string label = null)
        {
            Label = label;
        }
    }
}
