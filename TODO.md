based on research and utility required i have build `UTIL.cs` and more.
```md
=== Component Abbreviations ===
cr = CanvasRenderer
sr = ScrollRect
btnO = Button | Image | Outline
img = Image
autoFitH = HorizontalLayoutGroup | ContentSizeFitter
================================

=== Asset Type Abbreviations ===
mat = Material
pf = Prefab
tex = Texture
cs = Script
scene = Scene
txt = TextAsset
================================

./_/
├ =0.5.ver (DefaultAsset)
├ @PROMPT/
├ Critical-TODO.md (txt)
├ README/
│ └ UTIL.cs Improve --prompt.md (txt)
├ README.md (txt)
├ TODO.md (txt)
├ UnityLifeCycle.md (txt)
├ _/
│ ├ =1.0.2.ver (DefaultAsset)
│ ├ DemoScene/
│ │ └ _ DemoScene.unity (scene)
│ ├ Scripts/
│ │ ├ @PROMPT/
│ │ ├ INITManager.cs (cs | INITManager)
│ │ ├ README/
│ │ │ └ UTIL__v2_Board_MonoInterfaceFinder.cs.md (txt)
│ │ └ UTIL/
│ │   ├ UTIL.cs (cs | unknown)
│ │   └ UTIL_FLOW.md (txt)
│ ├ Textures/
│ │ ├ Transparent_1x1.png (tex | 1×1 | RGBA32)
│ │ │ └ Transparent_1x1 (Sprite)
│ │ └ White_1x1.png (tex | 1×1 | RGB24)
│ │   └ White_1x1 (Sprite)
│ ├ TM/
│ │ └ FONTS/
│ │   ├ CONSOLA.TTF (Font)
│ │   │ ├ Font Material (mat | GUI/Text Shader)
│ │   │ └ Font Texture (tex | 256×256 | Alpha8)
│ │   ├ CONSOLAI.TTF (Font)
│ │   │ ├ Font Material (mat | GUI/Text Shader)
│ │   │ └ Font Texture (tex | 256×256 | Alpha8)
│ │   └ pixelplay.ttf (Font)
│ │     ├ Font Material (mat | GUI/Text Shader)
│ │     └ Font Texture (tex | 256×256 | Alpha8)
│ └ UnityEditorUtil/
│   └ Editor/
│     ├ Resources/
│     │ └ custom GUISkin.guiskin (GUISkin)
│     ├ ToTextEditorUtil.cs (cs | unknown)
│     └ ViewGameDataDecrEditorUtil.cs (cs | ViewGameDataDecrEditorUtil)
├ _CamSystem/
│ ├ Mats/
│ │ └ mat.mat (mat | URP/Lit)
│ ├ Scenes/
│ │ └ BirdViewCamManager Demo.unity (scene)
│ └ Scripts/
│   └ BirdViewCamManager.cs (cs | BirdViewCamManager)
├ _DrawSystem/
│ ├ DrawManager.cs (cs | unknown)
│ └ DrawSystemDoc.md (txt)
├ _Game/
│ ├ UTILDependPerGame.md (txt)
│ └ _Secure/
├ _NodeSystem/
│ ├ Scenes/
│ │ └ NodeSystem Demo.unity (scene)
│ └ Scripts/
│   ├ GraphViewer/
│   │ ├ Demo GameData.txt (txt)
│   │ ├ GraphNodeManager.cs (cs | GraphNodeManager)
│   │ ├ GraphNode_IO.cs (cs | GraphNode_IO)
│   │ ├ Prefabs/
│   │ │ └ Resources/
│   │ │   └ pfGraphNode.prefab (pf | scale:1.0 | GraphNode_IO)
│   │ ├ README.md (txt)
│   │ └ Textures/
│   │   └ White_1x1.png (tex | 1×1 | RGB24)
│   │     └ White_1x1 (Sprite)
│   └ NodeManager.cs (cs | NodeManager)
├ _SyntaxSystem/
│ └ SyntaxManager.cs (cs | unknown)
├ _Template/
│ └ newSceneTemplate/
│   ├ New Scene.unity (scene)
│   └ New SceneTemplate.scenetemplate (SceneTemplateAsset)
├ _UIRebindingSystem/
│ ├ DEBUG_IAEventsAssetController.cs (cs | DEBUG_IAEventsAssetController)
│ ├ DEBUG_SampleGameSave.cs (cs | DEBUG_SampleGameSave)
│ ├ DEBUG_UIRebindingMenuButton.cs (cs | DEBUG_UIRebindingMenuButton)
│ ├ GameStore.cs (cs | GameStore)
│ ├ InputActions/
│ │ └ NewInputAction.inputactions (InputActionAsset)
│ │   ├ character/jump (InputActionReference)
│ │   └ character/shoot (InputActionReference)
│ ├ Prefabs/
│ │ ├ pfButton.prefab (pf | scale:1.0 | cr, btnO, autoFitH)
│ │ ├ pftemplate -- Scroll View.prefab (pf | scale:1.0 | cr, sr, img)
│ │ └ pftemplate --row.prefab (pf | scale:1.0 | autoFitH)
│ ├ UIRebindingSystem --flow.md (txt)
│ ├ UIRebindingSystem Demo.unity (scene)
│ ├ UIRebindingSystem.cs (cs | UIRebindingSystem)
│ └ UIRebindingSystem.md (txt)
├ _UIToolTipSystem/
│ ├ Prefabs/
│ │ └ ToolTip.prefab (pf | scale:1.0 | cr, autoFitH, UIToolTip)
│ ├ Scenes/
│ │ └ ToolTip Demo.unity (scene)
│ └ Scripts/
│   └ UIToolTip.cs (cs | UIToolTip)
└ _WebReqSystem/
  ├ Scenes/
  │ ├ WebReqSystem Demo.unity (scene)
  │ └ WebReqSystem DemoSettings.lighting (LightingSettings)
  ├ Scripts/
  │ ├ Demo/
  │ │ ├ DEBUG_WebRequest.cs (cs | DEBUG_WebRequest)
  │ │ └ WebReqSystem Demo.unity (scene)
  │ ├ NameGen/
  │ │ └ JapaneseNameGenerator.cs (cs | JapaneseNameGenerator)
  │ └ WebReqManager.cs (cs | WebReqManager)
  └ Textures/
    └ White_1x1.png (tex | 1×1 | RGB24)
      └ White_1x1 (Sprite)
```
what you think so far, what the further improvement required (crusial system required as indie game dev)?
also i heard recently
the  `TODO.md` contains the Systems built by code monkey(which was announced recently),

## Act As Professional Indie Dev
## Audio System (⭐⭐⭐) HIGHEST PRIORITY,
	- should also work in 3D(meaning played by a certain 3D gameObject to see dopler effect no ?), including 2D
	- planning to play Audio as AudioManager.PlayMusic(MusicType.audio__music__battle, fadeTime: 1f);  // which play audio located in audio/music/battle
	- for 3D AudioManager.PlayMusic(MusicType.audio__music__battle, transform.position, fadeTime: 1f); // using enum instead of string so its type safe,also since its easier to move transofrm or parent it so 3D sound with dopler can be played, consider pooling into Dictionary and dont destroy on load,   
	- or what you think is a better way to call the same?
# Code Monkey Systems:
  ## Welcome
	What is Problem Solving?
	Preview
	How this course is structured
	Start
	Problem Solving Frameworks
	Start
	Download Companion Project
	Start
	Private Livestreams and Discord
	Start
  ## Health System
	Health System Design
	Start
	Health System Implementation
	Start
	10_01 Scenario Complete
	Start
	10_02 Bullets Healing
	Start
	10_03 Health Bar not Updating
	Start
	10_04 Health Bar not Updating 2
	Start
	10_05 No Damage
	Start
	10_06 Going under 0 and above Max
	Start
	10_07 Bullets Spawning Wrong Position
	Start
	10_08 Health Bar Incorrect
	Start
	10_09 Error on Damage
	Start
	10_10 Bullet not Moving
	Start
	10_11 Health Bar Null Reference
	Start
	10_12 Enemy dying instantly
	Start
	10_13 Bullet not Damaging
	Start
	10_14 Fast Bullet
	Start
	10_15 Bullets no clean up
	Start
	10_16 Bullets not damaging
	Start
	10_17 Fix NullReferenceException
	Start
	10_18 Not dealing damage
	Start
	Intermission
	Intermission
	Start
  ## Movement System
	Movement System Design
	Start
	Movement System Implementation
	Start
	11_01 Scenario Complete
	Start
	11_02 Not Moving
	Start
	11_03 Error
	Start
	11_04 Movement is inverted
	Start
	11_05 No diagonals
	Start
	11_06 Not colliding with walls
	Start
	11_07 Movement Slow Inconsistent
	Start
	11_08 Player barely moves
	Start
	11_09 Only Moves Right
	Start
	11_10 Going through walls
	Start
	11_11 Pebbles solid
	Start
	11_12 Movement not linear
	Start
	11_13 Player rotating
	Start
	11_14 Movement jittery
	Start
	11_15 Cannot push pebbles
	Start
	11_16 Collisions strange
	Start
  ## Inventory System
	Inventory System Design + Implementation
	Start
	12_01 Scenario Completed
	Start
	12_02 Cannot pick up items
	Start
	12_03 Items not being added
	Start
	12_04 Picks up more stuff
	Start
	12_05 Always has Apple
	Start
	12_06 Picking object throws Error
	Start
	12_07 Dropping items duplicates
	Start
	12_08 Can only pick up One item
	Start
	12_09 Cannot pick up items
	Start
	12_10 Only adding Apples
	Start
	12_11 Cannot pick up items
	Start
	12_12 Not dropping items
	Start
	12_13 Dropping items when empty throws Error
	Start
	12_14 Invisible Items
	Start
	12_15 Error picking items
	Start
	12_16 Fix NullReferenceException
	Start
	12_17 Items not showing UI
	Start
	12_18 Drops multiple items at once
	Start
  ## Crafting System
	Crafting System Design + Implementation
	Start
	13_01 Scenario Completed
	Start
	13_02 Error InvalidOperation
	Start
	13_03 Crafts without Inputs
	Start
	13_04 Not Crafting Items
	Start
	13_05 Gold ingots only taking one ore
	Start
	13_06 Not crafting
	Start
	13_07 Consuming too many items
	Start
	13_08 Crafting Gold ingot with one ore
	Start
	13_09 Cannot craft Gold ingot
	Start
	13_10 Adding Gold ore doesn't work
	Start
	13_11 Add item NullReferenceException
	Start
	13_12 Inventory Empty
	Start
	13_13 Craft or Add with Error
	Start
	13_14 Crafting consumes but no craft
	Start
	13_15 Items all Apples
	Start
	13_16 Inventory doesn't work
	Start
	Enemy AI
	Enemy AI Design + Implementation
	Start
	14_01 Scenario Completed
	Start
	14_02 Attacks too far
	Start
	14_03 Aim wrong
	Start
	14_04 Doesn't Attack Player
	Start
	14_05 Enemy doesn't attack
	Start
	14_06 Player dying in one shot
	Start
	14_07 Shooting incorrectly
	Start
	14_08 Enemy doesn't find Player target
	Start
	14_09 Enemy attacking from too far
	Start
	14_10 Player not taking damage
	Start
	14_11 Enemy shooting itself
	Start
	14_12 Enemy moving while attacking
	Start
	14_13 Fix NullReferenceException
	Start
	14_14 Enemy shoots too far
	Start
	14_15 Attacking too far
	Start
	14_16 Fix Error
	Start
	14_17 Bullets wrong
	Start
	14_18 Enemy doesn't find Player
	Start
  ## Key Door System
	Key Door System Design + Implementation
	Start
	15_01 Scenario Completed
	Start
	15_02 Fix NullReferenceException
	Start
	15_03 Not picking up Keys
	Start
	15_04 Cannot open Red Door
	Start
	15_05 Door doesn't open
	Start
	15_06 Fix NullReferenceException
	Start
	15_07 Red Door won't open
	Start
	15_08 Blue Door won't open
	Start
	15_09 Fix MissingReferenceException
	Start
	15_10 Player not moving
	Start
	15_11 Not picking keys
	Start
	15_12 Not picking keys 2
	Start
	15_13 Blue Key grabbed twice
	Start
	15_14 Adding tons of keys
	Start
	15_15 Key being removed
	Start
	15_16 Key not being consumed
	Start
	15_17 Inventory starts with Red Key
	Start
	15_18 Key not being added
	Start
  ## Gathering System
	Gathering System Design + Implementation
	Start
	16_01 Scenario Completed
	Start
	16_02 Player not gathering anything
	Start
	16_03 Player not gathering anything 2
	Start
	16_04 Wood picked up double
	Start
	16_05 First pickup 10
	Start
	16_06 Gold gives Wood
	Start
	16_07 Fix NullReferenceException
	Start
	16_08 Animation not playing
	Start
	16_09 Not gathering
	Start
	16_10 Not gathering 2
	Start
	16_11 Not picking up items
	Start
	16_12 Start Stop inconsistent
	Start
	16_13 Spawning too much Gold
	Start
	16_14 Not gathering
	Start
	16_15 Fix NullReferenceException
	Start
  ## Save System
	17_01 Scenario Completed
	Start
	17_02 Game not loading
	Start
	17_03 Fix NullReferenceException on Load
	Start
	17_04 Not saving amounts
	Start
	17_05 Not saving amounts 2
	Start
	17_06 Not saving Gold ore in World
	Start
	17_07 Not loading amounts
	Start
	17_08 Not loading Wood objects
	Start
  ## Interaction System
	18_01 Scenario Completed
	Start
	18_02 Cannot interact
	Start
	18_03 Fix NullReferenceException
	Start
	18_04 Cannot interact
	Start
	18_05 Player not moving
	Start
	18_06 Interact popup wrong
	Start
	18_07 Stuck on picking Pistol
	Start
	18_08 Fix Error NPC
	Start
	18_09 Interacting in open space throws Error
	Start
	18_10 Player starts off disappearing
	Start
	18_11 Cannot pick up Pistol
	Start
	18_12 Pistol vanishes on pickup
	Start
	18_13 Cannot Interact with NPC
	Start
	18_14 Cannot interact with Pistol
	Start
	18_15 Pistol not following Player
	Start
	18_16 Not interacting
	Start
  ## Quest System
	19_02 Starting Quest Error
	Start
	19_03 Quests do not progress
	Start
	19_04 Walk Quest not working
	Start
	19_05 Walk Quest finishing too fast
	Start
	19_06 Multiple Quests
	Start
	19_07 Not picking up quests
	Start
	19_08 Get Apples Quest not working
	Start
	19_09 Quests not completing
	Start
	19_10 Cannot start Quests
	Start
	19_11 Cannot start Quests 2
	Start
	19_12 Apples progress not working
	Start
	19_13 Apple Quest not progressing
	Start
	19_14 Starts with one Quest
	Start
	19_15 Fix NullReferenceException starting Quest
	Start
	19_16 Walking Quest doesn't progress
	Start
	19_17 Quests keep completing
	Start
	19_18 Apples Quest not progressing
	Start
  ## Equipment System
	20_02 Equipment Visual Doesn't Hide
	Start
	20_03 Fix NullReferenceException Picking 1, 2
	Start
	20_04 Active Visual Doesn't Change
	Start
	20_05 Cannot Select Pickaxe
	Start
	20_06 UI Not Visible
	Start
	20_07 UI Active not Updating
	Start
	20_08 Bow not Shooting Arrows
	Start
	20_09 Equip not Working
	Start
	20_10 Bow doesn't hide
	Start
	20_11 Bow aiming wrong direction
	Start
	20_12 Pickaxe not Working
	Start
	20_13 UI Wrong
	Start
	20_14 Fix NullReferenceException
	Start
	20_15 Cannot Select Bow
	Start
  ## Building System
	21_02 Cannot Add Resources
	Start
	21_03 Fix FirePit Error
	Start
	21_04 Cannot Add Progress Construction
	Start
	21_05 Cannot Interact FirePit
	Start
	21_06 Second Building Construction Broken
	Start
	21_07 Building House places FirePit
	Start
	21_08 Final FirePit won't burn
	Start
	21_09 Not Picking Items
	Start
	21_10 Cannot Build House
	Start
	21_11 Cannot Add Items to Blueprint
	Start
	21_12 Cannot Construct Building
	Start
	21_13 Blueprint progress bar not working
	Start
	21_14 Fix Error Construction Window
	Start
	21_15 FirePit consumes all Wood
	Start
	21_16 Cannot Build FirePit
	Start
	21_17 UI Shows Buildings need Wood
	Start
	21_18 Fix NaN Error
	Start
  ## Needs System
	22_02 Amount Above Max
	Start
	22_03 UI Showing Hunger 100
	Start
	22_04 Needs Stuck Zero
	Start
	22_05 Apple Refilling Too Much
	Start
	22_06 Inventory Shows Empty
	Start
	22_07 Needs Bars Always Full
	Start
	22_08 Player Doesn't Take Damage Starving
	Start
	22_09 Hunger and Thirst not going down
	Start
	22_10 Fix NullReferenceException on starving
	Start
	22_11 Instantly Starving
	Start
	22_12 Wrong Hunger Thirst
	Start
	22_13 Cannot Consume Anything
	Start
	22_14 Not Consuming Anything
	Start
	22_15 Player dies instantly Starving
	Start
	22_16 Cannot Craft Royale with Cheese
	Start
	22_17 Cannot Eat First Apple
	Start
	22_18 Cannot Consume Items
	Start
  ## Farming System
	23_02 Cannot Interact Land Plot
	Start
	23_03 Land Plot breaks after growing Seeds
	Start
	23_04 Cannot Place Seeds
	Start
	23_05 Instantly Harvestable
	Start
	23_06 Cannot add Water
	Start
	23_07 Growth instant
	Start
	23_08 Cannot Harvest
	Start
	23_09 Cannot Grow Plants
	Start
	23_10 Plants not Growing
	Start
	23_11 Fix Error Select Seeds UI
	Start
	23_12 Harvesting Multiple Times
	Start
	23_13 Cannot Plant Seeds
	Start
	23_14 Cannot Add Water
	Start
	23_15 Plants Grow without Water
	Start
	23_16 Jumps from Seeds to Harvest
	Start
	23_17 Harvest Banana does Nothing
	Start
	23_18 Cannot Interact Land Plot
	Start
	MORE LECTURES SOON....


```cs
// ============================================================
// ADD TO YOUR UTIL.cs - CRITICAL INDIE GAME UTILITIES
// ============================================================

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SPACE_UTIL
{
	// ============================================================
	// 1. TIMER SYSTEM (replace Update() countdown spam)
	// ============================================================
	/// <summary>
	/// Usage: 
	/// Timer.create("reload", 3f, () => canShoot = true);
	/// Timer.pause("reload");
	/// if (Timer.getRemaining("reload") < 1f) ShowWarning();
	/// </summary>
	public static class Timer
	{
		private class TimerData
		{
			public string id;
			public float duration;
			public float elapsed;
			public bool paused;
			public Action onComplete;
			public bool repeat;
		}

		private static Dictionary<string, TimerData> timers = new Dictionary<string, TimerData>();
		private static MonoBehaviour runner; // Set this in INITManager.Awake()

		public static void SetRunner(MonoBehaviour mb) => runner = mb;

		public static void create(string id, float duration, Action onComplete, bool repeat = false)
		{
			if (timers.ContainsKey(id))
				timers.Remove(id);

			timers[id] = new TimerData
			{
				id = id,
				duration = duration,
				elapsed = 0f,
				paused = false,
				onComplete = onComplete,
				repeat = repeat
			};

			runner.StartCoroutine(RunTimer(id));
		}

		private static IEnumerator RunTimer(string id)
		{
			while (timers.ContainsKey(id))
			{
				var timer = timers[id];

				if (!timer.paused)
				{
					timer.elapsed += Time.deltaTime;

					if (timer.elapsed >= timer.duration)
					{
						timer.onComplete?.Invoke();

						if (timer.repeat)
							timer.elapsed = 0f;
						else
						{
							timers.Remove(id);
							yield break;
						}
					}
				}

				yield return null;
			}
		}

		public static void pause(string id)
		{
			if (timers.ContainsKey(id))
				timers[id].paused = true;
		}

		public static void resume(string id)
		{
			if (timers.ContainsKey(id))
				timers[id].paused = false;
		}

		public static void stop(string id)
		{
			if (timers.ContainsKey(id))
				timers.Remove(id);
		}

		public static float getRemaining(string id)
		{
			if (!timers.ContainsKey(id)) return 0f;
			return timers[id].duration - timers[id].elapsed;
		}

		public static bool exists(string id) => timers.ContainsKey(id);
	}

	// ============================================================
	// 2. EASE FUNCTIONS (for tweening/animations)
	// ============================================================
	/// <summary>
	/// Usage:
	/// float t = C.ease(timer / duration, "in_out_quad");
	/// transform.position = Vector3.Lerp(start, end, t);
	/// </summary>
	public static partial class C
	{
		public static float ease(float t, string type = "linear")
		{
			t = Mathf.Clamp01(t);

			switch (type.ToLower())
			{
				case "linear": return t;

				// Quad
				case "in_quad": return t * t;
				case "out_quad": return 1f - (1f - t) * (1f - t);
				case "in_out_quad":
					return t < 0.5f
						? 2f * t * t
						: 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;

				// Cubic
				case "in_cubic": return t * t * t;
				case "out_cubic": return 1f - Mathf.Pow(1f - t, 3f);
				case "in_out_cubic":
					return t < 0.5f
						? 4f * t * t * t
						: 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;

				// Elastic (bounce effect)
				case "out_elastic":
					float c4 = (2f * Mathf.PI) / 3f;
					return t == 0f ? 0f
						: t == 1f ? 1f
						: Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 10f - 0.75f) * c4) + 1f;

				// Back (overshoot)
				case "out_back":
					float c1 = 1.70158f;
					float c3 = c1 + 1f;
					return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);

				default:
					Debug.LogWarning($"Unknown ease type: {type}");
					return t;
			}
		}
	}

	// ============================================================
	// 3. CAMERA SHAKE (essential for game feel)
	// ============================================================
	/// <summary>
	/// Usage:
	/// CameraShake.Shake(intensity: 0.3f, duration: 0.2f);
	/// CameraShake.Trauma(0.5f); // Add trauma for shake
	/// </summary>
	public static class CameraShake
	{
		private static Camera cam;
		private static Vector3 originalPos;
		private static float trauma = 0f;
		private static float traumaDecay = 1.5f;

		public static void Init(Camera camera)
		{
			cam = camera;
			originalPos = cam.transform.localPosition;
		}

		public static void Shake(float intensity, float duration)
		{
			if (cam == null) return;
			cam.GetComponent<MonoBehaviour>()?.StartCoroutine(ShakeCoroutine(intensity, duration));
		}

		private static IEnumerator ShakeCoroutine(float intensity, float duration)
		{
			float elapsed = 0f;

			while (elapsed < duration)
			{
				float x = UnityEngine.Random.Range(-1f, 1f) * intensity;
				float y = UnityEngine.Random.Range(-1f, 1f) * intensity;

				cam.transform.localPosition = originalPos + new Vector3(x, y, 0f);

				elapsed += Time.deltaTime;
				yield return null;
			}

			cam.transform.localPosition = originalPos;
		}

		// Trauma-based shake (more advanced, decays over time)
		public static void Trauma(float amount)
		{
			trauma = Mathf.Clamp01(trauma + amount);
		}

		public static void Update()
		{
			if (cam == null) return;

			if (trauma > 0f)
			{
				trauma = Mathf.Max(0f, trauma - traumaDecay * Time.deltaTime);

				float shake = trauma * trauma; // Square for more dramatic effect
				float x = UnityEngine.Random.Range(-1f, 1f) * shake * 0.5f;
				float y = UnityEngine.Random.Range(-1f, 1f) * shake * 0.5f;
				float z = UnityEngine.Random.Range(-1f, 1f) * shake * 0.2f;

				cam.transform.localPosition = originalPos + new Vector3(x, y, 0f);
				cam.transform.localRotation = Quaternion.Euler(0f, 0f, z * 2f);
			}
			else
			{
				cam.transform.localPosition = originalPos;
				cam.transform.localRotation = Quaternion.identity;
			}
		}
	}

	// ============================================================
	// 4. FRAME-INDEPENDENT SMOOTHING (better than Lerp)
	// ============================================================
	/// <summary>
	/// Usage:
	/// currentValue = C.smooth(currentValue, targetValue, 5f);
	/// // 5f = stiffness (higher = faster convergence)
	/// </summary>
	public static partial class C
	{
		public static float smooth(float current, float target, float stiffness)
		{
			return Mathf.Lerp(current, target, 1f - Mathf.Exp(-stiffness * Time.deltaTime));
		}

		public static Vector3 smooth(Vector3 current, Vector3 target, float stiffness)
		{
			return Vector3.Lerp(current, target, 1f - Mathf.Exp(-stiffness * Time.deltaTime));
		}

		public static Quaternion smooth(Quaternion current, Quaternion target, float stiffness)
		{
			return Quaternion.Slerp(current, target, 1f - Mathf.Exp(-stiffness * Time.deltaTime));
		}
	}

	// ============================================================
	// 5. PARTICLE HELPER (quick VFX spawning)
	// ============================================================
	/// <summary>
	/// Usage:
	/// VFX.Play(VFXType.explosion, position);
	/// VFX.Play(VFXType.hitSpark, position, parent: enemy.transform);
	/// </summary>
	public static class VFX
	{
		private static Dictionary<string, ParticleSystem> cache = new Dictionary<string, ParticleSystem>();

		public static void Play(object vfxType, Vector3 position, Transform parent = null, float scale = 1f)
		{
			string key = vfxType.ToString();

			if (!cache.ContainsKey(key))
			{
				var prefab = R.get<GameObject>(vfxType);
				if (prefab == null)
				{
					Debug.LogError($"VFX prefab not found: {key}".colorTag("red"));
					return;
				}

				var instance = GameObject.Instantiate(prefab);
				instance.name = $"VFX_{key}";
				cache[key] = instance.GetComponent<ParticleSystem>();
			}

			var ps = cache[key];
			ps.transform.position = position;
			ps.transform.localScale = Vector3.one * scale;
			
			if (parent != null)
				ps.transform.SetParent(parent);

			ps.Play();
		}
	}

	// ============================================================
	// 6. COOLDOWN SYSTEM (simpler than Timer for abilities)
	// ============================================================
	/// <summary>
	/// Usage:
	/// if (Cooldown.Ready("dash")) {
	///     Dash();
	///     Cooldown.Start("dash", 2f);
	/// }
	/// float percent = Cooldown.GetPercent("dash"); // for UI bar
	/// </summary>
	public static class Cooldown
	{
		private static Dictionary<string, float> cooldowns = new Dictionary<string, float>();

		public static void Start(string id, float duration)
		{
			cooldowns[id] = Time.time + duration;
		}

		public static bool Ready(string id)
		{
			if (!cooldowns.ContainsKey(id))
				return true;

			return Time.time >= cooldowns[id];
		}

		public static float GetRemaining(string id)
		{
			if (!cooldowns.ContainsKey(id))
				return 0f;

			return Mathf.Max(0f, cooldowns[id] - Time.time);
		}

		public static float GetPercent(string id)
		{
			if (!cooldowns.ContainsKey(id))
				return 1f;

			// We don't store duration, so this returns 0 (on cooldown) or 1 (ready)
			return Ready(id) ? 1f : 0f;
		}
	}
}
```

https://gemini.google.com/share/cc2ad74ccce7