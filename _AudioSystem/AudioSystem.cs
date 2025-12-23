using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

using SPACE_UTIL;

namespace SPACE_AudioSystem
{
	/// <summary>
	/// Professional audio system with 2D/3D support, pooling, and type-safe enum access.
	/// 
	/// Setup:
	/// 1. Create Resources/audio/ folder with subfolders (sfx, music, ambient)
	/// 2. Define AudioType enum matching your folder structure
	/// 3. Create AudioManager GameObject with this component
	/// 4. Optionally assign AudioMixer for advanced mixing
	/// 
	/// Usage:
	/// AUDIO.play2D(AudioType.sfx__jump);
	/// AUDIO.play3D(AudioType.sfx__explosion, position);
	/// AUDIO.playMusic(AudioType.music__level1, fadeTime: 2f);
	/// AUDIO.stopAll();
	/// </summary>
	public class AudioManager : MonoBehaviour
	{
		#region Singleton
		private static AudioManager _instance;
		public static AudioManager Instance
		{
			get
			{
				if (_instance == null)
				{
					// Try to find existing instance
					_instance = FindObjectOfType<AudioManager>();

					if (_instance == null)
					{
						// Create new instance
						GameObject go = new GameObject("AudioManager");
						_instance = go.AddComponent<AudioManager>();
						DontDestroyOnLoad(go);
						Debug.Log("[AudioManager] Auto-created instance".colorTag("lime"));
					}
				}
				return _instance;
			}
		}

		private void Awake()
		{
			if (_instance != null && _instance != this)
			{
				Destroy(gameObject);
				return;
			}

			_instance = this;
			DontDestroyOnLoad(gameObject);

			Initialize();
		}
		#endregion

		#region Configuration
		[Header("Audio Configuration")]
		[SerializeField] private AudioMixerGroup sfxMixerGroup;
		[SerializeField] private AudioMixerGroup musicMixerGroup;
		[SerializeField] private AudioMixerGroup ambientMixerGroup;

		[Header("Pool Settings")]
		[SerializeField] private int initialPoolSize = 10;
		[SerializeField] private int maxPoolSize = 30;

		[Header("Volume Settings (0-1)")]
		[Range(0f, 1f)] public float masterVolume = 1f;
		[Range(0f, 1f)] public float sfxVolume = 1f;
		[Range(0f, 1f)] public float musicVolume = 0.7f;
		[Range(0f, 1f)] public float ambientVolume = 0.5f;

		[Header("3D Audio Settings")]
		[SerializeField] private float defaultMinDistance = 1f;
		[SerializeField] private float defaultMaxDistance = 50f;
		[SerializeField] private AnimationCurve spatialBlendCurve = AnimationCurve.Linear(0, 0, 1, 1);
		#endregion

		#region Audio Source Pooling
		private class AudioSourcePool
		{
			private List<AudioSource> available = new List<AudioSource>();
			private List<AudioSource> inUse = new List<AudioSource>();
			private Transform poolParent;
			private AudioMixerGroup mixerGroup;

			public AudioSourcePool(Transform parent, AudioMixerGroup mixer)
			{
				poolParent = parent;
				mixerGroup = mixer;
			}

			public AudioSource Get()
			{
				AudioSource source;

				if (available.Count > 0)
				{
					source = available[available.Count - 1];
					available.RemoveAt(available.Count - 1);
				}
				else
				{
					GameObject go = new GameObject("PooledAudioSource");
					go.transform.SetParent(poolParent);
					source = go.AddComponent<AudioSource>();
					source.outputAudioMixerGroup = mixerGroup;
					source.playOnAwake = false;
				}

				inUse.Add(source);
				return source;
			}

			public void Return(AudioSource source)
			{
				if (source == null) return;

				inUse.Remove(source);

				// Reset to default state
				source.Stop();
				source.clip = null;
				source.loop = false;
				source.spatialBlend = 0f;
				source.transform.position = Vector3.zero;
				source.transform.SetParent(poolParent);

				available.Add(source);
			}

			public void Initialize(int count)
			{
				for (int i = 0; i < count; i++)
				{
					GameObject go = new GameObject($"PooledAudioSource_{i}");
					go.transform.SetParent(poolParent);
					AudioSource source = go.AddComponent<AudioSource>();
					source.outputAudioMixerGroup = mixerGroup;
					source.playOnAwake = false;
					available.Add(source);
				}
			}

			public void StopAll()
			{
				foreach (var source in inUse)
				{
					if (source != null)
						source.Stop();
				}
			}

			public int GetActiveCount() => inUse.Count;
		}

		private AudioSourcePool sfxPool;
		private AudioSourcePool musicPool;
		private AudioSourcePool ambientPool;
		#endregion

		#region Initialization
		private void Initialize()
		{
			// Create pool parent
			Transform poolParent = new GameObject("AudioSourcePools").transform;
			poolParent.SetParent(transform);

			// Initialize pools
			sfxPool = new AudioSourcePool(poolParent, sfxMixerGroup);
			musicPool = new AudioSourcePool(poolParent, musicMixerGroup);
			ambientPool = new AudioSourcePool(poolParent, ambientMixerGroup);

			sfxPool.Initialize(initialPoolSize);
			musicPool.Initialize(2); // Usually only need 1-2 music tracks
			ambientPool.Initialize(3); // A few ambient layers

			Debug.Log("[AudioManager] Initialized with pooling".colorTag("lime"));
		}
		#endregion

		#region Core Audio Playback
		/// <summary>
		/// Play 2D audio (UI sounds, global effects)
		/// Returns the AudioSource for advanced control
		/// </summary>
		public AudioSource Play2D(object audioType, float volume = 1f, float pitch = 1f, bool loop = false)
		{
			AudioClip clip = R.get<AudioClip>(audioType);
			if (clip == null)
			{
				Debug.Log($"[AudioManager] AudioClip not found: {audioType}".colorTag("red"));
				return null;
			}

			AudioSource source = sfxPool.Get();
			source.clip = clip;
			source.volume = volume * sfxVolume * masterVolume;
			source.pitch = pitch;
			source.loop = loop;
			source.spatialBlend = 0f; // 2D

			source.Play();

			if (!loop)
				StartCoroutine(ReturnToPoolAfterPlay(source, sfxPool));

			return source;
		}

		/// <summary>
		/// Play 3D audio at a world position (explosions, footsteps, etc.)
		/// Returns the AudioSource for advanced control
		/// </summary>
		public AudioSource Play3D(object audioType, Vector3 position, float volume = 1f, float pitch = 1f,
			float minDistance = -1f, float maxDistance = -1f)
		{
			AudioClip clip = R.get<AudioClip>(audioType);
			if (clip == null)
			{
				Debug.Log($"[AudioManager] AudioClip not found: {audioType}".colorTag("red"));
				return null;
			}

			AudioSource source = sfxPool.Get();
			source.transform.position = position;
			source.clip = clip;
			source.volume = volume * sfxVolume * masterVolume;
			source.pitch = pitch;
			source.spatialBlend = 1f; // 3D
			source.minDistance = minDistance > 0 ? minDistance : defaultMinDistance;
			source.maxDistance = maxDistance > 0 ? maxDistance : defaultMaxDistance;
			source.rolloffMode = AudioRolloffMode.Linear;

			source.Play();
			StartCoroutine(ReturnToPoolAfterPlay(source, sfxPool));

			return source;
		}

		/// <summary>
		/// Play 3D audio attached to a GameObject (enemy sounds, moving objects)
		/// Audio source follows the transform
		/// </summary>
		public AudioSource Play3DAttached(object audioType, Transform parent, float volume = 1f, float pitch = 1f,
			float minDistance = -1f, float maxDistance = -1f, bool loop = false)
		{
			AudioClip clip = R.get<AudioClip>(audioType);
			if (clip == null)
			{
				Debug.Log($"[AudioManager] AudioClip not found: {audioType}".colorTag("red"));
				return null;
			}

			AudioSource source = sfxPool.Get();
			source.transform.SetParent(parent);
			source.transform.localPosition = Vector3.zero;
			source.clip = clip;
			source.volume = volume * sfxVolume * masterVolume;
			source.pitch = pitch;
			source.loop = loop;
			source.spatialBlend = 1f; // 3D
			source.minDistance = minDistance > 0 ? minDistance : defaultMinDistance;
			source.maxDistance = maxDistance > 0 ? maxDistance : defaultMaxDistance;
			source.rolloffMode = AudioRolloffMode.Linear;

			source.Play();

			if (!loop)
				StartCoroutine(ReturnToPoolAfterPlay(source, sfxPool));

			return source;
		}

		/// <summary>
		/// Play music with optional crossfade
		/// </summary>
		public AudioSource PlayMusic(object audioType, float fadeTime = 2f, float volume = 1f, bool loop = true)
		{
			AudioClip clip = R.get<AudioClip>(audioType);
			if (clip == null)
			{
				Debug.Log($"[AudioManager] Music clip not found: {audioType}".colorTag("red"));
				return null;
			}

			// Fade out current music
			if (currentMusicSource != null && currentMusicSource.isPlaying)
			{
				StartCoroutine(FadeOut(currentMusicSource, fadeTime, musicPool));
			}

			// Start new music
			AudioSource source = musicPool.Get();
			source.clip = clip;
			source.volume = 0f; // Start silent
			source.loop = loop;
			source.spatialBlend = 0f; // Music is always 2D

			source.Play();
			StartCoroutine(FadeIn(source, fadeTime, volume * musicVolume * masterVolume));

			currentMusicSource = source;
			return source;
		}

		/// <summary>
		/// Play ambient sound with optional fade
		/// </summary>
		public AudioSource PlayAmbient(object audioType, float fadeTime = 1f, float volume = 1f, bool loop = true)
		{
			AudioClip clip = R.get<AudioClip>(audioType);
			if (clip == null)
			{
				Debug.Log($"[AudioManager] Ambient clip not found: {audioType}".colorTag("red"));
				return null;
			}

			AudioSource source = ambientPool.Get();
			source.clip = clip;
			source.volume = 0f;
			source.loop = loop;
			source.spatialBlend = 0f; // Ambient is 2D

			source.Play();
			StartCoroutine(FadeIn(source, fadeTime, volume * ambientVolume * masterVolume));

			activeAmbientSources.Add(source);
			return source;
		}

		private AudioSource currentMusicSource;
		private List<AudioSource> activeAmbientSources = new List<AudioSource>();
		#endregion

		#region Stop/Pause Controls
		public void StopAllSFX()
		{
			sfxPool.StopAll();
		}

		public void StopMusic(float fadeTime = 1f)
		{
			if (currentMusicSource != null && currentMusicSource.isPlaying)
			{
				StartCoroutine(FadeOut(currentMusicSource, fadeTime, musicPool));
				currentMusicSource = null;
			}
		}

		public void StopAllAmbient(float fadeTime = 1f)
		{
			foreach (var source in activeAmbientSources)
			{
				if (source != null && source.isPlaying)
					StartCoroutine(FadeOut(source, fadeTime, ambientPool));
			}
			activeAmbientSources.Clear();
		}

		public void StopAll()
		{
			StopAllSFX();
			StopMusic(0f);
			StopAllAmbient(0f);
		}
		#endregion

		#region Coroutines
		private IEnumerator ReturnToPoolAfterPlay(AudioSource source, AudioSourcePool pool)
		{
			yield return new WaitWhile(() => source.isPlaying);
			pool.Return(source);
		}

		private IEnumerator FadeIn(AudioSource source, float duration, float targetVolume)
		{
			float elapsed = 0f;
			while (elapsed < duration)
			{
				elapsed += Time.unscaledDeltaTime;
				source.volume = Mathf.Lerp(0f, targetVolume, elapsed / duration);
				yield return null;
			}
			source.volume = targetVolume;
		}

		private IEnumerator FadeOut(AudioSource source, float duration, AudioSourcePool pool)
		{
			float startVolume = source.volume;
			float elapsed = 0f;

			while (elapsed < duration)
			{
				elapsed += Time.unscaledDeltaTime;
				source.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
				yield return null;
			}

			source.Stop();
			pool.Return(source);
		}
		#endregion

		#region Volume Control (Runtime)
		public void SetMasterVolume(float volume)
		{
			masterVolume = Mathf.Clamp01(volume);
			// Update all active sources
			RefreshVolumes();
		}

		public void SetSFXVolume(float volume)
		{
			sfxVolume = Mathf.Clamp01(volume);
			RefreshVolumes();
		}

		public void SetMusicVolume(float volume)
		{
			musicVolume = Mathf.Clamp01(volume);
			if (currentMusicSource != null)
				currentMusicSource.volume = musicVolume * masterVolume;
		}

		public void SetAmbientVolume(float volume)
		{
			ambientVolume = Mathf.Clamp01(volume);
			foreach (var source in activeAmbientSources)
			{
				if (source != null)
					source.volume = ambientVolume * masterVolume;
			}
		}

		private void RefreshVolumes()
		{
			// This is a simplified approach - in production you'd store original volumes per source
			if (currentMusicSource != null)
				currentMusicSource.volume = musicVolume * masterVolume;

			foreach (var source in activeAmbientSources)
			{
				if (source != null)
					source.volume = ambientVolume * masterVolume;
			}
		}
		#endregion

		#region Debug Info
		[ContextMenu("Show Pool Status")]
		public void ShowPoolStatus()
		{
			Debug.Log($"[AudioManager] SFX Pool: {sfxPool.GetActiveCount()} active sources".colorTag("cyan"));
			Debug.Log($"[AudioManager] Music: {(currentMusicSource != null && currentMusicSource.isPlaying ? "Playing" : "Stopped")}".colorTag("cyan"));
			Debug.Log($"[AudioManager] Ambient: {activeAmbientSources.Count} active layers".colorTag("cyan"));
		}
		#endregion
	}

	/// <summary>
	/// Static API for easy access everywhere
	/// Usage: AUDIO.play2D(AudioType.jump);
	/// </summary>
	public static class AUDIO
	{
		public static AudioSource play2D(object audioType, float volume = 1f, float pitch = 1f, bool loop = false)
		{
			return AudioManager.Instance.Play2D(audioType, volume, pitch, loop);
		}

		public static AudioSource play3D(object audioType, Vector3 position, float volume = 1f, float pitch = 1f,
			float minDistance = -1f, float maxDistance = -1f)
		{
			return AudioManager.Instance.Play3D(audioType, position, volume, pitch, minDistance, maxDistance);
		}

		public static AudioSource play3DAttached(object audioType, Transform parent, float volume = 1f, float pitch = 1f,
			float minDistance = -1f, float maxDistance = -1f, bool loop = false)
		{
			return AudioManager.Instance.Play3DAttached(audioType, parent, volume, pitch, minDistance, maxDistance, loop);
		}

		public static AudioSource playMusic(object audioType, float fadeTime = 2f, float volume = 1f)
		{
			return AudioManager.Instance.PlayMusic(audioType, fadeTime, volume);
		}

		public static AudioSource playAmbient(object audioType, float fadeTime = 1f, float volume = 1f)
		{
			return AudioManager.Instance.PlayAmbient(audioType, fadeTime, volume);
		}

		public static void stopAllSFX() => AudioManager.Instance.StopAllSFX();
		public static void stopMusic(float fadeTime = 1f) => AudioManager.Instance.StopMusic(fadeTime);
		public static void stopAll() => AudioManager.Instance.StopAll();

		public static void setMasterVolume(float volume) => AudioManager.Instance.SetMasterVolume(volume);
		public static void setSFXVolume(float volume) => AudioManager.Instance.SetSFXVolume(volume);
		public static void setMusicVolume(float volume) => AudioManager.Instance.SetMusicVolume(volume);
	}
}

/* ===============================================
   USAGE EXAMPLES
   ===============================================

// 1. Define your audio enum (in your game namespace):
public enum AudioType
{
	sfx__jump,
	sfx__shoot,
	sfx__explosion,
	sfx__footstep,
	music__menu,
	music__level1,
	music__boss,
	ambient__forest,
	ambient__rain,
}

// 2. Organize Resources folder:
Resources/
	audio/
		sfx/
			jump.wav
			shoot.wav
			explosion.wav
		music/
			menu.mp3
			level1.mp3
		ambient/
			forest.wav
			rain.wav

// 3. Use anywhere in your code:

// Simple 2D sound
AUDIO.play2D(AudioType.sfx__jump);

// 2D with volume and pitch variation
AUDIO.play2D(AudioType.sfx__shoot, volume: 0.8f, pitch: UnityEngine.Random.Range(0.9f, 1.1f));

// 3D sound at explosion position
AUDIO.play3D(AudioType.sfx__explosion, explosionPos, volume: 1.5f);

// 3D sound attached to enemy (follows transform)
AUDIO.play3DAttached(AudioType.sfx__footstep, enemyTransform, loop: true);

// Crossfade music
AUDIO.playMusic(AudioType.music__level1, fadeTime: 2f);

// Layer ambient sounds
AUDIO.playAmbient(AudioType.ambient__forest, fadeTime: 3f);
AUDIO.playAmbient(AudioType.ambient__rain, fadeTime: 3f, volume: 0.3f);

// Advanced: Get AudioSource for custom control
AudioSource loopingSFX = AUDIO.play2D(AudioType.sfx__engine, loop: true);
// Later...
loopingSFX.Stop();

// Volume control (for settings menu)
AUDIO.setMasterVolume(PlayerPrefs.GetFloat("MasterVolume", 1f));
AUDIO.setSFXVolume(PlayerPrefs.GetFloat("SFXVolume", 1f));
AUDIO.setMusicVolume(PlayerPrefs.GetFloat("MusicVolume", 0.7f));

// Stop all audio (scene transitions)
AUDIO.stopAll();

   =============================================== */