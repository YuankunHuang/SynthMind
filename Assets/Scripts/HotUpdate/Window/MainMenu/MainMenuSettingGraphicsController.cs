using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using YuankunHuang.Unity.LocalizationCore;
using YuankunHuang.Unity.ModuleCore;
using YuankunHuang.Unity.UICore;

namespace YuankunHuang.Unity.HotUpdate
{
    public class MainMenuSettingGraphicsController
    {
        #region UI References
        #endregion

        #region Fields
        private GeneralWidgetConfig _config;
        #endregion

        #region Lifecycle
        public MainMenuSettingGraphicsController(GeneralWidgetConfig config)
        {
            _config = config;
        }

        public void Init()
        {

        }

        public void Refresh()
        {
        }

        public void Dispose()
        {
        }
        #endregion

    }
}