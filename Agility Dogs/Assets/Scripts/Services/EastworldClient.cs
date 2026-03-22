using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace AgilityDogs.Services
{
    [Serializable]
    public class EastworldResponse
    {
        public string status;
        public string message;
        public string session_uuid;
        public string agent_uuid;
        public string content;
        public string action;
        public string emotion;
        public bool is_action;
    }

    [Serializable]
    public class EastworldRequest
    {
        public string game_uuid;
        public string session_uuid;
        public string agent_uuid;
        public string text;
        public string action;
        public string query;
        public string guardrail;
    }

    public class EastworldClient : MonoBehaviour
    {
        private string baseUrl;
        
        private void Awake()
        {
            baseUrl = EnvConfig.GetEastworldServerUrl();
            if (!baseUrl.EndsWith("/"))
                baseUrl += "/";
        }
        
        // Create a new game session
        public IEnumerator CreateSession(string gameUuid, Action<string> onSuccess, Action<string> onError)
        {
            string url = baseUrl + "game_sessions/create";
            EastworldRequest request = new EastworldRequest { game_uuid = gameUuid };
            yield return PostRequest(url, request, onSuccess, onError);
        }
        
        // Start a chat with an agent
        public IEnumerator StartChat(string sessionUuid, string agentUuid, string correspondent, Action onSuccess, Action<string> onError)
        {
            string url = baseUrl + "game_sessions/start_chat";
            EastworldRequest request = new EastworldRequest
            {
                session_uuid = sessionUuid,
                agent_uuid = agentUuid,
                text = correspondent // The agent's name?
            };
            yield return PostRequest(url, request, response => onSuccess?.Invoke(), onError);
        }
        
        // Chat with an agent (agent responds with text)
        public IEnumerator Chat(string sessionUuid, string agentUuid, string text, Action<string> onSuccess, Action<string> onError)
        {
            string url = baseUrl + "game_sessions/chat";
            EastworldRequest request = new EastworldRequest
            {
                session_uuid = sessionUuid,
                agent_uuid = agentUuid,
                text = text
            };
            yield return PostRequest(url, request, onSuccess, onError);
        }
        
        // Interact with an agent (agent may respond with text or perform an action)
        public IEnumerator Interact(string sessionUuid, string agentUuid, string text, Action<EastworldResponse> onSuccess, Action<string> onError)
        {
            string url = baseUrl + "game_sessions/interact";
            EastworldRequest request = new EastworldRequest
            {
                session_uuid = sessionUuid,
                agent_uuid = agentUuid,
                text = text
            };
            yield return PostRequest(url, request, onSuccess, onError);
        }
        
        // Ask agent to perform a specific action
        public IEnumerator Action(string sessionUuid, string agentUuid, string action, Action<string> onSuccess, Action<string> onError)
        {
            string url = baseUrl + "game_sessions/action";
            EastworldRequest request = new EastworldRequest
            {
                session_uuid = sessionUuid,
                agent_uuid = agentUuid,
                action = action
            };
            yield return PostRequest(url, request, onSuccess, onError);
        }
        
        // Query agent's inner thoughts/emotions
        public IEnumerator Query(string sessionUuid, string agentUuid, string query, Action<string> onSuccess, Action<string> onError)
        {
            string url = baseUrl + "game_sessions/query";
            EastworldRequest request = new EastworldRequest
            {
                session_uuid = sessionUuid,
                agent_uuid = agentUuid,
                query = query
            };
            yield return PostRequest(url, request, onSuccess, onError);
        }
        
        // Guardrail check
        public IEnumerator Guardrail(string sessionUuid, string agentUuid, string text, Action<string> onSuccess, Action<string> onError)
        {
            string url = baseUrl + "game_sessions/guardrail";
            EastworldRequest request = new EastworldRequest
            {
                session_uuid = sessionUuid,
                agent_uuid = agentUuid,
                guardrail = text
            };
            yield return PostRequest(url, request, onSuccess, onError);
        }
        
        // Helper method for POST requests
        private IEnumerator PostRequest(string url, EastworldRequest requestData, Action<string> onSuccess, Action<string> onError)
        {
            string jsonBody = JsonUtility.ToJson(requestData);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            
            UnityWebRequest request = new UnityWebRequest(url, "POST");
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Accept", "application/json");
            
            yield return request.SendWebRequest();
            
            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke($"Request failed: {request.error}");
            }
            else
            {
                string responseText = request.downloadHandler.text;
                onSuccess?.Invoke(responseText);
            }
        }
        
        // Overload that deserializes JSON response into EastworldResponse
        private IEnumerator PostRequest(string url, EastworldRequest requestData, Action<EastworldResponse> onSuccess, Action<string> onError)
        {
            yield return PostRequest(url, requestData, responseJson =>
            {
                try
                {
                    EastworldResponse response = JsonUtility.FromJson<EastworldResponse>(responseJson);
                    onSuccess?.Invoke(response);
                }
                catch (Exception e)
                {
                    onError?.Invoke($"Failed to parse response: {e.Message}");
                }
            }, onError);
        }
    }
}