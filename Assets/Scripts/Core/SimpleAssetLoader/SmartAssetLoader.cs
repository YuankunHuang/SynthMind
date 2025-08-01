using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.U2D;

namespace YuankunHuang.SynthMind.Core
{
    /// <summary>
    /// 智能资源加载器 - 使用ResManager，简单实用
    /// </summary>
    public static class SmartAssetLoader
    {
        // 缓存
        private static Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();
        private static Dictionary<string, SpriteAtlas> atlasCache = new Dictionary<string, SpriteAtlas>();
        
        // 简单配置
        private static bool enableCaching = true;
        private static int maxCacheSize = 100;
        private static bool isInitialized = false;
        
        // 线程安全锁
        private static readonly object cacheLock = new object();

        /// <summary>
        /// 初始化加载器
        /// </summary>
        public static void Initialize()
        {
            if (isInitialized) return;
            
            // 自动查找所有SpriteAtlas
            SpriteAtlas[] allAtlases = Resources.FindObjectsOfTypeAll<SpriteAtlas>();
            foreach (var atlas in allAtlases)
            {
                atlasCache[atlas.name] = atlas;
            }
            
            isInitialized = true;
            LogHelper.Log($"SmartAssetLoader 已初始化，找到 {atlasCache.Count} 个Atlas");
        }

        /// <summary>
        /// 智能加载Sprite - 使用ResManager（仅异步版本）
        /// </summary>
        /// <param name="spriteName">Sprite名称</param>
        /// <param name="atlasName">Atlas名称（可选）</param>
        /// <returns>加载的Sprite</returns>
        public static Sprite LoadSprite(string spriteName, string atlasName = null)
        {
            // 注意：这个方法现在只处理Atlas加载，ResManager加载请使用异步版本
            if (!isInitialized)
            {
                Initialize();
            }

            // 1. 检查缓存
            lock (cacheLock)
            {
                if (spriteCache.ContainsKey(spriteName))
                {
                    return spriteCache[spriteName];
                }
            }

            // 2. 尝试从Atlas加载
            if (!string.IsNullOrEmpty(atlasName) && atlasCache.ContainsKey(atlasName))
            {
                Sprite sprite = atlasCache[atlasName].GetSprite(spriteName);
                if (sprite != null)
                {
                    CacheSprite(spriteName, sprite);
                    LogHelper.Log($"从Atlas加载: {atlasName}/{spriteName}");
                    return sprite;
                }
            }

            // 3. 尝试从所有Atlas中查找
            foreach (var atlas in atlasCache.Values)
            {
                Sprite sprite = atlas.GetSprite(spriteName);
                if (sprite != null)
                {
                    CacheSprite(spriteName, sprite);
                    LogHelper.Log($"从Atlas自动查找: {atlas.name}/{spriteName}");
                    return sprite;
                }
            }

            // 4. 对于ResManager资源，建议使用异步版本
            LogHelper.LogWarning($"建议使用LoadSpriteAsync加载ResManager资源: {spriteName}");
            return null;
        }

        /// <summary>
        /// 异步加载Sprite - 使用ResManager
        /// </summary>
        /// <param name="spriteName">Sprite名称</param>
        /// <param name="atlasName">Atlas名称（可选）</param>
        /// <returns>异步加载任务</returns>
        public static async Task<Sprite> LoadSpriteAsync(string spriteName, string atlasName = null)
        {
            if (!isInitialized)
            {
                Initialize();
            }

            // 1. 检查缓存
            lock (cacheLock)
            {
                if (spriteCache.ContainsKey(spriteName))
                {
                    return spriteCache[spriteName];
                }
            }

            // 2. 尝试从Atlas加载
            if (!string.IsNullOrEmpty(atlasName) && atlasCache.ContainsKey(atlasName))
            {
                Sprite sprite = atlasCache[atlasName].GetSprite(spriteName);
                if (sprite != null)
                {
                    CacheSprite(spriteName, sprite);
                    LogHelper.Log($"从Atlas加载: {atlasName}/{spriteName}");
                    return sprite;
                }
            }

            // 3. 尝试从所有Atlas中查找
            foreach (var atlas in atlasCache.Values)
            {
                Sprite sprite = atlas.GetSprite(spriteName);
                if (sprite != null)
                {
                    CacheSprite(spriteName, sprite);
                    LogHelper.Log($"从Atlas自动查找: {atlas.name}/{spriteName}");
                    return sprite;
                }
            }

            // 4. 使用ResManager异步加载
            Sprite directSprite = await LoadSpriteFromResManagerAsync(spriteName);
            if (directSprite != null)
            {
                LogHelper.Log($"从ResManager异步加载: {spriteName}");
                return directSprite;
            }

            LogHelper.LogError($"无法加载Sprite: {spriteName}");
            return null;
        }

        /// <summary>
        /// 使用ResManager异步加载Sprite
        /// </summary>
        /// <param name="spriteName">Sprite名称</param>
        /// <returns>异步加载的Sprite</returns>
        private static async Task<Sprite> LoadSpriteFromResManagerAsync(string spriteName)
        {
            try
            {
                return await ResManager.LoadAssetAsync<Sprite>(spriteName);
            }
            catch (Exception ex)
            {
                LogHelper.LogError($"ResManager异步加载失败: {spriteName}, 错误: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 缓存Sprite（线程安全）
        /// </summary>
        /// <param name="spriteName">Sprite名称</param>
        /// <param name="sprite">Sprite对象</param>
        private static void CacheSprite(string spriteName, Sprite sprite)
        {
            if (enableCaching && sprite != null)
            {
                lock (cacheLock)
                {
                    // 检查缓存大小限制
                    if (spriteCache.Count >= maxCacheSize)
                    {
                        // 简单的LRU策略：移除第一个
                        var firstKey = spriteCache.Keys.GetEnumerator();
                        if (firstKey.MoveNext())
                        {
                            spriteCache.Remove(firstKey.Current);
                        }
                    }
                    
                    spriteCache[spriteName] = sprite;
                }
            }
        }

        /// <summary>
        /// 检查Sprite尺寸是否为4的倍数
        /// </summary>
        /// <param name="sprite">要检查的Sprite</param>
        /// <returns>是否为4的倍数</returns>
        public static bool IsOptimalSize(Sprite sprite)
        {
            if (sprite == null || sprite.texture == null) return false;
            
            return sprite.texture.width % 4 == 0 && sprite.texture.height % 4 == 0;
        }

        /// <summary>
        /// 获取Sprite的优化建议（异步版本）
        /// </summary>
        /// <param name="spriteName">Sprite名称</param>
        /// <returns>优化建议</returns>
        public static async Task<string> GetOptimizationSuggestionAsync(string spriteName)
        {
            Sprite sprite = await LoadSpriteAsync(spriteName);
            if (sprite == null) return "Sprite不存在";

            if (IsOptimalSize(sprite))
            {
                return $"✅ {spriteName} 尺寸优化 ({sprite.texture.width}x{sprite.texture.height})";
            }
            else
            {
                return $"⚠️ {spriteName} 建议放入Atlas ({sprite.texture.width}x{sprite.texture.height})";
            }
        }

        /// <summary>
        /// 获取Sprite的优化建议（仅用于Atlas资源）
        /// </summary>
        /// <param name="spriteName">Sprite名称</param>
        /// <returns>优化建议</returns>
        public static string GetOptimizationSuggestion(string spriteName)
        {
            // 只检查Atlas中的资源，避免死锁
            foreach (var atlas in atlasCache.Values)
            {
                Sprite sprite = atlas.GetSprite(spriteName);
                if (sprite != null)
                {
                    if (IsOptimalSize(sprite))
                    {
                        return $"✅ {spriteName} 尺寸优化 ({sprite.texture.width}x{sprite.texture.height})";
                    }
                    else
                    {
                        return $"⚠️ {spriteName} 建议放入Atlas ({sprite.texture.width}x{sprite.texture.height})";
                    }
                }
            }
            
            return $"❓ {spriteName} 未找到或需要异步加载";
        }

        /// <summary>
        /// 批量检查资源优化状态
        /// </summary>
        public static void CheckAllAssetsOptimization()
        {
            LogHelper.Log("=== 资源优化检查 ===");
            
            // 检查Atlas中的资源
            foreach (var atlas in atlasCache.Values)
            {
                Sprite[] sprites = new Sprite[atlas.spriteCount];
                atlas.GetSprites(sprites);
                
                foreach (var sprite in sprites)
                {
                    if (sprite != null)
                    {
                        LogHelper.Log($"Atlas {atlas.name}: {sprite.name} - {sprite.texture.width}x{sprite.texture.height}");
                    }
                }
            }
        }

        /// <summary>
        /// 清理缓存（线程安全）
        /// </summary>
        public static void ClearCache()
        {
            lock (cacheLock)
            {
                spriteCache.Clear();
            }
            LogHelper.Log("资源缓存已清理");
        }

        /// <summary>
        /// 获取缓存统计信息（线程安全）
        /// </summary>
        /// <returns>缓存信息</returns>
        public static string GetCacheInfo()
        {
            lock (cacheLock)
            {
                return $"缓存Sprite数量: {spriteCache.Count}, Atlas数量: {atlasCache.Count}";
            }
        }

        /// <summary>
        /// 预加载资源（仅Atlas资源）
        /// </summary>
        /// <param name="spriteNames">要预加载的Sprite名称数组</param>
        public static void PreloadSprites(string[] spriteNames)
        {
            LogHelper.Log("开始预加载Atlas资源...");
            foreach (string spriteName in spriteNames)
            {
                LoadSprite(spriteName);
            }
            LogHelper.Log($"预加载完成，共 {spriteNames.Length} 个资源");
        }

        /// <summary>
        /// 异步预加载资源
        /// </summary>
        /// <param name="spriteNames">要预加载的Sprite名称数组</param>
        /// <returns>异步任务</returns>
        public static async Task PreloadSpritesAsync(string[] spriteNames)
        {
            LogHelper.Log("开始异步预加载资源...");
            var tasks = new List<Task<Sprite>>();
            
            foreach (string spriteName in spriteNames)
            {
                tasks.Add(LoadSpriteAsync(spriteName));
            }
            
            await Task.WhenAll(tasks);
            LogHelper.Log($"异步预加载完成，共 {spriteNames.Length} 个资源");
        }

        /// <summary>
        /// 设置缓存选项
        /// </summary>
        /// <param name="enable">是否启用缓存</param>
        /// <param name="maxSize">最大缓存大小</param>
        public static void SetCacheOptions(bool enable, int maxSize = 100)
        {
            enableCaching = enable;
            maxCacheSize = maxSize;
            LogHelper.Log($"缓存设置已更新: 启用={enable}, 最大大小={maxSize}");
        }

        /// <summary>
        /// 手动添加Atlas
        /// </summary>
        /// <param name="atlasName">Atlas名称</param>
        /// <param name="atlas">Atlas对象</param>
        public static void AddAtlas(string atlasName, SpriteAtlas atlas)
        {
            if (atlas != null)
            {
                atlasCache[atlasName] = atlas;
                LogHelper.Log($"已添加Atlas: {atlasName}");
            }
        }

        /// <summary>
        /// 获取所有Atlas名称
        /// </summary>
        /// <returns>Atlas名称数组</returns>
        public static string[] GetAllAtlasNames()
        {
            string[] names = new string[atlasCache.Count];
            atlasCache.Keys.CopyTo(names, 0);
            return names;
        }

        /// <summary>
        /// 释放资源（线程安全）
        /// </summary>
        /// <param name="spriteName">要释放的Sprite名称</param>
        public static void ReleaseSprite(string spriteName)
        {
            // 从缓存中移除
            lock (cacheLock)
            {
                if (spriteCache.ContainsKey(spriteName))
                {
                    spriteCache.Remove(spriteName);
                }
            }
            
            // 通过ResManager释放
            ResManager.Release(spriteName);
            
            LogHelper.Log($"已释放资源: {spriteName}");
        }
    }
} 