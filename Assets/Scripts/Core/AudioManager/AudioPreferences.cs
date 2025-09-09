using YuankunHuang.Unity.Util;

namespace YuankunHuang.Unity.AudioCore
{
    public static class AudioPreferences
    {
        private const string MASTER_VOLUME_KEY = "Audio_MasterVolume";
        private const string BGM_VOLUME_KEY = "Audio_BGMVolume";
        private const string SFX_VOLUME_KEY = "Audio_SFXVolume";
        private const string MASTER_MUTED_KEY = "Audio_MasterMuted";
        
        private const int DEFAULT_VOLUME = 80;

        public static int MasterVolume
        {
            get => PlayerPrefsUtil.GetInt(MASTER_VOLUME_KEY, DEFAULT_VOLUME);
            set => PlayerPrefsUtil.TrySetInt(MASTER_VOLUME_KEY, UnityEngine.Mathf.Clamp(value, 0, 100));
        }

        public static int BGMVolume
        {
            get => PlayerPrefsUtil.GetInt(BGM_VOLUME_KEY, DEFAULT_VOLUME);
            set => PlayerPrefsUtil.TrySetInt(BGM_VOLUME_KEY, UnityEngine.Mathf.Clamp(value, 0, 100));
        }

        public static int SFXVolume
        {
            get => PlayerPrefsUtil.GetInt(SFX_VOLUME_KEY, DEFAULT_VOLUME);
            set => PlayerPrefsUtil.TrySetInt(SFX_VOLUME_KEY, UnityEngine.Mathf.Clamp(value, 0, 100));
        }

        public static bool MasterMuted
        {
            get => PlayerPrefsUtil.GetBool(MASTER_MUTED_KEY, false);
            set => PlayerPrefsUtil.TrySetBool(MASTER_MUTED_KEY, value);
        }

        public static float GetNormalizedVolume(int volume, bool applyMasterVolume = true)
        {
            if (MasterMuted) return 0f;
            
            var normalizedVolume = volume / 100f;
            if (applyMasterVolume)
            {
                normalizedVolume *= (MasterVolume / 100f);
            }
            
            return UnityEngine.Mathf.Clamp01(normalizedVolume);
        }
    }
}