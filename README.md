# Core Package

A modular, extensible Unity core library for game development, providing essential systems such as asset management, UI, audio, scene management, object pooling, and local data handling. Designed for scalability and maintainability in Unity projects.

## Features

- **AssetManager**: Async loading, instantiation, and releasing of Addressable assets and resources.
- **UIManager**: Stack-based UI system with support for dialogs, panels, widgets, and transitions.
- **AudioManager**: Music and sound effect management, pooling, and fade in/out.
- **CoreSceneManager**: Additive scene loading/unloading, context switching, and scene lifecycle events.
- **ObjectPooling**: Efficient pooling for GameObjects and components, async instantiation, and return-to-pool logic.
- **GameHub**: Central entry point for initializing and managing core systems and game state.
- **LocalDataManager**: Type-safe local data storage and retrieval.

## Installation

1. Import the package via Unity Package Manager (UPM) using the following Git URL:https://github.com/trungmk/unity_template_singleton.git.
2. Ensure dependencies are installed:
   - [UniTask](https://github.com/Cysharp/UniTask)
   - [Unity Addressables](https://docs.unity3d.com/Packages/com.unity.addressables@latest)
   - Newtonsoft.Json (for data serialization)
3. Reference the `com.maktrung.core.runtime` assembly in your project if using assembly definitions.

## Usage

### Initialization
- Add the `GameHub` prefab or component to your initial scene.
- Configure references to `CoreSceneManager`, `AudioManager`, `UIManager`, etc., in the inspector or via code.

### Asset Loadingvar asset = await AssetManager.Instance.LoadAssetAsync("addressable_key");
### UI ManagementUIManager.Instance.Show<MyDialogView>(args);
UIManager.Instance.Hide<MyDialogView>();
### AudioAudioManager.Instance.PlayMusic(AudioClipType.MusicBackground, myClip);
AudioManager.Instance.PlaySound(mySoundClip);
### Scene ManagementCoreSceneManager.Instance.ChangeScene(contextKey);
### Object Poolingvar pooledObj = await ObjectPooling.Instance.Get<MyComponent>("addressable_key");
ObjectPooling.Instance.ReturnToPool(pooledObj.gameObject);
### Local Datavar settings = GameData.Instance.LoadLocalData<SettingsLocalData>();
GameData.Instance.SaveLocalData<SettingsLocalData>(settings);
## Folder Structure
- `Runtime/Core/AssetManager` - Asset loading and instantiation
- `Runtime/Core/UI` - UI system and base types
- `Runtime/Core/Audio` - Audio management
- `Runtime/Core/Scenes` - Scene/context management
- `Runtime/Core/ObjectPooling` - Object pooling system
- `Runtime/Core/Data` - Local and remote data management
- `Runtime/Core/Common` - Base classes and singletons

## License
This package is provided as-is for use in your Unity projects. Dependencies may have their own licenses.
