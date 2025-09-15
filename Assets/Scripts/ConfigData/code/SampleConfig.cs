using System;
using System.Collections.Generic;
using System.IO;

namespace YuankunHuang.Unity.GameDataConfig
{
    public class SampleData
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public SampleType Type { get; set; }

        public int Level { get; set; }

        public float Cost { get; set; }

        public DateTime OpenTime { get; set; }

        public DateTime CloseTime { get; set; }

    }

    public partial class SampleConfig : BaseConfigData<SampleData>
    {
        /// <summary>
        /// Initializes and loads binary data file
        /// </summary>
        public static void Initialize()
        {
            string binaryPath = System.IO.Path.Combine(UnityEngine.Application.streamingAssetsPath, "ConfigData", "Sample.data");
            Initialize(binaryPath);
            PostInitialize();
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        /// <summary>
        /// Async initialization for WebGL platform
        /// </summary>
        public static async System.Threading.Tasks.Task InitializeAsync(string binaryPath)
        {
            await BaseConfigData<SampleData>.InitializeAsync(binaryPath);
            PostInitialize();
        }
#endif

        /// <summary>
        /// Custom post-initialization logic (optional, see .ext.cs)
        /// </summary>
        static partial void PostInitialize();
        // You can add your custom logic in the ext file
    }
}
