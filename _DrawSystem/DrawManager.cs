using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SPACE_UTIL;

namespace SPACE_DrawSystem
{
	
	/// <summary>
	/// Create a new persistent line
	/// Line.create(id: "somthng").setA(a).setB(b).setCol(color);
	/// Line line = new Line(); line.init(name: "somthng").setA(a).setB(b).setCol(color);
	/// </summary>
	public class Line
	{
		// ============================================================
		// PRIVATE FIELDS
		// ============================================================

		private GameObject lineObj;
		private LineRenderer lr;
		private Vector3 aRef;
		private Vector3 bRef;

		// ============================================================
		// PUBLIC PROPERTIES
		// ============================================================

		/// <summary>
		/// Start point of the line. Auto-updates on set.
		/// </summary>
		public Vector3 a
		{
			get { return aRef; }
			set
			{
				aRef = value;
				UpdatePositions();
			}
		}

		/// <summary>
		/// End point of the line. Auto-updates on set.
		/// </summary>
		public Vector3 b
		{
			get { return bRef; }
			set
			{
				bRef = value;
				UpdatePositions();
			}
		}

		/// <summary>
		/// Color of the line
		/// </summary>
		public Color color
		{
			get { return lr.startColor; }
			set
			{
				lr.startColor = value;
				lr.endColor = value;
			}
		}

		/// <summary>
		/// Thickness of the line
		/// </summary>
		public float e
		{
			get { return lr.startWidth; }
			set
			{
				lr.startWidth = value;
				lr.endWidth = value;
			}
		}


		// ========================= CHAIN SYNTAX =============================== >> //
		/*
		Line line = new Line();
		line.init().setA().setB().setCol().setE();

		Line.create(id).setA().setB().setCol().setE();
		*/
		public Line setA(Vector3 val)
		{
			this.aRef = val;
			this.UpdatePositions();
			return this;
		}
		public Line setB(Vector3 val)
		{
			this.bRef = val;
			this.UpdatePositions();
			return this;
		}
		public Line setCol(Color val)
		{
			lr.startColor = val;
			lr.endColor = val;
			return this;
		}
		public Line setE(float val = 1f / 50)
		{
			lr.startWidth = val;
			lr.endWidth = val;
			return this;
		}

		// create's multiple line obj
		static Transform DrawHolder;
		static Dictionary<string, Line> MAP_LINE;
		public static Line create(string id = "line", Color? color = null, float e = 1f / 50)
		{
			if (DrawHolder == null)
			{
				if (GameObject.Find("DrawHolder") != null)
					GameObject.Find("DrawHolder").destroy();
				DrawHolder = new GameObject("DrawHolder").transform;
				Debug.Log("initialized DRAWHolder now".colorTag("lime"));
			}
			if(MAP_LINE == null)
			{
				MAP_LINE = new Dictionary<string, Line>();
			}
			if(MAP_LINE.ContainsKey(id) == false)
			{
				Line line = new Line();

				line.aRef = Vector3.right * (float)1e6;
				line.bRef = Vector3.right * (float)1e6 - Vector3.right * 0.01f;

				line.lineObj = new GameObject($"{id}");
				line.lineObj.transform.SetParent(DrawHolder);

				line.lr = line.lineObj.AddComponent<LineRenderer>();
				line.SetupLineRenderer(e, color ?? Color.red);
				line.UpdatePositions();
				MAP_LINE[id] = line;

				return line;
			}
			return MAP_LINE[id];
		}
		// << ========================= CHAIN SYNTAX =============================== //

		// ============================================================
		// PUBLIC METHODS
		// ============================================================

		// when called as: this.line.init(name: "hoverLine") .setA(ray.origin).setB(ray.origin + ray.direction * rayDist); // still creating multiple gameObject Instance
		public Line init(string name = "line", Color? color = null, float e = 1f / 50)
		{
			if (DrawHolder == null) // occurs first line
			{
				if (GameObject.Find("DrawHolder") != null)
					GameObject.Find("DrawHolder").destroy();
				DrawHolder = new GameObject("DrawHolder").transform;
				Debug.Log("initialized DRAWHolder now".colorTag("lime"));
			}
			if (this.lineObj == null)  // occurs first call of a certain line
			{
				this.aRef = Vector3.right * (float)1e6;
				this.bRef = Vector3.right * (float)1e6 - Vector3.right * 0.01f;

				this.lineObj = new GameObject($"{name}");
				this.lineObj.transform.SetParent(DrawHolder);

				this.lr = this.lineObj.AddComponent<LineRenderer>();
				this.SetupLineRenderer(e, color ?? Color.red);
				this.UpdatePositions();
			}
			return this;
		}
		// clear line vertex positionsm to somewhere far
		public Line clear()
		{
			this.aRef = Vector3.right * (float)1e6;
			this.bRef = Vector3.right * (float)1e6 - Vector3.right * 0.01f;
			this.UpdatePositions();
			return this;
		}
		/// <summary>
		/// Destroy the line and clean up resources
		/// </summary>
		public void destroy()
		{
			if (lineObj != null)
				UnityEngine.Object.Destroy(lineObj);
		}


		// ============================================================
		// PRIVATE METHODS
		// ============================================================
		#region private API
		/// <summary>
		/// Configure LineRenderer with material, color, and thickness
		/// </summary>
		private void SetupLineRenderer(float thickness, Color color)
		{
			lr.startWidth = thickness;
			lr.endWidth = thickness;
			lr.material = new Material(Shader.Find("Sprites/Default"));
			lr.startColor = color;
			lr.endColor = color;
		}

		/// <summary>
		/// Apply current a/b values to LineRenderer
		/// </summary>
		private void UpdatePositions()
		{
			if (lr != null)
			{
				lr.SetPosition(0, aRef);
				lr.SetPosition(1, bRef);
			}
		} 
		#endregion
	}
}