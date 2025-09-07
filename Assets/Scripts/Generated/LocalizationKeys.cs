// Auto-generated LocalizationKeys
using System.Collections.Generic;
using System.Reflection;

namespace YuankunHuang.Unity.Core
{
    public static class LocalizationKeys
    {
        public const string Main = "Main";
        public const string MainMenuAbout = "MainMenu_About";
        public const string MainMenuAboutContent = "MainMenu_About_Content";
        public const string MainMenuChat = "MainMenu_Chat";
        public const string MainMenuHome = "MainMenu_Home";
        public const string MainMenuHomeIntro = "MainMenu_Home_Intro";
        public const string MainMenuLogin = "MainMenu_Login";
        public const string MainMenuNoticeEmptyUsernameOrPassword = "MainMenu_Notice_EmptyUsernameOrPassword";
        public const string MainMenuNoticeFailed = "MainMenu_Notice_Failed";
        public const string MainMenuNoticeSuccess = "MainMenu_Notice_Success";
        public const string MainMenuPassword = "MainMenu_Password";
        public const string MainMenuSandBox = "MainMenu_SandBox";
        public const string MainMenuSetting = "MainMenu_Setting";
        public const string MainMenuSettingLanguageTitle = "MainMenu_Setting_LanguageTitle";
        public const string MainMenuTitle = "MainMenu_Title";
        public const string MainMenuUsername = "MainMenu_Username";

        private static List<string> _allKeys;

        public static List<string> GetAllKeys()
        {
            if (_allKeys == null)
            {
                _allKeys = new List<string>();
                var fields = typeof(LocalizationKeys).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                
                foreach (var field in fields)
                {
                    if (field.IsLiteral && !field.IsInitOnly && field.FieldType == typeof(string))
                    {
                        _allKeys.Add((string)field.GetValue(null));
                    }
                }
            }
            
            return _allKeys;
        }
    }
}
