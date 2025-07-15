using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using YuankunHuang.Unity.Util;

namespace YuankunHuang.Unity.Core
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

    public abstract class WindowControllerBase
    {
        protected string WindowName { get; private set; }
        protected GeneralWindowConfig Config { get; private set; }

        public void Init(WindowStackEntry entry)
        {
            LogHelper.Log($"[WindowControllerBase]::Init: {entry.WindowName}");

            WindowName = entry.WindowName;
            Config = entry.Handle.Result.GetComponent<GeneralWindowConfig>();

            OnInit();
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
            OnDispose();
        }

        protected virtual void OnInit() { }
        protected virtual void OnShow(IWindowData data, WindowShowState state) { }
        protected virtual void OnHide(WindowHideState state) { }
        protected virtual void OnDispose() { }
    }

    public struct WindowStackEntry
    {
        public string WindowName;
        public WindowControllerBase Controller;
        public IWindowData Data;
        public AsyncOperationHandle<GameObject> Handle;

        public WindowStackEntry(string windowName, WindowControllerBase controller, IWindowData data, AsyncOperationHandle<GameObject> handle)
        {
            WindowName = windowName;
            Controller = controller;
            Data = data;
            Handle = handle;
        }
    }

    public class UIManager : IUIManager
    {
        private readonly Stack<WindowStackEntry> _windowStack = new();

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

        #region Interfaces
        public void ShowStackableWindow(string windowName, IWindowData data = null)
        {
            ShowStackableWindowAsync(windowName, data).GetAwaiter().GetResult();
        }

        public WindowStackEntry? GetWindowOnTop()
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

            var entry = _windowStack.Pop();
            entry.Controller.Hide(WindowHideState.Removed);
            entry.Controller.Dispose();

            if (entry.Handle.IsValid())
            {
                Addressables.Release(entry.Handle);
            }

            if (_windowStack.Count > 0)
            {
                var top = _windowStack.Peek();
                top.Controller.Show(top.Data, WindowShowState.Uncovered);
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

            while (_windowStack.Count > 0)
            {
                var entry = _windowStack.Peek();
                if (entry.WindowName == windowName)
                {
                    entry.Controller.Show(entry.Data, WindowShowState.Uncovered);
                    break;
                }

                entry = _windowStack.Pop();
                entry.Controller.Hide(WindowHideState.Removed);
                entry.Controller.Dispose();
            }
        }
        #endregion

        private async Task ShowStackableWindowAsync(string windowName, IWindowData data = null)
        {
            var (controller, handle) = await CreateWindowController(windowName);
            if (controller == null)
            {
                return;
            }

            // hide top window
            if (_windowStack.Count > 0)
            {
                var top = _windowStack.Peek();
                top.Controller.Hide(WindowHideState.Covered);
            }

            // show new window
            var entry = new WindowStackEntry(windowName, controller, data, handle);
            controller.Init(entry);
            controller.Show(data, WindowShowState.New);
            _windowStack.Push(entry);
        }

        private async Task<AsyncOperationHandle<GameObject>> LoadStackableWindowPrefabAsync(string windowName, float timeoutSeconds = 10f)
        {
            var key = string.Format(AddressablePaths.StackableWindow, windowName, windowName);

            LogHelper.LogError($"1 key: {key}");

            var handle = Addressables.LoadAssetAsync<GameObject>(key);
            await handle.Task;

            LogHelper.LogError($"2 handle.Status: {handle.Status}");

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                LogHelper.LogError($"[UIManager]::LoadStackableWindowPrefabAsync: Failed to load prefab: {key}, Exception: {handle.OperationException}");
                Addressables.Release(handle);
                return default;
            }

            return handle;
        }

        private async Task<(WindowControllerBase controller, AsyncOperationHandle<GameObject> handle)> CreateWindowController(string windowName)
        {
            // 1. load + instantiate window prefab
            var handle = await LoadStackableWindowPrefabAsync(windowName);
            if (!handle.IsValid())
            {
                return (null, default);
            }
            var windowGO = GameObject.Instantiate(handle.Result, _stackableRoot);
            windowGO.transform.SetAsLastSibling();

            // 2. create window controller
            var controllerTypeName = $"{Namespaces.HotUpdate}.{windowName}Controller";
            var controllerType = TypeUtil.GetType(controllerTypeName);
            if (controllerType == null)
            {
                LogHelper.LogError($"[UIManager]::CreateWindowController: Controller type not found: {controllerTypeName}");
                return (null, default);
            }
            var controller = Activator.CreateInstance(controllerType) as WindowControllerBase;
            if (controller == null)
            {
                LogHelper.LogError($"[UIManager]::CreateWindowController: Failed to create controller: {controllerTypeName}");
                return (null, default);
            }

            return (controller, handle);
        }
    }
}