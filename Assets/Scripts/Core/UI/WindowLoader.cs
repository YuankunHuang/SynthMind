using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using YuankunHuang.Unity.Core;
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
            var attrHandle = Addressables.LoadAssetAsync<WindowAttributeData>(attrKey);
            var result = await attrHandle.Task;
            
            // Release immediately since we only need to check the data
            Addressables.Release(attrHandle);
            return result;
        }

        public async Task<Window> LoadAsync(string windowName)
        {
            try
            {
                var prefabKey = string.Format(AddressablePaths.StackableWindow, windowName, windowName);
                var attrKey = string.Format(AddressablePaths.WindowAttributeData, windowName);

                var prefabHandle = Addressables.LoadAssetAsync<GameObject>(prefabKey);
                var attrHandle = Addressables.LoadAssetAsync<WindowAttributeData>(attrKey);

                await Task.WhenAll(prefabHandle.Task, attrHandle.Task);

                var gameObject = UnityEngine.Object.Instantiate(prefabHandle.Result, _root);
                gameObject.transform.SetAsLastSibling();
                
                var controller = CreateController(windowName);
                
                return new Window(windowName, controller, attrHandle.Result, gameObject, prefabHandle, attrHandle);
            }
            catch (Exception ex)
            {
                LogHelper.LogError($"[WindowLoader] Failed to load window {windowName}: {ex.Message}");
                throw;
            }
        }

        private WindowControllerBase CreateController(string windowName)
        {
            var type = TypeUtil.GetType($"{Namespaces.HotUpdate}.{windowName}Controller");
            return (WindowControllerBase)Activator.CreateInstance(type 
                ?? throw new Exception($"Controller not found: {windowName}Controller"));
        }
    }
}