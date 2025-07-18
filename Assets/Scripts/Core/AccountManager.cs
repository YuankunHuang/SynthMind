using System.Collections.Generic;
using YuankunHuang.Unity.GameDataConfig;

namespace YuankunHuang.Unity.Core
{
    public class Account : IAccount
    {
        public string UUID { get; private set; }
        public string Username { get; private set; }
        public string Email { get; private set; }
        public string Avatar { get; private set; }

        public Account(string uuid, string userName, string email, string avatar)
        {
            UUID = uuid;
            Username = userName;
            Email = email;
            Avatar = avatar;
        }

        public void Dispose()
        {
        }
    }

    public class AccountManager : IAccountManager
    {
        public IAccount AI { get; private set; }
        public IAccount Self { get; private set; }

        public IAccount GetAccount(string uuid)
        {
            if (string.IsNullOrEmpty(uuid))
            {
                return null;
            }

            if (Self != null && Self.UUID == uuid)
            {
                return Self;
            }

            if (_accounts.TryGetValue(uuid, out var account))
            {
                return account;
            }

            return null;
        }

        private Dictionary<string, Account> _accounts = new();

        public AccountManager()
        {
        }

        public void Dispose()
        {
            Self?.Dispose();
            AI?.Dispose();
            Self = null;
            AI = null;
            
            foreach (var account in _accounts.Values)
            {
                account.Dispose();
            }
            _accounts.Clear();
        }

        public void Login(string username, string password, System.Action onSuccess, System.Action<string> onError)
        {
            var accountData = AccountTestConfig.GetByUsername(username);
            if (accountData == null)
            {
                onError?.Invoke("Account not found");
                return;
            }

            if (accountData.password != password)
            {
                onError?.Invoke($"Invalid password -> Correct: {accountData.password} | wrong: {password}");
                return;
            }

            Self = new Account(accountData.uuid, accountData.username, accountData.email, accountData.avatar);
            AI = new Account(System.Guid.NewGuid().ToString(), "AI", "ai-email", "ai-avatar");

            onSuccess?.Invoke();
        }
    }
}