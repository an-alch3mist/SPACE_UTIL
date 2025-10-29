using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using SPACE_UTIL;
using SPACE_DrawSystem;
using TMPro;

namespace SPACE_UTIL
{
	[DefaultExecutionOrder(-1000)] // first MonoBehaviour script to run, after UnityEngine Initialization
	public class INITManager : MonoBehaviour
	{
		[TextArea(minLines: 3, maxLines: 10)]
		[SerializeField] string README = $@"0. Attach {typeof(INITManager).Name} to Empty Obj
1. Ref Scene MainCamera(for INPUT.M purpose)
2. Ref Scene Canvas(for INPUT.UI purpose)
3. Ref TMFps (for fps status purpose)"; 

		[SerializeField] Camera MainCam;
		[SerializeField] RectTransform CanvasRectTransform;
		[SerializeField] TMPro.TextMeshProUGUI TMFps;
		public static INITManager Ins { get; private set; }

		private void FixedUpdate()
		{
			
		}

		private void Awake()
		{
			Debug.Log(C.method("Awake", this));
			INPUT.Init(
				MainCam: MainCam,
				CanvasRectTransform: this.CanvasRectTransform
			);

			C.Init(); // PrefabHolder Obj Creation
			ITER.reset(); // reset ITER_1D

			INITManager.Ins = this; // instance of InitManager to use MonoBehaviour features
			//GameData.LoadGame(); // 
		}

		private void Update()
		{
			this.UpdateFps();
		}

		int iter = 0;
		private void UpdateFps()
		{
			if (iter % 10 == 0)
				if (this.TMFps != null)
				{
					// fps: 75/120
					string targetFrameRate = (Application.targetFrameRate == -1) ? "max" : Application.targetFrameRate.ToString();
					TMFps.text = $"fps: { C.round(1f / Time.unscaledDeltaTime) }/{ targetFrameRate }";
				}
			iter += 1;
		}

		// check >>
		#region check_gameData /**/
		/*
		[SerializeField] List<Enemy> ENEMY;
		private void Start()
		{
			Dictionary<int, string> MAP = new Dictionary<int, string>()
			{
				[0] = "a",
				[10] = "b",
				[0] = "c",
			};
			LOG.SaveLog(MAP.ToTable());

			//this.gameData = new GameData(new List<Enemy>()
			//{
			//	new Enemy(){ health = 1000, pos = (10, 2), },
			//	new Enemy(){ health = 2000, pos = (11, 1), },
			//	new Enemy(){ health = 1500, pos = (11, 3), },
			//});
			GameData.SaveGame(this.gameData);
		} 
		*/
		#endregion
		// << check

		// [SerializeField] GameData gameData = null;
		private void OnApplicationQuit()
		{
			//GameData.SaveGame(gameData);
		}
	}

	#region check_gameData /**/
	/*
	// serializable
	// upto depth 10
	// only 1D List<>
	[System.Serializable]
	public class Enemy
	{
		public float health;
		public v2 pos;
	}

	[System.Serializable]
	public class GameData
	{
		public static int prev_ver = 0;
		public int ver;
		public List<Enemy> ENEMY;

		public GameData(List<Enemy> ENEMY)
		{
			this.ver = GameData.prev_ver;
			prev_ver += 1;

			this.ENEMY = ENEMY;
		}

		public static void SaveGame(GameData gameData = null)
		{
			if (gameData != null)
				LOG.SaveGame(JsonUtility.ToJson(gameData));
			else
				LOG.SaveGame("~");
		}
		public static void LoadGame()
		{
			GameData gameData = null;
			try
			{
				gameData = JsonUtility.FromJson<GameData>(LOG.LoadGame);
			}
			catch (Exception)
			{
				Debug.Log("Game Data File Incorrect Format, Loading DefaultValues");
				return;
			}

			if (gameData == null)
			{
				// initialize to default values
			}
			else
			{
				// load value into CLASS
				// awake is called for each of them after INITManager.Awake();
				gameData.ENEMY = new List<Enemy>();

			}
		}
	} 
	*/
	#endregion
}
