using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using YuankunHuang.Unity.Core;
using YuankunHuang.Unity.ModuleCore;
using YuankunHuang.Unity.UICore;
using YuankunHuang.Unity.Util;
using YuankunHuang.Unity.LocalizationCore;

namespace YuankunHuang.Unity.HotUpdate
{
    public class MainMenuSettingController : IMainMenuWidgetController
    {
        #region UI References
        private enum ExtraTMP
        {
            LanguageDisplay = 0,
        }

        private enum ExtraBtn
        {
            LanguageButton = 0,
        }

        private TMP_Text _languageDisplayTxt;

        private GeneralButton _languageBtn;
        #endregion

        private GeneralWidgetConfig _config;
        private ILocalizationManager _localizationManager;


        private List<string> _availableLanguages;
        private int _currentLanguageIndex;

        public MainMenuSettingController(GeneralWidgetConfig config)
        {
            _config = config;
        }

        public void Init()
        {
            _localizationManager = ModuleRegistry.Get<ILocalizationManager>();
            
            _languageDisplayTxt = _config.ExtraTextMeshProList[(int)ExtraTMP.LanguageDisplay];
            _languageBtn = _config.ExtraButtonList[(int)ExtraBtn.LanguageButton];
            
            _languageBtn.onClick.AddListener(OnLanguageButtonClicked);
            _localizationManager.OnLanguageChanged += OnLanguageChanged;
            
            UpdateLanguageList();
            UpdateLanguageDisplay();
        }

        public void Show()
        {
            UpdateLanguageDisplay();
            _config.CanvasGroup.CanvasGroupOn();
        }

        public void Hide()
        {
            _config.CanvasGroup.CanvasGroupOff();
        }

        public void Dispose()
        {
            _languageBtn.onClick.RemoveListener(OnLanguageButtonClicked);
            _localizationManager.OnLanguageChanged -= OnLanguageChanged;
        }

        #region Language Management
        private void UpdateLanguageList()
        {
            _availableLanguages = _localizationManager.GetAvailableLanguages();
            
            if (_availableLanguages.Count > 0)
            {
                var currentLang = _localizationManager.CurrentLanguage;
                _currentLanguageIndex = _availableLanguages.IndexOf(currentLang);
                if (_currentLanguageIndex < 0) _currentLanguageIndex = 0;
            }
        }

        private void UpdateLanguageDisplay()
        {
            if (_availableLanguages?.Count > 0 && _languageDisplayTxt != null)
            {
                var currentLang = _availableLanguages[_currentLanguageIndex];
                var displayName = _localizationManager.GetLanguageDisplayName(currentLang);
                _languageDisplayTxt.text = displayName;
            }
        }

        private void OnLanguageButtonClicked()
        {
            if (_availableLanguages?.Count <= 1) return;

            _currentLanguageIndex = (_currentLanguageIndex + 1) % _availableLanguages.Count;
            var newLanguage = _availableLanguages[_currentLanguageIndex];
            
            _localizationManager.SetLanguageAsync(newLanguage);
        }

        private void OnLanguageChanged(string newLanguage)
        {
            UpdateLanguageList();
            UpdateLanguageDisplay();
            
            RefreshAllLocalizedTexts();
        }

        private void RefreshAllLocalizedTexts()
        {
            var localizedComponents = Object.FindObjectsOfType<LocalizedText>();
            foreach (var component in localizedComponents)
            {
                component.UpdateText();
            }
        }
        #endregion
    }
}