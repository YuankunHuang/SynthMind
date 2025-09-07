using Firebase;
using Firebase.Extensions;
using Firebase.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using YuankunHuang.Unity.Core;

namespace YuankunHuang.Unity.FirebaseCore
{
    public class FirebaseManager
    {
        public static bool IsInitialized { get; private set; } = false;
        public static bool IsInitializing { get; private set; } = false;

        private static FirebaseApp _firebaseApp;
        private static List<System.Threading.CancellationTokenSource> _activeTasks = new List<System.Threading.CancellationTokenSource>();

        public static void InitializeDataBase(Action<bool> onComplete = null)
        {
            if (IsInitialized)
            {
                onComplete?.Invoke(true);
                LogHelper.Log($"Firebase is already initialized.");
                return;
            }

            if (IsInitializing)
            {
                LogHelper.Log($"Firebase is already initializing.");
                return;
            }

            IsInitializing = true;

            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                IsInitializing = false;

                DependencyStatus dependencyStatus = task.Result;

                if (dependencyStatus == DependencyStatus.Available)
                {
                    // Firebase is ready to use
                    _firebaseApp = FirebaseApp.DefaultInstance;
                    IsInitialized = true;
                    LogHelper.Log("Firebase initialized successfully.");
                    onComplete?.Invoke(true);
                }
                else
                {
                    // Handle the error
                    IsInitialized = false;
                    LogHelper.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
                    onComplete?.Invoke(false);
                }
            });
        }

        public static void Dispose()
        {
            if (!IsInitialized && !IsInitializing)
            {
                return;
            }

            try
            {
                foreach (var tokenSource in _activeTasks)
                {
                    try
                    {
                        tokenSource.Cancel();
                        tokenSource.Dispose();
                    }
                    catch (Exception ex)
                    {
                        LogHelper.LogError($"Error disposing task: {ex.Message}");
                    }
                }
                _activeTasks.Clear();

                if (_firebaseApp != null)
                {
                    _firebaseApp.Dispose();
                    _firebaseApp = null;
                }

                FirebaseApp.DefaultInstance?.Dispose();

                var firebaseServices = GameObject.Find("Firebase Services");
                if (firebaseServices != null)
                {
                    GameObject.DestroyImmediate(firebaseServices);
                }

                IsInitialized = false;
                IsInitializing = false;
             
                LogHelper.Log("FirebaseManager disposed.");
            }
            catch (Exception ex)
            {
                LogHelper.LogError($"Error disposing FirebaseManager: {ex.Message}");
            }
        }

        private static System.Threading.CancellationTokenSource CreateCancellationToken()
        {
            var tokenSource = new System.Threading.CancellationTokenSource();
            _activeTasks.Add(tokenSource);
            return tokenSource;
        }

        private static void RemoveCancellationToken(System.Threading.CancellationTokenSource tokenSource)
        {
            if (tokenSource != null)
            {
                _activeTasks.Remove(tokenSource);
                tokenSource.Dispose();
            }
        }

        #region Conversation
        public static void CleanUpEmptyConversations(string conversationGroup, string uuid, Action<int> onComplete)
        {
            if (!IsInitialized)
            {
                onComplete?.Invoke(0);
                return;
            }

            var db = FirebaseFirestore.DefaultInstance;
            db.Collection(conversationGroup)
                .WhereArrayContains("participants", uuid) // which the user parcipates
                .GetSnapshotAsync()
                .ContinueWithOnMainThread(task =>
                {
                    if (!task.IsCompletedSuccessfully)
                    {
                        onComplete?.Invoke(0);
                        return;
                    }

                    var conversations = task.Result.Documents.ToList();
                    var deletedCount = 0;
                    var remaining = conversations.Count;

                    if (remaining < 1) // no conversation to delete
                    {
                        onComplete?.Invoke(0);
                        return;
                    }

                    foreach (var conv in conversations)
                    {
                        CheckIsConversationEmpty(conversationGroup, conv.Id, isEmpty =>
                        {
                            if (isEmpty)
                            {
                                DeleteConversation(conversationGroup, conv.Id, isDeleted =>
                                {
                                    if (isDeleted)
                                    {
                                        ++deletedCount;
                                    }
                                    --remaining;
                                    if (remaining < 1)
                                    {
                                        onComplete?.Invoke(deletedCount);
                                        return;
                                    }
                                });
                            }
                            else
                            {
                                --remaining;
                                if (remaining < 1)
                                {
                                    onComplete?.Invoke(deletedCount);
                                    return;
                                }
                            }
                        });
                    }
                });
        }

        public static void CheckIsConversationEmpty(string conversationGroup, string conversationId, Action<bool> onComplete)
        {
            if (!IsInitialized)
            {
                LogHelper.LogError("Firebase is not initialized. Cannot check if conversation is empty.");
                onComplete?.Invoke(false);
                return;
            }

            var db = FirebaseFirestore.DefaultInstance;
            db.Collection(conversationGroup)
                .Document(conversationId)
                .Collection(FirebaseCollections.Messages)
                .Limit(1)
                .GetSnapshotAsync()
                .ContinueWithOnMainThread(task =>
                {
                    var isEmpty = task.IsCompletedSuccessfully && task.Result.Count == 0;
                    onComplete?.Invoke(isEmpty);
                });
        }

        public static void DeleteConversation(string conversationGroup, string conversationId, Action<bool> onComplete)
        {
            if (!IsInitialized || string.IsNullOrEmpty(conversationId))
            {
                onComplete?.Invoke(false);
                return;
            }

            var db = FirebaseFirestore.DefaultInstance;
            db.Collection(conversationGroup)
                .Document(conversationId)
                .Collection(FirebaseCollections.Messages)
                .GetSnapshotAsync()
                .ContinueWithOnMainThread(task =>
                {
                    if (task.IsCompletedSuccessfully)
                    {
                        // start a db batch
                        var batch = db.StartBatch();

                        // add all messages to the batch-delete
                        foreach (var doc in task.Result.Documents)
                        {
                            batch.Delete(doc.Reference);
                        }

                        batch.CommitAsync().ContinueWithOnMainThread(batchTask =>
                        {
                            // after deleting all messages
                            // delete the conversation itself
                            db.Collection(conversationGroup)
                                .Document(conversationId)
                                .DeleteAsync()
                                .ContinueWithOnMainThread(deleteTask =>
                                {
                                    var success = deleteTask.IsCompletedSuccessfully;
                                    LogHelper.Log(success ? $"Deleted conversation {conversationId}" : $"Failed to delete conversation {conversationId}");
                                    onComplete?.Invoke(success);
                                });
                        });
                    }
                    else
                    {
                        onComplete?.Invoke(false);
                    }
                });
        }

        public static void LoadMostRecentConversation(string conversationGroup, Action<string> onComplete)
        {
            if (!IsInitialized)
            {
                LogHelper.LogError("Firebase is not initialized. Cannot load recent conversation.");
                onComplete?.Invoke(null);
                return;
            }

            var db = FirebaseFirestore.DefaultInstance;
            db.Collection(conversationGroup)
                .OrderByDescending("lastUpdated")
                .Limit(1)
                .GetSnapshotAsync()
                .ContinueWithOnMainThread(task =>
                {
                    if (task.IsCompletedSuccessfully && task.Result.Count > 0) // valid conversation found
                    {
                        var doc = task.Result.Documents.First();
                        var conversationId = doc.Id;
                        onComplete?.Invoke(conversationId);

                        LogHelper.Log($"Conversation found: {conversationId}");
                    }
                    else
                    {
                        LogHelper.Log($"No recent conversation found.");
                        onComplete?.Invoke(null);
                    }
                });
        }

        public static void CreateNewConversation(string conversationGroup, List<string> participantIds, Action<string> onComplete)
        {
            if (!IsInitialized)
            {
                LogHelper.LogError("Firebase is not initialized. Cannot create conversation.");
                onComplete?.Invoke(null);
                return;
            }

            if (participantIds == null || participantIds.Count < 1)
            {
                LogHelper.LogError("Cannot create conversation with no participants.");
                onComplete?.Invoke(null);
                return;
            }

            var db = FirebaseFirestore.DefaultInstance;
            var newConvRef = db.Collection(conversationGroup).Document();
            var convData = new Dictionary<string, object>
            {
                { "participants", participantIds },
                { "createdAt", Timestamp.GetCurrentTimestamp() },
                { "lastUpdated", Timestamp.GetCurrentTimestamp() }
            };

            newConvRef.SetAsync(convData).ContinueWithOnMainThread(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    LogHelper.Log($"Conversation created: {newConvRef.Id}");
                    onComplete?.Invoke(newConvRef.Id);
                }
                else
                {
                    LogHelper.LogError($"Failed to create conversation: {task.Exception}");
                    onComplete?.Invoke(null);
                }
            });
        }

        public static void SendMessageToConversation(string conversationGroup, string conversationId, string senderId, string content, Dictionary<string, object> metadata = null)
        {
            if (!IsInitialized || string.IsNullOrEmpty(conversationId) || string.IsNullOrEmpty(senderId) || string.IsNullOrEmpty(content))
            {
                LogHelper.LogError("Firebase is not initialized or invalid parameters provided for sending message.");
                return;
            }

            var db = FirebaseFirestore.DefaultInstance;
            var msgRef = db
                .Collection(conversationGroup).Document(conversationId)
                .Collection(FirebaseCollections.Messages).Document();
            var data = new Dictionary<string, object>()
            {
                { "messageId", Guid.NewGuid().ToString() },
                { "senderId", senderId },
                { "content", content },
                { "timestamp", FieldValue.ServerTimestamp }
            };
            if (metadata != null && metadata.Count > 0)
            {
                data["metadata"] = metadata;
            }
            msgRef.SetAsync(data).ContinueWithOnMainThread(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    LogHelper.Log($"Message sent successfully in conversation {conversationId}, by {senderId}, content: {content}");

                    db.Collection(conversationGroup).Document(conversationId)
                        .UpdateAsync("lastUpdated", Timestamp.GetCurrentTimestamp())
                        .ContinueWithOnMainThread(updateTask =>
                        {
                            if (updateTask.IsCompletedSuccessfully)
                            {
                                LogHelper.Log($"Conversation {conversationId} last updated timestamp updated successfully.");
                            }
                            else
                            {
                                LogHelper.LogError($"Failed to update last updated timestamp for conversation {conversationId}: {updateTask.Exception}. Error: {task.Exception?.Flatten()?.ToString()}");
                            }
                        });
                }
                else
                {
                    LogHelper.LogError($"Failed to send message in conversation {conversationId}: {task.Exception}");
                }
            });
        }

        public static void LoadRecentMessages(string conversationGroup, string conversationId, int limit, Action<List<FirebaseConversationMessage>> onComplete)
        {
            if (!IsInitialized)
            {
                LogHelper.LogError("Firebase is not initialized. Cannot load recent messages.");
                onComplete?.Invoke(new List<FirebaseConversationMessage>());
                return;
            }

            var db = FirebaseFirestore.DefaultInstance;
            db.Collection(conversationGroup).Document(conversationId)
                .Collection(FirebaseCollections.Messages)
                .OrderByDescending("timestamp")
                .Limit(limit)
                .GetSnapshotAsync().ContinueWithOnMainThread(task =>
                {
                    var messages = new List<FirebaseConversationMessage>();
                    if (task.Result != null)
                    {
                        foreach (var doc in task.Result.Documents)
                        {
                            messages.Add(new FirebaseConversationMessage(
                                conversationId,
                                doc.GetValue<string>("messageId"),
                                doc.GetValue<string>("senderId"),
                                doc.GetValue<string>("content"),
                                doc.ContainsField("timestamp") ? doc.GetValue<Timestamp>("timestamp") : Timestamp.GetCurrentTimestamp(),
                                doc.ContainsField("metadata") ? doc.GetValue<Dictionary<string, object>>("metadata") : null)
                            );
                        }
                    }

                    onComplete?.Invoke(messages);
                });
        }

        public static void LoadMessagesBefore(string conversationGroup, string conversationId, DocumentSnapshot lastDoc, int limit, Action<List<FirebaseConversationMessage>, DocumentSnapshot> onComplete)
        {
            if (!IsInitialized)
            {
                LogHelper.LogError("Firebase is not initialized. Cannot load messages before.");
                onComplete?.Invoke(new List<FirebaseConversationMessage>(), null);
                return;
            }

            if (lastDoc == null)
            {
                LogHelper.LogError("lastDoc is null in LoadMessagesBefore.");
                onComplete?.Invoke(new List<FirebaseConversationMessage>(), null);
                return;
            }

            var db = FirebaseFirestore.DefaultInstance;
            var query = db.Collection(conversationGroup).Document(conversationId)
                          .Collection(FirebaseCollections.Messages)
                          .OrderByDescending("timestamp")
                          .StartAfter(lastDoc)
                          .Limit(limit);

            query.GetSnapshotAsync().ContinueWithOnMainThread((Task<QuerySnapshot> task) =>
            {
                if (task.IsFaulted || !task.IsCompleted)
                {
                    LogHelper.LogError("LoadMessagesBefore failed:" + task.Exception);
                    onComplete?.Invoke(null, null);
                    return;
                }

                var result = task.Result;
                var messages = new List<FirebaseConversationMessage>();

                foreach (var doc in result.Documents)
                {
                    messages.Add(new FirebaseConversationMessage(
                        conversationId,
                        doc.GetValue<string>("messageId"),
                        doc.GetValue<string>("senderId"),
                        doc.GetValue<string>("content"),
                        doc.ContainsField("timestamp") ? doc.GetValue<Timestamp>("timestamp") : Timestamp.GetCurrentTimestamp(),
                        doc.ContainsField("metadata") ? doc.GetValue<Dictionary<string, object>>("metadata") : null
                    ));
                }

                var docs = result.Documents.ToList();
                var newAnchor = docs.Count > 0 ? docs[docs.Count - 1] : null;
                onComplete?.Invoke(messages, newAnchor);
            });
        }

        public static void LoadConversationMessages(string conversationGroup, string conversationId, Action<List<FirebaseConversationMessage>> onComplete)
        {
            if (!IsInitialized)
            {
                LogHelper.LogError("Firebase is not initialized. Cannot load conversation messages.");
                onComplete?.Invoke(new List<FirebaseConversationMessage>());
                return;
            }

            var db = FirebaseFirestore.DefaultInstance;
            db
            .Collection(conversationGroup).Document(conversationId)
            .Collection(FirebaseCollections.Messages)
            .OrderBy("timestamp")
            .GetSnapshotAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted && !task.IsFaulted)
                {
                    var snapshot = task.Result;
                    var messages = new List<FirebaseConversationMessage>();
                    foreach (var doc in snapshot.Documents)
                    {
                        var data = doc.ToDictionary();
                        var message = new FirebaseConversationMessage(
                            conversationId,
                            data.ContainsKey("messageId") ? data["messageId"].ToString() : string.Empty,
                            data.ContainsKey("senderId") ? data["senderId"].ToString() : string.Empty,
                            data.ContainsKey("content") ? data["content"].ToString() : string.Empty,
                            data.ContainsKey("timestamp") ? (Timestamp)data["timestamp"] : Timestamp.GetCurrentTimestamp(),
                            data.ContainsKey("metadata") ? (Dictionary<string, object>)data["metadata"] : null
                        );
                        messages.Add(message);
                    }
                    onComplete?.Invoke(messages);
                }
                else
                {
                    LogHelper.LogError($"Failed to load messages for conversation {conversationId}: {task.Exception}");
                    onComplete?.Invoke(null);
                }
            });
        }
        #endregion
    }

    public class FirebaseCollections
    {
        public static readonly string AI_Conversations = "ai_conversations";
        public static readonly string Command_Conversations = "command_conversations";
        public static readonly string Messages = "messages";
    }

    public struct FirebaseConversationMessage
    {
        public string ConversationId { get; private set; }
        public string MessageId { get; private set; }
        public string SenderId { get; private set; }
        public string Content { get; private set; }
        public Timestamp Timestamp { get; private set; }
        public Dictionary<string, object> Metadata { get; private set; }

        public FirebaseConversationMessage(string conversationId, string messageId, string senderId, string content, Timestamp timeStamp, Dictionary<string, object> metadata = null)
        {
            ConversationId = conversationId;
            MessageId = messageId;
            SenderId = senderId;
            Content = content;
            Timestamp = timeStamp;
            Metadata = metadata ?? new Dictionary<string, object>();
        }
    }
}