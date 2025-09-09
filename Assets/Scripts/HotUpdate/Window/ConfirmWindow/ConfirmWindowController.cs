using System;
using TMPro;
using YuankunHuang.Unity.AudioCore;
using YuankunHuang.Unity.Core;
using YuankunHuang.Unity.ModuleCore;
using YuankunHuang.Unity.UICore;
using YuankunHuang.Unity.Util;

namespace YuankunHuang.Unity.HotUpdate
{
    public struct ConfirmWindowData : IWindowData
    {
        public string Title;
        public string Content;
        public Action OnConfirm;

        public ConfirmWindowData(string title, string content, Action onConfirm)
        {
            Title = title;
            Content = content;
            OnConfirm = onConfirm;
        }
    }

    public class ConfirmWindowController : WindowControllerBase
    {
        #region UI Ref
        private enum ExtraBtn
        {
            Mask = 0,
            Cancel = 1,
            Confirm = 2,
        }

        private enum ExtraTMP
        {
            Title = 0,
            Content = 1,
        }

        private GeneralButton _maskBtn;
        private GeneralButton _cancelBtn;  
        private GeneralButton _confirmBtn;

        private TMP_Text _titleTxt;
        private TMP_Text _contentTxt;
        #endregion

        #region Fields
        private Action _onConfirm;
        #endregion

        #region Lifecycle
        protected override void OnInit()
        {
            _maskBtn = Config.ExtraButtonList[(int)ExtraBtn.Mask];
            _cancelBtn = Config.ExtraButtonList[(int)ExtraBtn.Cancel];
            _confirmBtn = Config.ExtraButtonList[(int)ExtraBtn.Confirm];

            _titleTxt = Config.ExtraTextMeshProList[(int)ExtraTMP.Title];
            _contentTxt = Config.ExtraTextMeshProList[(int)ExtraTMP.Content];

            _maskBtn.onClick.AddListener(OnMaskBtnClicked);
            _cancelBtn.onClick.AddListener(OnCancelBtnClicked);
            _confirmBtn.onClick.AddListener(OnConfirmBtnClicked);
        }

        protected override void OnShow(IWindowData data, WindowShowState state)
        {
            var windowData = (ConfirmWindowData)data;
            _titleTxt.text = windowData.Title;
            _contentTxt.text = windowData.Content;
            _onConfirm = windowData.OnConfirm;

            Config.CanvasGroup.CanvasGroupOn();        
        }

        protected override void OnHide(WindowHideState state)
        {
            Config.CanvasGroup.CanvasGroupOff();        
        }

        protected override void OnDispose()
        {
            _maskBtn.onClick.RemoveAllListeners();
            _cancelBtn.onClick.RemoveAllListeners();
            _confirmBtn.onClick.RemoveAllListeners();
        }
        #endregion

        #region UI Binding
        private void OnMaskBtnClicked()
        {
            ModuleRegistry.Get<IAudioManager>().PlayUI(GameDataConfig.AudioIdType.TestButtonClick);
            ModuleRegistry.Get<IUIManager>().GoBack();
        }

        private void OnCancelBtnClicked()
        {
            ModuleRegistry.Get<IAudioManager>().PlayUI(GameDataConfig.AudioIdType.TestButtonClick);
            ModuleRegistry.Get<IUIManager>().GoBack();
        }

        private void OnConfirmBtnClicked()
        {
            ModuleRegistry.Get<IAudioManager>().PlayUI(GameDataConfig.AudioIdType.TestButtonClick);
            _onConfirm?.Invoke();
            ModuleRegistry.Get<IUIManager>().GoBack();
        }
        #endregion
    }
}
