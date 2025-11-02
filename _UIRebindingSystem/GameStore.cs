using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

using SPACE_UTIL;

namespace SPACE_UISystem.Rebinding.Game
{
	[DefaultExecutionOrder(-50)] // Awake() should occur, at the very begining just after InputSystem
	public class GameStore: MonoBehaviour
	{
		[SerializeField] InputActionAsset _inputActionAsset;
		public static InputActionAsset IA;
		public static PlayerStats playerStats;

		private void Awake()
		{
			Debug.Log(C.method(this));
			this.LoadAll();
		}

		private void LoadAll()
		{
			_inputActionAsset.tryLoadBindingOverridesFromJson(LOG.LoadGameData(GameDataType.inputKeyBindings));
			GameStore.IA = _inputActionAsset;
			// in future: GameStore.A = LOG.LoadGameData<A>(GameDataType.A); // try is inbuilt inside string LoadGameData<T>("")
			GameStore.playerStats = LOG.LoadGameData<PlayerStats>(GameDataType.playerStats);
		}
	}

	/// <summary>
	/// Game data types - add new entries here for each saveable data type
	/// Each enum value maps to a JSON file: LOG/GameData/{enumName}.json
	/// </summary>
	public enum GameDataType
	{
		inputKeyBindings,
		playerStats,
	}

	[System.Serializable]
	public class PlayerStats
	{
		public string name = "somthng";
		public List<float> gameTimes = new List<float>();
		public int totalCoins = 0;
		public Dictionary<GameDataType, int> MAPCheck = new Dictionary<GameDataType, int>()
		{
			[GameDataType.playerStats] = 10,
			[GameDataType.playerStats] = 11,
			[GameDataType.inputKeyBindings] = 100,
		};
		public Board<char> board = new Board<char>((10, 10), '.');
		//
		public void saveGameData()
		{
			LOG.SaveGameData(GameDataType.playerStats, this.ToJson());
		}
	}
}