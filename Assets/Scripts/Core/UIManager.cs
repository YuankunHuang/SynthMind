using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using YuankunHuang.Unity.Core;
using YuankunHuang.Unity.Util;

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

    public struct WindowStackEntry : IWindowStackEntry
    {
        public string WindowName;
        public WindowControllerBase Controller;
        public IWindowData Data;
        public GameObject WindowGO;
        public AsyncOperationHandle<WindowAttributeData> AttributeDataHandle;
        public AsyncOperationHandle<GameObject> WindowHandle;

        public WindowStackEntry(string windowName, WindowControllerBase controller, IWindowData data, GameObject windowGO, AsyncOperationHandle<WindowAttributeData> attributeDataHandle, AsyncOperationHandle<GameObject> windowHandle)
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
        private readonly Stack<WindowStackEntry> _windowStack = new();
        private readonly List<WindowStackEntry> _pendingDestroyWindows = new();

        public bool IsInitialized { get; private set; } = false;

        public Transform StackableRoot
        {
            get
            {
                if (_stackableRoot == null)
                {
                    _stackableRoot = GameObject.FindGameObjectWithTag(TagNames.StackableRoot).transform;
                }
                return _stackableRoot;
            }
        }
        private Transform _stackableRoot;

        public UIManager()
        {
            IsInitialized = true;
            LogHelper.Log("UIManager initialized");
        }

        #region Interfaces
        public void ShowStackableWindow(string windowName, IWindowData data = null)
        {
            MonoManager.Instance.StartCoroutine(ShowStackableWindowCoroutine(windowName, data));
        }

        private IEnumerator ShowStackableWindowCoroutine(string windowName, IWindowData data = null)
        {
            InputBlocker.StartBlocking();

            try
            {
                // Load prefab
                var key = string.Format(AddressablePaths.StackableWindow, windowName, windowName);
                var handle = Addressables.LoadAssetAsync<GameObject>(key);
                yield return handle;

                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    LogHelper.LogError($"[UIManager]::LoadStackableWindowPrefabAsync: Failed to load prefab: {key}, Exception: {handle.OperationException}");
                    Addressables.Release(handle);
                    yield break;
                }

                // Load Attribute Data
                var attrKey = string.Format(AddressablePaths.WindowAttributeData, windowName);
                var attrHandle = Addressables.LoadAssetAsync<WindowAttributeData>(attrKey);
                yield return attrHandle;

                // Instantiate window
                var windowGO = GameObject.Instantiate(handle.Result, StackableRoot);
                windowGO.transform.SetAsLastSibling();

                // Create window controller
                var controllerTypeName = $"{Namespaces.HotUpdate}.{windowName}Controller";
                var controllerType = TypeUtil.GetType(controllerTypeName);
                if (controllerType == null)
                {
                    LogHelper.LogError($"[UIManager]::CreateWindowController: Controller type not found: {controllerTypeName}");
                    yield break;
                }
                var controller = Activator.CreateInstance(controllerType) as WindowControllerBase;
                if (controller == null)
                {
                    LogHelper.LogError($"[UIManager]::CreateWindowController: Failed to create controller: {controllerTypeName}");
                    yield break;
                }

                if (_windowStack.Count > 0)
                {
                    var top = _windowStack.Peek();
                    if (top.AttributeDataHandle.Result.selfDestructOnCovered)
                    {
                        ReleaseEntryImmediate(top);
                        _windowStack.Pop();
                    }

                    if (attrHandle.Result.hasMask)
                    {
                        top.Controller.Hide(WindowHideState.Covered, 0);
                    }
                }

                // Push to stack and show
                var entry = new WindowStackEntry(windowName, controller, data, windowGO, attrHandle, handle);
                controller.Init(entry);
                controller.Show(data, WindowShowState.New);
                _windowStack.Push(entry);
            }
            finally
            {
                InputBlocker.StopBlocking();
            }
        }

        public IWindowStackEntry GetWindowOnTop()
        {
            return _windowStack.Count > 0 ? _windowStack.Peek() : null;
        }

        public bool IsWindowInStack(string windowName)
        {
            foreach (var entry in _windowStack)
            {
                if (entry.WindowName == windowName)
                {
                    return true;
                }
            }

            return false;
        }

        public void GoBack()
        {
            if (_windowStack.Count < 1)
            {
                LogHelper.LogError($"[UIManager]::GoBack: Cannot go back when there is no window in stack.");
                return;
            }

            MonoManager.Instance.StartCoroutine(GoBackCoroutine());
        }

        private IEnumerator GoBackCoroutine()
        {
            InputBlocker.StartBlocking();

            try
            {
                var entry = _windowStack.Pop();
                yield return MonoManager.Instance.StartCoroutine(ReleaseEntryWithAnimation(entry));

                if (_windowStack.Count > 0)
                {
                    var top = _windowStack.Peek();
                    top.Controller.Show(top.Data, WindowShowState.Uncovered);
                }
            }
            finally
            {
                InputBlocker.StopBlocking();
            }

        }

        public void GoBackTo(string windowName)
        {
            if (_windowStack.Count < 1)
            {
                LogHelper.LogError($"[UIManager]::GoBackTo: No window in stack.");
                return;
            }

            var found = false;
            foreach (var entry in _windowStack)
            {
                if (entry.WindowName == windowName)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                LogHelper.LogError($"[UIManager]::GoBackTo: Target window not found in stack: {windowName}");
                return;
            }

            MonoManager.Instance.StartCoroutine(GoBackToCoroutine(windowName));
        }

        private IEnumerator GoBackToCoroutine(string windowName)
        {
            InputBlocker.StartBlocking();

            try
            {
                while (_windowStack.Count > 0)
                {
                    var entry = _windowStack.Peek();
                    if (entry.WindowName == windowName)
                    {
                        entry.Controller.Show(entry.Data, WindowShowState.Uncovered);
                        break;
                    }

                    entry = _windowStack.Pop();
                    yield return MonoManager.Instance.StartCoroutine(ReleaseEntryWithAnimation(entry));
                }
            }
            finally
            {
                InputBlocker.StopBlocking();
            }
        }

        private IEnumerator ReleaseEntryWithAnimation(WindowStackEntry entry)
        {
            _pendingDestroyWindows.Add(entry);

            var hasExitAnimation = entry.AttributeDataHandle.IsValid() && entry.AttributeDataHandle.Result.usePopupAnimation;
            if (hasExitAnimation)
            {
                var animationSettings = entry.AttributeDataHandle.Result.animationSettings;
                entry.Controller.Hide(WindowHideState.Removed, animationSettings.exitDuration);

                yield return new WaitForSeconds(animationSettings.exitDuration);
            }
            else
            {
                entry.Controller.Hide(WindowHideState.Removed, 0);
            }

            ReleaseEntryImmediate(entry);

            _pendingDestroyWindows.Remove(entry);
        }

        public void Dispose()
        {
            foreach (var entry in _windowStack)
            {
                ReleaseEntryImmediate(entry);
            }
            _windowStack.Clear();

            IsInitialized = false;

            LogHelper.Log("UIManager disposed");
        }
        #endregion

        private void ReleaseEntryImmediate(WindowStackEntry entry)
        {
            if (entry.WindowHandle.IsValid())
            {
                Addressables.Release(entry.WindowHandle);
            }
            if (entry.AttributeDataHandle.IsValid())
            {
                Addressables.Release(entry.AttributeDataHandle);
            }
            
            entry.Controller?.Dispose();
            
            if (entry.WindowGO != null)
            {
                GameObject.Destroy(entry.WindowGO);
            }
        }
    }
}