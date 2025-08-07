using System;
using System.Collections;
using System.Collections.Concurrent;
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

        private static readonly int MaxCacheSize = 1000;

        // Cache system
        private readonly ConcurrentDictionary<string, Task> _loadingTasks = new();
        private readonly LRUCache<string, string> _stringCache = new(MaxCacheSize);
        private readonly ConcurrentDictionary<string, LocalizedString> _locStringCache = new();
        private readonly object _cacheLock = new();

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

        public string GetLocalizedText(string key)
        {
            return GetLocalizedText(DefaultLocalizationTable, key);
        }

        public string GetLocalizedText(string table, string key)
        {
            var cacheKey = $"{table}_{key}";

            // Check cache first
            lock (_cacheLock)
            {
                if (_stringCache.TryGet(cacheKey, out var cachedValue))
                {
                    return cachedValue;
                }
            }

            // Start async loading in background
            _ = LoadTextAsync(table, key);

            // Return placeholder immediately
            return GetPlaceholder(key);
        }

        public string GetLocalizedTextFormatted(string key, params object[] args)
        {
            return GetLocalizedTextFormatted(DefaultLocalizationTable, key, args);
        }

        public string GetLocalizedTextFormatted(string table, string key, params object[] args)
        {
            var template = GetLocalizedText(table, key);

            if (IsPlaceholder(template))
                return template;

            try
            {
                return string.Format(template, args);
            }
            catch (Exception e)
            {
                LogHelper.LogWarning($"[LocalizationManager] String format error for key {key}: {e.Message}");
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

        public string GetLanguageDisplayName(string langCode)
        {
            if (!_isInitialized)
                return langCode;

            var locale = LocalizationSettings.AvailableLocales.Locales.FirstOrDefault(l => l.Identifier.Code == langCode);
            return locale?.LocaleName ?? langCode;
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

        public void ForceRefresh()
        {
            LogHelper.Log($"[LocalizationManager] Can only force refresh by restarting the game.");
            GameManager.Restart();
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

        #region Private Implementation

        private async Task LoadTextAsync(string table, string key)
        {
            var cacheKey = $"{table}_{key}";
            if (_loadingTasks.ContainsKey(cacheKey))
                return;

            var loadingTask = LoadTextInternalAsync(table, key);
            _loadingTasks[cacheKey] = loadingTask;

            try
            {
                await loadingTask;
            }
            catch (Exception e)
            {
                LogHelper.LogException(e);
            }
            finally
            {
                _loadingTasks.TryRemove(cacheKey, out _);
            }
        }

        private async Task<string> LoadTextInternalAsync(string table, string key)
        {
            if (!_isInitialized)
            {
                LogHelper.LogError($"[LocalizationManager] System not initialized when trying to load text");
                return GetPlaceholder(key);
            }

            try
            {
                var cacheKey = $"{table}_{key}";

                if (!_locStringCache.TryGetValue(cacheKey, out var locString))
                {
                    locString = new LocalizedString(table, key);
                    _locStringCache[cacheKey] = locString;
                }

                var handle = locString.GetLocalizedStringAsync();
                await handle.Task;

                if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                {
                    var result = handle.Result ?? GetPlaceholder(key);

                    lock (_cacheLock)
                    {
                        _stringCache.Add(cacheKey, result);
                    }

                    return result;
                }
                else
                {
                    LogHelper.LogError($"[LocalizationManager] Failed to load text for key {key} in table {table}");
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

            var placeholder = GetPlaceholder(key);
            lock (_cacheLock)
            {
                _stringCache.Add($"{table}_{key}", placeholder);
            }
            return placeholder;
        }

        private IEnumerator SetLanguageCoroutine(string langCode)
        {
            var task = SetLanguageAsync(langCode);
            yield return new WaitUntil(() => task.IsCompleted);
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

        private void ClearCache()
        {
            lock (_cacheLock)
            {
                _stringCache.Clear();
                _locStringCache.Clear();
            }
        }

        private static string GetPlaceholder(string key)
        {
            return $"#{key}#";
        }

        private static bool IsPlaceholder(string text)
        {
            return text != null && text.StartsWith('#') && text.EndsWith('#');
        }

        #endregion
    }
}