using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using SPACE_UTIL;
using SPACE_DrawSystem;

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

		private void Awake()
		{
			Debug.Log(C.method(this, "white"));

			INPUT.Init(
				MainCam: MainCam,
				CanvasRectTransform: this.CanvasRectTransform
			);

			C.Init(); // PrefabHolder Obj Creation
			ITER.reset(); // reset ITER_1D

			INITManager.Ins = this; // instance of InitManager to use MonoBehaviour features // not required
			//GameData.LoadGame(); // not required
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

		private void OnApplicationQuit()
		{
			//GameData.SaveGame(gameData);
		}
	}
}
