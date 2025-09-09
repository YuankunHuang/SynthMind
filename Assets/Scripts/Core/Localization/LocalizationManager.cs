using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using YuankunHuang.Unity.Core;
using YuankunHuang.Unity.Core.Debug;

namespace YuankunHuang.Unity.LocalizationCore
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
                LogHelper.Log("[LocalizationManager] Initializing...");
                
                await LocalizationSettings.InitializationOperation.Task;
                LocalizationSettings.SelectedLocaleChanged += OnSelectedLocaleChanged;
                
                LoadSavedLanguage();
                
                IsInitialized = true;
                LogHelper.Log($"[LocalizationManager] Initialized. Current language: {CurrentLanguage}");
            }
            catch (Exception e)
            {
                LogHelper.LogError($"[LocalizationManager] Failed to initialize: {e.Message}");
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
                LogHelper.LogWarning($"[LocalizationManager] Not initialized. Returning key: {key}");
                return key;
            }

            try
            {
                var stringTableCollection = LocalizationSettings.StringDatabase.GetTable(table);
                if (stringTableCollection == null)
                {
                    LogHelper.LogWarning($"[LocalizationManager] Table '{table}' not found");
                    return key;
                }

                var entry = stringTableCollection.GetEntry(key);
                if (entry == null)
                {
                    LogHelper.LogWarning($"[LocalizationManager] Key '{key}' not found in table '{table}'");
                    return key;
                }

                return entry.GetLocalizedString() ?? key;
            }
            catch (Exception e)
            {
                LogHelper.LogWarning($"[LocalizationManager] Failed to get text for key '{key}': {e.Message}");
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
                LogHelper.LogWarning($"[LocalizationManager] String format error for key '{key}': {e.Message}");
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
                LogHelper.Log($"[LocalizationManager] Changing language to {langCode}");
                
                var targetLocale = LocalizationSettings.AvailableLocales.Locales
                    .FirstOrDefault(locale => locale.Identifier.Code == langCode);
                    
                if (targetLocale == null)
                {
                    LogHelper.LogError($"[LocalizationManager] Language not found: {langCode}");
                    return;
                }

                LocalizationSettings.SelectedLocale = targetLocale;
                LocalizationPreferences.SaveLanguage(langCode);
                
                await Task.Yield();
            }
            catch (Exception e)
            {
                LogHelper.LogError($"[LocalizationManager] Failed to change language to {langCode}: {e.Message}");
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
                LogHelper.LogWarning("[LocalizationManager] System not initialized.");
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
                
                LogHelper.Log("[LocalizationManager] Disposed");
            }
            catch (Exception e)
            {
                LogHelper.LogError($"[LocalizationManager] Error during disposal: {e.Message}");
                LogHelper.LogException(e);
            }
        }

        #region Private Methods

        private System.Collections.IEnumerator SetLanguageCoroutine(string langCode)
        {
            var task = SetLanguageAsync(langCode).WithLogging();
            yield return new WaitUntil(() => task.IsCompleted);
        }

        private void LoadSavedLanguage()
        {
            try
            {
                var savedLanguage = LocalizationPreferences.GetSavedLanguage();
                LogHelper.Log($"[LocalizationManager] Loading language: {savedLanguage}");
                
                var targetLocale = LocalizationSettings.AvailableLocales.Locales
                    .FirstOrDefault(locale => locale.Identifier.Code == savedLanguage);
                
                if (targetLocale != null)
                {
                    LocalizationSettings.SelectedLocale = targetLocale;
                    LogHelper.Log($"[LocalizationManager] Language set to: {savedLanguage}");
                }
                else
                {
                    LogHelper.Log($"[LocalizationManager] Language '{savedLanguage}' not available, falling back to English");
                    
                    // Try to find English locale
                    var englishLocale = LocalizationSettings.AvailableLocales.Locales
                        .FirstOrDefault(locale => locale.Identifier.Code == "en");
                    
                    if (englishLocale != null)
                    {
                        LocalizationSettings.SelectedLocale = englishLocale;
                        LogHelper.Log($"[LocalizationManager] Fallback to English successful");
                    }
                    else
                    {
                        LogHelper.LogWarning($"[LocalizationManager] English locale not found, using first available locale");
                        if (LocalizationSettings.AvailableLocales.Locales.Count > 0)
                        {
                            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[0];
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.LogError($"[LocalizationManager] Failed to load language: {e.Message}");
            }
        }

        private void OnSelectedLocaleChanged(Locale locale)
        {
            if (locale != null)
            {
                var newLanguage = locale.Identifier.Code;
                OnLanguageChanged?.Invoke(newLanguage);
                LogHelper.Log($"[LocalizationManager] Language changed to {newLanguage}");
            }
        }

        #endregion
    }
}