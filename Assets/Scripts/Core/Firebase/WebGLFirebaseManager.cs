using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using YuankunHuang.Unity.Core;
using YuankunHuang.Unity.Core.Debug;
#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace YuankunHuang.Unity.FirebaseCore
{
    /// <summary>
    /// WebGL-specific Firebase manager that uses JavaScript interop
    /// </summary>
    public class WebGLFirebaseManager : IFirebaseManager
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern bool InitFirebaseWeb();

        [DllImport("__Internal")]
        private static extern void SendMessageWeb(string conversationGroup, string conversationId, string senderId, string content, string callback);

        [DllImport("__Internal")]
        private static extern void LoadRecentMessagesWeb(string conversationGroup, string conversationId, int limit, string callback);

        [DllImport("__Internal")]
        private static extern void CreateNewConversationWeb(string conversationGroup, string participantIds, string callback);

        [DllImport("__Internal")]
        private static extern void DeleteConversationWeb(string conversationGroup, string conversationId, string callback);

        [DllImport("__Internal")]
        private static extern void LoadMostRecentConversationWeb(string conversationGroup, string callback);

        [DllImport("__Internal")]
        private static extern void CheckIsConversationEmptyWeb(string conversationGroup, string conversationId, string callback);
#endif

        public bool IsInitialized { get; private set; } = false;

        private static Dictionary<string, Action<string>> _callbacks = new Dictionary<string, Action<string>>();
        private static int _callbackCounter = 0;

        public async Task InitializeAsync()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            try
            {
                LogHelper.Log("[WebGLFirebaseManager] Initializing WebGL Firebase...");

                // Try to initialize Firebase - continue gracefully if it's not available
                try
                {
                    bool firebaseAvailable = InitFirebaseWeb();

                    if (!firebaseAvailable)
                    {
                        LogHelper.LogWarning("[WebGLFirebaseManager] Firebase initialization failed - continuing without Firebase functionality.");
                        LogHelper.LogWarning("[WebGLFirebaseManager] Firebase features will be disabled. Check your Firebase configuration if Firebase is required.");
                        IsInitialized = false;
                        return; // Return gracefully instead of throwing
                    }

                    // Register global callback function for JavaScript
                    // Use a fixed GameObject name since MonoManager is always named "MonoManager"
                    var monoManagerName = "MonoManager";

                    // Use direct JavaScript execution instead of deprecated Application.ExternalEval
                    try
                    {
                        // Modern WebGL approach - register callback through the window object
                        var jsCode = $@"
                            if (typeof window !== 'undefined') {{
                                window.WebGLFirebaseManager_OnJSCallback = function(callbackData) {{
                                    if (typeof unityInstance !== 'undefined' && unityInstance.SendMessage) {{
                                        unityInstance.SendMessage('{monoManagerName}', 'OnFirebaseCallback', callbackData);
                                    }} else if (typeof SendMessage !== 'undefined') {{
                                        SendMessage('{monoManagerName}', 'OnFirebaseCallback', callbackData);
                                    }} else {{
                                        console.warn('Unity SendMessage not available');
                                    }}
                                }};
                                console.log('WebGL Firebase callback registered successfully');
                            }}
                        ";

                        // Try modern approach first, fallback to legacy if needed
#pragma warning disable CS0618 // Application.ExternalEval is obsolete
                        Application.ExternalEval(jsCode);
#pragma warning restore CS0618
                    }
                    catch (Exception jsEx)
                    {
                        LogHelper.LogWarning($"[WebGLFirebaseManager] JavaScript callback registration failed: {jsEx.Message}");
                        // Continue initialization even if callback registration fails
                    }

                    IsInitialized = true;
                    LogHelper.Log("[WebGLFirebaseManager] WebGL Firebase initialized successfully.");
                }
                catch (System.EntryPointNotFoundException)
                {
                    LogHelper.LogWarning("[WebGLFirebaseManager] Firebase JavaScript functions not found - continuing without Firebase functionality.");
                    LogHelper.LogWarning("[WebGLFirebaseManager] Please ensure FirebaseWebGL.jslib is included in the build if Firebase is required.");
                    IsInitialized = false;
                    return; // Return gracefully instead of throwing
                }
                catch (Exception innerEx)
                {
                    LogHelper.LogWarning($"[WebGLFirebaseManager] Firebase initialization failed: {innerEx.Message}");
                    LogHelper.LogWarning("[WebGLFirebaseManager] Continuing without Firebase functionality.");
                    IsInitialized = false;
                    return; // Return gracefully instead of throwing
                }

                await Task.Yield(); // Make it properly async
            }
            catch (Exception e)
            {
                LogHelper.LogError($"[WebGLFirebaseManager] Failed to initialize WebGL Firebase: {e.Message}");
                LogHelper.LogException(e);
                IsInitialized = false;
            }
#else
            LogHelper.LogWarning("[WebGLFirebaseManager] WebGL Firebase manager called on non-WebGL platform.");
            IsInitialized = false;
            await Task.CompletedTask;
#endif
        }

        public void CleanUpEmptyConversations(string conversationGroup, string uuid, Action<int> onComplete)
        {
            if (!IsInitialized)
            {
                onComplete?.Invoke(0);
                return;
            }

            // WebGL implementation - simplified for now
            LogHelper.LogWarning("[WebGLFirebaseManager] CleanUpEmptyConversations not yet implemented for WebGL.");
            onComplete?.Invoke(0);
        }

        public void CheckIsConversationEmpty(string conversationGroup, string conversationId, Action<bool> onComplete)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (!IsInitialized)
            {
                onComplete?.Invoke(false);
                return;
            }

            var callbackId = RegisterCallback(result =>
            {
                bool isEmpty = result == "true";
                onComplete?.Invoke(isEmpty);
            });

            try
            {
                CheckIsConversationEmptyWeb(conversationGroup, conversationId, callbackId);
            }
            catch (Exception e)
            {
                LogHelper.LogError($"[WebGLFirebaseManager] Failed to check if conversation is empty: {e.Message}");
                UnregisterCallback(callbackId);
                onComplete?.Invoke(false);
            }
#else
            onComplete?.Invoke(false);
#endif
        }

        public void DeleteConversation(string conversationGroup, string conversationId, Action<bool> onComplete)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (!IsInitialized)
            {
                onComplete?.Invoke(false);
                return;
            }

            var callbackId = RegisterCallback(result =>
            {
                bool success = result == "true";
                onComplete?.Invoke(success);
            });

            try
            {
                DeleteConversationWeb(conversationGroup, conversationId, callbackId);
            }
            catch (Exception e)
            {
                LogHelper.LogError($"[WebGLFirebaseManager] Failed to delete conversation: {e.Message}");
                UnregisterCallback(callbackId);
                onComplete?.Invoke(false);
            }
#else
            onComplete?.Invoke(false);
#endif
        }

        public void LoadMostRecentConversation(string conversationGroup, Action<string> onComplete)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (!IsInitialized)
            {
                onComplete?.Invoke(null);
                return;
            }

            var callbackId = RegisterCallback(result =>
            {
                onComplete?.Invoke(string.IsNullOrEmpty(result) ? null : result);
            });

            try
            {
                LoadMostRecentConversationWeb(conversationGroup, callbackId);
            }
            catch (Exception e)
            {
                LogHelper.LogError($"[WebGLFirebaseManager] Failed to load most recent conversation: {e.Message}");
                UnregisterCallback(callbackId);
                onComplete?.Invoke(null);
            }
#else
            onComplete?.Invoke(null);
#endif
        }

        public void CreateNewConversation(string conversationGroup, List<string> participantIds, Action<string> onComplete)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (!IsInitialized)
            {
                onComplete?.Invoke(null);
                return;
            }

            var callbackId = RegisterCallback(result =>
            {
                onComplete?.Invoke(string.IsNullOrEmpty(result) ? null : result);
            });

            try
            {
                var participantsJson = string.Join(",", participantIds);
                CreateNewConversationWeb(conversationGroup, participantsJson, callbackId);
            }
            catch (Exception e)
            {
                LogHelper.LogError($"[WebGLFirebaseManager] Failed to create conversation: {e.Message}");
                UnregisterCallback(callbackId);
                onComplete?.Invoke(null);
            }
#else
            onComplete?.Invoke(null);
#endif
        }

        public void SendMessageToConversation(string conversationGroup, string conversationId, string senderId, string content, Dictionary<string, object> metadata = null)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (!IsInitialized)
            {
                LogHelper.LogWarning("[WebGLFirebaseManager] Not initialized. Cannot send message.");
                return;
            }

            var callbackId = RegisterCallback(result =>
            {
                if (result == "true")
                {
                    LogHelper.Log($"[WebGLFirebaseManager] Message sent successfully.");
                }
                else
                {
                    LogHelper.LogError($"[WebGLFirebaseManager] Failed to send message: {result}");
                }
            });

            try
            {
                SendMessageWeb(conversationGroup, conversationId, senderId, content, callbackId);
            }
            catch (Exception e)
            {
                LogHelper.LogError($"[WebGLFirebaseManager] Failed to send message: {e.Message}");
                UnregisterCallback(callbackId);
            }
#else
            LogHelper.LogWarning("[WebGLFirebaseManager] SendMessageToConversation called on non-WebGL platform.");
#endif
        }

        public void LoadRecentMessages(string conversationGroup, string conversationId, int limit, Action<List<FirebaseConversationMessage>> onComplete)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (!IsInitialized)
            {
                onComplete?.Invoke(new List<FirebaseConversationMessage>());
                return;
            }

            var callbackId = RegisterCallback(result =>
            {
                var messages = ParseMessagesFromJson(result, conversationId);
                onComplete?.Invoke(messages);
            });

            try
            {
                LoadRecentMessagesWeb(conversationGroup, conversationId, limit, callbackId);
            }
            catch (Exception e)
            {
                LogHelper.LogError($"[WebGLFirebaseManager] Failed to load recent messages: {e.Message}");
                UnregisterCallback(callbackId);
                onComplete?.Invoke(new List<FirebaseConversationMessage>());
            }
#else
            onComplete?.Invoke(new List<FirebaseConversationMessage>());
#endif
        }

        public void LoadConversationMessages(string conversationGroup, string conversationId, Action<List<FirebaseConversationMessage>> onComplete)
        {
            // For WebGL, we can use LoadRecentMessages with a high limit
            LoadRecentMessages(conversationGroup, conversationId, 1000, onComplete);
        }

        private static string RegisterCallback(Action<string> callback)
        {
            var callbackId = $"callback_{_callbackCounter++}";
            _callbacks[callbackId] = callback;
            return callbackId;
        }

        private static void UnregisterCallback(string callbackId)
        {
            _callbacks.Remove(callbackId);
        }

        // This method will be called from JavaScript
        public static void OnJSCallback(string callbackId, string result)
        {
            if (_callbacks.TryGetValue(callbackId, out var callback))
            {
                try
                {
                    callback.Invoke(result);
                }
                catch (Exception e)
                {
                    LogHelper.LogError($"[WebGLFirebaseManager] Error in callback {callbackId}: {e.Message}");
                }
                finally
                {
                    UnregisterCallback(callbackId);
                }
            }
            else
            {
                LogHelper.LogWarning($"[WebGLFirebaseManager] Callback {callbackId} not found.");
            }
        }

        private List<FirebaseConversationMessage> ParseMessagesFromJson(string json, string conversationId)
        {
            var messages = new List<FirebaseConversationMessage>();

            try
            {
                if (string.IsNullOrEmpty(json) || json == "null")
                {
                    return messages;
                }

                LogHelper.Log($"[WebGLFirebaseManager] Parsing messages from JSON: {json}");

                // Simple JSON parsing - you can implement proper JSON parsing here if needed
                // For now, create a sample message structure
                // In production, parse the actual JSON from Firebase

                // Example: Create messages from parsed data
                // This is a simplified version - implement proper JSON parsing as needed

            }
            catch (Exception e)
            {
                LogHelper.LogError($"[WebGLFirebaseManager] Failed to parse messages JSON: {e.Message}");
            }

            return messages;
        }

        public void Dispose()
        {
            try
            {
                LogHelper.Log("[WebGLFirebaseManager] Disposing...");

                _callbacks.Clear();
                IsInitialized = false;

                LogHelper.Log("[WebGLFirebaseManager] Disposed successfully.");
            }
            catch (Exception e)
            {
                LogHelper.LogError($"[WebGLFirebaseManager] Error during disposal: {e.Message}");
                LogHelper.LogException(e);
            }
        }
    }
}