using UnityEngine;
using TMPro;
using YuankunHuang.Unity.Core;
using YuankunHuang.Unity.UICore;
using YuankunHuang.Unity.ModuleCore;
using YuankunHuang.Unity.AccountCore;
using YuankunHuang.Unity.LocalizationCore;
using YuankunHuang.Unity.AudioCore;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace YuankunHuang.Unity.HotUpdate
{
    public class LoginWindowData : IWindowData
    {
        public bool Reset;

        public LoginWindowData(bool reset)
        {
            Reset = reset;
        }
    }

    public class LoginWindowController : WindowControllerBase
    {
        #region UI Ref
        private enum ExtraTMP
        {
            Notice = 0,
        }

        private enum ExtraBtn
        {
            Login = 0,
            Quit = 1,
            Admin = 2,
        }

        private enum ExtraObj
        {
            UsernameInputField = 0,
            PasswordInputField = 1,
        }

        private TMP_Text _noticeTxt;
        
        private GeneralButton _loginBtn;
        private GeneralButton _quitBtn;
        private GeneralButton _adminBtn;

        private TMP_InputField _usernameInputField;
        private TMP_InputField _passwordInputField;
        #endregion

        #region Lifecycle
        protected override void OnInit()
        {
            _noticeTxt = Config.ExtraTextMeshProList[(int)ExtraTMP.Notice];

            _loginBtn = Config.ExtraButtonList[(int)ExtraBtn.Login];
            _quitBtn = Config.ExtraButtonList[(int)ExtraBtn.Quit];
            _adminBtn = Config.ExtraButtonList[(int)ExtraBtn.Admin];

            _usernameInputField = Config.ExtraObjectList[(int)ExtraObj.UsernameInputField].GetComponent<TMP_InputField>();
            _passwordInputField = Config.ExtraObjectList[(int)ExtraObj.PasswordInputField].GetComponent<TMP_InputField>();

            _loginBtn.onClick.AddListener(OnLoginBtnClicked);
            _quitBtn.onClick.AddListener(OnQuitBtnClicked);
            _adminBtn.onClick.AddListener(OnAdminBtnClicked);
        }

        protected override void OnShow(IWindowData data, WindowShowState state)
        {
            if (state == WindowShowState.New)
            {
                _usernameInputField.text = string.Empty;
                _passwordInputField.text = string.Empty;
                _noticeTxt.text = string.Empty;

                ModuleRegistry.Get<IAudioManager>().PlayBGMAsync(GameDataConfig.AudioIdType.TestBGM);
            }

            var windowData = (LoginWindowData)data;
            if (windowData != null && windowData.Reset)
            {
                _usernameInputField.text = string.Empty;
                _passwordInputField.text = string.Empty;
                _noticeTxt.text = string.Empty;
            }
        }

        protected override void OnDispose()
        {
            _loginBtn.onClick.RemoveAllListeners();
            _quitBtn.onClick.RemoveAllListeners();
            _adminBtn.onClick.RemoveAllListeners();
        }
        #endregion

        #region Event Handlers
        private void OnQuitBtnClicked()
        {
            ModuleRegistry.Get<IAudioManager>().PlayUI(GameDataConfig.AudioIdType.TestButtonClick);

            var locManager = ModuleRegistry.Get<ILocalizationManager>();
            var uiManager = ModuleRegistry.Get<IUIManager>();

            // Use batch localization for WebGL compatibility
            locManager.GetLocalizedTexts(
                new[] { LocalizationKeys.QuitGameTitle, LocalizationKeys.QuitGameContent },
                (texts) =>
                {
                    var title = texts[LocalizationKeys.QuitGameTitle];
                    var content = texts[LocalizationKeys.QuitGameContent];
                    uiManager.Show(WindowNames.ConfirmWindow, new ConfirmWindowData(title, content, QuitApp));
                });
        }

        private void QuitApp()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnLoginBtnClicked()
        {
            ModuleRegistry.Get<IAudioManager>().PlayUI(GameDataConfig.AudioIdType.TestButtonClick);

            string username = _usernameInputField.text.Trim();
            string password = _passwordInputField.text.Trim();
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ModuleRegistry.Get<ILocalizationManager>().GetLocalizedText(LocalizationKeys.MainMenuNoticeEmptyUsernameOrPassword, (text) => {
                    _noticeTxt.text = text;
                });
                return;
            }

            var accountManager = ModuleRegistry.Get<IAccountManager>();
            accountManager.Login(username, password, OnLoginSuccess, OnLoginError);
        }

        private void OnAdminBtnClicked()
        {
            _usernameInputField.text = "admin";
            _passwordInputField.text = "admin";

            OnLoginBtnClicked();
        }

        private void OnLoginSuccess()
        {
            ModuleRegistry.Get<ILocalizationManager>().GetLocalizedText(LocalizationKeys.MainMenuNoticeSuccess, (text) => {
                _noticeTxt.text = text;
            });

            ModuleRegistry.Get<IUIManager>().Show(WindowNames.MainMenu);
        }

        private void OnLoginError(string errorMessage)
        {
            ModuleRegistry.Get<ILocalizationManager>().GetLocalizedTextFormatted(LocalizationKeys.MainMenuNoticeFailed, (text) => {
                _noticeTxt.text = text;
            }, errorMessage);
        }
        #endregion
    }
}