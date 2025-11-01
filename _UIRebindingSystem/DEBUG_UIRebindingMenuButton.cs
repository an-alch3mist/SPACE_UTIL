using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using SPACE_UTIL;

namespace SPACE_UISystem.Rebinding
{
	public class DEBUG_UIRebindingMenuButton : MonoBehaviour
	{
		[SerializeField] Button _settingOpenCustomRebinding;
		[SerializeField] Button _closeCustomRebinding;

		[SerializeField] GameObject _UIRebindingHolder;

		private void Awake()
		{
			Debug.Log(C.method("Awake", this));
			this._settingOpenCustomRebinding.onClick.AddListener(() =>
			{
				this._UIRebindingHolder.toggle(true);
			});

			this._closeCustomRebinding.onClick.AddListener(() =>
			{
				this._UIRebindingHolder.toggle(false);
			});
		}
	}
}