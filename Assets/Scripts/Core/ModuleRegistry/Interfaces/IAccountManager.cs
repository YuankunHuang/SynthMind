using System;
using YuankunHuang.Unity.ModuleCore;

namespace YuankunHuang.Unity.AccountCore
{
    public interface IAccount
    {
        string UUID { get; }
        string Username { get; }
        string DisplayName { get; }
        string Email { get; }
        int Avatar { get; }
        bool IsEmailVerified { get; }
        void Dispose();
    }

    public interface IAccountManager : IModule
    {
        IAccount Self { get; }
        IAccount AI { get; }
        IAccount GetAccount(string uuid);

        // Authentication
        void Login(string email, string password, Action onSuccess, Action<string> onError);
        void Register(string email, string password, string displayName, Action onSuccess, Action<string> onError);
        void Logout(Action onComplete);
        void GetCurrentUser(Action<IAccount> onComplete);

        // Profile Management
        void UpdateProfile(string displayName, string photoUrl, Action<bool> onComplete);
        void SendEmailVerification(Action<bool> onComplete);
        void SendPasswordResetEmail(string email, Action<bool> onComplete);

        // User Data
        void SaveUserData(System.Collections.Generic.Dictionary<string, object> userData, Action<bool> onComplete);
        void LoadUserData(Action<System.Collections.Generic.Dictionary<string, object>> onComplete);
    }
}