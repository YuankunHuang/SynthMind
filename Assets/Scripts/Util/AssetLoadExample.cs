using UnityEngine;
using UnityEngine.UI;
using YuankunHuang.Unity.Core;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace YuankunHuang.Unity.Util
{
    /// <summary>
    /// 智能资源加载使用示例 - 避免死锁版本
    /// </summary>
    public class AssetLoadExample : MonoBehaviour
    {
        [Header("UI组件")]
        [SerializeField] private Image buttonImage;
        [SerializeField] private Image frameImage;
        
        [Header("测试配置")]
        [SerializeField] private string testSpriteName = "Frame 2166";
        [SerializeField] private string testAtlasName = "UIAtlas";

        [Header("可选配置")]
        [SerializeField] private SimpleAssetConfig assetConfig;

        private async void Start()
        {
            // 初始化加载器（自动查找Atlas）
            SmartAssetLoader.Initialize();
            
            // 可选：应用自定义配置
            if (assetConfig != null)
            {
                assetConfig.ApplyConfig();
            }
            
            // 运行优化检查
            SmartAssetLoader.CheckAllAssetsOptimization();
            
            // 异步预加载常用资源
            await PreloadCommonSprites();
        }

        /// <summary>
        /// 预加载常用资源
        /// </summary>
        private async Task PreloadCommonSprites()
        {
            string[] commonSprites = {
                "Frame 2166",
                "Frame 2167",
                "Group 256",
                "Group 257"
            };
            
            await SmartAssetLoader.PreloadSpritesAsync(commonSprites);
        }

        /// <summary>
        /// 测试从Atlas加载Sprite（同步，安全）
        /// </summary>
        [ContextMenu("测试Atlas加载")]
        public void TestAtlasLoading()
        {
            Sprite sprite = SmartAssetLoader.LoadSprite(testSpriteName, testAtlasName);
            if (sprite != null && buttonImage != null)
            {
                buttonImage.sprite = sprite;
                LogHelper.Log($"成功从Atlas加载: {testSpriteName}");
            }
        }

        /// <summary>
        /// 测试从ResManager加载Sprite（异步，推荐）
        /// </summary>
        [ContextMenu("测试ResManager异步加载")]
        public async void TestResManagerAsyncLoading()
        {
            Sprite sprite = await SmartAssetLoader.LoadSpriteAsync(testSpriteName);
            if (sprite != null && frameImage != null)
            {
                frameImage.sprite = sprite;
                LogHelper.Log($"成功从ResManager异步加载: {testSpriteName}");
                
                // 检查优化状态（异步版本）
                string suggestion = await SmartAssetLoader.GetOptimizationSuggestionAsync(testSpriteName);
                LogHelper.Log(suggestion);
            }
        }

        /// <summary>
        /// 测试异步加载
        /// </summary>
        [ContextMenu("测试异步加载")]
        public async void TestAsyncLoading()
        {
            Sprite sprite = await SmartAssetLoader.LoadSpriteAsync(testSpriteName);
            if (sprite != null && frameImage != null)
            {
                frameImage.sprite = sprite;
                LogHelper.Log($"成功异步加载: {testSpriteName}");
            }
        }

        /// <summary>
        /// 批量测试资源加载（异步，推荐）
        /// </summary>
        [ContextMenu("批量异步测试")]
        public async void TestAsyncBatchLoading()
        {
            string[] testSprites = {
                "Frame 2166",
                "Frame 2167", 
                "Frame 2168",
                "Group 256",
                "Group 257"
            };

            var tasks = new List<Task<Sprite>>();
            foreach (string spriteName in testSprites)
            {
                tasks.Add(SmartAssetLoader.LoadSpriteAsync(spriteName));
            }

            var results = await Task.WhenAll(tasks);
            
            for (int i = 0; i < testSprites.Length; i++)
            {
                if (results[i] != null)
                {
                    LogHelper.Log($"✅ 异步加载成功: {testSprites[i]} ({results[i].texture.width}x{results[i].texture.height})");
                    
                    // 检查是否为优化尺寸
                    bool isOptimal = SmartAssetLoader.IsOptimalSize(results[i]);
                    if (!isOptimal)
                    {
                        LogHelper.LogWarning($"⚠️ {testSprites[i]} 建议放入Atlas");
                    }
                }
                else
                {
                    LogHelper.LogError($"❌ 异步加载失败: {testSprites[i]}");
                }
            }
        }

        /// <summary>
        /// 批量测试Atlas资源加载（同步，安全）
        /// </summary>
        [ContextMenu("批量Atlas测试")]
        public void TestAtlasBatchLoading()
        {
            string[] testSprites = {
                "Frame 2166",
                "Frame 2167", 
                "Frame 2168",
                "Group 256",
                "Group 257"
            };

            foreach (string spriteName in testSprites)
            {
                Sprite sprite = SmartAssetLoader.LoadSprite(spriteName);
                if (sprite != null)
                {
                    LogHelper.Log($"✅ Atlas加载成功: {spriteName} ({sprite.texture.width}x{sprite.texture.height})");
                    
                    // 检查是否为优化尺寸
                    bool isOptimal = SmartAssetLoader.IsOptimalSize(sprite);
                    if (!isOptimal)
                    {
                        LogHelper.LogWarning($"⚠️ {spriteName} 建议放入Atlas");
                    }
                }
                else
                {
                    LogHelper.LogError($"❌ Atlas加载失败: {spriteName}");
                }
            }
        }

        /// <summary>
        /// 性能测试
        /// </summary>
        [ContextMenu("性能测试")]
        public void PerformanceTest()
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            
            // 测试Atlas加载性能
            stopwatch.Start();
            for (int i = 0; i < 100; i++)
            {
                SmartAssetLoader.LoadSprite(testSpriteName, testAtlasName);
            }
            stopwatch.Stop();
            LogHelper.Log($"Atlas加载100次耗时: {stopwatch.ElapsedMilliseconds}ms");

            // 测试异步加载性能（不阻塞）
            LogHelper.Log("异步加载性能测试需要在实际使用中观察");
        }

        /// <summary>
        /// 清理缓存测试
        /// </summary>
        [ContextMenu("清理缓存")]
        public void ClearCacheTest()
        {
            SmartAssetLoader.ClearCache();
            LogHelper.Log("缓存已清理");
        }

        /// <summary>
        /// 获取缓存信息
        /// </summary>
        [ContextMenu("获取缓存信息")]
        public void GetCacheInfo()
        {
            string cacheInfo = SmartAssetLoader.GetCacheInfo();
            LogHelper.Log(cacheInfo);
        }

        /// <summary>
        /// 预加载资源
        /// </summary>
        [ContextMenu("预加载Atlas资源")]
        public void PreloadSprites()
        {
            string[] spritesToPreload = {
                "Frame 2166",
                "Frame 2167",
                "Group 256",
                "Group 257"
            };
            
            SmartAssetLoader.PreloadSprites(spritesToPreload);
        }

        /// <summary>
        /// 异步预加载资源
        /// </summary>
        [ContextMenu("异步预加载资源")]
        public async void PreloadSpritesAsync()
        {
            string[] spritesToPreload = {
                "Frame 2166",
                "Frame 2167",
                "Group 256",
                "Group 257"
            };
            
            await SmartAssetLoader.PreloadSpritesAsync(spritesToPreload);
        }

        /// <summary>
        /// 获取资源优化建议（异步版本）
        /// </summary>
        [ContextMenu("获取异步优化建议")]
        public async void GetAsyncOptimizationSuggestions()
        {
            string[] testSprites = {
                "Frame 2166",
                "Frame 2167",
                "Group 256",
                "Group 257"
            };

            LogHelper.Log("=== 异步资源优化建议 ===");
            foreach (string spriteName in testSprites)
            {
                string suggestion = await SmartAssetLoader.GetOptimizationSuggestionAsync(spriteName);
                LogHelper.Log(suggestion);
            }
        }

        /// <summary>
        /// 获取资源优化建议（Atlas版本）
        /// </summary>
        [ContextMenu("获取Atlas优化建议")]
        public void GetAtlasOptimizationSuggestions()
        {
            string[] testSprites = {
                "Frame 2166",
                "Frame 2167",
                "Group 256",
                "Group 257"
            };

            LogHelper.Log("=== Atlas资源优化建议 ===");
            foreach (string spriteName in testSprites)
            {
                string suggestion = SmartAssetLoader.GetOptimizationSuggestion(spriteName);
                LogHelper.Log(suggestion);
            }
        }

        /// <summary>
        /// 动态加载UI资源示例
        /// </summary>
        public async void LoadUIResourcesAsync()
        {
            // 异步加载按钮背景
            Sprite buttonSprite = await SmartAssetLoader.LoadSpriteAsync("Frame 2166");
            if (buttonSprite != null && buttonImage != null)
            {
                buttonImage.sprite = buttonSprite;
            }

            // 异步加载框架背景
            Sprite frameSprite = await SmartAssetLoader.LoadSpriteAsync("Group 256");
            if (frameSprite != null && frameImage != null)
            {
                frameImage.sprite = frameSprite;
            }
        }

        /// <summary>
        /// 按需加载示例（异步）
        /// </summary>
        /// <param name="spriteName">要加载的Sprite名称</param>
        /// <param name="targetImage">目标Image组件</param>
        public async void LoadSpriteOnDemandAsync(string spriteName, Image targetImage)
        {
            if (targetImage == null) return;

            Sprite sprite = await SmartAssetLoader.LoadSpriteAsync(spriteName);
            if (sprite != null)
            {
                targetImage.sprite = sprite;
                LogHelper.Log($"异步按需加载成功: {spriteName}");
            }
            else
            {
                LogHelper.LogError($"异步按需加载失败: {spriteName}");
            }
        }

        /// <summary>
        /// 按需加载示例（Atlas，同步）
        /// </summary>
        /// <param name="spriteName">要加载的Sprite名称</param>
        /// <param name="targetImage">目标Image组件</param>
        public void LoadSpriteOnDemandAtlas(string spriteName, Image targetImage)
        {
            if (targetImage == null) return;

            Sprite sprite = SmartAssetLoader.LoadSprite(spriteName);
            if (sprite != null)
            {
                targetImage.sprite = sprite;
                LogHelper.Log($"Atlas按需加载成功: {spriteName}");
            }
            else
            {
                LogHelper.LogError($"Atlas按需加载失败: {spriteName}");
            }
        }

        /// <summary>
        /// 设置缓存选项
        /// </summary>
        [ContextMenu("设置缓存选项")]
        public void SetCacheOptions()
        {
            SmartAssetLoader.SetCacheOptions(true, 50);
            LogHelper.Log("缓存选项已设置：启用缓存，最大50个");
        }

        /// <summary>
        /// 获取所有Atlas名称
        /// </summary>
        [ContextMenu("获取Atlas列表")]
        public void GetAllAtlasNames()
        {
            string[] atlasNames = SmartAssetLoader.GetAllAtlasNames();
            LogHelper.Log($"找到 {atlasNames.Length} 个Atlas:");
            foreach (string name in atlasNames)
            {
                LogHelper.Log($"- {name}");
            }
        }

        /// <summary>
        /// 释放资源示例
        /// </summary>
        [ContextMenu("释放资源")]
        public void ReleaseSprite()
        {
            SmartAssetLoader.ReleaseSprite(testSpriteName);
        }
    }
} 