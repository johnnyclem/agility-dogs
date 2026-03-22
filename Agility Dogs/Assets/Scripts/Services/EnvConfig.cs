using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AgilityDogs.Services
{
    public static class EnvConfig
    {
        private static Dictionary<string, string> envVars = new Dictionary<string, string>();
        private static bool isLoaded = false;

        public static void Load()
        {
            if (isLoaded) return;

            // Try to load from .env file in project root
            string envFilePath = Path.Combine(Application.dataPath, "..", ".env");
            if (File.Exists(envFilePath))
            {
                LoadFromFile(envFilePath);
            }
            else
            {
                // Try to load from a .env file in the Assets folder
                envFilePath = Path.Combine(Application.dataPath, ".env");
                if (File.Exists(envFilePath))
                {
                    LoadFromFile(envFilePath);
                }
            }

            // Also load from system environment variables (override file)
            LoadFromSystemEnvironment();

            isLoaded = true;
        }

        private static void LoadFromFile(string filePath)
        {
            try
            {
                string[] lines = File.ReadAllLines(filePath);
                foreach (string line in lines)
                {
                    string trimmed = line.Trim();
                    if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#"))
                        continue;

                    int separatorIndex = trimmed.IndexOf('=');
                    if (separatorIndex == -1) continue;

                    string key = trimmed.Substring(0, separatorIndex).Trim();
                    string value = trimmed.Substring(separatorIndex + 1).Trim();

                    // Remove surrounding quotes if present
                    if (value.Length >= 2 && value[0] == '"' && value[value.Length - 1] == '"')
                        value = value.Substring(1, value.Length - 2);
                    else if (value.Length >= 2 && value[0] == '\'' && value[value.Length - 1] == '\'')
                        value = value.Substring(1, value.Length - 2);

                    envVars[key] = value;
                }

                Debug.Log($"Loaded environment variables from {filePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load .env file: {e.Message}");
            }
        }

        private static void LoadFromSystemEnvironment()
        {
            foreach (DictionaryEntry entry in Environment.GetEnvironmentVariables())
            {
                string key = entry.Key as string;
                string value = entry.Value as string;
                if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                {
                    envVars[key] = value;
                }
            }
        }

        public static string Get(string key, string defaultValue = null)
        {
            if (!isLoaded) Load();

            if (envVars.TryGetValue(key, out string value))
            {
                return value;
            }

            // Try system environment variable
            string systemValue = Environment.GetEnvironmentVariable(key);
            if (!string.IsNullOrEmpty(systemValue))
            {
                return systemValue;
            }

            return defaultValue;
        }

        public static string GetElevenLabsApiKey()
        {
            return Get("ELEVEN_LABS_API_KEY");
        }

        public static string GetElevenLabsMainVoiceId()
        {
            return Get("ELEVEN_LABS_VOICE_ID_ANNOUNCER_MAIN");
        }

        public static string GetElevenLabsColorVoiceId()
        {
            return Get("ELEVEN_LABS_VOICE_ID_ANNOUNCER_COLOR_COMMENTARY");
        }

        public static string GetEastworldServerUrl()
        {
            return Get("EASTWORLD_SERVER_URL", "http://localhost:8000");
        }

        public static string GetEastworldGameUuid()
        {
            return Get("EASTWORLD_GAME_UUID", "agility-dogs-game");
        }

        public static string GetEastworldMainAnnouncerAgentUuid()
        {
            return Get("EASTWORLD_MAIN_ANNOUNCER_AGENT_UUID", "main-announcer-agent");
        }

        public static string GetEastworldColorCommentatorAgentUuid()
        {
            return Get("EASTWORLD_COLOR_COMMENTATOR_AGENT_UUID", "color-commentator-agent");
        }
    }
}