# SynthMind

**Modular Unity client foundation** — production-oriented UI architecture, Addressables-driven content, and service-connected experiences (including AI chat).

Author: [Yuankun Huang](https://github.com/YuankunHuang) · [yuankunhuang.com](https://yuankunhuang.com)

---

## Overview

SynthMind is a **modular Unity client shell**: account, networking, UI, assets, localization, audio, and graphics preferences are composed through `ModuleRegistry`, making it easier to grow hot-update UI and live features. The repo also ships an **editor build pipeline** and a standalone **data/config toolchain** aimed at realistic iteration and release workflows.

## Capabilities

| Area | Description |
|------|-------------|
| **UI & hot-update windows** | Window stack and controller factory via `UIManager` / `WindowLoader`; hot-update screens live under `Assets/Scripts/HotUpdate` (main menu, login, settings, chat, sandbox, etc.). |
| **Addressables** | Content is organized with Addressables; the **SynthMind Build Pipeline** in the Editor can build Addressables together with the player. |
| **Networking & AI** | `RestApiClient` calls the chat backend with `UnityWebRequest`; `NetworkManager` ties message flow to Firebase conversation writes and analytics (with `#if` differences between WebGL and Editor/native). |
| **Data-driven config** | Runtime loads binary config from `StreamingAssets/ConfigData`; sheets are validated and exported from Excel by `GameDataConfigTool`. |
| **Firebase** | Firestore and related pieces back conversations and some online features (bring your own `google-services` / WebGL setup). |
| **Sandbox** | Experiments around natural-language commands and command routing (`SandboxManager`, `CommandManager`, etc.). |

## Requirements

- **Unity**: `2022.3.37f1` (see `ProjectSettings/ProjectVersion.txt`)
- **.NET**: Editor/player as required by Unity; the config tool targets **.NET 6** (`GameDataConfigTool`)

## Getting started

1. Open the repository root in **Unity 2022.3.37f1**.
2. Open your bootstrap scene (whatever is wired as the first scene in your local project).
3. Build **Addressables** according to project settings, or run the SynthMind build pipeline from the Editor menu.
4. **AI chat**: Point the HTTP endpoint in `Assets/Scripts/Core/Network/RestApiClient.cs` at your own proxy or backend. **Do not commit API keys** to the repository.
5. **Firebase**: Follow Firebase’s setup flow if you need full conversation/analytics paths; for UI-only local work, note the conditional compilation behavior per platform.

## GameDataConfigTool (config CLI)

`GameDataConfigTool/` is a **standalone .NET 6 CLI** that validates and exports game configuration from Excel, decoupled from the Unity Editor for CI or batch content workflows. Use `build.bat` / `build.sh` in that folder to build; see `GameDataConfigTool/guide/README.md` for usage.

Exported data is consumed at runtime by `GameDataManager` and related code under `Assets/StreamingAssets/ConfigData`.

## Build & release

- Editor menu: **SynthMind** **Build Pipeline** (`Assets/Scripts/Editor/BuildPipeline`) supports platform builds, optional Addressables steps, cache cleanup, and related helpers.
- **WebGL** helpers such as `Assets/Scripts/Editor/WebGLBuildTest.cs` are available for local debugging as needed.

## Repository layout (summary)

```
Assets/Scripts/Core/          # Core modules: GameManager, UI, network, account, Firebase, assets, etc.
Assets/Scripts/HotUpdate/     # Hot-update windows and controllers
Assets/Scripts/ConfigData/    # Runtime config code and generated types
Assets/StreamingAssets/       # Runtime config bundles and related assets
GameDataConfigTool/           # Excel → config export CLI
```

## Disclaimer

This repository is for **architecture and tooling exploration**, not a shipped commercial product. Third-party SDKs, Firebase projects, and backend endpoints must be provisioned and configured by you.

## License

If no `LICENSE` file is present, treat licensing as unspecified until one is added; check before redistributing or forking.
