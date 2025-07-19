# SmartAssetLoader 使用指南

## 🚀 快速开始

### 1. 编辑器设置

#### 必需设置：
1. **配置Addressables**
   ```
   Window → Asset Management → Addressables → Groups
   ```
   - 创建Group（如："UI Sprites"）
   - 将Sprite资源标记为Addressable
   - 设置Addressable Name（如："Frame 2166"）

#### 可选设置：
2. **创建SpriteAtlas**（用于非4倍数尺寸的sprite）
   ```
   Project窗口 → 右键 → Create → 2D → Sprite Atlas
   ```
   - 将需要优化的sprite拖入Atlas
   - 设置压缩格式为DXT/BC

3. **运行设置检查**
   ```
   Tools → SynthMind → 设置检查工具
   ```

### 2. 代码使用

#### 基本使用：
```csharp
// 初始化（通常在GameManager中）
SmartAssetLoader.Initialize();

// 加载Sprite
Sprite sprite = SmartAssetLoader.LoadSprite("Frame 2166");
image.sprite = sprite;
```

#### 异步使用：
```csharp
// 异步加载
Sprite sprite = await SmartAssetLoader.LoadSpriteAsync("Frame 2166");
image.sprite = sprite;
```

#### 从Atlas加载：
```csharp
// 指定Atlas加载
Sprite sprite = SmartAssetLoader.LoadSprite("Button", "UIAtlas");
```

## 📋 完整使用示例

### 1. 在GameManager中初始化
```csharp
public class GameManager : MonoBehaviour
{
    private async void Start()
    {
        // 初始化AssetLoader
        SmartAssetLoader.Initialize();
        
        // 预加载常用资源
        string[] commonSprites = { "Frame 2166", "Frame 2167", "Group 256" };
        await SmartAssetLoader.PreloadSpritesAsync(commonSprites);
    }
}
```

### 2. 在UI组件中使用
```csharp
public class UIButton : MonoBehaviour
{
    [SerializeField] private Image buttonImage;
    
    private async void Start()
    {
        // 加载按钮背景
        Sprite buttonSprite = await SmartAssetLoader.LoadSpriteAsync("Frame 2166");
        if (buttonSprite != null)
        {
            buttonImage.sprite = buttonSprite;
        }
    }
    
    private void OnDestroy()
    {
        // 释放资源
        SmartAssetLoader.ReleaseSprite("Frame 2166");
    }
}
```

### 3. 批量加载
```csharp
public class UIManager : MonoBehaviour
{
    private async void LoadAllUIElements()
    {
        string[] uiSprites = {
            "Frame 2166",
            "Frame 2167", 
            "Group 256",
            "Group 257"
        };
        
        // 批量异步加载
        var tasks = uiSprites.Select(s => SmartAssetLoader.LoadSpriteAsync(s));
        var sprites = await Task.WhenAll(tasks);
        
        // 使用加载的sprites
        for (int i = 0; i < sprites.Length; i++)
        {
            if (sprites[i] != null)
            {
                // 应用到UI元素
                Debug.Log($"成功加载: {uiSprites[i]}");
            }
        }
    }
}
```

## 🛠️ 编辑器工具

### 1. 设置检查工具
```
Tools → SynthMind → 设置检查工具
```
- 检查Addressables配置
- 检查SpriteAtlas设置
- 检查资源文件
- 提供修复建议

### 2. 资源优化工具
```
Tools → SynthMind → 资源优化工具
```
- 分析所有Sprite资源
- 检查尺寸优化
- 提供Atlas建议
- 导出优化报告

## 📊 性能优化建议

### 1. 资源分类
- **4倍数尺寸sprite** → 直接使用ResManager加载
- **非4倍数尺寸sprite** → 放入SpriteAtlas

### 2. 加载策略
- **常用资源** → 预加载
- **动态资源** → 按需加载
- **临时资源** → 及时释放

### 3. 缓存管理
```csharp
// 设置缓存选项
SmartAssetLoader.SetCacheOptions(true, 50);

// 清理缓存
SmartAssetLoader.ClearCache();

// 获取缓存信息
string info = SmartAssetLoader.GetCacheInfo();
```

## 🔧 故障排除

### 常见问题：

1. **加载失败**
   - 检查Addressable Name是否正确
   - 确认资源已标记为Addressable
   - 检查构建设置

2. **性能问题**
   - 使用异步加载避免阻塞
   - 合理设置缓存大小
   - 及时释放不需要的资源

3. **内存问题**
   - 定期清理缓存
   - 释放不再使用的资源
   - 监控缓存统计信息

## 📝 API参考

### 主要方法：
- `SmartAssetLoader.Initialize()` - 初始化
- `SmartAssetLoader.LoadSprite(name)` - 同步加载
- `SmartAssetLoader.LoadSpriteAsync(name)` - 异步加载
- `SmartAssetLoader.ReleaseSprite(name)` - 释放资源
- `SmartAssetLoader.PreloadSpritesAsync(names)` - 预加载
- `SmartAssetLoader.ClearCache()` - 清理缓存

### 配置方法：
- `SmartAssetLoader.SetCacheOptions(enable, maxSize)` - 设置缓存
- `SmartAssetLoader.AddAtlas(name, atlas)` - 添加Atlas
- `SmartAssetLoader.GetAllAtlasNames()` - 获取Atlas列表

### 工具方法：
- `SmartAssetLoader.IsOptimalSize(sprite)` - 检查优化尺寸
- `SmartAssetLoader.GetOptimizationSuggestion(name)` - 获取优化建议
- `SmartAssetLoader.GetCacheInfo()` - 获取缓存信息 