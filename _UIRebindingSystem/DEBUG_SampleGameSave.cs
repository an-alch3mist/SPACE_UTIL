using UnityEngine;
using System.Collections;
using SPACE_UTIL;

// SPACE_UISystem.Rebinding => generalized for RebindingSystem
// SPACE_UISystem.Rebinding.Game => a sample showcase, in future game should have its own architecture kinda similar to SPACE_UISystem.Rebinding.Game Demo

namespace SPACE_UISystem.Rebinding.Game
{
	public class DEBUG_SampleGameSave : MonoBehaviour
	{
		private void OnEnable()
		{
			//Debug.Log(C.method("OnEnable", this));
			Debug.Log(C.method(this));
			// C.SysInfo.method();
			StopAllCoroutines();
			StartCoroutine(STIMULATE());
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		IEnumerator STIMULATE()
		{
			yield return null;
			LOG.AddLog(GameStore.playerStats.ToJson(pretify: false), "json"); // just to log
			LOG.AddLog(GameStore.playerStats.board.ToString(), "txt"); // just to log
			//

			const float sceneDuration = 3f;
			int coinsCollected = 0; // shall append or override the GameStore later
			float sceneTime = 0;	// shall append or override the GameStore later

			// scene >>
			for (float time = 0f; time < sceneDuration; time += Time.deltaTime)
			{
				if (Random.Range(0, 100) < 10)
					coinsCollected += 1;
				//
				sceneTime += Time.deltaTime;
			}

			// perform the save >>
			GameStore.playerStats.totalCoins += coinsCollected;
			GameStore.playerStats.gameTimes.Add(sceneTime);
			GameStore.playerStats.saveGameData();
			// LOG.SaveGameData(GameDataType.playerStats, GameStore.playerStats.ToJson());
			// << perform the save

			// << scene
		}

		// save before quit
		private void OnApplicationQuit()
		{
			// Debug.Log(C.method("OnApplicationQuit", this, "orange"));
			Debug.Log(C.method(this, color: "orange"));
		}

	} 
}
