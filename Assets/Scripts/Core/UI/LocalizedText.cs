using TMPro;
using UnityEngine;
using YuankunHuang.Unity.Core;
using YuankunHuang.Unity.ModuleCore;
using YuankunHuang.Unity.LocalizationCore;

namespace YuankunHuang.Unity.UICore
{
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizedText : MonoBehaviour
    {
        [SerializeField] private string _localizationKey;
        [SerializeField] private string _tableName = "Localization";
        [SerializeField] private bool _updateOnLanguageChange = true;

        private TMP_Text _textComponent;
        private ILocalizationManager _localizationManager;

        private void Awake()
        {
            _textComponent = GetComponent<TMP_Text>();
        }

        private void Start()
        {
            _localizationManager = ModuleRegistry.Get<ILocalizationManager>();
            
            if (_updateOnLanguageChange && _localizationManager != null)
            {
                _localizationManager.OnLanguageChanged += OnLanguageChanged;
            }
            
            UpdateText();
        }

        private void OnDestroy()
        {
            if (_localizationManager != null)
            {
                _localizationManager.OnLanguageChanged -= OnLanguageChanged;
            }
        }

        public void SetKey(string key, string tableName = null)
        {
            _localizationKey = key;
            if (!string.IsNullOrEmpty(tableName))
                _tableName = tableName;
            
            UpdateText();
        }

        [ContextMenu("Update Text")]
        public void UpdateText()
        {
            if (_textComponent == null || string.IsNullOrEmpty(_localizationKey))
                return;

            if (_localizationManager == null)
                _localizationManager = ModuleRegistry.Get<ILocalizationManager>();

            if (_localizationManager != null)
            {
                _textComponent.text = _localizationManager.GetLocalizedText(_tableName, _localizationKey);
            }
        }

        private void OnLanguageChanged(string newLanguage)
        {
            UpdateText();
        }

        public string LocalizationKey
        {
            get => _localizationKey;
            set
            {
                _localizationKey = value;
                UpdateText();
            }
        }

        public string TableName
        {
            get => _tableName;
            set
            {
                _tableName = value;
                UpdateText();
            }
        }
    }
}