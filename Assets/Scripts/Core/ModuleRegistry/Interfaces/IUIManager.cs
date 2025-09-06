using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YuankunHuang.Unity.ModuleCore;

namespace YuankunHuang.Unity.UICore
{
    public interface IWindowData { }
    public interface IUIManager : IModule
    {
        void Show(string windowName, IWindowData data = null);
        void GoBack();
        void GoBackTo(string windowName);
        bool Contains(string windowName);
        string Current { get; }
    }
}