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
        private enum Mode
        {
            Login,
            Register
        }

        #region UI Ref
        private enum ExtraTMP
        {
            Notice = 0,
            Title = 1,
        }

        private enum ExtraBtn
        {
            Login = 0,
            Quit = 1,
            Admin = 2,
            Register = 3,
            ToggleMode = 4,
        }

        private enum ExtraObj
        {
            EmailInputField = 0,
            PasswordInputField = 1,
            DisplayNameInputField = 2,
        }

        private TMP_Text _noticeTxt;
        private TMP_Text _titleTxt;

        private GeneralButton _loginBtn;
        private GeneralButton _quitBtn;
        private GeneralButton _adminBtn;
        private GeneralButton _registerBtn;
        private GeneralButton _toggleModeBtn;

        private TMP_InputField _emailInputField;
        private TMP_InputField _passwordInputField;
        private TMP_InputField _displayNameInputField;

        private Mode _currentMode = Mode.Login;
        #endregion

        #region Lifecycle
        protected override void OnInit()
        {
            _noticeTxt = Config.ExtraTextMeshProList[(int)ExtraTMP.Notice];
            _titleTxt = Config.ExtraTextMeshProList.Count > (int)ExtraTMP.Title ?
                       Config.ExtraTextMeshProList[(int)ExtraTMP.Title] : null;

            _loginBtn = Config.ExtraButtonList[(int)ExtraBtn.Login];
            _quitBtn = Config.ExtraButtonList[(int)ExtraBtn.Quit];
            _adminBtn = Config.ExtraButtonList[(int)ExtraBtn.Admin];
            _registerBtn = Config.ExtraButtonList.Count > (int)ExtraBtn.Register ?
                          Config.ExtraButtonList[(int)ExtraBtn.Register] : null;
            _toggleModeBtn = Config.ExtraButtonList.Count > (int)ExtraBtn.ToggleMode ?
                           Config.ExtraButtonList[(int)ExtraBtn.ToggleMode] : null;

            _emailInputField = Config.ExtraObjectList[(int)ExtraObj.EmailInputField].GetComponent<TMP_InputField>();
            _passwordInputField = Config.ExtraObjectList[(int)ExtraObj.PasswordInputField].GetComponent<TMP_InputField>();
            _displayNameInputField = Config.ExtraObjectList.Count > (int)ExtraObj.DisplayNameInputField ?
                                    Config.ExtraObjectList[(int)ExtraObj.DisplayNameInputField].GetComponent<TMP_InputField>() : null;

            _loginBtn.onClick.AddListener(OnLoginBtnClicked);
            _quitBtn.onClick.AddListener(OnQuitBtnClicked);
            _adminBtn.onClick.AddListener(OnAdminBtnClicked);

            if (_registerBtn != null)
                _registerBtn.onClick.AddListener(OnRegisterBtnClicked);

            if (_toggleModeBtn != null)
                _toggleModeBtn.onClick.AddListener(OnToggleModeBtnClicked);

            // Initialize UI for login mode
            UpdateUIForCurrentMode();
        }

        protected override void OnShow(IWindowData data, WindowShowState state)
        {
            if (state == WindowShowState.New)
            {
                ClearInputFields();
                _noticeTxt.text = string.Empty;

                ModuleRegistry.Get<IAudioManager>().PlayBGMAsync(GameDataConfig.AudioIdType.TestBGM);
            }

            var windowData = (LoginWindowData)data;
            if (windowData != null && windowData.Reset)
            {
                ClearInputFields();
                _noticeTxt.text = string.Empty;
            }

            UpdateUIForCurrentMode();
        }

        protected override void OnDispose()
        {
            _loginBtn.onClick.RemoveAllListeners();
            _quitBtn.onClick.RemoveAllListeners();
            _adminBtn.onClick.RemoveAllListeners();

            if (_registerBtn != null)
                _registerBtn.onClick.RemoveAllListeners();

            if (_toggleModeBtn != null)
                _toggleModeBtn.onClick.RemoveAllListeners();
        }

        private void ClearInputFields()
        {
            _emailInputField.text = string.Empty;
            _passwordInputField.text = string.Empty;
            if (_displayNameInputField != null)
                _displayNameInputField.text = string.Empty;
        }

        private void UpdateUIForCurrentMode()
        {
            var locManager = ModuleRegistry.Get<ILocalizationManager>();

            if (_currentMode == Mode.Login)
            {
                // Update title and button texts for login mode
                if (_titleTxt != null)
                    locManager.GetLocalizedText("login_title", (text) => _titleTxt.text = text);

                // Show/hide appropriate buttons
                _loginBtn.gameObject.SetActive(true);
                if (_registerBtn != null)
                    _registerBtn.gameObject.SetActive(false);

                // Hide display name field in login mode
                if (_displayNameInputField != null)
                    _displayNameInputField.gameObject.SetActive(false);

                // Update toggle button text
                if (_toggleModeBtn != null)
                    locManager.GetLocalizedText("switch_to_register", (text) => _toggleModeBtn.GetComponentInChildren<TMP_Text>().text = text);
            }
            else // Register mode
            {
                // Update title and button texts for register mode
                if (_titleTxt != null)
                    locManager.GetLocalizedText("register_title", (text) => _titleTxt.text = text);

                // Show/hide appropriate buttons
                _loginBtn.gameObject.SetActive(false);
                if (_registerBtn != null)
                    _registerBtn.gameObject.SetActive(true);

                // Show display name field in register mode
                if (_displayNameInputField != null)
                    _displayNameInputField.gameObject.SetActive(true);

                // Update toggle button text
                if (_toggleModeBtn != null)
                    locManager.GetLocalizedText("switch_to_login", (text) => _toggleModeBtn.GetComponentInChildren<TMP_Text>().text = text);
            }
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

            string email = _emailInputField.text.Trim();
            string password = _passwordInputField.text.Trim();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ModuleRegistry.Get<ILocalizationManager>().GetLocalizedText("empty_email_password", (text) => {
                    _noticeTxt.text = text ?? "Please fill in both email and password";
                });
                return;
            }

            if (!IsValidEmail(email))
            {
                ModuleRegistry.Get<ILocalizationManager>().GetLocalizedText("invalid_email", (text) => {
                    _noticeTxt.text = text ?? "Please enter a valid email address";
                });
                return;
            }

            var accountManager = ModuleRegistry.Get<IAccountManager>();
            accountManager.Login(email, password, OnLoginSuccess, OnLoginError);
        }

        private void OnRegisterBtnClicked()
        {
            ModuleRegistry.Get<IAudioManager>().PlayUI(GameDataConfig.AudioIdType.TestButtonClick);

            string email = _emailInputField.text.Trim();
            string password = _passwordInputField.text.Trim();
            string displayName = _displayNameInputField != null ? _displayNameInputField.text.Trim() : string.Empty;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ModuleRegistry.Get<ILocalizationManager>().GetLocalizedText("empty_email_password", (text) => {
                    _noticeTxt.text = text ?? "Please fill in both email and password";
                });
                return;
            }

            if (!IsValidEmail(email))
            {
                ModuleRegistry.Get<ILocalizationManager>().GetLocalizedText("invalid_email", (text) => {
                    _noticeTxt.text = text ?? "Please enter a valid email address";
                });
                return;
            }

            if (password.Length < 6)
            {
                ModuleRegistry.Get<ILocalizationManager>().GetLocalizedText("password_too_short", (text) => {
                    _noticeTxt.text = text ?? "Password must be at least 6 characters long";
                });
                return;
            }

            if (string.IsNullOrEmpty(displayName))
            {
                displayName = email.Split('@')[0]; // Use email prefix as default display name
            }

            var accountManager = ModuleRegistry.Get<IAccountManager>();
            accountManager.Register(email, password, displayName, OnRegisterSuccess, OnRegisterError);
        }

        private void OnToggleModeBtnClicked()
        {
            ModuleRegistry.Get<IAudioManager>().PlayUI(GameDataConfig.AudioIdType.TestButtonClick);

            _currentMode = _currentMode == Mode.Login ? Mode.Register : Mode.Login;
            ClearInputFields();
            _noticeTxt.text = string.Empty;
            UpdateUIForCurrentMode();
        }

        private void OnAdminBtnClicked()
        {
            _emailInputField.text = "admin@synthmind.com";
            _passwordInputField.text = "admin123";

            if (_currentMode == Mode.Register)
            {
                OnToggleModeBtnClicked(); // Switch to login mode
            }

            OnLoginBtnClicked();
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private void OnLoginSuccess()
        {
            ModuleRegistry.Get<ILocalizationManager>().GetLocalizedText(LocalizationKeys.MainMenuNoticeSuccess, (text) => {
                _noticeTxt.text = text;
            });

            // Delay navigation to show success message
            MonoManager.Instance.StartCoroutine(DelayedNavigation());
        }

        private void OnLoginError(string errorMessage)
        {
            ModuleRegistry.Get<ILocalizationManager>().GetLocalizedText(LocalizationKeys.MainMenuNoticeFailed, (text) => {
                _noticeTxt.text = text;
            });
        }

        private void OnRegisterSuccess()
        {
            ModuleRegistry.Get<ILocalizationManager>().GetLocalizedText("register_success", (text) => {
                _noticeTxt.text = text ?? "Registration successful! Please check your email for verification.";
            });

            // Delay navigation to show success message
            MonoManager.Instance.StartCoroutine(DelayedNavigation());
        }

        private void OnRegisterError(string errorMessage)
        {
            ModuleRegistry.Get<ILocalizationManager>().GetLocalizedText("register_failed", (text) => {
                _noticeTxt.text = $"{text ?? "Registration failed"}: {errorMessage}";
            });
        }

        private System.Collections.IEnumerator DelayedNavigation()
        {
            yield return new UnityEngine.WaitForSeconds(1.5f);
            ModuleRegistry.Get<IUIManager>().Show(WindowNames.MainMenu);
        }
        #endregion
    }
}