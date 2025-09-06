# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

SynthMind is a Unity 2022.3.37f1 project with a modular architecture built around a custom module registry system. The project uses Unity's Localization Package, Addressables, Firebase, and Universal Render Pipeline.

## Architecture

### Module Registry System
The core architecture is built around `ModuleRegistry` located in `Assets/Scripts/Core/ModuleRegistry/Runtime/ModuleRegistry.cs`. All major systems are registered as modules and accessed through this registry:

- `IAssetManager` - Asset loading via Addressables
- `IUIManager` - UI window stack management
- `INetworkManager` - Network communications
- `ICameraManager` - Camera management
- `IAccountManager` - User account handling
- `ICommandManager` - Command pattern implementation
- `ILocalizationManager` - Localization support

### Namespaces
- `YuankunHuang.Unity.Core` - Core systems and managers
- `YuankunHuang.Unity.ModuleCore` - Module registration infrastructure
- `YuankunHuang.Unity.UICore` - UI management systems
- `YuankunHuang.Unity.LocalizationCore` - Localization functionality
- `YuankunHuang.Unity.AssetCore` - Asset management
- `YuankunHuang.Unity.HotUpdate` - Hot-updateable content (controllers, etc.)

### Key Systems

#### UI Management (`UIManager`)
- Stack-based window system with show/hide animations
- Windows loaded via Addressables with pattern: `Windows/Stackable/{WindowName}/{WindowName}.prefab`
- Window controllers dynamically loaded from `YuankunHuang.Unity.HotUpdate.{WindowName}Controller`
- Supports window masking and self-destruction on cover

#### Localization (`SimpleLocalizationManager`)
- Built on Unity's Localization Package
- Synchronous text loading for immediate results
- Default table: "Localization"
- Returns key as fallback for missing translations
- Simple language switching without complex caching

#### Asset Management
- Addressables-based with predefined paths in `AddressablePaths.cs`
- AssetManager handles loading/unloading with reference counting

### Directory Structure
```
Assets/Scripts/
├── Core/                 # Core managers and systems
├── HotUpdate/           # Hot-updateable content (window controllers)
├── ConfigData/          # Configuration data
├── Generated/           # Auto-generated files
├── Util/                # Utility classes
├── Editor/              # Editor-only scripts
└── Test/                # Test scripts
```

## Development Guidelines

**IMPORTANT: All code, comments, and documentation must be written in English only.**

### Adding New Windows
1. Create prefab in Addressables with path `Windows/Stackable/{WindowName}/{WindowName}.prefab`
2. Create `WindowAttributeData` asset for animation settings
3. Implement controller in `YuankunHuang.Unity.HotUpdate` namespace as `{WindowName}Controller`
4. Inherit from `WindowControllerBase`
5. Add window name to `WindowNames.cs`

### Module Registration
All modules must implement `IModule` interface and be registered in `GameManager.Init()`:
```csharp
ModuleRegistry.Register<IYourInterface>(new YourImplementation());
```

### Localization
- Add strings to Unity Localization tables via editor tools
- Use CSV export/import workflow: `Tools/Localization/Localization Tools`
- Use `locManager.GetLocalizedText(key)` for simple strings
- Use `locManager.GetLocalizedTextFormatted(key, args)` for formatted strings
- Strongly typed keys available in `LocalizationKeys.cs` (auto-generated)

### Input Blocking
Use `InputBlocker.StartBlocking()` and `InputBlocker.StopBlocking()` during async operations to prevent user input conflicts.

## Key Dependencies
- Unity Localization Package (1.5.5)
- Unity Addressables (1.21.21)
- Firebase integration
- Universal Render Pipeline (14.0.11)
- TextMeshPro (3.0.6)

## Important Notes
- Project uses hot-update architecture - controllers are dynamically loaded
- All async operations should use proper input blocking
- Module lifecycle is managed through `GameManager`
- Localization system provides immediate synchronous results
- UI system supports complex animation and stacking behaviors

## Localization Workflow
1. **Setup**: Ensure Unity Localization Package is configured with "Localization" table
2. **Export**: Use `Tools/Localization/Export Localization Data` to create CSV files
3. **Translate**: Send CSV to translators for localization
4. **Import**: Use `Tools/Localization/Import Localization Data` to import translated CSV
5. **Code**: Use `SimpleLocalizationManager.GetLocalizedText(key)` in runtime code