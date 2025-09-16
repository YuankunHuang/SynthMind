// Firebase WebGL JavaScript Library for Unity
mergeInto(LibraryManager.library, {
    InitFirebaseWeb: function() {
        console.log("[FirebaseWebGL] Initializing Firebase for WebGL...");

        // Check if Firebase is available
        if (typeof firebase === 'undefined') {
            console.error("[FirebaseWebGL] Firebase SDK is not loaded. Please include Firebase SDK in your HTML.");
            return false;
        }

        // Check if Firebase is properly initialized
        if (!firebase.apps.length) {
            console.error("[FirebaseWebGL] Firebase is not initialized. Please configure Firebase in your HTML template.");
            return false;
        }

        // Check if Firestore is available
        if (!firebase.firestore) {
            console.error("[FirebaseWebGL] Firestore is not available. Please include firebase-firestore-compat.js in your HTML.");
            return false;
        }

        // Verify Firebase app is properly configured
        var app = firebase.app();
        if (!app.options.projectId || app.options.projectId.includes('your-project')) {
            console.error("[FirebaseWebGL] Firebase project ID is not properly configured. Please update your Firebase config with real values.");
            return false;
        }

        console.log("[FirebaseWebGL] Firebase initialized successfully.");
        console.log("[FirebaseWebGL] Project ID: " + app.options.projectId);
        return true;
    },

    SendMessageWeb: function(conversationGroupPtr, conversationIdPtr, senderIdPtr, contentPtr, callbackPtr) {
        var conversationGroup = UTF8ToString(conversationGroupPtr);
        var conversationId = UTF8ToString(conversationIdPtr);
        var senderId = UTF8ToString(senderIdPtr);
        var content = UTF8ToString(contentPtr);
        var callbackId = UTF8ToString(callbackPtr);

        console.log("[FirebaseWebGL] Sending message:", {
            conversationGroup: conversationGroup,
            conversationId: conversationId,
            senderId: senderId,
            content: content
        });

        try {
            if (typeof firebase === 'undefined' || !firebase.firestore) {
                console.error("[FirebaseWebGL] Firebase Firestore not available");
                try {
                    // Call static C# method directly
                    if (window.unityInstance && window.unityInstance.Module) {
                        window.unityInstance.Module.ccall('WebGLFirebaseManager_InvokeCallback', null, ['string', 'string'], [callbackId, 'false']);
                    } else {
                        console.warn('[FirebaseWebGL] Unity instance not ready for callback');
                    }
                } catch(e) {
                    console.error('[FirebaseWebGL] Callback failed:', e);
                }
                return;
            }

            var db = firebase.firestore();
            var messageData = {
                messageId: generateUUID(),
                senderId: senderId,
                content: content,
                timestamp: firebase.firestore.FieldValue.serverTimestamp()
            };

            db.collection(conversationGroup)
                .doc(conversationId)
                .collection('messages')
                .add(messageData)
                .then(function(docRef) {
                    console.log("[FirebaseWebGL] Message sent successfully:", docRef.id);

                    // Update conversation lastUpdated
                    return db.collection(conversationGroup)
                        .doc(conversationId)
                        .update({
                            lastUpdated: firebase.firestore.FieldValue.serverTimestamp()
                        });
                })
                .then(function() {
                    try {
                        // Store result and trigger Unity callback via eval (simpler approach)
                        if (typeof window !== 'undefined') {
                            // Store the callback result globally
                            window.firebaseCallbackResults = window.firebaseCallbackResults || {};
                            window.firebaseCallbackResults[callbackId] = 'true';

                            // Call the static C# method directly via Module function
                            var callbackCode = `
                                if (typeof Module !== 'undefined' && Module.dynCall_vii) {
                                    try {
                                        var stringToPtr = function(str) {
                                            var len = lengthBytesUTF8(str) + 1;
                                            var ptr = _malloc(len);
                                            stringToUTF8(str, ptr, len);
                                            return ptr;
                                        };
                                        var callbackIdPtr = stringToPtr("${callbackId}");
                                        var resultPtr = stringToPtr("true");
                                        Module.dynCall_vii(Module._WebGLFirebaseManager_OnJSCallback, callbackIdPtr, resultPtr);
                                        _free(callbackIdPtr);
                                        _free(resultPtr);
                                    } catch(e) {
                                        console.error('[FirebaseWebGL] Direct callback failed:', e);
                                    }
                                }
                            `;
                            eval(callbackCode);
                        }
                    } catch(e) {
                        console.error('[FirebaseWebGL] Callback failed:', e);
                    }
                })
                .catch(function(error) {
                    console.error("[FirebaseWebGL] Error sending message:", error);
                    try {
                    // Call static C# method directly
                    if (window.unityInstance && window.unityInstance.Module) {
                        window.unityInstance.Module.ccall('WebGLFirebaseManager_InvokeCallback', null, ['string', 'string'], [callbackId, 'false']);
                    } else {
                        console.warn('[FirebaseWebGL] Unity instance not ready for callback');
                    }
                } catch(e) {
                    console.error('[FirebaseWebGL] Callback failed:', e);
                }
                });

        } catch (error) {
            console.error("[FirebaseWebGL] Exception in SendMessageWeb:", error);
            try {
                window.WebGLFirebaseManager_OnJSCallback(callbackId + '|false');
            } catch(e) {
                console.error('[FirebaseWebGL] Callback failed:', e);
            }
        }
    },

    LoadRecentMessagesWeb: function(conversationGroupPtr, conversationIdPtr, limit, callbackPtr) {
        var conversationGroup = UTF8ToString(conversationGroupPtr);
        var conversationId = UTF8ToString(conversationIdPtr);
        var callbackId = UTF8ToString(callbackPtr);

        console.log("[FirebaseWebGL] Loading recent messages:", {
            conversationGroup: conversationGroup,
            conversationId: conversationId,
            limit: limit
        });

        try {
            if (typeof firebase === 'undefined' || !firebase.firestore) {
                console.error("[FirebaseWebGL] Firebase Firestore not available");
                try {
                    window.WebGLFirebaseManager_OnJSCallback(callbackId + '|[]');
                } catch(e) {
                    console.error('[FirebaseWebGL] Callback failed:', e);
                }
                return;
            }

            var db = firebase.firestore();

            db.collection(conversationGroup)
                .doc(conversationId)
                .collection('messages')
                .orderBy('timestamp', 'desc')
                .limit(limit)
                .get()
                .then(function(querySnapshot) {
                    var messages = [];
                    querySnapshot.forEach(function(doc) {
                        var data = doc.data();
                        messages.push({
                            messageId: data.messageId || '',
                            senderId: data.senderId || '',
                            content: data.content || '',
                            timestamp: data.timestamp ? data.timestamp.toDate().toISOString() : new Date().toISOString()
                        });
                    });

                    var messagesJson = JSON.stringify(messages);
                    console.log("[FirebaseWebGL] Messages loaded:", messages.length);
                    try {
                        // Call static C# method directly
                        if (window.unityInstance && window.unityInstance.Module) {
                            window.unityInstance.Module.ccall('WebGLFirebaseManager_InvokeCallback', null, ['string', 'string'], [callbackId, messagesJson]);
                        } else {
                            console.warn('[FirebaseWebGL] Unity instance not ready for callback');
                        }
                    } catch(e) {
                        console.error('[FirebaseWebGL] Callback failed:', e);
                    }
                })
                .catch(function(error) {
                    console.error("[FirebaseWebGL] Error loading messages:", error);
                    try {
                        // Call static C# method directly
                        if (window.unityInstance && window.unityInstance.Module) {
                            window.unityInstance.Module.ccall('WebGLFirebaseManager_InvokeCallback', null, ['string', 'string'], [callbackId, '[]']);
                        } else {
                            console.warn('[FirebaseWebGL] Unity instance not ready for callback');
                        }
                    } catch(e) {
                        console.error('[FirebaseWebGL] Callback failed:', e);
                    }
                });

        } catch (error) {
            console.error("[FirebaseWebGL] Exception in LoadRecentMessagesWeb:", error);
            try {
                window.WebGLFirebaseManager_OnJSCallback(callbackId + '|[]');
            } catch(e) {
                console.error('[FirebaseWebGL] Callback failed:', e);
            }
        }
    },

    CreateNewConversationWeb: function(conversationGroupPtr, participantIdsPtr, callbackPtr) {
        var conversationGroup = UTF8ToString(conversationGroupPtr);
        var participantIds = UTF8ToString(participantIdsPtr).split(',');
        var callbackId = UTF8ToString(callbackPtr);

        console.log("[FirebaseWebGL] Creating new conversation:", {
            conversationGroup: conversationGroup,
            participantIds: participantIds
        });

        try {
            if (typeof firebase === 'undefined' || !firebase.firestore) {
                console.error("[FirebaseWebGL] Firebase Firestore not available");
                try {
                    window.WebGLFirebaseManager_OnJSCallback(callbackId + '|null');
                } catch(e) {
                    console.error('[FirebaseWebGL] Callback failed:', e);
                }
                return;
            }

            var db = firebase.firestore();
            var conversationData = {
                participants: participantIds,
                createdAt: firebase.firestore.FieldValue.serverTimestamp(),
                lastUpdated: firebase.firestore.FieldValue.serverTimestamp()
            };

            db.collection(conversationGroup)
                .add(conversationData)
                .then(function(docRef) {
                    console.log("[FirebaseWebGL] Conversation created:", docRef.id);
                    try {
                        // Call static C# method directly
                        if (window.unityInstance && window.unityInstance.Module) {
                            window.unityInstance.Module.ccall('WebGLFirebaseManager_InvokeCallback', null, ['string', 'string'], [callbackId, docRef.id]);
                        } else {
                            console.warn('[FirebaseWebGL] Unity instance not ready for callback');
                        }
                    } catch(e) {
                        console.error('[FirebaseWebGL] Callback failed:', e);
                    }
                })
                .catch(function(error) {
                    console.error("[FirebaseWebGL] Error creating conversation:", error);
                    try {
                        // Call static C# method directly
                        if (window.unityInstance && window.unityInstance.Module) {
                            window.unityInstance.Module.ccall('WebGLFirebaseManager_InvokeCallback', null, ['string', 'string'], [callbackId, 'null']);
                        } else {
                            console.warn('[FirebaseWebGL] Unity instance not ready for callback');
                        }
                    } catch(e) {
                        console.error('[FirebaseWebGL] Callback failed:', e);
                    }
                });

        } catch (error) {
            console.error("[FirebaseWebGL] Exception in CreateNewConversationWeb:", error);
            try {
                window.WebGLFirebaseManager_OnJSCallback(callbackId + '|null');
            } catch(e) {
                console.error('[FirebaseWebGL] Callback failed:', e);
            }
        }
    },

    DeleteConversationWeb: function(conversationGroupPtr, conversationIdPtr, callbackPtr) {
        var conversationGroup = UTF8ToString(conversationGroupPtr);
        var conversationId = UTF8ToString(conversationIdPtr);
        var callbackId = UTF8ToString(callbackPtr);

        console.log("[FirebaseWebGL] Deleting conversation:", {
            conversationGroup: conversationGroup,
            conversationId: conversationId
        });

        try {
            if (typeof firebase === 'undefined' || !firebase.firestore) {
                console.error("[FirebaseWebGL] Firebase Firestore not available");
                try {
                    // Call static C# method directly
                    if (window.unityInstance && window.unityInstance.Module) {
                        window.unityInstance.Module.ccall('WebGLFirebaseManager_InvokeCallback', null, ['string', 'string'], [callbackId, 'false']);
                    } else {
                        console.warn('[FirebaseWebGL] Unity instance not ready for callback');
                    }
                } catch(e) {
                    console.error('[FirebaseWebGL] Callback failed:', e);
                }
                return;
            }

            var db = firebase.firestore();

            // First delete all messages in the conversation
            db.collection(conversationGroup)
                .doc(conversationId)
                .collection('messages')
                .get()
                .then(function(querySnapshot) {
                    var batch = db.batch();
                    querySnapshot.forEach(function(doc) {
                        batch.delete(doc.ref);
                    });
                    return batch.commit();
                })
                .then(function() {
                    // Then delete the conversation document itself
                    return db.collection(conversationGroup).doc(conversationId).delete();
                })
                .then(function() {
                    console.log("[FirebaseWebGL] Conversation deleted successfully");
                    try {
                        // Store result and trigger Unity callback via eval (simpler approach)
                        if (typeof window !== 'undefined') {
                            // Store the callback result globally
                            window.firebaseCallbackResults = window.firebaseCallbackResults || {};
                            window.firebaseCallbackResults[callbackId] = 'true';

                            // Call the static C# method directly via Module function
                            var callbackCode = `
                                if (typeof Module !== 'undefined' && Module.dynCall_vii) {
                                    try {
                                        var stringToPtr = function(str) {
                                            var len = lengthBytesUTF8(str) + 1;
                                            var ptr = _malloc(len);
                                            stringToUTF8(str, ptr, len);
                                            return ptr;
                                        };
                                        var callbackIdPtr = stringToPtr("${callbackId}");
                                        var resultPtr = stringToPtr("true");
                                        Module.dynCall_vii(Module._WebGLFirebaseManager_OnJSCallback, callbackIdPtr, resultPtr);
                                        _free(callbackIdPtr);
                                        _free(resultPtr);
                                    } catch(e) {
                                        console.error('[FirebaseWebGL] Direct callback failed:', e);
                                    }
                                }
                            `;
                            eval(callbackCode);
                        }
                    } catch(e) {
                        console.error('[FirebaseWebGL] Callback failed:', e);
                    }
                })
                .catch(function(error) {
                    console.error("[FirebaseWebGL] Error deleting conversation:", error);
                    try {
                    // Call static C# method directly
                    if (window.unityInstance && window.unityInstance.Module) {
                        window.unityInstance.Module.ccall('WebGLFirebaseManager_InvokeCallback', null, ['string', 'string'], [callbackId, 'false']);
                    } else {
                        console.warn('[FirebaseWebGL] Unity instance not ready for callback');
                    }
                } catch(e) {
                    console.error('[FirebaseWebGL] Callback failed:', e);
                }
                });

        } catch (error) {
            console.error("[FirebaseWebGL] Exception in DeleteConversationWeb:", error);
            try {
                window.WebGLFirebaseManager_OnJSCallback(callbackId + '|false');
            } catch(e) {
                console.error('[FirebaseWebGL] Callback failed:', e);
            }
        }
    },

    LoadMostRecentConversationWeb: function(conversationGroupPtr, callbackPtr) {
        var conversationGroup = UTF8ToString(conversationGroupPtr);
        var callbackId = UTF8ToString(callbackPtr);

        console.log("[FirebaseWebGL] Loading most recent conversation:", {
            conversationGroup: conversationGroup
        });

        try {
            if (typeof firebase === 'undefined' || !firebase.firestore) {
                console.error("[FirebaseWebGL] Firebase Firestore not available");
                try {
                    window.WebGLFirebaseManager_OnJSCallback(callbackId + '|null');
                } catch(e) {
                    console.error('[FirebaseWebGL] Callback failed:', e);
                }
                return;
            }

            var db = firebase.firestore();

            db.collection(conversationGroup)
                .orderBy('lastUpdated', 'desc')
                .limit(1)
                .get()
                .then(function(querySnapshot) {
                    if (querySnapshot.empty) {
                        console.log("[FirebaseWebGL] No recent conversation found");
                        try {
                        // Call static C# method directly
                        if (window.unityInstance && window.unityInstance.Module) {
                            window.unityInstance.Module.ccall('WebGLFirebaseManager_InvokeCallback', null, ['string', 'string'], [callbackId, 'null']);
                        } else {
                            console.warn('[FirebaseWebGL] Unity instance not ready for callback');
                        }
                    } catch(e) {
                        console.error('[FirebaseWebGL] Callback failed:', e);
                    }
                        return;
                    }

                    var doc = querySnapshot.docs[0];
                    console.log("[FirebaseWebGL] Most recent conversation found:", doc.id);
                    try {
                        // Call static C# method directly
                        if (window.unityInstance && window.unityInstance.Module) {
                            window.unityInstance.Module.ccall('WebGLFirebaseManager_InvokeCallback', null, ['string', 'string'], [callbackId, doc.id]);
                        } else {
                            console.warn('[FirebaseWebGL] Unity instance not ready for callback');
                        }
                    } catch(e) {
                        console.error('[FirebaseWebGL] Callback failed:', e);
                    }
                })
                .catch(function(error) {
                    console.error("[FirebaseWebGL] Error loading most recent conversation:", error);
                    try {
                        // Call static C# method directly
                        if (window.unityInstance && window.unityInstance.Module) {
                            window.unityInstance.Module.ccall('WebGLFirebaseManager_InvokeCallback', null, ['string', 'string'], [callbackId, 'null']);
                        } else {
                            console.warn('[FirebaseWebGL] Unity instance not ready for callback');
                        }
                    } catch(e) {
                        console.error('[FirebaseWebGL] Callback failed:', e);
                    }
                });

        } catch (error) {
            console.error("[FirebaseWebGL] Exception in LoadMostRecentConversationWeb:", error);
            try {
                window.WebGLFirebaseManager_OnJSCallback(callbackId + '|null');
            } catch(e) {
                console.error('[FirebaseWebGL] Callback failed:', e);
            }
        }
    },

    CheckIsConversationEmptyWeb: function(conversationGroupPtr, conversationIdPtr, callbackPtr) {
        var conversationGroup = UTF8ToString(conversationGroupPtr);
        var conversationId = UTF8ToString(conversationIdPtr);
        var callbackId = UTF8ToString(callbackPtr);

        console.log("[FirebaseWebGL] Checking if conversation is empty:", {
            conversationGroup: conversationGroup,
            conversationId: conversationId
        });

        try {
            if (typeof firebase === 'undefined' || !firebase.firestore) {
                console.error("[FirebaseWebGL] Firebase Firestore not available");
                try {
                    // Call static C# method directly
                    if (window.unityInstance && window.unityInstance.Module) {
                        window.unityInstance.Module.ccall('WebGLFirebaseManager_InvokeCallback', null, ['string', 'string'], [callbackId, 'false']);
                    } else {
                        console.warn('[FirebaseWebGL] Unity instance not ready for callback');
                    }
                } catch(e) {
                    console.error('[FirebaseWebGL] Callback failed:', e);
                }
                return;
            }

            var db = firebase.firestore();

            db.collection(conversationGroup)
                .doc(conversationId)
                .collection('messages')
                .limit(1)
                .get()
                .then(function(querySnapshot) {
                    var isEmpty = querySnapshot.empty;
                    console.log("[FirebaseWebGL] Conversation empty check result:", isEmpty);
                    try {
                        // Call static C# method directly
                        if (window.unityInstance && window.unityInstance.Module) {
                            window.unityInstance.Module.ccall('WebGLFirebaseManager_InvokeCallback', null, ['string', 'string'], [callbackId, isEmpty ? 'true' : 'false']);
                        } else {
                            console.warn('[FirebaseWebGL] Unity instance not ready for callback');
                        }
                    } catch(e) {
                        console.error('[FirebaseWebGL] Callback failed:', e);
                    }
                })
                .catch(function(error) {
                    console.error("[FirebaseWebGL] Error checking if conversation is empty:", error);
                    try {
                    // Call static C# method directly
                    if (window.unityInstance && window.unityInstance.Module) {
                        window.unityInstance.Module.ccall('WebGLFirebaseManager_InvokeCallback', null, ['string', 'string'], [callbackId, 'false']);
                    } else {
                        console.warn('[FirebaseWebGL] Unity instance not ready for callback');
                    }
                } catch(e) {
                    console.error('[FirebaseWebGL] Callback failed:', e);
                }
                });

        } catch (error) {
            console.error("[FirebaseWebGL] Exception in CheckIsConversationEmptyWeb:", error);
            try {
                window.WebGLFirebaseManager_OnJSCallback(callbackId + '|false');
            } catch(e) {
                console.error('[FirebaseWebGL] Callback failed:', e);
            }
        }
    }
});

// Utility function to generate UUID
function generateUUID() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
        var r = Math.random() * 16 | 0,
            v = c == 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}