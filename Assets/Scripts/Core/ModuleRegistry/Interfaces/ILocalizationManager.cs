using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using YuankunHuang.Unity.ModuleCore;

namespace YuankunHuang.Unity.LocalizationCore
{
    public interface ILocalizationManager : IModule
    {
        // legacy synchronous methods (deprecated for UI usage)
        string GetLocalizedText(string key);
        string GetLocalizedText(string table, string key);
        string GetLocalizedTextFormatted(string key, params object[] args);
        string GetLocalizedTextFormatted(string table, string key, params object[] args);

        // unified callback-based methods (recommended for UI)
        void GetLocalizedText(string key, Action<string> callback);
        void GetLocalizedText(string table, string key, Action<string> callback);
        void GetLocalizedTextFormatted(string key, Action<string> callback, params object[] args);
        void GetLocalizedTextFormatted(string table, string key, Action<string> callback, params object[] args);

        // batch methods for multiple keys
        void GetLocalizedTexts(string[] keys, Action<Dictionary<string, string>> callback);
        void GetLocalizedTexts(string table, string[] keys, Action<Dictionary<string, string>> callback);

        // management
        Task InitializeAsync();
        void SetLanguage(string langCode);
        Task SetLanguageAsync(string langCode);
        string GetLanguageDisplayName(string langCode);
        List<string> GetAvailableLanguages();

        // state + event
        string CurrentLanguage { get; }
        event Action<string> OnLanguageChanged;
    }
}