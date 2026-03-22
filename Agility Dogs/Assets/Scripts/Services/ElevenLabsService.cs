using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace AgilityDogs.Services
{
    public class ElevenLabsService : MonoBehaviour
    {
        private string apiKey;
        private string mainVoiceId;
        private string colorVoiceId;
        
        private const string BASE_URL = "https://api.elevenlabs.io/v1/text-to-speech/";
        
        private void Awake()
        {
            apiKey = EnvConfig.GetElevenLabsApiKey();
            mainVoiceId = EnvConfig.GetElevenLabsMainVoiceId();
            colorVoiceId = EnvConfig.GetElevenLabsColorVoiceId();
            
            if (string.IsNullOrEmpty(apiKey))
            {
                Debug.LogError("ElevenLabs API key not found in environment variables.");
            }
        }
        
        public IEnumerator SpeakAsMainAnnouncer(string text, Action<AudioClip> onAudioLoaded = null)
        {
            yield return GenerateSpeech(text, mainVoiceId, onAudioLoaded);
        }
        
        public IEnumerator SpeakAsColorCommentator(string text, Action<AudioClip> onAudioLoaded = null)
        {
            yield return GenerateSpeech(text, colorVoiceId, onAudioLoaded);
        }
        
        public IEnumerator GenerateSpeech(string text, string voiceId, Action<AudioClip> onAudioLoaded = null)
        {
            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(voiceId))
            {
                Debug.LogError("Missing ElevenLabs API key or voice ID.");
                yield break;
            }
            
            string url = BASE_URL + voiceId;
            
            // Create request body
            string jsonBody = $"{{\"text\": \"{EscapeJsonString(text)}\"}}";
            
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            
            UnityWebRequest request = new UnityWebRequest(url, "POST");
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("xi-api-key", apiKey);
            request.SetRequestHeader("Accept", "audio/mpeg");
            
            yield return request.SendWebRequest();
            
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"ElevenLabs API error: {request.error}");
                yield break;
            }
            
            byte[] audioData = request.downloadHandler.data;
            
            // Save to temporary file
            string tempPath = Path.Combine(Application.temporaryCachePath, "temp_audio.mp3");
            File.WriteAllBytes(tempPath, audioData);
            
            // Load audio clip from file
            yield return LoadAudioClipFromFile(tempPath, onAudioLoaded);
            
            // Clean up temporary file
            try { File.Delete(tempPath); } catch { }
        }
        
        private IEnumerator LoadAudioClipFromFile(string filePath, Action<AudioClip> callback)
        {
            string url = "file://" + filePath;
            UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG);
            
            yield return request.SendWebRequest();
            
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to load audio clip: {request.error}");
                callback?.Invoke(null);
                yield break;
            }
            
            AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
            clip.name = "ElevenLabsAudio";
            callback?.Invoke(clip);
        }
        
        public void PlayAudioClip(AudioClip clip, AudioSource audioSource)
        {
            if (clip == null || audioSource == null) return;
            
            audioSource.clip = clip;
            audioSource.Play();
        }
        
        private string EscapeJsonString(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";
            return str.Replace("\\", "\\\\")
                      .Replace("\"", "\\\"")
                      .Replace("\n", "\\n")
                      .Replace("\r", "\\r")
                      .Replace("\t", "\\t");
        }
        
        // Test method
        public void TestElevenLabs()
        {
            StartCoroutine(SpeakAsMainAnnouncer("Hello, welcome to the agility competition!", clip =>
            {
                if (clip != null)
                {
                    Debug.Log("Audio clip loaded successfully.");
                    // You would play it via an AudioSource component
                }
            }));
        }
    }
}