using System;
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

		/// <summary>
		/// Must be called before using DRAW. Creates DebugHolder parent object.
		/// </summary>
		public static void Init()
		{
			if (GameObject.Find("DrawHolder") != null)
				UnityEngine.Object.Destroy(GameObject.Find("DrawHolder"));

			DrawHolder = new GameObject("DrawHolder").transform;
		}

		// ============================================================
		// PRIVATE METHODS
		// ============================================================

	}


	/// <summary>
	/// Create a new persistent line
	/// order of calling: 
	/// Line line = new Line(e: ,col: , name: ""); line.a = ; line.b = ;
	/// line.Clear();
	/// </summary>
	public class Line
	{
		// ============================================================
		// PRIVATE FIELDS
		// ============================================================

		private GameObject _lineObj;
		private LineRenderer _lr;
		private Vector3 _a;
		private Vector3 _b;

		// ============================================================
		// PUBLIC PROPERTIES
		// ============================================================

		/// <summary>
		/// Start point of the line. Auto-updates on set.
		/// </summary>
		public Vector3 a
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
		public Vector3 b
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
		public Color color
		{
			get { return _lr.startColor; }
			set
			{
				_lr.startColor = value;
				_lr.endColor = value;
			}
		}

		/// <summary>
		/// Thickness of the line
		/// </summary>
		public float e
		{
			get { return _lr.startWidth; }
			set
			{
				_lr.startWidth = value;
				_lr.endWidth = value;
			}
		}

		/// <summary>
		/// Is the line still valid (not destroyed)
		/// </summary>
		public bool Exist
		{
			get { return _lineObj != null && _lr != null; }
		}

		// ============================================================
		// CONSTRUCTOR
		// ============================================================


		/*
		public static void Line(Vector3 a, Vector3 b, Color? color = null, float duration = 0f)
		{
			Debug.DrawLine(a, b, color ?? Color.red, duration);
		}
		*/


		public Line(Vector3 a, Vector3 b, float e = 1f / 50, Color? color = null, string name = "")
		{
			if (DRAW.DrawHolder == null)
			{
				Debug.LogError("DRAW.Init() must be called first");
				return;
			}

			this._a = a;
			this._b = b;

			this._lineObj = new GameObject($"{name}, a: {a}, b: {b}");
			this._lineObj.transform.SetParent(DRAW.DrawHolder);

			this._lr = _lineObj.AddComponent<LineRenderer>();
			this.SetupLineRenderer(e, color ?? DRAW.Col);
			this.UpdatePositions();
		}

		/// <summary>
		/// Create a new persistent line
		/// </summary>
		public Line(float e = 1f / 50, Color? color = null, string name = "")
		{
			if (DRAW.DrawHolder == null)
			{
				Debug.LogError("DRAW.Init() must be called first");
				return;
			}

			this._a = Vector3.right * (float)1e6;
			this._b = Vector3.right * (float)1e6 - Vector3.right * 0.01f;

			this._lineObj = new GameObject($"{name}");
			this._lineObj.transform.SetParent(DRAW.DrawHolder);

			this._lr = _lineObj.AddComponent<LineRenderer>();
			this.SetupLineRenderer(e, color ?? DRAW.Col);
			this.UpdatePositions();
		}

		// ============================================================
		// PUBLIC METHODS
		// ============================================================

		// clear line vertex positions
		public void Clear()
		{
			this._a = Vector3.right * (float)1e6;
			this._b = Vector3.right * (float)1e6 - Vector3.right * 0.01f;
			this.UpdatePositions();
		}

		/// <summary>
		/// Destroy the line and clean up resources
		/// </summary>
		public void Destroy()
		{
			if (_lineObj != null)
				UnityEngine.Object.Destroy(_lineObj);
		}

		// ============================================================
		// PRIVATE METHODS
		// ============================================================

		/// <summary>
		/// Configure LineRenderer with material, color, and thickness
		/// </summary>
		private void SetupLineRenderer(float thickness, Color color)
		{
			_lr.startWidth = thickness;
			_lr.endWidth = thickness;
			_lr.material = new Material(Shader.Find("Sprites/Default"));
			_lr.startColor = color;
			_lr.endColor = color;
		}

		/// <summary>
		/// Apply current a/b values to LineRenderer
		/// </summary>
		private void UpdatePositions()
		{
			if (_lr != null)
			{
				_lr.SetPosition(0, _a);
				_lr.SetPosition(1, _b);
			}
		}
	}
}