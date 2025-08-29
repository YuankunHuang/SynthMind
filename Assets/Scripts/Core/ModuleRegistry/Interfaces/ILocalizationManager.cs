using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YuankunHuang.Unity.ModuleCore;

namespace YuankunHuang.Unity.LocalizationCore
{
    /// <summary>
    /// Simplified localization manager interface
    /// Design Philosophy:
    /// 1. Preload all texts, external synchronous access
    /// 2. Directly register UI components, automatically manage updates
    /// </summary>
    public interface ILocalizationManager : IModule
    {
        #region Basic Properties

        /// <summary>
        /// Whether initialized and preloading completed
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Current language code
        /// </summary>
        string CurrentLanguage { get; }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize and preload all localization content
        /// </summary>
        Task InitializeAsync();

        #endregion

        #region Text Retrieval API (Preload Mode)

        /// <summary>
        /// Get localized text (using default table)
        /// Synchronous return, all texts are preloaded
        /// </summary>
        /// <param name="key">Text key</param>
        /// <returns>Localized text</returns>
        string GetLocalizedText(string key);

        /// <summary>
        /// Get localized text (specify table)
        /// </summary>
        /// <param name="table">Localization table name</param>
        /// <param name="key">Text key</param>
        /// <returns>Localized text</returns>
        string GetLocalizedText(string table, string key);

        /// <summary>
        /// Get formatted localized text (using default table)
        /// </summary>
        /// <param name="key">Text key</param>
        /// <param name="args">Format parameters</param>
        /// <returns>Formatted text</returns>
        string GetLocalizedTextFormatted(string key, params object[] args);

        /// <summary>
        /// Get formatted localized text (specify table)
        /// </summary>
        /// <param name="table">Localization table name</param>
        /// <param name="key">Text key</param>
        /// <param name="args">Format parameters</param>
        /// <returns>Formatted text</returns>
        string GetLocalizedTextFormatted(string table, string key, params object[] args);

        #endregion

        #region UI Component Registration API (Auto-Management Mode)

        /// <summary>
        /// Register Text component, automatically manage text content and language switching updates
        /// </summary>
        /// <param name="textComponent">Text component</param>
        /// <param name="key">Localization key</param>
        /// <param name="table">Localization table (optional)</param>
        void RegisterText(Text textComponent, string key, string table = null);

        /// <summary>
        /// Register TextMeshPro component, automatically manage text content and language switching updates
        /// </summary>
        /// <param name="tmpComponent">TextMeshPro component</param>
        /// <param name="key">Localization key</param>
        /// <param name="table">Localization table (optional)</param>
        void RegisterText(TextMeshProUGUI tmpComponent, string key, string table = null);

        /// <summary>
        /// Register Text component with format parameters
        /// </summary>
        /// <param name="textComponent">Text component</param>
        /// <param name="key">Localization key</param>
        /// <param name="table">Localization table (optional)</param>
        /// <param name="args">Format parameters</param>
        void RegisterTextFormatted(Text textComponent, string key, string table = null, params object[] args);

        /// <summary>
        /// Register TextMeshPro component with format parameters
        /// </summary>
        /// <param name="tmpComponent">TextMeshPro component</param>
        /// <param name="key">Localization key</param>
        /// <param name="table">Localization table (optional)</param>
        /// <param name="args">Format parameters</param>
        void RegisterTextFormatted(TextMeshProUGUI tmpComponent, string key, string table = null, params object[] args);

        /// <summary>
        /// Unregister Text component
        /// </summary>
        /// <param name="textComponent">Text component to unregister</param>
        void UnregisterText(Text textComponent);

        /// <summary>
        /// Unregister TextMeshPro component
        /// </summary>
        /// <param name="tmpComponent">TextMeshPro component to unregister</param>
        void UnregisterText(TextMeshProUGUI tmpComponent);

        #endregion

        #region Language Management

        /// <summary>
        /// Switch language (coroutine method)
        /// Automatically reload all texts and update registered UI components
        /// </summary>
        /// <param name="langCode">Language code</param>
        void SetLanguage(string langCode);

        /// <summary>
        /// Asynchronously switch language
        /// </summary>
        /// <param name="langCode">Language code</param>
        Task SetLanguageAsync(string langCode);

        /// <summary>
        /// Get all available language codes
        /// </summary>
        /// <returns>List of language codes</returns>
        List<string> GetAvailableLanguages();

        /// <summary>
        /// Get language display name
        /// </summary>
        /// <param name="langCode">Language code</param>
        /// <returns>Language display name</returns>
        string GetLanguageDisplayName(string langCode);

        #endregion

        #region Utility Methods

        /// <summary>
        /// Force refresh all localization content
        /// Clear cache and reload, update all registered UI components
        /// </summary>
        void ForceRefresh();

        /// <summary>
        /// Get system status (for debugging)
        /// </summary>
        /// <returns>(Cached text count, Registered UI component count)</returns>
        (int cached, int registered) GetCacheStats();

        #endregion
    }
}