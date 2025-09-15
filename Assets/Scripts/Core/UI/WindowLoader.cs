using System;
using System.Threading.Tasks;
using UnityEngine;
using YuankunHuang.Unity.Core;
using YuankunHuang.Unity.Core.Debug;
using YuankunHuang.Unity.Util;

namespace YuankunHuang.Unity.UICore
{
    public class WindowLoader
    {
        private readonly Transform _root;

        public WindowLoader(Transform root)
        {
            _root = root;
        }

        public async Task<WindowAttributeData> LoadAttributeDataAsync(string windowName)
        {
            var attrKey = string.Format(AddressablePaths.WindowAttributeData, windowName);
            var tAttributeData = ResManager.LoadAssetAsync<WindowAttributeData>(attrKey).WithLogging();
            var result = await tAttributeData;

            // Release immediately since we only need to check the data
            ResManager.Release(attrKey);
            return result;
        }

        public async Task<Window> LoadAsync(string windowName)
        {
            try
            {
                var prefabKey = string.Format(AddressablePaths.StackableWindow, windowName, windowName);
                var attrKey = string.Format(AddressablePaths.WindowAttributeData, windowName);

                var tPrefab = ResManager.LoadAssetAsync<GameObject>(prefabKey).WithLogging();
                var tAttributeData = ResManager.LoadAssetAsync<WindowAttributeData>(attrKey).WithLogging();

                await Task.WhenAll(tPrefab, tAttributeData);

                var gameObject = UnityEngine.Object.Instantiate(tPrefab.Result, _root);
                gameObject.transform.SetAsLastSibling();
                
                var controller = CreateController(windowName);
                
                return new Window(windowName, controller, tAttributeData.Result, gameObject, prefabKey, attrKey);
            }
            catch (Exception ex)
            {
                LogHelper.LogError($"[WindowLoader] Failed to load window {windowName}: {ex.Message}");
                throw;
            }
        }

        private WindowControllerBase CreateController(string windowName)
        {
            LogHelper.Log($"[WindowLoader] Creating controller for {windowName}");

#if UNITY_WEBGL && !UNITY_EDITOR
            // Use factory for WebGL to avoid reflection issues
            return WindowControllerFactory.CreateController(windowName);
#else
            // Use reflection for other platforms (Windows, etc.)
            var type = TypeUtil.GetType($"{Namespaces.HotUpdate}.{windowName}Controller");
            return (WindowControllerBase)Activator.CreateInstance(type
                ?? throw new Exception($"Controller not found: {windowName}Controller"));
#endif
        }
    }
}