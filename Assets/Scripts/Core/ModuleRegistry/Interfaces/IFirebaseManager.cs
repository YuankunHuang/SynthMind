using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YuankunHuang.Unity.ModuleCore;

namespace YuankunHuang.Unity.FirebaseCore
{
    public interface IFirebaseManager : IModule
    {
        // Initialization
        Task InitializeAsync();

        // Conversation Management
        void CleanUpEmptyConversations(string conversationGroup, string uuid, Action<int> onComplete);
        void CheckIsConversationEmpty(string conversationGroup, string conversationId, Action<bool> onComplete);
        void DeleteConversation(string conversationGroup, string conversationId, Action<bool> onComplete);
        void LoadMostRecentConversation(string conversationGroup, Action<string> onComplete);
        void CreateNewConversation(string conversationGroup, List<string> participantIds, Action<string> onComplete);

        // Message Management
        void SendMessageToConversation(string conversationGroup, string conversationId, string senderId, string content, Dictionary<string, object> metadata = null);
        void LoadRecentMessages(string conversationGroup, string conversationId, int limit, Action<List<FirebaseConversationMessage>> onComplete);
        void LoadConversationMessages(string conversationGroup, string conversationId, Action<List<FirebaseConversationMessage>> onComplete);
    }
}