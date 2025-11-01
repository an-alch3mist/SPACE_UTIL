using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

using SPACE_UTIL;

namespace SPACE_UISystem.Rebinding.Game
{
	public class GameStore: MonoBehaviour
	{
		public static InputActionAsset IA;
		[SerializeField] InputActionAsset _inputActionAsset;

		private void Awake()
		{
			Debug.Log(C.method("Awake", this));
			GameStore.IA = _inputActionAsset;
			this.LoadAll();
		}

		private void LoadAll()
		{
			_inputActionAsset.tryLoadBindingOverridesFromJson(LOG.LoadGameData(GameDataType.inputKeyBindings));
			// in future: GameStore.A = LOG.LoadGameData<A>(GameDataType.A); // try is inbuilt inside string LoadGameData<T>("")
		}
	}

	/// <summary>
	/// Game data types - add new entries here for each saveable data type
	/// Each enum value maps to a JSON file: LOG/GameData/{enumName}.json
	/// </summary>
	public enum GameDataType
	{
		inputKeyBindings,
	}
}