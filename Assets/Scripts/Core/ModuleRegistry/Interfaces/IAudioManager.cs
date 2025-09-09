using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using YuankunHuang.Unity.GameDataConfig;
using YuankunHuang.Unity.ModuleCore;

namespace YuankunHuang.Unity.AudioCore
{
    public interface IAudioHandle
    {
        bool IsPlaying { get; }
        bool IsValid { get; }
        void Stop(float fadeTime);
        void SetVolume(int volume);
    }

    public interface IAudioManager : IModule
    {
        // BGM
        Task PlayBGMAsync(AudioIdType audioId);
        void StopBGM(float fadeTime);
        void PauseBGM();
        void ResumeBGM();

        // SFX
        IAudioHandle PlaySFX(AudioIdType audioId, Vector3? position = null);
        IAudioHandle PlayUI(AudioIdType audioId);

        // Control
        void SetMasterVolume(int volume);
        void SetBGMVolume(int volume);
        void SetSFXVolume(int volume);
        void SetMasterMuted(bool mute);

        // Preload
        Task PreloadAudioAsync(AudioIdType audioId);
        void UnloadAudio(AudioIdType audioId);
    }
}
