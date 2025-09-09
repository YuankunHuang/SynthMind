using System;
using System.Collections.Generic;
using System.IO;

namespace YuankunHuang.Unity.GameDataConfig
{
    public class LanguageData
    {
        public int Id { get; set; }

        public string LangCode { get; set; }

        public string Icon { get; set; }

    }

    public partial class LanguageConfig : BaseConfigData<LanguageData>
    {
        /// <summary>
        /// Initializes and loads binary data file
        /// </summary>
        public static void Initialize()
        {
            string binaryPath = System.IO.Path.Combine(UnityEngine.Application.streamingAssetsPath, "ConfigData", "Language.data");
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
