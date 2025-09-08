using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using YuankunHuang.Unity.Core;

namespace YuankunHuang.Unity.UICore
{
    public class Window : IDisposable
    {
        public string Name { get; }
        public WindowControllerBase Controller { get; }
        public WindowAttributeData Attributes { get; }
        public GameObject GameObject { get; }
        public IWindowData Data { get; private set; }
        
        private readonly string _prefabKey;
        private readonly string _attributeDataKey;

        public Window(string name, WindowControllerBase controller, WindowAttributeData attributes, 
            GameObject gameObject, string prefabKey, string attributeDataKey)
        {
            Name = name;
            Controller = controller;
            Attributes = attributes;
            GameObject = gameObject;
            _prefabKey = prefabKey;
            _attributeDataKey = attributeDataKey;
        }

        public void Init(RenderTexture blurTexture = null) 
        {
            Controller.Init(Name, GameObject, Attributes, blurTexture);
        }

        public void Show(IWindowData data, WindowShowState state)
        {
            Data = data;
            Controller.Show(data, state);
        }

        public void Hide(WindowHideState state, float delay = 0)
        {
            Controller.Hide(state, delay);
        }

        public void Dispose()
        {
            Controller?.Dispose();

            if (GameObject != null)
            {
                UnityEngine.Object.Destroy(GameObject);
            }
            
            if (!string.IsNullOrEmpty(_prefabKey))
            {
                ResManager.Release(_prefabKey);
            }

            if (!string.IsNullOrEmpty(_attributeDataKey))
            {
                ResManager.Release(_attributeDataKey);
            }
        }
    }
}