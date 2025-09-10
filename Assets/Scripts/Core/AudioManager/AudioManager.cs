using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using YuankunHuang.Unity.AssetCore;
using YuankunHuang.Unity.Core;
using YuankunHuang.Unity.Core.Debug;
using YuankunHuang.Unity.GameDataConfig;
using YuankunHuang.Unity.ModuleCore;

namespace YuankunHuang.Unity.AudioCore
{
    public class AudioHandle : IAudioHandle
    {
        private AudioSource _audioSource;
        private Coroutine _fadeCoroutine;
        private AudioManager _manager;
        
        public bool IsPlaying => _audioSource != null && _audioSource.isPlaying;
        public bool IsValid => _audioSource != null;

        internal AudioHandle(AudioSource audioSource, AudioManager manager)
        {
            _audioSource = audioSource;
            _manager = manager;
        }

        public void Stop(float fadeTime)
        {
            if (!IsValid) return;

            if (fadeTime > 0f)
            {
                if (_fadeCoroutine != null)
                {
                    MonoManager.Instance.StopCoroutine(_fadeCoroutine);
                }
                _fadeCoroutine = MonoManager.Instance.StartCoroutine(FadeOutAndStop(fadeTime));
            }
            else
            {
                _audioSource.Stop();
                _manager?.ReturnAudioSourceToPool(_audioSource);
                Release();
            }
        }

        public void SetVolume(int volume)
        {
            if (!IsValid) return;
            _audioSource.volume = volume / 100f;
        }

        private IEnumerator FadeOutAndStop(float fadeTime)
        {
            var startVolume = _audioSource.volume;
            var elapsed = 0f;

            while (elapsed < fadeTime && IsValid)
            {
                elapsed += Time.deltaTime;
                var progress = elapsed / fadeTime;
                _audioSource.volume = Mathf.Lerp(startVolume, 0f, progress);
                yield return null;
            }

            if (IsValid)
            {
                _audioSource.Stop();
                _audioSource.volume = startVolume;
                _manager?.ReturnAudioSourceToPool(_audioSource);
                Release();
            }
            
            _fadeCoroutine = null;
        }

        internal void Release()
        {
            if (_fadeCoroutine != null)
            {
                MonoManager.Instance.StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = null;
            }
            _audioSource = null;
            _manager = null;
        }
    }

    public class AudioManager : IAudioManager
    {
        #region Private Fields
        private readonly Dictionary<AudioIdType, AudioClip> _audioCache = new();
        private readonly Queue<AudioSource> _audioSourcePool = new();
        private readonly List<AudioSource> _activeAudioSources = new();
        private readonly List<AudioHandle> _activeHandles = new();
        
        private GameObject _audioRoot;
        private AudioSource _bgmAudioSource;
        private Coroutine _bgmFadeCoroutine;
        
        private const int INITIAL_POOL_SIZE = 10;
        private const int MAX_POOL_SIZE = 50;
        #endregion

        #region IModule Implementation
        public bool IsInitialized { get; private set; }

        public AudioManager()
        {
            Initialize();
        }

        private void Initialize()
        {
            try
            {
                // Create audio root object
                if (_audioRoot != null)
                {
                    GameObject.Destroy(_audioRoot);
                }

                _audioRoot = new GameObject("AudioManager");

                // Create BGM audio source
                _bgmAudioSource = _audioRoot.AddComponent<AudioSource>();
                _bgmAudioSource.loop = true;
                _bgmAudioSource.playOnAwake = false;

                // Initialize audio source pool
                for (int i = 0; i < INITIAL_POOL_SIZE; i++)
                {
                    _audioSourcePool.Enqueue(CreatePooledAudioSource());
                }

                IsInitialized = true;
                LogHelper.Log("[AudioManager] Initialized successfully");
            }
            catch (Exception e)
            {
                LogHelper.LogError($"[AudioManager] Failed to initialize: {e.Message}");
                LogHelper.LogException(e);
            }
        }

        public void Dispose()
        {
            try
            {
                // Stop all audio handles
                for (int i = _activeHandles.Count - 1; i >= 0; i--)
                {
                    _activeHandles[i]?.Release();
                }
                _activeHandles.Clear();

                // Stop BGM and coroutines
                if (_bgmFadeCoroutine != null)
                {
                    MonoManager.Instance.StopCoroutine(_bgmFadeCoroutine);
                    _bgmFadeCoroutine = null;
                }
                StopBGM(0f);
                StopAllSFX();

                // Clear cache and release assets
                foreach (var kv in _audioCache)
                {
                    if (kv.Value != null)
                    {
                        var config = AudioConfig.GetByAudioId(kv.Key);
                        if (config != null)
                        {
                            ResManager.Release(config.GetAssetPath());
                        }
                    }
                }
                _audioCache.Clear();

                // Destroy audio root
                if (_audioRoot != null)
                {
                    UnityEngine.Object.Destroy(_audioRoot);
                    _audioRoot = null;
                }

                _audioSourcePool.Clear();
                _activeAudioSources.Clear();
                _bgmAudioSource = null;

                IsInitialized = false;
                LogHelper.Log("[AudioManager] Disposed");
            }
            catch (Exception e)
            {
                LogHelper.LogError($"[AudioManager] Error during disposal: {e.Message}");
            }
        }
        #endregion

        #region BGM Methods
        public async Task PlayBGMAsync(AudioIdType audioId)
        {
            if (!IsInitialized)
            {
                LogHelper.LogWarning("[AudioManager] Not initialized");
                return;
            }

            try
            {
                var audioClip = await LoadAudioClipAsync(audioId);
                if (audioClip == null)
                {
                    LogHelper.LogError($"[AudioManager] Failed to load BGM: {audioId}");
                    return;
                }

                if (_bgmFadeCoroutine != null)
                {
                    MonoManager.Instance.StopCoroutine(_bgmFadeCoroutine);
                }

                _bgmAudioSource.clip = audioClip;
                _bgmAudioSource.volume = AudioPreferences.GetNormalizedVolume(AudioPreferences.BGMVolume);
                _bgmAudioSource.Play();

                LogHelper.Log($"[AudioManager] Playing BGM: {audioId}");
            }
            catch (Exception e)
            {
                LogHelper.LogError($"[AudioManager] Error playing BGM {audioId}: {e.Message}");
            }
        }

        public void StopBGM(float fadeTime)
        {
            if (!IsInitialized || _bgmAudioSource == null) return;

            if (fadeTime > 0f)
            {
                if (_bgmFadeCoroutine != null)
                {
                    MonoManager.Instance.StopCoroutine(_bgmFadeCoroutine);
                }
                _bgmFadeCoroutine = MonoManager.Instance.StartCoroutine(FadeBGM(0f, fadeTime, true));
            }
            else
            {
                _bgmAudioSource.Stop();
            }
        }

        public void PauseBGM()
        {
            if (IsInitialized && _bgmAudioSource != null)
            {
                _bgmAudioSource.Pause();
            }
        }

        public void ResumeBGM()
        {
            if (IsInitialized && _bgmAudioSource != null)
            {
                _bgmAudioSource.UnPause();
            }
        }

        private IEnumerator FadeBGM(float targetVolume, float fadeTime, bool stopAfterFade = false)
        {
            var startVolume = _bgmAudioSource.volume;
            var elapsed = 0f;

            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                var progress = elapsed / fadeTime;
                _bgmAudioSource.volume = Mathf.Lerp(startVolume, targetVolume, progress);
                yield return null;
            }

            _bgmAudioSource.volume = targetVolume;

            if (stopAfterFade)
            {
                _bgmAudioSource.Stop();
            }

            _bgmFadeCoroutine = null;
        }
        #endregion

        #region SFX Methods
        public IAudioHandle PlaySFX(AudioIdType audioId, Vector3? position = null)
        {
            return PlayAudioInternal(audioId, AudioGroupType.SFX, position);
        }

        public IAudioHandle PlayUI(AudioIdType audioId)
        {
            return PlayAudioInternal(audioId, AudioGroupType.UI, null);
        }

        private IAudioHandle PlayAudioInternal(AudioIdType audioId, AudioGroupType audioType, Vector3? position)
        {
            if (!IsInitialized)
            {
                LogHelper.LogWarning("[AudioManager] Not initialized");
                return null;
            }

            try
            {
                var audioSource = GetPooledAudioSource();
                if (audioSource == null)
                {
                    LogHelper.LogWarning("[AudioManager] No available audio source");
                    return null;
                }

                var handle = new AudioHandle(audioSource, this);
                _activeHandles.Add(handle);
                
                MonoManager.Instance.StartCoroutine(PlayAudioCoroutine(audioId, audioSource, audioType, position, handle));
                
                return handle;
            }
            catch (Exception e)
            {
                LogHelper.LogError($"[AudioManager] Error playing audio {audioId}: {e.Message}");
                return null;
            }
        }

        private IEnumerator PlayAudioCoroutine(AudioIdType audioId, AudioSource audioSource, AudioGroupType audioType, Vector3? position, AudioHandle handle)
        {
            // Load audio clip
            var loadTask = LoadAudioClipAsync(audioId);
            yield return new WaitUntil(() => loadTask.IsCompleted);

            if (loadTask.Result == null)
            {
                LogHelper.LogError($"[AudioManager] Failed to load audio: {audioId}");
                ReturnAudioSourceToPool(audioSource);
                _activeHandles.Remove(handle);
                handle.Release();
                yield break;
            }

            // Get audio configuration
            var config = AudioConfig.GetByAudioId(audioId);
            if (config == null)
            {
                LogHelper.LogWarning($"[AudioManager] No configuration found for audio: {audioId}");
                ReturnAudioSourceToPool(audioSource);
                _activeHandles.Remove(handle);
                handle.Release();
                yield break;
            }

            // Configure audio source
            audioSource.clip = loadTask.Result;
            audioSource.loop = config.Loop;
            audioSource.priority = config.Priority;

            // Set volume based on audio type
            var volume = audioType switch
            {
                AudioGroupType.SFX => AudioPreferences.GetNormalizedVolume(AudioPreferences.SFXVolume),
                AudioGroupType.UI => AudioPreferences.GetNormalizedVolume(AudioPreferences.SFXVolume),
                _ => AudioPreferences.GetNormalizedVolume(AudioPreferences.SFXVolume)
            };

            // Apply default volume from config
            volume *= config.DefaultVolume / 100f;

            audioSource.volume = volume;

            // Set position for 3D audio
            if (position.HasValue)
            {
                audioSource.transform.position = position.Value;
                audioSource.spatialBlend = 1f; // 3D audio
            }
            else
            {
                audioSource.spatialBlend = 0f; // 2D audio
            }

            // Play audio
            audioSource.Play();
            _activeAudioSources.Add(audioSource);

            // Wait for completion if not looping
            if (!config.Loop)
            {
                yield return new WaitWhile(() => audioSource.isPlaying);
                ReturnAudioSourceToPool(audioSource);
                _activeHandles.Remove(handle);
                handle.Release();
            }
        }

        private void StopAllSFX()
        {
            for (int i = _activeAudioSources.Count - 1; i >= 0; i--)
            {
                var source = _activeAudioSources[i];
                if (source != null)
                {
                    source.Stop();
                    ReturnAudioSourceToPool(source);
                }
            }
            _activeAudioSources.Clear();
        }
        #endregion

        #region Audio Source Pool Management
        private AudioSource GetPooledAudioSource()
        {
            AudioSource audioSource = null;

            if (_audioSourcePool.Count > 0)
            {
                audioSource = _audioSourcePool.Dequeue();
            }
            else if (_activeAudioSources.Count + _audioSourcePool.Count < MAX_POOL_SIZE)
            {
                audioSource = CreatePooledAudioSource();
            }

            return audioSource;
        }

        internal void ReturnAudioSourceToPool(AudioSource audioSource)
        {
            if (audioSource == null) return;

            _activeAudioSources.Remove(audioSource);
            
            // Reset audio source
            audioSource.clip = null;
            audioSource.loop = false;
            audioSource.volume = 1f;
            audioSource.pitch = 1f;
            audioSource.spatialBlend = 0f;
            audioSource.transform.position = Vector3.zero;

            _audioSourcePool.Enqueue(audioSource);
        }

        private AudioSource CreatePooledAudioSource()
        {
            var go = new GameObject($"AudioSource_{_audioSourcePool.Count + _activeAudioSources.Count}");
            go.transform.SetParent(_audioRoot.transform);
            
            var audioSource = go.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            
            return audioSource;
        }
        #endregion

        #region Volume Control
        public void SetMasterVolume(int volume)
        {
            AudioPreferences.MasterVolume = volume;
            UpdateAllVolumes();
        }

        public void SetBGMVolume(int volume)
        {
            AudioPreferences.BGMVolume = volume;
            if (_bgmAudioSource != null)
            {
                _bgmAudioSource.volume = AudioPreferences.GetNormalizedVolume(AudioPreferences.BGMVolume);
            }
        }

        public void SetSFXVolume(int volume)
        {
            AudioPreferences.SFXVolume = volume;
            UpdateActiveSFXVolumes();
        }

        public void SetMasterMuted(bool mute)
        {
            AudioPreferences.MasterMuted = mute;
            UpdateAllVolumes();
        }

        private void UpdateAllVolumes()
        {
            // Update BGM volume
            if (_bgmAudioSource != null)
            {
                _bgmAudioSource.volume = AudioPreferences.GetNormalizedVolume(AudioPreferences.BGMVolume);
            }

            // Update SFX volumes
            UpdateActiveSFXVolumes();
        }

        private void UpdateActiveSFXVolumes()
        {
            var normalizedVolume = AudioPreferences.GetNormalizedVolume(AudioPreferences.SFXVolume);
            foreach (var source in _activeAudioSources)
            {
                if (source != null && source.clip != null)
                {
                    // Apply config volume multiplier if available
                    var config = AudioConfig.GetByAudioId(GetAudioIdFromClip(source.clip));
                    var volume = normalizedVolume;
                    volume *= config.DefaultVolume / 100f;
                    source.volume = volume;
                }
            }
        }

        private AudioIdType GetAudioIdFromClip(AudioClip clip)
        {
            foreach (var kv in _audioCache)
            {
                if (kv.Value == clip)
                    return kv.Key;
            }
            return default;
        }
        #endregion

        #region Asset Management
        public async Task PreloadAudioAsync(AudioIdType audioId)
        {
            if (!IsInitialized)
            {
                LogHelper.LogWarning("[AudioManager] Not initialized");
                return;
            }

            if (_audioCache.ContainsKey(audioId))
            {
                return; // Already loaded
            }

            try
            {
                var audioClip = await LoadAudioClipAsync(audioId);
                if (audioClip != null)
                {
                    LogHelper.Log($"[AudioManager] Preloaded audio: {audioId}");
                }
            }
            catch (Exception e)
            {
                LogHelper.LogError($"[AudioManager] Failed to preload audio {audioId}: {e.Message}");
            }
        }

        public void UnloadAudio(AudioIdType audioId)
        {
            if (_audioCache.TryGetValue(audioId, out var clip))
            {
                _audioCache.Remove(audioId);
                var audioData = AudioConfig.GetByAudioId(audioId);
                if (audioData != null)
                {
                    ResManager.Release(audioData.GetAssetPath());
                }
                LogHelper.Log($"[AudioManager] Unloaded audio: {audioId}");
            }
        }

        private async Task<AudioClip> LoadAudioClipAsync(AudioIdType audioId)
        {
            // Return cached clip if available
            if (_audioCache.TryGetValue(audioId, out var cachedClip) && cachedClip != null)
            {
                return cachedClip;
            }

            // Get audio configuration
            var config = AudioConfig.GetByAudioId(audioId);
            if (config == null)
            {
                LogHelper.LogError($"[AudioManager] No configuration found for audio: {audioId}");
                return null;
            }

            try
            {
                var audioClip = await ResManager.LoadAssetAsync<AudioClip>(config.GetAssetPath());
                if (audioClip != null)
                {
                    _audioCache[audioId] = audioClip;
                    return audioClip;
                }

                LogHelper.LogError($"[AudioManager] Failed to load audio asset: {config.GetAssetPath()}");
                return null;
            }
            catch (Exception e)
            {
                LogHelper.LogError($"[AudioManager] Error loading audio {audioId}: {e.Message}");
                return null;
            }
        }
        #endregion
    }
}