using System;
using System.Collections.Generic;
using System.IO;

namespace YuankunHuang.Unity.GameDataConfig
{
    public class AudioData
    {
        public int Id { get; set; }

        public AudioIdType AudioId { get; set; }

        public AudioGroupType Group { get; set; }

        public string AssetPath { get; set; }

        public int DefaultVolume { get; set; }

        public bool Loop { get; set; }

        public int Priority { get; set; }

    }

    public partial class AudioConfig : BaseConfigData<AudioData>
    {
        /// <summary>
        /// Initializes and loads binary data file
        /// </summary>
        public static void Initialize()
        {
            string binaryPath = System.IO.Path.Combine(UnityEngine.Application.streamingAssetsPath, "ConfigData", "Audio.data");
            Initialize(binaryPath);
            PostInitialize();
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        /// <summary>
        /// Async initialization for WebGL platform
        /// </summary>
        public static async System.Threading.Tasks.Task InitializeAsync(string binaryPath)
        {
            await BaseConfigData<AudioData>.InitializeAsync(binaryPath);
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
