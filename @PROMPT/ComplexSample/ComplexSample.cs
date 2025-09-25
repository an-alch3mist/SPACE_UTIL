/*
 * Complex Sample File for Documentation Testing
 * Tests multiple namespaces, nested classes, interfaces, events, enums
 * Unity 2020.3 LTS Compatible (.NET Standard 2.0)
 * Strict SYNTAX COMPLIANCE: .NET Standard 2.0 / C# 7.3 Maximum
 * - NO switch expressions (use traditional switch statements)
 * - NO target-typed new expressions (explicit type declarations)
 * - NO C# 8.0+ features
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Global external namespace enum
namespace ComplexSample.Enums
{
	/// <summary>
	/// Global game state enumeration
	/// </summary>
	public enum GameState
	{
		MainMenu,
		Playing,
		Paused,
		GameOver,
		Victory,
		Loading
	}

	/// <summary>
	/// Player action types for input system
	/// </summary>
	public enum PlayerAction
	{
		Move,
		Jump,
		Attack,
		Defend,
		UseItem,
		Interact
	}

	/// <summary>
	/// Animation easing types
	/// </summary>
	public enum EaseType
	{
		Linear,
		EaseIn,
		EaseOut,
		EaseInOut,
		Bounce,
		Elastic
	}

	/// <summary>
	/// Network connection status
	/// </summary>
	public enum NetworkStatus
	{
		Disconnected,
		Connecting,
		Connected,
		Failed,
		Timeout
	}
}

// Interfaces namespace
namespace ComplexSample.Interfaces
{
	/// <summary>
	/// Generic saveable interface
	/// </summary>
	public interface ISaveable<T>
	{
		T SaveData();
		void LoadData(T data);
		string GetSaveKey();
		bool HasUnsavedChanges { get; }
	}

	/// <summary>
	/// Update interface for game objects
	/// </summary>
	public interface IUpdateable
	{
		void GameUpdate(float deltaTime);
		void FixedGameUpdate(float fixedDeltaTime);
		bool RequiresUpdate { get; }
		int UpdatePriority { get; set; }
	}

	/// <summary>
	/// Network synchronization interface
	/// </summary>
	public interface INetworkSync
	{
		void SendNetworkUpdate(byte[] data);
		void ReceiveNetworkUpdate(byte[] data);
		bool IsNetworked { get; }
		int NetworkId { get; }
	}

	/// <summary>
	/// Animation contract interface
	/// </summary>
	public interface IAnimatable
	{
		void StartAnimation();
		void StopAnimation();
		void PauseAnimation();
		bool IsAnimating { get; }
		float AnimationProgress { get; }
	}
}

// Data structures namespace
namespace ComplexSample.Data
{
	/// <summary>
	/// Player save data structure
	/// </summary>
	public class PlayerSaveData
	{
		public string PlayerName { get; set; }
		public int Level { get; set; }
		public float Experience { get; set; }
		public int[] ItemCounts { get; set; }

		public PlayerSaveData()
		{
			PlayerName = "Unknown";
			Level = 1;
			Experience = 0.0f;
			ItemCounts = new int[10];
		}

		public PlayerSaveData(string name, int level, float exp)
		{
			PlayerName = name;
			Level = level;
			Experience = exp;
			ItemCounts = new int[10];
		}
	}

	/// <summary>
	/// Game configuration data
	/// </summary>
	public struct GameConfig
	{
		public float MasterVolume;
		public float SFXVolume;
		public float MusicVolume;
		public bool FullScreen;
		public int TargetFrameRate;
		public bool VSync;

		public GameConfig(float master, float sfx, float music)
		{
			MasterVolume = master;
			SFXVolume = sfx;
			MusicVolume = music;
			FullScreen = true;
			TargetFrameRate = 60;
			VSync = false;
		}

		public static GameConfig Default
		{
			get
			{
				return new GameConfig(1.0f, 0.8f, 0.6f);
			}
		}
	}

	/// <summary>
	/// Network packet data
	/// </summary>
	public class NetworkPacket
	{
		public enum PacketType
		{
			PlayerJoin,
			PlayerLeave,
			GameState,
			PlayerAction,
			ChatMessage
		}

		public PacketType Type { get; set; }
		public int SenderId { get; set; }
		public byte[] Data { get; set; }
		public DateTime Timestamp { get; set; }

		public NetworkPacket(PacketType type, int senderId, byte[] data)
		{
			Type = type;
			SenderId = senderId;
			Data = data;
			Timestamp = DateTime.Now;
		}
	}
}

// Animation utilities namespace
namespace ComplexSample.Animation
{
	/// <summary>
	/// Static animation utilities and coroutines
	/// </summary>
	public static class AnimationUtils
	{
		public const float DEFAULT_DURATION = 1.0f;
		public const float MIN_DURATION = 0.01f;

		/// <summary>
		/// Fade canvas group coroutine
		/// </summary>
		public static IEnumerator FadeCoroutine(CanvasGroup group, float targetAlpha, float duration)
		{
			if (group == null) yield break;

			float startAlpha = group.alpha;
			float elapsed = 0.0f;

			while (elapsed < duration)
			{
				elapsed += Time.deltaTime;
				float t = elapsed / duration;
				group.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
				yield return null;
			}

			group.alpha = targetAlpha;
		}

		/// <summary>
		/// Move transform coroutine
		/// </summary>
		public static IEnumerator MoveCoroutine(Transform transform, Vector3 targetPosition, float duration)
		{
			if (transform == null) yield break;

			Vector3 startPosition = transform.position;
			float elapsed = 0.0f;

			while (elapsed < duration)
			{
				elapsed += Time.deltaTime;
				float t = elapsed / duration;
				transform.position = Vector3.Lerp(startPosition, targetPosition, t);
				yield return null;
			}

			transform.position = targetPosition;
		}

		/// <summary>
		/// Scale transform coroutine with easing
		/// </summary>
		public static IEnumerator ScaleCoroutine(Transform transform, Vector3 targetScale, float duration, ComplexSample.Enums.EaseType easeType)
		{
			if (transform == null) yield break;

			Vector3 startScale = transform.localScale;
			float elapsed = 0.0f;

			while (elapsed < duration)
			{
				elapsed += Time.deltaTime;
				float t = elapsed / duration;
				float easedT = ApplyEasing(t, easeType);
				transform.localScale = Vector3.Lerp(startScale, targetScale, easedT);
				yield return null;
			}

			transform.localScale = targetScale;
		}

		/// <summary>
		/// Apply easing function to time value
		/// </summary>
		public static float ApplyEasing(float t, ComplexSample.Enums.EaseType easeType)
		{
			switch (easeType)
			{
				case ComplexSample.Enums.EaseType.Linear:
					return t;
				case ComplexSample.Enums.EaseType.EaseIn:
					return t * t;
				case ComplexSample.Enums.EaseType.EaseOut:
					return 1.0f - (1.0f - t) * (1.0f - t);
				case ComplexSample.Enums.EaseType.EaseInOut:
					return t < 0.5f ? 2.0f * t * t : 1.0f - 2.0f * (1.0f - t) * (1.0f - t);
				case ComplexSample.Enums.EaseType.Bounce:
					return BounceEase(t);
				case ComplexSample.Enums.EaseType.Elastic:
					return ElasticEase(t);
				default:
					return t;
			}
		}

		private static float BounceEase(float t)
		{
			if (t < 1.0f / 2.75f)
				return 7.5625f * t * t;
			else if (t < 2.0f / 2.75f)
				return 7.5625f * (t -= 1.5f / 2.75f) * t + 0.75f;
			else if (t < 2.5f / 2.75f)
				return 7.5625f * (t -= 2.25f / 2.75f) * t + 0.9375f;
			else
				return 7.5625f * (t -= 2.625f / 2.75f) * t + 0.984375f;
		}

		private static float ElasticEase(float t)
		{
			return Mathf.Sin(13.0f * Mathf.PI * 0.5f * t) * Mathf.Pow(2.0f, 10.0f * (t - 1.0f));
		}
	}

	/// <summary>
	/// Animation controller for complex sequences
	/// </summary>
	public class AnimationController : MonoBehaviour, ComplexSample.Interfaces.IAnimatable
	{
		[SerializeField] private float defaultDuration = 1.0f;
		[SerializeField] private ComplexSample.Enums.EaseType defaultEasing = ComplexSample.Enums.EaseType.EaseInOut;

		private Coroutine currentAnimation;
		private bool isAnimating = false;
		private float animationProgress = 0.0f;

		// Properties
		public bool IsAnimating { get { return isAnimating; } }
		public float AnimationProgress { get { return animationProgress; } }

		// Events
		public static event Action<AnimationController> OnAnimationStarted;
		public static event Action<AnimationController> OnAnimationCompleted;
		public event Action OnAnimationStep;

		// Interface implementation
		public void StartAnimation()
		{
			StartCoroutine(PlaySequenceAnimation());
		}

		public void StopAnimation()
		{
			if (currentAnimation != null)
			{
				StopCoroutine(currentAnimation);
				currentAnimation = null;
			}
			isAnimating = false;
			animationProgress = 0.0f;
		}

		public void PauseAnimation()
		{
			// Implementation would pause current animation
			Time.timeScale = 0.0f;
		}

		// Public methods
		public Coroutine PlayFadeAnimation(CanvasGroup target, float targetAlpha)
		{
			return StartCoroutine(AnimationUtils.FadeCoroutine(target, targetAlpha, defaultDuration));
		}

		public Coroutine PlayMoveAnimation(Transform target, Vector3 targetPosition)
		{
			return StartCoroutine(AnimationUtils.MoveCoroutine(target, targetPosition, defaultDuration));
		}

		// Private implementation
		private IEnumerator PlaySequenceAnimation()
		{
			isAnimating = true;
			animationProgress = 0.0f;
			OnAnimationStarted?.Invoke(this);

			float elapsed = 0.0f;
			while (elapsed < defaultDuration)
			{
				elapsed += Time.deltaTime;
				animationProgress = elapsed / defaultDuration;
				OnAnimationStep?.Invoke();
				yield return null;
			}

			animationProgress = 1.0f;
			isAnimating = false;
			OnAnimationCompleted?.Invoke(this);
		}
	}
}

// Utilities namespace
namespace ComplexSample.Utilities
{
	/// <summary>
	/// Static utility class for game calculations
	/// </summary>
	public static class GameUtils
	{
		public const float GRAVITY_SCALE = 9.81f;
		public const int MAX_PLAYERS = 8;
		public const string DEFAULT_SAVE_PATH = "saves/";

		// Nested enum in static class
		public enum CalculationType
		{
			Linear,
			Exponential,
			Logarithmic,
			Polynomial
		}

		/// <summary>
		/// Calculate damage with multiplier
		/// </summary>
		public static float CalculateDamage(float baseDamage, float multiplier)
		{
			return baseDamage * multiplier;
		}

		/// <summary>
		/// Get random position within bounds
		/// </summary>
		public static Vector3 GetRandomPosition(Bounds bounds)
		{
			return new Vector3(
				UnityEngine.Random.Range(bounds.min.x, bounds.max.x),
				UnityEngine.Random.Range(bounds.min.y, bounds.max.y),
				UnityEngine.Random.Range(bounds.min.z, bounds.max.z)
			);
		}

		/// <summary>
		/// Calculate value using specified calculation type
		/// </summary>
		public static float CalculateValue(float input, CalculationType calcType, float parameter = 1.0f)
		{
			switch (calcType)
			{
				case CalculationType.Linear:
					return input * parameter;
				case CalculationType.Exponential:
					return Mathf.Pow(input, parameter);
				case CalculationType.Logarithmic:
					return Mathf.Log(input) * parameter;
				case CalculationType.Polynomial:
					return input * input * parameter;
				default:
					return input;
			}
		}
	}

	/// <summary>
	/// Static string utilities
	/// </summary>
	public static class StringUtils
	{
		public static string FormatScore(int score)
		{
			return score.ToString("D6");
		}

		public static string FormatTime(float timeInSeconds)
		{
			int minutes = Mathf.FloorToInt(timeInSeconds / 60.0f);
			int seconds = Mathf.FloorToInt(timeInSeconds % 60.0f);
			return string.Format("{0:00}:{1:00}", minutes, seconds);
		}

		public static bool IsValidPlayerName(string name)
		{
			return !string.IsNullOrEmpty(name) && name.Length >= 3 && name.Length <= 20;
		}
	}
}

// Primary namespace with multiple root classes
namespace ComplexSample.Managers
{
	/// <summary>
	/// Main game manager MonoBehaviour with nested types
	/// </summary>
	public class GameManager : MonoBehaviour, ComplexSample.Interfaces.ISaveable<ComplexSample.Data.PlayerSaveData>
	{
		[Header("Game Configuration")]
		[SerializeField] private float gameSpeed = 1.0f;
		[SerializeField] private int maxPlayers = 4;
		[SerializeField] private bool debugMode = false;

		// Public enum inside class
		public enum GameMode
		{
			SinglePlayer,
			Multiplayer,
			Tournament,
			Practice,
			Spectator
		}

		// Public nested class
		public class GameSettings
		{
			public float Volume { get; set; } = 0.8f;
			public bool FullScreen { get; set; } = true;
			public int TargetFrameRate { get; set; } = 60;

			// Nested struct inside nested class
			public struct AudioSettings
			{
				public float MasterVolume;
				public float SFXVolume;
				public float MusicVolume;
				public bool MuteAll;

				public AudioSettings(float master, float sfx, float music, bool mute = false)
				{
					MasterVolume = master;
					SFXVolume = sfx;
					MusicVolume = music;
					MuteAll = mute;
				}

				public void ApplySettings()
				{
					AudioListener.volume = MuteAll ? 0.0f : MasterVolume;
				}
			}

			// Nested interface inside nested class
			public interface IConfigurable
			{
				void LoadConfiguration();
				void SaveConfiguration();
				bool ValidateConfiguration();
				string ConfigurationPath { get; }
			}

			public GameSettings()
			{
				Volume = 0.8f;
				FullScreen = true;
				TargetFrameRate = 60;
			}
		}

		// Static nested class
		public static class GameConstants
		{
			public const int MAX_SCORE = 999999;
			public const float RESPAWN_TIME = 3.0f;
			public const string SAVE_FILE_NAME = "gamesave.dat";
			public const string VERSION = "1.0.0";

			// Static method in static nested class
			public static string GetFormattedScore(int score)
			{
				return score.ToString("D6");
			}

			public static bool IsValidScore(int score)
			{
				return score >= 0 && score <= MAX_SCORE;
			}
		}

		// Private fields
		private ComplexSample.Enums.GameState currentState = ComplexSample.Enums.GameState.MainMenu;
		private GameMode currentMode = GameMode.SinglePlayer;
		private List<IGameEventListener> eventListeners = new List<IGameEventListener>();
		private ComplexSample.Data.PlayerSaveData playerData;

		// Public properties
		public ComplexSample.Enums.GameState CurrentState { get; private set; }
		public GameMode CurrentMode { get; set; }
		public bool IsGameActive { get { return CurrentState == ComplexSample.Enums.GameState.Playing; } }
		public int PlayerCount { get; private set; }

		// Events
		public static event Action<ComplexSample.Enums.GameState> OnGameStateChanged;
		public static event Action<int> OnScoreChanged;
		public static event Action<int> OnPlayerCountChanged;
		public event Action<string> OnPlayerMessage;

		// Unity lifecycle
		void Awake()
		{
			DontDestroyOnLoad(gameObject);
			InitializeGameSystems();
		}

		void Start()
		{
			ChangeGameState(ComplexSample.Enums.GameState.MainMenu);
			StartCoroutine(GameUpdateLoop());
		}

		void Update()
		{
			if (debugMode)
			{
				HandleDebugInput();
			}
		}

		void OnDestroy()
		{
			SaveData();
		}

		// Interface implementation
		public ComplexSample.Data.PlayerSaveData SaveData()
		{
			return new ComplexSample.Data.PlayerSaveData("Player", 1, 0.0f);
		}

		public void LoadData(ComplexSample.Data.PlayerSaveData data)
		{
			playerData = data;
		}

		public string GetSaveKey()
		{
			return "GameManager_PlayerData";
		}

		public bool HasUnsavedChanges { get; private set; }

		// Public methods
		public void ChangeGameState(ComplexSample.Enums.GameState newState)
		{
			CurrentState = newState;
			currentState = newState;
			OnGameStateChanged?.Invoke(newState);
			HasUnsavedChanges = true;
		}

		public bool RegisterEventListener(IGameEventListener listener)
		{
			if (listener != null && !eventListeners.Contains(listener))
			{
				eventListeners.Add(listener);
				return true;
			}
			return false;
		}

		public void UnregisterEventListener(IGameEventListener listener)
		{
			eventListeners.Remove(listener);
		}

		public IEnumerator LoadGameAsync(string saveFileName)
		{
			ChangeGameState(ComplexSample.Enums.GameState.Loading);
			yield return new WaitForSeconds(1.0f);
			// Simulated async loading
			ChangeGameState(ComplexSample.Enums.GameState.Playing);
		}

		public void AddPlayer(string playerName)
		{
			PlayerCount++;
			OnPlayerCountChanged?.Invoke(PlayerCount);
		}

		public void RemovePlayer(int playerId)
		{
			if (PlayerCount > 0)
			{
				PlayerCount--;
				OnPlayerCountChanged?.Invoke(PlayerCount);
			}
		}

		// Private implementation
		private void InitializeGameSystems()
		{
			Application.targetFrameRate = 60;
			Time.timeScale = gameSpeed;
			playerData = new ComplexSample.Data.PlayerSaveData();
		}

		private void HandleDebugInput()
		{
			if (Input.GetKeyDown(KeyCode.F1))
			{
				OnPlayerMessage?.Invoke("Debug mode active");
			}
			if (Input.GetKeyDown(KeyCode.F2))
			{
				ChangeGameState(ComplexSample.Enums.GameState.Playing);
			}
		}

		private IEnumerator GameUpdateLoop()
		{
			while (true)
			{
				yield return new WaitForSeconds(0.1f);

				for (int i = 0; i < eventListeners.Count; i++)
				{
					eventListeners[i].OnGameTick();
				}
			}
		}
	}

	/// <summary>
	/// Interface for game event handling
	/// </summary>
	public interface IGameEventListener
	{
		void OnGameTick();
		void OnPlayerJoined(int playerId);
		void OnPlayerLeft(int playerId);
		void OnGameStateChanged(ComplexSample.Enums.GameState newState);
		bool IsActive { get; }
		int Priority { get; set; }
	}

	/// <summary>
	/// Player controller managing individual player logic
	/// </summary>
	public class PlayerController : MonoBehaviour, ComplexSample.Interfaces.IUpdateable, IGameEventListener
	{
		[SerializeField] private float moveSpeed = 5.0f;
		[SerializeField] private float jumpForce = 10.0f;

		private ComplexSample.Data.PlayerSaveData playerData;
		private ComplexSample.Enums.PlayerAction lastAction;

		// Properties
		public bool RequiresUpdate { get; set; } = true;
		public int UpdatePriority { get; set; } = 1;
		public bool IsActive { get; private set; } = true;
		public int Priority { get; set; } = 0;

		// Events
		public event Action<ComplexSample.Enums.PlayerAction> OnPlayerAction;

		// Interface implementations
		public void GameUpdate(float deltaTime)
		{
			HandleMovement(deltaTime);
		}

		public void FixedGameUpdate(float fixedDeltaTime)
		{
			// Physics updates
		}

		public void OnGameTick()
		{
			// Process per-tick logic
		}

		public void OnPlayerJoined(int playerId)
		{
			Debug.Log($"Player {playerId} joined");
		}

		public void OnPlayerLeft(int playerId)
		{
			Debug.Log($"Player {playerId} left");
		}

		public void OnGameStateChanged(ComplexSample.Enums.GameState newState)
		{
			IsActive = newState == ComplexSample.Enums.GameState.Playing;
		}

		// Public methods
		public void PerformAction(ComplexSample.Enums.PlayerAction action)
		{
			lastAction = action;
			OnPlayerAction?.Invoke(action);
		}

		public void SetPlayerData(ComplexSample.Data.PlayerSaveData data)
		{
			playerData = data;
		}

		// Private implementation
		private void HandleMovement(float deltaTime)
		{
			if (Input.GetKey(KeyCode.W))
			{
				transform.Translate(Vector3.forward * moveSpeed * deltaTime);
				PerformAction(ComplexSample.Enums.PlayerAction.Move);
			}
		}
	}
}

// Audio system namespace
namespace ComplexSample.Audio
{
	/// <summary>
	/// Audio manager for game sounds
	/// </summary>
	public class AudioManager : MonoBehaviour, ComplexSample.Managers.IGameEventListener
	{
		[SerializeField] private AudioSource musicSource;
		[SerializeField] private AudioSource sfxSource;
		[SerializeField] private AudioClip[] musicClips;
		[SerializeField] private AudioClip[] sfxClips;

		// Public enum in secondary namespace
		public enum AudioType
		{
			Music,
			SFX,
			Voice,
			Ambient,
			UI
		}

		// Properties
		public bool IsActive { get; private set; } = true;
		public int Priority { get; set; } = 10;
		public float MasterVolume { get; set; } = 1.0f;
		public bool IsMuted { get; private set; } = false;

		// Events
		public static event Action<AudioType, string> OnAudioPlayed;
		public static event Action<AudioType> OnAudioStopped;

		void Start()
		{
			GameObject gameManagerObj = GameObject.Find("GameManager");
			if (gameManagerObj != null)
			{
				ComplexSample.Managers.GameManager gameManager = gameManagerObj.GetComponent<ComplexSample.Managers.GameManager>();
				gameManager?.RegisterEventListener(this);
			}
		}

		// Interface implementation
		public void OnGameTick()
		{
			// Audio processing per tick
		}

		public void OnPlayerJoined(int playerId)
		{
			PlaySFX("player_join");
		}

		public void OnPlayerLeft(int playerId)
		{
			PlaySFX("player_leave");
		}

		public void OnGameStateChanged(ComplexSample.Enums.GameState newState)
		{
			switch (newState)
			{
				case ComplexSample.Enums.GameState.MainMenu:
					PlayMusic("menu_theme", true);
					break;
				case ComplexSample.Enums.GameState.Playing:
					PlayMusic("game_theme", true);
					break;
				case ComplexSample.Enums.GameState.GameOver:
					PlaySFX("game_over");
					break;
			}
		}

		// Public methods
		public void PlaySFX(string clipName)
		{
			if (IsMuted) return;
			OnAudioPlayed?.Invoke(AudioType.SFX, clipName);
		}

		public void PlayMusic(string musicName, bool loop = true)
		{
			if (IsMuted) return;
			OnAudioPlayed?.Invoke(AudioType.Music, musicName);
		}

		public void StopAudio(AudioType audioType)
		{
			OnAudioStopped?.Invoke(audioType);
		}

		public void SetMute(bool mute)
		{
			IsMuted = mute;
			AudioListener.volume = mute ? 0.0f : MasterVolume;
		}

		public void SetVolume(AudioType audioType, float volume)
		{
			switch (audioType)
			{
				case AudioType.Music:
					if (musicSource != null) musicSource.volume = volume;
					break;
				case AudioType.SFX:
					if (sfxSource != null) sfxSource.volume = volume;
					break;
			}
		}

		// Nested interface in secondary namespace
		public interface IAudioProcessor
		{
			void ProcessAudio(AudioClip clip);
			float GetProcessedVolume(float originalVolume);
			bool SupportsAudioType(AudioType audioType);
			int ProcessingPriority { get; }
		}

		// Nested class for audio effects
		public class AudioEffects
		{
			public enum EffectType
			{
				Reverb,
				Echo,
				Distortion,
				LowPass,
				HighPass
			}

			public static void ApplyEffect(AudioSource source, EffectType effectType, float intensity)
			{
				// Implementation would apply audio effects
			}
		}
	}

	/// <summary>
	/// Static audio utilities in secondary namespace
	/// </summary>
	public static class AudioUtils
	{
		public const float MIN_VOLUME = 0.0f;
		public const float MAX_VOLUME = 1.0f;

		public static float ConvertToDecibels(float linearVolume)
		{
			return 20.0f * Mathf.Log10(Mathf.Clamp(linearVolume, 0.001f, 1.0f));
		}

		public static float ConvertFromDecibels(float decibels)
		{
			return Mathf.Pow(10.0f, decibels / 20.0f);
		}

		public static bool IsAudible(float volume)
		{
			return volume > 0.001f;
		}

		public static float NormalizeVolume(float volume)
		{
			return Mathf.Clamp01(volume);
		}
	}
}

// UI system namespace
namespace ComplexSample.UI
{
	/// <summary>
	/// UI controller with complex nested structure
	/// </summary>
	public class UIController : MonoBehaviour, ComplexSample.Interfaces.IAnimatable
	{
		[SerializeField] private Canvas mainCanvas;
		[SerializeField] private Button[] menuButtons;
		[SerializeField] private CanvasGroup[] panelGroups;

		// Complex nested class hierarchy
		public class UIPanel
		{
			public string PanelName { get; set; }
			public bool IsVisible { get; set; }
			public int DisplayPriority { get; set; }

			// Nested class inside nested class
			public class PanelAnimation
			{
				public enum AnimationType
				{
					Fade,
					Slide,
					Scale,
					Rotate,
					Custom
				}

				public AnimationType Type { get; set; }
				public float Duration { get; set; }
				public ComplexSample.Enums.EaseType EaseType { get; set; }

				public PanelAnimation(AnimationType type, float duration)
				{
					Type = type;
					Duration = duration;
					EaseType = ComplexSample.Enums.EaseType.EaseInOut;
				}

				// Interface inside deeply nested class
				public interface IAnimatable
				{
					void StartAnimation();
					void StopAnimation();
					void PauseAnimation();
					bool IsAnimating { get; }
					float Progress { get; }
				}
			}

			// Static nested class inside nested class
			public static class PanelFactory
			{
				public static UIPanel CreateMenuPanel()
				{
					return new UIPanel { PanelName = "Menu", IsVisible = true, DisplayPriority = 1 };
				}

				public static UIPanel CreateGamePanel()
				{
					return new UIPanel { PanelName = "Game", IsVisible = false, DisplayPriority = 2 };
				}

				public static UIPanel CreateSettingsPanel()
				{
					return new UIPanel { PanelName = "Settings", IsVisible = false, DisplayPriority = 3 };
				}
			}

			public UIPanel()
			{
				PanelName = "Default";
				IsVisible = false;
				DisplayPriority = 0;
			}

			public UIPanel(string name, bool visible, int priority)
			{
				PanelName = name;
				IsVisible = visible;
				DisplayPriority = priority;
			}

			public void Show()
			{
				IsVisible = true;
			}

			public void Hide()
			{
				IsVisible = false;
			}
		}

		// Properties
		public bool IsAnimating { get; private set; }
		public float AnimationProgress { get; private set; }

		// Events
		public static event Action<string> OnPanelOpened;
		public static event Action<string> OnPanelClosed;
		public event Action<UIPanel> OnPanelCreated;

		// Interface implementation
		public void StartAnimation()
		{
			StartCoroutine(AnimateUI());
		}

		public void StopAnimation()
		{
			StopAllCoroutines();
			IsAnimating = false;
			AnimationProgress = 0.0f;
		}

		public void PauseAnimation()
		{
			// Implementation would pause animations
		}

		// Public methods
		public void ShowPanel(string panelName)
		{
			Debug.Log($"Showing panel: {panelName}");
			OnPanelOpened?.Invoke(panelName);
		}

		public void HidePanel(string panelName)
		{
			Debug.Log($"Hiding panel: {panelName}");
			OnPanelClosed?.Invoke(panelName);
		}

		public UIPanel CreatePanel(string name)
		{
			UIPanel newPanel = UIPanel.PanelFactory.CreateMenuPanel();
			OnPanelCreated?.Invoke(newPanel);
			return newPanel;
		}

		public void SetPanelVisibility(string panelName, bool visible)
		{
			if (visible)
				ShowPanel(panelName);
			else
				HidePanel(panelName);
		}

		// Private implementation
		private IEnumerator AnimateUI()
		{
			IsAnimating = true;
			AnimationProgress = 0.0f;

			float elapsed = 0.0f;
			float duration = 1.0f;

			while (elapsed < duration)
			{
				elapsed += Time.deltaTime;
				AnimationProgress = elapsed / duration;
				yield return null;
			}

			AnimationProgress = 1.0f;
			IsAnimating = false;
		}
	}

	/// <summary>
	/// Interface for UI interaction
	/// </summary>
	public interface IUIInteractable
	{
		void OnHover();
		void OnClick();
		void OnDoubleClick();
		void OnDragStart();
		void OnDragEnd();
		bool IsInteractable { get; set; }
		int Priority { get; }
		Vector2 Position { get; set; }
	}

	/// <summary>
	/// UI button component with extended functionality
	/// </summary>
	public class UIButton : MonoBehaviour, IUIInteractable
	{
		[SerializeField] private Button button;
		[SerializeField] private Image buttonImage;

		public enum ButtonState
		{
			Normal,
			Highlighted,
			Pressed,
			Disabled
		}

		// Properties
		public bool IsInteractable { get; set; } = true;
		public int Priority { get; private set; } = 0;
		public Vector2 Position { get; set; }
		public ButtonState CurrentState { get; private set; }

		// Events
		public event Action<UIButton> OnButtonClicked;

		void Start()
		{
			if (button != null)
			{
				button.onClick.AddListener(() => OnClick());
			}
		}

		// Interface implementation
		public void OnHover()
		{
			CurrentState = ButtonState.Highlighted;
		}

		public void OnClick()
		{
			if (IsInteractable)
			{
				CurrentState = ButtonState.Pressed;
				OnButtonClicked?.Invoke(this);
			}
		}

		public void OnDoubleClick()
		{
			// Handle double-click logic
		}

		public void OnDragStart()
		{
			// Handle drag start
		}

		public void OnDragEnd()
		{
			// Handle drag end
		}

		// Public methods
		public void SetButtonState(ButtonState newState)
		{
			CurrentState = newState;
			IsInteractable = newState != ButtonState.Disabled;
		}
	}
}

// Network system namespace
namespace ComplexSample.Network
{
	/// <summary>
	/// Network manager for multiplayer functionality
	/// </summary>
	public class NetworkManager : MonoBehaviour, ComplexSample.Interfaces.INetworkSync
	{
		[SerializeField] private int maxConnections = 8;
		[SerializeField] private float timeoutDuration = 30.0f;

		private ComplexSample.Enums.NetworkStatus connectionStatus = ComplexSample.Enums.NetworkStatus.Disconnected;
		private List<ComplexSample.Data.NetworkPacket> pendingPackets = new List<ComplexSample.Data.NetworkPacket>();

		// Properties
		public bool IsNetworked { get; private set; } = true;
		public int NetworkId { get; private set; } = -1;
		public ComplexSample.Enums.NetworkStatus Status { get { return connectionStatus; } }
		public int ConnectedClients { get; private set; }

		// Events
		public static event Action<ComplexSample.Enums.NetworkStatus> OnNetworkStatusChanged;
		public static event Action<ComplexSample.Data.NetworkPacket> OnPacketReceived;
		public event Action<int> OnClientConnected;
		public event Action<int> OnClientDisconnected;

		void Start()
		{
			NetworkId = UnityEngine.Random.Range(1000, 9999);
		}

		// Interface implementation
		public void SendNetworkUpdate(byte[] data)
		{
			if (connectionStatus != ComplexSample.Enums.NetworkStatus.Connected) return;

			ComplexSample.Data.NetworkPacket packet = new ComplexSample.Data.NetworkPacket(
				ComplexSample.Data.NetworkPacket.PacketType.GameState,
				NetworkId,
				data
			);

			pendingPackets.Add(packet);
			ProcessPendingPackets();
		}

		public void ReceiveNetworkUpdate(byte[] data)
		{
			ComplexSample.Data.NetworkPacket packet = new ComplexSample.Data.NetworkPacket(
				ComplexSample.Data.NetworkPacket.PacketType.GameState,
				-1,
				data
			);

			OnPacketReceived?.Invoke(packet);
		}

		// Public methods
		public void StartServer()
		{
			ChangeNetworkStatus(ComplexSample.Enums.NetworkStatus.Connecting);
			StartCoroutine(SimulateConnection(true));
		}

		public void StartClient(string serverAddress)
		{
			ChangeNetworkStatus(ComplexSample.Enums.NetworkStatus.Connecting);
			StartCoroutine(SimulateConnection(false));
		}

		public void Disconnect()
		{
			ChangeNetworkStatus(ComplexSample.Enums.NetworkStatus.Disconnected);
			ConnectedClients = 0;
		}

		public void SendChatMessage(string message)
		{
			byte[] messageData = System.Text.Encoding.UTF8.GetBytes(message);
			ComplexSample.Data.NetworkPacket chatPacket = new ComplexSample.Data.NetworkPacket(
				ComplexSample.Data.NetworkPacket.PacketType.ChatMessage,
				NetworkId,
				messageData
			);

			pendingPackets.Add(chatPacket);
		}

		// Private implementation
		private void ChangeNetworkStatus(ComplexSample.Enums.NetworkStatus newStatus)
		{
			connectionStatus = newStatus;
			OnNetworkStatusChanged?.Invoke(newStatus);
		}

		private IEnumerator SimulateConnection(bool isServer)
		{
			yield return new WaitForSeconds(2.0f);

			if (UnityEngine.Random.Range(0f, 1f) > 0.1f) // 90% success rate
			{
				ChangeNetworkStatus(ComplexSample.Enums.NetworkStatus.Connected);
				ConnectedClients = isServer ? 1 : 0;
			}
			else
			{
				ChangeNetworkStatus(ComplexSample.Enums.NetworkStatus.Failed);
			}
		}

		private void ProcessPendingPackets()
		{
			for (int i = pendingPackets.Count - 1; i >= 0; i--)
			{
				// Simulate packet processing
				pendingPackets.RemoveAt(i);
			}
		}
	}

	/// <summary>
	/// Network client for connecting to servers
	/// </summary>
	public class NetworkClient : ComplexSample.Interfaces.INetworkSync
	{
		private int clientId;
		private ComplexSample.Enums.NetworkStatus status;

		// Properties
		public bool IsNetworked { get; private set; } = true;
		public int NetworkId { get { return clientId; } }
		public ComplexSample.Enums.NetworkStatus Status { get { return status; } }

		// Constructor
		public NetworkClient(int id)
		{
			clientId = id;
			status = ComplexSample.Enums.NetworkStatus.Disconnected;
		}

		// Interface implementation
		public void SendNetworkUpdate(byte[] data)
		{
			// Send data to server
		}

		public void ReceiveNetworkUpdate(byte[] data)
		{
			// Process received data
		}

		// Public methods
		public void Connect(string serverAddress)
		{
			status = ComplexSample.Enums.NetworkStatus.Connecting;
		}

		public void Disconnect()
		{
			status = ComplexSample.Enums.NetworkStatus.Disconnected;
		}
	}

	/// <summary>
	/// Static network utilities
	/// </summary>
	public static class NetworkUtils
	{
		public const int DEFAULT_PORT = 7777;
		public const int MAX_PACKET_SIZE = 1024;

		public static bool IsValidIPAddress(string ipAddress)
		{
			return !string.IsNullOrEmpty(ipAddress) && ipAddress.Contains(".");
		}

		public static byte[] SerializeString(string data)
		{
			return System.Text.Encoding.UTF8.GetBytes(data);
		}

		public static string DeserializeString(byte[] data)
		{
			return System.Text.Encoding.UTF8.GetString(data);
		}

		public static bool IsPortValid(int port)
		{
			return port > 0 && port < 65536;
		}
	}
}