using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using YuankunHuang.Unity.GameDataConfig;

namespace YuankunHuang.Unity.AudioCore
{
    public class AudioHandle : IAudioHandle
    {
        public bool IsPlaying { get; set; }
        public bool IsValid { get; set; }
        
        public void Stop(float fadeTime)
        {

        }

        public void SetVolume(int volume)
        {

        }
    }

    public class AudioManager : IAudioManager
    {
        #region Interfaces
        public bool IsInitialized { get; private set; }

        public AudioManager()
        {
            IsInitialized = true;
        }

        // BGM
        public Task PlayBGMAsync(AudioIdType audioId)
        {
            throw new System.NotImplementedException();
        }
        public void StopBGM(float fadeTime)
        {
            throw new System.NotImplementedException();
        }
        public void PauseBGM()
        {
            throw new System.NotImplementedException();
        }
        public void ResumeBGM()
        {
            throw new System.NotImplementedException();
        }

        // SFX
        public IAudioHandle PlaySFX(AudioIdType audioId, Vector3? position = null)
        {
            throw new System.NotImplementedException();
        }
        public IAudioHandle PlayUI(AudioIdType audioId)
        {
            throw new System.NotImplementedException();
        }

        // Control
        public void SetMasterVolume(int volume)
        {
            AudioPreferences.MasterVolume = volume;
        }
        public void SetBGMVolume(int volume)
        {
            AudioPreferences.BGMVolume = volume;
        }
        public void SetSFXVolume(int volume)
        {
            AudioPreferences.SFXVolume = volume;
        }
        public void SetMasterMuted(bool mute)
        {
            AudioPreferences.MasterMuted = mute;
        }

        // Preload
        public Task PreloadAudioAsync(AudioIdType audioId)
        {
            throw new System.NotImplementedException();
        }
        public void UnloadAudio(AudioIdType audioId)
        {
            throw new System.NotImplementedException();
        }

        public void Dispose()
        {
            IsInitialized = false;
        }
        #endregion
    }
}