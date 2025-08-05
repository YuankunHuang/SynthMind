using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace YuankunHuang.Unity.Core
{
    public interface IModule
    {
        void Dispose();
    }

    #region UI
    public interface IWindowData { }
    public interface IUIManager : IModule
    {
        void ShowStackableWindow(string windowName, IWindowData data = null);
        WindowStackEntry? GetWindowOnTop();
        bool IsWindowInStack(string windowName);
        void GoBack();
        void GoBackTo(string windowName);
    }
    #endregion

    #region Camera
    public interface ICameraManager : IModule
    {
        Camera MainCamera { get; }
        Camera UICamera { get; }

        void AddToMainStack(Camera cam);
        void RemoveFromMainStack(Camera cam);
        void AddToMainStackWithOwner(object owner, Camera cam);
        void RemoveFromMainStackWithOwner(object owner);
    }
    #endregion

    #region Account
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
    #endregion

    #region Network
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

        void SendMessage(string conversationGroup, string conversationId, string senderId, string content, Dictionary<string, object> metadata, ServerType server, Action<string> onSuccess, Action<string> onError);
    }
    #endregion

    #region Command
    public interface ICommandManager : IModule
    {
        void RegisterCommand(IGameCommand command);
        void UnregisterCommand(string commandName);
        bool TryExecuteCommand(string input);
        bool TryExecuteNatural(string input);
        string[] GetAvailableCommands();
    }

    public interface IGameCommand
    {
        string CommandName { get; }
        string Description { get; }
        bool CanExecute(string[] parameters);
        void Execute(string[] parameters);
    }
    #endregion

    #region Localization
    public interface ILocalizationManager : IModule
    {
        Task InitializeAsync();
        Task<string> GetLocalizedText(string key);
        Task<string> GetLocalizedText(string table, string key);
        Task<string> GetLocalizedTextFormatted(string key, params object[] args);
        Task<string> GetLocalizedTextFormatted(string table, string key, params object[] args);
        void SetLanguage(string langCode);
        Task SetLanguageAsync(string langCode);
        string GetLanguageDisplayName(string langCode);
        List<string> GetAvailableLanguages();
        void ForceRefresh();
        string CurrentLanguage { get; }
        bool IsInitialized { get; }
        event Action<string> OnLanguageChanged;
    }
    #endregion
}