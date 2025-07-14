using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace YuankunHuang.Unity.Core
{
    public class SceneManager
    {
        // scenes
        private static readonly Dictionary<string, AsyncOperationHandle<SceneInstance>> _sceneHandles = new();
        private static Dictionary<string, HashSet<string>> _groupMap = new();
        private static readonly object _sceneLock = new();

        #region Scene Loading & Unloading
        public static async Task LoadSceneAsync(string key, LoadSceneMode mode = LoadSceneMode.Additive, string group = null)
        {
            lock (_sceneLock)
            {
                if (_sceneHandles.ContainsKey(key))
                {
                    Logger.LogError($"[ResManager]::LoadSceneAsync: Same scene has already been loaded: {key}");
                    return;
                }
            }

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
                Logger.LogError($"[ResManager]::LoadSceneAsync: {key}");
            }
        }

        public static async Task UnloadSceneAsync(string key)
        {
            AsyncOperationHandle<SceneInstance> handle;
            lock (_sceneLock)
            {
                if (!_sceneHandles.TryGetValue(key, out handle))
                {
                    Logger.LogError($"[ResManager]::UnloadSceneAsync: Trying to unload a scene not loaded: {key}");
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
        #endregion
    }
}