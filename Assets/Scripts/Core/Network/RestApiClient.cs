using System;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine;
using YuankunHuang.Unity.ModuleCore;
using YuankunHuang.Unity.Core;

namespace YuankunHuang.Unity.NetworkCore
{
    [Serializable]
    public class MessageWrapper
    {
        public string message;
    }

    [Serializable]
    public class ServerReply
    {
        public string reply;
    }

    public class RestApiClient : IDisposable
    {
        private static readonly string aiServerUrl = "http://localhost:5000/chat";

        public void SendMessage(string message, ServerType server, Action<string> onSuccess, Action<string> onError)
        {
            MonoManager.Instance.StartCoroutine(SendMessageCoroutine(message, server, onSuccess, onError));
        }

        private IEnumerator SendMessageCoroutine(string message, ServerType server, Action<string> onSuccess, Action<string> onError)
        {
            var json = JsonUtility.ToJson(new MessageWrapper { message = message });
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

            var url = server switch
            {
                ServerType.ChatAI => aiServerUrl,
                ServerType.ChatPlayer => "http://localhost:5000/chat", // Replace with actual chat server URL
                _ => throw new ArgumentOutOfRangeException(nameof(server), server, null)
            };

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    onError?.Invoke(request.error);
                }
                else
                {
                    try
                    {
                        var response = JsonUtility.FromJson<ServerReply>(request.downloadHandler.text);
                        onSuccess?.Invoke(response.reply);
                    }
                    catch (Exception ex)
                    {
                        onError?.Invoke($"Error parsing response: {ex.Message}");
                    }
                }
            }
        }

        public void Dispose()
        {

        }
    }
}