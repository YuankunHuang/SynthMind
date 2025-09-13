using System.Collections;
using System.Collections.Generic;
using TMPro;
using YuankunHuang.Unity.AccountCore;
using YuankunHuang.Unity.AudioCore;
using YuankunHuang.Unity.Core;
using YuankunHuang.Unity.GameDataConfig;
using YuankunHuang.Unity.ModuleCore;
using YuankunHuang.Unity.UICore;
using YuankunHuang.Unity.Util;

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
            Avatar = 5,
        }

        private enum ExtraBtn
        {
            Home = 0,
            Chat = 1,
            Sandbox = 2,
            Setting = 3,
            About = 4,
            Back = 5,
        }

        private Dictionary<Page, IMainMenuWidgetController> _widgetControllers;
        private GeneralWidgetConfig _avatarConfig;

        private GeneralButton _homeBtn;
        private GeneralButton _chatBtn;
        private GeneralButton _sandboxBtn;
        private GeneralButton _settingBtn;
        private GeneralButton _aboutBtn;
        private GeneralButton _backBtn;
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
            _avatarConfig = Config.ExtraWidgetConfigList[(int)ExtraConfig.Avatar];

            _homeBtn = Config.ExtraButtonList[(int)ExtraBtn.Home];
            _chatBtn = Config.ExtraButtonList[(int)ExtraBtn.Chat];
            _sandboxBtn = Config.ExtraButtonList[(int)ExtraBtn.Sandbox];
            _settingBtn = Config.ExtraButtonList[(int)ExtraBtn.Setting];
            _aboutBtn = Config.ExtraButtonList[(int)ExtraBtn.About];
            _backBtn = Config.ExtraButtonList[(int)ExtraBtn.Back];

            _homeBtn.onClick.AddListener(OnHomeBtnClicked);
            _chatBtn.onClick.AddListener(OnChatBtnClicked);
            _sandboxBtn.onClick.AddListener(OnSandboxBtnClicked);
            _settingBtn.onClick.AddListener(OnSettingBtnClicked);
            _aboutBtn.onClick.AddListener(OnAboutBtnClicked);
            _backBtn.onClick.AddListener(OnBackBtnClicked);
        }

        protected override void OnShow(IWindowData data, WindowShowState state)
        {
            if (state == WindowShowState.New)
            {
                ShowWidget(Page.Home, true);
            }

            var self = ModuleRegistry.Get<IAccountManager>().Self;
            CommPlayerAvatarController.Show(_avatarConfig, new CommPlayerAvatarData(self.Avatar, self.Nickname, () =>
            {
                ModuleRegistry.Get<IUIManager>().Show(WindowNames.ProfileWindow);
            }));

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
            _backBtn.onClick.RemoveAllListeners();
        }
        #endregion

        #region Events
        private void OnHomeBtnClicked()
        {
            ModuleRegistry.Get<IAudioManager>().PlayUI(GameDataConfig.AudioIdType.TestButtonClick);
            ShowWidget(Page.Home, false);
        }

        private void OnChatBtnClicked()
        {
            ModuleRegistry.Get<IAudioManager>().PlayUI(GameDataConfig.AudioIdType.TestButtonClick);
            ShowWidget(Page.Chat, false);
        }

        private void OnSandboxBtnClicked()
        {
            ModuleRegistry.Get<IAudioManager>().PlayUI(GameDataConfig.AudioIdType.TestButtonClick);
            ShowWidget(Page.Sandbox, false);
        }

        private void OnSettingBtnClicked()
        {
            ModuleRegistry.Get<IAudioManager>().PlayUI(GameDataConfig.AudioIdType.TestButtonClick);
            ShowWidget(Page.Setting, false);
        }

        private void OnAboutBtnClicked()
        {
            ModuleRegistry.Get<IAudioManager>().PlayUI(GameDataConfig.AudioIdType.TestButtonClick);
            ShowWidget(Page.About, false);
        }

        private void OnBackBtnClicked()
        {
            ModuleRegistry.Get<IAudioManager>().PlayUI(GameDataConfig.AudioIdType.TestButtonClick);
            ModuleRegistry.Get<IUIManager>().GoBackTo(WindowNames.LoginWindow, new LoginWindowData(true));
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