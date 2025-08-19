using System.Collections.Generic;
using UnityEngine;
using YuankunHuang.Unity.ModuleCore;

namespace YuankunHuang.Unity.AssetCore
{
    public enum AssetType
    {
        None = 0,
        Texture2D = 1,
        Sprite = 2,
        AudioClip = 3,
        GameObject = 4,
        Material = 5,
        Shader = 6,
        AnimationClip = 7,
        Font = 8,
        TextAsset = 9,
        ScriptableObject = 10,
        TMP_ColorGradient = 11,
        Animation = 12,
        SpriteAtlas = 13,
        TMP_FontAsset = 14,
        AnimatorController = 15,
    }

    public interface IAssetManagerConfig
    {
        UnityEngine.Object GetAsset(string key);
    }

    public interface IAssetManager : IModule
    {
        T GetAsset<T>(string key) where T : UnityEngine.Object;
        void Initialize(IAssetManagerConfig config);
    }
}