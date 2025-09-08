using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using YuankunHuang.Unity.Core;
using YuankunHuang.Unity.Core.Debug;
using YuankunHuang.Unity.ModuleCore;

namespace YuankunHuang.Unity.UICore
{
    public abstract class WindowControllerBase
    {
        protected string WindowName { get; private set; }
        protected GeneralWindowConfig Config { get; private set; }
        protected WindowAttributeData AttributeData { get; private set; }

        private BlurBackground _blurBackground;
        private Coroutine _currentAnimationCoroutine;

        public void Init(string windowName, GameObject windowGO, WindowAttributeData attributeData, RenderTexture blurTexture = null)
        {
            LogHelper.Log($"[WindowControllerBase]::Init: {windowName}");

            WindowName = windowName;
            Config = windowGO.GetComponent<GeneralWindowConfig>();
            AttributeData = attributeData;

            // attributes
            if (AttributeData != null)
            {
                if (AttributeData.usePopupAnimation)
                {
                    _currentAnimationCoroutine = MonoManager.Instance.StartCoroutine(WindowAnimation.PlayEnter(Config.Root, Config.CanvasGroup, AttributeData.animationSettings));
                }

                if (AttributeData.hasMask)
                {
                    CreateMask();
                }

                if (AttributeData.useBlurredBackground && blurTexture != null)
                {
                    _ = CreateBlurBackgroundAsync(blurTexture);
                }
            }

            OnInit();
        }

        private void CreateMask()
        {
            var maskGO = new GameObject("Mask");
            var rt = maskGO.AddComponent<RectTransform>();
            var img = maskGO.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 1);
            img.raycastTarget = true;

            rt.SetParent(Config.Root);
            rt.SetAsFirstSibling();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 1);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            rt.localPosition = Vector3.zero;
            rt.localScale = Vector3.one;
        }

        private async Task CreateBlurBackgroundAsync(RenderTexture blurTexture)
        {
            _blurBackground = new BlurBackground();
            var success = await _blurBackground.CreateAsync(Config.transform, blurTexture).WithLogging();
            
            if (!success)
            {
                _blurBackground = null;
                LogHelper.LogError($"[WindowControllerBase] Failed to create blur background for {WindowName}");
            }
        }

        public void Show(IWindowData data, WindowShowState state)
        {
            LogHelper.Log($"[WindowControllerBase]::Show: {WindowName}");
            OnShow(data, state);
        }
        public void Hide(WindowHideState state, float delay)
        {
            LogHelper.Log($"[WindowControllerBase]::Hide: {WindowName}");
            MonoManager.Instance.StartCoroutine(HideCoroutine(state, delay));
        }
        private IEnumerator HideCoroutine(WindowHideState state, float delay)
        {
            if (AttributeData != null)
            {
                if (AttributeData.usePopupAnimation)
                {
                    _currentAnimationCoroutine = MonoManager.Instance.StartCoroutine(WindowAnimation.PlayExit(Config.Root, Config.CanvasGroup, AttributeData.animationSettings));
                }
            }

            yield return new WaitForSeconds(delay);
            OnHide(state);
        }
        public void Dispose()
        {
            LogHelper.Log($"[WindowControllerBase]::Dispose: {WindowName}");

            _blurBackground?.Dispose();
            _blurBackground = null;

            if (_currentAnimationCoroutine != null)
            {
                if (MonoManager.Instance != null)
                {
                    MonoManager.Instance.StopCoroutine(_currentAnimationCoroutine);
                }
                _currentAnimationCoroutine = null;
            }

            OnDispose();
        }

        protected virtual void OnInit() { }
        protected virtual void OnShow(IWindowData data, WindowShowState state) { }
        protected virtual void OnHide(WindowHideState state) { }
        protected virtual void OnDispose() { }
    }
}