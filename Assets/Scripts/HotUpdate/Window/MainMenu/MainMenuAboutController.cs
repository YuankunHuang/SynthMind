using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YuankunHuang.Unity.Core;
using YuankunHuang.Unity.Util;
using YuankunHuang.Unity.UICore;

namespace YuankunHuang.Unity.HotUpdate
{
    public class MainMenuAboutController : IMainMenuWidgetController
    {
        private GeneralWidgetConfig _config;

        public MainMenuAboutController(GeneralWidgetConfig config)
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