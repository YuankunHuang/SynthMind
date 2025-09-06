using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using YuankunHuang.Unity.Core;
using YuankunHuang.Unity.ModuleCore;

namespace YuankunHuang.Unity.UICore
{
    public abstract class WindowControllerBase
    {
        protected string WindowName { get; private set; }
        protected GeneralWindowConfig Config { get; private set; }
        protected WindowAttributeData AttributeData { get; private set; }

        private RenderTexture _blurRenderTex;
        private Coroutine _currentAnimationCoroutine;

        public void Init(WindowStackEntry entry, RenderTexture blurTexture = null)
        {
            LogHelper.Log($"[WindowControllerBase]::Init: {entry.WindowName}");

            WindowName = entry.WindowName;
            Config = entry.WindowGO.GetComponent<GeneralWindowConfig>();
            AttributeData = entry.AttributeDataHandle.Result;

            // attributes
            if (AttributeData != null)
            {
                if (AttributeData.usePopupAnimation)
                {
                    _currentAnimationCoroutine = MonoManager.Instance.StartCoroutine(PopupAnimator.AnimatePopupEnter(Config.transform, Config.CanvasGroup, AttributeData.animationSettings));
                }

                if (AttributeData.hasMask)
                {
                    CreateMask();
                }

                if (AttributeData.useBlurredBackground && blurTexture != null)
                {
                    CreateBlurBackground(blurTexture);
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

            rt.SetParent(Config.transform);
            rt.SetAsFirstSibling();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 1);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            rt.localPosition = Vector3.zero;
        }

        private async void CreateBlurBackground(RenderTexture blurTexture)
        {
            _blurRenderTex = blurTexture;

            var blurGO = new GameObject("Blur");
            var rt = blurGO.AddComponent<RectTransform>();
            var img = blurGO.AddComponent<RawImage>();
            img.raycastTarget = false;
            img.texture = _blurRenderTex;

            rt.SetParent(Config.transform);
            rt.SetAsFirstSibling();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 1);
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            var mat = await ResManager.LoadAssetAsync<Material>(AddressablePaths.UIBoxBlurMaterial);
            if (mat != null)
            {
                img.material = mat;
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
                    _currentAnimationCoroutine = MonoManager.Instance.StartCoroutine(PopupAnimator.AnimatePopupExit(Config.transform, Config.CanvasGroup, AttributeData.animationSettings));
                }
            }

            yield return new WaitForSeconds(delay);
            OnHide(state);
        }
        public void Dispose()
        {
            LogHelper.Log($"[WindowControllerBase]::Dispose: {WindowName}");

            if (AttributeData.useBlurredBackground)
            {
                ResManager.Release(AddressablePaths.UIBoxBlurMaterial);
            }
            if (_blurRenderTex != null)
            {
                _blurRenderTex.Release();
                UnityEngine.Object.Destroy(_blurRenderTex);
                _blurRenderTex = null;
            }

            if (_currentAnimationCoroutine != null)
            {
                MonoManager.Instance.StopCoroutine(_currentAnimationCoroutine);
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