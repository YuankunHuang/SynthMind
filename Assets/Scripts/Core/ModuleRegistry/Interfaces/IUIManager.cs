using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YuankunHuang.Unity.ModuleCore;

namespace YuankunHuang.Unity.UICore
{
    public interface IWindowStackEntry { }
    public interface IWindowData { }
    public interface IUIManager : IModule
    {
        void ShowStackableWindow(string windowName, IWindowData data = null);
        IWindowStackEntry? GetWindowOnTop();
        bool IsWindowInStack(string windowName);
        void GoBack();
        void GoBackTo(string windowName);
    }
}