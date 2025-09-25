using UnityEngine;
using YuankunHuang.Unity.Core;
using YuankunHuang.Unity.ModuleCore;
using YuankunHuang.Unity.CameraCore;
using System.Collections;

namespace YuankunHuang.Unity.UICore
{
    public static class UIScreenCapture
    {
        public static IEnumerator CaptureFullFrameCoroutine(System.Action<RenderTexture> onComplete)
        {
            yield return new WaitForEndOfFrame();

            var rt = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
            rt.Create();

#if UNITY_2021_2_OR_NEWER
            ScreenCapture.CaptureScreenshotIntoRenderTexture(rt);
#else
            var tex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGBA32, false);
            tex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            tex.Apply();
            Graphics.Blit(tex, rt);
            UnityEngine.Object.Destroy(tex);
#endif

            onComplete?.Invoke(rt);
        }

    }
}