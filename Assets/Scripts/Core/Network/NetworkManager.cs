#if !UNITY_WEBGL || UNITY_EDITOR
using Firebase.Analytics;
using Firebase.Firestore;
#endif
using System;
using System.Collections.Generic;
using YuankunHuang.Unity.Core;
using YuankunHuang.Unity.ModuleCore;
using YuankunHuang.Unity.AccountCore;
using YuankunHuang.Unity.FirebaseCore;

namespace YuankunHuang.Unity.NetworkCore
{
    public class NetworkManager : INetworkManager
    {
        public bool IsInitialized { get; private set; } = false;

        public void Connect(string address, int port)
        {
            // Implement connection logic here
            LogHelper.Log($"Connecting to {address}:{port}");
            OnConnected?.Invoke();
        }

        public void Disconnect()
        {
            // Implement disconnection logic here
            LogHelper.Log("Disconnecting from network");
            OnDisconnected?.Invoke();
        }

        public bool IsConnected { get; private set; } = false;

        public event Action OnConnected;
        public event Action OnDisconnected;
        public event Action<string> OnError;

        public RestApiClient RestApi { get; private set; }

        public NetworkManager()
        {
            RestApi = new RestApiClient();

            IsInitialized = true;

            LogHelper.Log("NetworkManager initialized");
        }

        public void Dispose()
        {
            var self = ModuleRegistry.Get<IAccountManager>().Self;
            if (self != null)
            {
                var firebaseManager = ModuleRegistry.Get<IFirebaseManager>();
                if (firebaseManager.IsInitialized)
                {
                    firebaseManager.CleanUpEmptyConversations(FirebaseCollections.AI_Conversations, self.UUID, null);
                    firebaseManager.CleanUpEmptyConversations(FirebaseCollections.Command_Conversations, self.UUID, null);
                }
            }

            RestApi.Dispose();
            RestApi = null;

            Disconnect();

            IsInitialized = false;

            LogHelper.Log("NetworkManager disposed");
        }

        public void SendMessage(string conversationGroup, string conversationId, string senderId, string content, Dictionary<string, object> metadata, ServerType server, Action<string> onSuccess, Action<string> onError)
        {
            if (RestApi == null)
            {
                onError?.Invoke("RestApiClient is not initialized.");
                return;
            }

#if !UNITY_WEBGL || UNITY_EDITOR
            FirebaseAnalytics.LogEvent("send_message", new Parameter[]
                {
                    new Parameter("conversation_group", conversationGroup),
                    new Parameter("conversation_id", conversationId),
                    new Parameter("sender_id", senderId),
                    new Parameter("content", content),
                    new Parameter("timestamp", Timestamp.GetCurrentTimestamp().ToString())
                });
#endif
#if UNITY_WEBGL && !UNITY_EDITOR
            LogHelper.Log($"[Analytics] send_message by {senderId}: {content} in conversation: {conversationId} in conversation_group: {conversationGroup} at time {System.DateTime.Now.ToString()}");
#else
            LogHelper.Log($"[Analytics] send_message by {senderId}: {content} in conversation: {conversationId} in conversation_group: {conversationGroup} at time {Timestamp.GetCurrentTimestamp().ToString()}");
#endif

            var firebaseManager = ModuleRegistry.Get<IFirebaseManager>();
            if (firebaseManager.IsInitialized)
            {
                firebaseManager.SendMessageToConversation(conversationGroup, conversationId, ModuleRegistry.Get<IAccountManager>().Self.UUID, content, null);
            }

            RestApi.SendMessage(content, server, reply =>
            {
                var ai = ModuleRegistry.Get<IAccountManager>().AI;

#if !UNITY_WEBGL || UNITY_EDITOR
                FirebaseAnalytics.LogEvent("receive_message", new Parameter[]
                {
                    new Parameter("conversation_group", conversationGroup),
                    new Parameter("conversation_id", conversationId),
                    new Parameter("sender_id", ai.UUID),
                    new Parameter("content", reply),
                    new Parameter("timestamp", Timestamp.GetCurrentTimestamp().ToString())
                });
#endif
#if UNITY_WEBGL && !UNITY_EDITOR
                LogHelper.Log($"[Analytics] receive_message by {senderId}: {content} in conversation {conversationId} in conversation_group: {conversationGroup} at time {System.DateTime.Now.ToString()}");
#else
                LogHelper.Log($"[Analytics] receive_message by {senderId}: {content} in conversation {conversationId} in conversation_group: {conversationGroup} at time {Timestamp.GetCurrentTimestamp().ToString()}");
#endif

                if (firebaseManager.IsInitialized)
                {
                    firebaseManager.SendMessageToConversation(conversationGroup, conversationId, ai.UUID, reply, null);
                }

                onSuccess?.Invoke(reply);
            }, onError);
        }

    }
}