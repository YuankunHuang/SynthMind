using System;
using System.Collections.Generic;
using YuankunHuang.Unity.Core;

namespace YuankunHuang.Unity.GameDataConfig
{
    /// <summary>
    /// AccountTestConfig extension class
    /// Add your custom logic and methods here
    /// This file is NOT overwritten during build
    /// </summary>
    public partial class AccountTestConfig : BaseConfigData<AccountTestData>
    {
        private static Dictionary<string, AccountTestData> _usernameMap = new();

        static partial void PostInitialize()
        {
            foreach (var data in GetAll())
            {
                if (data != null && !string.IsNullOrEmpty(data.username))
                {
                    _usernameMap[data.username] = data;
                }
            }
        }

        public static AccountTestData GetByUsername(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return null;
            }
            _usernameMap.TryGetValue(username, out var data);
            return data;
        }
    }
}
