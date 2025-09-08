using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.U2D;
using YuankunHuang.Unity.Core;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace YuankunHuang.Unity.AssetCore
{
    [System.Serializable]
    public class AssetEntry
    {
        public string key;
        public UnityEngine.Object asset;
        public AssetType type;

#if UNITY_EDITOR
        private AssetType DetectType()
        {
            if (asset == null) return AssetType.None;

            if (asset is Texture2D)
            {
                var path = AssetDatabase.GetAssetPath(asset);
                var ti = AssetImporter.GetAtPath(path) as TextureImporter;
                if (ti != null && ti.textureType == TextureImporterType.Sprite)
                {
                    return AssetType.Sprite;
                }
                var subAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);
                if (subAssets != null && subAssets.Length > 0)
                {
                    foreach (var subAsset in subAssets)
                    {
                        if (subAsset is Sprite) return AssetType.Sprite;
                    }
                }
                return AssetType.Texture2D;
            }
            if (asset is Sprite) return AssetType.Sprite;
            if (asset is GameObject) return AssetType.GameObject;
            if (asset is Font) return AssetType.Font;
            if (asset is TMP_ColorGradient) return AssetType.TMP_ColorGradient;
            if (asset is AnimationClip) return AssetType.AnimationClip;
            if (asset is AudioClip) return AssetType.AudioClip;
            if (asset is Material) return AssetType.Material;
            if (asset is Shader) return AssetType.Shader;
            if (asset is TextAsset) return AssetType.TextAsset;
            if (asset is ScriptableObject) return AssetType.ScriptableObject;
            if (asset is Animation) return AssetType.Animation;
            if (asset is SpriteAtlas) return AssetType.SpriteAtlas;
            if (asset is RuntimeAnimatorController) return AssetType.AnimatorController;

            return AssetType.None;
        }

        public void AutoDetectType()
        {
            type = DetectType();
        }

        public bool IsTypeMatched() 
        {
            return type == DetectType();
        }
#endif
    }

    public class AssetManager : IAssetManager
    {
        public bool IsInitialized { get; private set; } = false;

        private AssetManagerConfig _config;
        private Dictionary<string, UnityEngine.Object> _cache;

        public AssetManager()
        {
            _cache = new Dictionary<string, UnityEngine.Object>();
        }

        public void Initialize(IAssetManagerConfig config)
        {
            if (IsInitialized)
            {
                LogHelper.LogWarning($"AssetManager is already initialized.");
                return;
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _config = (AssetManagerConfig)config;
            IsInitialized = true;

            Debug.Log($"[AssetManager] Initialized with AssetManagerConfig: {_config}.");
        }

        public void Dispose()
        {
            _cache.Clear();
            _config = null;
            IsInitialized = false;
            Debug.Log($"[AssetManager] Disposed.");
        }

        public T GetAsset<T>(string key) where T : UnityEngine.Object
        {
            if (_cache.TryGetValue(key, out var cachedAsset))
            {
                return cachedAsset as T;
            }
            var asset = _config.GetAsset(key);
            if (asset != null && typeof(T).IsAssignableFrom(asset.GetType()))
            {
                _cache[key] = (T)asset;
                return (T)asset;
            }
            Debug.LogError($"Cannot find asset with key: {key} or type mismatch.");
            return null;
        }
    }
}