using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Text.RegularExpressions;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

using System.Threading.Tasks;

namespace SPACE_UTIL
{
	#region v2
	// v2 a = new v2(0, 0)
	// v2 b = (1, 2)
	[System.Serializable]
	public struct v2
	{
		public int x, y;

		public v2(int x, int y) { this.x = x; this.y = y; }
		public override string ToString()
		{
			return $"({x}, {y})";
			//return base.ToString();
		}

		public static v2 operator +(v2 a, v2 b) { return new v2(a.x + b.x, a.y + b.y); }
		public static v2 operator -(v2 a, v2 b) { return new v2(a.x - b.x, a.y - b.y); }
		public static v2 operator *(v2 a, v2 b) { return new v2(a.x * b.x, a.y * b.y); }
		public static v2 operator *(v2 v, int m) { return new v2(v.x * m, v.y * m); }
		public static v2 operator *(int m, v2 v) { return new v2(v.x * m, v.y * m); }
		public static float dot(v2 a, v2 b) { return a.x * b.x + a.y * b.y; }
		public static float area(v2 a, v2 b) { return a.x * b.y - a.y * b.x; }

		#region compare
		public static bool operator ==(v2 a, v2 b) { return a.x == b.x && a.y == b.y; }
		public static bool operator !=(v2 a, v2 b) { return a.x != b.x || a.y != b.y; }

		public static bool operator >(v2 a, v2 b) => a.x > b.x && a.y > b.y;
		public static bool operator <(v2 a, v2 b) => a.x < b.x && a.y < b.y;
		public static bool operator >=(v2 a, v2 b) => a.x >= b.x && a.y >= b.y;
		public static bool operator <=(v2 a, v2 b) => a.x <= b.x && a.y <= b.y;
		#endregion

		// Allow implicit conversion from tuple
		public static implicit operator v2((int, int) tuple) => new v2(tuple.Item1, tuple.Item2);

		#region getDIR(bool)
		public static List<v2> getDIR(bool diagonal = false)
		{
			List<v2> DIR = new List<v2>();

			DIR.Add((+1, 0));
			if (diagonal == true) DIR.Add((+1, +1));
			DIR.Add((0, +1));
			if (diagonal == true) DIR.Add((-1, +1));
			DIR.Add((-1, 0));
			if (diagonal == true) DIR.Add((-1, -1));
			DIR.Add((0, -1));
			if (diagonal == true) DIR.Add((+1, -1));

			return DIR;
		}

		/// <summary>
		/// get dir based on <paramref name="dir_str"/> name,
		/// example: "r" = (+1, 0), "ru" or "ur" = (+1, +1) 
		/// </summary>
		public static v2 getdir(string dir_str = "r")
		{
			v2 dir = (0, 0);
			foreach (char _char in dir_str)
			{
				if (_char == 'r') dir += (+1, 0);
				if (_char == 'u') dir += (0, +1);
				if (_char == 'l') dir += (-1, 0);
				if (_char == 'd') dir += (0, -1);
			}
			return dir;
		}
		#endregion

		#region ad vec3, vec2 conversion
		public static char axisY = 'y';
		public static implicit operator v2(Vector3 vec3)
		{
			if (v2.axisY == 'y')
				return new v2(C.round(vec3.x), C.round(vec3.y));    // depend on C
			return new v2(C.round(vec3.x), C.round(vec3.z));        // depend on C
		}
		public static implicit operator v2(Vector2 vec2)
		{
			return new v2(C.round(vec2.x), C.round(vec2.y));        // depend on C
		}

		// depend on v2.axisY
		public static implicit operator Vector3(v2 @this)
		{
			if (v2.axisY == 'y')
				return new Vector3(@this.x, @this.y, 0);
			return new Vector3(@this.x, 0, @this.y);
		}
		public static implicit operator Vector2(v2 @this)
		{
			return new Vector2(@this.x, @this.y);
		}
		#endregion
	}
	#endregion

	#region Board
	/*
		- depends on v2
	*/
	public class Board<T>
	{
		public int w, h;
		public v2 m, M;
		public T[][] B;

		#region for clone
		T default_val;
		#endregion

		public Board(v2 size, T default_val)
		{
			this.w = size.x; this.h = size.y;
			this.m = (0, 0); this.M = (size.x - 1, size.y - 1);

			#region for clone
			this.default_val = default_val;
			#endregion

			B = new T[this.h][];
			for (int y = 0; y < this.h; y += 1)
			{
				B[y] = new T[w];
				for (int x = 0; x < this.w; x += 1)
					B[y][x] = default_val;
			}
		}

		public T GT(v2 coord)
		{
			if (coord.in_range((0, 0), (w - 1, h - 1)) == false)
				Debug.LogError($"{coord} not in range of Board range (0, 0) to ({w - 1}, {h - 1})");
			return B[coord.y][coord.x];
		}
		public void ST(v2 coord, T val)
		{
			if (coord.in_range((0, 0), (w - 1, h - 1)) == false)
				Debug.LogError($"{coord} not in range of Board range (0, 0) to ({w - 1}, {h - 1})");
			B[coord.y][coord.x] = val;
		}
		public override string ToString()
		{
			string str = "";
			for (int y = h - 1; y >= 0; y -= 1)
			{
				for (int x = 0; x < w; x += 1)
					str += this.GT((x, y));
				str += '\n';
			}
			return str;
			//return base.ToString();
		}

		#region clone
		public Board<T> clone
		{
			get
			{
				Board<T> new_B = new Board<T>((this.w, this.h), this.default_val);
				for (int y = 0; y < this.h; y += 1)
					for (int x = 0; x < this.w; x += 1)
						new_B.B[y][x] = this.B[y][x];
				return new_B;
			}
		}
		#endregion
	}
	#endregion

	#region MonoInterfaceFinder
	public static class MonoInterfaceFinder
	{
		/// <summary>
		/// Find the first active MonoBehaviour in the scene that implements interface T.
		/// Usage: var imono = UnityInterfaceFinder.FindInterface<IMono>();
		/// </summary>
		public static T FindInterface<T>() where T : class
		{
			if (!typeof(T).IsInterface)
				throw new ArgumentException($"{typeof(T).FullName} is not an interface type.");

			// FindObjectsOfType<MonoBehaviour>() returns active(eneabled GameObjects) MonoBehaviours in the scene.
			// If your Unity version supports the includeInactive overload you can pass true to include inactive GameObjects.
			return UnityEngine.Object.FindObjectsOfType<MonoBehaviour>()
					.OfType<T>()
					.FirstOrDefault();
		}

		/// <summary>
		/// Return all active MonoBehaviours in the scene that implement interface T.
		/// Usage: var all = UnityInterfaceFinder.FindAllInterfaces<IMono>();
		/// </summary>
		public static IEnumerable<T> FindAllInterfaces<T>() where T : class
		{
			if (!typeof(T).IsInterface)
				throw new ArgumentException($"{typeof(T).FullName} is not an interface type.");

			return UnityEngine.Object.FindObjectsOfType<MonoBehaviour>()
					.OfType<T>();
		}

		// if you want another interface say ITestable: 
		/*
			var itetrominoManager = MonoInterfaceFinder.FindInterface(ITetroMinoManager);
			var itestable = itetrominoManager as ITestable;
			itestable?.RunAllTests();
		*/
	}
	#endregion

	public static class Z
	{
		#region dot
		public static float dot(Vector3 a, Vector3 b)
		{
			return a.x * b.x + a.y * b.y + a.z * b.z;
		}
		#endregion

		#region lerp
		public static float lerp(float a, float b, float t)
		{
			float n = b - a;
			return a + n * t;
		}
		public static float t(float v, float min, float max)
		{
			return (v - min) / (max - min);
		}

		public static Vector3 lerp(Vector3 a, Vector3 b, float t)
		{
			Vector3 n = b - a;
			return a + n * t; ;
		}
		#endregion

		#region Path, Bezier
		public static Vector3 Path(float t, params Vector3[] P)
		{
			// error
			#region error
			if (P.Length < 2) throw new Exception();
			#endregion

			if (t >= 1f) return P[P.Length - 1];
			if (t <= 0f) return P[0];

			int N = P.Length - 1;

			float i_F = N * t;
			int i_I = (int)i_F;

			return Z.lerp(P[i_I], P[i_I + 1], t: i_F - i_I);
		}

		public static Vector3 Bezier(float t, params Vector3[] P)
		{
			if (P.Length == 3)
			{
				Vector3 l_0 = Z.lerp(P[0], P[1], t),
						l_1 = Z.lerp(P[1], P[2], t);
				return Z.lerp(l_0, l_1, t);
			}
			else if (P.Length == 4)
			{
				Vector3 l_0 = Z.lerp(P[0], P[1], t),
						l_1 = Z.lerp(P[1], P[2], t),
						l_2 = Z.lerp(P[2], P[3], t);
				Vector3 q_0 = Z.lerp(l_0, l_1, t),
						q_1 = Z.lerp(l_1, l_2, t);

				return Z.lerp(q_0, q_1, t);
			}
			return Vector3.one;
		}
		#endregion
	}

	public class INPUT
	{
		public static void Init(Camera MainCam, RectTransform CanvasRectTransform)
		{
			M.MainCam = MainCam;
			UI.CanvasRectTransform = CanvasRectTransform;
		}

		#region MOUSE
		public static class M
		{
			public static Camera MainCam;
			// default up = new vec3(0, 0, +1)
			public static Vector3 up = new Vector3(0, 0, +1);
			public static Vector3 getPos3D
			{
				get
				{
					// plane: (r - o).up = 0
					Vector3 up = INPUT.M.up;
					Vector3 o = Vector3.zero;

					Ray ray = MainCam.ScreenPointToRay(Input.mousePosition);

					// line: a + n * L
					Vector3 a = ray.origin;
					Vector3 n = ray.direction;

					float L = -Z.dot(a - o, up) / Z.dot(n, up);
					return a + n * L;
				}
			}
			public static bool InstantDown(int mouse_btn_type = 0)
			{
				return Input.GetMouseButtonDown(mouse_btn_type);
			}
			public static bool HeldDown(int mouse_btn_type = 0)
			{
				return Input.GetMouseButton(mouse_btn_type);
			}
			public static bool InstantUp(int mouse_btn_type = 0)
			{
				return Input.GetMouseButtonUp(mouse_btn_type);
			}
		}
		#endregion

		#region K
		public static class K
		{
			public static bool InstantDown(KeyCode keyCode)
			{
				return Input.GetKeyDown(keyCode);
			}
			public static bool HeldDown(KeyCode keyCode)
			{
				return Input.GetKey(keyCode);
			}
			public static bool InstantUp(KeyCode keyCode)
			{
				return Input.GetKeyUp(keyCode);
			}

			public static KeyCode KeyCodeInstantDown
			{
				get
				{
					if (K.InstantDown(KeyCode.W)) return KeyCode.W;
					if (K.InstantDown(KeyCode.A)) return KeyCode.A;
					if (K.InstantDown(KeyCode.S)) return KeyCode.S;
					if (K.InstantDown(KeyCode.D)) return KeyCode.D;
					if (K.InstantDown(KeyCode.Tab)) return KeyCode.Tab;
					if (K.InstantDown(KeyCode.Escape)) return KeyCode.Escape;

					if (K.InstantDown(KeyCode.LeftArrow)) return KeyCode.LeftArrow;
					if (K.InstantDown(KeyCode.RightArrow)) return KeyCode.RightArrow;
					if (K.InstantDown(KeyCode.UpArrow)) return KeyCode.UpArrow;
					if (K.InstantDown(KeyCode.DownArrow)) return KeyCode.DownArrow;

					return KeyCode.Backslash;
				}
			}
		}
		#endregion

		#region UI
		public static class UI
		{
			// is move pointer over (any UI gameobject) / (UI EventSystem) ?
			public static bool Hover
			{
				get { return EventSystem.current.IsPointerOverGameObject(); }
			}

			public static RectTransform CanvasRectTransform;
			public static Vector2 pos
			{
				get { return convert(Input.mousePosition); }
			}
			public static Vector2 size
			{
				get { return convert(new Vector2(Screen.width, Screen.height)); }
			}
			// just scale the vector 2, not the cooardinate axis conversion
			public static Vector2 convert(Vector2 v)
			{
				// return 1280, 720 regardless of canvas scale provided same ratio
				return v / CanvasRectTransform.localScale.x;
			}
			public static Vector2 invConvert(Vector2 v)
			{
				// return 1280, 720 regardless of canvas scale provided same ratio
				return v * CanvasRectTransform.localScale.x;
			}

			#region rect operations
			// using INPUT.UI.convert(vec2)
			// .min, .max, .width, .height
			public static Rect getBounds(RectTransform rectTransform)
			{
				Vector3[] CORNER = new Vector3[4];
				rectTransform.GetWorldCorners(CORNER);

				// convert to canvas scale
				for (int i0 = 0; i0 < CORNER.Length; i0 += 1)
					CORNER[i0] = INPUT.UI.convert(CORNER[i0]);
				// convert to canvas scale

				Rect rect = new Rect(CORNER[0], CORNER[2] - CORNER[0]);
				return rect;
			}
			#endregion
		}
		#endregion
	}

	public static class AN
	{
		public static IEnumerator typewriter_effect(this TextMeshProUGUI tm_gui, float waitInBetween = 0.05f)
		{
			string str = tm_gui.text.ToString();
			string new_str = "";

			for (int i0 = 0; i0 < str.Length; i0 += 1)
			{
				new_str += str[i0];
				tm_gui.text = new_str + "_";

				yield return new WaitForSeconds(waitInBetween);
			}
			//
			tm_gui.text = str;
		}
		public static IEnumerator typewriter_effect(this TextMeshPro tm_gui, float _wait = 0.05f)
		{
			string str = tm_gui.text.ToString();
			string new_str = "";

			for (int i0 = 0; i0 < str.Length; i0 += 1)
			{
				new_str += str[i0];
				tm_gui.text = new_str + "_";

				yield return new WaitForSeconds(_wait);
			}
			//
			tm_gui.text = str;
		}
	}

	// TODO Random Int Generator based on Seed
	public static class C
	{
		public static void Init()
		{
			PrefabHolder = new GameObject("PrefabHolder").transform;
		}
		public static Transform PrefabHolder;

		#region constant
		public static float e = 1f / 100;
		public static float pi = Mathf.PI;
		#endregion

		#region float, vec3 operations
		public static float clamp(float x, float min, float max, float e = 0f)
		{
			if (x > max) return max - e;
			if (x < min) return min + e;
			return x;
		}
		public static Vector3 clamp(Vector3 v, Vector3 min, Vector3 max, float e = 0f)
		{
			return new Vector3()
			{
				x = C.clamp(v.x, min.x, max.x, e),
				y = C.clamp(v.y, min.y, max.y, e),
				z = C.clamp(v.z, min.z, max.z, e),
			};
		}

		public static int round(float x)
		{
			if (x > 0f)
			{
				int x_I = (int)x;
				float frac = x - x_I;
				if (frac > 0.5f) return x_I + 1;
				else return x_I;

			}
			else if (x < 0f)
			{
				int x_I = (int)x;
				float frac = x - x_I;
				if (frac < -0.5f) return x_I - 1;
				else return x_I;
			}
			return 0;
		}
		public static Vector3 round(Vector3 v)
		{
			return new Vector3()
			{
				x = C.round(v.x),
				y = C.round(v.y),
				z = C.round(v.z),
			};
		}

		public static int floor(float x)
		{
			return Mathf.FloorToInt(x);
		}
		public static int ceil(float x)
		{
			return Mathf.CeilToInt(x);
		}
		public static int sign(float x, float e = 1f / 100)
		{
			if (x <= e && x >= -e)
				return 0;

			if (x > e) return +1;
			else return -1;
		}
		public static int mod(int i, int length, int offset = 0)
		{
			int new_i = (i + offset) % length;
			if (new_i < 0) new_i += length;
			return new_i;
		}

		// less than 0.001f considered as zero
		public static bool zero(this float x, float e = (float)1e-4)
		{
			return Mathf.Abs(x) < e;
		}
		public static bool zero(this Vector3 v, float e = (float)1e-4)
		{
			return zero(v.x, e) && zero(v.y, e) && zero(v.z, e);
		}

		public static float abs(float x) { return Mathf.Abs(x); }
		public static int abs(int x) { return Mathf.RoundToInt(Mathf.Abs(x)); }
		public static Vector3 abs(Vector3 vec3) { return new Vector3(Mathf.Abs(vec3.x), Mathf.Abs(vec3.y), Mathf.Abs(vec3.z)); }
		public static Vector2 abs(Vector2 vec2) { return new Vector2(Mathf.Abs(vec2.x), Mathf.Abs(vec2.y)); }
		public static v2 abs(v2 _v2) { return (abs(_v2.x), abs(_v2.y)); }

		public static bool in_range(this float x, float m, float M)
		{
			return x >= m && x <= M;
		}
		public static bool in_range(this Vector3 v, Vector3 m, Vector3 M)
		{
			return C.in_range(v.x, m.x, M.x) &&
					C.in_range(v.y, m.y, M.y) &&
					C.in_range(v.z, m.z, M.z);
		}
		public static bool in_range(this Vector2 v, Vector2 m, Vector2 M)
		{
			return C.in_range(v.x, m.x, M.x) &&
					C.in_range(v.y, m.y, M.y);
		}
		public static bool in_range(this v2 v, v2 m, v2 M)
		{
			return C.in_range(v.x, m.x, M.x) &&
					C.in_range(v.y, m.y, M.y);
		}

		public static Vector3 normalizedZero(this Vector3 v, float e = (float)1E-4)
		{
			if (v.magnitude.zero(e: e) == true)
				return new Vector3(0f, 0f, 0f);
			else return v.normalized;
		}
		public static Vector3 xz(this Vector3 v)
		{
			return new Vector3(v.x, 0f, v.z);
		}
		#endregion

		#region string Operations
		public static string AbrrevatedNumber(int value)
		{
			// Define scales
			Dictionary<long, string> scales = new Dictionary<long, string>
			{
				{1_000_000_000_000, "T"},
				{1_000_000_000,     "B"},
				{1_000_000,         "M"},
				{1_000,             "K"}
			};

			// Numbers below the smallest scale are unchanged
			if (value < 1_000)
				return value.ToString();

			// Find the largest applicable scale
			foreach (long threshold in scales.Keys.OrderByDescending(k => k))
			{
				if (value >= threshold)
				{
					double scaled = (double)value / threshold;
					double truncated = Math.Floor(scaled * 10) / 10;  // one decimal, always down :contentReference[oaicite:7]{index=7}

					// If the number part is 20 or more, drop decimals
					if (truncated >= 10)
						return $"{(int)truncated}{scales[threshold]}";

					// Otherwise, show one decimal (e.g. 1.1k, 19.9k)
					return $"{truncated:0.#}{scales[threshold]}";
				}
			}
			// default
			return value.ToString();
		}
		public static string RoundDecimal(float val, int digits = 2)
		{
			float new_val = (int)(val * Mathf.Pow(10, digits)) / (Mathf.Pow(10, digits));
			return new_val.ToString();
		}
		public static char toChar(this string str)
		{
			if (str.Length < 1)
				Debug.LogError("string length < 1, for .toChar conversion");
			return str[0];
		}
		public static List<char> toCHAR(this string str)
		{
			return str.ToCharArray().ToList();
		}
		public static int parseInt(this string str)
		{
			try
			{
				return int.Parse(str);
			}
			catch (Exception e)
			{
				Debug.LogError($"{str} not in integer format");
				return (int)1e8;
			}
		}
		public static int parseInt(this char _char)
		{
			return C.parseInt(_char.ToString());
		}

		// ad essential >>
		static RegexOptions str_to_flags(string flags)
		{
			RegexOptions options = RegexOptions.None;
			if (!string.IsNullOrEmpty(flags))
			{
				foreach (char flag in flags.ToLower())
					switch (flag)
					{
						case 'i':
							options |= RegexOptions.IgnoreCase;
							break;
						case 'm':
							options |= RegexOptions.Multiline;
							break;
						case 's':
							options |= RegexOptions.Singleline;
							break;
						case 'g':
							// Global flag - doesn't affect split behavior in .NET
							break;
						case 'x':
							options |= RegexOptions.ExplicitCapture;
							break;
					}
			}
			return options;
		}

		/* 
		remove \r from the given string, and 
		also .Trim() which removes terminal
			Spaces  
			Tabs \t
			Newlines \n
			Carriage returns \r
			Form feeds \f
			Vertical tabs \v
			And other Unicode whitespace characters
		*/
		public static string clean(this string raw_str)
		{
			// Remove every '\r' so just '\n' remains
			string clean = raw_str.Replace("\r", "");
			return clean.Trim();
		}
		public static string esc(string str)
		{
			return Regex.Escape(str);
		}
		// << ad essential

		/// <summary>
		/// Splits <paramref name="str"/> on the regex <paramref name="re"/>
		/// Example: "A -> B\n\nC".split(@"\n\n", "gm") ⇒ ["A -> B", "C"]
		/// </summary>
		/// regular expression explicit match approach
		public static string[] split(this string str, string re, string flags = "gx")
		{
			if (str == null) return null;

			// Always include ExplicitCapture by default for split
			return Regex.Split(str.clean(), re, str_to_flags(flags));
		}

		/// <summary>
		/// Returns all substrings of <paramref name="str"/> that match the regex <paramref name="re"/>.
		/// Example: "A -> B, X -> Y".match(@"\w\s*->\s*\w", "gm") ⇒ [ "A -> B", "X -> Y" ]
		/// </summary>
		public static string[] match(this string str, string re, string flags = "gm")
		{
			if (str == null)
				return null;

			// Always include global, multiline capture by default for split
			var matches = Regex.Matches(str.clean(), re, str_to_flags(flags));
			if (matches.Count == 0) return Array.Empty<string>();

			return matches
				.Cast<Match>()
				.Select(m => m.Value)
				.ToArray();
		}
		/// <summary>
		/// Returns weather there is a pattern somewhere in <paramref name="str"/> that match the regex <paramref name="re"/> entirely.
		/// Eg: 'A'.match(@"^[a-g]$", "gi") ⇒ true,
		/// Eg: "TMP text field".match(@"text", "gi") ⇒ true,
		/// </summary>
		public static bool fmatch(this char _char, string re, string flags = "g")
		{
			return Regex.IsMatch(_char.ToString(), re, str_to_flags(flags));
		}
		public static bool fmatch(this string str, string re, string flags = "g")
		{
			return Regex.IsMatch(str, re, str_to_flags(flags));
		}

		/// <summary>
		/// Replaces all occurrences of the regex pattern <paramref name="re"/> with <paramref name="replace_with"/>
		/// Example: "Hello world123 test456".replace(@"\d+", "X", "gm") ⇒ "Hello worldX testX"
		/// </summary>
		public static string replace(this string str, string re, string replace_with, string flags = "gm")
		{
			if (str == null)
				return null;

			// default flags: "gm"
			return Regex.Replace(str.clean(), re, replace_with, str_to_flags(flags));
		}

		/// <summary>
		/// shows the raw string in a single line with \r\n \t appeanded as chars, ad: \f, \v
		/// </summary>
		public static string flat(this string str, string name = "")
		{
			string singleLine = str
					.Replace("\r", "\\r")   // \r
					.Replace("\n", "\\n")   // \n
					.Replace("\t", "\\t")   // \t
					.Replace("\f", "\\f")
					.Replace("\v", "\\v");
			return name + singleLine;
		}

		public static string repeat(this char _char, int count = 100) { return new string(_char, count); }

		public static string join(this IEnumerable<string> STRING, string separator = ", ")
		{
			return string.Join(separator, STRING);
		}

		public static string colorTag(this string str, string color = "white")
		{
			return $"<color={color}>{str}</color>";
		}

		public static string method(string method, object obj, string color = "white")
		{
			return $"{method}(): {obj.ToString()}".colorTag(color);
		}
		#endregion

		#region enum operations
		// used as: C.getEnumLength<PieceType>();
		public static int getEnumLength<T>() where T : struct
		{
			Type enumType = typeof(T);

			if (!enumType.IsEnum)
			{
				throw new ArgumentException("The type must be an enum.");
			}

			return Enum.GetValues(enumType).Length;
		}
		/*
		used as: 
			PieceType piece = PieceType.Rook;
			int index = C.getEnumIndex(piece);
		*/
		public static int getEnumIndex<T>(T enumValue) where T : struct
		{
			Type enumType = typeof(T);

			if (!enumType.IsEnum)
			{
				throw new ArgumentException("The type must be an enum.");
			}

			T[] enumValues = (T[])Enum.GetValues(enumType);
			return Array.IndexOf(enumValues, enumValue);
		}

		// used as 
		// float weight = C.getEnumWeight(PieceType.Knight); 
		// return 0f to 1f weight
		public static float getEnumWeight<T>(T enumValue) where T : struct
		{
			Type enumType = typeof(T);

			if (!enumType.IsEnum)
			{
				throw new ArgumentException("The type must be an enum.");
			}

			T[] enumValues = (T[])Enum.GetValues(enumType);
			return Array.IndexOf(enumValues, enumValue) * 1f / enumValues.Length;
		}

		// used as: foreach(PieceType piece in C.getEnumValues<PieceType>())
		public static T[] getEnumValues<T>() where T : struct
		{
			return (T[])Enum.GetValues(typeof(T));
		}
		#endregion

		#region Anim
		public static async Task delay(int ms = 1000)
		{
			await Task.Delay(ms);
		}
		public static IEnumerator wait(int ms = 1000)
		{
			yield return new WaitForSeconds(ms * 1f / 1000);
		}
		/*
		// animation approach for a given duration >>
		for(float time = 0f; time <= duration; time += dt)
		{
			float t = 1f/duration
			// do somthng with C.ease(t, "in_out");
			yield return null
		}
		// << animation approach for a given duration
		*/
		#endregion

		#region transform util
		public static void clearLeaves(this Transform transform)
		{
			for (int i0 = 0; i0 < transform.childCount; i0 += 1)
				GameObject.Destroy(transform.GetChild(i0).gameObject);
		}

		public static void disableLeaves(this Transform transform)
		{
			for (int i0 = 0; i0 < transform.childCount; i0 += 1)
				transform.GetChild(i0).gameObject.SetActive(false);
		}
		#endregion
		/*
			[System.Serializable]object.ToJson() -> string
			str.FromJson<T>() -> T
		*/
		#region json, byte operations
		/// <summary>
		/// Convert A Serielizable (object) To JSON (string).
		/// called as string Json = object.ToJson(true);
		/// </summary>
		public static string ToJson(this object obj, bool pretify = true)
		{
			if (obj == null)
				return "obj is null";

			return JsonUtility.ToJson(obj, pretify);
		}

		/// <summary>
		/// Deserializes a JSON string into a new instance of T.
		/// called as T _T = str.FromJson<T>();
		/// </summary>
		public static T FromJson<T>(this string json)
		{
			try
			{
				return JsonUtility.FromJson<T>(json);
			}
			catch (Exception e)
			{
				Debug.LogError("Cannot Parse Json to Type T");
				//return default;
				throw;
			}
		}

		/// <summary>
		/// string to byte[]
		/// </summary>
		public static byte[] ToBytes(this string json)
		{
			return System.Text.Encoding.UTF8.GetBytes(json);
		}
		#endregion

		#region INFO
		public static class SYS
		{
			public static string id = SystemInfo.deviceUniqueIdentifier;
			public static string device = SystemInfo.deviceModel.ToString();
			public static string os = SystemInfo.operatingSystem.ToString();
			public static string ram = SystemInfo.systemMemorySize.ToString();
			public static string cpu = SystemInfo.processorType.ToString();
			public static string gpu = SystemInfo.graphicsDeviceName.ToString();
			public static string gpu_ver = SystemInfo.graphicsDeviceVersion.ToString();
			public static string time_stamp = System.DateTime.UtcNow.ToString("yyyy/MMMM/dd HH:mm:ss.fff").ToString();
		}
		#endregion
	}

	/*
		- GameOnjectt/Transform Search
		- Find Non Collision Spot 2D, 3D

	*/
	public static class U
	{
		#region Transform/GameObject Leaf Search
		// converted to lowercase before check
		#region .NameStartsWith
		/// <summary>
		/// Get Transform at Gen 1
		/// </summary>
		public static Transform NameStartsWith(this Transform transform, string name)
		{
			for (int i0 = 0; i0 < transform.childCount; i0 += 1)
				if (transform.GetChild(i0).name.ToLower().StartsWith(name.ToLower()))
					return transform.GetChild(i0);
			Debug.LogError($"found no leaf starting with that name: {name.ToLower()}, under transform: {transform.name}");
			return null;
		}
		public static GameObject NameStartsWith(this GameObject gameObject, string name)
		{
			Transform transform = gameObject.transform;
			for (int i0 = 0; i0 < transform.childCount; i0 += 1)
				if (transform.GetChild(i0).name.ToLower().StartsWith(name.ToLower()))
					return transform.GetChild(i0).gameObject;
			Debug.LogError($"found no leaf starting with that name: {name.ToLower()}, under transform: {transform.name}");
			return null;
		}

		#region Query location: b > c > d > e
		/// <summary>
		/// if transform @"a" = anscestor, than transform @"a".Query("b > c > d > e") =  transform @"d".NameStartsWith("e")
		/// </summary>
		public static Transform Query(this Transform transform, string query, char sep = '>') // query: leaf > leaflet
		{
			string[] QUERY = query.split($@"(\s)*(\{sep})(\s)*"); // clean(prior) by default than split
			Transform leaf = transform;
			foreach (string name in QUERY)
				leaf = leaf.NameStartsWith(name); // error pause(name not found) handled by .NameStartsWith(name)
			return leaf;
		}
		/// <summary>
		/// if gameObject @"a" = anscestor, than gameObject @"a".Query("b > c > d > e") = gameObject @"d".NameStartsWith("e")
		/// </summary>
		public static GameObject Query(this GameObject gameObject, string query, char sep = '>') // query: leaf > leaflet
		{
			return gameObject.transform.Query(query, sep).gameObject;
		}
		#endregion
		#endregion

		// get Leaves under a Transform/GameObject
		#region .GetFirstGenLeaves
		/// <summary>
		/// Get L<Transform> at Gen 1
		/// </summary>
		public static List<Transform> GetLeaves(this Transform transform)
		{
			List<Transform> T = new List<Transform>();
			for (int i0 = 0; i0 < transform.childCount; i0 += 1)
				if (transform.GetChild(i0))
					T.Add(transform.GetChild(i0));

			if (T.Count == 0)
				Debug.LogError($"found no leaves under: {transform.name}");
			return T;
		}
		public static List<GameObject> GetLeaves(this GameObject gameObject)
		{
			List<GameObject> G = new List<GameObject>();
			Transform transform = gameObject.transform;
			for (int i0 = 0; i0 < transform.childCount; i0 += 1)
				if (transform.GetChild(i0))
					G.Add(transform.GetChild(i0).gameObject);

			if (G.Count == 0)
				Debug.LogError($"found no leaves under: {transform.name}");
			return G;
		}
		#endregion

		// GetDepthLeaf under multiple gen of Transform/GameObject
		#region GetDepthLeaf
		/// <summary>
		/// Get Transform after Depth Search
		/// </summary>
		public static Transform GetDepthLeaf(this Transform transform, string name)
		{
			foundChild = null;
			DepthSearch(transform, name);
			if (foundChild == null)
				Debug.LogError($"no leaf found in depth search with name {name} under {transform.name}");
			return foundChild;
		}
		public static GameObject GetDepthLeaf(this GameObject gameObject, string name)
		{
			foundChild = null;
			DepthSearch(gameObject.transform, name);
			if (foundChild == null)
				Debug.LogError($"no leaf found in depth search with name {name} under {gameObject.name}");
			return foundChild.gameObject;
		}

		static Transform foundChild;
		// self comparision approach >>
		static void DepthSearch(Transform transform, string name)
		{
			// exit //
			if (transform.name.ToLower().StartsWith(name) == true)
			{
				foundChild = transform;
				return;
			}

			// exit //
			if (foundChild != null)
				return;

			for (int i0 = 0; i0 < transform.childCount; i0 += 1)
				// recurive >>
				DepthSearch(transform.GetChild(i0), name);
			// << recursive
		}
		// << self comparision approach
		#endregion

		// Get Component Shorter format 
		#region GetComponent -> GC
		public static T GC<T>(this GameObject go) where T : Component
		{
			return go.GetComponent<T>();
		}

		public static Transform GTrLeaf<T>(this GameObject go) where T : Component
		{
			return go.GetComponentInChildren<T>().transform;
		}

		public static T GCLeaf<T>(this GameObject go) where T : Component
		{
			return go.GetComponentInChildren<T>();
		}

		public static IEnumerable<T> GCLeaves<T>(this GameObject go) where T : Component
		{
			return go.GetComponentsInChildren<T>();
		}

		#endregion
		#endregion

		#region CanPlaceObject ? at a give pos, _prefab.collider, rotationY
		// CanPlaceBuilding.... pos2D, gameObject with a collider2D 
		#region CanPlaceObject2D(Vector2 pos2D, GameObject gameObject, int rotationZ = 0)
		public static bool CanPlaceObject2D(Vector2 pos2D, GameObject gameObject, int rotationZ = 0)
		{
			Collider2D collider = gameObject.GetComponent<Collider2D>();

			if (collider is BoxCollider2D)
			{
				BoxCollider2D boxCollider2D = (BoxCollider2D)collider;
				Collider2D[] COLLIDER = Physics2D.OverlapBoxAll(pos2D + boxCollider2D.offset, boxCollider2D.size, angle: 0f);
				return COLLIDER.Length == 0;
			}
			else if (collider is CircleCollider2D)
			{
				CircleCollider2D circleCollider2D = (CircleCollider2D)collider;
				Collider2D[] COLLIDER = Physics2D.OverlapCircleAll(pos2D + circleCollider2D.offset, circleCollider2D.radius);
				return COLLIDER.Length == 0;
			}
			else if (collider is CapsuleCollider2D)
			{
				CapsuleCollider2D capsuleCollider2D = (CapsuleCollider2D)collider;
				Collider2D[] COLLIDER = Physics2D.OverlapCapsuleAll(pos2D + capsuleCollider2D.offset, capsuleCollider2D.size, capsuleCollider2D.direction, angle: 0f);
				return COLLIDER.Length == 0;
			}
			//
			Debug.LogError($"no collider attached to {gameObject.name} at {gameObject.transform.position}");
			return true;
		}

		#endregion
		// CanPlaceBuilding.... pos3D, gameObject with a collider
		#region CanPlaceObject3D(Vector3 pos3D,GameObject _prefab,int rotationY = 0)
		/// <summary>
		/// Determines whether a prefab (with potentially multiple BoxColliders) can be placed
		/// at the given world position and euler rotation, without overlapping any existing colliders.
		/// </summary>
		/// <param name="_prefab">A GameObject template that has one or more BoxCollider components.</param>
		/// <param name="pos3D">Desired world‑space position for the prefab’s root.</param>
		/// <param name="eulerRotation">Desired world‑space rotation (in degrees) for the prefab’s root.</param>
		/// <param name="layerMask">Which layers to include in the overlap test (defaults to all layers).</param>
		/// <returns>True if no overlaps occur; false if any collider would intersect something else.</returns>
		public static bool CanPlaceObject3D(
			Vector3 pos3D,
			GameObject _prefab,
			int rotationY = 0)
		{
			// Make sure the physics engine is up to date
			Physics.SyncTransforms();
			LayerMask layerMask = Physics.AllLayers;

			// Parent orientation from the requested Euler angles
			var parentOrientation = Quaternion.Euler(new Vector3(0f, 90 * rotationY, 0f));

			// Gather all BoxColliders (including on children)
			var boxes = _prefab.GetComponents<BoxCollider>();

			foreach (var box in boxes)
			{
				// 1. Calculate world‐space half extents:
				//    half = local size * 0.5, then scale by the collider's lossyScale
				float e = 1f / 1000;
				Vector3 halfExtents = Vector3.Scale(box.size * 0.5f, box.transform.lossyScale) - Vector3.one * e;

				// 2. Calculate world‐space center:
				//    take local center offset, scale it, rotate by parentOrientation, then translate
				Vector3 scaledCenterOffset = Vector3.Scale(box.center, box.transform.lossyScale);
				Vector3 worldCenter = pos3D + parentOrientation * scaledCenterOffset;

				// 3. Calculate world‐space orientation of this collider:
				//    combine parentRotation with the collider's local rotation
				Quaternion worldRot = parentOrientation * box.transform.localRotation;

				// 4. Perform the overlap test, ignoring trigger-only colliders
				Collider[] hits = Physics.OverlapBox(
					worldCenter,
					halfExtents,
					worldRot,
					layerMask,
					QueryTriggerInteraction.Ignore);

				// 5. If any hit is **not** part of our prefab, placement is invalid
				if (hits.Length != 0)
					return false;
			}

			// All colliders were clear
			return true;
		}
		#endregion
		#endregion

		// Extension
		#region minMax(func, bool), find(func), findIndex(func), forEach(func), map()
		public static T minMax<T>(this T[] T_1D, Func<T, T, float> cmp_func)
		{
			T min = T_1D[0];
			for (int i0 = 1; i0 < T_1D.Length; i0 += 1)
				if (cmp_func(T_1D[i0], min) < 0f) // if ( b - a ) < 0f, than a < b, so swap
					min = T_1D[i0];
			return min;
		}
		public static T minMax<T>(this List<T> T_1D, Func<T, T, float> cmp_func, bool splice = false)
		{
			T min = minMax(T_1D.ToArray(), cmp_func);
			if (splice)
				T_1D.Remove(min);
			return min;
		}
		public static T find<T>(this IEnumerable<T> collection, Func<T, bool> predicate)
		{
			if (collection == null) throw new ArgumentNullException(nameof(collection));
			if (predicate == null) throw new ArgumentNullException(nameof(predicate));

			// collection.MoveNext(), or a foreach loop
			foreach (var item in collection)
				if (predicate(item))
					return item;
			Debug.Log("found none with collection name provided");
			return default(T); // Returns null for reference types, default value for value types
		}
		public static int findIndex<T>(this IEnumerable<T> collection, Func<T, bool> predicate)
		{
			if (collection == null) throw new ArgumentNullException(nameof(collection));
			if (predicate == null) throw new ArgumentNullException(nameof(predicate));

			int index = 0;
			// collection.MoveNext(), or a foreach loop
			foreach (var item in collection)
			{
				if (predicate(item))
					return index;
				index += 1;
			}
			return -1; // Returns -1 if found none
		}
		public static void forEach<T>(this IEnumerable<T> collection, Action<T> action)
		{
			foreach (var e in collection)
				action(e);
		}

		#region map(func(elem)), map(func(elem, index))
		public static IEnumerable<TResult> map<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
		{
			return source.Select(selector);
		}
		public static IEnumerable<TResult> map<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, int, TResult> selector)
		{
			return source.Select((item, index) => selector(item, index));
		}
		#endregion

		#region refine(func(elem)), refine(func(elem, index))
		public static IEnumerable<TSource> refine<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
		{
			return source.Where(predicate);
		}
		public static IEnumerable<TSource> refine<TSource>(this IEnumerable<TSource> source, Func<TSource, int, bool> predicate)
		{
			return source.Where((item, index) => predicate(item, index));
		}
		#endregion

		public static T gl<T>(this IEnumerable<T> collection, int index_from_last)
		{
			if (collection == null) throw new ArgumentNullException(nameof(collection)); // nameof(collection) = "collection"

			if (index_from_last < 0 || index_from_last >= collection.Count())
				Debug.LogError($"in {nameof(collection)} gl{index_from_last} > count: {collection.Count()}");

			return collection.ElementAt(collection.Count() - 1 - index_from_last);
		}

		/// <summary>
		/// Groups elements by a selector function and returns a dictionary with unique keys and lists of all matching elements
		/// </summary>
		/// <typeparam name="TSource">The type of elements in the source collection</typeparam>
		/// <typeparam name="TKey">The type of the key returned by the selector</typeparam>
		/// <param name="LIST">The source collection</param>
		/// <param name="key_func">A function to extract the key from each element</param>
		/// <returns>A dictionary with unique keys and lists of all matching elements</returns>
		public static Dictionary<TKey, List<TSource>> UNQ<TSource, TKey>(
			this IEnumerable<TSource> LIST,
			Func<TSource, TKey> key_func)
		{
			if (LIST == null) Debug.LogError($"list is null");
			if (key_func == null) Debug.LogError($"key_func is null");

			Dictionary<TKey, List<TSource>> result = new Dictionary<TKey, List<TSource>>();
			foreach (var item in LIST)
			{
				TKey key = key_func(item);
				if (result.ContainsKey(key) == false)
					result[key] = new List<TSource>();
				result[key].Add(item);
			}
			return result;
		}
		#endregion
	}

	#region ITER
	// ITER.iter_inc(1e4) => true when limit exeed
	// ITER.reset()
	public static class ITER
	{
		public static void reset() { ITER_1D = new List<int>() { 0 }; }

		private static List<int> ITER_1D;
		public static void create() { ITER_1D.Add(0); }
		public static void done() { if (ITER_1D.Count != 0) ITER_1D.RemoveAt(ITER_1D.Count - 1); }
		public static bool iter_inc(double limit = 1e4)
		{
			ITER_1D[ITER_1D.Count - 1] += 1;
			if (ITER_1D[ITER_1D.Count - 1] > limit)
			{
				Debug.Log($"iter @{ITER_1D.Count - 1} > {limit}");
				return true;
			}
			else
				return false;
		}
	}
	#endregion

	#region LOG
	/*
		.SaveLog(str)
		.SaveGame(str)
		.LoadGame
		.ToTable(toString(bool), "name")
	*/
	// file LOG.INITIALIZE() before
	public static class LOG
	{
		static string LocFolder = Application.dataPath + "/LOG";
		static string LocFile_LOG = Application.dataPath + "/LOG/LOG.md";
		static string LocFile_GameData = Application.dataPath + "/LOG/GameData.txt";
		public static void Init() // create dir LOG/LOG.txt, LOG/GameData.txt if it doesn't exist
		{
			if (System.IO.Directory.Exists(LocFolder) == false) System.IO.Directory.CreateDirectory(LocFolder);
			if (System.IO.File.Exists(LocFile_LOG) == false) System.IO.File.Create(LocFile_LOG);
			if (System.IO.File.Exists(LocFile_GameData) == false) System.IO.File.Create(LocFile_GameData);
		}

		public static void SaveLog(string str, string syntaxType = "")
		{
			if (syntaxType != "")
				str = $"```{syntaxType}\n{str}\n```"; // format for markDown

			// string str = string.Join("\n\n", args);
			//string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
			//string logEntry = $"[{timestamp}] {str}";
			Debug.Log($"logged into file LOG.txt: {str}".colorTag("lime"));
			// File logging

			try { System.IO.File.AppendAllText(LocFile_LOG, str + Environment.NewLine + Environment.NewLine); }
			catch (Exception e) { Debug.LogError($"Failed to write to log file: {e.Message}"); }
		}
		public static void H(string header) { SaveLog($"# {header} >>\n"); }
		public static void HEnd(string header) { SaveLog($"# << {header}"); }

		public static void SaveGame(string str)
		{
			Debug.Log($"SaveGame(str)".colorTag("lime"));
			Debug.Log($"logged into file GameData.txt: {str}");
			System.IO.File.WriteAllText(LocFile_GameData, str);
		}
		public static string LoadGame
		{
			get
			{
				try
				{
					Debug.Log("Loaded GameData.txt");
					// Read raw text from disk…
					string raw = System.IO.File.ReadAllText(LocFile_GameData);
					return raw;
				}
				catch (Exception)
				{
					Debug.LogError("no file found at: " + LocFile_GameData);
					throw;
				}
			}
		}

		// Used As: LOG.SaveLog(LIST.ToTable(name = "LIST<> "))
		#region ToTable
		#region ToTable Util
		/// <summary>
		/// Sanitizes a string by converting control characters and non-printable ASCII to readable representations
		/// </summary>
		/// <param name="input">The input string to sanitize</param>
		/// <returns>A sanitized string safe for text file output</returns>
		public static string SanitizeForTextOutput(string input)
		{
			if (string.IsNullOrEmpty(input))
				return input ?? "null";

			var sb = new StringBuilder();

			foreach (char c in input)
			{
				// Handle common control characters with readable names
				switch (c)
				{
					case '\0': sb.Append("(ascii:0-NUL)"); break;
					case '\a': sb.Append("(ascii:7-BEL)"); break;
					case '\b': sb.Append("(ascii:8-BS)"); break;
					case '\t': sb.Append("(ascii:9-TAB)"); break;
					case '\n': sb.Append("(ascii:10-LF)"); break;
					case '\v': sb.Append("(ascii:11-VT)"); break;
					case '\f': sb.Append("(ascii:12-FF)"); break;
					case '\r': sb.Append("(ascii:13-CR)"); break;
					case '\x1B': sb.Append("(ascii:27-ESC)"); break;
					default:
						// Check if it's a control character or extended ASCII that might cause issues
						if (char.IsControl(c) || (int)c > 126)
						{
							sb.Append($"(ascii:{(int)c})");
						}
						else
						{
							// Printable ASCII character, keep as-is
							sb.Append(c);
						}
						break;
				}
			}

			return sb.ToString();
		}
		#endregion

		/// <summary>
		/// <paramref name="toString"/>: by default false, if true each row is logged based on simple value.ToString().flat()
		/// Produces a simple ASCII "table" of all public/instance/private fields of each element in <paramref name="list"/>.
		/// If a field's value is any IEnumerable (but not a string), prints its item‐count instead of ToString().
		/// Now handles control characters and non-printable ASCII safely for text file output.
		/// </summary>
		// LIST<> or HASH<> or Q<> or MAP<>
		public static string ToTable<T>(this IEnumerable<T> list, bool toString = false, string name = "LIST<>")
		{
			if (list == null)
				return "_list/hash/map/queue is null_";

			var items = list.ToList();
			if (items.Count == 0)
				return "_list/hash/map/queue got no elem_";

			// @ - if toString enabled
			#region toString enabled for Dictionary Values
			if (toString == true)
			{
				string str = "";
				string header = $"* {name} Count: {list.Count()}";

				// Calculate column widths based on field names and all item‐values
				int cw = header.Length;
				foreach (var e in list)
				{
					string flatValue = SanitizeForTextOutput(e.ToString().flat());
					int width = flatValue.Length;
					cw = Mathf.Max(cw, width);
				}
				// extra padding for column width

				// Build the header row ("field names")
				// push right by cw and 1 beyond, add a left padding of 1 more
				str = header.PadRight(cw + 1).PadLeft(cw + 2);

				// Separator line (e.g. "------+-------+------")
				str += '\n' + new string('-', cw + 2) + '\n';

				// Each Row
				foreach (var e in list)
				{
					string sanitizedValue = SanitizeForTextOutput(e.ToString().flat());
					str += sanitizedValue.PadRight(cw + 1).PadLeft(cw + 2) + '\n';
				}
				return str;
			}
			#endregion

			// @ if toString disabled
			#region toString disabled
			var sb = new StringBuilder();
			var type = typeof(T);

			// Gather all fields (public, instance, non-public)
			var fields = type.GetFields(
				BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic
			);

			// Helper: get a "display string" for a field‐value.
			// If it's IEnumerable (and not a string), show "TypeName: count"; otherwise, ToString()/"null".
			string RenderElemValue(object val)
			{
				if (val == null)
					return "null";

				// If it's a string, treat it as a scalar and sanitize it:
				if (val is string s)
					return SanitizeForTextOutput(s);

				// If it's any other IEnumerable, count its elements and prepend the collection type name:
				if (val is IEnumerable enumerable)
				{
					int count = 0; foreach (var _ in enumerable) count += 1;

					// Determine type name:
					var tVal = val.GetType();
					string typeName;

					if (tVal.IsArray)
						typeName = "[]";
					else if (tVal.IsGenericType)
						// e.g. List`1 → "List", HashSet`1 → "HashSet", Queue`1 → "Queue"
						typeName = tVal.GetGenericTypeDefinition().Name.Split('`')[0];
					else
						typeName = tVal.Name;

					return $"{typeName}: {count}";
				}

				// Otherwise, fallback to ToString() and sanitize:
				return SanitizeForTextOutput(val.ToString());
			}

			// pri~ for private field
			// sta~ for static field
			//  ~  for otherwise which also include public field
			string GetPrefixedFieldName(FieldInfo fieldInfo)
			{
				string rawName = fieldInfo.Name;
				if (fieldInfo.IsPrivate) rawName = "pri~" + rawName;
				else if (fieldInfo.IsStatic) rawName = "sta~" + rawName;
				else rawName = "~" + rawName;
				return rawName;
			}

			// 1) Calculate column widths based on field names and all item‐values
			var columnWidths = new int[fields.Length];
			for (int i = 0; i < fields.Length; i += 1)
			{
				// base width = field name length
				columnWidths[i] = GetPrefixedFieldName(fields[i]).Length;

				// check each item's value in that field
				foreach (var item in items)
				{
					var rawValue = fields[i].GetValue(item);
					string disp = RenderElemValue(rawValue);
					columnWidths[i] = Math.Max(columnWidths[i], disp.Length); // alter columnWidth when higher width string is found in the column val
				}

				columnWidths[i] += 2; // add some padding
			}

			// 2) Build the header row ("field names")
			sb.AppendLine(
				string.Join(" | ",
					fields.Select((f, idx) =>
					{
						string header = GetPrefixedFieldName(f);
						return header.PadRight(columnWidths[idx]);
					})
			));

			// 3) Separator line (e.g. "------+-------+------")
			for (int i = 0; i < fields.Length; i += 1)
			{
				sb.Append(new string('-', columnWidths[i]));
				if (i < fields.Length - 1)
					sb.Append("-+-");
			}
			sb.AppendLine();

			// 4) Rows: for each item, render each field's value (or "count" for IEnumerable)
			foreach (var item in items)
			{
				var rowValues = fields.Select((f, idx) =>
				{
					object rawValue = f.GetValue(item);
					string text = RenderElemValue(rawValue);
					return text.PadRight(columnWidths[idx]);
				});

				sb.AppendLine(string.Join(" | ", rowValues));
			}

			return $"# {name}:\n" + sb.ToString();
			#endregion
		} 
		#endregion
	}
	#endregion

	#region DRAW
	public static class DRAW_prev
	{
		public static Color col = Color.red;
		public static float dt = 10f;

		public static Transform DebugHolder;

		public static void Init()
		{
			if (GameObject.Find("DebugHolder") != null)
				GameObject.Destroy(GameObject.Find("DebugHolder"));
			DebugHolder = new GameObject("DebugHolder").transform;
		}


		#region Line
		public static void Line(Vector3 a, Vector3 b, float e = 1f / 200)
		{

		} 
		#endregion

		#region ARROW
		public static void Arrow(Vector3 a, Vector3 b, float t = 1f, float s = 1f / 15, float e = 1f / 200)
		{

		}
		#endregion

	}
	#endregion
}


