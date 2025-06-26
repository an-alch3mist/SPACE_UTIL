using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SPACE_UTIL;
using TMPro;

namespace SPACE_GRAPH_VIEW
{
	namespace SPACE_DirGraphNode
	{
		public class GraphNode_IO : MonoBehaviour
		{
			public int id;
			public string name;
			public List<GraphNode_IO> INP, OUT;

			public void Init(int id, string name)
			{
				this.INP = new List<GraphNode_IO>();
				this.OUT = new List<GraphNode_IO>();
				
				// id from node, not from a_inc
				this.id = id;

				this.name = name;
				// text
				this.SetText(name);
				this.gameObject.name = name + " : GraphNode";
			}

			void SetText(string str)
			{
				TextMeshPro tm = U.NameStartsWith(this.gameObject, "text").GetComponent<TextMeshPro>();
				tm.text = str;
				tm.ForceMeshUpdate();

				float pad = 0.2f;
				Vector2 textSize = tm.GetRenderedValues(onlyVisibleCharacters: false); // including new line, space
				Transform back_Transform = this.transform.NameStartsWith("back");
				back_Transform.localScale = (Vector3)textSize + new Vector3(pad, pad, 1);
			}

			/*
			How To Get Size of TMProUI.text : 
				static void SetText(string str)
				{
					text_TM.text = str;

					text_TM.ForceMeshUpdate();
					Vector2 textSize = text_TM.GetRenderedValues(onlyVisibleCharacters: false); // size with entire text
					back_RectTransform.sizeDelta = textSize + new Vector2(pad, pad); // size with respect to anchor
				}
			*/


			// DRAW.dt is set before calling node.draw();
			public void draw(float s = 1f / 10, float e = 1f / 80)
			{
				//Debug.Log($"drawn: {this.gameObject.name}");
				// this.node to node.OUT[i0].pos
				DRAW.col = Color.green;
				foreach (GraphNode_IO node in OUT)
				{
					Vector3 a = this.transform.position,
							b = node.transform.position;
					DRAW.ARROW(Z.lerp(a, b, 0.3f), Z.lerp(a, b, 0.7f), t: 0.7f, s: s, e: e);
				}
			}
		}
	}
}
