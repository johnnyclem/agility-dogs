using System.Collections;
using UnityEditor;
using UnityEngine;
using AgilityDogs.Services;

namespace AgilityDogs.Editor
{
    public class ElevenLabsTestWindow : EditorWindow
    {
        private string testText = "Welcome to the agility competition! The dog looks ready to run.";
        private AnnouncerType selectedVoice = AnnouncerType.Main;
        private ElevenLabsService service;
        private AudioSource audioSource;
        private AudioClip lastClip;
        
        [MenuItem("Agility Dogs/ElevenLabs Test")]
        public static void ShowWindow()
        {
            GetWindow<ElevenLabsTestWindow>("ElevenLabs Test");
        }
        
        private void OnEnable()
        {
            // Create a temporary GameObject with ElevenLabsService and AudioSource
            GameObject tempGO = new GameObject("ElevenLabsTest");
            service = tempGO.AddComponent<ElevenLabsService>();
            audioSource = tempGO.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            
            // Hide the temporary GameObject
            tempGO.hideFlags = HideFlags.HideAndDontSave;
        }
        
        private void OnDisable()
        {
            if (service != null)
            {
                DestroyImmediate(service.gameObject);
            }
        }
        
        private void OnGUI()
        {
            GUILayout.Label("ElevenLabs Integration Test", EditorStyles.boldLabel);
            
            testText = EditorGUILayout.TextField("Test Text:", testText);
            selectedVoice = (AnnouncerType)EditorGUILayout.EnumPopup("Voice:", selectedVoice);
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Generate Speech"))
            {
                StartSpeechGeneration();
            }
            
            GUILayout.Space(10);
            
            if (lastClip != null)
            {
                GUILayout.Label($"Last clip: {lastClip.name}, length: {lastClip.length:F2}s");
                if (GUILayout.Button("Play Again"))
                {
                    PlayClip();
                }
            }
            
            GUILayout.Space(10);
            GUILayout.Label("Environment Variables:", EditorStyles.boldLabel);
            GUILayout.Label($"API Key present: {!string.IsNullOrEmpty(EnvConfig.GetElevenLabsApiKey())}");
            GUILayout.Label($"Main Voice ID: {EnvConfig.GetElevenLabsMainVoiceId()}");
            GUILayout.Label($"Color Voice ID: {EnvConfig.GetElevenLabsColorVoiceId()}");
        }
        
        private void StartSpeechGeneration()
        {
            if (service == null) return;
            
            EditorCoroutineUtility.StartCoroutine(GenerateSpeech(), this);
        }
        
        private IEnumerator GenerateSpeech()
        {
            AudioClip clip = null;
            bool completed = false;
            
            if (selectedVoice == AnnouncerType.Main)
            {
                yield return service.SpeakAsMainAnnouncer(testText, loadedClip =>
                {
                    clip = loadedClip;
                    completed = true;
                });
            }
            else
            {
                yield return service.SpeakAsColorCommentator(testText, loadedClip =>
                {
                    clip = loadedClip;
                    completed = true;
                });
            }
            
            while (!completed) yield return null;
            
            if (clip != null)
            {
                lastClip = clip;
                PlayClip();
                Debug.Log("Speech generated successfully.");
            }
            else
            {
                Debug.LogError("Failed to generate speech. Check environment variables and API key.");
            }
        }
        
        private void PlayClip()
        {
            if (lastClip == null || audioSource == null) return;
            
            audioSource.clip = lastClip;
            audioSource.Play();
        }
        
        // Helper class to run coroutines in editor
        private class EditorCoroutineUtility
        {
            public static IEnumerator StartCoroutine(IEnumerator routine, EditorWindow owner)
            {
                return new EditorCoroutine(routine, owner);
            }
            
            private class EditorCoroutine : IEnumerator
            {
                private IEnumerator routine;
                private EditorWindow owner;
                private bool isRunning = false;
                
                public EditorCoroutine(IEnumerator routine, EditorWindow owner)
                {
                    this.routine = routine;
                    this.owner = owner;
                    isRunning = true;
                    EditorApplication.update += Update;
                }
                
                public object Current => routine.Current;
                
                public bool MoveNext()
                {
                    return routine.MoveNext();
                }
                
                public void Reset()
                {
                    routine.Reset();
                }
                
                private void Update()
                {
                    if (!isRunning)
                    {
                        EditorApplication.update -= Update;
                        return;
                    }
                    
                    if (!routine.MoveNext())
                    {
                        isRunning = false;
                        EditorApplication.update -= Update;
                    }
                }
            }
        }
    }
}