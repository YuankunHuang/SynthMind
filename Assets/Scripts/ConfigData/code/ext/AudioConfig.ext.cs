using System;
using System.Collections.Generic;
using YuankunHuang.Unity.Core;

namespace YuankunHuang.Unity.GameDataConfig
{
    /// <summary>
    /// AudioConfig extension class
    /// Add your custom logic and methods here
    /// This file is NOT overwritten during build
    /// </summary>
    public partial class AudioConfig : BaseConfigData<AudioData>
    {
        // Add your custom methods here
        private static Dictionary<AudioIdType, AudioData> _idDict;

        static partial void PostInitialize()
        {
            foreach (var data in GetAll())
            {
                var id = data.AudioId;
                _idDict[id] = data;
            }
        }

        public static AudioData GetByAudioId(AudioIdType audioId)
        {
            if (_idDict.TryGetValue(audioId, out var data))
            {
                return data;
            }

            return null;
        }
    }

    public static class AudioDataExtensions
    {
        public static string GetAssetPath(this AudioData audioData)
        {
            var audioPath = System.IO.Path.Combine(AddressablePaths.Audio, audioData.AssetPath);
            return audioPath;
        }
    }
}
