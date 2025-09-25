# `ComplexSample.cs` – Multi-namespace Unity game system with managers, UI, audio, networking and animation

**Purpose:** Comprehensive game architecture file containing seven namespaces with core game systems including state management, player control, UI animation, audio processing, and multiplayer networking. Demonstrates complex nested type hierarchies and interface-driven design patterns for Unity game development.

## Metadata
* **File:** `ComplexSample.cs`
* **All namespaces:** ComplexSample.Enums, ComplexSample.Interfaces, ComplexSample.Data, ComplexSample.Animation, ComplexSample.Utilities, ComplexSample.Managers, ComplexSample.Audio, ComplexSample.UI, ComplexSample.Network
* **External dependencies:** UnityEngine, UnityEngine.UI, System, System.Collections, System.Collections.Generic
* **Public root types:** 
  - `ComplexSample.Enums.GameState (enum)`, `ComplexSample.Enums.PlayerAction (enum)`, `ComplexSample.Enums.EaseType (enum)`, `ComplexSample.Enums.NetworkStatus (enum)`
  - `ComplexSample.Interfaces.ISaveable<T> (interface)`, `ComplexSample.Interfaces.IUpdateable (interface)`, `ComplexSample.Interfaces.INetworkSync (interface)`, `ComplexSample.Interfaces.IAnimatable (interface)`
  - `ComplexSample.Data.PlayerSaveData (class)`, `ComplexSample.Data.GameConfig (struct)`, `ComplexSample.Data.NetworkPacket (class)`
  - `ComplexSample.Animation.AnimationUtils (static class)`, `ComplexSample.Animation.AnimationController (MonoBehaviour class)`
  - `ComplexSample.Utilities.GameUtils (static class)`, `ComplexSample.Utilities.StringUtils (static class)`
  - `ComplexSample.Managers.GameManager (MonoBehaviour class)`, `ComplexSample.Managers.IGameEventListener (interface)`, `ComplexSample.Managers.PlayerController (MonoBehaviour class)`
  - `ComplexSample.Audio.AudioManager (MonoBehaviour class)`, `ComplexSample.Audio.AudioUtils (static class)`
  - `ComplexSample.UI.UIController (MonoBehaviour class)`, `ComplexSample.UI.IUIInteractable (interface)`, `ComplexSample.UI.UIButton (MonoBehaviour class)`
  - `ComplexSample.Network.NetworkManager (MonoBehaviour class)`, `ComplexSample.Network.NetworkClient (class)`, `ComplexSample.Network.NetworkUtils (static class)`
* **Unity integration:** 8 MonoBehaviour classes, lifecycle methods (Awake, Start, Update, OnDestroy), SerializeField attributes, coroutines

## Public API Reference
| Type | Member | Signature | Usage Example |
|------|--------|-----------|---------------|
| GameManager | Method | void ChangeGameState(GameState state) | manager.ChangeGameState(GameState.Playing); |
| GameManager | Property | GameState CurrentState {get;} | var state = manager.CurrentState; |
| GameManager | Event | static Action<GameState> OnGameStateChanged | GameManager.OnGameStateChanged += HandleChange; |
| IGameEventListener | Method | void OnGameTick() | listener.OnGameTick(); |
| IGameEventListener | Method | void OnGameStateChanged(GameState newState) | listener.OnGameStateChanged(GameState.Playing); |
| IGameEventListener | Property | bool IsActive {get;} | if (listener.IsActive) process(); |
| ISaveable<T> | Method | T SaveData() | var data = saveable.SaveData(); |
| ISaveable<T> | Method | void LoadData(T data) | saveable.LoadData(playerData); |
| ISaveable<T> | Property | bool HasUnsavedChanges {get;} | if (obj.HasUnsavedChanges) save(); |
| IUpdateable | Method | void GameUpdate(float deltaTime) | updatable.GameUpdate(Time.deltaTime); |
| IUpdateable | Property | bool RequiresUpdate {get;} | if (obj.RequiresUpdate) update(); |
| IAnimatable | Method | void StartAnimation() | animatable.StartAnimation(); |
| IAnimatable | Property | bool IsAnimating {get;} | if (obj.IsAnimating) wait(); |
| INetworkSync | Method | void SendNetworkUpdate(byte[] data) | netObj.SendNetworkUpdate(payload); |
| INetworkSync | Property | bool IsNetworked {get;} | if (obj.IsNetworked) sync(); |
| GameState | Enum | MainMenu, Playing, Paused, GameOver, Victory, Loading | var state = GameState.Playing; |
| PlayerAction | Enum | Move, Jump, Attack, Defend, UseItem, Interact | var action = PlayerAction.Jump; |
| NetworkStatus | Enum | Disconnected, Connecting, Connected, Failed, Timeout | var status = NetworkStatus.Connected; |

## MonoBehaviour Classes
**MonoBehaviour root types:** GameManager, AnimationController, PlayerController, AudioManager, UIController, UIButton, NetworkManager

**Unity Lifecycle:**
* **GameManager:** `Awake()` - system initialization, `Start()` - state setup, `Update()` - debug input, `OnDestroy()` - data saving
* **AnimationController:** Animation sequencing and coroutine management
* **PlayerController:** Movement processing and input handling
* **AudioManager:** Audio state management based on game events
* **UIController:** UI animation and panel transitions
* **NetworkManager:** Network connection simulation and packet processing

**Inspector Dependencies:**
* **GameManager:** `[SerializeField] float gameSpeed`, `[SerializeField] int maxPlayers`, `[SerializeField] bool debugMode`
* **AnimationController:** `[SerializeField] float defaultDuration`, `[SerializeField] EaseType defaultEasing`
* **AudioManager:** `[SerializeField] AudioSource musicSource`, `[SerializeField] AudioClip[] musicClips`

## Important Types

### `GameManager`
* **Kind:** MonoBehaviour class implementing ISaveable<PlayerSaveData>
* **Purpose:** Central game state controller with event system and player management
* **Key Methods:** `ChangeGameState(GameState)` - state transitions, `RegisterEventListener(IGameEventListener)` - event registration
* **Key Properties:** `CurrentState` - GameState access, `IsGameActive` - playing state check
* **Events:** `OnGameStateChanged` - state change notifications, `OnPlayerCountChanged` - player management
* **Nested Types:** `GameManager.GameSettings` - configuration data, `GameManager.GameConstants` - static values

### `ISaveable<T>`
* **Kind:** Generic interface for save/load functionality
* **Interface Contract:** Requires SaveData() return type T, LoadData(T) for loading, GetSaveKey() for identification, HasUnsavedChanges property

### `IUpdateable`
* **Kind:** Interface for custom update cycles
* **Interface Contract:** GameUpdate(float) for frame updates, FixedGameUpdate(float) for physics, RequiresUpdate boolean, UpdatePriority integer

### `AnimationUtils`
* **Kind:** Static utility class for animation coroutines
* **Purpose:** Provides reusable animation sequences for UI and transforms
* **Key Methods:** `FadeCoroutine(CanvasGroup, float, float)` - fade animations, `ScaleCoroutine(Transform, Vector3, float, EaseType)` - scaling with easing

### `NetworkManager`
* **Kind:** MonoBehaviour class implementing INetworkSync
* **Purpose:** Handles multiplayer networking and packet management
* **Key Methods:** `StartServer()` - server initialization, `SendChatMessage(string)` - messaging system
* **Events:** `OnNetworkStatusChanged` - connection updates, `OnPacketReceived` - data handling

## Core Usage Patterns

**Basic game state management:**
```csharp
using ComplexSample.Managers;
using ComplexSample.Enums;

var gameManager = FindObjectOfType<GameManager>();
gameManager.ChangeGameState(GameState.Playing);
GameManager.OnGameStateChanged += (state) => Debug.Log($"State: {state}");
```

**Interface implementations:**
```csharp
public class MyUpdatable : MonoBehaviour, IUpdateable
{
    public bool RequiresUpdate { get; set; } = true;
    public int UpdatePriority { get; set; } = 1;
    
    public void GameUpdate(float deltaTime) { /* custom update */ }
    public void FixedGameUpdate(float fixedDeltaTime) { /* physics */ }
}
```

**Animation system:**
```csharp
using ComplexSample.Animation;

var controller = GetComponent<AnimationController>();
controller.StartAnimation();
StartCoroutine(AnimationUtils.FadeCoroutine(canvasGroup, 0.5f, 1.0f));
```

**Networking:**
```csharp
using ComplexSample.Network;

var networkManager = GetComponent<NetworkManager>();
networkManager.StartServer();
networkManager.SendChatMessage("Hello World");
```

## Architecture Notes
* **Control flow:** Event-driven architecture with GameManager as central coordinator, interface-based updates, coroutine animations
* **Performance:** Static utility classes minimize allocations, interface patterns enable polymorphism, coroutines handle smooth animations
* **Dependencies:** Extensive cross-namespace references, Unity UI integration, generic save system
* **Key features:** Multi-namespace organization, complex nested types, interface contracts, MonoBehaviour lifecycle integration, networking simulation

---
*Generated: 2025-09-20 | Target: 4K-6K chars*

---
`checksum: 2025-sept-20 v0.8.5`