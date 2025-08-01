using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YuankunHuang.SynthMind.Core;
using YuankunHuang.SynthMind.Util;

namespace YuankunHuang.SynthMind.HotUpdate
{
    public class MainMenuSettingController : IMainMenuWidgetController
    {
        private GeneralWidgetConfig _config;

        public MainMenuSettingController(GeneralWidgetConfig config)
        {
            _config = config;
        }

        public void Init()
        {

        }

        public void Show()
        {
            _config.CanvasGroup.CanvasGroupOn();
        }

        public void Hide()
        {
            _config.CanvasGroup.CanvasGroupOff();
        }

        public void Dispose()
        {

        }
    }
}