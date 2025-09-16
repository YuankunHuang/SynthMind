using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
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

        private readonly Dictionary<string, string> _textCache = new Dictionary<string, string>();

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

        // Callback-based methods for cross-platform compatibility
        public void GetLocalizedText(string key, Action<string> callback)
        {
            GetLocalizedText(DefaultLocalizationTable, key, callback);
        }

        public void GetLocalizedText(string table, string key, Action<string> callback)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            // WebGL: always use async
            GetLocalizedTextAsync(table, key, callback);
#else
            // Other platforms: use sync loading and call callback immediately
            try
            {
                if (!IsInitialized)
                {
                    LogHelper.LogWarning($"[LocalizationManager] Not initialized. Returning key: {key}");
                    callback?.Invoke(key);
                    return;
                }

                var cacheKey = $"{table}:{key}";
                if (_textCache.ContainsKey(cacheKey))
                {
                    callback?.Invoke(_textCache[cacheKey]);
                    return;
                }

                var stringTableCollection = LocalizationSettings.StringDatabase.GetTable(table);
                if (stringTableCollection == null)
                {
                    LogHelper.LogWarning($"[LocalizationManager] Table '{table}' not found");
                    callback?.Invoke(key);
                    return;
                }

                var entry = stringTableCollection.GetEntry(key);
                if (entry == null)
                {
                    LogHelper.LogWarning($"[LocalizationManager] Key '{key}' not found in table '{table}'");
                    callback?.Invoke(key);
                    return;
                }

                var localizedText = entry.GetLocalizedString() ?? key;
                _textCache[cacheKey] = localizedText;
                callback?.Invoke(localizedText);
            }
            catch (Exception e)
            {
                LogHelper.LogWarning($"[LocalizationManager] Failed to get text for key '{key}': {e.Message}");
                callback?.Invoke(key);
            }
#endif
        }

        public void GetLocalizedTextFormatted(string key, Action<string> callback, params object[] args)
        {
            GetLocalizedTextFormatted(DefaultLocalizationTable, key, callback, args);
        }

        public void GetLocalizedTextFormatted(string table, string key, Action<string> callback, params object[] args)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            // WebGL: get text async then format
            GetLocalizedTextAsync(table, key, (text) =>
            {
                try
                {
                    var formatted = string.Format(text, args);
                    callback?.Invoke(formatted);
                }
                catch (Exception e)
                {
                    LogHelper.LogWarning($"[LocalizationManager] Failed to format text '{text}': {e.Message}");
                    callback?.Invoke(text);
                }
            });
#else
            // Other platforms: get text sync then format
            GetLocalizedText(table, key, (text) =>
            {
                try
                {
                    if (args == null || args.Length == 0)
                    {
                        callback?.Invoke(text);
                        return;
                    }

                    var formatted = string.Format(text, args);
                    callback?.Invoke(formatted);
                }
                catch (Exception e)
                {
                    LogHelper.LogWarning($"[LocalizationManager] Failed to format text '{text}': {e.Message}");
                    callback?.Invoke(text);
                }
            });
#endif
        }

        // Batch methods for multiple keys
        public void GetLocalizedTexts(string[] keys, Action<Dictionary<string, string>> callback)
        {
            GetLocalizedTexts(DefaultLocalizationTable, keys, callback);
        }

        public void GetLocalizedTexts(string table, string[] keys, Action<Dictionary<string, string>> callback)
        {
            if (keys == null || keys.Length == 0)
            {
                callback?.Invoke(new Dictionary<string, string>());
                return;
            }

#if UNITY_WEBGL && !UNITY_EDITOR
            // WebGL: use async for all keys
            var results = new Dictionary<string, string>();
            int completedCount = 0;
            int totalCount = keys.Length;

            foreach (var key in keys)
            {
                GetLocalizedTextAsync(table, key, (text) =>
                {
                    lock (results)
                    {
                        results[key] = text;
                        completedCount++;

                        if (completedCount == totalCount)
                        {
                            callback?.Invoke(results);
                        }
                    }
                });
            }
#else
            // Other platforms: use sync loading for all keys
            var results = new Dictionary<string, string>();

            foreach (var key in keys)
            {
                try
                {
                    if (!IsInitialized)
                    {
                        results[key] = key;
                        continue;
                    }

                    var cacheKey = $"{table}:{key}";
                    if (_textCache.ContainsKey(cacheKey))
                    {
                        results[key] = _textCache[cacheKey];
                        continue;
                    }

                    var stringTableCollection = LocalizationSettings.StringDatabase.GetTable(table);
                    if (stringTableCollection == null)
                    {
                        results[key] = key;
                        continue;
                    }

                    var entry = stringTableCollection.GetEntry(key);
                    if (entry == null)
                    {
                        results[key] = key;
                        continue;
                    }

                    var localizedText = entry.GetLocalizedString() ?? key;
                    _textCache[cacheKey] = localizedText;
                    results[key] = localizedText;
                }
                catch (Exception e)
                {
                    LogHelper.LogWarning($"[LocalizationManager] Failed to get text for key '{key}': {e.Message}");
                    results[key] = key;
                }
            }

            callback?.Invoke(results);
#endif
        }

        // Internal async implementation for WebGL platform
        private async void GetLocalizedTextAsync(string table, string key, System.Action<string> callback)
        {
            if (!IsInitialized)
            {
                LogHelper.LogWarning($"[LocalizationManager] Not initialized. Returning key: {key}");
                callback?.Invoke(key);
                return;
            }

            var cacheKey = $"{table}:{key}";

            // Check cache first
            if (_textCache.ContainsKey(cacheKey))
            {
                callback?.Invoke(_textCache[cacheKey]);
                return;
            }

            try
            {
                // Use async loading for WebGL compatibility
                var stringOperation = LocalizationSettings.StringDatabase.GetLocalizedStringAsync(table, key);
                await stringOperation.Task;

                if (stringOperation.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                {
                    var localizedText = stringOperation.Result ?? key;
                    _textCache[cacheKey] = localizedText;
                    callback?.Invoke(localizedText);
                }
                else
                {
                    LogHelper.LogWarning($"[LocalizationManager] Failed to get localized string for key '{key}' in table '{table}'");
                    callback?.Invoke(key);
                }
            }
            catch (Exception e)
            {
                LogHelper.LogWarning($"[LocalizationManager] Failed to get text for key '{key}': {e.Message}");
                callback?.Invoke(key);
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

                // Clear cache when language changes
                _textCache.Clear();
                LogHelper.Log($"[LocalizationManager] Cache cleared due to language change");

                OnLanguageChanged?.Invoke(newLanguage);
                LogHelper.Log($"[LocalizationManager] Language changed to {newLanguage}");
            }
        }

        #endregion
    }
}