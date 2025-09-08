using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using YuankunHuang.Unity.ModuleCore;

namespace YuankunHuang.Unity.AssetCore
{
    [CreateAssetMenu(fileName = "AssetManagerConfig", menuName = "AssetManager/AssetManagerConfig", order = 1)]
    public class AssetManagerConfig : ScriptableObject, IAssetManagerConfig
    {
        [SerializeField] private List<AssetEntry> _assetEntries = new List<AssetEntry>();

        public List<AssetEntry> AssetEntries => _assetEntries;

        public UnityEngine.Object GetAsset(string key)
        {
            var entry = _assetEntries.Find(e => e.key == key);
            if (entry != null)
            {
                return entry.asset;
            }
            Debug.LogError($"Cannot find asset with key: {key}");
            return null;
        }

#if UNITY_EDITOR
        private Sprite TryGetSpriteFromTexture2D(Texture2D texture2D)
        {
            var assetPath = AssetDatabase.GetAssetPath(texture2D);
            if (string.IsNullOrEmpty(assetPath)) return null;

            var textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (textureImporter != null && textureImporter.textureType == TextureImporterType.Sprite)
            {
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                if (sprite != null)
                {
                    return sprite;
                }

                var allAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath);
                foreach (var subAsset in allAssets)
                {
                    if (subAsset is Sprite spriteAsset)
                    {
                        return spriteAsset;
                    }
                }
            }

            return null;
        }

        public void AddAsset(UnityEngine.Object asset)
        {
            if (asset == null)
            {
                Debug.LogError("Cannot add null asset.");
                return;
            }

            if (asset is Texture2D texture2D)
            {
                var sprite = TryGetSpriteFromTexture2D(texture2D);
                if (sprite != null)
                {
                    asset = sprite;
                    Debug.Log($"Automatically converted Texture2D to Sprite: {sprite.name}");
                }
            }

            var key = GenerateUniqueKey(asset);
            var entry = new AssetEntry()
            {
                key = key,
                asset = asset,
            };
            entry.AutoDetectType();
            _assetEntries.Add(entry);
            UnityEditor.EditorUtility.SetDirty(this);
        }

        public bool HasKey(string key)
        {
            return _assetEntries.Exists(e => e.key == key);
        }

        public string GenerateUniqueKey(UnityEngine.Object asset)
        {
            var baseKey = asset.name;
            var key = baseKey;
            var counter = 1;
            while (HasKey(key))
            {
                key = $"{baseKey}_{counter}";
                ++counter;
            }
            return key;
        }

        public void ClearInvalidEntries()
        {
            _assetEntries.RemoveAll(e => e.asset == null);
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}