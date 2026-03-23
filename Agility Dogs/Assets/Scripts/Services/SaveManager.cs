using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace AgilityDogs.Services
{
    /// <summary>
    /// Manages save/load operations with cloud save abstraction and local fallback.
    /// Supports multiple save slots and automatic backup creation.
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        [Header("Save Configuration")]
        [SerializeField] private int maxSaveSlots = 3;
        [SerializeField] private bool autoSaveEnabled = true;
        [SerializeField] private float autoSaveInterval = 60f; // seconds
        [SerializeField] private bool createBackups = true;

        [Header("Cloud Save")]
        [SerializeField] private bool cloudSaveEnabled = false;
        [SerializeField] private float cloudSyncInterval = 300f; // seconds

        // State
        private float lastAutoSaveTime;
        private float lastCloudSyncTime;
        private string currentSlotId;
        private bool isDirty;

        // Events
        public event Action<string> OnSaveCompleted; // slotId
        public event Action<string> OnLoadCompleted; // slotId
        public event Action<string> OnSaveFailed; // errorMessage
        public event Action<string> OnCloudSyncCompleted;
        public event Action<string> OnCloudSyncFailed;

        // Properties
        public bool IsCloudSaveEnabled => cloudSaveEnabled;
        public string CurrentSlotId => currentSlotId;
        public int MaxSaveSlots => maxSaveSlots;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            currentSlotId = "slot_1"; // Default slot
            LoadSlotManifest();
        }

        private void Update()
        {
            if (!autoSaveEnabled) return;

            if (isDirty && Time.time - lastAutoSaveTime > autoSaveInterval)
            {
                AutoSave();
            }
        }

        #region Save Operations

        /// <summary>
        /// Save game data to the specified slot.
        /// </summary>
        public bool SaveToSlot(string slotId, SaveData data)
        {
            if (string.IsNullOrEmpty(slotId) || data == null)
            {
                OnSaveFailed?.Invoke("Invalid slot ID or data");
                return false;
            }

            try
            {
                // Update metadata
                data.lastSaved = DateTime.Now;
                data.slotId = slotId;
                data.version = Application.version;

                // Serialize to JSON
                string json = JsonUtility.ToJson(data, true);
                string savePath = GetSaveFilePath(slotId);

                // Create backup if enabled
                if (createBackups && File.Exists(savePath))
                {
                    CreateBackup(savePath);
                }

                // Write save file
                File.WriteAllText(savePath, json);

                // Update slot manifest
                UpdateSlotManifest(slotId, data);

                // Mark as clean
                isDirty = false;
                lastAutoSaveTime = Time.time;

                OnSaveCompleted?.Invoke(slotId);
                Debug.Log($"[SaveManager] Saved to slot: {slotId}");

                // Trigger cloud sync if enabled
                if (cloudSaveEnabled)
                {
                    SyncToCloud(slotId, json);
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to save: {e.Message}");
                OnSaveFailed?.Invoke(e.Message);
                return false;
            }
        }

        /// <summary>
        /// Load game data from the specified slot.
        /// </summary>
        public SaveData LoadFromSlot(string slotId)
        {
            if (string.IsNullOrEmpty(slotId)) return null;

            string savePath = GetSaveFilePath(slotId);

            if (!File.Exists(savePath))
            {
                Debug.LogWarning($"[SaveManager] No save file found for slot: {slotId}");
                return null;
            }

            try
            {
                string json = File.ReadAllText(savePath);
                var data = JsonUtility.FromJson<SaveData>(json);

                currentSlotId = slotId;
                OnLoadCompleted?.Invoke(slotId);
                Debug.Log($"[SaveManager] Loaded from slot: {slotId}");

                return data;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to load: {e.Message}");

                // Try to load backup
                if (createBackups)
                {
                    return LoadBackup(slotId);
                }

                return null;
            }
        }

        /// <summary>
        /// Delete save data from the specified slot.
        /// </summary>
        public bool DeleteSlot(string slotId)
        {
            string savePath = GetSaveFilePath(slotId);

            if (!File.Exists(savePath)) return false;

            try
            {
                File.Delete(savePath);

                // Also delete backups
                string backupPattern = $"{slotId}_backup_*.json";
                string saveDir = GetSaveDirectory();
                foreach (string backup in Directory.GetFiles(saveDir, backupPattern))
                {
                    File.Delete(backup);
                }

                // Update manifest
                RemoveFromSlotManifest(slotId);

                Debug.Log($"[SaveManager] Deleted slot: {slotId}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to delete slot: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get list of all available save slots.
        /// </summary>
        public SaveSlotInfo[] GetAllSlots()
        {
            var manifest = LoadSlotManifest();
            return manifest.slots.Values.ToArray();
        }

        /// <summary>
        /// Switch to a different save slot.
        /// </summary>
        public bool SwitchToSlot(string slotId)
        {
            if (string.IsNullOrEmpty(slotId)) return false;
            currentSlotId = slotId;
            return true;
        }

        /// <summary>
        /// Mark save data as dirty (needs saving).
        /// </summary>
        public void MarkDirty()
        {
            isDirty = true;
        }

        #endregion

        #region Auto Save

        private void AutoSave()
        {
            if (string.IsNullOrEmpty(currentSlotId)) return;

            // Gather save data from all services
            var saveData = GatherSaveData();
            SaveToSlot(currentSlotId, saveData);
        }

        private SaveData GatherSaveData()
        {
            var data = new SaveData
            {
                gameVersion = Application.version,
                playTimeSeconds = GetTotalPlayTime()
            };

            // Gather data from services
            if (CareerProgressionService.Instance != null)
            {
                // Would serialize career progression data
            }

            if (SkillTreeService.Instance != null)
            {
                // Would serialize skill tree data
            }

            if (TrainingManager.Instance != null)
            {
                // Would serialize training progress
            }

            return data;
        }

        private float GetTotalPlayTime()
        {
            // Would track actual play time
            return Time.realtimeSinceStartup;
        }

        #endregion

        #region Cloud Save

        /// <summary>
        /// Sync save data to cloud storage.
        /// </summary>
        public async void SyncToCloud(string slotId, string jsonData)
        {
            if (!cloudSaveEnabled) return;

            try
            {
                // This would integrate with platform-specific cloud save:
                // - Steam Cloud
                // - Xbox Live Save
                // - PlayStation Plus Storage
                // - Nintendo Switch Online
                // - Epic Games Save System
                // - Generic REST API

                Debug.Log($"[SaveManager] Syncing to cloud: {slotId}");

                // Simulate cloud sync
                await System.Threading.Tasks.Task.Delay(100);

                lastCloudSyncTime = Time.time;
                OnCloudSyncCompleted?.Invoke(slotId);
                Debug.Log($"[SaveManager] Cloud sync completed: {slotId}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Cloud sync failed: {e.Message}");
                OnCloudSyncFailed?.Invoke(e.Message);
            }
        }

        /// <summary>
        /// Load save data from cloud storage.
        /// </summary>
        public async void LoadFromCloud(string slotId)
        {
            if (!cloudSaveEnabled) return;

            try
            {
                Debug.Log($"[SaveManager] Loading from cloud: {slotId}");

                // Simulate cloud load
                await System.Threading.Tasks.Task.Delay(100);

                // Would download and save locally
                Debug.Log($"[SaveManager] Cloud load completed: {slotId}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Cloud load failed: {e.Message}");
            }
        }

        #endregion

        #region Backup System

        private void CreateBackup(string originalPath)
        {
            try
            {
                string backupPath = GetBackupPath(originalPath);
                File.Copy(originalPath, backupPath, true);

                // Keep only last 3 backups
                CleanupOldBackups(originalPath);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveManager] Failed to create backup: {e.Message}");
            }
        }

        private SaveData LoadBackup(string slotId)
        {
            try
            {
                string savePath = GetSaveFilePath(slotId);
                string backupPath = GetBackupPath(savePath);

                if (File.Exists(backupPath))
                {
                    string json = File.ReadAllText(backupPath);
                    var data = JsonUtility.FromJson<SaveData>(json);

                    Debug.Log($"[SaveManager] Loaded from backup: {slotId}");
                    return data;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to load backup: {e.Message}");
            }

            return null;
        }

        private string GetBackupPath(string originalPath)
        {
            string directory = Path.GetDirectoryName(originalPath);
            string fileName = Path.GetFileNameWithoutExtension(originalPath);
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            return Path.Combine(directory, $"{fileName}_backup_{timestamp}.json");
        }

        private void CleanupOldBackups(string originalPath)
        {
            string directory = Path.GetDirectoryName(originalPath);
            string fileName = Path.GetFileNameWithoutExtension(originalPath);
            string pattern = $"{fileName}_backup_*.json";

            var backupFiles = Directory.GetFiles(directory, pattern)
                .OrderByDescending(f => File.GetCreationTime(f))
                .Skip(3) // Keep last 3
                .ToList();

            foreach (string backup in backupFiles)
            {
                try
                {
                    File.Delete(backup);
                }
                catch { }
            }
        }

        #endregion

        #region Slot Manifest

        private SlotManifest LoadSlotManifest()
        {
            string manifestPath = GetManifestPath();

            if (File.Exists(manifestPath))
            {
                try
                {
                    string json = File.ReadAllText(manifestPath);
                    return JsonUtility.FromJson<SlotManifest>(json);
                }
                catch { }
            }

            return new SlotManifest();
        }

        private void UpdateSlotManifest(string slotId, SaveData data)
        {
            var manifest = LoadSlotManifest();

            manifest.slots[slotId] = new SaveSlotInfo
            {
                slotId = slotId,
                lastSaved = DateTime.Now,
                playTimeSeconds = data.playTimeSeconds,
                playerLevel = 0, // Would get from career service
                version = Application.version
            };

            SaveSlotManifest(manifest);
        }

        private void RemoveFromSlotManifest(string slotId)
        {
            var manifest = LoadSlotManifest();

            if (manifest.slots.ContainsKey(slotId))
            {
                manifest.slots.Remove(slotId);
                SaveSlotManifest(manifest);
            }
        }

        private void SaveSlotManifest(SlotManifest manifest)
        {
            string json = JsonUtility.ToJson(manifest, true);
            File.WriteAllText(GetManifestPath(), json);
        }

        #endregion

        #region Helpers

        private string GetSaveDirectory()
        {
            string path = Path.Combine(Application.persistentDataPath, "Saves");
            Directory.CreateDirectory(path);
            return path;
        }

        private string GetSaveFilePath(string slotId)
        {
            return Path.Combine(GetSaveDirectory(), $"{slotId}.json");
        }

        private string GetManifestPath()
        {
            return Path.Combine(GetSaveDirectory(), "manifest.json");
        }

        #endregion
    }

    [Serializable]
    public class SaveData
    {
        public string slotId;
        public string gameVersion;
        public string version;
        public DateTime lastSaved;
        public float playTimeSeconds;

        // Progression data
        public int playerLevel;
        public int totalXP;
        public int careerWings;

        // Collections
        public string[] unlockedHandlers;
        public string[] unlockedDogs;
        public string[] unlockedVenues;

        // Skill trees
        public int availableSkillPoints;
        public string[] unlockedSkills;

        // Settings
        public bool highContrastMode;
        public bool reducedMotion;
        public bool screenReaderEnabled;
        public float masterVolume;
        public float musicVolume;
        public float sfxVolume;
        public float commentaryVolume;
    }

    [Serializable]
    public class SlotManifest
    {
        public SerializableDictionary<string, SaveSlotInfo> slots = new SerializableDictionary<string, SaveSlotInfo>();
    }

    [Serializable]
    public class SaveSlotInfo
    {
        public string slotId;
        public DateTime lastSaved;
        public float playTimeSeconds;
        public int playerLevel;
        public string version;
        public string thumbnailBase64; // Optional: save screenshot
    }

    /// <summary>
    /// Simple serializable dictionary for Unity.
    /// </summary>
    [Serializable]
    public class SerializableDictionary<TKey, TValue>
    {
        public List<TKey> keys = new List<TKey>();
        public List<TValue> values = new List<TValue>();

        public Dictionary<TKey, TValue> ToDictionary()
        {
            var dict = new Dictionary<TKey, TValue>();
            for (int i = 0; i < keys.Count; i++)
            {
                dict[keys[i]] = values[i];
            }
            return dict;
        }

        public void FromDictionary(Dictionary<TKey, TValue> dict)
        {
            keys.Clear();
            values.Clear();
            foreach (var kvp in dict)
            {
                keys.Add(kvp.Key);
                values.Add(kvp.Value);
            }
        }
    }

    // Extension class for dictionary access
    public static class SerializableDictionaryExtensions
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this SerializableDictionary<TKey, TValue> dict, TKey key, TValue defaultValue = default)
        {
            var dictionary = dict.ToDictionary();
            return dictionary.TryGetValue(key, out var value) ? value : defaultValue;
        }
    }
}
