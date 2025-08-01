using Firebase.Firestore;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YuankunHuang.SynthMind.Core;
using YuankunHuang.SynthMind.Util;

namespace YuankunHuang.SynthMind.HotUpdate
{
    public class MainMenuChatController : IMainMenuWidgetController, IGridHandler
    {
        private GeneralWidgetConfig _config;

        #region UI Ref
        private enum ExtraBtn
        {
            Send = 0,
        }

        private enum ExtraObj
        {
            Grid = 0,
            InputField = 1,
            HiddenRoot = 2,
        }

        private GeneralButton _sendBtn;

        private GridScrollView _grid;
        private TMP_InputField _inputField;
        private Transform _hiddenRoot;
        #endregion

        #region Fields
        private List<MainMenuMessageData> _messages;
        private Dictionary<int, GeneralWidgetConfig> _tmpPrefabConfigs; // k: prefabType, v: config
        private string _conversationId;

        private static int MessageIDTest = 0;
        #endregion

        #region Lifecycle
        public MainMenuChatController(GeneralWidgetConfig config)
        {
            _config = config;
        }

        public void Init()
        {
            _messages = new();

            _sendBtn = _config.ExtraButtonList[(int)ExtraBtn.Send];

            _grid = _config.ExtraObjectList[(int)ExtraObj.Grid].GetComponent<GridScrollView>();
            _grid.SetHandler(this);
            _grid.Activate();

            _inputField = _config.ExtraObjectList[(int)ExtraObj.InputField].GetComponent<TMP_InputField>();
            _hiddenRoot = _config.ExtraObjectList[(int)ExtraObj.HiddenRoot];

            _sendBtn.onClick.AddListener(OnSendBtnClicked);
            _inputField.onSelect.AddListener(OnInputFieldSelected);

            _tmpPrefabConfigs = new();
            for (var i = 0; i < _grid.itemPrefabs.Length; ++i)
            {
                var config = GameObject.Instantiate(_grid.itemPrefabs[i], _hiddenRoot).GetComponent<GeneralWidgetConfig>();
                _tmpPrefabConfigs[i] = config;
            }
        }

        public void Show()
        {
            if (!FirebaseManager.IsInitializing)
            {
                FirebaseManager.InitializeDataBase(success =>
                {
                    if (success)
                    {
                        FirebaseManager.LoadMostRecentConversation(convId =>
                        {
                            if (!string.IsNullOrEmpty(convId))
                            {
                                _conversationId = convId;

                                FirebaseManager.LoadConversationMessages(_conversationId, messages =>
                                {
                                    if (messages != null && messages.Count > 0)
                                    {
                                        foreach (var msg in messages)
                                        {
                                            var sender = ModuleRegistry.Get<IAccountManager>().GetAccount(msg.SenderId);
                                            _messages.Add(new MainMenuMessageData(msg.MessageId, sender, msg.Content, msg.Timestamp.ToDateTime()));
                                        }

                                        _grid.Refresh();
                                    }
                                });
                            }
                            else
                            {
                                var self = ModuleRegistry.Get<IAccountManager>().Self;
                                var ai = ModuleRegistry.Get<IAccountManager>().AI;
                                FirebaseManager.CreateNewConversation(new List<string>() { self.UUID, ai.UUID }, convId =>
                                {
                                    _conversationId = convId;
                                });
                            }
                        });
                    }
                });
            }
            else
            {
                LogHelper.LogError($"FirebaseManager is initializing.");
            }

            _config.CanvasGroup.CanvasGroupOn();
        }

        public void Hide()
        {
            _config.CanvasGroup.CanvasGroupOff();
        }

        public void Dispose()
        {
            _messages.Clear();

            _grid.Deactivate();
            _grid = null;

            _sendBtn.onClick.RemoveAllListeners();
            _inputField.onSelect.RemoveAllListeners();
        }
        #endregion

        #region Events
        private void OnInputFieldSelected(string content)
        {
            _grid.GoToBottom();
        }

        private void OnSendBtnClicked()
        {
            var content = _inputField.text.Trim();

            if (!string.IsNullOrEmpty(content))
            {
                _messages.Add(new MainMenuMessageData($"{++MessageIDTest}", ModuleRegistry.Get<IAccountManager>().Self, content, Timestamp.GetCurrentTimestamp().ToDateTime()));
                _grid.AppendBottom(_messages.Count - 1);
                _grid.scrollRect.StopMovement();
                _grid.GoToBottom();

                var self = ModuleRegistry.Get<IAccountManager>().Self;
                ModuleRegistry.Get<INetworkManager>().SendMessage(_conversationId, self.UUID, content, null,
                    ServerType.ChatAI,
                    (reply) =>
                    {
                        var ai = ModuleRegistry.Get<IAccountManager>().AI;
                        var data = new MainMenuMessageData($"{++MessageIDTest}", ai, reply, Timestamp.GetCurrentTimestamp().ToDateTime());

                        _messages.Add(data);
                        _grid.AppendBottom(_messages.Count - 1);
                        _grid.scrollRect.StopMovement();
                        _grid.GoToBottom();
                    },
                    (error) =>
                    {
                        Debug.LogError($"Failed to get a reply: {error}");
                    }
                );
            }

            _inputField.text = string.Empty;
        }
        #endregion

        #region Grid Control
        public int GetDataCount()
        {
            return _messages.Count;
        }

        public int GetPrefabType(int index)
        {
            var message = _messages[index];
            switch (message.Type)
            {
                case MainMenuMessageType.Other:
                    return 0;
                case MainMenuMessageType.Self:
                    return 1;
                default:
                    LogHelper.LogError($"Undefined message type: {message.Type}");
                    return 0;
            }
        }

        public Vector2 GetElementSize(int index)
        {
            var message = _messages[index];
            var prefabType = GetPrefabType(index);

            var config = _tmpPrefabConfigs[prefabType];
            MainMenuMessageController.Show(config, message);

            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)config.transform);

            return ((RectTransform)config.transform).sizeDelta;
        }

        public void OnElementShow(GridScrollViewElement element)
        {
            if (element is GeneralWidgetConfig config)
            {
                MainMenuMessageController.Show(config, _messages[element.Index]);
            }
        }

        public void OnElementHide(GridScrollViewElement element)
        {
            if (element is GeneralWidgetConfig config)
            {
                MainMenuMessageController.Hide(config);
            }
        }

        public void OnElementCreate(GridScrollViewElement element)
        {
        }

        public void OnElementRelease(GridScrollViewElement element)
        {
        }
        #endregion
    }
}