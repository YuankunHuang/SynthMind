using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using YuankunHuang.Unity.Core;
using YuankunHuang.Unity.Core.Debug;

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
            _ = ShowAsync(windowName, data).WithLogging();
        }

        private async Task ShowAsync(string windowName, IWindowData data)
        {
            using (new InputBlock())
            {
                try
                {
                    // Load attributes first to check if blur is needed
                    var attributes = await _loader.LoadAttributeDataAsync(windowName).WithLogging();

                    // Capture screen using Coroutine for proper frame timing
                    var blurTexture = await CaptureBlurIfNeededAsync(attributes).WithLogging();

                    // Load window using async
                    var window = await _loader.LoadAsync(windowName).WithLogging();

                    HandleCurrentWindow(window.Attributes);
                    window.Init(blurTexture);
                    window.Show(data, WindowShowState.New);
                    _stack.Push(window);
                }
                catch (System.Exception ex)
                {
                    LogHelper.LogError($"[UIManager] ShowAsync failed for {windowName}: {ex.Message}");
                    LogHelper.LogError($"[UIManager] Stack trace: {ex.StackTrace}");
                }
            }
        }
        
        private Task<RenderTexture> CaptureBlurIfNeededAsync(WindowAttributeData attrs)
        {
            if (!attrs.useBlurredBackground) 
                return Task.FromResult<RenderTexture>(null);
            
            var tcs = new TaskCompletionSource<RenderTexture>();
            MonoManager.Instance.StartCoroutine(CaptureBlurCoroutine(tcs));
            return tcs.Task;
        }

        private IEnumerator CaptureBlurCoroutine(TaskCompletionSource<RenderTexture> tcs)
        {
            yield return UIScreenCapture.CaptureFullFrameCoroutine(rt =>
            {
                tcs.TrySetResult(rt);
            });
        }

        private void HandleCurrentWindow(WindowAttributeData newAttrs)
        {
            if (_stack.Count == 0) return;
            
            var current = _stack.Peek();
            if (current.Attributes.selfDestructOnCovered)
            {
                current = _stack.Pop();
                current.Hide(WindowHideState.Removed);
                current.Dispose();
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
                try
                {
                    var window = _stack.Pop();
                    await HideWithAnimationAsync(window);
                    window.Dispose();

                    if (_stack.Count > 0)
                    {
                        var top = _stack.Peek();
                        top.Show(top.Data, WindowShowState.Uncovered);
                    }
                }
                catch (System.Exception ex)
                {
                    LogHelper.LogError($"[UIManager] GoBackAsync failed: {ex.Message}");
                    LogHelper.LogError($"[UIManager] Stack trace: {ex.StackTrace}");
                }
            }
        }

        public void GoBackTo(string windowName, IWindowData data = null)
        {
            if (!Contains(windowName)) return;
            _ = GoBackToAsync(windowName, data).WithLogging();
        }
        
        private async Task GoBackToAsync(string windowName, IWindowData data)
        {
            using (new InputBlock())
            {
                while (_stack.Count > 0)
                {
                    var window = _stack.Peek();
                    if (window.Name == windowName)
                    {
                        if (data != null)
                        {
                            window.Show(data, WindowShowState.Uncovered);
                        }
                        else
                        {
                            window.Show(window.Data, WindowShowState.Uncovered);
                        }
                        break;
                    }
                    
                    window = _stack.Pop();
                    await HideWithAnimationAsync(window).WithLogging();
                    window.Dispose();
                }
            }
        }

        private async Task HideWithAnimationAsync(Window window)
        {
            var duration = window.Attributes.usePopupAnimation ? window.Attributes.animationSettings.exitDuration : 0;
            window.Hide(WindowHideState.Removed, duration);

            if (duration > 0)
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                // Use coroutine-based delay for WebGL compatibility
                var tcs = new TaskCompletionSource<bool>();
                MonoManager.Instance.StartCoroutine(DelayCoroutine(duration, tcs));
                await tcs.Task;
#else
                await Task.Delay((int)(duration * 1000));
#endif
            }
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        private IEnumerator DelayCoroutine(float duration, TaskCompletionSource<bool> tcs)
        {
            yield return new WaitForSeconds(duration);
            tcs.TrySetResult(true);
        }
#endif
    }
    
    public class InputBlock : IDisposable
    {
        public InputBlock() => InputBlocker.StartBlocking();
        public void Dispose() => InputBlocker.StopBlocking();
    }
}