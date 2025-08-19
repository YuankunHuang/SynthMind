using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using YuankunHuang.Unity.ModuleCore;

namespace YuankunHuang.Unity.LocalizationCore
{
    public interface ILocalizationManager : IModule
    {
        // core
        string GetLocalizedText(string key);
        string GetLocalizedText(string table, string key);
        string GetLocalizedTextFormatted(string key, params object[] args);
        string GetLocalizedTextFormatted(string table, string key, params object[] args);

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