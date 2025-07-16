using System;
using System.Collections;
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

        public void Init(WindowStackEntry entry)
        {
            LogHelper.Log($"[WindowControllerBase]::Init: {entry.WindowName}");

            WindowName = entry.WindowName;
            Config = entry.WindowGO.GetComponent<GeneralWindowConfig>();
            AttributeData = entry.AttributeData;

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
                    MonoManager.Instance.StartCoroutine(LoadBlurCoroutine());
                }
            }

            OnInit();
        }

        private IEnumerator LoadBlurCoroutine()
        {
            var blurGO = new GameObject("Blur");
            var rt = blurGO.AddComponent<RectTransform>();
            var img = blurGO.AddComponent<RawImage>();
            img.raycastTarget = true;

            rt.SetParent(Config.transform);
            rt.SetAsFirstSibling();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 1);
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            var handle = ResManager.LoadAssetAsync<Material>(AddressablePaths.UIBoxBlurMaterial);
            yield return handle;
            var mat = handle.Result;
            if (mat != null)
            {
                img.material = mat;
            }
            else
            {
                LogHelper.LogError($"[WindowControllerBase] Failed to load blur material!");
            }
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

            OnDispose();
        }

        protected virtual void OnInit() { }
        protected virtual void OnShow(IWindowData data, WindowShowState state) { }
        protected virtual void OnHide(WindowHideState state) { }
        protected virtual void OnDispose() { }
    }
}