using System;
using System.Collections.Generic;
using System.IO;

namespace YuankunHuang.SynthMind.GameDataConfig
{
    public class AccountTestData
    {
        public int id { get; set; }

        public string uuid { get; set; }

        public bool isai { get; set; }

        public string username { get; set; }

        public string password { get; set; }

        public string nickname { get; set; }

        public string email { get; set; }

        public int avatar { get; set; }

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
            PostInitialize();
        }

        /// <summary>
        /// Custom post-initialization logic (optional, see .ext.cs)
        /// </summary>
        static partial void PostInitialize();
        // You can add your custom logic in the ext file
    }
}
