// Auto-generated LocalizationKeys
using System.Collections.Generic;
using System.Reflection;

namespace YuankunHuang.Unity.Core
{
    public static class LocalizationKeys
    {
        public const string CommonCancel = "Common_Cancel";
        public const string CommonConfirm = "Common_Confirm";
        public const string CommonHigh = "Common_High";
        public const string CommonLow = "Common_Low";
        public const string CommonMid = "Common_Mid";
        public const string FullScreenModeExclusiveFullScreen = "FullScreenMode_ExclusiveFullScreen";
        public const string FullScreenModeFullScreenWindow = "FullScreenMode_FullScreenWindow";
        public const string FullScreenModeMaximizedWindow = "FullScreenMode_MaximizedWindow";
        public const string FullScreenModeWindowed = "FullScreenMode_Windowed";
        public const string GraphicFPSLimitFPS30 = "GraphicFPSLimit_FPS_30";
        public const string GraphicFPSLimitFPS60 = "GraphicFPSLimit_FPS_60";
        public const string GraphicFPSLimitFPSDefault = "GraphicFPSLimit_FPS_Default";
        public const string GraphicVSyncEveryFrame = "GraphicVSync_EveryFrame";
        public const string GraphicVSyncEveryTwoFrames = "GraphicVSync_EveryTwoFrames";
        public const string GraphicVSyncOff = "GraphicVSync_Off";
        public const string MainMenuAbout = "MainMenu_About";
        public const string MainMenuAboutContent = "MainMenu_About_Content";
        public const string MainMenuAdmin = "MainMenu_Admin";
        public const string MainMenuChat = "MainMenu_Chat";
        public const string MainMenuHome = "MainMenu_Home";
        public const string MainMenuHomeIntro = "MainMenu_Home_Intro";
        public const string MainMenuInputPlaceholder = "MainMenu_InputPlaceholder";
        public const string MainMenuLogin = "MainMenu_Login";
        public const string MainMenuLoginEnterPassword = "MainMenu_Login_EnterPassword";
        public const string MainMenuLoginEnterUsername = "MainMenu_Login_EnterUsername";
        public const string MainMenuNoticeEmptyUsernameOrPassword = "MainMenu_Notice_EmptyUsernameOrPassword";
        public const string MainMenuNoticeFailed = "MainMenu_Notice_Failed";
        public const string MainMenuNoticeSuccess = "MainMenu_Notice_Success";
        public const string MainMenuPassword = "MainMenu_Password";
        public const string MainMenuSandBox = "MainMenu_SandBox";
        public const string MainMenuSetting = "MainMenu_Setting";
        public const string MainMenuSettingBGMVolume = "MainMenu_Setting_BGMVolume";
        public const string MainMenuSettingFPSLimit = "MainMenu_Setting_FPSLimit";
        public const string MainMenuSettingFullScreenMode = "MainMenu_Setting_FullScreenMode";
        public const string MainMenuSettingGraphicsTitle = "MainMenu_Setting_GraphicsTitle";
        public const string MainMenuSettingLanguageTitle = "MainMenu_Setting_LanguageTitle";
        public const string MainMenuSettingMasterMuted = "MainMenu_Setting_MasterMuted";
        public const string MainMenuSettingMasterVolume = "MainMenu_Setting_MasterVolume";
        public const string MainMenuSettingQualityPreset = "MainMenu_Setting_QualityPreset";
        public const string MainMenuSettingResolution = "MainMenu_Setting_Resolution";
        public const string MainMenuSettingSFXVolume = "MainMenu_Setting_SFXVolume";
        public const string MainMenuSettingSoundTitle = "MainMenu_Setting_SoundTitle";
        public const string MainMenuSettingVSync = "MainMenu_Setting_VSync";
        public const string MainMenuTitle = "MainMenu_Title";
        public const string MainMenuUsername = "MainMenu_Username";
        public const string ProfileContent = "Profile_Content";
        public const string ProfileTitle = "Profile_Title";
        public const string QuitGameContent = "QuitGame_Content";
        public const string QuitGameTitle = "QuitGame_Title";

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
