using Firebase.Analytics;
using Firebase.Firestore;
using System;
using System.Collections.Generic;

namespace YuankunHuang.Unity.Core
{
    public class NetworkManager : INetworkManager
    {
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
            LogHelper.Log("NetworkManager initialized");
        }

        public void Dispose()
        {
            var self = ModuleRegistry.Get<IAccountManager>().Self;
            FirebaseManager.CleanUpEmptyConversations(FirebaseCollections.AI_Conversations, self.UUID, null);
            FirebaseManager.CleanUpEmptyConversations(FirebaseCollections.Command_Conversations, self.UUID, null);

            RestApi.Dispose();
            RestApi = null;

            Disconnect();

            LogHelper.Log("NetworkManager disposed");
        }

        public void SendMessage(string conversationGroup, string conversationId, string senderId, string content, Dictionary<string, object> metadata, ServerType server, Action<string> onSuccess, Action<string> onError)
        {
            if (RestApi == null)
            {
                onError?.Invoke("RestApiClient is not initialized.");
                return;
            }

            FirebaseAnalytics.LogEvent("send_message", new Parameter[]
                {
                    new Parameter("conversation_group", conversationGroup),
                    new Parameter("conversation_id", conversationId),
                    new Parameter("sender_id", senderId),
                    new Parameter("content", content),
                    new Parameter("timestamp", Timestamp.GetCurrentTimestamp().ToString())
                });
            LogHelper.Log($"[Analytics] send_message by {senderId}: {content} in conversation: {conversationId} in conversation_group: {conversationGroup} at time {Timestamp.GetCurrentTimestamp().ToString()}");

            FirebaseManager.SendMessageToConversation(conversationGroup, conversationId, ModuleRegistry.Get<IAccountManager>().Self.UUID, content, null);

            RestApi.SendMessage(content, server, reply =>
            {
                var ai = ModuleRegistry.Get<IAccountManager>().AI;

                FirebaseAnalytics.LogEvent("receive_message", new Parameter[]
                {
                    new Parameter("conversation_group", conversationGroup),
                    new Parameter("conversation_id", conversationId),
                    new Parameter("sender_id", ai.UUID),
                    new Parameter("content", reply),
                    new Parameter("timestamp", Timestamp.GetCurrentTimestamp().ToString())
                });
                LogHelper.Log($"[Analytics] receive_message by {senderId}: {content} in conversation {conversationId} in conversation_group: {conversationGroup} at time {Timestamp.GetCurrentTimestamp().ToString()}");

                FirebaseManager.SendMessageToConversation(conversationGroup, conversationId, ai.UUID, reply, null);

                onSuccess?.Invoke(reply);
            }, onError);
        }

    }
}