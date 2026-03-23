using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;

namespace AgilityDogs.Services
{
    /// <summary>
    /// Service for managing voice-over localization.
    /// Handles language-specific VO assets and fallback strategies.
    /// </summary>
    public class VOLocalizationService : MonoBehaviour
    {
        public static VOLocalizationService Instance { get; private set; }

        [Header("Localization Settings")]
        [SerializeField] private string defaultLanguage = "en";
        [SerializeField] private bool enableFallback = true;
        [SerializeField] private string fallbackLanguage = "en";
        
        [Header("Language Configurations")]
        [SerializeField] private List<LanguageConfig> languageConfigs = new List<LanguageConfig>
        {
            new LanguageConfig { languageCode = "en", languageName = "English", voiceId = "english_voice" },
            new LanguageConfig { languageCode = "es", languageName = "Spanish", voiceId = "spanish_voice" },
            new LanguageConfig { languageCode = "fr", languageName = "French", voiceId = "french_voice" },
            new LanguageConfig { languageCode = "de", languageName = "German", voiceId = "german_voice" },
            new LanguageConfig { languageCode = "ja", languageName = "Japanese", voiceId = "japanese_voice" }
        };

        [Header("Localization Tables")]
        [SerializeField] private List<VOLocalizationTable> localizationTables = new List<VOLocalizationTable>();

        // Current language
        private string currentLanguage;
        private LanguageConfig currentLanguageConfig;

        // VO key mappings
        private Dictionary<string, Dictionary<string, string>> voTranslations = new Dictionary<string, Dictionary<string, string>>();
        private Dictionary<string, VOLocalizationEntry> voEntries = new Dictionary<string, VOLocalizationEntry>();

        // Events
        public event Action<string> OnLanguageChanged;
        public event Action<string, string> OnVOLocalizationMissing;

        // Properties
        public string CurrentLanguage => currentLanguage;
        public List<LanguageConfig> AvailableLanguages => languageConfigs;
        public LanguageConfig CurrentLanguageConfig => currentLanguageConfig;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeLocalization();
        }

        private void InitializeLocalization()
        {
            // Load saved language preference
            currentLanguage = PlayerPrefs.GetString("VOLanguage", defaultLanguage);
            
            // Set current language config
            UpdateCurrentLanguageConfig();
            
            // Load localization tables
            LoadLocalizationTables();
            
            Debug.Log($"[VOLocalization] Initialized with language: {currentLanguage}");
        }

        /// <summary>
        /// Set the current language for voice-over.
        /// </summary>
        public void SetLanguage(string languageCode)
        {
            if (!IsLanguageAvailable(languageCode))
            {
                Debug.LogWarning($"[VOLocalization] Language not available: {languageCode}");
                return;
            }

            currentLanguage = languageCode;
            PlayerPrefs.SetString("VOLanguage", languageCode);
            
            UpdateCurrentLanguageConfig();
            
            OnLanguageChanged?.Invoke(currentLanguage);
            Debug.Log($"[VOLocalization] Language changed to: {languageCode}");
        }

        /// <summary>
        /// Get the localized text for a VO key.
        /// </summary>
        public string GetLocalizedText(string voKey, string context = null)
        {
            // Try to find in current language
            string localizedKey = GetLocalizedKey(voKey, currentLanguage, context);
            if (voEntries.TryGetValue(localizedKey, out var entry))
            {
                return entry.localizedText;
            }

            // Try fallback language
            if (enableFallback && currentLanguage != fallbackLanguage)
            {
                localizedKey = GetLocalizedKey(voKey, fallbackLanguage, context);
                if (voEntries.TryGetValue(localizedKey, out entry))
                {
                    return entry.localizedText;
                }
            }

            // Return default key if nothing found
            OnVOLocalizationMissing?.Invoke(voKey, currentLanguage);
            return voKey;
        }

        /// <summary>
        /// Get the voice ID for the current language.
        /// </summary>
        public string GetVoiceIdForLanguage(string languageCode = null)
        {
            string lang = languageCode ?? currentLanguage;
            var config = languageConfigs.Find(c => c.languageCode == lang);
            return config?.voiceId ?? currentLanguageConfig?.voiceId;
        }

        /// <summary>
        /// Check if a language is available.
        /// </summary>
        public bool IsLanguageAvailable(string languageCode)
        {
            return languageConfigs.Exists(c => c.languageCode == languageCode && c.isEnabled);
        }

        /// <summary>
        /// Get all available language names.
        /// </summary>
        public List<string> GetAvailableLanguageNames()
        {
            List<string> names = new List<string>();
            foreach (var config in languageConfigs)
            {
                if (config.isEnabled)
                {
                    names.Add(config.languageName);
                }
            }
            return names;
        }

        /// <summary>
        /// Get language config by code.
        /// </summary>
        public LanguageConfig GetLanguageConfig(string languageCode)
        {
            return languageConfigs.Find(c => c.languageCode == languageCode);
        }

        /// <summary>
        /// Register a localized VO entry.
        /// </summary>
        public void RegisterLocalizedVO(string voKey, string languageCode, string localizedText, string context = null)
        {
            string localizedKey = GetLocalizedKey(voKey, languageCode, context);
            
            voEntries[localizedKey] = new VOLocalizationEntry
            {
                key = voKey,
                languageCode = languageCode,
                localizedText = localizedText,
                context = context
            };

            Debug.Log($"[VOLocalization] Registered localized VO: {voKey} -> {languageCode}");
        }

        /// <summary>
        /// Load localization tables from ScriptableObjects.
        /// </summary>
        private void LoadLocalizationTables()
        {
            voEntries.Clear();

            foreach (var table in localizationTables)
            {
                if (table == null) continue;

                foreach (var entry in table.entries)
                {
                    string localizedKey = GetLocalizedKey(entry.key, table.languageCode, entry.context);
                    voEntries[localizedKey] = entry;
                }
            }

            Debug.Log($"[VOLocalization] Loaded {voEntries.Count} localized VO entries");
        }

        /// <summary>
        /// Get localized key for dictionary lookup.
        /// </summary>
        private string GetLocalizedKey(string voKey, string languageCode, string context = null)
        {
            if (string.IsNullOrEmpty(context))
            {
                return $"{voKey}_{languageCode}";
            }
            return $"{voKey}_{context}_{languageCode}";
        }

        /// <summary>
        /// Update current language config.
        /// </summary>
        private void UpdateCurrentLanguageConfig()
        {
            currentLanguageConfig = languageConfigs.Find(c => c.languageCode == currentLanguage);
            if (currentLanguageConfig == null && languageConfigs.Count > 0)
            {
                currentLanguageConfig = languageConfigs[0];
                currentLanguage = currentLanguageConfig.languageCode;
            }
        }

        /// <summary>
        /// Get localized VO request key.
        /// </summary>
        public string GetLocalizedVOKey(string baseKey, string context = null)
        {
            return GetLocalizedKey(baseKey, currentLanguage, context);
        }

        /// <summary>
        /// Check if a VO key has localization.
        /// </summary>
        public bool HasLocalization(string voKey, string context = null)
        {
            string localizedKey = GetLocalizedKey(voKey, currentLanguage, context);
            return voEntries.ContainsKey(localizedKey);
        }

        /// <summary>
        /// Get all contexts for a VO key.
        /// </summary>
        public List<string> GetAvailableContexts(string voKey)
        {
            List<string> contexts = new List<string>();
            foreach (var entry in voEntries.Values)
            {
                if (entry.key == voKey && !string.IsNullOrEmpty(entry.context))
                {
                    if (!contexts.Contains(entry.context))
                    {
                        contexts.Add(entry.context);
                    }
                }
            }
            return contexts;
        }

        /// <summary>
        /// Export localization data for external tools.
        /// </summary>
        public string ExportLocalizationData(string format = "json")
        {
            var exportData = new LocalizationExportData
            {
                defaultLanguage = defaultLanguage,
                languages = languageConfigs.ToArray(),
                entries = new List<VOLocalizationEntry>(voEntries.Values)
            };

            return JsonUtility.ToJson(exportData, true);
        }

        /// <summary>
        /// Import localization data from external source.
        /// </summary>
        public bool ImportLocalizationData(string jsonData)
        {
            try
            {
                var importData = JsonUtility.FromJson<LocalizationExportData>(jsonData);
                
                if (importData.languages != null)
                {
                    foreach (var lang in importData.languages)
                    {
                        if (!languageConfigs.Exists(c => c.languageCode == lang.languageCode))
                        {
                            languageConfigs.Add(lang);
                        }
                    }
                }

                if (importData.entries != null)
                {
                    foreach (var entry in importData.entries)
                    {
                        RegisterLocalizedVO(entry.key, entry.languageCode, entry.localizedText, entry.context);
                    }
                }

                Debug.Log($"[VOLocalization] Imported localization data");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[VOLocalization] Failed to import: {e.Message}");
                return false;
            }
        }
    }

    /// <summary>
    /// Configuration for a supported language.
    /// </summary>
    [Serializable]
    public class LanguageConfig
    {
        public string languageCode;
        public string languageName;
        public string voiceId;
        public bool isEnabled = true;
        public string locale; // e.g., "en-US", "en-GB"
        public Sprite flagIcon;
    }

    /// <summary>
    /// A localized VO entry.
    /// </summary>
    [Serializable]
    public class VOLocalizationEntry
    {
        public string key;
        public string languageCode;
        public string localizedText;
        public string context;
        public string notes;
    }

    /// <summary>
    /// ScriptableObject for storing localization tables.
    /// </summary>
    [CreateAssetMenu(fileName = "VOLocalizationTable", menuName = "Agility Dogs/VO Localization Table")]
    public class VOLocalizationTable : ScriptableObject
    {
        public string languageCode;
        public string languageName;
        public List<VOLocalizationEntry> entries = new List<VOLocalizationEntry>();
    }

    /// <summary>
    /// Data structure for localization export/import.
    /// </summary>
    [Serializable]
    public class LocalizationExportData
    {
        public string defaultLanguage;
        public LanguageConfig[] languages;
        public List<VOLocalizationEntry> entries;
    }

    /// <summary>
    /// Helper class for localized text in UI.
    /// </summary>
    public class LocalizedTextComponent : MonoBehaviour
    {
        [SerializeField] private string voKey;
        [SerializeField] private string context;
        [SerializeField] private bool updateOnLanguageChange = true;

        private TMP_Text textComponent;

        private void Start()
        {
            textComponent = GetComponent<TMP_Text>();
            UpdateText();

            if (updateOnLanguageChange && VOLocalizationService.Instance != null)
            {
                VOLocalizationService.Instance.OnLanguageChanged += OnLanguageChanged;
            }
        }

        private void OnDestroy()
        {
            if (VOLocalizationService.Instance != null)
            {
                VOLocalizationService.Instance.OnLanguageChanged -= OnLanguageChanged;
            }
        }

        public void SetVOKey(string key, string context = null)
        {
            voKey = key;
            this.context = context;
            UpdateText();
        }

        private void UpdateText()
        {
            if (textComponent == null || VOLocalizationService.Instance == null) return;

            textComponent.text = VOLocalizationService.Instance.GetLocalizedText(voKey, context);
        }

        private void OnLanguageChanged(string languageCode)
        {
            UpdateText();
        }
    }
}