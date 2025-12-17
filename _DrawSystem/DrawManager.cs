using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SPACE_UTIL;

namespace SPACE_DrawSystem
{
	public static class DRAW
	{
		public static Transform DrawHolder; // make sure it exist and created at runtime
	}

	/// <summary>
	/// Line.create("myLine").setA(a).setB(b); // ← Cleaner for persistent lines
	/// </summary>
	/// alt approach: Line line = new Line().init("myLine").setA(a).setB(b);

	// built to work when compile and re-run is made without the use of Awake() from any method 
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
		public Line setN(Vector3 val)
		{
			this.bRef = this.a + val;
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

		public string id { get; private set; }

		// when called as Line.create(id) .setA(ray.origin) .setN(ray.SetN);
		public static Dictionary<string, Line> MAP_IdLine;
		/// <summary>
		/// try to get Line of given id, if doesn;t exist yet? create new one and return.
		/// </summary>
		/// <param name="id">unique line id</param>
		/// <param name="color">line color, could be set later via .setCol(Color)</param>
		/// <param name="e">thickness of line, could be set later via .setE(float)</param>
		/// <returns></returns>
		public static Line create(object id, Color? color = null, double e = 1f / 50)
		{
			if (Line.MAP_IdLine == null)
			{
				Debug.Log(C.method(null, "lime", "init MAP_IdLine<>"));
				Line.MAP_IdLine = new Dictionary<string, Line>();
			}
			if (DRAW.DrawHolder == null) // occurs first line
			{
				if (GameObject.Find("DrawHolder") != null)
					GameObject.Find("DrawHolder").destroy();
				DRAW.DrawHolder = new GameObject("DrawHolder").transform;
				Debug.Log("initialized DRAWHolder now".colorTag("lime"));
			}

			string idStr = id.ToString();
			if(Line.MAP_IdLine.ContainsKey(idStr) == true)
				return Line.MAP_IdLine[idStr];
			else
			{
				Debug.Log(C.method(null, "lime", $"created a new line and linked to MAP_IdLine"));
				Line line = new Line().init(name: idStr, color, e);  // DRAW.DrawHolder is handled inside init()
				Line.MAP_IdLine[idStr] = line;
				return line;
			}
		}

		// (private for now) when called as: this.line.init(name: "hoverLine") .setA(ray.origin).setB(ray.origin + ray.direction * rayDist); // still creating multiple gameObject Instance
		// private constructor, to make Line.create(id) as standard
		private Line init(string name = "line", Color? color = null, double e = 1f / 50)
		{
			this.id = name;
			if (this.lineObj == null)  // occurs first call of a certain line
			{
				this.aRef = Vector3.right * (float)1e6;
				this.bRef = Vector3.right * (float)1e6 - Vector3.right * (float)1e-3;

				this.lineObj = new GameObject($"{name}");
				this.lineObj.transform.SetParent(DRAW.DrawHolder);

				this.lr = this.lineObj.AddComponent<LineRenderer>();
				this.SetupLineRenderer((float)e, color ?? Color.red);
				this.UpdatePositions();
			}
			return this;
		}

		// << ========================= CHAIN SYNTAX =============================== //
		// clear line vertex positionsm to somewhere far
		public Line toggle(bool value = false)
		{
			this.lr.enabled = value;
			/*
			this.aRef = Vector3.right * (float)1e6;
			this.bRef = Vector3.right * (float)1e6 - Vector3.right * 0.01f;
			this.UpdatePositions();
			*/		
			return this;
		}
		/// <summary>
		/// Destroy the line and clean up resources
		/// </summary>
		public void destroy()
		{
			UnityEngine.Object.Destroy(lineObj);
			if(Line.MAP_IdLine.ContainsKey(this.id) == true)
			{
				Line.MAP_IdLine.Remove(this.id);
				Debug.Log(C.method(null, "lime", $"success destroying line of id: {this.id} from MAP_IdLine"));
			}
			else
				Debug.Log(C.method(null, "red", $"error destroying line of id: {this.id} in MAP_IdLine , doesn;t exist"));
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