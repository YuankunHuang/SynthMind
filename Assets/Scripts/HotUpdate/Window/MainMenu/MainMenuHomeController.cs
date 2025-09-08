using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YuankunHuang.Unity.Util;
using YuankunHuang.Unity.UICore;
using UnityEngine.UI;

namespace YuankunHuang.Unity.HotUpdate
{
    public class MainMenuHomeController : IMainMenuWidgetController
    {
        private GeneralWidgetConfig _config;

        #region UI Ref
        private enum ExtraObj
        {
            ScrollRect = 0,
        }

        private ScrollRect _scrollRect;
        #endregion

        public MainMenuHomeController(GeneralWidgetConfig config)
        {
            _config = config;
        }

        public void Init()
        {
            _scrollRect = _config.ExtraObjectList[(int)ExtraObj.ScrollRect].GetComponent<ScrollRect>();
        }

        public void Show()
        {
            _scrollRect.verticalNormalizedPosition = 1;

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