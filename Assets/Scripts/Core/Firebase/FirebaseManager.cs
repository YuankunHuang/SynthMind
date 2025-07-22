using Firebase;
using Firebase.Extensions;
using Firebase.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YuankunHuang.Unity.Core
{
    public class FirebaseManager
    {
        public static bool IsInitialized { get; private set; } = false;
        public static string SessionId { get; private set; }

        public static void InitializeDataBase(Action<bool> onComplete = null)
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                SessionId = Guid.NewGuid().ToString(); // Unique session ID for the current user session

                DependencyStatus dependencyStatus = task.Result;
                if (dependencyStatus == DependencyStatus.Available)
                {
                    // Firebase is ready to use
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
            // Firebase does not provide a direct dispose method, but you can clean up resources if needed.
            IsInitialized = false;
            LogHelper.Log("FirebaseManager disposed.");
        }

        #region Conversation
        public static void CreateNewConversation(List<string> participantIds, Action<string> onComplete)
        {
            if (participantIds == null || participantIds.Count < 1)
            {
                LogHelper.LogError("Cannot create conversation with no participants.");
                onComplete?.Invoke(null);
                return;
            }

            var db = FirebaseFirestore.DefaultInstance;
            var newConvRef = db.Collection(FirebaseCollections.Conversations).Document();
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
                    LogHelper.LogError(SessionId + $" Failed to create conversation: {task.Exception}");
                    onComplete?.Invoke(null);
                }
            });
        }

        public static void SendMessageToConversation(string conversationId, string senderId, string content, Dictionary<string, object> metadata = null)
        {
            var db = FirebaseFirestore.DefaultInstance;
            var msgRef = db
                .Collection(FirebaseCollections.Conversations).Document(conversationId)
                .Collection(FirebaseCollections.Messages).Document();
            var data = new Dictionary<string, object>()
            {
                { "senderId", senderId },
                { "content", content },
                { "timestamp", Timestamp.GetCurrentTimestamp() }
            };
            if (metadata != null && metadata.Count > 0)
            {
                data["metadata"] = metadata;
            }
            msgRef.SetAsync(data).ContinueWithOnMainThread(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    LogHelper.Log($"Message sent successfully in conversation {conversationId}.");

                    db.Collection(FirebaseCollections.Conversations).Document(conversationId)
                        .UpdateAsync("lastUpdated", Timestamp.GetCurrentTimestamp())
                        .ContinueWithOnMainThread(updateTask =>
                        {
                            if (updateTask.IsCompletedSuccessfully)
                            {
                                LogHelper.Log($"Conversation {conversationId} last updated timestamp updated successfully.");
                            }
                            else
                            {
                                LogHelper.LogError($"Failed to update last updated timestamp for conversation {conversationId}: {updateTask.Exception}");
                            }
                        });
                }
                else
                {
                    LogHelper.LogError($"Failed to send message in conversation {conversationId}: {task.Exception}");
                }
            });
        }

        public static void LoadRecentMessages(string convId, int limit, Action<List<FirebaseConversationMessage>> onComplete)
        {
            var db = FirebaseFirestore.DefaultInstance;
            db.Collection("conversations").Document(convId)
              .Collection("messages")
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
                              convId,
                              doc.GetValue<string>("senderId"),
                              doc.GetValue<string>("content"),
                              doc.ContainsField("metadata") ? doc.GetValue<Dictionary<string, object>>("metadata") : null));
                      }
                  }

                  onComplete?.Invoke(messages);
              });
        }

        public static void LoadMessagesBefore(string convId, DocumentSnapshot lastDoc, int limit, Action<List<FirebaseConversationMessage>, DocumentSnapshot> onComplete)
        {
            var db = FirebaseFirestore.DefaultInstance;
            var query = db.Collection("conversations").Document(convId)
                          .Collection("messages")
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
                        convId,
                        doc.GetValue<string>("senderId"),
                        doc.GetValue<string>("content"),
                        doc.ContainsField("metadata") ? doc.GetValue<Dictionary<string, object>>("metadata") : null
                    ));
                }

                var docs = result.Documents.ToList();
                var newAnchor = docs.Count > 0 ? docs[docs.Count - 1] : null;
                onComplete?.Invoke(messages, newAnchor);
            });
        }

        public static void LogConversationMessage(FirebaseConversationMessage msg)
        {
            var db = FirebaseFirestore.DefaultInstance;
            var msgRef = db
                .Collection(FirebaseCollections.Conversations).Document(msg.ConversationId)
                .Collection(FirebaseCollections.Messages);
            var data = new Dictionary<string, object>
            {
                { "senderId", msg.SenderId },
                { "content", msg.Content },
                { "timestamp", Timestamp.GetCurrentTimestamp() }
            };
            if (msg.Metadata != null && msg.Metadata.Count > 0)
            {
                data["metadata"] = msg.Metadata;
            }

            msgRef.AddAsync(data).ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted && !task.IsFaulted)
                {
                    LogHelper.Log($"Message logged successfully in conversation {msg.ConversationId}.");
                }
                else
                {
                    LogHelper.LogError($"Failed to log message in conversation {msg.ConversationId}: {task.Exception}");
                }
            });
        }

        public static void LoadConversationMessages(string conversationId, Action<List<FirebaseConversationMessage>> onComplete)
        {
            var db = FirebaseFirestore.DefaultInstance;
            db
            .Collection(FirebaseCollections.Conversations).Document(conversationId)
            .Collection(FirebaseCollections.Messages)
            .OrderByDescending("timestamp")
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
                            data.ContainsKey("senderId") ? data["senderId"].ToString() : string.Empty,
                            data.ContainsKey("content") ? data["content"].ToString() : string.Empty,
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
        public static readonly string Conversations = "conversations";
        public static readonly string Messages = "messages";
    }

    public struct FirebaseConversationMessage
    {
        public string ConversationId { get; private set; }
        public string SenderId { get; private set; }
        public string Content { get; private set; }
        public Dictionary<string, object> Metadata { get; private set; }

        public FirebaseConversationMessage(string conversationId, string senderId, string content, Dictionary<string, object> metadata = null)
        {
            ConversationId = conversationId;
            SenderId = senderId;
            Content = content;
            Metadata = metadata ?? new Dictionary<string, object>();
        }
    }
}