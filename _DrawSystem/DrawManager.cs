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

	// built to work when compile and re-run is made without the use of Awake() from any method 
	public class Line
	{
		#region private API
		// ============================================================
		// FIELDS
		// ============================================================
		private GameObject lineObj;
		private LineRenderer lr;
		private Vector3 _a;
		private Vector3 _b;

		// ============================================================
		// PROPERTIES
		// ============================================================

		/// <summary>
		/// Start point of the line. Auto-updates on set.
		/// </summary>
		private Vector3 a
		{
			get { return _a; }
			set
			{
				_a = value;
				UpdatePositions();
			}
		}
		/// <summary>
		/// End point of the line. Auto-updates on set.
		/// </summary>
		private Vector3 b
		{
			get { return _b; }
			set
			{
				_b = value;
				UpdatePositions();
			}
		}
		/// <summary>
		/// Color of the line
		/// </summary>
		private Color color
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
		private float e
		{
			get { return lr.startWidth; }
			set
			{
				lr.startWidth = value;
				lr.endWidth = value;
			}
		}
		private string id { get; set; }
		private Line init(string name = "line", Color? color = null, double e = 1f / 50)
		{
			this.id = name;
			if (this.lineObj == null)  // occurs first call of a certain line
			{
				this._a = Vector3.right * (float)1e6;
				this._b = Vector3.right * (float)1e6 - Vector3.right * (float)1e-3;

				this.lineObj = new GameObject($"{name}");
				this.lineObj.transform.SetParent(DRAW.DrawHolder);

				this.lr = this.lineObj.AddComponent<LineRenderer>();
				this.SetupLineRenderer((float)e, color ?? Color.red);
				this.UpdatePositions();
			}
			return this;
		}

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
				lr.SetPosition(0, _a);
				lr.SetPosition(1, _b);
			}
		}
		#endregion

		public static Dictionary<string, Line> MAP_IdLine;
		// Line.create(id).setA().setB().setCol().setE();
		// ========================= CHAIN SYNTAX =============================== >> //
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
			if (DRAW.DrawHolder == null)
			{
				var existing = GameObject.Find("DrawHolder");
				GameObject.Destroy(existing);
				DRAW.DrawHolder = new GameObject("DrawHolder").transform;
				Debug.Log(C.method(null, "init draw holder made"));
			}
			/*
			if (DRAW.DrawHolder == null) // occurs first line
			{
				if (GameObject.Find("DrawHolder") != null)
					GameObject.Find("DrawHolder").destroy();
				DRAW.DrawHolder = new GameObject("DrawHolder").transform;
				Debug.Log("initialized DRAWHolder now".colorTag("lime"));
			}
			*/

			string idStr = id.ToString();
			if (Line.MAP_IdLine.ContainsKey(idStr) == true)
				return Line.MAP_IdLine[idStr];
			else
			{
				Debug.Log(C.method(null, "lime", $"created a new line and linked to MAP_IdLine"));
				Line line = new Line().init(name: idStr, color, e);  // DRAW.DrawHolder is handled inside init()
				Line.MAP_IdLine[idStr] = line;
				return line;
			}
		}
		public Line setA(Vector3 val)
		{
			this._a = val;
			this.UpdatePositions();
			return this;
		}
		public Line setB(Vector3 val)
		{
			this._b = val;
			this.UpdatePositions();
			return this;
		}
		public Line setN(Vector3 val)
		{
			this._b = this.a + val;
			this.UpdatePositions();
			return this;
		}
		public Line setCol(Color val)
		{
			lr.startColor = val;
			lr.endColor = val;
			return this;
		}
		public Line setE(double val = 1f / 50)
		{
			lr.startWidth = (float)val;
			lr.endWidth = (float)val;
			return this;
		}
		public Line getId(out string varName)
		{
			varName = this.id;
			return this;
		}
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
		public void destroy()
		{
			UnityEngine.Object.Destroy(lineObj);
			if (Line.MAP_IdLine.ContainsKey(this.id) == true)
			{
				Line.MAP_IdLine.Remove(this.id);
				Debug.Log(C.method(null, "lime", $"success destroying line of id: {this.id} from MAP_IdLine"));
			}
			else
				Debug.Log(C.method(null, "red", $"error destroying line of id: {this.id} in MAP_IdLine , doesn;t exist"));
		}
	}

	/// <summary>
	/// Cube.create("marker").setPos(pos).setCol(Color.green).setSize(0.5f);
	/// Works in Built-in, URP, and HDRP pipelines.
	/// </summary>
	public class Cube
	{
		#region private API
		// ============================================================
		// FIELDS
		// ============================================================
		private GameObject cubeObj;
		private MeshRenderer mr;
		private Material mat;
		private Vector3 _pos;
		private Color _color;
		private float _size;

		// ============================================================
		// PROPERTIES
		// ============================================================
		private Vector3 pos
		{
			get { return _pos; }
			set
			{
				_pos = value;
				cubeObj.transform.position = value;
			}
		}

		private Color color
		{
			get { return _color; }
			set
			{
				_color = value;
				if (mat != null)
					mat.color = value;
			}
		}

		private float size
		{
			get { return _size; }
			set
			{
				_size = value;
				cubeObj.transform.localScale = Vector3.one * value;
			}
		}

		private string id { get; set; }

		private Cube init(string name, Color? color, float size)
		{
			this.id = name;
			if (this.cubeObj == null)
			{
				// Create primitive cube
				this.cubeObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
				this.cubeObj.name = name;
				this.cubeObj.transform.SetParent(DRAW.DrawHolder);

				// Remove collider (we're just visualizing)
				UnityEngine.Object.Destroy(this.cubeObj.GetComponent<Collider>());

				// Setup rendering
				this.mr = this.cubeObj.GetComponent<MeshRenderer>();
				this.SetupMaterial(color ?? Color.white);

				// Initialize properties
				this._pos = Vector3.zero;
				this._size = size;
				this._color = color ?? Color.white;

				this.cubeObj.transform.position = _pos;
				this.cubeObj.transform.localScale = Vector3.one * _size;
			}
			return this;
		}

		/// <summary>
		/// Creates material compatible with Built-in, URP, and HDRP
		/// </summary>
		private void SetupMaterial(Color color)
		{
			// Try to find the appropriate shader based on pipeline
			Shader shader = null;

			// Try URP/HDRP Lit shader first
			shader = Shader.Find("Universal Render Pipeline/Lit");
			if (shader == null)
				shader = Shader.Find("HDRP/Lit");

			// Fallback to Built-in
			if (shader == null)
				shader = Shader.Find("Standard");

			// Last resort fallback
			if (shader == null)
			{
				Debug.LogWarning("[Cube] No lit shader found, using Unlit/Color");
				shader = Shader.Find("Unlit/Color");
			}

			this.mat = new Material(shader);
			this.mat.color = color;

			// Make it unlit-looking for visualization (optional, remove if you want lighting)
			if (shader.name.Contains("Standard"))
			{
				// Built-in: disable specular, increase emission
				this.mat.SetFloat("_Glossiness", 0f);
				this.mat.SetFloat("_Metallic", 0f);
				this.mat.EnableKeyword("_EMISSION");
				this.mat.SetColor("_EmissionColor", color * 0.3f);
			}
			else if (shader.name.Contains("Universal") || shader.name.Contains("HDRP"))
			{
				// URP/HDRP: similar settings
				this.mat.SetFloat("_Smoothness", 0f);
				this.mat.SetFloat("_Metallic", 0f);
			}

			mr.material = this.mat;
		}

		#endregion

		public static Dictionary<string, Cube> MAP_IdCube;

		// ========================= CHAIN SYNTAX =============================== >> //
		/// <summary>
		/// Try to get Cube of given id, if doesn't exist yet, create new one and return.
		/// </summary>
		/// <param name="id">unique cube id</param>
		/// <param name="color">cube color, could be set later via .setCol(Color)</param>
		/// <param name="size">size of cube (default 0.1f), could be set later via .setSize(float)</param>
		/// <returns></returns>
		public static Cube create(object id, Color? color = null, float size = 0.1f)
		{
			if (Cube.MAP_IdCube == null)
			{
				Debug.Log(C.method(null, "lime", "init MAP_IdCube<>"));
				Cube.MAP_IdCube = new Dictionary<string, Cube>();
			}

			if (DRAW.DrawHolder == null)
			{
				var existing = GameObject.Find("DrawHolder");
				if (existing != null)
					GameObject.Destroy(existing);
				DRAW.DrawHolder = new GameObject("DrawHolder").transform;
				Debug.Log(C.method(null, "lime", "init draw holder made"));
			}

			string idStr = id.ToString();
			if (Cube.MAP_IdCube.ContainsKey(idStr))
				return Cube.MAP_IdCube[idStr];
			else
			{
				Debug.Log(C.method(null, "lime", $"created a new cube and linked to MAP_IdCube"));
				Cube cube = new Cube().init(name: idStr, color, size);
				Cube.MAP_IdCube[idStr] = cube;
				return cube;
			}
		}

		public Cube setPos(Vector3 val)
		{
			this._pos = val;
			this.cubeObj.transform.position = val;
			return this;
		}
		public Cube setCol(Color val)
		{
			this._color = val;
			if (this.mat != null)
				this.mat.color = val;

			// Update emission if it exists
			if (this.mat.HasProperty("_EmissionColor"))
				this.mat.SetColor("_EmissionColor", val * 0.3f);

			return this;
		}
		public Cube setSize(double valDouble)
		{
			float val = (float)valDouble;
			this._size = val;
			this.cubeObj.transform.localScale = Vector3.one * val;
			return this;
		}
		public Cube getId(out string varName)
		{
			varName = this.id;
			return this;
		}
		public Cube toggle(bool value = true)
		{
			this.cubeObj.SetActive(value);
			return this;
		}
		public void destroy()
		{
			UnityEngine.Object.Destroy(cubeObj);
			if (Cube.MAP_IdCube.ContainsKey(this.id))
			{
				Cube.MAP_IdCube.Remove(this.id);
				Debug.Log(C.method(null, "lime", $"success destroying cube of id: {this.id} from MAP_IdCube"));
			}
			else
				Debug.Log(C.method(null, "red", $"error destroying cube of id: {this.id} in MAP_IdCube, doesn't exist"));
		}
	}
}