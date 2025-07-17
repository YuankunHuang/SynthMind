using UnityEngine;

namespace YuankunHuang.Unity.Core
{
    public interface IWindowData { }
    public interface IUIManager
    {
        void ShowStackableWindow(string windowName, IWindowData data = null);
        WindowStackEntry? GetWindowOnTop();
        bool IsWindowInStack(string windowName);
        void GoBack();
        void GoBackTo(string windowName);
    }

    public interface ICameraManager 
    {
        Camera MainCamera { get; }
        Camera UICamera { get; }

        void AddToMainStack(Camera cam);
        void RemoveFromMainStack(Camera cam);
        void AddToMainStackWithOwner(object owner, Camera cam);
        void RemoveFromMainStackWithOwner(object owner);
    }

    public interface IAccount 
    { 
        string UUID { get; }
        string Username { get; }
        string Email { get; }
        string Avatar { get; }
    }
    public interface IAccountManager
    {
        IAccount Self { get; }
        IAccount GetAccount(string uuid);
    }
}