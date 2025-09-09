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

        public string DefaultVolume { get; set; }

        public bool Loop { get; set; }

        public int Priority { get; set; }

    }

    public partial class AudioConfig : BaseConfigData<AudioData>
    {
        public static void Initialize()
        {
            string binaryPath = System.IO.Path.Combine(UnityEngine.Application.streamingAssetsPath, "ConfigData", "Audio.data");
            Initialize(binaryPath);
            PostInitialize();
        }

        static partial void PostInitialize();
    }
}
