using TMPro;
using YuankunHuang.Unity.Core;

namespace YuankunHuang.Unity.HotUpdate
{
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
        }

        private enum ExtraObj
        {
            UsernameInputField = 0,
            PasswordInputField = 1,
        }

        private TMP_Text _noticeTxt;
        
        private GeneralButton _loginBtn;

        private TMP_InputField _usernameInputField;
        private TMP_InputField _passwordInputField;
        #endregion

        #region Lifecycle
        protected override void OnInit()
        {
            _noticeTxt = Config.ExtraTextMeshProList[(int)ExtraTMP.Notice];

            _loginBtn = Config.ExtraButtonList[(int)ExtraBtn.Login];

            _usernameInputField = Config.ExtraObjectList[(int)ExtraObj.UsernameInputField].GetComponent<TMP_InputField>();
            _passwordInputField = Config.ExtraObjectList[(int)ExtraObj.PasswordInputField].GetComponent<TMP_InputField>();

            _loginBtn.onClick.AddListener(OnLoginBtnClicked);
        }

        protected override void OnShow(IWindowData data, WindowShowState state)
        {
            _usernameInputField.text = string.Empty;
            _passwordInputField.text = string.Empty;
            _noticeTxt.text = string.Empty;
        }

        protected override void OnDispose()
        {
            _loginBtn.onClick.RemoveAllListeners();
        }
        #endregion

        #region Event Handlers
        private void OnLoginBtnClicked()
        {
            string username = _usernameInputField.text.Trim();
            string password = _passwordInputField.text.Trim();
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                _noticeTxt.text = "Username and password cannot be empty.";
                return;
            }

            var accountManager = ModuleRegistry.Get<IAccountManager>();
            accountManager.Login(username, password, OnLoginSuccess, OnLoginError);
        }

        private void OnLoginSuccess()
        {
            _noticeTxt.text = "Login successful!";
            ModuleRegistry.Get<IUIManager>().ShowStackableWindow(WindowNames.MainMenu);
        }

        private void OnLoginError(string errorMessage)
        {
            _noticeTxt.text = $"Login failed: {errorMessage}";
        }
        #endregion
    }
}