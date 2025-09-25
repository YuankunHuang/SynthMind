using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using YuankunHuang.Unity.Core;

namespace YuankunHuang.Unity.FirebaseCore
{
    /// <summary>
    /// Unified Firebase Manager that handles both WebGL and native platforms
    /// </summary>
    public class UnifiedFirebaseManager : IFirebaseManager
    {
        public bool IsInitialized { get; private set; } = false;

        private IFirebaseManager _implementation;

        public async Task InitializeAsync()
        {
            try
            {
                LogHelper.Log("[UnifiedFirebaseManager] Initializing...");

#if UNITY_WEBGL && !UNITY_EDITOR
                // Use WebGL implementation
                _implementation = new WebGLFirebaseManager();
#elif !UNITY_WEBGL || UNITY_EDITOR
                // Use native Firebase implementation
                _implementation = new NativeFirebaseManager();
#else
                // Fallback - should not happen
                LogHelper.LogError("[UnifiedFirebaseManager] No suitable Firebase implementation available");
                IsInitialized = false;
                return;
#endif

                await _implementation.InitializeAsync();
                IsInitialized = _implementation.IsInitialized;

                LogHelper.Log($"[UnifiedFirebaseManager] Initialized with {_implementation.GetType().Name}. Status: {IsInitialized}");
            }
            catch (Exception e)
            {
                LogHelper.LogError($"[UnifiedFirebaseManager] Failed to initialize: {e.Message}");
                LogHelper.LogException(e);
                IsInitialized = false;
            }
        }

        #region Conversation Management
        public void CleanUpEmptyConversations(string conversationGroup, string uuid, Action<int> onComplete)
        {
            if (!IsInitialized || _implementation == null)
            {
                LogHelper.LogWarning("[UnifiedFirebaseManager] Not initialized. Cannot cleanup conversations.");
                onComplete?.Invoke(0);
                return;
            }

            _implementation.CleanUpEmptyConversations(conversationGroup, uuid, onComplete);
        }

        public void CheckIsConversationEmpty(string conversationGroup, string conversationId, Action<bool> onComplete)
        {
            if (!IsInitialized || _implementation == null)
            {
                LogHelper.LogWarning("[UnifiedFirebaseManager] Not initialized. Cannot check conversation.");
                onComplete?.Invoke(false);
                return;
            }

            _implementation.CheckIsConversationEmpty(conversationGroup, conversationId, onComplete);
        }

        public void DeleteConversation(string conversationGroup, string conversationId, Action<bool> onComplete)
        {
            if (!IsInitialized || _implementation == null)
            {
                LogHelper.LogWarning("[UnifiedFirebaseManager] Not initialized. Cannot delete conversation.");
                onComplete?.Invoke(false);
                return;
            }

            _implementation.DeleteConversation(conversationGroup, conversationId, onComplete);
        }

        public void LoadMostRecentConversation(string conversationGroup, Action<string> onComplete)
        {
            if (!IsInitialized || _implementation == null)
            {
                LogHelper.LogWarning("[UnifiedFirebaseManager] Not initialized. Cannot load conversation.");
                onComplete?.Invoke(null);
                return;
            }

            _implementation.LoadMostRecentConversation(conversationGroup, onComplete);
        }

        public void CreateNewConversation(string conversationGroup, List<string> participantIds, Action<string> onComplete)
        {
            if (!IsInitialized || _implementation == null)
            {
                LogHelper.LogWarning("[UnifiedFirebaseManager] Not initialized. Cannot create conversation.");
                onComplete?.Invoke(null);
                return;
            }

            _implementation.CreateNewConversation(conversationGroup, participantIds, onComplete);
        }

        public void SendMessageToConversation(string conversationGroup, string conversationId, string senderId, string content, Dictionary<string, object> metadata = null)
        {
            if (!IsInitialized || _implementation == null)
            {
                LogHelper.LogWarning("[UnifiedFirebaseManager] Not initialized. Cannot send message.");
                return;
            }

            _implementation.SendMessageToConversation(conversationGroup, conversationId, senderId, content, metadata);
        }

        public void LoadRecentMessages(string conversationGroup, string conversationId, int limit, Action<List<FirebaseConversationMessage>> onComplete)
        {
            if (!IsInitialized || _implementation == null)
            {
                LogHelper.LogWarning("[UnifiedFirebaseManager] Not initialized. Cannot load messages.");
                onComplete?.Invoke(new List<FirebaseConversationMessage>());
                return;
            }

            _implementation.LoadRecentMessages(conversationGroup, conversationId, limit, onComplete);
        }

        public void LoadConversationMessages(string conversationGroup, string conversationId, Action<List<FirebaseConversationMessage>> onComplete)
        {
            if (!IsInitialized || _implementation == null)
            {
                LogHelper.LogWarning("[UnifiedFirebaseManager] Not initialized. Cannot load conversation messages.");
                onComplete?.Invoke(new List<FirebaseConversationMessage>());
                return;
            }

            _implementation.LoadConversationMessages(conversationGroup, conversationId, onComplete);
        }
        #endregion

        #region Authentication
        public void RegisterWithEmail(string email, string password, string displayName, Action<IFirebaseUser> onSuccess, Action<string> onError)
        {
            if (!IsInitialized || _implementation == null)
            {
                LogHelper.LogWarning("[UnifiedFirebaseManager] Not initialized. Cannot register user.");
                onError?.Invoke("Firebase not initialized");
                return;
            }

            _implementation.RegisterWithEmail(email, password, displayName, onSuccess, onError);
        }

        public void SignInWithEmail(string email, string password, Action<IFirebaseUser> onSuccess, Action<string> onError)
        {
            if (!IsInitialized || _implementation == null)
            {
                LogHelper.LogWarning("[UnifiedFirebaseManager] Not initialized. Cannot sign in user.");
                onError?.Invoke("Firebase not initialized");
                return;
            }

            _implementation.SignInWithEmail(email, password, onSuccess, onError);
        }

        public void SignOut(Action onComplete)
        {
            if (!IsInitialized || _implementation == null)
            {
                LogHelper.LogWarning("[UnifiedFirebaseManager] Not initialized. Cannot sign out.");
                onComplete?.Invoke();
                return;
            }

            _implementation.SignOut(onComplete);
        }

        public void GetCurrentUser(Action<IFirebaseUser> onComplete)
        {
            if (!IsInitialized || _implementation == null)
            {
                LogHelper.LogWarning("[UnifiedFirebaseManager] Not initialized. Cannot get current user.");
                onComplete?.Invoke(null);
                return;
            }

            _implementation.GetCurrentUser(onComplete);
        }

        public void UpdateUserProfile(string displayName, string photoUrl, Action<bool> onComplete)
        {
            if (!IsInitialized || _implementation == null)
            {
                LogHelper.LogWarning("[UnifiedFirebaseManager] Not initialized. Cannot update user profile.");
                onComplete?.Invoke(false);
                return;
            }

            _implementation.UpdateUserProfile(displayName, photoUrl, onComplete);
        }

        public void SendEmailVerification(Action<bool> onComplete)
        {
            if (!IsInitialized || _implementation == null)
            {
                LogHelper.LogWarning("[UnifiedFirebaseManager] Not initialized. Cannot send email verification.");
                onComplete?.Invoke(false);
                return;
            }

            _implementation.SendEmailVerification(onComplete);
        }

        public void SendPasswordResetEmail(string email, Action<bool> onComplete)
        {
            if (!IsInitialized || _implementation == null)
            {
                LogHelper.LogWarning("[UnifiedFirebaseManager] Not initialized. Cannot send password reset email.");
                onComplete?.Invoke(false);
                return;
            }

            _implementation.SendPasswordResetEmail(email, onComplete);
        }
        #endregion

        #region User Data Management
        public void SaveUserData(string userId, Dictionary<string, object> userData, Action<bool> onComplete)
        {
            if (!IsInitialized || _implementation == null)
            {
                LogHelper.LogWarning("[UnifiedFirebaseManager] Not initialized. Cannot save user data.");
                onComplete?.Invoke(false);
                return;
            }

            _implementation.SaveUserData(userId, userData, onComplete);
        }

        public void LoadUserData(string userId, Action<Dictionary<string, object>> onComplete)
        {
            if (!IsInitialized || _implementation == null)
            {
                LogHelper.LogWarning("[UnifiedFirebaseManager] Not initialized. Cannot load user data.");
                onComplete?.Invoke(null);
                return;
            }

            _implementation.LoadUserData(userId, onComplete);
        }

        public void UpdateUserData(string userId, Dictionary<string, object> updates, Action<bool> onComplete)
        {
            if (!IsInitialized || _implementation == null)
            {
                LogHelper.LogWarning("[UnifiedFirebaseManager] Not initialized. Cannot update user data.");
                onComplete?.Invoke(false);
                return;
            }

            _implementation.UpdateUserData(userId, updates, onComplete);
        }
        #endregion

        public void Dispose()
        {
            try
            {
                LogHelper.Log("[UnifiedFirebaseManager] Disposing...");

                if (_implementation != null)
                {
                    _implementation.Dispose();
                    _implementation = null;
                }

                IsInitialized = false;

                LogHelper.Log("[UnifiedFirebaseManager] Disposed successfully.");
            }
            catch (Exception e)
            {
                LogHelper.LogError($"[UnifiedFirebaseManager] Error during disposal: {e.Message}");
                LogHelper.LogException(e);
            }
        }
    }
}