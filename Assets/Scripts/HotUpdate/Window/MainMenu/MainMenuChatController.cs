using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YuankunHuang.Unity.Core;
using YuankunHuang.Unity.Util;

namespace YuankunHuang.Unity.HotUpdate
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
        private GeneralWidgetConfig _tmpMessageConfig;

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

            _tmpMessageConfig = GameObject.Instantiate(_grid.itemPrefab, _hiddenRoot).GetComponent<GeneralWidgetConfig>();
        }

        public void Show()
        {
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
        }
        #endregion

        #region Events
        private void OnSendBtnClicked()
        {
            var txt = _inputField.text.Trim();

            if (!string.IsNullOrEmpty(txt))
            {
                _messages.Add(new MainMenuMessageData($"{++MessageIDTest}", ModuleRegistry.Get<IAccountManager>().Self, txt));
                _grid.AppendBottom(_messages.Count - 1);
                _grid.scrollRect.StopMovement();
                _grid.GoToBottom();

                ModuleRegistry.Get<INetworkManager>().RestApi.GetDummyMessage(
                    (message) =>
                    {
                        var data = new MainMenuMessageData($"{++MessageIDTest}", ModuleRegistry.Get<IAccountManager>().AI, message.body);
                        _messages.Add(data);
                        _grid.AppendBottom(_messages.Count - 1);
                        _grid.scrollRect.StopMovement();
                        _grid.GoToBottom();
                    },
                    (error) =>
                    {
                        Debug.LogError($"Failed to get dummy message: {error}");
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

        public Vector2 GetElementSize(int index)
        {
            var message = _messages[index];
            var size = CalculateMessageSize(message.Content);
            return size;
        }

        private Vector2 CalculateMessageSize(string text)
        {
            MainMenuMessageController.Show(_tmpMessageConfig, new MainMenuMessageData("temp", null, text));

            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)_tmpMessageConfig.transform);

            var height = LayoutUtility.GetPreferredHeight((RectTransform)_tmpMessageConfig.transform);
            var width = LayoutUtility.GetPreferredWidth((RectTransform)_tmpMessageConfig.transform);
            var size = new Vector2(width, height);
            return size;
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