using System;
using System.Collections.Generic;
using YuankunHuang.Unity.Core;

namespace YuankunHuang.Unity.GameDataConfig
{
    public partial class AudioConfig : BaseConfigData<AudioData>
    {
        private static Dictionary<AudioIdType, AudioData> _idDict = new();

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
            var audioPath = $"{AddressablePaths.Audio}/{audioData.AssetPath}";
            return audioPath;
        }
    }
}
