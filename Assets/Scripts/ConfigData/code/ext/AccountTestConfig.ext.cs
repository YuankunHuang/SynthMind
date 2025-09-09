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
        public static HashSet<int> AI_ID_SET = new();

        private static Dictionary<string, AccountTestData> _usernameMap = new();

        static partial void PostInitialize()
        {
            AI_ID_SET.Clear();

            foreach (var data in GetAll())
            {
                if (data != null)
                {
                    if (!string.IsNullOrEmpty(data.Username))
                    {
                        _usernameMap[data.Username] = data;
                    }

                    if (data.IsAI)
                    {
                        AI_ID_SET.Add(data.Id);
                    }
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
