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
using YuankunHuang.Unity.Core.Debug;

namespace YuankunHuang.Unity.HotUpdate
{
    public class MainMenuSettingController : IMainMenuWidgetController
    {
        #region UI References
        private enum ExtraConfig
        {
            Language = 0,
            Sound = 1,
            Graphics = 2,
        }

        private MainMenuSettingLanguageController _langController;
        private MainMenuSettingSoundController _soundController;
        private MainMenuSettingGraphicsController _graphicsController;
        #endregion

        private GeneralWidgetConfig _config;

        public MainMenuSettingController(GeneralWidgetConfig config)
        {
            _config = config;
        }

        public void Init()
        {
            // language
            var languageConfig = _config.ExtraWidgetConfigList[(int)ExtraConfig.Language];
            _langController = new MainMenuSettingLanguageController(languageConfig);
            _langController.Init();

            // sound
            var soundConfig = _config.ExtraWidgetConfigList[(int)ExtraConfig.Sound];
            _soundController = new MainMenuSettingSoundController(soundConfig);
            _soundController.Init();

            // graphics
            var graphicsConfig = _config.ExtraWidgetConfigList[(int)ExtraConfig.Graphics];
            _graphicsController = new MainMenuSettingGraphicsController(graphicsConfig);
            _graphicsController.Init();
        }

        public void Show()
        {
            try
            {
                // language
                _langController.Refresh();

                // sound
                _soundController.Refresh();

                // graphics
                _graphicsController.Refresh();

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
            // language
            _langController.Dispose();

            // sound
            _soundController.Dispose();

            // graphics
            _graphicsController.Dispose();
        }

    }
}