using System.Collections;
using System.Collections.Generic;
using TMPro;
using YuankunHuang.Unity.Core;
using YuankunHuang.Unity.Util;
using System;
using UnityEngine;

namespace YuankunHuang.Unity.HotUpdate
{
    public class MainMenuController : WindowControllerBase
    {
        private enum Panel
        {
            None,
            Home,
            Chat,
            Sandbox,
            Setting,
            About,
        }

        #region UI Ref
        private enum ExtraTMP
        {
            Box = 0,
            Title = 1,
        }

        private enum ExtraBtn
        {
            Home = 0,
            Chat = 1,
            Sandbox = 2,
            Setting = 3,
            About = 4,
        }

        private TMP_Text _boxTxt;
        private TMP_Text _titleTxt;

        private GeneralButton _homeBtn;
        private GeneralButton _chatBtn;
        private GeneralButton _sandboxBtn;
        private GeneralButton _settingBtn;
        private GeneralButton _aboutBtn;
        #endregion

        #region Field
        private Panel _currentPanel = Panel.None;
        #endregion

        #region Lifecycle
        protected override void OnInit()
        {
            _boxTxt = Config.ExtraTextMeshProList[(int)ExtraTMP.Box];
            _titleTxt = Config.ExtraTextMeshProList[(int)ExtraTMP.Title];

            _homeBtn = Config.ExtraButtonList[(int)ExtraBtn.Home];
            _chatBtn = Config.ExtraButtonList[(int)ExtraBtn.Chat];
            _sandboxBtn = Config.ExtraButtonList[(int)ExtraBtn.Sandbox];
            _settingBtn = Config.ExtraButtonList[(int)ExtraBtn.Setting];
            _aboutBtn = Config.ExtraButtonList[(int)ExtraBtn.About];

            _homeBtn.onClick.AddListener(OnHomeBtnClicked);
            _chatBtn.onClick.AddListener(OnChatBtnClicked);
            _sandboxBtn.onClick.AddListener(OnSandboxBtnClicked);
            _settingBtn.onClick.AddListener(OnSettingBtnClicked);
            _aboutBtn.onClick.AddListener(OnAboutBtnClicked);
        }

        protected override void OnShow(IWindowData data, WindowShowState state)
        {
            if (state == WindowShowState.New)
            {
                ShowSandbox(true);
            }
            else
            {
                switch (_currentPanel)
                {
                    case Panel.Home:
                        ShowHome(true);
                        break;
                    case Panel.Chat:
                        ShowChat(true); 
                        break;
                    case Panel.Sandbox:
                        ShowSandbox(true);
                        break;
                    case Panel.Setting:
                        ShowSetting(true);
                        break;
                    case Panel.About:
                        ShowAbout(true);
                        break;
                    default:
                        LogHelper.LogError($"Undefined panel: {_currentPanel}");
                        break;
                }
            }

            Config.CanvasGroup.CanvasGroupOn();
        }

        protected override void OnHide(WindowHideState state)
        {
            Config.CanvasGroup.CanvasGroupOff();
        }

        protected override void OnDispose()
        {
            _homeBtn.onClick.RemoveAllListeners();
            _chatBtn.onClick.RemoveAllListeners();
            _sandboxBtn.onClick.RemoveAllListeners();
            _settingBtn.onClick.RemoveAllListeners();
            _aboutBtn.onClick.RemoveAllListeners();
        }
        #endregion

        #region Events
        private void OnHomeBtnClicked()
        {
            ShowHome(false);
        }

        private void OnChatBtnClicked()
        {
            ShowChat(false);
        }

        private void OnSandboxBtnClicked()
        {
            ShowSandbox(false);
        }

        private void OnSettingBtnClicked()
        {
            ShowSetting(false);
        }

        private void OnAboutBtnClicked()
        {
            ShowAbout(false);
        }
        #endregion

        #region Content
        private void ShowHome(bool forceShow)
        {
            if (_currentPanel == Panel.Home && !forceShow)
            {
                return;
            }

            _currentPanel = Panel.Home;
            _boxTxt.text = "Welcome to SynthMind, an AI interaction sandbox based on Unity, demonstrating modular UI, AI dialogue, event tracking and physical system integration capabilities";
        }

        private void ShowChat(bool forceShow)
        {
            if (_currentPanel == Panel.Chat && !forceShow)
            {
                return;
            }

            _currentPanel = Panel.Chat;
            _boxTxt.text = "Chat";
        }

        private void ShowSandbox(bool forceShow)
        {
            if (_currentPanel == Panel.Sandbox && !forceShow)
            {
                return;
            }

            _currentPanel = Panel.Sandbox;
            _boxTxt.text = "Sandbox";
        }

        private void ShowSetting(bool forceShow)
        {
            if (_currentPanel == Panel.Setting && !forceShow)
            {
                return;
            }

            _currentPanel = Panel.Setting;
            _boxTxt.text = "Setting";
        }

        private void ShowAbout(bool forceShow)
        {
            if (_currentPanel == Panel.About && !forceShow)
            {
                return;
            }

            _currentPanel = Panel.About;
            _boxTxt.text = "About";
        }
        #endregion
    }
}