using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine;
using System;

namespace YuankunHuang.SynthMind.Core
{
    public class SceneManager
    {
        // scenes
        private static readonly Dictionary<string, AsyncOperationHandle<SceneInstance>> _sceneHandles = new();
        private static Dictionary<string, HashSet<string>> _groupMap = new();
        private static readonly object _sceneLock = new();

        #region Scene Loading & Unloading
        public static async void LoadSceneAsync(string key, LoadSceneMode mode = LoadSceneMode.Additive, string group = null, Action onFinished = null)
        {
            lock (_sceneLock)
            {
                if (_sceneHandles.ContainsKey(key))
                {
                    LogHelper.LogError($"[ResManager]::LoadSceneAsync: Same scene has already been loaded: {key}");
                    onFinished?.Invoke();
                    return;
                }
            }

            try
            {
                var handle = Addressables.LoadSceneAsync(key, mode);
                await handle.Task;

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    lock (_sceneLock)
                    {
                        _sceneHandles[key] = handle;

                        if (!string.IsNullOrEmpty(group))
                        {
                            if (!_groupMap.TryGetValue(group, out var set))
                            {
                                set = new();
                                _groupMap[group] = set;
                            }
                            set.Add(key);
                        }
                    }
                }
                else
                {
                    LogHelper.LogError($"[ResManager]::LoadSceneAsync failed: {key}");
                }
            }
            catch (Exception e)
            {
                LogHelper.LogException(e);
            }

            onFinished?.Invoke();
        }

        public static async void UnloadSceneAsync(string key, Action onFinished = null)
        {
            AsyncOperationHandle<SceneInstance> handle;
            lock (_sceneLock)
            {
                if (!_sceneHandles.TryGetValue(key, out handle))
                {
                    LogHelper.LogError($"[ResManager]::UnloadSceneAsync: Trying to unload a scene not loaded: {key}");
                    return;
                }
            }

            if (handle.IsValid())
            {
                var unloadOp = Addressables.UnloadSceneAsync(handle);
                await unloadOp.Task;
                lock (_sceneLock)
                {
                    _sceneHandles.Remove(key);
                    foreach (var set in _groupMap.Values)
                    {
                        set.Remove(key);
                    }
                }
            }
        }

        public static bool IsSceneLoaded(string key)
        {
            lock (_sceneLock)
            {
                return _sceneHandles.ContainsKey(key);
            }
        }

        public static void UnloadAll(Action onFinished)
        {
            var count = _sceneHandles.Count;
            foreach (var kv in _sceneHandles)
            {
                Addressables.UnloadSceneAsync(kv.Value).Completed += handle =>
                {
                    if (--count < 1)
                    {
                        _sceneHandles.Clear();
                        onFinished?.Invoke();
                    }
                };
            }
        }
        #endregion
    }
}