using System.Collections.Generic;

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
            Self = new Account("self-uuid", "self-username", "self-email", "self-avatar");
        }

        public void Dispose()
        {
            Self = null;
            
            foreach (var account in _accounts.Values)
            {
                account.Dispose();
            }
            _accounts.Clear();
        }
    }
}