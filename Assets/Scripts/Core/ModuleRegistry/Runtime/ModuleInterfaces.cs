using System;
using System.Collections.Generic;
using UnityEngine;

namespace YuankunHuang.Unity.Core
{
    public interface IModule
    {
        void Dispose();
    }

    public interface IWindowData { }
    public interface IUIManager : IModule
    {
        void ShowStackableWindow(string windowName, IWindowData data = null);
        WindowStackEntry? GetWindowOnTop();
        bool IsWindowInStack(string windowName);
        void GoBack();
        void GoBackTo(string windowName);
    }

    public interface ICameraManager : IModule
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
        string Nickname { get; }
        string Email { get; }
        int Avatar { get; }
        void Dispose();
    }

    public interface IAccountManager : IModule
    {
        IAccount Self { get; }
        IAccount AI { get; }
        IAccount GetAccount(string uuid);
        void Login(string username, string password, Action onSuccess, Action<string> onError);
    }

    public enum ServerType
    {
        ChatAI,
        ChatPlayer,
    }
    public interface INetworkManager : IModule
    {
        void Connect(string address, int port);
        void Disconnect();
        bool IsConnected { get; }
        event Action OnConnected;
        event Action OnDisconnected;
        event Action<string> OnError;

        void SendMessage(string conversationId, string senderId, string content, Dictionary<string, object> metadata, ServerType server, Action<string> onSuccess, Action<string> onError);
    }
}