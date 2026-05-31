# Project Base

Core framework for Unity mobile game development. Provides reusable systems that accelerate game development across projects.

**Unity 6** (6000.3.9f1) | **URP** | **Mobile-first**

## Systems

| System | Description |
|--------|-------------|
| **UI System** | MVP pattern, 3 layers (Screen/Popup/Overlay), animation presets, popup stack, dimmer |
| **Data System** | SO-based persistence, JSON > GZip > AES-256, auto-save, optimistic update, server sync |
| **Time System** | Anti-cheat server time, countdown/cooldown timers, daily/weekly schedules, offline persistence |
| **Audio System** | Pooled AudioSources, Addressables loading, DOTween fade |
| **Asset System** | Addressables wrapper, scene management |
| **Common** | Singleton, EventBus, Object Pool, State Machine, SO Architecture (Variables/Events/References/RuntimeSets) |
| **GameFlow** | Bootstrap pipeline, splash screen, scene loading tasks |
| **Notification** | Local push notification service interface |

## Prerequisites

Install these **before** adding this package:

1. **UniTask** - Add to `Packages/manifest.json`:
   ```json
   "com.cysharp.unitask": "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask"
   ```

2. **DOTween Pro** - Import from Asset Store. Then run `Tools > Demigiant > DOTween Utility Panel > Create ASMDEF`.
   > This package uses `DOTween.To()` core API only (no extension methods), so DOTween DLLs must be present in the project.

## Installation

Add to your `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.gmx.top.core": "https://github.com/HaTrungVI/unity-project-base.git#v1.0.0"
  }
}
```

Or use a specific commit:

```json
"com.gmx.top.core": "https://github.com/HaTrungVI/unity-project-base.git#COMMIT_HASH"
```

## Quick Start

1. Install prerequisites (UniTask, DOTween Pro)
2. Add package via manifest.json
3. Create SO assets via `Create > ProjectBase > ...` menus
4. Use Editor wizards: `Tools > Project Base > UI Creator` / `Data Module Creator`

## Architecture

- **SOLID** principles throughout
- **No DI frameworks** - manual dependency passing via constructor/SerializeField/SO references
- **Async**: UniTask (no Coroutines)
- **Tweening**: DOTween Pro (`DOTween.To()` only, no extension methods in asmdef code)
- **Assets**: Addressables
- **Data**: SO-based modules with `[SerializeField]` references (no Singleton)
- **Time**: SO Service Reference pattern (no Singleton)

## Assembly Definitions

```
ProjectBase.Common          (leaf - no project dependencies)
ProjectBase.UI              (refs: Common)
ProjectBase.Asset           (refs: Common)
ProjectBase.Data            (refs: Common)
ProjectBase.GameFlow        (refs: Common, UI, Asset)
ProjectBase.Time            (refs: Common, Data, GameFlow)
ProjectBase.Audio           (refs: Common, Data)
ProjectBase.Notification    (refs: Common)
ProjectBase.Editor          (Editor only, refs: Common)
ProjectBase.UI.Editor       (Editor only, refs: UI, Common, Editor)
ProjectBase.Asset.Editor    (Editor only, refs: Asset, Common)
ProjectBase.Data.Editor     (Editor only, refs: Data, Common, Editor)
```

## License

MIT
