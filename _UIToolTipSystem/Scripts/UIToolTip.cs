using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using TMPro;
using SPACE_UTIL;

namespace SPACE_UISystem
{
	/*
		Used As: 
		using SPACE_UISystem;
			UIToolTip.Ins.Show(bool);
			UIToolTip.Ins.SetText(string);
			UIToolTip.Ins.SetPos(vec2 UICoord UI MousePos is used as practice );
	*/
	public class UIToolTip : MonoBehaviour
	{
		[Header("README")]
		[TextArea(minLines: 5, maxLines: 10)]
		[SerializeField] string README = @"Used As: 
using SPACE_UISystem;
	Call UIToolTip.Ins.Show(bool);
	Call UIToolTip.Ins.SetText(string);
	Call UIToolTip.Ins.SetPos(vec2 UICoord UI MousePos is used as practice );";

		[SerializeField] bool show_by_default = false;
		[SerializeField] RectTransform BoundRect;
		public static UIToolTip Ins;

		TextMeshProUGUI tm;
		RectTransform ToolTipRect;
		int border;
		private void Awake()
		{
			this.ToolTipRect = this.gameObject.GetComponent<RectTransform>();
			this.tm = this.gameObject.leafNameStartsWith("back").leafNameStartsWith("text").GetComponent<TextMeshProUGUI>();
			UIToolTip.Ins = this;

			// optional outline border >>
			if (this.gameObject.leafNameStartsWith("back").GetComponent<Outline>() != null)
				this.border = C.ceil(this.gameObject.leafNameStartsWith("back").GetComponent<Outline>().effectDistance.x);
			else
				this.border = 0;

			this.Show(this.show_by_default);
			// << optional outline border
			Debug.Log("Awake(): " + this);
		}

		// unity editor mode
		private void OnValidate()
		{
			this.Show(this.show_by_default);
		}

		/* >> check
		<< check */

		[SerializeField] string check_str;
		private void Update()
		{
			/*
			if (INPUT.M.InstantDown(0))
				SetText(this.check_str + Random.Range(0, (int)1e8));
			*/
			SetPos(INPUT.UI.pos);
		}

		public void SetText(string str)
		{
			this.tm.text = str;
			this.tm.ForceMeshUpdate();
		}

		public void Show(bool Active = true)
		{
			this.gameObject.leafNameStartsWith("back").gameObject.SetActive(Active);
		}

		public void SetPos(Vector2 pos) // its INPUT.UI.pos
		{
			Vector2 target_pos = pos;
			if (this.BoundRect != null) // if there is bound specified by main_canvas/a_panel
			{
				// clamp >>
				Rect PanelBounds = INPUT.UI.getBounds(this.BoundRect);
				Rect ToolTipBounds = INPUT.UI.getBounds(this.ToolTipRect);
				int border = this.border;

				// for origin/anchor at center-bottom
				target_pos.x = C.clamp(target_pos.x, PanelBounds.min.x + (border + ToolTipBounds.width / 2), PanelBounds.max.x - (border + ToolTipBounds.width / 2));
				target_pos.y = C.clamp(target_pos.y, PanelBounds.min.y + (border + 0), PanelBounds.max.y - (border + ToolTipBounds.height));
				// << clamp			
			}
			this.ToolTipRect.anchoredPosition = target_pos;
		}
	}
}