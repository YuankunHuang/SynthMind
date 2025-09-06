using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using YuankunHuang.Unity.Core;
using YuankunHuang.Unity.LocalizationCore;
using YuankunHuang.Unity.ModuleCore;

namespace YuankunHuang.Unity.Core
{
    public class LocalizationManager : ILocalizationManager
    {
        public static string DefaultLocalizationTable = "Localization";
        
        public bool IsInitialized { get; private set; } = false;
        public event Action<string> OnLanguageChanged;
        public string CurrentLanguage => LocalizationSettings.SelectedLocale?.Identifier.Code ?? "en";

        public async Task InitializeAsync()
        {
            try
            {
                LogHelper.Log("[SimpleLocalizationManager] Initializing...");
                
                await LocalizationSettings.InitializationOperation.Task;
                LocalizationSettings.SelectedLocaleChanged += OnSelectedLocaleChanged;
                
                IsInitialized = true;
                LogHelper.Log($"[SimpleLocalizationManager] Initialized. Current language: {CurrentLanguage}");
            }
            catch (Exception e)
            {
                LogHelper.LogError($"[SimpleLocalizationManager] Failed to initialize: {e.Message}");
                LogHelper.LogException(e);
            }
        }

        public string GetLocalizedText(string key)
        {
            return GetLocalizedText(DefaultLocalizationTable, key);
        }

        public string GetLocalizedText(string table, string key)
        {
            if (!IsInitialized)
            {
                LogHelper.LogWarning($"[SimpleLocalizationManager] Not initialized. Returning key: {key}");
                return key;
            }

            try
            {
                var localizedString = new LocalizedString(table, key);
                var handle = localizedString.GetLocalizedStringAsync();
                
                if (handle.IsDone && handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                {
                    return handle.Result ?? key;
                }
                
                return key;
            }
            catch (Exception e)
            {
                LogHelper.LogWarning($"[SimpleLocalizationManager] Failed to get text for key '{key}': {e.Message}");
                return key;
            }
        }

        public string GetLocalizedTextFormatted(string key, params object[] args)
        {
            return GetLocalizedTextFormatted(DefaultLocalizationTable, key, args);
        }

        public string GetLocalizedTextFormatted(string table, string key, params object[] args)
        {
            var template = GetLocalizedText(table, key);
            
            if (args == null || args.Length == 0)
                return template;

            try
            {
                return string.Format(template, args);
            }
            catch (Exception e)
            {
                LogHelper.LogWarning($"[SimpleLocalizationManager] String format error for key '{key}': {e.Message}");
                return template;
            }
        }

        public void SetLanguage(string langCode)
        {
            MonoManager.Instance.StartCoroutine(SetLanguageCoroutine(langCode));
        }

        public async Task SetLanguageAsync(string langCode)
        {
            try
            {
                LogHelper.Log($"[SimpleLocalizationManager] Changing language to {langCode}");
                
                var targetLocale = LocalizationSettings.AvailableLocales.Locales
                    .FirstOrDefault(locale => locale.Identifier.Code == langCode);
                    
                if (targetLocale == null)
                {
                    LogHelper.LogError($"[SimpleLocalizationManager] Language not found: {langCode}");
                    return;
                }

                LocalizationSettings.SelectedLocale = targetLocale;
                await Task.Yield();
            }
            catch (Exception e)
            {
                LogHelper.LogError($"[SimpleLocalizationManager] Failed to change language to {langCode}: {e.Message}");
                LogHelper.LogException(e);
            }
        }

        public string GetLanguageDisplayName(string langCode)
        {
            if (!IsInitialized)
                return langCode;

            var locale = LocalizationSettings.AvailableLocales.Locales
                .FirstOrDefault(l => l.Identifier.Code == langCode);
                
            return locale?.LocaleName ?? langCode;
        }

        public List<string> GetAvailableLanguages()
        {
            if (!IsInitialized)
            {
                LogHelper.LogWarning("[SimpleLocalizationManager] System not initialized.");
                return new List<string>();
            }

            return LocalizationSettings.AvailableLocales.Locales
                .Select(locale => locale.Identifier.Code)
                .ToList();
        }

        public void Dispose()
        {
            try
            {
                if (LocalizationSettings.InitializationOperation.IsDone)
                {
                    LocalizationSettings.SelectedLocaleChanged -= OnSelectedLocaleChanged;
                }
                
                OnLanguageChanged = null;
                IsInitialized = false;
                
                LogHelper.Log("[SimpleLocalizationManager] Disposed");
            }
            catch (Exception e)
            {
                LogHelper.LogError($"[SimpleLocalizationManager] Error during disposal: {e.Message}");
                LogHelper.LogException(e);
            }
        }

        #region Private Methods

        private System.Collections.IEnumerator SetLanguageCoroutine(string langCode)
        {
            var task = SetLanguageAsync(langCode);
            yield return new WaitUntil(() => task.IsCompleted);
        }

        private void OnSelectedLocaleChanged(Locale locale)
        {
            if (locale != null)
            {
                var newLanguage = locale.Identifier.Code;
                OnLanguageChanged?.Invoke(newLanguage);
                LogHelper.Log($"[SimpleLocalizationManager] Language changed to {newLanguage}");
            }
        }

        #endregion
    }
}