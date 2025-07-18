using System;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine;

namespace YuankunHuang.Unity.Core
{
    [System.Serializable]
    public class JsonPlaceholderMessage
    {
        public int postId;
        public int id;
        public string name;
        public string email;
        public string body;
    }

    public class RestApiClient : IDisposable
    {
        public void GetDummyMessage(Action<JsonPlaceholderMessage> onSuccess, Action<string> onError)
        {
            MonoManager.Instance.StartCoroutine(GetDummyMessageCoroutine(onSuccess, onError));
        }

        private IEnumerator GetDummyMessageCoroutine(Action<JsonPlaceholderMessage> onSuccess, Action<string> onError)
        {
            var url = "https://jsonplaceholder.typicode.com/comments/1";
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    onSuccess?.Invoke(JsonUtility.FromJson<JsonPlaceholderMessage>(request.downloadHandler.text));
                }
                else
                {
                    onError?.Invoke($"Error: {request.error}");
                }
            }

            
        }

        public void Dispose()
        {

        }
    }
}