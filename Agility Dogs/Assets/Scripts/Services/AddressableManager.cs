using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace AgilityDogs.Services
{
    /// <summary>
    /// Service for managing Addressable assets and content streaming.
    /// Handles loading, caching, and unloading of addressable assets.
    /// </summary>
    public class AddressableManager : MonoBehaviour
    {
        public static AddressableManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private bool preInitializeOnStart = false;
        [SerializeField] private float downloadTimeoutSeconds = 60f;
        [SerializeField] private bool enableCaching = true;

        [Header("Catalogs")]
        [SerializeField] private List<string> catalogUrls = new List<string>();

        // Loaded assets tracking
        private Dictionary<string, AsyncOperationHandle> loadedHandles = new Dictionary<string, AsyncOperationHandle>();
        private Dictionary<string, List<string>> groupAssets = new Dictionary<string, List<string>>();

        // Events
        public event Action<string> OnAssetLoaded;
        public event Action<string, string> OnAssetLoadFailed;
        public event Action<string, float> OnDownloadProgress;
        public event Action<string> OnCatalogUpdated;

        // Properties
        public bool IsInitialized { get; private set; }
        public long CachedSizeBytes { get; private set; }

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

        private async void Start()
        {
            if (preInitializeOnStart)
            {
                await InitializeAsync();
            }
        }

        #region Initialization

        /// <summary>
        /// Initialize the Addressable system and check for catalog updates.
        /// </summary>
        public async System.Threading.Tasks.Task InitializeAsync()
        {
            if (IsInitialized) return;

            try
            {
                Debug.Log("[AddressableManager] Initializing...");

                // Check for catalog updates
                var checkHandle = Addressables.CheckForCatalogUpdates();
                await checkHandle.Task;

                if (checkHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    var catalogs = checkHandle.Result;
                    if (catalogs != null && catalogs.Count > 0)
                    {
                        Debug.Log($"[AddressableManager] Found {catalogs.Count} catalog updates");
                        await UpdateCatalogsAsync(catalogs);
                    }
                }
                Addressables.Release(checkHandle);

                // Calculate cached size
                await CalculateCachedSizeAsync();

                IsInitialized = true;
                Debug.Log("[AddressableManager] Initialization complete");
            }
            catch (Exception e)
            {
                Debug.LogError($"[AddressableManager] Initialization failed: {e.Message}");
            }
        }

        /// <summary>
        /// Update catalogs from remote URLs.
        /// </summary>
        private async System.Threading.Tasks.Task UpdateCatalogsAsync(List<string> catalogs)
        {
            foreach (var catalog in catalogs)
            {
                var updateHandle = Addressables.UpdateCatalogs(catalog);
                await updateHandle.Task;

                if (updateHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    OnCatalogUpdated?.Invoke(catalog);
                    Debug.Log($"[AddressableManager] Catalog updated: {catalog}");
                }
                else
                {
                    Debug.LogWarning($"[AddressableManager] Failed to update catalog: {catalog}");
                }

                Addressables.Release(updateHandle);
            }
        }

        /// <summary>
        /// Calculate total cached size.
        /// </summary>
        private async System.Threading.Tasks.Task CalculateCachedSizeAsync()
        {
            var sizeHandle = Addressables.GetDownloadSizeAsync(string.Empty);
            await sizeHandle.Task;

            if (sizeHandle.Status == AsyncOperationStatus.Succeeded)
            {
                CachedSizeBytes = sizeHandle.Result;
                Debug.Log($"[AddressableManager] Cached size: {CachedSizeBytes / (1024 * 1024)}MB");
            }

            Addressables.Release(sizeHandle);
        }

        #endregion

        #region Asset Loading

        /// <summary>
        /// Load an asset by address.
        /// </summary>
        public void LoadAsset<T>(string address, Action<T> onSuccess, Action<string> onError = null) where T : class
        {
            if (loadedHandles.TryGetValue(address, out var existingHandle))
            {
                if (existingHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    onSuccess?.Invoke(existingHandle.Result as T);
                    return;
                }
            }

            var handle = Addressables.LoadAssetAsync<T>(address);
            handle.Completed += (op) =>
            {
                if (op.Status == AsyncOperationStatus.Succeeded)
                {
                    loadedHandles[address] = op;
                    onSuccess?.Invoke(op.Result);
                    OnAssetLoaded?.Invoke(address);
                }
                else
                {
                    string error = $"Failed to load asset: {address}";
                    Debug.LogError($"[AddressableManager] {error}");
                    onError?.Invoke(error);
                    OnAssetLoadFailed?.Invoke(address, error);
                    Addressables.Release(op);
                }
            };
        }

        /// <summary>
        /// Load an asset by reference.
        /// </summary>
        public void LoadAsset<T>(AssetReference reference, Action<T> onSuccess, Action<string> onError = null) where T : class
        {
            if (reference == null)
            {
                onError?.Invoke("Asset reference is null");
                return;
            }

            var handle = reference.LoadAssetAsync<T>();
            handle.Completed += (op) =>
            {
                if (op.Status == AsyncOperationStatus.Succeeded)
                {
                    loadedHandles[reference.AssetGUID] = op;
                    onSuccess?.Invoke(op.Result);
                    OnAssetLoaded?.Invoke(reference.AssetGUID);
                }
                else
                {
                    string error = $"Failed to load asset reference: {reference.AssetGUID}";
                    Debug.LogError($"[AddressableManager] {error}");
                    onError?.Invoke(error);
                    OnAssetLoadFailed?.Invoke(reference.AssetGUID, error);
                }
            };
        }

        /// <summary>
        /// Load multiple assets by label.
        /// </summary>
        public void LoadAssetsByLabel<T>(string label, Action<IList<T>> onSuccess, Action<string> onError = null) where T : class
        {
            var handle = Addressables.LoadAssetsAsync<T>(label, null);
            handle.Completed += (op) =>
            {
                if (op.Status == AsyncOperationStatus.Succeeded)
                {
                    onSuccess?.Invoke(op.Result);
                    
                    // Track loaded assets
                    foreach (var asset in op.Result)
                    {
                        string key = $"{label}_{asset.GetInstanceID()}";
                        loadedHandles[key] = handle; // Note: same handle for all
                    }
                }
                else
                {
                    string error = $"Failed to load assets with label: {label}";
                    Debug.LogError($"[AddressableManager] {error}");
                    onError?.Invoke(error);
                    Addressables.Release(handle);
                }
            };
        }

        /// <summary>
        /// Instantiate a prefab from Addressables.
        /// </summary>
        public void InstantiateAsync(string address, Vector3 position, Quaternion rotation, Transform parent = null, Action<GameObject> onSuccess = null, Action<string> onError = null)
        {
            var handle = Addressables.InstantiateAsync(address, position, rotation, parent);
            handle.Completed += (op) =>
            {
                if (op.Status == AsyncOperationStatus.Succeeded)
                {
                    string instanceKey = $"instance_{address}_{op.Result.GetInstanceID()}";
                    loadedHandles[instanceKey] = op;
                    onSuccess?.Invoke(op.Result);
                    OnAssetLoaded?.Invoke(address);
                }
                else
                {
                    string error = $"Failed to instantiate: {address}";
                    Debug.LogError($"[AddressableManager] {error}");
                    onError?.Invoke(error);
                    OnAssetLoadFailed?.Invoke(address, error);
                    Addressables.Release(handle);
                }
            };
        }

        /// <summary>
        /// Instantiate a prefab from AssetReference.
        /// </summary>
        public void InstantiateAsync(AssetReference reference, Vector3 position, Quaternion rotation, Transform parent = null, Action<GameObject> onSuccess = null, Action<string> onError = null)
        {
            if (reference == null)
            {
                onError?.Invoke("Asset reference is null");
                return;
            }

            var handle = reference.InstantiateAsync(position, rotation, parent);
            handle.Completed += (op) =>
            {
                if (op.Status == AsyncOperationStatus.Succeeded)
                {
                    string instanceKey = $"instance_{reference.AssetGUID}_{op.Result.GetInstanceID()}";
                    loadedHandles[instanceKey] = op;
                    onSuccess?.Invoke(op.Result);
                    OnAssetLoaded?.Invoke(reference.AssetGUID);
                }
                else
                {
                    string error = $"Failed to instantiate reference: {reference.AssetGUID}";
                    Debug.LogError($"[AddressableManager] {error}");
                    onError?.Invoke(error);
                    OnAssetLoadFailed?.Invoke(reference.AssetGUID, error);
                }
            };
        }

        /// <summary>
        /// Load a scene using Addressables.
        /// </summary>
        public void LoadSceneAsync(string sceneAddress, LoadSceneMode mode = LoadSceneMode.Single, Action<SceneInstance> onSuccess = null, Action<string> onError = null)
        {
            var handle = Addressables.LoadSceneAsync(sceneAddress, mode);
            handle.Completed += (op) =>
            {
                if (op.Status == AsyncOperationStatus.Succeeded)
                {
                    loadedHandles[sceneAddress] = handle; // Scene handles must be kept
                    onSuccess?.Invoke(op.Result);
                    OnAssetLoaded?.Invoke(sceneAddress);
                }
                else
                {
                    string error = $"Failed to load scene: {sceneAddress}";
                    Debug.LogError($"[AddressableManager] {error}");
                    onError?.Invoke(error);
                    OnAssetLoadFailed?.Invoke(sceneAddress, error);
                    Addressables.Release(handle);
                }
            };
        }

        #endregion

        #region Download Management

        /// <summary>
        /// Download assets for a specific label before they're needed.
        /// </summary>
        public void PreDownloadAssets(string label, Action onSuccess = null, Action<string> onError = null)
        {
            var sizeHandle = Addressables.GetDownloadSizeAsync(label);
            sizeHandle.Completed += (op) =>
            {
                if (op.Status == AsyncOperationStatus.Succeeded && op.Result > 0)
                {
                    Debug.Log($"[AddressableManager] Downloading {op.Result / (1024 * 1024)}MB for label: {label}");
                    
                    var downloadHandle = Addressables.DownloadDependenciesAsync(label);
                    downloadHandle.Completed += (dlOp) =>
                    {
                        if (dlOp.Status == AsyncOperationStatus.Succeeded)
                        {
                            Debug.Log($"[AddressableManager] Download complete for: {label}");
                            onSuccess?.Invoke();
                        }
                        else
                        {
                            string error = $"Download failed for: {label}";
                            Debug.LogError($"[AddressableManager] {error}");
                            onError?.Invoke(error);
                        }
                        Addressables.Release(dlHandle);
                    };

                    // Monitor download progress
                    StartCoroutine(MonitorDownloadProgress(downloadHandle, label));
                }
                else if (op.Status == AsyncOperationStatus.Succeeded)
                {
                    Debug.Log($"[AddressableManager] No download needed for: {label}");
                    onSuccess?.Invoke();
                }
                else
                {
                    string error = $"Failed to get download size for: {label}";
                    onError?.Invoke(error);
                }
                Addressables.Release(sizeHandle);
            };
        }

        private System.Collections.IEnumerator MonitorDownloadProgress(AsyncOperationHandle handle, string label)
        {
            while (!handle.IsDone)
            {
                OnDownloadProgress?.Invoke(label, handle.PercentComplete);
                yield return null;
            }
        }

        #endregion

        #region Asset Management

        /// <summary>
        /// Release a loaded asset.
        /// </summary>
        public void ReleaseAsset(string key)
        {
            if (loadedHandles.TryGetValue(key, out var handle))
            {
                Addressables.Release(handle);
                loadedHandles.Remove(key);
                Debug.Log($"[AddressableManager] Released asset: {key}");
            }
        }

        /// <summary>
        /// Release an instantiated object.
        /// </summary>
        public void ReleaseInstance(GameObject instance)
        {
            if (instance == null) return;

            string key = null;
            foreach (var kvp in loadedHandles)
            {
                if (kvp.Key.StartsWith("instance_") && kvp.Value.Result as GameObject == instance)
                {
                    key = kvp.Key;
                    break;
                }
            }

            if (key != null)
            {
                Addressables.ReleaseInstance(loadedHandles[key]);
                loadedHandles.Remove(key);
                Debug.Log($"[AddressableManager] Released instance: {instance.name}");
            }
            else
            {
                Addressables.ReleaseInstance(instance);
            }
        }

        /// <summary>
        /// Clear all loaded assets.
        /// </summary>
        public void ReleaseAll()
        {
            foreach (var kvp in loadedHandles)
            {
                Addressables.Release(kvp.Value);
            }
            loadedHandles.Clear();
            Debug.Log("[AddressableManager] Released all assets");
        }

        /// <summary>
        /// Clear unused assets.
        /// </summary>
        public void ClearUnusedAssets()
        {
            var toRemove = new List<string>();
            
            foreach (var kvp in loadedHandles)
            {
                if (!kvp.Value.IsDone || kvp.Value.Status != AsyncOperationStatus.Succeeded)
                {
                    toRemove.Add(kvp.Key);
                }
            }

            foreach (var key in toRemove)
            {
                ReleaseAsset(key);
            }

            Resources.UnloadUnusedAssets();
            GC.Collect();
        }

        /// <summary>
        /// Check if an asset is loaded.
        /// </summary>
        public bool IsAssetLoaded(string key)
        {
            return loadedHandles.ContainsKey(key) && loadedHandles[key].Status == AsyncOperationStatus.Succeeded;
        }

        /// <summary>
        /// Get a loaded asset.
        /// </summary>
        public T GetLoadedAsset<T>(string key) where T : class
        {
            if (loadedHandles.TryGetValue(key, out var handle) && handle.Status == AsyncOperationStatus.Succeeded)
            {
                return handle.Result as T;
            }
            return null;
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
            sb.AppendLine($"Loaded Assets: {loadedHandles.Count}");
            sb.AppendLine($"Cached Size: {CachedSizeBytes / (1024 * 1024)}MB");
            sb.AppendLine();
            sb.AppendLine("Loaded Assets:");
            
            foreach (var kvp in loadedHandles)
            {
                sb.AppendLine($"  - {kvp.Key}: {kvp.Value.Status}");
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
    /// Helper class for Addressable asset references in components.
    /// </summary>
    [Serializable]
    public class AddressableAssetRef
    {
        public AssetReference reference;
        public string fallbackAddress;
        public string label;
        
        [NonSerialized]
        public bool isLoaded;
        
        [NonSerialized]
        public object loadedAsset;
    }

    /// <summary>
    /// Attribute to mark fields for Addressable conversion.
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