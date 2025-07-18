using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace YuankunHuang.Unity.Core
{
    public abstract class WindowControllerBase
    {
        protected string WindowName { get; private set; }
        protected GeneralWindowConfig Config { get; private set; }
        protected WindowAttributeData AttributeData { get; private set; }

        private RenderTexture _blurRenderTex;

        public void Init(WindowStackEntry entry)
        {
            LogHelper.Log($"[WindowControllerBase]::Init: {entry.WindowName}");

            WindowName = entry.WindowName;
            Config = entry.WindowGO.GetComponent<GeneralWindowConfig>();
            AttributeData = entry.AttributeDataHandle.Result;

            // attributes
            if (AttributeData != null)
            {
                if (AttributeData.hasMask)
                {
                    var maskGO = new GameObject("Mask");
                    var rt = maskGO.AddComponent<RectTransform>();
                    var img = maskGO.AddComponent<Image>();
                    img.color = new Color(0, 0, 0, 0);
                    img.raycastTarget = true;

                    rt.SetParent(Config.transform);
                    rt.SetAsFirstSibling();
                    rt.anchorMin = new Vector2(0, 0);
                    rt.anchorMax = new Vector2(1, 1);
                    rt.offsetMin = rt.offsetMax = Vector2.zero;
                }

                if (AttributeData.useBlurredBackground)
                {
                    LoadBlurAsync();
                }

                if (AttributeData.usePopupScaleAnimation)
                {
                    MonoManager.Instance.StartCoroutine(PopupScaleIn());
                }
            }

            OnInit();
        }

        private async void LoadBlurAsync()
        {
            await Task.Yield();

            _blurRenderTex = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
            ScreenCapture.CaptureScreenshotIntoRenderTexture(_blurRenderTex);

            var blurGO = new GameObject("Blur");
            var rt = blurGO.AddComponent<RectTransform>();
            var img = blurGO.AddComponent<RawImage>();
            img.raycastTarget = true;
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
            else
            {
                LogHelper.LogError($"[WindowControllerBase]::LoadBlurAsync: Failed to load blur material.");
            }
        }

        private IEnumerator PopupScaleIn()
        {
            float time = 0f;
            float duration = AttributeData.popupScaleDuration;
            var start = Vector3.zero;
            var end = Vector3.one;
            Config.transform.localScale = start;
            while (time < duration)
            {
                time += Time.deltaTime;
                float t = Mathf.Clamp01(time / duration);
                Config.transform.localScale = Vector3.LerpUnclamped(start, end, t);
                yield return null;
            }
            Config.transform.localScale = end;
        }

        public void Show(IWindowData data, WindowShowState state)
        {
            LogHelper.Log($"[WindowControllerBase]::Show: {WindowName}");
            OnShow(data, state);
        }
        public void Hide(WindowHideState state)
        {
            LogHelper.Log($"[WindowControllerBase]::Hide: {WindowName}");
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

            OnDispose();
        }

        protected virtual void OnInit() { }
        protected virtual void OnShow(IWindowData data, WindowShowState state) { }
        protected virtual void OnHide(WindowHideState state) { }
        protected virtual void OnDispose() { }
    }
}