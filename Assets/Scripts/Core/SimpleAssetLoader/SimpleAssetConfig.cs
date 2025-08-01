using UnityEngine;
using UnityEngine.U2D;

namespace YuankunHuang.SynthMind.Core
{
    /// <summary>
    /// 简单实用的资源配置
    /// </summary>
    [CreateAssetMenu(fileName = "SimpleAssetConfig", menuName = "SynthMind/Simple Asset Config")]
    public class SimpleAssetConfig : ScriptableObject
    {
        [Header("Atlas配置")]
        [SerializeField] private SpriteAtlas[] spriteAtlases;
        
        [Header("缓存设置")]
        [SerializeField] private bool enableCaching = true;
        [SerializeField] private int maxCacheSize = 100;
        
        [Header("优化设置")]
        [SerializeField] private bool enableLogging = true;
        [SerializeField] private bool autoInitialize = true;

        public SpriteAtlas[] SpriteAtlases => spriteAtlases;
        public bool EnableCaching => enableCaching;
        public int MaxCacheSize => maxCacheSize;
        public bool EnableLogging => enableLogging;
        public bool AutoInitialize => autoInitialize;

        /// <summary>
        /// 应用配置到SmartAssetLoader
        /// </summary>
        public void ApplyConfig()
        {
            // 设置缓存选项
            SmartAssetLoader.SetCacheOptions(enableCaching, maxCacheSize);
            
            // 添加Atlas
            if (spriteAtlases != null)
            {
                foreach (var atlas in spriteAtlases)
                {
                    if (atlas != null)
                    {
                        SmartAssetLoader.AddAtlas(atlas.name, atlas);
                    }
                }
            }
            
            Debug.Log($"SimpleAssetConfig 已应用，Atlas数量: {spriteAtlases?.Length ?? 0}");
        }
    }
} 