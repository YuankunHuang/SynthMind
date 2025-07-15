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
            Config = entry.WindowGO.GetComponent<GeneralWindowConfig>();

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
        public GameObject WindowGO;
        public AsyncOperationHandle<GameObject> Handle;

        public WindowStackEntry(string windowName, WindowControllerBase controller, IWindowData data, GameObject windowGO, AsyncOperationHandle<GameObject> handle)
        {
            WindowName = windowName;
            Controller = controller;
            Data = data;
            WindowGO = windowGO;
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
            MonoManager.Instance.StartCoroutine(ShowStackableWindowCoroutine(windowName, data));
        }

        private IEnumerator ShowStackableWindowCoroutine(string windowName, IWindowData data = null)
        {
            // 1. Load prefab
            var key = string.Format(AddressablePaths.StackableWindow, windowName, windowName);
            var handle = Addressables.LoadAssetAsync<GameObject>(key);
            yield return handle;

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                LogHelper.LogError($"[UIManager]::LoadStackableWindowPrefabAsync: Failed to load prefab: {key}, Exception: {handle.OperationException}");
                Addressables.Release(handle);
                yield break;
            }

            // 2. Instantiate window
            var windowGO = GameObject.Instantiate(handle.Result, StackableRoot);
            windowGO.transform.SetAsLastSibling();

            // 3. Create window controller
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

            // 4. Hide window on top
            if (_windowStack.Count > 0)
            {
                var top = _windowStack.Peek();
                top.Controller.Hide(WindowHideState.Covered);
            }

            // 5. Push to stack and show
            var entry = new WindowStackEntry(windowName, controller, data, windowGO, handle);
            controller.Init(entry);
            controller.Show(data, WindowShowState.New);
            _windowStack.Push(entry);
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
    }
}