using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YuankunHuang.Unity.Core;

#if !UNITY_WEBGL || UNITY_EDITOR
namespace YuankunHuang.Unity.FirebaseCore
{
    /// <summary>
    /// Native Firebase implementation wrapper for desktop/mobile platforms
    /// </summary>
    public class NativeFirebaseManager : IFirebaseManager
    {
        public bool IsInitialized { get; private set; } = false;

        public async Task InitializeAsync()
        {
            try
            {
                LogHelper.Log("[NativeFirebaseManager] Initializing...");

                var tcs = new TaskCompletionSource<bool>();

                FirebaseManager.InitializeDataBase(success =>
                {
                    IsInitialized = success;
                    tcs.SetResult(success);
                });

                bool result = await tcs.Task;

                if (result)
                {
                    LogHelper.Log("[NativeFirebaseManager] Native Firebase initialized successfully.");
                }
                else
                {
                    LogHelper.LogError("[NativeFirebaseManager] Failed to initialize native Firebase.");
                }
            }
            catch (Exception e)
            {
                LogHelper.LogError($"[NativeFirebaseManager] Exception during initialization: {e.Message}");
                LogHelper.LogException(e);
                IsInitialized = false;
            }
        }

        #region Authentication
        public void RegisterWithEmail(string email, string password, string displayName, Action<IFirebaseUser> onSuccess, Action<string> onError) { }
        public void SignInWithEmail(string email, string password, Action<IFirebaseUser> onSuccess, Action<string> onError) { }
        public void SignOut(Action onComplete) { }
        public void GetCurrentUser(Action<IFirebaseUser> onComplete) { }
        public void UpdateUserProfile(string displayName, string photoUrl, Action<bool> onComplete) { }
        public void SendEmailVerification(Action<bool> onComplete) { }
        public void SendPasswordResetEmail(string email, Action<bool> onComplete) { }
        #endregion

        #region Conversation Management
        public void CleanUpEmptyConversations(string conversationGroup, string uuid, Action<int> onComplete)
        {
            if (!IsInitialized)
            {
                onComplete?.Invoke(0);
                return;
            }

            FirebaseManager.CleanUpEmptyConversations(conversationGroup, uuid, onComplete);
        }

        public void CheckIsConversationEmpty(string conversationGroup, string conversationId, Action<bool> onComplete)
        {
            if (!IsInitialized)
            {
                onComplete?.Invoke(false);
                return;
            }

            FirebaseManager.CheckIsConversationEmpty(conversationGroup, conversationId, onComplete);
        }

        public void DeleteConversation(string conversationGroup, string conversationId, Action<bool> onComplete)
        {
            if (!IsInitialized)
            {
                onComplete?.Invoke(false);
                return;
            }

            FirebaseManager.DeleteConversation(conversationGroup, conversationId, onComplete);
        }

        public void LoadMostRecentConversation(string conversationGroup, Action<string> onComplete)
        {
            if (!IsInitialized)
            {
                onComplete?.Invoke(null);
                return;
            }

            FirebaseManager.LoadMostRecentConversation(conversationGroup, onComplete);
        }

        public void CreateNewConversation(string conversationGroup, List<string> participantIds, Action<string> onComplete)
        {
            if (!IsInitialized)
            {
                onComplete?.Invoke(null);
                return;
            }

            FirebaseManager.CreateNewConversation(conversationGroup, participantIds, onComplete);
        }

        public void SendMessageToConversation(string conversationGroup, string conversationId, string senderId, string content, Dictionary<string, object> metadata = null)
        {
            if (!IsInitialized)
            {
                return;
            }

            FirebaseManager.SendMessageToConversation(conversationGroup, conversationId, senderId, content, metadata);
        }

        public void LoadRecentMessages(string conversationGroup, string conversationId, int limit, Action<List<FirebaseConversationMessage>> onComplete)
        {
            if (!IsInitialized)
            {
                onComplete?.Invoke(new List<FirebaseConversationMessage>());
                return;
            }

            FirebaseManager.LoadRecentMessages(conversationGroup, conversationId, limit, onComplete);
        }

        public void LoadConversationMessages(string conversationGroup, string conversationId, Action<List<FirebaseConversationMessage>> onComplete)
        {
            if (!IsInitialized)
            {
                onComplete?.Invoke(new List<FirebaseConversationMessage>());
                return;
            }

            FirebaseManager.LoadConversationMessages(conversationGroup, conversationId, onComplete);
        }
        #endregion

        #region User Data Management
        public void SaveUserData(string userId, Dictionary<string, object> userData, Action<bool> onComplete) { }
        public void LoadUserData(string userId, Action<Dictionary<string, object>> onComplete) { }
        public void UpdateUserData(string userId, Dictionary<string, object> updates, Action<bool> onComplete) { }
        #endregion

        public void Dispose()
        {
            try
            {
                LogHelper.Log("[NativeFirebaseManager] Disposing...");

                FirebaseManager.Dispose();
                IsInitialized = false;

                LogHelper.Log("[NativeFirebaseManager] Disposed successfully.");
            }
            catch (Exception e)
            {
                LogHelper.LogError($"[NativeFirebaseManager] Error during disposal: {e.Message}");
                LogHelper.LogException(e);
            }
        }
    }
}
#endif