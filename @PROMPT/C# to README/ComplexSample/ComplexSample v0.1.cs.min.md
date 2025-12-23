# `ComplexSample.cs` – Multi-namespace Unity game architecture with managers, UI, audio, and networking

**Purpose:** Comprehensive game architecture file implementing multiple game systems across 8 namespaces including core game management, player controls, audio system, UI framework, animation utilities, and network multiplayer functionality. Demonstrates complex nested type hierarchies with extensive interface implementations.

## Metadata
* **File:** `ComplexSample.cs`
* **All namespaces:** `ComplexSample.Enums, ComplexSample.Interfaces, ComplexSample.Data, ComplexSample.Animation, ComplexSample.Utilities, ComplexSample.Managers, ComplexSample.Audio, ComplexSample.UI, ComplexSample.Network`
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
* **Unity integration:** 8 MonoBehaviour classes with extensive lifecycle methods, SerializeField usage, coroutines

## Public API Reference
| Type | Member | Signature | Usage Example |
|------|--------|-----------|---------------|
| GameState | Enum | MainMenu, Playing, Paused, GameOver, Victory, Loading | var state = GameState.Playing; |
| PlayerAction | Enum | Move, Jump, Attack, Defend, UseItem, Interact | var action = PlayerAction.Jump; |
| EaseType | Enum | Linear, EaseIn, EaseOut, EaseInOut, Bounce, Elastic | var ease = EaseType.EaseInOut; |
| NetworkStatus | Enum | Disconnected, Connecting, Connected, Failed, Timeout | var status = NetworkStatus.Connected; |
| ISaveable<T> | Method | T SaveData() | var data = saveable.SaveData(); |
| ISaveable<T> | Method | void LoadData(T data) | saveable.LoadData(saveData); |
| ISaveable<T> | Method | string GetSaveKey() | var key = saveable.GetSaveKey(); |
| ISaveable<T> | Property | bool HasUnsavedChanges {get;} | if (obj.HasUnsavedChanges) Save(); |
| IUpdateable | Method | void GameUpdate(float deltaTime) | updateable.GameUpdate(Time.deltaTime); |
| IUpdateable | Method | void FixedGameUpdate(float fixedDeltaTime) | updateable.FixedGameUpdate(Time.fixedDeltaTime); |
| IUpdateable | Property | bool RequiresUpdate {get;} | if (obj.RequiresUpdate) obj.GameUpdate(dt); |
| IUpdateable | Property | int UpdatePriority {get; set;} | obj.UpdatePriority = 5; |
| INetworkSync | Method | void SendNetworkUpdate(byte[] data) | networkSync.SendNetworkUpdate(data); |
| INetworkSync | Method | void ReceiveNetworkUpdate(byte[] data) | networkSync.ReceiveNetworkUpdate(data); |
| INetworkSync | Property | bool IsNetworked {get;} | if (obj.IsNetworked) SendUpdate(); |
| INetworkSync | Property | int NetworkId {get;} | var id = obj.NetworkId; |
| IAnimatable | Method | void StartAnimation() | animatable.StartAnimation(); |
| IAnimatable | Method | void StopAnimation() | animatable.StopAnimation(); |
| IAnimatable | Method | void PauseAnimation() | animatable.PauseAnimation(); |
| IAnimatable | Property | bool IsAnimating {get;} | if (obj.IsAnimating) return; |
| IAnimatable | Property | float AnimationProgress {get;} | var progress = obj.AnimationProgress; |
| PlayerSaveData | Constructor | PlayerSaveData(string name, int level, float exp) | var data = new PlayerSaveData("Player", 5, 100f); |
| PlayerSaveData | Property | string PlayerName {get; set;} | data.PlayerName = "Hero"; |
| PlayerSaveData | Property | int Level {get; set;} | data.Level = 10; |
| GameConfig | Constructor | GameConfig(float master, float sfx, float music) | var config = new GameConfig(1f, 0.8f, 0.6f); |
| GameConfig | Property | static GameConfig Default {get;} | var config = GameConfig.Default; |
| NetworkPacket | Constructor | NetworkPacket(PacketType type, int senderId, byte[] data) | var packet = new NetworkPacket(PacketType.PlayerJoin, 1, data); |
| NetworkPacket | Enum | PacketType: PlayerJoin, PlayerLeave, GameState, PlayerAction, ChatMessage | var type = PacketType.PlayerJoin; |
| AnimationUtils | Method | static IEnumerator FadeCoroutine(CanvasGroup group, float targetAlpha, float duration) | StartCoroutine(AnimationUtils.FadeCoroutine(group, 0f, 1f)); |
| AnimationUtils | Method | static IEnumerator MoveCoroutine(Transform transform, Vector3 targetPosition, float duration) | StartCoroutine(AnimationUtils.MoveCoroutine(transform, pos, 2f)); |
| AnimationUtils | Method | static float ApplyEasing(float t, EaseType easeType) | float easedT = AnimationUtils.ApplyEasing(0.5f, EaseType.EaseIn); |
| AnimationController | Method | Coroutine PlayFadeAnimation(CanvasGroup target, float targetAlpha) | controller.PlayFadeAnimation(canvasGroup, 0f); |
| AnimationController | Method | Coroutine PlayMoveAnimation(Transform target, Vector3 targetPosition) | controller.PlayMoveAnimation(transform, newPos); |
| AnimationController | Event | static Action<AnimationController> OnAnimationStarted | AnimationController.OnAnimationStarted += HandleStart; |
| GameUtils | Method | static float CalculateDamage(float baseDamage, float multiplier) | float dmg = GameUtils.CalculateDamage(10f, 1.5f); |
| GameUtils | Method | static Vector3 GetRandomPosition(Bounds bounds) | var pos = GameUtils.GetRandomPosition(bounds); |
| GameUtils | Method | static float CalculateValue(float input, CalculationType calcType, float parameter) | var val = GameUtils.CalculateValue(5f, CalculationType.Linear, 2f); |
| GameUtils | Enum | CalculationType: Linear, Exponential, Logarithmic, Polynomial | var calc = CalculationType.Exponential; |
| StringUtils | Method | static string FormatScore(int score) | string formatted = StringUtils.FormatScore(12345); |
| StringUtils | Method | static bool IsValidPlayerName(string name) | bool valid = StringUtils.IsValidPlayerName("Player1"); |
| GameManager | Method | void ChangeGameState(GameState newState) | manager.ChangeGameState(GameState.Playing); |
| GameManager | Method | bool RegisterEventListener(IGameEventListener listener) | bool success = manager.RegisterEventListener(listener); |
| GameManager | Method | IEnumerator LoadGameAsync(string saveFileName) | StartCoroutine(manager.LoadGameAsync("save1")); |
| GameManager | Method | void AddPlayer(string playerName) | manager.AddPlayer("Player1"); |
| GameManager | Method | void RemovePlayer(int playerId) | manager.RemovePlayer(0); |
| GameManager | Property | GameState CurrentState {get;} | var state = manager.CurrentState; |
| GameManager | Property | bool IsGameActive {get;} | if (manager.IsGameActive) ProcessGame(); |
| GameManager | Event | static Action<GameState> OnGameStateChanged | GameManager.OnGameStateChanged += HandleStateChange; |
| GameManager | Enum | GameMode: SinglePlayer, Multiplayer, Tournament, Practice, Spectator | var mode = GameMode.Multiplayer; |
| GameManager.GameSettings | Constructor | GameSettings() | var settings = new GameSettings(); |
| GameManager.GameSettings | Property | float Volume {get; set;} | settings.Volume = 0.8f; |
| GameManager.GameSettings.AudioSettings | Constructor | AudioSettings(float master, float sfx, float music, bool mute) | var audio = new AudioSettings(1f, 0.8f, 0.6f); |
| GameManager.GameSettings.AudioSettings | Method | void ApplySettings() | audioSettings.ApplySettings(); |
| GameManager.GameSettings.IConfigurable | Method | void LoadConfiguration() | configurable.LoadConfiguration(); |
| GameManager.GameConstants | Method | static string GetFormattedScore(int score) | string score = GameConstants.GetFormattedScore(1000); |
| GameManager.GameConstants | Method | static bool IsValidScore(int score) | bool valid = GameConstants.IsValidScore(500); |
| IGameEventListener | Method | void OnGameTick() | listener.OnGameTick(); |
| IGameEventListener | Method | void OnPlayerJoined(int playerId) | listener.OnPlayerJoined(1); |
| IGameEventListener | Method | void OnGameStateChanged(GameState newState) | listener.OnGameStateChanged(GameState.Playing); |
| IGameEventListener | Property | bool IsActive {get;} | if (listener.IsActive) Process(); |
| PlayerController | Method | void PerformAction(PlayerAction action) | controller.PerformAction(PlayerAction.Jump); |
| PlayerController | Method | void SetPlayerData(PlayerSaveData data) | controller.SetPlayerData(saveData); |
| PlayerController | Event | Action<PlayerAction> OnPlayerAction | controller.OnPlayerAction += HandleAction; |
| AudioManager | Method | void PlaySFX(string clipName) | audioManager.PlaySFX("jump_sound"); |
| AudioManager | Method | void PlayMusic(string musicName, bool loop) | audioManager.PlayMusic("theme", true); |
| AudioManager | Method | void SetMute(bool mute) | audioManager.SetMute(true); |
| AudioManager | Method | void SetVolume(AudioType audioType, float volume) | audioManager.SetVolume(AudioType.Music, 0.5f); |
| AudioManager | Enum | AudioType: Music, SFX, Voice, Ambient, UI | var type = AudioType.Music; |
| AudioManager | Event | static Action<AudioType, string> OnAudioPlayed | AudioManager.OnAudioPlayed += HandleAudioPlay; |
| AudioManager.IAudioProcessor | Method | void ProcessAudio(AudioClip clip) | processor.ProcessAudio(clip); |
| AudioManager.AudioEffects | Method | static void ApplyEffect(AudioSource source, EffectType effectType, float intensity) | AudioEffects.ApplyEffect(source, EffectType.Reverb, 0.5f); |
| AudioManager.AudioEffects | Enum | EffectType: Reverb, Echo, Distortion, LowPass, HighPass | var effect = EffectType.Reverb; |
| AudioUtils | Method | static float ConvertToDecibels(float linearVolume) | float db = AudioUtils.ConvertToDecibels(0.8f); |
| AudioUtils | Method | static bool IsAudible(float volume) | bool audible = AudioUtils.IsAudible(0.1f); |
| UIController | Method | void ShowPanel(string panelName) | uiController.ShowPanel("MainMenu"); |
| UIController | Method | void HidePanel(string panelName) | uiController.HidePanel("Settings"); |
| UIController | Method | UIPanel CreatePanel(string name) | var panel = uiController.CreatePanel("NewPanel"); |
| UIController | Event | static Action<string> OnPanelOpened | UIController.OnPanelOpened += HandlePanelOpen; |
| UIController.UIPanel | Constructor | UIPanel(string name, bool visible, int priority) | var panel = new UIPanel("Menu", true, 1); |
| UIController.UIPanel | Method | void Show() | panel.Show(); |
| UIController.UIPanel | Method | void Hide() | panel.Hide(); |
| UIController.UIPanel.PanelAnimation | Constructor | PanelAnimation(AnimationType type, float duration) | var anim = new PanelAnimation(AnimationType.Fade, 1f); |
| UIController.UIPanel.PanelAnimation | Enum | AnimationType: Fade, Slide, Scale, Rotate, Custom | var type = AnimationType.Fade; |
| UIController.UIPanel.PanelFactory | Method | static UIPanel CreateMenuPanel() | var panel = PanelFactory.CreateMenuPanel(); |
| IUIInteractable | Method | void OnHover() | interactable.OnHover(); |
| IUIInteractable | Method | void OnClick() | interactable.OnClick(); |
| IUIInteractable | Property | bool IsInteractable {get; set;} | obj.IsInteractable = false; |
| UIButton | Method | void SetButtonState(ButtonState newState) | button.SetButtonState(ButtonState.Disabled); |
| UIButton | Enum | ButtonState: Normal, Highlighted, Pressed, Disabled | var state = ButtonState.Normal; |
| UIButton | Event | Action<UIButton> OnButtonClicked | button.OnButtonClicked += HandleClick; |
| NetworkManager | Method | void StartServer() | networkManager.StartServer(); |
| NetworkManager | Method | void StartClient(string serverAddress) | networkManager.StartClient("192.168.1.1"); |
| NetworkManager | Method | void SendChatMessage(string message) | networkManager.SendChatMessage("Hello"); |
| NetworkManager | Property | NetworkStatus Status {get;} | var status = networkManager.Status; |
| NetworkManager | Event | static Action<NetworkStatus> OnNetworkStatusChanged | NetworkManager.OnNetworkStatusChanged += HandleStatus; |
| NetworkClient | Constructor | NetworkClient(int id) | var client = new NetworkClient(1); |
| NetworkClient | Method | void Connect(string serverAddress) | client.Connect("192.168.1.1"); |
| NetworkUtils | Method | static bool IsValidIPAddress(string ipAddress) | bool valid = NetworkUtils.IsValidIPAddress("192.168.1.1"); |
| NetworkUtils | Method | static byte[] SerializeString(string data) | byte[] bytes = NetworkUtils.SerializeString("data"); |

## MonoBehaviour Classes
**MonoBehaviour root types:** AnimationController, GameManager, PlayerController, AudioManager, UIController, UIButton, NetworkManager
**Regular root types:** PlayerSaveData, NetworkPacket, NetworkClient

**Unity Lifecycle:**
* **AnimationController:**
  - No explicit lifecycle methods (uses coroutines and events)
* **GameManager:**
  - `Awake()` - DontDestroyOnLoad setup, system initialization
  - `Start()` - Initial state setup, coroutine startup
  - `Update()` - Debug input handling
  - `OnDestroy()` - Save data cleanup
* **PlayerController:**
  - No explicit lifecycle methods (interface-driven updates)
* **AudioManager:**
  - `Start()` - GameManager registration, listener setup
* **UIController:**
  - No explicit lifecycle methods (coroutine-based animations)
* **UIButton:**
  - `Start()` - Button click listener setup
* **NetworkManager:**
  - `Start()` - Network ID generation

**Inspector Dependencies:**
* **AnimationController:** `[SerializeField] float defaultDuration, EaseType defaultEasing`
* **GameManager:** `[SerializeField] float gameSpeed, int maxPlayers, bool debugMode`
* **PlayerController:** `[SerializeField] float moveSpeed, float jumpForce`
* **AudioManager:** `[SerializeField] AudioSource musicSource, AudioSource sfxSource, AudioClip[] musicClips, AudioClip[] sfxClips`
* **UIController:** `[SerializeField] Canvas mainCanvas, Button[] menuButtons, CanvasGroup[] panelGroups`
* **UIButton:** `[SerializeField] Button button, Image buttonImage`
* **NetworkManager:** `[SerializeField] int maxConnections, float timeoutDuration`

## Important Types

### `GameState` (Reference: `ComplexSample.Enums.GameState`)
* **Kind:** enum - game flow states
* **Enum Values:** `MainMenu, Playing, Paused, GameOver, Victory, Loading` - complete game state machine

### `GameManager` (Reference: `ComplexSample.Managers.GameManager`)
* **Kind:** MonoBehaviour class + ISaveable<PlayerSaveData>
* **Key Methods:** `ChangeGameState(GameState)` - state transitions, `AddPlayer(string)` - player management, `LoadGameAsync(string)` - async save loading
* **Key Properties:** `CurrentState` - GameState access, `IsGameActive` - playing state check
* **Events:** `OnGameStateChanged` - state change notifications, `OnPlayerCountChanged` - player count updates
* **Nested Types:** `GameMode` enum, `GameSettings` class with `AudioSettings` struct and `IConfigurable` interface, `GameConstants` static class

### `ComplexSample.Managers.GameManager.GameSettings` (Nested)
* **Kind:** nested class in `GameManager`
* **Methods:** Constructor for initialization
* **Properties:** `Volume, FullScreen, TargetFrameRate` - configuration settings
* **Nested Types:** `AudioSettings` struct, `IConfigurable` interface

### `IUpdateable` (Reference: `ComplexSample.Interfaces.IUpdateable`)
* **Kind:** interface - game object update contract
* **Interface Contract:** `GameUpdate(float)` and `FixedGameUpdate(float)` for frame updates, `RequiresUpdate` bool property, `UpdatePriority` int property

### `AnimationController` (Reference: `ComplexSample.Animation.AnimationController`)
* **Kind:** MonoBehaviour class + IAnimatable
* **Key Methods:** `StartAnimation()` - sequence start, `PlayFadeAnimation(CanvasGroup, float)` - UI fading, `PlayMoveAnimation(Transform, Vector3)` - transform movement
* **Events:** `OnAnimationStarted, OnAnimationCompleted` - lifecycle events
* **Properties:** `IsAnimating, AnimationProgress` - animation state tracking

### `ComplexSample.UI.UIController.UIPanel` (Nested)
* **Kind:** nested class in `UIController`
* **Methods:** `Show()`, `Hide()` - visibility control
* **Properties:** `PanelName, IsVisible, DisplayPriority` - panel configuration
* **Nested Types:** `PanelAnimation` class with `IAnimatable` interface, `PanelFactory` static class

### `NetworkManager` (Reference: `ComplexSample.Network.NetworkManager`)
* **Kind:** MonoBehaviour class + INetworkSync
* **Methods:** `StartServer()`, `StartClient(string)` - connection management, `SendChatMessage(string)` - messaging
* **Properties:** `Status` - NetworkStatus access, `ConnectedClients` - connection count
* **Events:** `OnNetworkStatusChanged, OnPacketReceived` - network events

### `AudioManager` (Reference: `ComplexSample.Audio.AudioManager`)
* **Kind:** MonoBehaviour class + IGameEventListener
* **Methods:** `PlaySFX(string)`, `PlayMusic(string, bool)` - audio playback, `SetVolume(AudioType, float)` - volume control
* **Properties:** `MasterVolume, IsMuted` - audio state
* **Nested Types:** `AudioType` enum, `IAudioProcessor` interface, `AudioEffects` class

## Core Usage Patterns

**Basic workflow:**
```csharp
using ComplexSample.Managers;
using ComplexSample.Enums;
using ComplexSample.Interfaces;

// Game management
var manager = GetComponent<GameManager>();
manager.ChangeGameState(GameState.Playing);
manager.AddPlayer("Player1");
GameManager.OnGameStateChanged += HandleStateChange;

// Animation system
var controller = GetComponent<AnimationController>();
controller.StartAnimation();
controller.PlayFadeAnimation(canvasGroup, 0f);

// Audio system
var audioManager = GetComponent<AudioManager>();
audioManager.PlayMusic("theme", true);
audioManager.SetVolume(AudioType.Music, 0.8f);
```

**Interface implementation:**
```csharp
public class MyUpdater : IUpdateable
{
    public bool RequiresUpdate { get; set; } = true;
    public int UpdatePriority { get; set; } = 1;
    
    public void GameUpdate(float deltaTime) { /* implementation */ }
    public void FixedGameUpdate(float fixedDeltaTime) { /* implementation */ }
}

public class MySaveSystem : ISaveable<PlayerSaveData>
{
    public PlayerSaveData SaveData() { return new PlayerSaveData(); }
    public void LoadData(PlayerSaveData data) { /* load logic */ }
    public string GetSaveKey() { return "MySaveSystem"; }
    public bool HasUnsavedChanges { get; private set; }
}
```

**Complex nested types:**
```csharp
// Using nested classes and enums
var settings = new GameManager.GameSettings();
var audioSettings = new GameManager.GameSettings.AudioSettings(1f, 0.8f, 0.6f);
audioSettings.ApplySettings();

var panel = UIController.UIPanel.PanelFactory.CreateMenuPanel();
var animation = new UIController.UIPanel.PanelAnimation(AnimationType.Fade, 1f);

// Network communication
var packet = new NetworkPacket(NetworkPacket.PacketType.PlayerJoin, 1, data);
networkManager.SendNetworkUpdate(packetData);
```

## Architecture Notes
* **Control flow:** Event-driven architecture with manager hierarchy, interface-based update systems, and coroutine-managed async operations
* **Performance:** Extensive coroutine usage for animations and async operations, list-based event listener management, static utility classes for performance-critical calculations
* **Dependencies:** Complex cross-namespace dependencies with interfaces providing decoupling, Unity component-based architecture with SerializeField configurations
* **Key features:** Complete game state management, multi-layered UI system with animations, network multiplayer support, comprehensive audio management, save/load system with generic interfaces

---
*Generated: 2025-09-20 | Target: 4K-6K chars*

---
`checksum: 2025-sept-20 v1.0.0`