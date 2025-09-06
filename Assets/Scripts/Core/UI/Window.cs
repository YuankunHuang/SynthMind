using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace YuankunHuang.Unity.UICore
{
    public class Window : IDisposable
    {
        public string Name { get; }
        public WindowControllerBase Controller { get; }
        public WindowAttributeData Attributes { get; }
        public GameObject GameObject { get; }
        public IWindowData Data { get; private set; }
        
        private readonly AsyncOperationHandle<GameObject> _prefabHandle;
        private readonly AsyncOperationHandle<WindowAttributeData> _attrHandle;

        public Window(string name, WindowControllerBase controller, WindowAttributeData attributes, 
            GameObject gameObject, AsyncOperationHandle<GameObject> prefabHandle, 
            AsyncOperationHandle<WindowAttributeData> attrHandle)
        {
            Name = name;
            Controller = controller;
            Attributes = attributes;
            GameObject = gameObject;
            _prefabHandle = prefabHandle;
            _attrHandle = attrHandle;
        }

        public void Show(IWindowData data, WindowShowState state, RenderTexture blurTexture = null)
        {
            Data = data;
            var entry = new WindowStackEntry(Name, Controller, data, GameObject, default, default);
            Controller.Init(entry, blurTexture);
            Controller.Show(data, state);
        }

        public void Hide(WindowHideState state, float delay = 0)
        {
            Controller.Hide(state, delay);
        }

        public void Dispose()
        {
            Controller?.Dispose();
            if (GameObject != null) UnityEngine.Object.Destroy(GameObject);
            
            if (_prefabHandle.IsValid()) Addressables.Release(_prefabHandle);
            if (_attrHandle.IsValid()) Addressables.Release(_attrHandle);
        }
    }
}