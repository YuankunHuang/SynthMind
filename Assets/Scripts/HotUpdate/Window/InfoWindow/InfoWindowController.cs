using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using YuankunHuang.Unity.AudioCore;
using YuankunHuang.Unity.ModuleCore;
using YuankunHuang.Unity.UICore;
using YuankunHuang.Unity.Util;

namespace YuankunHuang.Unity.HotUpdate
{
    public struct InfoWindowData : IWindowData
    {
        public string Title;
        public string Content;
        public InfoWindowData(string title, string content)
        {
            Title = title;
            Content = content;
        }
    }

    public class InfoWindowController : WindowControllerBase
    {
        #region UI Ref
        private enum ExtraTMP
        {
            Title = 0,
            Content = 1,
        }

        private enum ExtraBtn
        {
            Mask = 0,
        }

        private TMP_Text _titleTxt;
        private TMP_Text _contentTxt;

        private GeneralButton _maskBtn;
        #endregion

        #region Lifecycle
        protected override void OnInit()
        {
            _titleTxt = Config.ExtraTextMeshProList[(int)ExtraTMP.Title];
            _contentTxt = Config.ExtraTextMeshProList[(int)ExtraTMP.Content];
            _maskBtn = Config.ExtraButtonList[(int)ExtraBtn.Mask];

            _maskBtn.onClick.AddListener(OnMaskBtnClick);
        }

        protected override void OnShow(IWindowData data, WindowShowState state)
        {
            var windowData = (InfoWindowData)data;
            _titleTxt.text = windowData.Title;
            _contentTxt.text = windowData.Content;

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
        private void OnMaskBtnClick()
        {
            ModuleRegistry.Get<IAudioManager>().PlayUI(GameDataConfig.AudioIdType.TestButtonClick);
            ModuleRegistry.Get<IUIManager>().GoBack();
        }
        #endregion
    }
}
