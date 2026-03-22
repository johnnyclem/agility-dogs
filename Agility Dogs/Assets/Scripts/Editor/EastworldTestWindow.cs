using System.Collections;
using UnityEditor;
using UnityEngine;
using AgilityDogs.Services;

namespace AgilityDogs.Editor
{
    public class EastworldTestWindow : EditorWindow
    {
        private string gameUuid = "agility-dogs-game";
        private string agentUuid = "main-announcer-agent";
        private string sessionUuid = "";
        private string message = "The dog just completed the tunnel obstacle cleanly.";
        private string response = "";
        private EastworldClient client;
        
        [MenuItem("Agility Dogs/Eastworld Test")]
        public static void ShowWindow()
        {
            GetWindow<EastworldTestWindow>("Eastworld Test");
        }
        
        private void OnEnable()
        {
            // Create a temporary GameObject with EastworldClient
            GameObject tempGO = new GameObject("EastworldTest");
            client = tempGO.AddComponent<EastworldClient>();
            tempGO.hideFlags = HideFlags.HideAndDontSave;
        }
        
        private void OnDisable()
        {
            if (client != null)
            {
                DestroyImmediate(client.gameObject);
            }
        }
        
        private void OnGUI()
        {
            GUILayout.Label("Eastworld Integration Test", EditorStyles.boldLabel);
            
            gameUuid = EditorGUILayout.TextField("Game UUID:", gameUuid);
            agentUuid = EditorGUILayout.TextField("Agent UUID:", agentUuid);
            sessionUuid = EditorGUILayout.TextField("Session UUID (optional):", sessionUuid);
            
            GUILayout.Space(10);
            GUILayout.Label("Server URL from .env: " + EnvConfig.GetEastworldServerUrl());
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Create Session"))
            {
                CreateSession();
            }
            
            if (!string.IsNullOrEmpty(sessionUuid))
            {
                GUILayout.Label($"Session: {sessionUuid}");
                
                GUILayout.Space(10);
                message = EditorGUILayout.TextArea(message, GUILayout.Height(60));
                
                if (GUILayout.Button("Send Message (Interact)"))
                {
                    SendMessage();
                }
                
                if (GUILayout.Button("Start Chat"))
                {
                    StartChat();
                }
            }
            
            GUILayout.Space(10);
            if (!string.IsNullOrEmpty(response))
            {
                GUILayout.Label("Response:", EditorStyles.boldLabel);
                GUILayout.TextArea(response, GUILayout.Height(100));
            }
            
            GUILayout.Space(10);
            if (GUILayout.Button("Test Connectivity"))
            {
                TestConnectivity();
            }
        }
        
        private void CreateSession()
        {
            EditorCoroutineUtility.StartCoroutine(CreateSessionCoroutine(), this);
        }
        
        private IEnumerator CreateSessionCoroutine()
        {
            bool completed = false;
            string error = null;
            
            yield return client.CreateSession(gameUuid, successJson =>
            {
                EastworldResponse result = JsonUtility.FromJson<EastworldResponse>(successJson);
                sessionUuid = result.session_uuid;
                response = $"Session created: {sessionUuid}";
                completed = true;
            }, errorMsg =>
            {
                error = errorMsg;
                completed = true;
            });
            
            while (!completed) yield return null;
            
            if (error != null)
            {
                response = $"Error: {error}";
            }
        }
        
        private void SendMessage()
        {
            EditorCoroutineUtility.StartCoroutine(SendMessageCoroutine(), this);
        }
        
        private IEnumerator SendMessageCoroutine()
        {
            bool completed = false;
            string error = null;
            
            yield return client.Interact(sessionUuid, agentUuid, message, eastworldResponse =>
            {
                response = $"Agent response: {eastworldResponse.content}";
                if (!string.IsNullOrEmpty(eastworldResponse.action))
                    response += $"\nAction: {eastworldResponse.action}";
                completed = true;
            }, errorMsg =>
            {
                error = errorMsg;
                completed = true;
            });
            
            while (!completed) yield return null;
            
            if (error != null)
            {
                response = $"Error: {error}";
            }
        }
        
        private void StartChat()
        {
            EditorCoroutineUtility.StartCoroutine(StartChatCoroutine(), this);
        }
        
        private IEnumerator StartChatCoroutine()
        {
            bool completed = false;
            string error = null;
            
            yield return client.StartChat(sessionUuid, agentUuid, "Main Announcer", () =>
            {
                response = "Chat started with agent.";
                completed = true;
            }, errorMsg =>
            {
                error = errorMsg;
                completed = true;
            });
            
            while (!completed) yield return null;
            
            if (error != null)
            {
                response = $"Error: {error}";
            }
        }
        
        private void TestConnectivity()
        {
            response = "Testing connectivity to Eastworld server...";
            EditorCoroutineUtility.StartCoroutine(TestConnectivityCoroutine(), this);
        }
        
        private IEnumerator TestConnectivityCoroutine()
        {
            // Try to create a session as a simple test
            bool completed = false;
            string error = null;
            
            yield return client.CreateSession(gameUuid, successJson =>
            {
                response = "Connectivity successful! Server is reachable.";
                completed = true;
            }, errorMsg =>
            {
                error = errorMsg;
                completed = true;
            });
            
            while (!completed) yield return null;
            
            if (error != null)
            {
                response = $"Connectivity failed: {error}\nCheck if Eastworld server is running at {EnvConfig.GetEastworldServerUrl()}";
            }
        }
        
        // Helper class to run coroutines in editor (same as in ElevenLabsTestWindow)
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