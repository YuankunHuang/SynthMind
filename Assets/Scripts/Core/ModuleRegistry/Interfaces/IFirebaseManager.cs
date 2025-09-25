using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YuankunHuang.Unity.ModuleCore;

namespace YuankunHuang.Unity.FirebaseCore
{
    public interface IFirebaseUser
    {
        string UUID { get; set; }
        string Email { get; set; }
        string DisplayName { get; set; }
        bool IsEmailVerified { get; set; }
        string Avatar { get; set; }
    }

    public interface IFirebaseManager : IModule
    {
        // Initialization
        Task InitializeAsync();

        // Authentication
        void RegisterWithEmail(string email, string password, string displayName, Action<IFirebaseUser> onSuccess, Action<string> onError);
        void SignInWithEmail(string email, string password, Action<IFirebaseUser> onSuccess, Action<string> onError);
        void SignOut(Action onComplete);
        void GetCurrentUser(Action<IFirebaseUser> onComplete);
        void UpdateUserProfile(string displayName, string photoUrl, Action<bool> onComplete);
        void SendEmailVerification(Action<bool> onComplete);
        void SendPasswordResetEmail(string email, Action<bool> onComplete);

        // User Data Management
        void SaveUserData(string userId, Dictionary<string, object> userData, Action<bool> onComplete);
        void LoadUserData(string userId, Action<Dictionary<string, object>> onComplete);
        void UpdateUserData(string userId, Dictionary<string, object> updates, Action<bool> onComplete);

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