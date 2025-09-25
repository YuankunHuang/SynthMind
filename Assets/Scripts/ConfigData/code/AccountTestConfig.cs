using System;
using System.Collections.Generic;
using System.IO;

namespace YuankunHuang.Unity.GameDataConfig
{
    public class AccountTestData
    {
        public int Id { get; set; }

        public string Uuid { get; set; }

        public bool IsAI { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string Nickname { get; set; }

        public string Email { get; set; }

        public int Avatar { get; set; }

    }

    public partial class AccountTestConfig : BaseConfigData<AccountTestData>
    {
        /// <summary>
        /// Initializes and loads binary data file
        /// </summary>
        public static void Initialize()
        {
            string binaryPath = System.IO.Path.Combine(UnityEngine.Application.streamingAssetsPath, "ConfigData", "AccountTest.data");
            Initialize(binaryPath);
        }

        /// <summary>
        /// Custom post-initialization logic (optional, see .ext.cs)
        /// </summary>
        static partial void PostInitialize();
        // You can add your custom logic in the ext file
    }
}
