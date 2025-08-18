using System;
using YuankunHuang.Unity.ModuleCore;

namespace YuankunHuang.Unity.AccountCore
{
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
}