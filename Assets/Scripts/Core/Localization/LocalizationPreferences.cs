using YuankunHuang.Unity.Util;

namespace YuankunHuang.Unity.LocalizationCore
{
    public static class LocalizationPreferences
    {
        private const string LANGUAGE_KEY = "Language";
        private const string DEFAULT_LANGUAGE = "en";

        public static string GetSavedLanguage()
        {
            return PlayerPrefsUtil.HasKey(LANGUAGE_KEY) 
                ? PlayerPrefsUtil.GetString(LANGUAGE_KEY) 
                : DEFAULT_LANGUAGE;
        }

        public static void SaveLanguage(string languageCode)
        {
            PlayerPrefsUtil.TrySetString(LANGUAGE_KEY, languageCode);
        }

        public static void ClearLanguagePreference()
        {
            PlayerPrefsUtil.TryDeleteKey(LANGUAGE_KEY);
        }
    }
}