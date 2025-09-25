using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using YuankunHuang.Unity.Core;
using YuankunHuang.Unity.Core.Debug;

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