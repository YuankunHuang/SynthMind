using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using YuankunHuang.Unity.Core;

namespace YuankunHuang.Unity.UICore
{
    public enum WindowShowState
    {
        New,
        Uncovered,
    }

    public enum WindowHideState
    {
        Removed,
        Covered,
    }

    public struct WindowStackEntry
    {
        public string WindowName;
        public WindowControllerBase Controller;
        public IWindowData Data;
        public GameObject WindowGO;
        public AsyncOperationHandle<WindowAttributeData> AttributeDataHandle;
        public AsyncOperationHandle<GameObject> WindowHandle;

        public WindowStackEntry(string windowName, WindowControllerBase controller, IWindowData data, 
            GameObject windowGO, AsyncOperationHandle<WindowAttributeData> attributeDataHandle, 
            AsyncOperationHandle<GameObject> windowHandle)
        {
            WindowName = windowName;
            Controller = controller;
            Data = data;
            WindowGO = windowGO;
            AttributeDataHandle = attributeDataHandle;
            WindowHandle = windowHandle;
        }
    }

    public class UIManager : IUIManager
    {
        public bool IsInitialized { get; private set; } = false;

        private readonly Stack<Window> _stack = new();
        private readonly WindowLoader _loader;
        
        public string Current => _stack.Count > 0 ? _stack.Peek().Name : null;

        public UIManager()
        {
            var root = GameObject.FindGameObjectWithTag(TagNames.StackableRoot).transform;
            _loader = new WindowLoader(root);

            IsInitialized = true;
        }

        public void Dispose()
        {
            foreach (var window in _stack)
            {
                window.Dispose();
            }
            _stack.Clear();
            IsInitialized = false;
        }

        public void Show(string windowName, IWindowData data = null)
        {
            _ = ShowAsync(windowName, data);
        }
        
        private async Task ShowAsync(string windowName, IWindowData data)
        {
            using (new InputBlock())
            {
                var window = await _loader.LoadAsync(windowName);
                var blurTexture = CaptureBlurIfNeeded(window.Attributes);
                
                HandleCurrentWindow(window.Attributes);
                window.Show(data, WindowShowState.New, blurTexture);
                _stack.Push(window);
            }
        }
        
        private RenderTexture CaptureBlurIfNeeded(WindowAttributeData attrs)
        {
            if (!attrs.useBlurredBackground) return null;
            
            var texture = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
            ScreenCapture.CaptureScreenshotIntoRenderTexture(texture);
            return texture;
        }
        
        private void HandleCurrentWindow(WindowAttributeData newAttrs)
        {
            if (_stack.Count == 0) return;
            
            var current = _stack.Peek();
            if (current.Attributes.selfDestructOnCovered)
            {
                _stack.Pop().Dispose();
            }
            else if (newAttrs.hasMask)
            {
                current.Hide(WindowHideState.Covered);
            }
        }

        public bool Contains(string windowName)
        {
            foreach (var window in _stack)
            {
                if (window.Name == windowName) return true;
            }
            return false;
        }

        public void GoBack()
        {
            if (_stack.Count == 0) return;
            _ = GoBackAsync();
        }
        
        private async Task GoBackAsync()
        {
            using (new InputBlock())
            {
                var window = _stack.Pop();
                await HideWithAnimation(window);
                window.Dispose();
                
                if (_stack.Count > 0)
                {
                    _stack.Peek().Show(null, WindowShowState.Uncovered);
                }
            }
        }

        public void GoBackTo(string windowName)
        {
            if (!Contains(windowName)) return;
            _ = GoBackToAsync(windowName);
        }
        
        private async Task GoBackToAsync(string windowName)
        {
            using (new InputBlock())
            {
                while (_stack.Count > 0)
                {
                    var window = _stack.Peek();
                    if (window.Name == windowName)
                    {
                        window.Show(window.Data, WindowShowState.Uncovered);
                        break;
                    }
                    
                    window = _stack.Pop();
                    await HideWithAnimation(window);
                    window.Dispose();
                }
            }
        }

        private async Task HideWithAnimation(Window window)
        {
            var duration = window.Attributes.usePopupAnimation ? window.Attributes.animationSettings.exitDuration : 0;
            window.Hide(WindowHideState.Removed, duration);
            
            if (duration > 0)
            {
                await Task.Delay((int)(duration * 1000));
            }
        }
    }
    
    public class InputBlock : IDisposable
    {
        public InputBlock() => InputBlocker.StartBlocking();
        public void Dispose() => InputBlocker.StopBlocking();
    }
}