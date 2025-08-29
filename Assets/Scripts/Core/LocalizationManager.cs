using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using TMPro;
using YuankunHuang.Unity.Core;
using YuankunHuang.Unity.ModuleCore;

namespace YuankunHuang.Unity.LocalizationCore
{
    /// <summary>
    /// Simplified Localization Manager
    /// Design Philosophy:
    /// 1. Preload all localization content, external direct usage
    /// 2. Or directly register UI components for automatic refresh management
    /// </summary>
    public class LocalizationManager : ILocalizationManager
    {
        public static string DefaultLocalizationTable = "Localization";

        // Public properties
        public bool IsInitialized { get; private set; } = false;
        public string CurrentLanguage => _currentLanguage;

        // Private fields
        private string _currentLanguage = "en";
        private readonly Dictionary<string, Locale> _localeMap = new();

        // Simplified storage - all texts preloaded in memory
        private readonly ConcurrentDictionary<string, string> _textCache = new();
        private readonly ConcurrentDictionary<string, LocalizedString> _locStringCache = new();

        // UI component registration - directly manage UI updates
        private readonly List<RegisteredTextComponent> _registeredComponents = new();

        private volatile bool _isDisposed = false;

        #region Singleton

        private static LocalizationManager _instance;
        public static LocalizationManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new LocalizationManager();
                }
                return _instance;
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize and preload all localization content
        /// </summary>
        public async Task InitializeAsync()
        {
            if (IsInitialized || _isDisposed)
                return;

            try
            {
                LogHelper.Log("[LocalizationManager] Starting initialization...");

                await LocalizationSettings.InitializationOperation.Task;
                BuildLocaleMap();

                if (LocalizationSettings.SelectedLocale != null)
                {
                    _currentLanguage = LocalizationSettings.SelectedLocale.Identifier.Code;
                }
                else if (_localeMap.Count > 0)
                {
                    var firstLocale = _localeMap.Values.First();
                    LocalizationSettings.SelectedLocale = firstLocale;
                    _currentLanguage = firstLocale.Identifier.Code;
                }

                LocalizationSettings.SelectedLocaleChanged += OnSelectedLocaleChanged;

                // Preload all texts
                await PreloadAllTexts();

                IsInitialized = true;
                LogHelper.Log($"[LocalizationManager] Initialization completed, current language: {_currentLanguage}, loaded {_textCache.Count} texts");
            }
            catch (Exception e)
            {
                LogHelper.LogError($"[LocalizationManager] Initialization failed: {e.Message}");
                LogHelper.LogException(e);
            }
        }

        /// <summary>
        /// Preload all localization table contents
        /// </summary>
        private async Task PreloadAllTexts()
        {
            try
            {
                var tables = await LocalizationSettings.StringDatabase.GetAllTables().Task;
                LogHelper.Log($"[LocalizationManager] Starting to preload {tables.Count} localization tables...");

                foreach (var tableRef in tables)
                {
                    await PreloadTable(tableRef.TableCollectionName);
                }
            }
            catch (Exception e)
            {
                LogHelper.LogError($"[LocalizationManager] Preloading failed: {e.Message}");
                LogHelper.LogException(e);
            }
        }

        /// <summary>
        /// Preload all contents of specified table
        /// </summary>
        private async Task PreloadTable(string tableName)
        {
            try
            {
                var table = await LocalizationSettings.StringDatabase.GetTableAsync(tableName).Task;
                if (table == null)
                {
                    LogHelper.LogWarning($"[LocalizationManager] Table not found: {tableName}");
                    return;
                }

                var loadTasks = new List<Task>();

                foreach (var entry in table.Values)
                {
                    if (entry != null)
                    {
                        loadTasks.Add(LoadAndCacheText(tableName, entry.Key));
                    }
                }

                await Task.WhenAll(loadTasks);
                LogHelper.Log($"[LocalizationManager] Table {tableName} preloaded, total {loadTasks.Count} texts");
            }
            catch (Exception e)
            {
                LogHelper.LogError($"[LocalizationManager] Failed to preload table: {tableName} - {e.Message}");
            }
        }

        #endregion

        #region Core API - Direct Text Retrieval (Synchronous)

        /// <summary>
        /// Get localized text - synchronous return, preload mode
        /// </summary>
        public string GetLocalizedText(string key)
        {
            return GetLocalizedText(DefaultLocalizationTable, key);
        }

        /// <summary>
        /// Get localized text - specify table
        /// </summary>
        public string GetLocalizedText(string table, string key)
        {
            if (string.IsNullOrEmpty(key))
                return "[EMPTY_KEY]";

            if (string.IsNullOrEmpty(table))
                table = DefaultLocalizationTable;

            var cacheKey = GetCacheKey(table, key, _currentLanguage);

            // Directly get from cache, should always have value if preloading is correct
            if (_textCache.TryGetValue(cacheKey, out var cachedText))
            {
                return cachedText;
            }

            // If not in cache, might be newly added text, try real-time loading
            if (IsInitialized)
            {
                _ = LoadAndCacheText(table, key); // Background loading, effective on next call
            }

            return $"[{key}]"; // Return friendly placeholder
        }

        /// <summary>
        /// Get formatted localized text
        /// </summary>
        public string GetLocalizedTextFormatted(string key, params object[] args)
        {
            return GetLocalizedTextFormatted(DefaultLocalizationTable, key, args);
        }

        /// <summary>
        /// Get formatted localized text - specify table
        /// </summary>
        public string GetLocalizedTextFormatted(string table, string key, params object[] args)
        {
            var template = GetLocalizedText(table, key);

            if (args == null || args.Length == 0 || IsPlaceholder(template))
                return template;

            try
            {
                return string.Format(template, args);
            }
            catch (Exception e)
            {
                LogHelper.LogWarning($"[LocalizationManager] Formatting failed: {table}.{key} - {e.Message}");
                return template;
            }
        }

        #endregion

        #region UI Component Registration - Direct UI Update Management

        /// <summary>
        /// Register Text component - automatically manage localization and updates during language switching
        /// </summary>
        public void RegisterText(Text textComponent, string key, string table = null)
        {
            if (textComponent == null || string.IsNullOrEmpty(key))
                return;

            table = table ?? DefaultLocalizationTable;

            var registered = new RegisteredTextComponent
            {
                TextComponent = textComponent,
                TMPComponent = null,
                Table = table,
                Key = key,
                FormatArgs = null
            };

            _registeredComponents.Add(registered);

            // Immediately update text
            UpdateRegisteredComponent(registered);
        }

        /// <summary>
        /// Register TextMeshPro component
        /// </summary>
        public void RegisterText(TextMeshProUGUI tmpComponent, string key, string table = null)
        {
            if (tmpComponent == null || string.IsNullOrEmpty(key))
                return;

            table = table ?? DefaultLocalizationTable;

            var registered = new RegisteredTextComponent
            {
                TextComponent = null,
                TMPComponent = tmpComponent,
                Table = table,
                Key = key,
                FormatArgs = null
            };

            _registeredComponents.Add(registered);

            // Immediately update text
            UpdateRegisteredComponent(registered);
        }

        /// <summary>
        /// Register Text component with formatting parameters
        /// </summary>
        public void RegisterTextFormatted(Text textComponent, string key, string table = null, params object[] args)
        {
            if (textComponent == null || string.IsNullOrEmpty(key))
                return;

            table = table ?? DefaultLocalizationTable;

            var registered = new RegisteredTextComponent
            {
                TextComponent = textComponent,
                TMPComponent = null,
                Table = table,
                Key = key,
                FormatArgs = args
            };

            _registeredComponents.Add(registered);
            UpdateRegisteredComponent(registered);
        }

        /// <summary>
        /// Register TextMeshPro component with formatting parameters
        /// </summary>
        public void RegisterTextFormatted(TextMeshProUGUI tmpComponent, string key, string table = null, params object[] args)
        {
            if (tmpComponent == null || string.IsNullOrEmpty(key))
                return;

            table = table ?? DefaultLocalizationTable;

            var registered = new RegisteredTextComponent
            {
                TextComponent = null,
                TMPComponent = tmpComponent,
                Table = table,
                Key = key,
                FormatArgs = args
            };

            _registeredComponents.Add(registered);
            UpdateRegisteredComponent(registered);
        }

        /// <summary>
        /// Unregister UI component
        /// </summary>
        public void UnregisterText(Text textComponent)
        {
            _registeredComponents.RemoveAll(r => r.TextComponent == textComponent);
        }

        /// <summary>
        /// Unregister TextMeshPro component
        /// </summary>
        public void UnregisterText(TextMeshProUGUI tmpComponent)
        {
            _registeredComponents.RemoveAll(r => r.TMPComponent == tmpComponent);
        }

        #endregion

        #region Language Management

        /// <summary>
        /// Switch language
        /// </summary>
        public void SetLanguage(string langCode)
        {
            if (_isDisposed || string.IsNullOrEmpty(langCode) || _currentLanguage == langCode)
                return;

            MonoManager.Instance.StartCoroutine(SetLanguageCoroutine(langCode));
        }

        /// <summary>
        /// Asynchronously switch language
        /// </summary>
        public async Task SetLanguageAsync(string langCode)
        {
            if (_isDisposed || string.IsNullOrEmpty(langCode) || _currentLanguage == langCode)
                return;

            if (!_localeMap.TryGetValue(langCode, out var targetLocale))
            {
                LogHelper.LogError($"[LocalizationManager] Language not found: {langCode}");
                return;
            }

            try
            {
                LogHelper.Log($"[LocalizationManager] Switching language: {_currentLanguage} -> {langCode}");

                LocalizationSettings.SelectedLocale = targetLocale;
                await Task.Yield();

                // Reload all texts
                _textCache.Clear();
                await PreloadAllTexts();

                // Update all registered UI components
                UpdateAllRegisteredComponents();

            }
            catch (Exception e)
            {
                LogHelper.LogError($"[LocalizationManager] Language switching failed: {e.Message}");
                LogHelper.LogException(e);
            }
        }

        /// <summary>
        /// Get available language list
        /// </summary>
        public List<string> GetAvailableLanguages()
        {
            return _localeMap.Keys.ToList();
        }

        /// <summary>
        /// Get language display name
        /// </summary>
        public string GetLanguageDisplayName(string langCode)
        {
            return _localeMap.TryGetValue(langCode, out var locale) ? locale.LocaleName : langCode;
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Force refresh
        /// </summary>
        public void ForceRefresh()
        {
            if (!IsInitialized)
                return;

            MonoManager.Instance.StartCoroutine(ForceRefreshCoroutine());
        }

        private IEnumerator ForceRefreshCoroutine()
        {
            _textCache.Clear();
            var task = PreloadAllTexts();
            yield return new WaitUntil(() => task.IsCompleted);
            UpdateAllRegisteredComponents();
        }

        /// <summary>
        /// Get cache status
        /// </summary>
        public (int cached, int registered) GetCacheStats()
        {
            return (_textCache.Count, _registeredComponents.Count);
        }

        #endregion

        #region Private Implementation

        /// <summary>
        /// Actual text loading method
        /// </summary>
        private async Task LoadAndCacheText(string table, string key)
        {
            try
            {
                var locStringKey = $"{table}_{key}";
                if (!_locStringCache.TryGetValue(locStringKey, out var locString))
                {
                    locString = new LocalizedString(table, key);
                    _locStringCache[locStringKey] = locString;
                }

                var handle = locString.GetLocalizedStringAsync();
                await handle.Task;

                if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded
                    && !string.IsNullOrEmpty(handle.Result))
                {
                    var cacheKey = GetCacheKey(table, key, _currentLanguage);
                    _textCache[cacheKey] = handle.Result;
                }
            }
            catch (Exception e)
            {
                LogHelper.LogError($"[LocalizationManager] Failed to load text: {table}.{key} - {e.Message}");
            }
        }

        /// <summary>
        /// Update single registered component
        /// </summary>
        private void UpdateRegisteredComponent(RegisteredTextComponent component)
        {
            if (component == null)
                return;

            string text;
            if (component.FormatArgs != null && component.FormatArgs.Length > 0)
            {
                text = GetLocalizedTextFormatted(component.Table, component.Key, component.FormatArgs);
            }
            else
            {
                text = GetLocalizedText(component.Table, component.Key);
            }

            // Update UI component
            if (component.TextComponent != null)
            {
                component.TextComponent.text = text;
            }
            else if (component.TMPComponent != null)
            {
                component.TMPComponent.text = text;
            }
        }

        /// <summary>
        /// Update all registered UI components
        /// </summary>
        private void UpdateAllRegisteredComponents()
        {
            // Clean up destroyed components
            _registeredComponents.RemoveAll(r =>
                (r.TextComponent == null && r.TMPComponent == null) ||
                (r.TextComponent != null && r.TextComponent == null) ||
                (r.TMPComponent != null && r.TMPComponent == null));

            // Update all valid components
            foreach (var component in _registeredComponents)
            {
                UpdateRegisteredComponent(component);
            }

            LogHelper.Log($"[LocalizationManager] Updated {_registeredComponents.Count} UI components");
        }

        private IEnumerator SetLanguageCoroutine(string langCode)
        {
            var task = SetLanguageAsync(langCode);
            yield return new WaitUntil(() => task.IsCompleted);
        }

        private void OnSelectedLocaleChanged(Locale locale)
        {
            if (locale == null || _isDisposed)
                return;

            var newLanguage = locale.Identifier.Code;
            if (_currentLanguage != newLanguage)
            {
                _currentLanguage = newLanguage;
                LogHelper.Log($"[LocalizationManager] Language switched to: {newLanguage}");
            }
        }

        private void BuildLocaleMap()
        {
            _localeMap.Clear();
            if (LocalizationSettings.AvailableLocales?.Locales != null)
            {
                foreach (var locale in LocalizationSettings.AvailableLocales.Locales)
                {
                    if (locale?.Identifier != null)
                    {
                        _localeMap[locale.Identifier.Code] = locale;
                    }
                }
            }
        }

        #region Helper Methods

        private static string GetCacheKey(string table, string key, string language)
        {
            return $"{table}_{key}_{language}";
        }

        private static bool IsPlaceholder(string text)
        {
            return text != null && text.StartsWith('[') && text.EndsWith(']');
        }

        #endregion

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            if (LocalizationSettings.InitializationOperation.IsDone)
            {
                LocalizationSettings.SelectedLocaleChanged -= OnSelectedLocaleChanged;
            }

            _textCache.Clear();
            _locStringCache.Clear();
            _registeredComponents.Clear();
            _localeMap.Clear();

            IsInitialized = false;
            LogHelper.Log("[LocalizationManager] Disposed");
        }

        #endregion
    }

    /// <summary>
    /// Registered UI component information
    /// </summary>
    internal class RegisteredTextComponent
    {
        public Text TextComponent { get; set; }
        public TextMeshProUGUI TMPComponent { get; set; }
        public string Table { get; set; }
        public string Key { get; set; }
        public object[] FormatArgs { get; set; }
    }

    /// <summary>
    /// Simplified static helper class
    /// </summary>
    public static class L10n
    {
        private static LocalizationManager _manager => LocalizationManager.Instance;

        /// <summary>
        /// Get localized text
        /// </summary>
        public static string Text(string key) => _manager.GetLocalizedText(key);

        /// <summary>
        /// Get localized text (specify table)
        /// </summary>
        public static string Text(string table, string key) => _manager.GetLocalizedText(table, key);

        /// <summary>
        /// Get formatted localized text
        /// </summary>
        public static string Text(string key, params object[] args) => _manager.GetLocalizedTextFormatted(key, args);

        /// <summary>
        /// Switch language
        /// </summary>
        public static void SetLanguage(string langCode) => _manager.SetLanguage(langCode);

        /// <summary>
        /// Register Text component for automatic management
        /// </summary>
        public static void Register(Text textComponent, string key, string table = null)
            => _manager.RegisterText(textComponent, key, table);

        /// <summary>
        /// Register TextMeshPro component for automatic management
        /// </summary>
        public static void Register(TextMeshProUGUI tmpComponent, string key, string table = null)
            => _manager.RegisterText(tmpComponent, key, table);

        /// <summary>
        /// Register formatted Text component
        /// </summary>
        public static void RegisterFormatted(Text textComponent, string key, string table = null, params object[] args)
            => _manager.RegisterTextFormatted(textComponent, key, table, args);

        /// <summary>
        /// Register formatted TextMeshPro component
        /// </summary>
        public static void RegisterFormatted(TextMeshProUGUI tmpComponent, string key, string table = null, params object[] args)
            => _manager.RegisterTextFormatted(tmpComponent, key, table, args);

        /// <summary>
        /// Current language
        /// </summary>
        public static string CurrentLanguage => _manager.CurrentLanguage;

        /// <summary>
        /// Available languages
        /// </summary>
        public static List<string> AvailableLanguages => _manager.GetAvailableLanguages();
    }
}