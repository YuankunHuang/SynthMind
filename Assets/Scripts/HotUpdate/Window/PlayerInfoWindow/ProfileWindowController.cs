using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YuankunHuang.Unity.AudioCore;
using YuankunHuang.Unity.ModuleCore;
using YuankunHuang.Unity.UICore;
using YuankunHuang.Unity.Util;

namespace YuankunHuang.Unity.HotUpdate
{
    public class ProfileWindowController : WindowControllerBase
    {
        #region UI Ref
        private enum ExtraBtn
        {
            Mask = 0,
        }

        private GeneralButton _maskBtn;
        #endregion

        #region Lifecycle
        protected override void OnInit()
        {
            _maskBtn = Config.ExtraButtonList[(int)ExtraBtn.Mask];
            _maskBtn.onClick.AddListener(OnGoBack);
        }

        protected override void OnShow(IWindowData data, WindowShowState state)
        {
            Config.CanvasGroup.CanvasGroupOn();
        }

        protected override void OnHide(WindowHideState state)
        {
            Config.CanvasGroup.CanvasGroupOff();
        }

        protected override void OnDispose()
        {
            _maskBtn.onClick.RemoveAllListeners();
        }
        #endregion

        #region Events
        private void OnGoBack()
        {
            ModuleRegistry.Get<IAudioManager>().PlayUI(GameDataConfig.AudioIdType.TestButtonClick);
            ModuleRegistry.Get<IUIManager>().GoBack();
        }
        #endregion
    }
}