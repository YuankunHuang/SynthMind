using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using YuankunHuang.Unity.ModuleCore;

namespace YuankunHuang.Unity.LocalizationCore
{
    public interface ILocalizationManager : IModule
    {
        // Callback-based methods for cross-platform compatibility
        void GetLocalizedText(string key, Action<string> callback);
        void GetLocalizedText(string table, string key, Action<string> callback);
        void GetLocalizedTextFormatted(string key, Action<string> callback, params object[] args);
        void GetLocalizedTextFormatted(string table, string key, Action<string> callback, params object[] args);

        // Batch methods for multiple keys
        void GetLocalizedTexts(string[] keys, Action<Dictionary<string, string>> callback);
        void GetLocalizedTexts(string table, string[] keys, Action<Dictionary<string, string>> callback);

        // Management
        Task InitializeAsync();
        void SetLanguage(string langCode);
        Task SetLanguageAsync(string langCode);
        string GetLanguageDisplayName(string langCode);
        List<string> GetAvailableLanguages();

        // State and events
        string CurrentLanguage { get; }
        event Action<string> OnLanguageChanged;
    }
}