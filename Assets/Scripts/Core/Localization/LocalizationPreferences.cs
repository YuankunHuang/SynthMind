using System.Globalization;
using UnityEngine;
using YuankunHuang.Unity.Util;

namespace YuankunHuang.Unity.LocalizationCore
{
    public static class LocalizationPreferences
    {
        private const string LANGUAGE_KEY = "Language";
        private const string DEFAULT_LANGUAGE = "en";

        public static string GetSavedLanguage()
        {
            if (PlayerPrefsUtil.HasKey(LANGUAGE_KEY))
            {
                return PlayerPrefsUtil.GetString(LANGUAGE_KEY);
            }
            
            // If no saved language, detect system locale
            return GetSystemLocale();
        }

        public static string GetSystemLocale()
        {
            try
            {
                switch (Application.systemLanguage)
                {
                    case SystemLanguage.Chinese:
                    case SystemLanguage.ChineseSimplified:
                        return "zh-cn";
                    case SystemLanguage.ChineseTraditional:
                        return "zh-TW";
                    case SystemLanguage.English:
                        return "en";
                    case SystemLanguage.Japanese:
                        return "ja";
                    case SystemLanguage.Korean:
                        return "ko";
                    case SystemLanguage.French:
                        return "fr";
                    case SystemLanguage.German:
                        return "de";
                    case SystemLanguage.Spanish:
                        return "es";
                    case SystemLanguage.Portuguese:
                        return "pt";
                    case SystemLanguage.Russian:
                        return "ru";
                    case SystemLanguage.Italian:
                        return "it";
                    case SystemLanguage.Dutch:
                        return "nl";
                    case SystemLanguage.Arabic:
                        return "ar";
                    case SystemLanguage.Thai:
                        return "th";
                    case SystemLanguage.Vietnamese:
                        return "vi";
                    default:
                        return DEFAULT_LANGUAGE;
                }
            }
            catch
            {
                return DEFAULT_LANGUAGE;
            }
        }

        public static void SaveLanguage(string languageCode)
        {
            PlayerPrefsUtil.TrySetString(LANGUAGE_KEY, languageCode);
        }

        public static void ClearLanguagePreference()
        {
            PlayerPrefsUtil.TryDeleteKey(LANGUAGE_KEY);
        }

        public static bool HasSavedLanguage()
        {
            return PlayerPrefsUtil.HasKey(LANGUAGE_KEY);
        }
    }
}