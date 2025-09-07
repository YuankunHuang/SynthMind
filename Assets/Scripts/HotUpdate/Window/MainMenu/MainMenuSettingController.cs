using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using YuankunHuang.Unity.Core;
using YuankunHuang.Unity.ModuleCore;
using YuankunHuang.Unity.UICore;
using YuankunHuang.Unity.Util;
using YuankunHuang.Unity.LocalizationCore;
using YuankunHuang.Unity.GameDataConfig;

namespace YuankunHuang.Unity.HotUpdate
{
    public class MainMenuSettingController : IMainMenuWidgetController
    {
        #region UI References
        private enum ExtraObj
        {
            BtnRoot = 0,
        }

        private enum ExtraGO
        {
            BtnPrefab = 0,
        }

        private Transform _btnRoot;

        private GameObject _btnPrefab;
        #endregion

        private GeneralWidgetConfig _config;
        private ILocalizationManager _localizationManager;

        private List<string> _availableLanguages;
        private int _currentLanguageIndex;
        private List<GameObject> _languageButtonInstances;
        private bool _isLanguageSwitching;

        public MainMenuSettingController(GeneralWidgetConfig config)
        {
            _config = config;
        }

        public void Init()
        {
            _localizationManager = ModuleRegistry.Get<ILocalizationManager>();

            _btnRoot = _config.ExtraObjectList[(int)ExtraObj.BtnRoot];
            _btnPrefab = _config.ExtraGameObjectList[(int)ExtraGO.BtnPrefab];
            _languageButtonInstances = new List<GameObject>();

            _localizationManager.OnLanguageChanged += OnLanguageChanged;
        }

        public void Show()
        {
            try
            {
                UpdateLanguageList();
                CreateLanguageButtons();
                UpdateSelectedLanguageDisplay();
                _config.CanvasGroup.CanvasGroupOn();
            }
            catch (System.Exception ex)
            {
                LogHelper.LogError($"[MainMenuSettingController] Error in Show: {ex.Message}");
            }
        }

        public void Hide()
        {
            _config.CanvasGroup.CanvasGroupOff();
        }

        public void Dispose()
        {
            if (_localizationManager != null)
            {
                _localizationManager.OnLanguageChanged -= OnLanguageChanged;
            }
            
            ClearLanguageButtons();
        }

        #region Language Management
        private void UpdateLanguageList()
        {
            try
            {
                _availableLanguages = _localizationManager?.GetAvailableLanguages() ?? new List<string>();
                
                if (_availableLanguages.Count > 0)
                {
                    var currentLang = _localizationManager.CurrentLanguage;
                    _currentLanguageIndex = _availableLanguages.IndexOf(currentLang);
                    if (_currentLanguageIndex < 0)
                    {
                        _currentLanguageIndex = 0;
                        LogHelper.LogWarning($"[MainMenuSettingController] Current language '{currentLang}' not found in available languages, defaulting to index 0");
                    }
                }
                else
                {
                    LogHelper.LogWarning("[MainMenuSettingController] No available languages found");
                }
            }
            catch (System.Exception ex)
            {
                LogHelper.LogError($"[MainMenuSettingController] Error updating language list: {ex.Message}");
                _availableLanguages = new List<string>();
                _currentLanguageIndex = 0;
            }
        }

        private void CreateLanguageButtons()
        {
            if (_availableLanguages?.Count <= 0) return;

            // Clear existing buttons first
            ClearLanguageButtons();

            try
            {
                // Create new buttons
                for (var i = 0; i < _availableLanguages.Count; i++)
                {
                    var index = i;
                    var currentLang = _availableLanguages[index];
                    var langCfgData = LanguageConfig.GetByLangCode(currentLang);
                    
                    if (langCfgData == null)
                    {
                        LogHelper.LogWarning($"[MainMenuSettingController] Language config not found for: {currentLang}");
                        continue;
                    }
                    
                    var displayName = _localizationManager.GetLanguageDisplayName(currentLang);
                    var data = new MainMenuLanguageBtnData(langCfgData, displayName, () =>
                    {
                        OnLanguageButtonClicked(index);
                    });
                    
                    var buttonInstance = GameObject.Instantiate(_btnPrefab, _btnRoot);
                    var config = buttonInstance.GetComponent<GeneralWidgetConfig>();
                    
                    if (config != null)
                    {
                        MainMenuLanguageBtnController.Show(config, data);
                        MainMenuLanguageBtnController.SetSelected(config, _currentLanguageIndex == index);
                        _languageButtonInstances.Add(buttonInstance);
                    }
                    else
                    {
                        LogHelper.LogError("[MainMenuSettingController] GeneralWidgetConfig not found on language button prefab");
                        GameObject.Destroy(buttonInstance);
                    }
                }
            }
            catch (System.Exception ex)
            {
                LogHelper.LogError($"[MainMenuSettingController] Error creating language buttons: {ex.Message}");
            }
        }

        private void UpdateSelectedLanguageDisplay()
        {
            // Update visual state of buttons to show current selection
            try
            {
                for (int i = 0; i < _languageButtonInstances.Count && i < _availableLanguages.Count; i++)
                {
                    var buttonInstance = _languageButtonInstances[i];
                    if (buttonInstance != null)
                    {
                        var config = buttonInstance.GetComponent<GeneralWidgetConfig>();
                        if (config != null)
                        {
                            // Update visual state based on selection
                            var isSelected = i == _currentLanguageIndex;
                            MainMenuLanguageBtnController.SetSelected(config, isSelected);
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                LogHelper.LogError($"[MainMenuSettingController] Error updating language button selection: {ex.Message}");
            }
        }

        private void ClearLanguageButtons()
        {
            try
            {
                foreach (var buttonInstance in _languageButtonInstances)
                {
                    if (buttonInstance != null)
                    {
                        GameObject.Destroy(buttonInstance);
                    }
                }
                _languageButtonInstances.Clear();
            }
            catch (System.Exception ex)
            {
                LogHelper.LogError($"[MainMenuSettingController] Error clearing language buttons: {ex.Message}");
            }
        }

        private async void OnLanguageButtonClicked(int index)
        {
            if (_currentLanguageIndex == index || index < 0 || index >= _availableLanguages.Count || _isLanguageSwitching)
            {
                return;
            }

            try
            {
                _isLanguageSwitching = true;
                InputBlocker.StartBlocking();
                
                _currentLanguageIndex = index;
                var newLanguage = _availableLanguages[_currentLanguageIndex];
                
                LogHelper.Log($"[MainMenuSettingController] Switching language to: {newLanguage}");
                await _localizationManager.SetLanguageAsync(newLanguage);
            }
            catch (System.Exception ex)
            {
                LogHelper.LogError($"[MainMenuSettingController] Error switching language: {ex.Message}");
            }
            finally
            {
                _isLanguageSwitching = false;
                InputBlocker.StopBlocking();
            }
        }

        private void OnLanguageChanged(string newLanguage)
        {
            try
            {
                LogHelper.Log($"[MainMenuSettingController] Language changed event received: {newLanguage}");
                
                UpdateLanguageList();
                UpdateSelectedLanguageDisplay();
                
                // Refresh all localized texts more efficiently
                RefreshLocalizedTexts();
            }
            catch (System.Exception ex)
            {
                LogHelper.LogError($"[MainMenuSettingController] Error handling language change: {ex.Message}");
            }
        }

        private void RefreshLocalizedTexts()
        {
            try
            {
                // only refresh texts in active UI -> more efficient
                var localizedTexts = _config.transform.GetComponentsInChildren<LocalizedText>(true);
                foreach (var localizedText in localizedTexts)
                {
                    if (localizedText != null)
                    {
                        localizedText.UpdateText();
                    }
                }
                
                // Also refresh language button texts
                foreach (var buttonInstance in _languageButtonInstances)
                {
                    if (buttonInstance != null)
                    {
                        var buttonTexts = buttonInstance.GetComponentsInChildren<LocalizedText>(true);
                        foreach (var text in buttonTexts)
                        {
                            if (text != null)
                            {
                                text.UpdateText();
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                LogHelper.LogError($"[MainMenuSettingController] Error refreshing localized texts: {ex.Message}");
            }
        }
        #endregion
    }
}