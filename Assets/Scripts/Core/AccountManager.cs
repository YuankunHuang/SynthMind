using System.Collections.Generic;
using YuankunHuang.Unity.GameDataConfig;
using YuankunHuang.Unity.Core;
using YuankunHuang.Unity.ModuleCore;
using YuankunHuang.Unity.FirebaseCore;

namespace YuankunHuang.Unity.AccountCore
{
    public class Account : IAccount
    {
        public string UUID { get; private set; }
        public string Username { get; private set; }
        public string DisplayName { get; private set; }
        public string Email { get; private set; }
        public int Avatar
        {
            get => int.TryParse(_avatar, out var avatar) ? avatar : 1;
            private set => _avatar = value.ToString();
        }
        public bool IsEmailVerified { get; private set; }

        private string _avatar;

        public Account(string uuid, string username, string displayName, string email, string avatar, bool isEmailVerified = false)
        {
            UUID = uuid;
            Username = username;
            DisplayName = displayName;
            Email = email;
            _avatar = avatar;
            IsEmailVerified = isEmailVerified;
        }

        public static Account FromFirebaseUser(IFirebaseUser firebaseUser)
        {
            return new Account(
                firebaseUser.UUID,
                firebaseUser.Email,
                firebaseUser.DisplayName ?? firebaseUser.Email,
                firebaseUser.Email,
                "1", // Default avatar
                firebaseUser.IsEmailVerified
            );
        }

        public void Dispose()
        {
        }
    }

    public class AccountManager : IAccountManager
    {
        public bool IsInitialized { get; private set; } = false;
        public IAccount AI { get; private set; }
        public IAccount Self { get; private set; }

        private IFirebaseManager _firebaseManager;
        private Dictionary<string, Account> _accounts = new();

        public AccountManager()
        {
            _firebaseManager = ModuleRegistry.Get<IFirebaseManager>();
            IsInitialized = true;

            // Initialize AI account from config for backward compatibility
            InitializeAIAccount();

            LogHelper.Log("[AccountManager] Initialized with Firebase Auth support.");
        }

        private void InitializeAIAccount()
        {
            // Load AI account from config for backward compatibility with existing chat system
            foreach (var id in AccountTestConfig.AI_ID_SET)
            {
                var aiAccountCfgData = AccountTestConfig.GetById(id);
                AI = new Account(aiAccountCfgData.Uuid, aiAccountCfgData.Username, aiAccountCfgData.Nickname, aiAccountCfgData.Email, aiAccountCfgData.Avatar.ToString());
                break;
            }
        }

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

            if (AI != null && AI.UUID == uuid)
            {
                return AI;
            }

            if (_accounts.TryGetValue(uuid, out var account))
            {
                return account;
            }

            return null;
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

            IsInitialized = false;
        }

        // Authentication Methods
        public void Login(string email, string password, System.Action onSuccess, System.Action<string> onError)
        {
            if (_firebaseManager == null)
            {
                LogHelper.LogError("[AccountManager] Firebase manager not available");
                onError?.Invoke("Authentication service not available");
                return;
            }

            _firebaseManager.SignInWithEmail(email, password,
                (firebaseUser) =>
                {
                    Self = Account.FromFirebaseUser(firebaseUser);
                    LogHelper.Log($"[AccountManager] User logged in: {Self.Email}");
                    onSuccess?.Invoke();
                },
                (error) =>
                {
                    LogHelper.LogWarning($"[AccountManager] Login failed: {error}");
                    onError?.Invoke(error);
                });
        }

        public void Register(string email, string password, string displayName, System.Action onSuccess, System.Action<string> onError)
        {
            if (_firebaseManager == null)
            {
                LogHelper.LogError("[AccountManager] Firebase manager not available");
                onError?.Invoke("Authentication service not available");
                return;
            }

            _firebaseManager.RegisterWithEmail(email, password, displayName,
                (firebaseUser) =>
                {
                    Self = Account.FromFirebaseUser(firebaseUser);
                    LogHelper.Log($"[AccountManager] User registered: {Self.Email}");
                    onSuccess?.Invoke();
                },
                (error) =>
                {
                    LogHelper.LogWarning($"[AccountManager] Registration failed: {error}");
                    onError?.Invoke(error);
                });
        }

        public void Logout(System.Action onComplete)
        {
            if (_firebaseManager == null)
            {
                Self = null;
                onComplete?.Invoke();
                return;
            }

            _firebaseManager.SignOut(() =>
            {
                Self = null;
                LogHelper.Log("[AccountManager] User logged out");
                onComplete?.Invoke();
            });
        }

        public void GetCurrentUser(System.Action<IAccount> onComplete)
        {
            if (_firebaseManager == null)
            {
                onComplete?.Invoke(Self);
                return;
            }

            _firebaseManager.GetCurrentUser((firebaseUser) =>
            {
                if (firebaseUser != null)
                {
                    Self = Account.FromFirebaseUser(firebaseUser);
                }
                onComplete?.Invoke(Self);
            });
        }

        // Profile Management
        public void UpdateProfile(string displayName, string photoUrl, System.Action<bool> onComplete)
        {
            if (_firebaseManager == null)
            {
                onComplete?.Invoke(false);
                return;
            }

            _firebaseManager.UpdateUserProfile(displayName, photoUrl, onComplete);
        }

        public void SendEmailVerification(System.Action<bool> onComplete)
        {
            if (_firebaseManager == null)
            {
                onComplete?.Invoke(false);
                return;
            }

            _firebaseManager.SendEmailVerification(onComplete);
        }

        public void SendPasswordResetEmail(string email, System.Action<bool> onComplete)
        {
            if (_firebaseManager == null)
            {
                onComplete?.Invoke(false);
                return;
            }

            _firebaseManager.SendPasswordResetEmail(email, onComplete);
        }

        // User Data Management
        public void SaveUserData(Dictionary<string, object> userData, System.Action<bool> onComplete)
        {
            if (_firebaseManager == null || Self == null)
            {
                onComplete?.Invoke(false);
                return;
            }

            _firebaseManager.SaveUserData(Self.UUID, userData, onComplete);
        }

        public void LoadUserData(System.Action<Dictionary<string, object>> onComplete)
        {
            if (_firebaseManager == null || Self == null)
            {
                onComplete?.Invoke(null);
                return;
            }

            _firebaseManager.LoadUserData(Self.UUID, onComplete);
        }
    }
}