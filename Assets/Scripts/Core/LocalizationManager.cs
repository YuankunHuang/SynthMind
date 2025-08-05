using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace YuankunHuang.Unity.Core
{
    public class LocalizationManager : ILocalizationManager
    {
        public static string DefaultLocalizationTable = "Localization";

        public event Action<string> OnLanguageChanged;
        public string CurrentLanguage => _currentLanguage;
        public bool IsInitialized => _isInitialized;

        private bool _isInitialized;
        private string _currentLanguage = "en";

        // cache
        private Dictionary<string, string> _stringCache = new();
        private Dictionary<string, LocalizedString> _locStringCache = new();

        public async Task InitializeAsync()
        {
            try
            {
                LogHelper.Log($"[LocalizationManager] Initializing localization system...");
                await LocalizationSettings.InitializationOperation.Task;

                if (LocalizationSettings.SelectedLocale != null)
                {
                    _currentLanguage = LocalizationSettings.SelectedLocale.Identifier.Code;
                }

                LocalizationSettings.SelectedLocaleChanged += OnSelectedLocaleChanged;

                _isInitialized = true;
                LogHelper.Log($"[LocalizationManager] Initialized successfully. Current language: {_currentLanguage}");
            }
            catch (Exception e)
            {
                LogHelper.LogError($"[LocalizationManager] Failed to initialize: {e.Message}");
                LogHelper.LogException(e);
            }
        }

        public async Task<string> GetLocalizedText(string key)
        {
            return await GetLocalizedText(DefaultLocalizationTable, key);
        }

        public async Task<string> GetLocalizedText(string table, string key)
        {
            if (!_isInitialized)
            {
                LogHelper.LogError($"[LocalizationManager] System not initialized when trying to get a localized string. Attempting to initialize...");
                await InitializeAsync();
            }

            try
            {
                // check cache
                var locKey = $"{table}/{key}";
                if (!_locStringCache.TryGetValue(locKey, out var locString))
                {
                    locString = new LocalizedString(table, key);
                    _locStringCache[locKey] = locString;
                }

                var handle = locString.GetLocalizedStringAsync();
                await handle.Task;

                if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                {
                    var result = handle.Result;
                    _stringCache[locKey] = result;
                }
                else
                {
                    LogHelper.LogError($"[LocalizationManager] Failed to get localized text for key {key} in table {table}");
                    if (handle.OperationException != null)
                    {
                        LogHelper.LogException(handle.OperationException);
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.LogException(e);
            }

            return $"[MISSING: {table}/{key}]";
        }

        public async Task<string> GetLocalizedTextFormatted(string key, params object[] args)
        {
            var template = await GetLocalizedText(key);
            try
            {
                return string.Format(template, args);
            }
            catch (Exception e)
            {
                LogHelper.LogException(e);
                return template;
            }
        }

        public async Task<string> GetLocalizedTextFormatted(string table, string key, params object[] args)
        {
            var template = await GetLocalizedText(table, key);
            try
            {
                return string.Format(template, args);
            }
            catch (Exception e)
            {
                LogHelper.LogException(e);
                return template;
            }
        }

        public void SetLanguage(string langCode)
        {
            MonoManager.Instance.StartCoroutine(SetLanguageCoroutine(langCode));
        }

        private IEnumerator SetLanguageCoroutine(string langCode)
        {
            var task = SetLanguageAsync(langCode);
            yield return new WaitUntil(() => task.IsCompleted);
        }

        public async Task SetLanguageAsync(string langCode)
        {
            try
            {
                LogHelper.Log($"[LocalizationManager] Changing Language to {langCode}");
                var targetLocale = LocalizationSettings.AvailableLocales.Locales.FirstOrDefault(locale => locale.Identifier.Code == langCode);
                if (targetLocale == null)
                {
                    LogHelper.LogError($"[LocalizationManager] Language not found: {langCode}");
                    return;
                }

                LocalizationSettings.SelectedLocale = targetLocale;

                await Task.Yield();

                ClearCache();
            }
            catch (Exception e)
            {
                LogHelper.LogError($"[LocalizationManager] Failed to change language to {langCode}");
                LogHelper.LogException(e);
            }
        }

        private void OnSelectedLocaleChanged(Locale locale)
        {
            if (locale != null)
            {
                var newLanguage = locale.Identifier.Code;
                if (_currentLanguage != newLanguage)
                {
                    var oldLanguage = _currentLanguage;
                    _currentLanguage = newLanguage;

                    ClearCache();

                    OnLanguageChanged?.Invoke(newLanguage);

                    LogHelper.Log($"[LocalizationManager] Language changed from {oldLanguage} to {newLanguage}");
                }
            }
        }

        public void ForceRefresh()
        {
            LogHelper.Log($"[LocalizationManager] Can only force refresh by restarting the game.");

            GameManager.Restart();
        }

        public string GetLanguageDisplayName(string langCode)
        {
            if (!_isInitialized)
            {
                return langCode;
            }

            var locale = LocalizationSettings.AvailableLocales.Locales.FirstOrDefault(l => l.Identifier.Code == langCode);
            if (locale != null)
            {
                return locale.LocaleName;
            }
            return langCode;
        }

        public List<string> GetAvailableLanguages()
        {
            if (!_isInitialized)
            {
                LogHelper.LogError($"[LocalizationManager] System is not initialized.");
                return new List<string>();
            }

            return LocalizationSettings.AvailableLocales.Locales.Select(locale => locale.Identifier.Code).ToList();
        }

        private void ClearCache()
        {
            _stringCache.Clear();
            _locStringCache.Clear();
        }

        public void Dispose()
        {
            try
            {
                LocalizationSettings.SelectedLocaleChanged -= OnSelectedLocaleChanged;

                OnLanguageChanged = null;

                ClearCache();

                _isInitialized = false;

                LogHelper.Log($"[LocalizationManager] Disposed");
            }
            catch (Exception e)
            {
                LogHelper.LogError($"[LocalizationManager] Error during disposal");
                LogHelper.LogException(e);
            }
        }
    }
}