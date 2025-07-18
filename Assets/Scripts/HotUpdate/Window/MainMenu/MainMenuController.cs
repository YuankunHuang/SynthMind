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
        private enum Page
        {
            None,
            Home,
            Chat,
            Sandbox,
            Setting,
            About,
        }

        #region UI Ref
        private enum ExtraConfig
        {
            Home = 0,
            Chat = 1,
            Sandbox = 2,
            Setting = 3,
            About = 4,
        }

        private enum ExtraTMP
        {
            Title = 0,
        }

        private enum ExtraBtn
        {
            Home = 0,
            Chat = 1,
            Sandbox = 2,
            Setting = 3,
            About = 4,
        }

        private Dictionary<Page, IMainMenuWidgetController> _widgetControllers;

        private TMP_Text _titleTxt;

        private GeneralButton _homeBtn;
        private GeneralButton _chatBtn;
        private GeneralButton _sandboxBtn;
        private GeneralButton _settingBtn;
        private GeneralButton _aboutBtn;
        #endregion

        #region Field
        private Page _currentPage = Page.None;
        #endregion

        #region Lifecycle
        protected override void OnInit()
        {
            _widgetControllers = new Dictionary<Page, IMainMenuWidgetController>()
            {
                { Page.Home, new MainMenuHomeController(Config.ExtraWidgetConfigList[(int)ExtraConfig.Home]) },
                { Page.Chat, new MainMenuChatController(Config.ExtraWidgetConfigList[(int)ExtraConfig.Chat]) },
                { Page.Sandbox, new MainMenuSandboxController(Config.ExtraWidgetConfigList[(int)ExtraConfig.Sandbox]) },
                { Page.Setting, new MainMenuSettingController(Config.ExtraWidgetConfigList[(int)ExtraConfig.Setting]) },
                { Page.About, new MainMenuAboutController(Config.ExtraWidgetConfigList[(int)ExtraConfig.About]) },
            };
            foreach (var widgetController in _widgetControllers.Values)
            {
                widgetController.Init();
            }

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
                ShowWidget(Page.Home, true);
            }
            else
            {
                ShowWidget(_currentPage, true);
            }

            Config.CanvasGroup.CanvasGroupOn();
        }

        protected override void OnHide(WindowHideState state)
        {
            Config.CanvasGroup.CanvasGroupOff();
        }

        protected override void OnDispose()
        {
            foreach (var widgetController in _widgetControllers.Values)
            {
                widgetController.Dispose();
            }
            _widgetControllers.Clear();

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
            ShowWidget(Page.Home, false);
        }

        private void OnChatBtnClicked()
        {
            ShowWidget(Page.Chat, false);
        }

        private void OnSandboxBtnClicked()
        {
            ShowWidget(Page.Sandbox, false);
        }

        private void OnSettingBtnClicked()
        {
            ShowWidget(Page.Setting, false);
        }

        private void OnAboutBtnClicked()
        {
            ShowWidget(Page.About, false);
        }
        #endregion

        #region Content
        private void ShowWidget(Page page, bool forceShow)
        {
            if (_currentPage == page && !forceShow)
            {
                return;
            }

            _currentPage = page;

            foreach (var kv in _widgetControllers)
            {
                if (kv.Key == page)
                {
                    kv.Value.Show();
                }
                else
                {
                    kv.Value.Hide();
                }
            }
        }
        #endregion
    }
}