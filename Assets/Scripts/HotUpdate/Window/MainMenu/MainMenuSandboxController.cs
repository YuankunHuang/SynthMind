using Firebase.Firestore;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YuankunHuang.Unity.Core;
using YuankunHuang.Unity.Util;

namespace YuankunHuang.Unity.HotUpdate
{
    public class MainMenuSandboxController : IMainMenuWidgetController, IGridHandler
    {
        private GeneralWidgetConfig _config;

        #region UI Ref
        private enum ExtraObj
        {
            Grid = 0,
            InputField = 1,
            HiddenRoot = 2,
            RawImage = 3,
        }

        private enum ExtraBtn
        {
            Send = 0,
        }

        private GridScrollView _grid;
        private TMP_InputField _inputField;
        private Transform _hiddenRoot;
        private RawImage _rawImg;

        private GeneralButton _sendBtn;
        #endregion

        #region Fields
        private List<MainMenuMessageData> _messages;
        private Dictionary<int, GeneralWidgetConfig> _tmpPrefabConfigs; // k: prefabType, v: config
        private string _conversationId;
        private bool _isSandboxLoaded;

        private static int MessageIDTest = 0;
        #endregion

        #region Lifecycle
        public MainMenuSandboxController(GeneralWidgetConfig config)
        {
            _config = config;
        }

        public void Init()
        {
            _messages = new();

            _grid = _config.ExtraObjectList[(int)ExtraObj.Grid].GetComponent<GridScrollView>();
            _inputField = _config.ExtraObjectList[(int)ExtraObj.InputField].GetComponent<TMP_InputField>();
            _hiddenRoot = _config.ExtraObjectList[(int)ExtraObj.HiddenRoot];
            _rawImg = _config.ExtraObjectList[(int)ExtraObj.RawImage].GetComponent<RawImage>();

            _sendBtn = _config.ExtraButtonList[(int)ExtraBtn.Send];

            _sendBtn.onClick.AddListener(OnSendBtnClicked);
            _inputField.onSelect.AddListener(OnInputFieldSelected);

            _tmpPrefabConfigs = new();
            for (var i = 0; i < _grid.itemPrefabs.Length; ++i)
            {
                var config = GameObject.Instantiate(_grid.itemPrefabs[i], _hiddenRoot).GetComponent<GeneralWidgetConfig>();
                _tmpPrefabConfigs[i] = config;
            }

            _grid.SetHandler(this);
            _grid.Activate();
        }

        public void Show()
        {
            SceneManager.LoadSceneAsync(SceneKeys.Sandbox, onFinished: () =>
            {
                _isSandboxLoaded = true;
                SandboxManager.Instance.Initialize(_rawImg, () =>
                {
                    if (!FirebaseManager.IsInitializing)
                    {
                        FirebaseManager.InitializeDataBase(success =>
                        {
                            var self = ModuleRegistry.Get<IAccountManager>().Self;
                            FirebaseManager.CreateNewConversation(FirebaseCollections.Command_Conversations, new List<string>() { self.UUID }, convId =>
                            {
                                _conversationId = convId;

                                var ai = ModuleRegistry.Get<IAccountManager>().AI;
                                AddToChat(
                                    ai,
                                    "Sandbox Environment Ready!",
                                    "Try these commands:",
                                    "'move character left' - Move the AI",
                                    "'spawn tree 3 2' - Plant at coordinates",
                                    "'clear all' - Reset environment"
                                );
                            });
                        });
                    }

                    _config.CanvasGroup.CanvasGroupOn();
                });
            });
        }

        public void Hide()
        {
            if (_isSandboxLoaded)
            {
                SceneManager.UnloadSceneAsync(SceneKeys.Sandbox, () =>
                {
                    _isSandboxLoaded = false;
                });
            }
            _config.CanvasGroup.CanvasGroupOff();
        }

        public void Dispose()
        {
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
            var input = _inputField.text.Trim();
            _inputField.text = "";

            if (string.IsNullOrEmpty(input))
            {
                return;
            }

            var commandManager = ModuleRegistry.Get<ICommandManager>();
            var isExecuted = commandManager.TryExecuteCommand(input);
            if (!isExecuted) // try natural language interpreter
            {
                isExecuted = commandManager.TryExecuteNatural(input);
            }

            var self = ModuleRegistry.Get<IAccountManager>().Self;
            var ai = ModuleRegistry.Get<IAccountManager>().AI;
            AddToChat(self, input);

            if (isExecuted)
            {
                AddToChat(ai, "Command executed successfully!");
            }
            else
            {
                AddToChat(ai, "Sorry. I couldn't understand.");
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

        #region Command
        private void AddToChat(List<MainMenuMessageData> messages)
        {
            _messages.AddRange(messages);
            _grid.AppendBottom(_messages.Count - 1);
            _grid.scrollRect.StopMovement();
            _grid.GoToBottom();

            foreach (var message in messages)
            {
                ModuleRegistry.Get<INetworkManager>().SendMessage(FirebaseCollections.Command_Conversations, _conversationId, message.Sender.UUID, message.Content, null,
                    ServerType.ChatPlayer, null, null);
            }
        }

        private void AddToChat(IAccount sender, params string[] messages)
        {
            var list = new List<MainMenuMessageData>();

            foreach (var message in messages)
            {
                var msgData = new MainMenuMessageData($"{++MessageIDTest}", sender, message, Timestamp.GetCurrentTimestamp().ToDateTime());
                list.Add(msgData);
            }

            AddToChat(list);
        }
        #endregion
    }
}