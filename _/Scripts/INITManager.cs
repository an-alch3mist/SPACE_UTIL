using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using SPACE_UTIL;

namespace SPACE_UTIL
{
	[DefaultExecutionOrder(-1000)] // first MonoBehaviour script to run, after UnityEngine Initialization
	public class INITManager : MonoBehaviour
	{
		[TextArea(minLines: 3, maxLines: 10)]
		[SerializeField] string README = $@"0. Attach {typeof(INITManager).Name} to Empty Obj
1. Ref Scene MainCamera(for INPUT.M purpose)
2. Ref Scene Canvas(for INPUT.UI purpose)";

		[SerializeField] Camera MainCam;
		[SerializeField] RectTransform CanvasRectTransform;
		public static INITManager Ins { get; private set; }

		private void Awake()
		{
			Debug.Log("Awake(): " + this);
			INPUT.Init(
				MainCam: MainCam,
				CanvasRectTransform: this.CanvasRectTransform
			);
			C.Init(); // PrefabHolder Obj Creation
			LOG.Init(); // Directory, .txt File SetUp
			INITManager.Ins = this; // instance of InitManager to use MonoBehaviour features
			//GameData.LoadGame(); // 
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
