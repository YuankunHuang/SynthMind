using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace YuankunHuang.Unity.Core
{
    public class ResHandle<T> : ResHandle where T : UnityEngine.Object
    {
        public new AsyncOperationHandle<T> Handle { get; private set; }
        public T Asset => Handle.Result;

        public ResHandle(AsyncOperationHandle<T> handle, string group = null)
        {
            base.Handle = Handle = handle;
            Group = group;
        }
    }

    public abstract class ResHandle
    {
        public int RefCount { get; protected set; } = 1;
        public AsyncOperationHandle Handle { get; protected set; }
        public string Group { get; protected set; }
        public DateTime LastAccessTime { get; protected set; }

        public void Retain()
        {
            ++RefCount;
            LastAccessTime = DateTime.Now;
        }

        public void Release()
        {
            if (RefCount <= 0)
            {
                LogHelper.LogError($"[ResHandle]::Release called too many times.");
                return;
            }

            RefCount--;
            if (RefCount < 1)
            {
                Addressables.Release(Handle);
            }
        }
    }

    public static class ResManager
    {
        // general assets
        private static Dictionary<string, ResHandle> _loaded = new();
        private static Dictionary<string, object> _loading = new(); // 修改为object类型，支持泛型Task<T>
        private static object _lock = new();
        private static Dictionary<string, HashSet<string>> _groupMap = new();

        #region General Asset Loading
        public static async Task<T> LoadAssetAsync<T>(string key, string group = null) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(key))
            {
                LogHelper.LogError($"[ResManager]::LoadAssetAsync: key is empty");
                return null;
            }

            Task<T> loadingTask = null;
            AsyncOperationHandle<T> handle = default;

            lock (_lock)
            {
                if (_loaded.TryGetValue(key, out var baseHandle))
                {
                    var resHandle = baseHandle as ResHandle<T>;
                    resHandle.Retain();
                    return resHandle.Asset;
                }

                if (!_loading.TryGetValue(key, out var loadingObj))
                {
                    handle = Addressables.LoadAssetAsync<T>(key);
                    loadingTask = handle.Task;
                    _loading[key] = loadingTask;
                }
                else
                {
                    loadingTask = loadingObj as Task<T>;
                }
            }

            // Await the task outside the lock
            var asset = await loadingTask;

            lock (_lock)
            {
                _loading.Remove(key);

                if (_loaded.TryGetValue(key, out var existingBaseHandle))
                {
                    // Already loaded by another thread while we were waiting
                    var existingHandle = existingBaseHandle as ResHandle<T>;
                    existingHandle.Retain();
                    return existingHandle.Asset;
                }

                var resHandle = new ResHandle<T>(handle, group);
                _loaded[key] = resHandle;

                if (!string.IsNullOrEmpty(group))
                {
                    if (!_groupMap.TryGetValue(group, out var set))
                    {
                        set = new HashSet<string>();
                        _groupMap[group] = set;
                    }
                    set.Add(key);
                }

                return resHandle.Asset;
            }
        }
        #endregion

        #region General Asset Release
        public static void Release(string key)
        {
            lock (_lock)
            {
                if (_loaded.TryGetValue(key, out var baseHandle))
                {
                    baseHandle.Release();

                    if (baseHandle.RefCount < 1)
                    {
                        _loaded.Remove(key);
                        if (!string.IsNullOrEmpty(baseHandle.Group))
                        {
                            if (_groupMap.TryGetValue(baseHandle.Group, out var set))
                            {
                                set.Remove(key);
                                if (set.Count < 1)
                                {
                                    _groupMap.Remove(baseHandle.Group);
                                }
                            }
                        }
                    }
                }
                else
                {
                    LogHelper.LogError($"[ResManager]::Release: Try to release an asset not loaded: {key}");
                }
            }
        }

        public static void ReleaseGroup(string group)
        {
            lock (_lock)
            {
                if (_groupMap.TryGetValue(group, out var set))
                {
                    var keys = new List<string>(set);
                    foreach (var key in keys)
                    {
                        Release(key);
                    }

                    if (_groupMap.ContainsKey(group))
                    {
                        _groupMap.Remove(group);
                    }
                }
                else
                {
                    LogHelper.LogError($"[ResManager]::ReleaseGroup: Trying to release asset group not loaded: {group}");
                }
            }
        }
        #endregion
    }
}