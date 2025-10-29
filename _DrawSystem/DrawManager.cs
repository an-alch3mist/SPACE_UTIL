using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SPACE_UTIL;

namespace SPACE_DrawSystem
{
	/// <summary>
	/// Debug drawing utilities for runtime visualization
	/// </summary>
	public static class DRAW
	{
		// ============================================================
		// PUBLIC FIELDS
		// ============================================================

		public static Transform DrawHolder;
		public static Color Col = Color.red;

		// ============================================================
		// PUBLIC API
		// ============================================================

		public static Dictionary<string, IDraw> MAP_IDraw;

		/// <summary>
		/// Must be called before using DRAW. Creates DebugHolder parent object.
		/// </summary>
		public static void Init()
		{
			if (GameObject.Find("DrawHolder") != null) // root gameObject
				UnityEngine.Object.Destroy(GameObject.Find("DrawHolder"));

			DrawHolder = new GameObject("DrawHolder").transform;

			// not working at the movement
			MAP_IDraw = new Dictionary<string, IDraw>();
		}

		// ============================================================
		// PRIVATE METHODS
		// ============================================================

	}

	public interface IDraw
	{

	}

	/// <summary>
	/// Create a new persistent line
	/// order of calling: 
	/// Line line = new Line(e: ,col: , name: ""); line.a = ; line.b = ;
	/// line.Clear();
	/// </summary>
	public class Line : IDraw
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

		// when called as: this.line.init(name: "hoverLine") .setA(ray.origin).setB(ray.origin + ray.direction * rayDist); // still creating multiple gameObject Instance
		public Line init(string name = "line", Color? color = null, float e = 1f / 50)
		{
			if (DRAW.DrawHolder == null)
			{
				DRAW.Init();
				Debug.Log("DRAW wasnt initizlized, just done now".colorTag("red"));
			}

			this.aRef = Vector3.right * (float)1e6;
			this.bRef = Vector3.right * (float)1e6 - Vector3.right * 0.01f;

			this.lineObj = new GameObject($"{name}");
			this.lineObj.transform.SetParent(DRAW.DrawHolder);

			this.lr = this.lineObj.AddComponent<LineRenderer>();
			this.SetupLineRenderer(e, color ?? DRAW.Col);
			this.UpdatePositions();

			return this;
		}
		

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
		public static Line create(string id = "line", Color? color = null, float e = 1f / 50)
		{
			if (DRAW.DrawHolder == null)
			{
				DRAW.Init();
				Debug.Log("DRAW wasnt initizlized, just done now".colorTag("red"));
			}

			Line line = new Line();

			line.aRef = Vector3.right * (float)1e6;
			line.bRef = Vector3.right * (float)1e6 - Vector3.right * 0.01f;

			line.lineObj = new GameObject($"{id}");
			line.lineObj.transform.SetParent(DRAW.DrawHolder);

			line.lr = line.lineObj.AddComponent<LineRenderer>();
			line.SetupLineRenderer(e, color ?? DRAW.Col);
			line.UpdatePositions();

			return line;
		}
		// << ========================= CHAIN SYNTAX =============================== //

		#region prev constructor approach
		// ============================================================
		// CONSTRUCTOR
		// ============================================================

		#endregion

		// ============================================================
		// PUBLIC METHODS
		// ============================================================

		// clear line vertex positions
		public void Clear()
		{
			this.aRef = Vector3.right * (float)1e6;
			this.bRef = Vector3.right * (float)1e6 - Vector3.right * 0.01f;
			this.UpdatePositions();
		}

		/// <summary>
		/// Destroy the line and clean up resources
		/// </summary>
		public void Destroy()
		{
			if (lineObj != null)
				UnityEngine.Object.Destroy(lineObj);
		}

		// ============================================================
		// PRIVATE METHODS
		// ============================================================

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
	}
}