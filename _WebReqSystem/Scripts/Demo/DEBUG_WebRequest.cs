using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SPACE_UTIL;
using UnityEngine.UI;
using TMPro;

namespace SPACE_WebReqSystem
{
	public class DEBUG_WebRequest : MonoBehaviour
	{
		[TextArea(minLines: 5, maxLines: 20)]
		[SerializeField] string Feedback_str = $"### Well\n_That_ `Seem` **Interesting.** ||spoiler||";

		[SerializeField] GameObject feedBackPanel;

		// depends on WebReqManager Awake
		private void Start()
		{
			// submit button action
			this.feedBackPanel.leafNameStartsWith("submit").GetComponent<Button>() // todo path submit > text > 
				.onClick.AddListener(() => 
				{
					WebReqManager.Discord.SendPayLoadJson_Feedback(
					this.feedBackPanel.leafNameStartsWith("inp").GetComponent<TMP_InputField>().text);
				});

			// x button action
			this.feedBackPanel.leafNameStartsWith("x").GetComponent<Button>()
				.onClick.AddListener(() => this.feedBackPanel.SetActive(false));
		}

		private void Update()
		{
			if (INPUT.M.InstantDown(2))
				// WebReqSystemManager.Discord.SendPayLoadJson_SysSpec();
				WebReqManager.Discord.SendPayLoadJson_Feedback(this.Feedback_str);
		}
	}
}
