using System.Collections;
using System.Collections.Generic;
using TMPro;
using YuankunHuang.Unity.Core;
using YuankunHuang.Unity.Util;
using System;

namespace YuankunHuang.Unity.HotUpdate
{
    public class MainMenuController : WindowControllerBase
    {
        private enum Panel
        {
            None,
            Home,
            Chat,
            Sandbox,
            Setting,
            About,
        }

        #region UI Ref
        private enum ExtraTMP
        {
            Box = 0,
        }

        private TMP_Text _boxTxt;
        #endregion

        #region Field
        private Panel _currentPanel = Panel.None;
        #endregion

        #region Lifecycle
        protected override void OnInit()
        {
            _boxTxt = Config.ExtraTextMeshProList[(int)ExtraTMP.Box];
        }

        protected override void OnShow(IWindowData data, WindowShowState state)
        {
            if (state == WindowShowState.New)
            {
                ShowHome(true);
            }
            else
            {
                switch (_currentPanel)
                {
                    case Panel.Home:
                        ShowHome(true);
                        break;
                    case Panel.Chat:
                        ShowChat(true); 
                        break;
                    case Panel.Sandbox:
                        ShowSandbox(true);
                        break;
                    case Panel.Setting:
                        ShowSetting(true);
                        break;
                    case Panel.About:
                        ShowAbout(true);
                        break;
                    default:
                        LogHelper.LogError($"Undefined panel: {_currentPanel}");
                        break;
                }
            }

            Config.CanvasGroup.CanvasGroupOn();
        }

        protected override void OnHide(WindowHideState state)
        {
            Config.CanvasGroup.CanvasGroupOff();
        }

        protected override void OnDispose()
        {
        }
        #endregion

        #region Content
        private void ShowHome(bool forceShow)
        {
            if (_currentPanel == Panel.Home && !forceShow)
            {
                return;
            }

            _currentPanel = Panel.Home;
            _boxTxt.text = "Welcome to SynthMind, an AI interaction sandbox based on Unity, demonstrating modular UI, AI dialogue, event tracking and physical system integration capabilities";
        }

        private void ShowChat(bool forceShow)
        {
            throw new NotImplementedException("ShowChat not implemented");
        }

        private void ShowSandbox(bool forceShow)
        {
            throw new NotImplementedException("ShowSandbox not implemented");
        }

        private void ShowSetting(bool forceShow)
        {
            throw new NotImplementedException("ShowSetting not implemented");
        }

        private void ShowAbout(bool forceShow)
        {
            throw new NotImplementedException("ShowAbout not implemented");
        }
        #endregion
    }
}