using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using System.Threading.Tasks;
using System.IO;

using System.Security.Cryptography;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using TMPro;

namespace SPACE_UTIL
{
	#region v2
	// creation of v2:
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
		#region arithmatic operator
		public static v2 operator +(v2 a, v2 b) { return new v2(a.x + b.x, a.y + b.y); }
		public static v2 operator -(v2 a, v2 b) { return new v2(a.x - b.x, a.y - b.y); }
		public static v2 operator *(v2 a, v2 b) { return new v2(a.x * b.x, a.y * b.y); }
		public static v2 operator *(v2 v, int m) { return new v2(v.x * m, v.y * m); }
		public static v2 operator *(int m, v2 v) { return new v2(v.x * m, v.y * m); }

		public static bool operator ==(v2 a, v2 b) { return a.x == b.x && a.y == b.y; }
		public static bool operator !=(v2 a, v2 b) { return a.x != b.x || a.y != b.y; }

		public static bool operator >(v2 a, v2 b) => a.x > b.x && a.y > b.y;
		public static bool operator <(v2 a, v2 b) => a.x < b.x && a.y < b.y;
		public static bool operator >=(v2 a, v2 b) => a.x >= b.x && a.y >= b.y;
		public static bool operator <=(v2 a, v2 b) => a.x <= b.x && a.y <= b.y;
		#endregion

		#region ad implicit conversion
		// Allow implicit conversion from tuple
		public static implicit operator v2((int, int) tuple) => new v2(tuple.Item1, tuple.Item2); 
		#endregion

		#region getDIR(bool)
		/// <summary>
		/// get List of v2 in all 4 direction, with (optional) if diagonal required.
		/// </summary>
		/// <param name="includeDiagonal"></param>
		/// <returns></returns>
		public static List<v2> getDIR(bool includeDiagonal = false, bool randomOrder = false)
		{
			List<v2> DIR = new List<v2>();

			DIR.Add((+1, 0));
			if (includeDiagonal == true) DIR.Add((+1, +1));
			DIR.Add((0, +1));
			if (includeDiagonal == true) DIR.Add((-1, +1));
			DIR.Add((-1, 0));
			if (includeDiagonal == true) DIR.Add((-1, -1));
			DIR.Add((0, -1));
			if (includeDiagonal == true) DIR.Add((+1, -1));

			if(randomOrder)
			{
				for(int iter = 0; iter < DIR.Count; iter += 1)
				{
					int indexA = UnityEngine.Random.Range(0, DIR.Count);
					int indexB = UnityEngine.Random.Range(0, DIR.Count);
					// swap >>
					v2 temp = DIR[indexA];
					DIR[indexA] = DIR[indexB];
					DIR[indexB] = temp;
					// << swap
				}
			}
			return DIR;
		}

		/// <summary>
		/// get dir based on <paramref name="dirStr"/> name,
		/// example: "r" = (+1, 0), "ru" or "ur" = (+1, +1) 
		/// </summary>
		public static v2 getdirFromName(string dirStr = "r")
		{
			v2 dir = (0, 0);
			foreach (char _char in dirStr)
			{
				if (_char == 'r') dir += (+1, 0);
				if (_char == 'u') dir += (0, +1);
				if (_char == 'l') dir += (-1, 0);
				if (_char == 'd') dir += (0, -1);
			}
			return dir;
		}

		// constant
		public static v2 right = (+1, 0),
							up = (0, +1),
						  left = (-1, 0),
						  down = (0, -1);
		#endregion

		#region ad: v3, vec3, vec2 conversion
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

	#region Board 2D via v2
	#region Board_prev
	/*
		- depends on v2
	*/
	[System.Serializable]
	class Board_prev<T>
	{
		public int w, h;
		public v2 m, M;
		public T[][] B;

		public Board_prev(v2 size, T default_val)
		{
			this.w = size.x; this.h = size.y;
			this.m = (0, 0); this.M = (size.x - 1, size.y - 1);

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
			if (coord.inRange((0, 0), (w - 1, h - 1)) == false)
				Debug.LogError($"{coord} not in range of Board range (0, 0) to ({w - 1}, {h - 1})");
			return B[coord.y][coord.x];
		}
		public void ST(v2 coord, T val)
		{
			if (coord.inRange((0, 0), (w - 1, h - 1)) == false)
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
		public Board_prev<T> clone
		{
			get
			{
				Board_prev<T> new_B = new Board_prev<T>((this.w, this.h), this.GT(new v2(0, 0)));
				for (int y = 0; y < this.h; y += 1)
					for (int x = 0; x < this.w; x += 1)
						new_B.B[y][x] = this.B[y][x];
				return new_B;
			}
		}
		#endregion
	}
	#endregion
	/*
		- depends on v2
		- Serializable with flat array storage
		- Maintains clean indexer syntax: board[x, y] or board[coord]
		- Supports increment/decrement operations
	*/
	[System.Serializable]
	public class Board<T>
	{
		public int w, h;
		public v2 m, M; // [NonSerialized] if do not want to reflect in inspector by default or saved in Json.

		// Flat array for serialization (JsonUtility compatible)
		[SerializeField] private T[] B_flat;

		// Cached jagged array for compatibility (lazy-loaded, non-serialized)
		[System.NonSerialized] private T[][] B_cache;

		#region Constructors
		public Board(v2 size, T default_val)
		{
			this.w = size.x;
			this.h = size.y;
			this.m = (0, 0);
			this.M = (size.x - 1, size.y - 1);

			// Initialize flat array
			B_flat = new T[w * h];
			for (int i = 0; i < B_flat.Length; i += 1)
				B_flat[i] = default_val;
		}

		// Constructor for deserialization (when loading from JSON)
		public Board() { }
		#endregion

		#region Index Conversion
		private int ToIndex(int x, int y) => y * w + x;
		private int ToIndex(v2 coord) => coord.y * w + coord.x;
		#endregion

		#region Indexers - Clean Syntax!
		/// <summary>
		/// Access via: board[x, y] = value; or value = board[x, y];
		/// Example: board[5, 3]++; board[2, 1] += 10;
		/// </summary>
		public T this[int x, int y]
		{
			get
			{
				if (!new v2(x, y).inRange((0, 0), (w - 1, h - 1)))
					Debug.LogError($"({x}, {y}) not in range of Board (0, 0) to ({w - 1}, {h - 1})");
				return B_flat[ToIndex(x, y)];
			}
			set
			{
				if (!new v2(x, y).inRange((0, 0), (w - 1, h - 1)))
					Debug.LogError($"({x}, {y}) not in range of Board (0, 0) to ({w - 1}, {h - 1})");
				B_flat[ToIndex(x, y)] = value;
				InvalidateCache(); // Clear cache when modified
			}
		}

		/// <summary>
		/// Access via: board[coord] = value; or value = board[coord];
		/// Example: board[(5, 3)]++; board[new v2(2, 1)] += 10;
		/// </summary>
		public T this[v2 coord]
		{
			get => this[coord.x, coord.y];
			set => this[coord.x, coord.y] = value;
		}
		#endregion

		#region Legacy API (Get/Set) - Kept for backwards compatibility
		public T Get(v2 coord) => this[coord];
		public void Set(v2 coord, T val) => this[coord] = val;
		#endregion

		#region Jagged Array Access - B[][] (lazy-loaded for compatibility)
		/// <summary>
		/// Legacy jagged array access: board.B[y][x]
		/// NOTE: Cached and regenerated on-demand. Use indexers for better performance.
		/// </summary>
		public T[][] B
		{
			get
			{
				if (B_cache == null)
				{
					B_cache = new T[h][];
					for (int y = 0; y < h; y++)
					{
						B_cache[y] = new T[w];
						for (int x = 0; x < w; x++)
							B_cache[y][x] = B_flat[ToIndex(x, y)];
					}
				}
				return B_cache;
			}
		}

		private void InvalidateCache() => B_cache = null;

		/// <summary>
		/// Sync jagged array changes back to flat array (call before saving if you used B[][])
		/// </summary>
		public void SyncFromJagged()
		{
			if (B_cache != null)
			{
				for (int y = 0; y < h; y++)
					for (int x = 0; x < w; x++)
						B_flat[ToIndex(x, y)] = B_cache[y][x];
			}
		}
		#endregion

		#region Helper Methods
		#region ToString()
		/// <summary>
		/// Creates a detailed bordered table with custom value extraction.
		/// NOW CORRECTLY ACCOUNTS FOR X-AXIS COORDINATE WIDTH
		/// </summary>
		public string ToString<T1>(bool detailed = false, System.Func<T, T1> valFuncToStr = null)
		{
			// simple version >>
			if (detailed == false)
			{
				string str = "";
				for (int y = h - 1; y >= 0; y -= 1)
				{
					for (int x = 0; x < w; x += 1)
					{
						if (valFuncToStr != null)
							str += valFuncToStr(this[x, y]).ToString();
						else
							str += this[x, y];
					}
					str += '\n';
				}
				return str;
			}
			// << simple version

			if (valFuncToStr == null)
			{
				Debug.Log("ToString(bool, Func<T, string>) called with null function".colorTag("red"));
				return "error";
			}

			// Detailed version with borders and custom function
			var sb = new StringBuilder();

			// Calculate coordinate label widths
			int yLabelWidth = Math.Max(2, (h - 1).ToString().Length);
			int xCoordWidth = Math.Max(1, (w - 1).ToString().Length); // ← NEW: Width of x-coordinates

			// Determine cell width based on BOTH content AND x-coordinate labels
			int cellWidth = xCoordWidth; // ← Start with coordinate width as minimum
			for (int y = 0; y < h; y++)
			{
				for (int x = 0; x < w; x++)
				{
					string content = valFuncToStr(this[x, y]).ToString();
					cellWidth = Math.Max(cellWidth, content.Length);
				}
			}

			// Top border
			sb.Append(new string(' ', yLabelWidth + 1));
			sb.Append('┌');
			for (int x = 0; x < w; x++)
			{
				sb.Append(new string('─', cellWidth));
				sb.Append(x < w - 1 ? '┬' : '┐');
			}
			sb.AppendLine();

			// Rows (from top to bottom: y = h-1 down to 0)
			for (int y = h - 1; y >= 0; y--)
			{
				// Row with data
				sb.Append(y.ToString().PadLeft(yLabelWidth));
				sb.Append(' ');
				sb.Append('│');

				for (int x = 0; x < w; x++)
				{
					string content = valFuncToStr(this[x, y]).ToString();
					sb.Append(content.padCenter(cellWidth));
					sb.Append('│');
				}
				sb.AppendLine();

				// Separator row (except after last row)
				if (y > 0)
				{
					sb.Append(new string(' ', yLabelWidth + 1));
					sb.Append('├');
					for (int x = 0; x < w; x++)
					{
						sb.Append(new string('─', cellWidth));
						sb.Append(x < w - 1 ? '┼' : '┤');
					}
					sb.AppendLine();
				}
			}

			// Bottom border
			sb.Append(new string(' ', yLabelWidth + 1));
			sb.Append('└');
			for (int x = 0; x < w; x++)
			{
				sb.Append(new string('─', cellWidth));
				sb.Append(x < w - 1 ? '┴' : '┘');
			}
			sb.AppendLine();

			// X-axis labels - NOW PROPERLY CENTERED IN CELLS
			sb.Append(new string(' ', yLabelWidth + 1));
			sb.Append(' ');
			for (int x = 0; x < w; x++)
			{
				string xLabel = x.ToString();
				sb.Append(xLabel.padCenter(cellWidth)); // ← FIX: Center x-coord in cell
				sb.Append(' ');
			}
			sb.AppendLine();

			// Bottom decorative line
			sb.Append(new string(' ', yLabelWidth + 1));
			sb.Append('└');
			for (int x = 0; x < w; x++)
			{
				sb.Append(new string('─', cellWidth));
				sb.Append(x < w - 1 ? '┴' : '┘');
			}

			return sb.ToString();
		}

		/// <summary>
		/// Creates a compact character representation of the board with coordinates.
		/// <paramref name="valFuncToChar"/>: Function to convert each cell value to a character
		/// Example: board.ToString(val => val == TileType.empty ? '.' : '#')
		/// </summary>
		public string ToString(System.Func<T, char> valFuncToChar)
		{
			if (valFuncToChar == null)
			{
				Debug.Log("ToString(Func<T, char>) called with null function".colorTag("red"));
				return "error";
			}

			var sb = new StringBuilder();

			// Calculate coordinate label width (for y-axis)
			int yLabelWidth = Math.Max(2, (h - 1).ToString().Length);

			// Rows (from top to bottom: y = h-1 down to 0)
			for (int y = h - 1; y >= 0; y--)
			{
				// Y-axis label
				sb.Append(y.ToString().PadLeft(yLabelWidth));
				sb.Append(' ');

				// Row data
				for (int x = 0; x < w; x++)
				{
					char c = valFuncToChar(this[x, y]);
					sb.Append(c);
				}
				sb.AppendLine();
			}

			// X-axis labels
			sb.Append(new string(' ', yLabelWidth + 1));
			for (int x = 0; x < w; x++)
			{
				// Use modulo 10 for compact display
				sb.Append(x % 10);
			}

			return sb.ToString();
		}
		/// <summary>
		/// Creates a compact character representation with access to coordinates.
		/// <paramref name="valFuncToChar"/>: Function that takes (coord, value) and returns a character
		/// Example: board.ToString((coord, val) => coord == start ? 'S' : coord == end ? 'E' : '.')
		/// </summary>
		public string ToString(System.Func<v2, T, char> valFuncToChar)
		{
			if (valFuncToChar == null)
			{
				Debug.Log("ToString(Func<v2, T, char>) called with null function".colorTag("red"));
				return "";
			}

			var sb = new StringBuilder();

			// Calculate coordinate label width (for y-axis)
			int yLabelWidth = Math.Max(2, (h - 1).ToString().Length);

			// Rows (from top to bottom: y = h-1 down to 0)
			for (int y = h - 1; y >= 0; y--)
			{
				// Y-axis label
				sb.Append(y.ToString().PadLeft(yLabelWidth));
				sb.Append(' ');

				// Row data
				for (int x = 0; x < w; x++)
				{
					v2 coord = (x, y);
					char c = valFuncToChar(coord, this[coord]);
					sb.Append(c);
				}
				sb.AppendLine();
			}

			// X-axis labels
			sb.Append(new string(' ', yLabelWidth + 1));
			for (int x = 0; x < w; x++)
			{
				// Use modulo 10 for compact display
				sb.Append(x % 10);
			}

			return sb.ToString();
		}
		#endregion

		/// <summary>
		/// Apply a function to modify a cell value in-place
		/// Example: board.Modify((5, 3), val => val + 1);
		/// </summary>
		public void Modify(v2 coord, System.Func<T, T> modifier)
		{
			this[coord] = modifier(this[coord]);
		}

		/// <summary>
		/// Apply a function to all cells
		/// Example: board.ForEach((x, y, val) => Debug.Log($"{x},{y}: {val}"));
		/// </summary>
		public void ForEach(System.Action<int, int, T> action)
		{
			for (int y = 0; y < h; y++)
				for (int x = 0; x < w; x++)
					action(x, y, B_flat[ToIndex(x, y)]);
		}
		#endregion

		#region Clone
		/// <summary>
		/// Creates a deep clone of the board.
		/// For value types (int, char, etc.): Direct copy
		/// For reference types (classes): Uses IClonable or serialization-based deep copy
		/// </summary>
		public Board<T> clone
		{
			get
			{
				Board<T> new_B = new Board<T>((this.w, this.h), default(T));

				// Check if T is a value type or string (immutable)
				if (typeof(T).IsValueType || typeof(T) == typeof(string))
				{
					// Simple copy for value types
					for (int i = 0; i < this.B_flat.Length; i++)
						new_B.B_flat[i] = this.B_flat[i];
				}
				else
				{
					// Deep copy for reference types
					for (int i = 0; i < this.B_flat.Length; i++)
					{
						T original = this.B_flat[i];

						if (original == null)
						{
							new_B.B_flat[i] = default(T);
						}
						else if (original is System.ICloneable cloneable)
						{
							// If implements ICloneable, use it
							new_B.B_flat[i] = (T)cloneable.Clone();
						}
						else
						{
							// Fallback: JSON-based deep copy (works for [Serializable] classes)
							string json = JsonUtility.ToJson(original);
							new_B.B_flat[i] = JsonUtility.FromJson<T>(json);
						}
					}
				}

				return new_B;
			}
		}
		#endregion
	}
	#endregion

	#region v3
	// creation of v3:
	// v3 a = new v3(0, 0, 0)
	// v3 b = (1, 2, 1)
	[System.Serializable]
	public struct v3
	{
		public int x, y, z;

		public v3(int x, int y, int z) { this.x = x; this.y = y; this.z = z; }
		public override string ToString()
		{
			return $"({x}, {y}, {z})";
			//return base.ToString();
		}
		#region arithmatic operator
		public static v3 operator +(v3 a, v3 b) { return new v3(a.x + b.x, a.y + b.y, a.z + b.z); }
		public static v3 operator -(v3 a, v3 b) { return new v3(a.x - b.x, a.y - b.y, a.z - b.z); }
		public static v3 operator *(v3 a, v3 b) { return new v3(a.x * b.x, a.y * b.y, a.z * b.z); }
		public static v3 operator *(v3 v, int m) { return new v3(v.x * m, v.y * m, v.z * m); }
		public static v3 operator *(int m, v3 v) { return v * m; }

		public static bool operator ==(v3 a, v3 b) { return a.x == b.x && a.y == b.y && a.z == b.z; }
		public static bool operator !=(v3 a, v3 b) { return a.x != b.x || a.y != b.y || a.z != b.z; }

		public static bool operator >(v3 a, v3 b) => a.x > b.x && a.y > b.y && a.z > b.z;
		public static bool operator <(v3 a, v3 b) => a.x < b.x && a.y < b.y && a.z < b.z;
		public static bool operator >=(v3 a, v3 b) => a.x >= b.x && a.y >= b.y && a.z >= b.z;
		public static bool operator <=(v3 a, v3 b) => a.x <= b.x && a.y <= b.y && a.z <= b.z;
		#endregion

		#region ad implicit conversion
		// Allow implicit conversion from tuple
		public static implicit operator v3((int, int, int) tuple) => new v3(tuple.Item1, tuple.Item2, tuple.Item3);
		#endregion

		#region getDIR(bool)
		/// <summary>
		/// get List of v3 in all 4 direction, with (optional) if diagonal required.
		/// </summary>
		/// <param name="includeDiagonal"></param>
		/// <returns></returns>
		public static List<v3> getDIR(bool includeDiagonal = false, bool randomOrder = false)
		{
			List<v3> DIR = new List<v3>();
			// TODO
			return DIR;
		}

		/// <summary>
		/// get dir based on <paramref name="dirStr"/> name,
		/// example: "r" = (+1, 0, 0), "ru" or "ur" = (+1, +1, 0) or "fr" = (+1, 0, +1) 
		/// </summary>
		public static v3 getdirFromName(string dirStr = "r")
		{
			v3 dir = (0, 0, 0);
			foreach (char _char in dirStr)
			{
				if (_char == 'r') dir += (+1, 0, 0);
				if (_char == 'u') dir += (0, +1, 0);
				if (_char == 'l') dir += (-1, 0, 0);
				if (_char == 'd') dir += (0, -1, 0);
				if (_char == 'f') dir += (0, 0, +1);
				if (_char == 'b') dir += (0, 0, -1);
			}
			return dir;
		}

		// constant
		public static v3 right = (+1, 0, 0),
							up = (0, +1, 0),
						  left = (-1, 0, 0),
						  down = (0, -1, 0),
						   fwd = (0, 0, +1),
						  back = (0, 0, -1);
		#endregion

		#region ad: v2, vec3, vec2 conversion (requires casting to form extension)
		public static implicit operator v3(v2 _v2) { return new v3(_v2.x, _v2.y, 0); }
		public static implicit operator v3(Vector3 vec3) { return new v3(C.round(vec3.x), C.round(vec3.y), C.round(vec3.z)); }
		public static implicit operator v3(Vector2 vec2) { return new v3(C.round(vec2.x), C.round(vec2.y), 0); }
		public static implicit operator v2(v3 @this) { return new v2(@this.x, @this.y); }
		public static implicit operator Vector3(v3 @this) { return new Vector3(@this.x, @this.y, @this.z); }
		public static implicit operator Vector2(v3 @this) { return new Vector2(@this.x, @this.y); }
		#endregion
	}

	public static class Extension_v2_v3
	{
		#region dot, area -> v2
		public static float dot(this v2 a, v2 b) { return a.x * b.x + a.y * b.y; }
		public static float area(this v2 a, v2 b) { return a.x * b.y - a.y * b.x; }
		#endregion

		#region dot, cross -> v3
		/// <summary>
		/// 
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns>projection of a on b</returns>
		public static float dot(this v3 a, v3 b) { return a.x * b.x + a.y * b.y + a.z * b.z; }
		/// <summary>
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns>the normal vector, using left-hand rule</returns>
		public static Vector3 cross(this v3 a, v3 b) { return Vector3.Cross(a, b); }
		#endregion

		#region magnitude, sqrMagnitude
		// main logic
		public static float sqrMag(this v3 v){return v.x.pow(2) + v.y.pow(2) + v.z.pow(2);}
		public static float mag(this v3 v){return v.sqrMag().pow(0.5f);} // depend on sqrMag
		public static float mag(this v2 v){ return ((v3)v).mag(); }
		public static float sqrMag(this v2 v){ return ((v3)v).sqrMag(); }
		#endregion
	}
	#endregion

	#region MonoInterfaceFinder For Modular Interface MonoBehaviour Approach
	public static class MonoInterfaceFinder
	{
		// better approach example without MonoBehaviour: 
		/* public interface ILogger
		{
			void AddLog(string str, string syntaxType = "");
			void SaveGameData(object dataType, string jsonContent);
		}

		public class LOG : ILogger
		{
			private static LOG _instance;
			public static LOG Instance => _instance ??= new LOG();

			// Private constructor prevents external instantiation
			private LOG() { }

			// Now you can use both:
			// 1. Static access: LOG.Instance.AddLog(...)
			// 2. Interface finding: var logger = MonoInterfaceFinder.FindInterface<ILogger>();
    
			public void AddLog(string str, string syntaxType = "")
			{
				// ... implementation
			}

			public void SaveGameData(object dataType, string jsonContent)
			{
				// ... implementation
			}
		}
		*/

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
		// vec3, float >>
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
		// << vec3, float
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
	public static partial class C
	{
		public static void Init()
		{
			string name = "prefabHolder";
			if (GameObject.Find(name) != null)
				GameObject.Destroy(GameObject.Find(name));
			prefabHolder = new GameObject(name).transform;
		}
		public static Transform prefabHolder;

		#region CONSTANT
		public static double e = 1e-2;
		public static float pi = Mathf.PI;
		#region colorStr
		public static class colorStr
		{
			// --- Whitish / Light Colors ---
			public const string white = "#FFFFFF"; // keep pure
			public const string silver = "#C8C8C8"; // softened from C0C0C0
			public const string lightblue = "#7EC9F8"; // softer, less neon
			public const string aqua = "#4FEFFB"; // reduced sharpness
			public const string cyan = "#4FEFFB";

			// --- Cool / Blue-Green Colors ---
			public const string darkblue = "#0A1A6F"; // deeper, calmer
			public const string navy = "#0F2740"; // improved readability
			public const string teal = "#26B7A3"; // less harsh than 1ABC9C
			public const string blue = "#4EA3FF"; // softened from 3498FF

			// warning
			public const string yellow = "#FFE55C"; // warmer, softer than FF EA 00

			// --- Greenish / Yellowish Colors ---
			public const string lime = "#6DFF6D"; // softer lime, still bright
			public const string olive = "#AFC265"; // easier on eyes
			public const string green = "#44FF44"; // softer, less pure neon

			// exit case
			public const string orange = "#FF9C33"; // friendlier orange

			// --- Warm / Reddish / Orange Colors ---
			public const string brown = "#B07149"; // warmer + more readable
			public const string maroon = "#C05474"; // softened, easier on eyes
			public const string red = "#FF5C5C"; // less aggressive than FF4444
			public const string purple = "#C48BFF"; // smoother purple
			public const string fuchsia = "#FF66FF"; // softened magenta
			public const string magenta = "#FF66FF";
		}
		#endregion
		#endregion

		#region float, vec3 operations
		public static float clamp(this float x, float min, float max, double de = 0f)
		{
			if (x > max) return max - (float)de;
			if (x < min) return min + (float)de;
			return x;
		}
		public static Vector3 clamp(this Vector3 v, Vector3 min, Vector3 max, double de = 0f)
		{
			return new Vector3()
			{
				x = C.clamp(v.x, min.x, max.x, de),
				y = C.clamp(v.y, min.y, max.y, de),
				z = C.clamp(v.z, min.z, max.z, de),
			};
		}

		public static int round(this float x)
		{
			if (x > 0f)
			{
				int xI = (int)x;
				float frac = x - xI;
				if (frac > 0.5f)
					return xI + 1;
				else return xI;

			}
			else if (x < 0f)
			{
				int xI = (int)x;
				float frac = x - xI;
				if (frac < -0.5f)
					return xI - 1;
				else return xI;
			}
			return 0;
		}
		public static Vector3 round(this Vector3 v)
		{
			return new Vector3()
			{
				x = C.round(v.x),
				y = C.round(v.y),
				z = C.round(v.z),
			};
		}
		public static float roundDecimal(this float x, int digits = 1)
		{
			float newX = (x * 10.pow(digits)).round();
			return newX * 1f / (10.pow(digits)); // int * float
		} // floating inaccuracy still exist, roundDecimal useful just as a string

		public static int floor(this float x)
		{
			return Mathf.FloorToInt(x);
		}
		public static int ceil(this float x)
		{
			return Mathf.CeilToInt(x);
		}
		public static int sign(this float x, double e = 1e-3)
		{
			if (x <= e && x >= -e)
				return 0;

			if (x > e) return +1;
			else return -1;
		}
		public static float pow(this float x, float exp = 2)
		{
			return Mathf.Pow(x, exp);
		}
		public static int pow(this int x, int exp = 2)
		{
			return ((float)x).pow().round();
		}
		/// <summary>
		/// return: (x.abs().pow(exp)) * x.sign()
		/// </summary>
		public static float powSign(this float x, float exp = 2)
		{
			return Mathf.Pow(Mathf.Abs(x), exp) * x.sign();
		}
		public static int powSign(this int x, int exp = 2)
		{
			return ((float)x).powSign().round();
		}
		public static int mod(this int i, int length, int offset = 0)
		{
			int new_i = (i + offset) % length;
			if (new_i < 0) new_i += length;
			return new_i;
		}

		// less than 0.001f considered as zero
		public static float approxZero(this float x, double e = 1e-3)
		{
			if (x < e) return 0f;
			return x;
		}
		public static Vector3 approxZero(this Vector3 v, double e = 1e-3)
		{
			return new Vector3(approxZero(v.x, e), approxZero(v.y, e), approxZero(v.z, e));
		}

		public static bool zero(this float x, double e = 1e-4)
		{
			return Mathf.Abs(x) < e;
		}
		public static bool zero(this Vector3 v, double e = 1e-4)
		{
			return zero(v.x, e) && zero(v.y, e) && zero(v.z, e);
		}

		public static float abs(this float x) { return Mathf.Abs(x); }
		public static int abs(this int x) { return Mathf.RoundToInt(Mathf.Abs(x)); }
		public static Vector3 abs(this Vector3 vec3) { return new Vector3(Mathf.Abs(vec3.x), Mathf.Abs(vec3.y), Mathf.Abs(vec3.z)); }
		public static Vector2 abs(this Vector2 vec2) { return new Vector2(Mathf.Abs(vec2.x), Mathf.Abs(vec2.y)); }
		public static v2 abs(this v2 _v2) { return (abs(_v2.x), abs(_v2.y)); }

		public static bool inRange(this float x, float m, float M)
		{
			return x >= m && x <= M;
		}
		public static bool inRange(this Vector3 v, Vector3 m, Vector3 M)
		{
			return C.inRange(v.x, m.x, M.x) &&
					C.inRange(v.y, m.y, M.y) &&
					C.inRange(v.z, m.z, M.z);
		}
		public static bool inRange(this Vector2 v, Vector2 m, Vector2 M)
		{
			return C.inRange(v.x, m.x, M.x) &&
					C.inRange(v.y, m.y, M.y);
		}
		/// <summary>
		/// start from m, end at M(including M)
		/// </summary>
		/// <param name="v"></param>
		/// <param name="m"></param>
		/// <param name="M"></param>
		/// <returns></returns>
		public static bool inRange(this v2 v, v2 m, v2 M)
		{
			return C.inRange(v.x, m.x, M.x) &&
					C.inRange(v.y, m.y, M.y);
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

		#region quaternion operations
		/// <summary>
		/// Usage: transform.localRotation = transform.localRotation.rotateAndClamp(axis, deltaAngle, min, max);
		/// Example: blade.localRotation = blade.localRotation.rotateAndClamp(Vector3.right, deltaAngle: 10 * Time.deltaTime, -30f, 30f);
		/// </summary>
		/// still got the gimbal lock goin on:
		/// better appraoch to clamp the angle via float, than .rotation = Quaternion.Euler(axis * angle)
		//public static Quaternion rotateAndClamp(this Quaternion currentRotation, Vector3 localAxis, float deltaAngle, float minAngle, float maxAngle)
		//{
		//	// Extract current angle around the axis
		//	Vector3 eulerAngles = currentRotation.eulerAngles;

		//	// Determine which component to modify based on the dominant axis
		//	if (Mathf.Abs(localAxis.x) > 0.5f)
		//	{
		//		float angle = normalizeAngle(eulerAngles.x);
		//		angle = Mathf.Clamp(angle + deltaAngle, minAngle, maxAngle);
		//		eulerAngles.x = angle;
		//	}
		//	else if (Mathf.Abs(localAxis.y) > 0.5f)
		//	{
		//		float angle = normalizeAngle(eulerAngles.y);
		//		angle = Mathf.Clamp(angle + deltaAngle, minAngle, maxAngle);
		//		eulerAngles.y = angle;
		//	}
		//	else if (Mathf.Abs(localAxis.z) > 0.5f)
		//	{
		//		float angle = normalizeAngle(eulerAngles.z);
		//		angle = Mathf.Clamp(angle + deltaAngle, minAngle, maxAngle);
		//		eulerAngles.z = angle;
		//	}

		//	return Quaternion.Euler(eulerAngles);
		//}
		//static float normalizeAngle(float angle)
		//{
		//	angle = angle % 360f;
		//	if (angle > 180f) angle -= 360f;
		//	return angle;
		//}

		#endregion

		// TODO: str.fuzzy("cube") extension, (might be similar to str.Contains("somthng"); built-in)
		#region string operations
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
		public static string roundDecimalStr(float val, int digits = 2)
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
			return str.getIntFirstMatch();
		}
		public static long parseLong(this string str)
		{
			return str.getLongFirstMatch();
		}
		public static string join<T>(this IEnumerable<T> list, char separator)
		{
			return string.Join(separator.ToString(), list);
		}
		// ad essential >>
		static RegexOptions strToFlags(string flags)
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
		public static string esc(string str)
		{
			return Regex.Escape(str);
		}
		public static string repeat(this char chr, int count = 100)
		{ return new string(chr, count); }
		public static string join(this IEnumerable<string> STR, string sep = ", ")
		{
			return string.Join(sep, STR);
		}
		/// <summary>
		/// Centers a string with optional spacing, padding with the specified character.
		/// Example: "hello".padCenter(17, '=', addSpacing: true) → "===== hello ====="
		/// </summary>
		public static string padCenter(this string str, int totalWidth, char paddingChar = ' ', bool addSpacing = true)
		{
			if (str == null)
				str = "";

			if (str.Length >= totalWidth)
				return str;

			// Add spacing around the string if requested
			if (addSpacing && paddingChar != ' ')
			{
				str = " " + str + " ";
				if (str.Length >= totalWidth)
					return str;
			}

			int totalPadding = totalWidth - str.Length;
			int leftPadding = totalPadding / 2;
			int rightPadding = totalPadding - leftPadding;

			return new string(paddingChar, leftPadding) + str + new string(paddingChar, rightPadding);
		}
		public static string padLeft(this string str, int totalWidth, char paddingChar = ' ', bool addSpacing = false)
		{
			if (str == null)
				str = "";

			if (str.Length >= totalWidth)
				return str;

			// Add spacing before the string if requested
			if (addSpacing && paddingChar != ' ')
			{
				str = " " + str;
				if (str.Length >= totalWidth)
					return str;
			}

			int totalPadding = totalWidth - str.Length;
			return new string(paddingChar, totalPadding) + str;
		}
		public static string padRight(this string str, int totalWidth, char paddingChar = ' ', bool addSpacing = false)
		{
			if (str == null)
				str = "";

			if (str.Length >= totalWidth)
				return str;

			// Add spacing after the string if requested
			if (addSpacing && paddingChar != ' ')
			{
				str = str + " ";
				if (str.Length >= totalWidth)
					return str;
			}

			int totalPadding = totalWidth - str.Length;
			return str + new string(paddingChar, totalPadding);
		}
		// << ad essential

		/// <summary>
		/// Splits <paramref name="str"/> on the regex <paramref name="re"/>
		/// Example: "A -> B\n\nC".split(@"\n\n", "gm") ⇒ ["A -> B", "C"]
		/// </summary>
		/// regular expression explicit match approach
		public static IEnumerable<string> split(this string str, string re, string flags = "gx")
		{
			if (str == null) return null;

			// Always include ExplicitCapture by default for split
			return Regex.Split(str.clean(), re, strToFlags(flags));
		}
		/// <summary>
		/// Returns all substrings of <paramref name="str"/> that match the regex <paramref name="re"/>.
		/// Example: "A -> B, X -> Y".match(@"\w\s*->\s*\w", "gmi") ⇒ [ "A -> B", "X -> Y" ]
		/// </summary>
		public static IEnumerable<string> allMatch(this string str, string re, string flags = "gmi")
		{
			if (str == null)
				return null;

			// Always include global, multiline capture by default for split
			var matches = Regex.Matches(str.clean(), re, strToFlags(flags));
			if (matches.Count == 0) return Array.Empty<string>();

			return matches
				.Cast<Match>()
				.Select(m => m.Value);
		}
		/// <summary>
		/// Returns weather there is a pattern somewhere in <paramref name="str"/> that match the regex <paramref name="re"/> entirely.
		/// Eg: 'A'.match(@"^[a-g]$", "gmi") ⇒ true,
		/// Eg: "TMP text field".match(@"text", "gmi") ⇒ true,
		/// </summary>
		public static bool isAnyMatch(this string str, string re, string flags = "gmi")
		{
			return Regex.IsMatch(str, re, strToFlags(flags));
			// return str.match(re, flags).Count() > 0;
		}
		public static bool anyMatch(this char chr, string re, string flags = "gmi")
		{
			return chr.ToString().isAnyMatch(re, flags);
		}
		/// <summary>
		/// returns first integer match via str.allMatch(re: @"\d+", flags: "gmi")
		/// </summary>
		/// <param name="str"></param>
		/// <param name="flags"></param>
		/// <returns></returns>
		public static int getIntFirstMatch(this string str, string flags = "gmi")
		{
			return int.Parse(str.allMatch(@"([+-])?\d+", flags: flags).getAt(0));
		}
		public static long getLongFirstMatch(this string str, string flags = "gmi")
		{
			return long.Parse(str.allMatch(@"([+-])?\d+", flags: flags).getAt(0));
		}

		/// <summary>
		/// Replaces all occurrences of the regex pattern <paramref name="re"/> with <paramref name="insert"/>
		/// Example: "Hello world123 test456".replace(@"\d+", "X", "gm") ⇒ "Hello worldX testX"
		/// </summary>
		public static string replace(this string str, string re, string insert, string flags = "gm")
		{
			if (str == null)
				return null;

			// default flags: "gm"
			return Regex.Replace(str.clean(), re, insert, strToFlags(flags));
		}
		#endregion

		#region string extension for Debug.Log
		/*
			str.colorTag
				red: critical error
				yellow: not so critical error
				orange: exit events
				lime: success root message inside a certain method
				cyan: any other crucial message inside a method
				gray: default Debug.Log color

				white: when entering a certain method
		*/

		/// <summary>
		/// Wraps string in Unity Rich Text color tags.
		/// Usage: "Hello".colorTag("red") → "&lt;color=red&gt;Hello&lt;/color&gt;"
		/// </summary>
		public static string colorTag(this string str, string color = "red")
		{
			return $"<color={color}>{str}</color>";
		}

		/// <summary>
		/// Automatically gets caller's class and method name with built-in color.
		/// C.method(this);
		/// C.method(this, "white");
		/// C.method("red", $"error somthng occured");
		/// </summary>
		// Object: includes GameObject, Component kind
		public static string method(
			UnityEngine.Object unityObject,
			string color = "white",
			string adMssg = "",
			[System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
			[System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "")
		{
			string fileName = System.IO.Path.GetFileNameWithoutExtension(sourceFilePath);
			string className = "Unknown";

			object obj = (object)unityObject;
			if (obj != null)
			{
				// Handle both Type objects (from typeof) and instance objects
				if (obj is Type type)
				{
					// Static class case: typeof(LOG) or typeof(LOG.SysInfo)
					// Use FullName to get nested path, then remove namespace
					className = type.FullName ?? type.Name;

					// Remove namespace prefix (everything before last dot that's not part of nested class)
					// e.g., "SPACE_UTIL.LOG+SysInfo" -> "LOG.SysInfo"
					if (className.Contains("."))
					{
						int lastNamespaceIndex = className.LastIndexOf('.');
						string classPart = className.Substring(lastNamespaceIndex + 1);

						// Replace '+' with '.' for nested classes
						classPart = classPart.Replace('+', '.');
						className = classPart;
					}
					else
					{
						// No namespace, just replace '+' with '.'
						className = className.Replace('+', '.');
					}
				}
				else
				{
					// Instance case: this
					Type instanceType = obj.GetType();
					className = instanceType.FullName ?? instanceType.Name;

					// Same namespace removal logic
					if (className.Contains("."))
					{
						int lastNamespaceIndex = className.LastIndexOf('.');
						string classPart = className.Substring(lastNamespaceIndex + 1);
						classPart = classPart.Replace('+', '.');
						className = classPart;
					}
					else
					{
						className = className.Replace('+', '.');
					}
				}
			}
			else
			{
				// Auto-detect calling class using StackTrace
				try
				{
					var stackTrace = new System.Diagnostics.StackTrace();
					// Frame 0 = this method (C.method)
					// Frame 1 = the method that called C.method (e.g., LOG.AddLog)
					var callingFrame = stackTrace.GetFrame(1);
					if (callingFrame != null)
					{
						var callingMethod = callingFrame.GetMethod();
						if (callingMethod != null && callingMethod.DeclaringType != null)
						{
							Type declaringType = callingMethod.DeclaringType;
							className = declaringType.FullName ?? declaringType.Name;

							// Remove namespace
							if (className.Contains("."))
							{
								int lastNamespaceIndex = className.LastIndexOf('.');
								string classPart = className.Substring(lastNamespaceIndex + 1);
								classPart = classPart.Replace('+', '.');
								className = classPart;
							}
							else
							{
								className = className.Replace('+', '.');
							}
						}
					}
				}
				catch
				{
					// Fallback to Unknown if stack trace fails
					className = "Unknown";
				}
			}

			if (obj == null)
				return $"{memberName}() -> class: {className} -> file: {fileName}.cs -> // {adMssg} //".colorTag(color);
			else
				return $"{memberName}() -> class: {className} -> obj: {obj.ToString()} -> file: {fileName}.cs -> // {adMssg} //".colorTag(color);
		}

		// won't' work rather provide the Object null as parameter(instead of parameter default) to announce its location.
		/*
		/// <summary>
		/// Automatically gets caller's class and method name with built-in color.
		/// C.method(this);
		/// C.method(this, "white");
		/// C.method("red", $"error somthng occured");
		/// </summary>
		/// 
		public static string method(
			string color = "lime",
			string adMssg = "",
			[System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
			[System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "")
		{
			return method(unityObject: null, color, adMssg);
		}
		*/
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

		#region Anim Asyc, IEnumerator
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

		#region UI util
		public static void setBtnTxt(this Button btn, string str)
		{
			btn.transform.GetChild(0).gameObject.gc<TextMeshProUGUI>().text = str;
		}
		#endregion

		#region INFO
		public static class SysInfo
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

		public static class UnityInfo
		{
			public static string UnityLifeCycle = @"==== Unity Life Cycle ====
INITIALIZATION PHASE
├─ Awake()           → Called when script instance is loaded (even if disabled)
├─ OnEnable()        → Called when object/component becomes enabled
└─ Start()           → Called before first frame update (only if enabled)

PHYSICS LOOP (Fixed Timestep - Default 0.02s)
├─ FixedUpdate()     → Called at fixed intervals for physics
└─ Internal Physics Update

GAME LOOP (Per Frame)
├─ Update()          → Called once per frame (main logic)
├─ LateUpdate()      → Called after all Updates (cameras, follow systems)
├─ Animation Events  → Triggered during animation playback
└─ Animation Rigging → IK and rig calculations

RENDERING
├─ OnWillRenderObject()
├─ OnPreCull()
├─ OnBecameVisible() / OnBecameInvisible()
├─ OnPreRender()
├─ OnRenderObject()
└─ OnPostRender()

COLLISION/TRIGGER DETECTION
├─ OnCollisionEnter/Stay/Exit()
├─ OnTriggerEnter/Stay/Exit()
├─ OnCollisionEnter2D/Stay2D/Exit2D()
└─ OnTriggerEnter2D/Stay2D/Exit2D()

DEINITIALIZATION PHASE
├─ OnDisable()       → Called when object/component becomes disabled
├─ OnDestroy()       → Called when object is destroyed
└─ OnApplicationQuit() → Global, called before application quits
==== Unity Life Cycle ====";
		}
		#endregion

	}
	// loop, example: while (C.Safe(1e3, "loopA"))
	public static partial class C
	{
		private static Dictionary<string, int> MAP_safeCounters = new Dictionary<string, int>();

		/// <summary>
		/// Safe loop guard with automatic iteration tracking.
		/// Usage: while (C.Safe(1e3, "myLoop")) { ... }
		/// Returns false when limit exceeded.
		/// </summary>
		public static bool Safe(double limit = 1e3, string id = "loop",
			[System.Runtime.CompilerServices.CallerMemberName] string caller = "",
			[System.Runtime.CompilerServices.CallerLineNumber] int line = 0)
		{
			string key = $"{caller}:{line}:{id}";

			if (!MAP_safeCounters.ContainsKey(key))
				MAP_safeCounters[key] = 0;

			MAP_safeCounters[key]++;

			if (MAP_safeCounters[key] > limit)
			{
				Debug.Log($"[C.Safe] Loop '{id}' at {caller}() line:{line} exceeded {limit} iterations".colorTag("orange"));
				MAP_safeCounters.Remove(key);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Reset a specific safe loop counter.
		/// Usage: C.SafeReset("myLoop");
		/// </summary>
		public static void SafeReset(string id = "loop",
			[System.Runtime.CompilerServices.CallerMemberName] string caller = "",
			[System.Runtime.CompilerServices.CallerLineNumber] int line = 0)
		{
			string key = $"{caller}:{line}:{id}";
			MAP_safeCounters.Remove(key);
		}

		/// <summary>
		/// Clear ALL safe loop counters (useful for level transitions, restarts).
		/// Usage: C.SafeClear();
		/// </summary>
		public static void SafeClearAll()
		{
			int count = MAP_safeCounters.Count;
			MAP_safeCounters.Clear();
			Debug.Log($"[C.SafeClear] Cleared {count} safe loop counter(s)".colorTag("lime"));
		}

		/// <summary>
		/// Get diagnostic info about active safe loop counters.
		/// Usage: Debug.Log(C.SafeInfo());
		/// </summary>
		public static string SafeInfo()
		{
			if (MAP_safeCounters.Count == 0)
				return "[C.Safe] No active loop counters";

			var sb = new StringBuilder();
			sb.AppendLine($"[C.Safe] Active loop counters: {MAP_safeCounters.Count}");

			foreach (var kvp in MAP_safeCounters.OrderByDescending(x => x.Value))
			{
				string[] parts = kvp.Key.Split(':');
				string method = parts.Length > 0 ? parts[0] : "?";
				string line = parts.Length > 1 ? parts[1] : "?";
				string id = parts.Length > 2 ? parts[2] : "?";

				sb.AppendLine($"  {method}():{line} '{id}' = {kvp.Value} iterations");
			}
			return sb.ToString();
		}
	}
	// TODO, C.Ease along with tween

	public static class U
	{
		/*
			- Find Non Collision Spot 2D, 3D
		*/
		#region ad CanPlaceObject ? at a give pos, _prefab.collider, rotationY
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
		#region minMax(func, bool), find(func), findIndex(func), forEach(func), map(func), getAt, getAtLast
		/// <summary>
		/// to get min: (a) => float; the a with least float is returned
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list"></param>
		/// <param name="cmpFunc"></param>
		/// <param name="splice"></param>
		/// <returns></returns>
		public static T minMaxA<T>(this List<T> list, Func<T, float> cmpFunc, bool splice = false)
		{
			if (list.Count < 1)
			{
				Debug.Log("minMaxA require atleast Count Of 1".colorTag("red"));
				throw new ArgumentException();
			}
			int minIndex = 0;
			for (int i0 = 0; i0 < list.Count; i0 += 1)
				if (cmpFunc(list[i0]) < cmpFunc(list[minIndex]))
					minIndex = i0;
			var min = list[minIndex];
			if (splice)
				list.RemoveAt(minIndex);
			return min;
			/*
			int minIndex = 0;
			for (int i0 = 1; i0 < T_1D.Count; i0 += 1)
				if (cmp_func(T_1D[i0], T_1D[minIndex]) < 0f) // if ( b - a ) < 0f, than a < b, so swap
					minIndex = i0;

			return min;
			*/
		}
		/// <summary>
		/// to get min (a, b) => float; (the pair with least float is returned)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list"></param>
		/// <param name="cmpFunc"></param>
		/// <param name="splice"></param>
		/// <returns></returns>
		public static List<T> minMaxAB<T>(this List<T> list, Func<T, T, double> cmpFunc, bool splice = false)
		{
			if (list.Count < 2)
			{
				Debug.Log("minMaxAB require atleast 2 elements to compare");
				throw new ArgumentNullException();
			}

			int[] minIndexPair = new int[] { 0, 1 };
			for (int i0 = 0; i0 <= list.Count - 2; i0 += 1)
				for (int i1 = i0 + 1; i1 <= list.Count - 1; i1 += 1)
				{
					if (cmpFunc(list[i0], list[i1]) < cmpFunc(list[minIndexPair[0]], list[minIndexPair[1]])) // smaller connection than before
					{
						minIndexPair[0] = i0;
						minIndexPair[1] = i1;
					}
				}
			var minAB = new List<T>() { list[minIndexPair[0]], list[minIndexPair[1]] }; ;
			if (splice)
			{
				if (minIndexPair[1] > minIndexPair[0])
				{
					list.RemoveAt(minIndexPair[1]);
					list.RemoveAt(minIndexPair[0]);

				}
				else
				{
					list.RemoveAt(minIndexPair[0]);
					list.RemoveAt(minIndexPair[1]);
				}
			}
			return minAB;
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

		/// <summary>
		/// get an elem with index relative to last, 0 for last, 1 for 1th from last etc
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="collection"></param>
		/// <param name="indexFromLast"></param>
		/// <returns></returns>
		public static T getAtLast<T>(this IEnumerable<T> collection, int indexFromLast)
		{
			if (collection == null) throw new ArgumentNullException(nameof(collection)); // nameof(collection) = "collection"

			if (indexFromLast < 0 || indexFromLast >= collection.Count())
				Debug.LogError($"in {nameof(collection)} gl{indexFromLast} > count: {collection.Count()}");

			return collection.ElementAt(collection.Count() - 1 - indexFromLast);
		}

		/// <summary>
		/// Index accessor for IEnumerable (like JavaScript arrays).
		/// Usage: items.getAt(1) instead of items.ToList()[1]
		/// </summary>
		public static T getAt<T>(this IEnumerable<T> source, int index)
		{
			if (index < 0)
			{
				var list = source as IList<T> ?? source.ToList();
				index += list.Count;
				if (index < 0) throw new IndexOutOfRangeException();
				return list[index];
			}
			return source.ElementAtOrDefault(index);
		}

		/* why not get<T>
		/// <summary>
		/// Makes IEnumerable act like an array with [] indexing.
		/// Usage: IN.split("\n")[0] instead of IN.split("\n").ToList()[0]
		/// WARNING: Creates a list internally - use sparingly!
		/// </summary>
		public static T get<T>(this IEnumerable<T> source, int index)
		{
			var list = source as IList<T> ?? source.ToList();
			return list[index];
		}
		*/

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

		public static List<int> ITER_1D;
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

	#region Extension

	/// <summary>
	/// Search flags for name matching (can be combined with | operator)
	/// </summary>
	[Flags]
	public enum HierarchyFlags
	{
		/// <summary>Default: case-insensitive AND search, match anywhere in name</summary>
		None = 0,
		/// <summary>Match case-sensitively (default is case-insensitive)</summary>
		CaseSensitive = 1 << 0,
		/// <summary>Use OR logic instead of AND (any term matches vs all terms match)</summary>
		Or = 1 << 1,
		/// <summary>Only match whole words separated by space/underscore/dash</summary>
		WholeWord = 1 << 2
	}
	public static class ExtensionHierarchyQuery
	{

		/// <summary>
		/// Fluent API for querying Unity GameObjects in the hierarchy.
		/// Usage: gameObject.Q().downNamed("door", "trigger").deepDown<Rigidbody>().where(go => go.activeSelf).all()
		/// Multi-term search: gameObject.Q().downNamed(false, "cube", "red", "trigger").gf()
		/// </summary>
		public class HierarchyQuery
		{
			private GameObject root;
			private List<GameObject> results;
			private bool hasExecutedQuery;

			public HierarchyQuery(GameObject root)
			{
				if (root == null)
				{
					Debug.Log("[HierarchyQuery] Root GameObject is null".colorTag("red"));
					this.root = null;
					this.results = new List<GameObject>();
					this.hasExecutedQuery = true;
					return;
				}

				this.root = root;
				this.results = new List<GameObject>();
				this.hasExecutedQuery = false;
			}

			#region Direct Component Retrieval - Descendants/Ancestors

			/// <summary>
			/// Finds the first descendant component T at shallowest depth (BFS).
			/// Equivalent to: down<T>().gf<T>()
			/// </summary>
			public T downCompoGf<T>() where T : Component
			{
				return down<T>().gf<T>();
			}

			/// <summary>
			/// Finds all descendant components T (DFS).
			/// Equivalent to: deepDown<T>().all<T>()
			/// </summary>
			public List<T> deepDownCompoAll<T>() where T : Component
			{
				return deepDown<T>().all<T>();
			}

			/// <summary>
			/// Finds the first ancestor component T.
			/// Equivalent to: up<T>().gf<T>()
			/// </summary>
			public T upCompoGf<T>() where T : Component
			{
				return up<T>().gf<T>();
			}

			/// <summary>
			/// Finds all ancestor components T.
			/// Equivalent to: deepUp<T>().all<T>()
			/// </summary>
			public List<T> deepUpCompoAll<T>() where T : Component
			{
				return deepUp<T>().all<T>();
			}

			#endregion

			#region Query Methods - Descendants

			/// <summary>
			/// Finds the first descendant (shallowest depth, BFS) whose name matches.
			/// <paramref name="flags"/>: Combine flags with | operator (e.g. SearchFlags.Or | SearchFlags.WholeWord)
			/// - Default (None): case-insensitive, AND logic, match anywhere
			/// - CaseSensitive: Enable case-sensitive matching
			/// - Or: Use OR logic (any term matches)
			/// - WholeWord: Only match words separated by space/underscore/dash
			/// Usage: downNamed("cube", "red") or downNamed(SearchFlags.Or | SearchFlags.WholeWord, "player", "enemy")
			/// </summary>
			public HierarchyQuery downNamed(HierarchyFlags flags, params string[] names)
			{
				if (root == null) return this;

				hasExecutedQuery = true;
				results.Clear();

				// BFS to find shallowest match
				var queue = new Queue<Transform>();
				for (int i = 0; i < root.transform.childCount; i++)
					queue.Enqueue(root.transform.GetChild(i));

				while (queue.Count > 0)
				{
					Transform current = queue.Dequeue();

					if (MatchesNames(current.gameObject, names, flags))
					{
						results.Add(current.gameObject);
						return this; // Return first match at shallowest depth
					}

					// Add children to queue for next level
					for (int i = 0; i < current.childCount; i++)
						queue.Enqueue(current.GetChild(i));
				}

				return this;
			}

			/// <summary>
			/// Convenience overload with default flags (case-insensitive, AND logic, match anywhere)
			/// </summary>
			public HierarchyQuery downNamed(params string[] names)
			{
				return downNamed(HierarchyFlags.None, names);
			}

			/// <summary>
			/// Finds all descendants whose name matches (DFS, all depths).
			/// <paramref name="flags"/>: Combine flags with | operator (e.g. SearchFlags.Or | SearchFlags.WholeWord)
			/// - Default (None): case-insensitive, AND logic, match anywhere
			/// - CaseSensitive: Enable case-sensitive matching
			/// - Or: Use OR logic (any term matches)
			/// - WholeWord: Only match words separated by space/underscore/dash
			/// </summary>
			public HierarchyQuery deepDownNamed(HierarchyFlags flags, params string[] names)
			{
				if (root == null) return this;

				hasExecutedQuery = true;
				results.Clear();

				// DFS to find all matches
				SearchDescendantsRecursive(root.transform, names, flags, results);

				return this;
			}

			/// <summary>
			/// Convenience overload with default flags (case-insensitive, AND logic, match anywhere)
			/// </summary>
			public HierarchyQuery deepDownNamed(params string[] names)
			{
				return deepDownNamed(HierarchyFlags.None, names);
			}

			/// <summary>
			/// Finds the first descendant (shallowest depth) with component T.
			/// </summary>
			public HierarchyQuery down<T>() where T : Component
			{
				if (root == null) return this;

				hasExecutedQuery = true;
				results.Clear();

				// BFS to find shallowest match
				var queue = new Queue<Transform>();
				for (int i = 0; i < root.transform.childCount; i++)
					queue.Enqueue(root.transform.GetChild(i));

				while (queue.Count > 0)
				{
					Transform current = queue.Dequeue();

					if (current.GetComponent<T>() != null)
					{
						results.Add(current.gameObject);
						return this;
					}

					for (int i = 0; i < current.childCount; i++)
						queue.Enqueue(current.GetChild(i));
				}

				return this;
			}

			/// <summary>
			/// Finds all descendants with component T (all depths).
			/// </summary>
			public HierarchyQuery deepDown<T>() where T : Component
			{
				if (root == null) return this;

				hasExecutedQuery = true;
				results.Clear();

				SearchComponentDescendantsRecursive<T>(root.transform, results);

				return this;
			}

			#endregion
			#region Query Methods - Ancestors

			/// <summary>
			/// Finds the first ancestor (~~immediate parent~~, closest anscestor) whose name matches.
			/// <paramref name="flags"/>: Combine flags with | operator (e.g. SearchFlags.Or | SearchFlags.WholeWord)
			/// </summary>
			public HierarchyQuery upNamed(HierarchyFlags flags, params string[] names)
			{
				if (root == null) return this;

				hasExecutedQuery = true;
				results.Clear();

				Transform current = root.transform.parent;
				while (current != null)
				{
					if (MatchesNames(current.gameObject, names, flags))
					{
						results.Add(current.gameObject);
						break;
					}
					current = current.parent;
				}

				return this;
			}

			/// <summary>
			/// Convenience overload with default flags
			/// </summary>
			public HierarchyQuery upNamed(params string[] names)
			{
				return upNamed(HierarchyFlags.None, names);
			}

			/// <summary>
			/// Finds all ancestors whose name matches (walks up entire hierarchy).
			/// <paramref name="flags"/>: Combine flags with | operator (e.g. SearchFlags.Or | SearchFlags.WholeWord)
			/// </summary>
			public HierarchyQuery deepUpNamed(HierarchyFlags flags, params string[] names)
			{
				if (root == null) return this;

				hasExecutedQuery = true;
				results.Clear();

				Transform current = root.transform.parent;
				while (current != null)
				{
					if (MatchesNames(current.gameObject, names, flags))
						results.Add(current.gameObject);
					current = current.parent;
				}

				return this;
			}

			/// <summary>
			/// Convenience overload with default flags
			/// </summary>
			public HierarchyQuery deepUpNamed(params string[] names)
			{
				return deepUpNamed(HierarchyFlags.None, names);
			}

			/// <summary>
			/// Finds the first ancestor (~~immediate parent~~, closest anscestor) with component T.
			/// </summary>
			public HierarchyQuery up<T>() where T : Component
			{
				if (root == null) return this;

				hasExecutedQuery = true;
				results.Clear();

				Transform current = root.transform.parent;
				while (current != null)
				{
					if (current.GetComponent<T>() != null)
					{
						results.Add(current.gameObject);
						break;
					}
					current = current.parent;
				}

				return this;
			}

			/// <summary>
			/// Finds all ancestors with component T (walks up entire hierarchy).
			/// </summary>
			public HierarchyQuery deepUp<T>() where T : Component
			{
				if (root == null) return this;

				hasExecutedQuery = true;
				results.Clear();

				Transform current = root.transform.parent;
				while (current != null)
				{
					if (current.GetComponent<T>() != null)
						results.Add(current.gameObject);
					current = current.parent;
				}

				return this;
			}

			#endregion
			#region Filter GameObject Method

			/// <summary>
			/// Filters current results using a predicate.
			/// Must be called after a query method (downNamed, deepDown, etc.).
			/// </summary>
			public HierarchyQuery where(Func<GameObject, bool> predicate)
			{
				if (!hasExecutedQuery)
				{
					Debug.Log("[HierarchyQuery] where() called before any query method. Use downNamed/deepDown/up etc. first.".colorTag("yellow"));
					return this;
				}

				if (predicate == null)
				{
					Debug.Log("[HierarchyQuery] where() predicate is null".colorTag("red"));
					return this;
				}

				// Filter in-place to avoid extra allocations
				for (int i = results.Count - 1; i >= 0; i--)
				{
					if (results[i] == null || !predicate(results[i]))
						results.RemoveAt(i);
				}

				return this;
			}

			#endregion
			#region Terminators - GameObject/Component Extraction

			/// <summary>
			/// Returns the first result GameObject.
			/// </summary>
			public GameObject gf()
			{
				if (!hasExecutedQuery)
				{
					Debug.Log("[HierarchyQuery] gf() called without a prior query.".colorTag("yellow"));
					return null;
				}
				return results.Count > 0 ? results[0] : null;
			}

			/// <summary>
			/// Returns the first result's component T, or null if not found.
			/// Usage: transform.Q().up<Canvas>().gf<Canvas>()
			/// </summary>
			public T gf<T>() where T : Component
			{
				GameObject go = gf();
				return go != null ? go.GetComponent<T>() : null;
			}

			/// <summary>
			/// Returns the last result GameObject.
			/// </summary>
			public GameObject gl()
			{
				if (!hasExecutedQuery)
				{
					Debug.Log("[HierarchyQuery] gl() called without a prior query.".colorTag("yellow"));
					return null;
				}
				return results.Count > 0 ? results[results.Count - 1] : null;
			}

			/// <summary>
			/// Returns the last result's component T, or null if not found.
			/// Usage: transform.Q().deepDown<Rigidbody>().gl<Rigidbody>()
			/// </summary>
			public T gl<T>() where T : Component
			{
				GameObject go = gl();
				return go != null ? go.GetComponent<T>() : null;
			}

			/// <summary>
			/// Returns all result GameObjects.
			/// </summary>
			public List<GameObject> all()
			{
				if (!hasExecutedQuery)
				{
					Debug.Log("[HierarchyQuery] all() called without a prior query.".colorTag("yellow"));
					return new List<GameObject>();
				}
				return new List<GameObject>(results);
			}

			/// <summary>
			/// Returns all results as a list of component T (skips GameObjects without T).
			/// Usage: transform.Q().deepDown<Collider>().all<Collider>()
			/// </summary>
			public List<T> all<T>() where T : Component
			{
				if (!hasExecutedQuery)
				{
					Debug.Log("[HierarchyQuery] all<T>() called without a prior query.".colorTag("yellow"));
					return new List<T>();
				}

				var components = new List<T>(results.Count);
				for (int i = 0; i < results.Count; i++)
				{
					if (results[i] != null)
					{
						T comp = results[i].GetComponent<T>();
						if (comp != null)
							components.Add(comp);
					}
				}
				return components;
			}

			#endregion
			#region Additional Terminators

			/// <summary>
			/// Returns the count of results.
			/// </summary>
			public int Count()
			{
				if (!hasExecutedQuery)
				{
					Debug.Log("[HierarchyQuery] count() called without a prior query. Use downNamed/deepDown/up etc. first.".colorTag("yellow"));
					return 0;
				}

				return results.Count;
			}

			/// <summary>
			/// Returns true if any results were found.
			/// </summary>
			public bool Exists()
			{
				if (!hasExecutedQuery)
				{
					Debug.Log("[HierarchyQuery] exists() called without a prior query. Use downNamed/deepDown/up etc. first.".colorTag("yellow"));
					return false;
				}

				return results.Count > 0;
			}

			/// <summary>
			/// Returns full hierarchy paths for all results.
			/// Example: ["Root/Parent/Child", "Root/Parent/Sibling"]
			/// </summary>
			public List<string> GetFullPath()
			{
				if (!hasExecutedQuery)
				{
					Debug.Log("[HierarchyQuery] getFullPath() called without a prior query. Use downNamed/deepDown/up etc. first.".colorTag("yellow"));
					return new List<string>();
				}

				var paths = new List<string>(results.Count);
				for (int i = 0; i < results.Count; i++)
				{
					if (results[i] != null)
						paths.Add(results[i].getFullPath());
				}
				return paths;
			}

			public List<Transform> GetFirstGen()
			{
				List<Transform> CHILD = new List<Transform>();
				for (int i0 = 0; i0 < this.root.transform.childCount; i0 += 1)
					CHILD.Add(this.root.transform.GetChild(i0));
				return CHILD;
			}
			#endregion
			#region Helper Methods

			/// <summary>
			/// Checks if a GameObject's name matches the search criteria.
			/// Supports multiple search terms with AND/OR logic and various matching modes.
			/// </summary>
			private static bool MatchesNames(GameObject go, string[] searchTerms, HierarchyFlags flags)
			{
				if (go == null || searchTerms == null || searchTerms.Length == 0)
					return false;

				bool caseSensitive = (flags & HierarchyFlags.CaseSensitive) != 0;
				bool useOrLogic = (flags & HierarchyFlags.Or) != 0;
				bool wholeWord = (flags & HierarchyFlags.WholeWord) != 0;

				string goName = caseSensitive ? go.name : go.name.ToLower();

				// AND logic: ALL terms must match
				if (!useOrLogic)
				{
					foreach (string term in searchTerms)
					{
						if (string.IsNullOrEmpty(term))
							continue;

						string searchTerm = caseSensitive ? term : term.ToLower();

						if (!MatchesSingleTerm(goName, searchTerm, wholeWord, caseSensitive))
							return false; // This term didn't match
					}
					return true; // All terms matched
				}
				// OR logic: ANY term must match
				else
				{
					foreach (string term in searchTerms)
					{
						if (string.IsNullOrEmpty(term))
							continue;

						string searchTerm = caseSensitive ? term : term.ToLower();

						if (MatchesSingleTerm(goName, searchTerm, wholeWord, caseSensitive))
							return true; // Found a match
					}
					return false; // No terms matched
				}
			}

			/// <summary>
			/// Checks if a single search term matches the name
			/// </summary>
			private static bool MatchesSingleTerm(string name, string searchTerm, bool wholeWord = false, bool caseSensitive = false)
			{
				if (wholeWord)
				{
					// Match only words separated by space/underscore/dash
					string pattern = @"(^|[\s_-])" + Regex.Escape(searchTerm) + @"([\s_-]|$)";
					RegexOptions options = caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
					return Regex.IsMatch(name, pattern, options);
				}
				else
				{
					// Match anywhere (substring search)
					return name.Contains(searchTerm);
				}
			}

			private static void SearchDescendantsRecursive(Transform current, string[] names, HierarchyFlags flags, List<GameObject> results)
			{
				// Don't check current (it's the root), only children
				for (int i = 0; i < current.childCount; i++)
				{
					Transform child = current.GetChild(i);

					if (MatchesNames(child.gameObject, names, flags))
						results.Add(child.gameObject);

					// Recurse
					SearchDescendantsRecursive(child, names, flags, results);
				}
			}

			private static void SearchComponentDescendantsRecursive<T>(Transform current, List<GameObject> results) where T : Component
			{
				// Don't check current (it's the root), only children
				for (int i = 0; i < current.childCount; i++)
				{
					Transform child = current.GetChild(i);

					if (child.GetComponent<T>() != null)
						results.Add(child.gameObject);

					// Recurse
					SearchComponentDescendantsRecursive<T>(child, results);
				}
			}

			#endregion
		}

		/// <summary>
		/// Entry point for GameObject hierarchy queries.
		/// Usage: 
		/// - gameObject.Q().downNamed("door").gf()
		/// - gameObject.Q().downNamed("cube", "red", "trigger").gf()
		/// - gameObject.Q().downNamed(SearchFlags.Or, "player", "enemy").all()
		/// - gameObject.Q().downNamed(SearchFlags.Or | SearchFlags.WholeWord, "main", "camera").gf()
		/// - gameObject.Q().downNamed(SearchFlags.CaseSensitive | SearchFlags.WholeWord, "Player").gf()
		/// </summary>
		public static HierarchyQuery Q(this GameObject gameObject)
		{
			return new HierarchyQuery(gameObject);
		}
		/// <summary>
		/// Entry point for Component hierarchy queries.
		/// Usage: component.Q().deepDown<Rigidbody>().all()
		/// </summary>
		public static HierarchyQuery Q(this Component component)
		{
			return new HierarchyQuery(component != null ? component.gameObject : null);
		}

		/// <summary>
		/// Returns the full hierarchy path for a GameObject.
		/// Example: "Root/Parent/Child/GrandChild"
		/// </summary>
		public static string getFullPath(this GameObject gameObject)
		{
			if (gameObject == null)
				return string.Empty;

			// Build path from root to this object
			var sb = new StringBuilder();
			Transform current = gameObject.transform;

			// Collect ancestors in reverse order
			var ancestors = new Stack<string>();
			while (current != null)
			{
				ancestors.Push(current.name);
				current = current.parent;
			}

			// Build path string
			bool first = true;
			while (ancestors.Count > 0)
			{
				if (!first)
					sb.Append('/');
				sb.Append(ancestors.Pop());
				first = false;
			}

			return sb.ToString();
		}
		public static string getFullPath(this Component component)
		{
			return component.gameObject.getFullPath();
		}
	}

	public static class ExtensionGameObjectOrComponent
	{
		#region .leafNameStartsWith, .leafQuery, .getLeavesGen1, .getDepthLeafNameStartingWith
		/// <summary>
		/// Get A Certain Transform at Gen 1
		/// </summary>
		public static Transform leafNameStartsWith(this GameObject gameObject, string name)
		{
			Transform transform = gameObject.transform;
			for (int i0 = 0; i0 < transform.childCount; i0 += 1)
				if (transform.GetChild(i0).name.ToLower().StartsWith(name.ToLower()))
					return transform.GetChild(i0);
			Debug.LogError($"found no leaf starting with that name: {name.ToLower()}, under transform: {transform.name}");
			return null;
		}
		public static Transform leafNameStartsWith(this Component component, string name)
		{
			return component.gameObject.leafNameStartsWith(name);
		}

		/// <summary>
		/// if gameObject @"a" = ancestor, then gameObject @"a".Query("b > c > d > e") = gameObject @"d".NameStartsWith("e")
		/// </summary>
		public static Transform leafQuery(this GameObject gameObject, string query, char sep = '>')
		{
			string[] QUERY = query.Split(sep); // or your custom .split()
			Transform leaf = gameObject.transform;

			foreach (string name in QUERY)
			{
				if (leaf == null)
				{
					Debug.LogError($"leafQuery starting from rootName: {gameObject.name} is null for leafName: {name} with query: {query}");
					return null; // 👈 Add this
				}
				leaf = leaf.leafNameStartsWith(name.Trim());
			}

			return leaf;
		}
		public static Transform leafQuery(this Component component, string query, char sep = '>') // query: leaf > leaflet
		{
			return component.gameObject.leafQuery(query, sep);
		}

		// get Leaves under a Transform/GameObject/Component
		/// <summary>
		/// Get List<Transform> at Gen 1
		/// </summary>
		public static List<Transform> getLeavesGen1(this GameObject gameObject)
		{
			List<Transform> TRANSFORM = new List<Transform>();
			Transform transform = gameObject.transform;
			for (int i0 = 0; i0 < transform.childCount; i0 += 1)
				TRANSFORM.Add(transform.GetChild(i0));

			return TRANSFORM;
		}
		public static List<Transform> getLeavesGen1(this Component component)
		{
			return component.gameObject.getLeavesGen1();
		}

		// GetDepthLeaf under multiple gen of Transform/GameObject/Component
		// Better depth search without static state
		public static Transform getDepthLeafNameStartingWith(this GameObject gameObject, string name)
		{
			return DepthSearchRecursive(gameObject.transform, name.ToLower());
		}
		public static Transform getDepthLeafNameStartingWith(this Component component, string name)
		{
			return component.gameObject.getDepthLeafNameStartingWith(name);
		}
		static Transform DepthSearchRecursive(Transform current, string lowerName)
		{
			if (current.name.ToLower().StartsWith(lowerName))
				return current;

			for (int i = 0; i < current.childCount; i += 1)
			{
				Transform found = DepthSearchRecursive(current.GetChild(i), lowerName);
				if (found != null) return found;
			}

			return null;
		}

		#region prev approach, thread safe issue(according to seek)
		/// <summary>
		/// Get Transform after Depth Search
		/// </summary>
		static Transform getDepthLeafNameStartingWith_prev(this GameObject gameObject, string name)
		{
			foundChild = null;
			DepthSearch(gameObject.transform, name);
			if (foundChild == null)
				Debug.LogError($"no leaf found in depth search with name {name} under {gameObject.name}");
			return foundChild?.transform;
		}
		static Transform getDepthLeafNameStartingWith_prev(this Component component, string name)
		{
			return component.gameObject.getDepthLeafNameStartingWith_prev(name);
		}

		static Transform foundChild;
		// self comparison approach >>
		static void DepthSearch(Transform transform, string name)
		{
			// exit //
			if (transform.name.ToLower().StartsWith(name.ToLower()) == true)
			{
				foundChild = transform;
				return;
			}

			// exit //
			if (foundChild != null)
				return;

			for (int i0 = 0; i0 < transform.childCount; i0 += 1)
				// recursive >>
				DepthSearch(transform.GetChild(i0), name);
			// << recursive
		}
		// << self comparison approach 
		#endregion

		#endregion

		#region .gc<T>, .gcLeaf<T>, .gcLeaves<T>
		// Get Component Shorter format 
		public static T gc<T>(this GameObject go) where T : Component
		{
			return go.GetComponent<T>();
		}
		public static T gc<T>(this Component component) where T : Component
		{
			return component.gameObject.gc<T>();
		}

		public static T gcLeaf<T>(this GameObject go) where T : Component
		{
			return go.GetComponentInChildren<T>();
		}
		public static T gcLeaf<T>(this Component component) where T : Component
		{
			return component.gameObject.gcLeaf<T>();
		}

		public static IEnumerable<T> gcLeaves<T>(this GameObject go) where T : Component
		{
			return go.GetComponentsInChildren<T>();
		}
		public static IEnumerable<T> gcLeaves<T>(this Component component) where T : Component
		{
			return component.gameObject.gcLeaves<T>();
		}
		#endregion

		#region .clearLeaves, .toggleLeaves, .toggle
		// destroy
		public static void destroyLeaves(this GameObject gameObject)
		{
			Transform transform = gameObject.transform;
			for (int i = transform.childCount - 1; i >= 0; i -= 1)
				GameObject.Destroy(transform.GetChild(i).gameObject);
		}
		public static void destroyLeaves(this Component component)
		{
			component.gameObject.destroyLeaves();
		}
		public static void destroy(this GameObject gameObject)
		{
			GameObject.Destroy(gameObject);
		}
		public static void destroy(this Component component)
		{
			component.gameObject.destroy();
		}

		// gameObjects setActive
		public static void toggleLeaves(this GameObject gameObject, bool value = false)
		{
			Transform transform = gameObject.transform;
			for (int i0 = 0; i0 < transform.childCount; i0 += 1)
				transform.GetChild(i0).gameObject.SetActive(value);
		}
		public static void toggleLeaves(this Component component, bool value = false)
		{
			component.gameObject.toggleLeaves(value);
		}

		// gameObject setActive
		public static Transform toggle(this GameObject gameObject, bool value = false)
		{
			gameObject.SetActive(value);
			return gameObject.transform;
		}
		public static Transform toggle(this Component component, bool value = false)
		{
			return component.gameObject.toggle(value);
		}

		#endregion
	}

	public static class ExtensionAnimator
	{
		/*
			if(animator.trySet....(.doorOpen, true))
				Debug.Log("Door Opening Set in animator");
		*/
		public static bool trySetBool(this Animator animator, object parameterType, bool val)
		{
			string paramName = parameterType.ToString();

			// Check if parameter exists
			foreach (AnimatorControllerParameter param in animator.parameters)
			{
				if (param.name == paramName && param.type == AnimatorControllerParameterType.Bool)
				{
					animator.SetBool(paramName, val);
					return true;
				}
			}

			Debug.Log($"Bool parameter '{paramName}' not found in Animator".colorTag("red"));
			return false;
		}
		public static bool tryGetBool(this Animator animator, object parameterType, out bool val)
		{
			string paramName = parameterType.ToString();

			// Check if parameter exists
			foreach (AnimatorControllerParameter param in animator.parameters)
			{
				if (param.name == paramName && param.type == AnimatorControllerParameterType.Bool)
				{
					val = animator.GetBool(paramName);
					return true;
				}
			}

			Debug.Log($"Bool parameter '{paramName}' not found in Animator".colorTag("red"));
			val = false;
			return false;
		}

		public static bool trySetTrigger(this Animator animator, object parameterType)
		{
			string paramName = parameterType.ToString();

			// Check if parameter exists
			foreach (AnimatorControllerParameter param in animator.parameters)
			{
				if (param.name == paramName && param.type == AnimatorControllerParameterType.Trigger)
				{
					animator.SetTrigger(paramName);
					return true;
				}
			}

			Debug.Log($"Trigger parameter '{paramName}' not found in Animator".colorTag("red"));
			return false;
		}

		public static bool trySetFloat(this Animator animator, object parameterType, float val)
		{
			string paramName = parameterType.ToString();

			// Check if parameter exists
			foreach (AnimatorControllerParameter param in animator.parameters)
			{
				if (param.name == paramName && param.type == AnimatorControllerParameterType.Float)
				{
					animator.SetFloat(paramName, val);
					return true;
				}
			}

			Debug.Log($"Float parameter '{paramName}' not found in Animator".colorTag("red"));
			return false;
		}
		public static bool tryGetFloat(this Animator animator, object parameterType, out float val)
		{
			string paramName = parameterType.ToString();

			// Check if parameter exists
			foreach (AnimatorControllerParameter param in animator.parameters)
			{
				if (param.name == paramName && param.type == AnimatorControllerParameterType.Float)
				{
					val = animator.GetFloat(paramName);
					return true;
				}
			}

			Debug.Log($"Float parameter '{paramName}' not found in Animator".colorTag("red"));
			val = -1f;
			return false;
		}

		public static bool trySetInt(this Animator animator, object parameterType, int val)
		{
			string paramName = parameterType.ToString();

			// Check if parameter exists
			foreach (AnimatorControllerParameter param in animator.parameters)
			{
				if (param.name == paramName && param.type == AnimatorControllerParameterType.Int)
				{
					animator.SetInteger(paramName, val);
					return true;
				}
			}

			Debug.Log($"Int parameter '{paramName}' not found in Animator".colorTag("red"));
			return false;
		}
		public static bool tryGetInt(this Animator animator, object parameterType, out int val)
		{
			string paramName = parameterType.ToString();

			// Check if parameter exists
			foreach (AnimatorControllerParameter param in animator.parameters)
			{
				if (param.name == paramName && param.type == AnimatorControllerParameterType.Int)
				{
					val = animator.GetInteger(paramName);
					return true;
				}
			}

			Debug.Log($"Int parameter '{paramName}' not found in Animator".colorTag("red"));
			val = -1;
			return false;
		}

		public static void checkAllParamExistInAnimatorController<T>(this Animator animator) where T : struct
		{
			foreach (var param in (T[])Enum.GetValues(typeof(T)))
			{
				bool exists = false;
				// search >>
				foreach (var animParam in animator.parameters)
					if (param.ToString() == animParam.name)
					{
						exists = true;
						Debug.Log($"found {param}: existance in {animator}".colorTag(C.colorStr.aqua));
						break;
					}
				// << search
				if (exists == false)
					Debug.Log($"not found {param}: existance in {animator}".colorTag("red"));
			}
		}
	}

	public static class ExtensionInputSystem
	{
		// no cross dependency with any other UTIL.cs class
		/// <summary>
		/// Attempts to retrieve an InputAction from an InputActionAsset using enum-style naming. (seperateor: "__")
		/// Example: GameActionType.character__jump → actionMap: "character", action: "jump"
		/// </summary>
		public static UnityEngine.InputSystem.InputAction tryGetAction(this UnityEngine.InputSystem.InputActionAsset IAAsset, object actionType)
		{
			string fullName = actionType.ToString();

			// Split by double underscore to get actionMap and action name
			string[] parts = fullName.split(@"__").ToArray();

			if (parts.Length != 2)
			{
				Debug.Log($"[IA.tryGetAction()] Invalid action format '{fullName}'. Expected format: 'actionMap__actionName' (e.g., 'character__jump')".colorTag("red"));
				return null;
			}

			string actionMapName = parts[0];
			string actionName = parts[1];

			// Find the action map
			var actionMap = IAAsset.FindActionMap(actionMapName);
			if (actionMap == null)
			{
				Debug.Log($"[IA.tryGetAction()] ActionMap '{actionMapName}' not found in InputActionAsset: {IAAsset.name}".colorTag("red"));
				return null;
			}

			// Find the action within the map
			var action = actionMap.FindAction(actionName);
			if (action == null)
			{
				Debug.Log($"[IA.tryGetAction()] Action '{actionName}' not found in ActionMap '{actionMapName}'".colorTag("red"));
				return null;
			}

			return action;
		}

		public static void tryLoadBindingOverridesFromJson(this UnityEngine.InputSystem.InputActionAsset IAAsset, string overrideJSON)
		{
			try
			{
				Debug.Log(C.method(null, color: "lime", adMssg: "success tried parsing(if hash Id for binding value match) overriden bindings"));
				// Debug.Log($"[InputActionAsset.tryLoadBindingOverridesFromJson()] success tried parsing(if hash Id for binding value match) overriden bindings".colorTag("lime"));
				// Load from Saved GameData
				IAAsset.LoadBindingOverridesFromJson(overrideJSON);
			}
			catch (Exception)
			{
				Debug.Log(C.method(null, color: "red", adMssg: "error parsing overriden bindings so loaded default IA with no override."));
				//Debug.Log($"[InputActionAsset.tryLoadBindingOverridesFromJson()] error parsing overriden bindings so loaded default IA".colorTag("red"));
			}
		}
	}

	public static class ExtentionAd
	{
		public static bool contains(this LayerMask layerMask, GameObject other)
		{
			return ((layerMask.value & (1 << other.layer)) == 0);
		}
	}

	#endregion

	#region R
	/// <summary>
	/// Unified resource cache with generic syntax: R.get<T>(enum)
	/// Example: GameObject prefab = R.get<GameObject>(ResourceType.prefab__bullet__cannon);
	/// Example: AudioClip clip = R.get<AudioClip>(ResourceType.audio__sfx__shoot);
	/// All resources use "__" separator which converts to "/" for folder structure
	/// </summary>
	public static class R
	{
		// Unified cache for all resource types
		private static Dictionary<string, UnityEngine.Object> cache = new Dictionary<string, UnityEngine.Object>();

		/// <summary>
		/// Gets any resource type from Resources folder based on enum/object name.
		/// prefab__bullet__cannon → Resources/prefab/bullet/cannon
		/// RLoadType.audio__sfx__shoot → Resources/audio/sfx/shoot
		/// Caches the result for subsequent calls.
		/// </summary>
		public static T get<T>(object resourceType) where T : UnityEngine.Object
		{
			string resourceTypeStr = resourceType.ToString();

			// Return cached if available
			if (cache.TryGetValue(resourceTypeStr, out UnityEngine.Object cached))
			{
				if (cached is T typedCache)
					return typedCache;

				Debug.LogError($"Cached resource '{resourceTypeStr}' is type {cached.GetType().Name}, not {typeof(T).Name}".colorTag("red"));
				return null;
			}

			// Convert enum to path: prefab__bullet__cannon → prefab/bullet/cannon
			string path = resourceTypeStr.replace(@"__", "/");

			// Load from Resources
			T resource = Resources.Load<T>(path);

			if (resource == null)
			{
				Debug.LogError($"Failed to load {typeof(T).Name} at path: Resources/{path}".colorTag("red"));
				return null;
			}

			// Cache and return
			cache[resourceTypeStr] = resource;
			return resource;
		}

		#region public API
		// error: Failed to load Object at path: Resources/ResourceType[]
		// resolved by removing params since it assume object[] which is also considered as object.
		/// <summary>
		/// used as: R.preloadAll(C.getEnumValues<ResourceType>().map(en => (object)en).ToArray());
		/// </summary>
		public static void preloadAll(object[] resourceTypes)
		{
			resourceTypes.forEach(resourceType => get<UnityEngine.Object>(resourceType));
			Debug.Log($"Preloaded {resourceTypes.Length} resources into cache".colorTag("lime"));
		}

		/// <summary>
		/// Clears all cached resources and unloads unused assets.
		/// </summary>
		public static void clearCache()
		{
			cache.Clear();
			Resources.UnloadUnusedAssets();
		}

		#region ad
		/// <summary>
		/// Gets cache statistics for debugging.
		/// </summary>
		public static string stats()
		{
			// Group by type
			var typeGroups = new Dictionary<Type, int>();

			foreach (var kvp in cache)
			{
				if (kvp.Value != null)
				{
					Type type = kvp.Value.GetType(); // GameObject/AudioClip/Material/TextAsset
					if (!typeGroups.ContainsKey(type))
						typeGroups[type] = 0;
					typeGroups[type]++;
				}
			}

			string result = "ResourceCache Stats:\n";
			result += $"  Total cached: {cache.Count}\n";

			foreach (var kvp in typeGroups)
			{
				result += $"  {kvp.Key.Name}: {kvp.Value}\n";
			}

			return result;
		}

		/// <summary>
		/// Logs the Resources folder structure based on cached resources.
		/// Shows a tree view of all loaded resources organized by folder.
		/// Usage: R.getHeirarchy();
		/// </summary>
		public static string getHeirarchy()
		{
			if (cache.Count == 0)
			{
				Debug.Log("ResourceCache is empty. No resources loaded yet.".colorTag("yellow"));
				return "";
			}

			// Build tree structure from cached resource paths
			var tree = new Dictionary<string, List<string>>();

			foreach (var kvp in cache)
			{
				if (kvp.Value == null) continue;

				string fullPath = kvp.Key.replace(@"__", "/");
				string[] parts = fullPath.Split('/');

				// Build path incrementally: "bullet" -> "bullet/sphere"
				string currentPath = "";
				for (int i = 0; i < parts.Length; i++)
				{
					string parentPath = currentPath;
					currentPath = currentPath == "" ? parts[i] : $"{currentPath}/{parts[i]}";

					if (!tree.ContainsKey(parentPath))
						tree[parentPath] = new List<string>();

					if (!tree[parentPath].Contains(currentPath))
						tree[parentPath].Add(currentPath);
				}
			}

			// Build visual tree
			string result = "./Resources/\n";
			result += BuildTreeRecursive(tree, "", "    ", true);

			return result;
		}
		#region getHeirarchy Helper
		private static string GetExtension(UnityEngine.Object obj)
		{
			if (obj is GameObject) return ".prefab";
			if (obj is Sprite || obj is Texture2D) return ".png";
			if (obj is AudioClip) return ".wav";
			if (obj is Material) return ".mat";
			if (obj is TextAsset) return ".txt";
			return "";
		}
		private static string BuildTreeRecursive(Dictionary<string, List<string>> tree, string currentPath, string indent, bool isRoot)
		{
			if (!tree.ContainsKey(currentPath))
				return "";

			var children = tree[currentPath];
			children.Sort(); // Alphabetical order

			string result = "";

			for (int i = 0; i < children.Count; i++)
			{
				bool isLast = (i == children.Count - 1);
				string childPath = children[i];
				string childName = childPath.Split('/')[childPath.Split('/').Length - 1];

				// Check if this is a file (has cached resource)
				bool isFile = false;
				foreach (var kvp in cache)
				{
					if (kvp.Value != null && kvp.Key.replace(@"__", "/") == childPath)
					{
						string extension = GetExtension(kvp.Value);
						childName += extension;
						isFile = true;
						break;
					}
				}

				// Draw tree lines with box-drawing characters
				string connector = isLast ? "└─ " : "├─ ";
				string childIndent = indent + (isLast ? "   " : "│  ");

				result += $"{indent}{connector}{childName}\n";

				// Recurse if it's a folder
				if (!isFile)
					result += BuildTreeRecursive(tree, childPath, childIndent, false);
			}

			return result;
		}
		#endregion
		#endregion
		#endregion
	}
	#endregion

	#region LOG
	/// <summary>
	/// EnsureDirExists, GetFilePath()
	/// </summary>
	public static partial class LOG
	{
		public static string locRootPath => Application.dataPath; // could be set in INITManager/GameStore Awake() to Application.persistantDataPath or Application.dataPath.
		private static string locLOGDirectory => Path.Combine(locRootPath, "LOG");
		private static string locLOGFile => Path.Combine(locLOGDirectory, "LOG.md");
		public static string locGameDataDirectory => Path.Combine(locLOGDirectory, "GameData");
		public static string locGameDataNoEncrDirectory => Path.Combine(locLOGDirectory, "GameDataWithNoEncryption"); // todo: make use of it in editor script

		#region API EnsureAllDirExists(), GetGameDataFilePath()
		/// <summary>
		/// Ensures the LOG directory structure exists
		/// </summary>
		private static void EnsureAllDirectoryExists()
		{
			// LOG/GameData
			if (!Directory.Exists(locGameDataDirectory))
			{
				Directory.CreateDirectory(locGameDataDirectory);
			}

			// LOG/LOG.md
			if (!File.Exists(locLOGFile))
				File.WriteAllText(locLOGFile, "# LOG.md created, perform LOG.SaveLog(str, format) to append text here:\n\n");
		}
		/// <summary>
		/// Gets the full file path for a given GameDataType
		/// </summary>
		public static string GetGameDataFilePath(string fileName)
		{
			return Path.Combine(locGameDataDirectory, $"{fileName}.json");
		}
		#endregion
	}

	/// <summary>
	/// AddLog(str)
	/// TimeStart, End for diagnostics
	/// </summary>
	public static partial class LOG
	{
		#region AddLog
		#region disable AddLog
		public static bool isAddLogDisabled = false; 
		#endregion
		public static void AddLog(string str, string syntaxType = "")
		{
			if (LOG.isAddLogDisabled == true)
				return;

			LOG.EnsureAllDirectoryExists();

			if (syntaxType != "")
				str = $"```{syntaxType}\n{str}\n```"; // format for markDown

			// string str = string.Join("\n\n", args);
			//string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
			//string logEntry = $"[{timestamp}] {str}";
			// File logging
			try
			{
				Debug.Log(C.method(null, color: "grey", adMssg: "success wiriting file"));
				System.IO.File.AppendAllText(locLOGFile, str + Environment.NewLine + Environment.NewLine);
			}
			catch (Exception e)
			{
				Debug.Log(C.method(null, color: "red", adMssg: "error wrinting to log file"));
			}
		}

		public static void H(string header) { AddLog($"# {header} >>\n"); }
		public static void HEnd(string header) { AddLog($"# << {header}"); }
		#endregion

		#region TimeStart(), TimeEnd()
		private static Dictionary<string, System.Diagnostics.Stopwatch> _timers =
			new Dictionary<string, System.Diagnostics.Stopwatch>();

		/// <summary>
		/// Start a named timer (like console.time() in JavaScript).
		/// Usage: LOG.TimeStart("parsing");
		/// </summary>
		public static void TimeStart(string label = "timer")
		{
			if (_timers.ContainsKey(label))
			{
				Debug.Log($"[LOG.TimeStart] Timer '{label}' already running. Restarting.".colorTag("yellow"));
				_timers[label].Restart();
			}
			else
			{
				var sw = new System.Diagnostics.Stopwatch();
				sw.Start();
				_timers[label] = sw;
			}
		}

		/// <summary>
		/// Stop and log a named timer (like console.timeEnd() in JavaScript).
		/// Usage: LOG.TimeEnd("parsing"); // Logs: "[LOG] parsing: 123.45ms"
		/// </summary>
		public static void TimeEnd(string label = "timer")
		{
			if (!_timers.ContainsKey(label))
			{
				Debug.Log($"[LOG.TimeEnd] Timer '{label}' not found. Did you call TimeStart()?".colorTag("red"));
				return;
			}

			var sw = _timers[label];
			sw.Stop();

			// Format time intelligently
			string timeStr;
			if (sw.ElapsedMilliseconds < 1)
				timeStr = $"{sw.Elapsed.TotalMilliseconds:F3}ms ({sw.ElapsedTicks} ticks)";
			else if (sw.ElapsedMilliseconds < 1000)
				timeStr = $"{sw.ElapsedMilliseconds}ms";
			else
				timeStr = $"{sw.Elapsed.TotalSeconds:F2}s";

			Debug.Log($"[LOG] {label}: {timeStr}".colorTag("yellow"));

			// Also write to LOG.md
			AddLog($"⏱️ **{label}**: {timeStr}");

			_timers.Remove(label);
		}

		/// <summary>
		/// Get elapsed time without stopping timer.
		/// Usage: float ms = LOG.TimeGet("myTimer");
		/// </summary>
		public static double TimeGet(string label = "timer")
		{
			if (!_timers.ContainsKey(label))
			{
				Debug.LogError($"[LOG.TimeGet] Timer '{label}' not found".colorTag("red"));
				return -1;
			}
			return _timers[label].Elapsed.TotalMilliseconds;
		}

		/// <summary>
		/// Clear all timers (useful for scene transitions).
		/// </summary>
		public static void TimeClear()
		{
			int count = _timers.Count;
			_timers.Clear();
			Debug.Log($"[LOG.TimeClear] Cleared {count} timer(s)".colorTag("grey"));
		} 
		#endregion
	}

	/// <summary>
	/// LoadGameData<T>(enum), LoadGameData(enum), SaveGameData(enum)
	/// (optional) Uses AES-256 with PBKDF2 key derivation for secure, lossless encryption.
	/// </summary>
	public static partial class LOG
	{
		#region API encr/decr

		#region private API encr/decr
		// IMPORTANT: Change this to a unique value for your game!
		// This is NOT the encryption key - it's used to derive the key
		private const string GAME_SALT = "zero-one"; // ← CHANGE THIS, should be atleast 8 bytes(8 char)

		/// <summary>
		/// Derives an encryption key from the device ID and game salt.
		/// Uses PBKDF2 with 10000 iterations for security.
		/// </summary>
		private static byte[] DeriveKey()
		{
			// Use device ID as password (unique per device)
			string password = SystemInfo.deviceUniqueIdentifier;
			byte[] salt = Encoding.UTF8.GetBytes(GAME_SALT);

			// PBKDF2: industry standard for key derivation
			using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, 10000))
			{
				return deriveBytes.GetBytes(32); // 256-bit key for AES-256
			}
		}

		/// <summary>
		/// Encrypts a string using AES-256-CBC.
		/// Returns Base64-encoded ciphertext (safe for text files).
		/// </summary>
		private static string Encrypt(string plainText)
		{
			if (string.IsNullOrEmpty(plainText))
				return plainText;

			byte[] key = DeriveKey();

			using (Aes aes = Aes.Create())
			{
				aes.Key = key;
				aes.GenerateIV(); // Random IV for each encryption

				ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

				using (MemoryStream ms = new MemoryStream())
				{
					// Prepend IV to ciphertext (needed for decryption)
					ms.Write(aes.IV, 0, aes.IV.Length);

					using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
					using (StreamWriter sw = new StreamWriter(cs))
					{
						sw.Write(plainText);
					}

					// Convert to Base64 (safe for text files, no data loss)
					return Convert.ToBase64String(ms.ToArray());
				}
			}
		}

		/// <summary>
		/// Decrypts AES-256-CBC ciphertext.
		/// Input must be Base64-encoded.
		/// </summary>
		private static string Decrypt(string cipherText)
		{
			if (string.IsNullOrEmpty(cipherText))
				return cipherText;

			byte[] key = DeriveKey();
			byte[] buffer = Convert.FromBase64String(cipherText);

			using (Aes aes = Aes.Create())
			{
				aes.Key = key;

				// Extract IV from beginning of ciphertext
				byte[] iv = new byte[aes.IV.Length];
				Array.Copy(buffer, 0, iv, 0, iv.Length);
				aes.IV = iv;

				ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

				using (MemoryStream ms = new MemoryStream(buffer, iv.Length, buffer.Length - iv.Length))
				using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
				using (StreamReader sr = new StreamReader(cs))
				{
					return sr.ReadToEnd();
				}
			}
		}
		#endregion

		#region public API check encr/decr
		/// <summary>
		/// Test encryption/decryption with sample data.
		/// Usage: LOG.TestEncryption();
		/// </summary>
		public static void CheckEncryption()
		{
			string original = "Test data: 你好 🎮 \n\t\r Special chars!";

			Debug.Log("=== Encryption Test ===");
			Debug.Log($"Original: {original}");

			string encrypted = Encrypt(original);
			Debug.Log($"Encrypted (Base64): {encrypted}");

			string decrypted = Decrypt(encrypted);
			Debug.Log($"Decrypted: {decrypted}");

			bool success = original == decrypted;
			Debug.Log($"Test Result: {(success ? "✓ PASS" : "✗ FAIL")}".colorTag(success ? "lime" : "red"));
		}

		#endregion

		#endregion

		#region LoadGameData<T>(enum), LoadGameData(enum), SaveGameData(enum)
		/// <summary>
		/// Save game data with optional encryption.
		/// Usage: LOG.SaveGameData(GameDataType.PlayerProgress, jsonString, encryptRequired: true);
		/// </summary>
		// todo: call encryptRequired, from GameStore (since its game specific, note that for a certain GameDataTypeif encr is enabled for load it must be enabled for save too and vice versa)
		public static void SaveGameData(object dataType, string jsonContent, bool encryptRequired = !true)
		{
			EnsureAllDirectoryExists();
			string filePath = GetGameDataFilePath(dataType.ToString());

			try
			{
				string contentToSave = encryptRequired ? Encrypt(jsonContent) : jsonContent;
				File.WriteAllText(filePath, contentToSave);

				string encStatus = encryptRequired ? "[ENCRYPTED]" : "[PLAIN]";
				Debug.Log(C.method(null, "lime", $"Saved {encStatus}: {filePath}"));
			}
			catch (Exception e)
			{
				Debug.Log(C.method(null, "red", $"Error saving {filePath}: {e.Message}"));
			}
		}

		/// <summary>
		/// Load and optionally decrypt game data.
		/// Returns default(T) if file doesn't exist or decryption fails.
		/// Usage: PlayerData data = LOG.LoadGameData<PlayerData>(GameDataType.PlayerProgress, encryptRequired: true);
		/// </summary>
		public static T LoadGameData<T>(object dataType, bool encryptRequired = !true) where T : new()
		{
			string filePath = GetGameDataFilePath(dataType.ToString());

			// Scenario 1: File doesn't exist
			if (!File.Exists(filePath))
			{
				Debug.Log(C.method(null, "red", $"File not found: {filePath}. Returning default instance."));
				return new T();
			}

			// Scenario 0: File exists
			try
			{
				string fileContent = File.ReadAllText(filePath);
				string jsonContent = encryptRequired ? Decrypt(fileContent) : fileContent;

				T data = JsonUtility.FromJson<T>(jsonContent);

				// If parsing failed (returns null or default)
				if (data == null || EqualityComparer<T>.Default.Equals(data, default(T)))
				{
					Debug.Log(C.method(null, "red", $"Failed to parse JSON from: {filePath}. Returning default instance."));
					return new T();
				}

				string encStatus = encryptRequired ? "[DECRYPTED]" : "[PLAIN]";
				Debug.Log(C.method(null, "lime", $"Successfully loaded {encStatus}: {filePath}"));
				return data;
			}
			catch (CryptographicException)
			{
				Debug.Log(C.method(null, "red", $"Decryption failed for {filePath} (corrupted/tampered). Returning default instance."));
				return new T();
			}
			catch (Exception e)
			{
				Debug.Log(C.method(null, "red", $"Error loading {filePath}: {e.Message}. Returning default instance."));
				return new T();
			}
		}

		/// <summary>
		/// Load raw content with optional decryption.
		/// Returns empty string if file doesn't exist or decryption fails.
		/// </summary>
		public static string LoadGameData(object dataType, bool encryptRequired = !true)
		{
			string filePath = GetGameDataFilePath(dataType.ToString());

			// Scenario 1: File doesn't exist
			if (!File.Exists(filePath))
			{
				Debug.Log(C.method(null, "red", $"File not found: {filePath}. Returning string.Empty"));
				return string.Empty;
			}

			// Scenario 0: File exists
			try
			{
				string fileContent = File.ReadAllText(filePath);
				string result = encryptRequired ? Decrypt(fileContent) : fileContent;

				string encStatus = encryptRequired ? "[DECRYPTED]" : "[PLAIN]";
				Debug.Log(C.method(null, "lime", $"Successfully loaded {encStatus} content from: {filePath}"));
				return result;
			}
			catch (CryptographicException)
			{
				Debug.Log(C.method(null, "red", $"Decryption failed for {filePath} (corrupted/tampered). Returning string.Empty."));
				return string.Empty;
			}
			catch (Exception e)
			{
				Debug.Log(C.method(null, "red", $"Error reading {filePath}: {e.Message}. Returning string.Empty."));
				return string.Empty;
			}
		}

		#endregion
	}

	/// <summary>
	/// .ToJson(), .ToTable()
	/// </summary>
	public static partial class LOG
	{
		// Used As: object.ToJson(pretify: true); // with empty string fall back if error parsing
		#region extension .ToJson()
		/// <summary>
		/// Convert A Serielizable (object) To JSON (string).
		/// called as string Json = object.ToJson(true);
		/// </summary>
		public static string ToJson(this object obj, bool pretify = true)
		{
			try
			{
				if (obj == null) return "{ } /* null */";
				return JsonUtility.ToJson(obj, pretify);
			}
			catch (Exception e)
			{
				Debug.Log($"error parsing {obj} to Json reason: {e.Message}".colorTag("red"));
				return "";
				// throw;
			}
		}
		#endregion

		// Used As: LOG.SaveLog(LIST.ToTable(name = "LIST<> "))
		#region extension .ToTable()

		/// <summary>
		/// Helper class to represent either a field or property for unified access
		/// </summary>
		private class MemberAccessor
		{
			public string Name { get; private set; }
			public bool IsPrivate { get; private set; }
			public bool IsStatic { get; private set; }
			public Type MemberType { get; private set; }

			private FieldInfo fieldInfo;
			private PropertyInfo propertyInfo;

			public MemberAccessor(FieldInfo field)
			{
				fieldInfo = field;
				Name = field.Name;
				IsPrivate = field.IsPrivate;
				IsStatic = field.IsStatic;
				MemberType = field.FieldType;
			}

			public MemberAccessor(PropertyInfo property)
			{
				propertyInfo = property;
				Name = property.Name;
				IsPrivate = property.GetMethod?.IsPrivate ?? true;
				IsStatic = property.GetMethod?.IsStatic ?? false;
				MemberType = property.PropertyType;
			}

			public object GetValue(object obj)
			{
				if (fieldInfo != null)
					return fieldInfo.GetValue(obj);
				else if (propertyInfo != null && propertyInfo.CanRead)
					return propertyInfo.GetValue(obj);
				return null;
			}
		}

		/// <summary>
		/// Sanitizes a string by converting control characters and non-printable ASCII to readable representations
		/// </summary>
		private static string SanitizeForTextOutput(string input)
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
						if (char.IsControl(c) || (int)c > 126)
							sb.Append($"(ascii:{(int)c})");
						else
							sb.Append(c);
						break;
				}
			}

			return sb.ToString();
		}

		/// <summary>
		/// <paramref name="toString"/>: by default false, if true each row is logged based on simple value.ToString().flat()
		/// <paramref name="includeProperties"/>: by default true, includes public readable properties alongside fields
		/// Produces a simple ASCII "table" of all public/instance/private fields and properties of each element.
		/// If a field/property's value is any IEnumerable (but not a string), prints its item-count instead of ToString().
		/// Handles control characters and non-printable ASCII safely for text file output.
		/// </summary>
		public static string ToTable<T>(
			this IEnumerable<T> list,
			bool toString = false,
			bool includeProperties = true,
			string name = "LIST<>", char pad = ' ')
		{
			if (list == null)
				return $"* {name}: _list/hash/map/queue is null_";

			var items = list.ToList();
			if (items.Count == 0)
				return $"* {name}: _list/hash/map/queue got no elem_";

			string nameCountHeader = $"* {name} Count: {list.Count()}";

			// @ - if toString enabled
			#region toString enabled
			if (toString == true)
			{
				string str = "";

				// Calculate column widths based on field names and all item-values
				int cw = nameCountHeader.Length;
				foreach (var e in list)
				{
					string flatValue = SanitizeForTextOutput(e.ToString().flat());
					int width = flatValue.Length;
					cw = Mathf.Max(cw, width);
				}

				// Build the header row
				str = nameCountHeader.PadRight(cw + 1).PadLeft(cw + 2);

				// Separator line
				str += '\n' + new string('-', cw + 2) + '\n';

				// Each Row
				foreach (var e in list)
				{
					string sanitizedValue = SanitizeForTextOutput(e.ToString().flat());
					str += sanitizedValue.padRight(cw + 1, pad).padLeft(cw + 2, pad) + '\n';
				}
				return str;
			}
			#endregion

			// @ if toString disabled - use reflection
			#region toString disabled
			var sb = new StringBuilder();
			var type = typeof(T);

			// Gather members (fields and optionally properties)
			var members = new List<MemberAccessor>();

			// Get all fields (public, instance, non-public)
			var fields = type.GetFields(
				BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic
			);
			foreach (var field in fields)
				members.Add(new MemberAccessor(field));

			// Get all properties (public, instance) if enabled
			if (includeProperties)
			{
				var properties = type.GetProperties(
					BindingFlags.Public | BindingFlags.Instance
				);
				foreach (var prop in properties)
				{
					// Only include readable properties
					if (prop.CanRead)
						members.Add(new MemberAccessor(prop));
				}
			}

			// If no members found, show a message
			if (members.Count == 0)
			{
				return $"{nameCountHeader}\n No fields or properties found for type {typeof(T).Name}. Try using toString: true";
			}

			// Helper: get a "display string" for a member value
			string RenderElemValue(object val)
			{
				if (val == null)
					return "null";

				// If it's a string, treat it as a scalar and sanitize it
				if (val is string s)
					return SanitizeForTextOutput(s);

				// If it's any other IEnumerable, count its elements
				if (val is IEnumerable enumerable && !(val is string))
				{
					int count = 0;
					foreach (var _ in enumerable) count += 1;

					var tVal = val.GetType();
					string typeName;

					if (tVal.IsArray)
						typeName = "[]";
					else if (tVal.IsGenericType)
						typeName = tVal.GetGenericTypeDefinition().Name.Split('`')[0];
					else
						typeName = tVal.Name;

					return $"{typeName}: {count}";
				}

				// Otherwise, fallback to ToString() and sanitize
				return SanitizeForTextOutput(val.ToString());
			}

			// Get prefixed member name
			string GetPrefixedMemberName(MemberAccessor accessor)
			{
				string rawName = accessor.Name;
				if (accessor.IsPrivate) rawName = "pri~" + rawName;
				else if (accessor.IsStatic) rawName = "sta~" + rawName;
				else rawName = "~" + rawName;
				return rawName;
			}

			// Helper to format error messages consistently
			string FormatError(Exception e)
			{
				// Keep error message short but consistent between width calc and rendering
				return "(n/a)";
			}

			// 1) Calculate column widths based on member names and all item values
			var columnWidths = new int[members.Count];
			for (int i = 0; i < members.Count; i += 1)
			{
				// base width = member name length
				columnWidths[i] = GetPrefixedMemberName(members[i]).Length;

				// check each item's value in that member
				foreach (var item in items)
				{
					try
					{
						var rawValue = members[i].GetValue(item);
						string disp = RenderElemValue(rawValue);
						columnWidths[i] = Math.Max(columnWidths[i], disp.Length);
					}
					catch (Exception e)
					{
						// Use consistent error formatting
						string errorMsg = FormatError(e);
						columnWidths[i] = Math.Max(columnWidths[i], errorMsg.Length);
					}
				}

				columnWidths[i] += 2; // add some padding
			}

			int sumCw = 0;
			for (int i0 = 0; i0 < columnWidths.Length; i0 += 1)
			{
				sumCw += columnWidths[i0];
				if (i0 <= columnWidths.Length - 2)
					sumCw += 3; // "-+-" or " | "
			}
			//Debug.Log($"sum(columnWidth): {sumCw}, columns: {columnWidths.Length} ");

			// 2) Build the header row ("member names")
			sb.AppendLine(
				string.Join(" | ",
					members.Select((m, idx) =>
					{
						string header = GetPrefixedMemberName(m);
						return header.PadRight(columnWidths[idx]);
					})
			));

			// 3) Separator line
			for (int i = 0; i < members.Count; i += 1)
			{
				sb.Append(new string('-', columnWidths[i]));
				if (i < members.Count - 1)
					sb.Append("-+-");
			}
			sb.AppendLine();

			// 4) Rows: for each item, render each member's value
			foreach (var item in items)
			{
				var rowValues = members.Select((m, idx) =>
				{
					try
					{
						object rawValue = m.GetValue(item);
						string text = RenderElemValue(rawValue);
						return text.PadRight(columnWidths[idx]);
					}
					catch (Exception e)
					{
						// Use consistent error formatting
						string errorMsg = FormatError(e);
						return errorMsg.PadRight(columnWidths[idx]);
					}
				});
				sb.AppendLine(string.Join(" | ", rowValues));
			}

			// * {name} Count: {list.Count()}
			return $"{nameCountHeader.padRight(sumCw)}\n{sb.ToString()}";
			#endregion
		}

		#endregion
	}

	#region /* LOG_prev
	/*
		Used as: 
		LOG.AddLog(str)
		LOG.SaveGameData(str)
		LOG.LoadGameData<T>(json)
		LOG.LoadGameData(str)
	*/
	/*
	// file LOG.INITIALIZE() not required, since EnsureAllDirectoryExists called at runTIme access(in both LoadGameData<>, SaveGameData)
	// LOG.AddLog(str), LoadGameData<T>(enum), LoadGameData(enum), SaveGameData(str), 
	public static partial class LOG
	{
		private static string locRootPath => Application.dataPath;
		private static string locLOGDirectory => Path.Combine(locRootPath, "LOG");
		private static string locLOGFile => Path.Combine(locLOGDirectory, "LOG.md");
		private static string locGameDataDirectory => Path.Combine(locLOGDirectory, "GameData");

		#region private API
		/// <summary>
		/// Ensures the LOG directory structure exists
		/// </summary>
		private static void EnsureAllDirectoryExists()
		{
			// LOG/GameData
			if (!Directory.Exists(locGameDataDirectory))
			{
				Directory.CreateDirectory(locGameDataDirectory);
			}

			// LOG/LOG.md
			if (!File.Exists(locLOGFile))
				File.WriteAllText(locLOGFile, "# LOG.md created, perform LOG.SaveLog(str, format) to append text here:\n\n");
		}
		/// <summary>
		/// Gets the full file path for a given GameDataType
		/// </summary>
		public static string GetGameDataFilePath(string fileName)
		{
			return Path.Combine(locGameDataDirectory, $"{fileName}.json");
		}
		#endregion

		#region public API
		#region AddLog
		public static void AddLog(string str, string syntaxType = "")
		{
			LOG.EnsureAllDirectoryExists();

			if (syntaxType != "")
				str = $"```{syntaxType}\n{str}\n```"; // format for markDown

			// string str = string.Join("\n\n", args);
			//string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
			//string logEntry = $"[{timestamp}] {str}";
			// File logging
			try
			{
				Debug.Log(C.method(null, color: "grey", adMssg: "success wiriting file"));
				System.IO.File.AppendAllText(locLOGFile, str + Environment.NewLine + Environment.NewLine);
			}
			catch (Exception e)
			{
				Debug.Log(C.method(null, color: "red", adMssg: "error wrinting to log file"));
			}
		}

		public static void H(string header) { AddLog($"# {header} >>\n"); }
		public static void HEnd(string header) { AddLog($"# << {header}"); }
		#endregion

		#region LoadGameData<T>(enum), LoadGameData(enum), SaveGameData(str)
		/// <summary>
		/// Load game data and deserialize to type T
		/// Returns default T instance if file doesn't exist or parsing fails
		/// </summary>
		public static T LoadGameData<T>(object dataType) where T : new()
		{
			string filePath = GetGameDataFilePath(dataType.ToString());

			// Scenario 1: File doesn't exist
			if (!File.Exists(filePath))
			{
				Debug.Log(C.method(null, "red", adMssg: $"File not found: {filePath}. Returning default instance."));
				return new T();
			}

			// Scenario 0: File exists
			try
			{
				string jsonContent = File.ReadAllText(filePath);

				// Try to parse JSON
				T data = JsonUtility.FromJson<T>(jsonContent);

				// If parsing failed (returns null or default)
				if (data == null || EqualityComparer<T>.Default.Equals(data, default(T)))
				{
					Debug.Log(C.method(null, "red", adMssg: $"Failed to parse JSON from: {filePath}. Returning default instance."));
					return new T();
				}
				Debug.Log(C.method(null, "lime", adMssg: $"Successfully Loaded JSON from: {filePath}"));
				return data;
			}
			catch (Exception e)
			{
				Debug.Log(C.method(null, "red", adMssg: $"Error loading {filePath}: {e.Message}. Returning default instance."));
				return new T();
			}
		}

		/// <summary>
		/// Load game data as raw JSON string
		/// Returns empty string if file doesn't exist
		/// </summary>
		public static string LoadGameData(object dataType)
		{
			string filePath = GetGameDataFilePath(dataType.ToString());

			if (!File.Exists(filePath))
			{
				Debug.Log(C.method(null, "red", adMssg: $"File not found: {filePath}. Returning string.Empty."));
				return string.Empty;
			}

			try
			{
				string content = File.ReadAllText(filePath);
				Debug.Log(C.method(null, "lime", adMssg: $"Successfully loaded raw content from: {filePath}"));
				return content;
			}
			catch (Exception e)
			{
				Debug.Log(C.method(null, "red", adMssg: $"Error reading {filePath}: {e.Message}"));
				return string.Empty;
			}
		}

		/// <summary>
		/// Save game data from JSON string
		/// Creates file if it doesn't exist, overwrites if it does
		/// </summary>
		public static void SaveGameData(object dataType, string jsonContent)
		{
			EnsureAllDirectoryExists();
			string filePath = GetGameDataFilePath(dataType.ToString());

			try
			{
				File.WriteAllText(filePath, jsonContent);
				Debug.Log(C.method(null, "lime", $"success saving @ {filePath}")); // Debug.Log($"[LOG.SaveGameData()] Successfully saved: {filePath}");
			}
			catch (Exception e)
			{
				Debug.Log(C.method(null, "red", $"error saving @ {filePath}")); // Debug.Log($"[LOG.SaveGameData()] Error saving {filePath}: {e.Message}".colorTag("red"));
			}
		}
		#endregion
		#endregion
	}
	*/
	#endregion
	#endregion
}

namespace SPACE_prev
{
	// for DRAW prev legacy -> now its SPACE_DrawSystem(with chainable creation during runtime(with id blocking))
	#region ad DRAW_prev
	public static class DRAW_prev
	{
		public static Color col = Color.red;
		public static float dt = 10f;

		#region LINE
		public static void Line(Vector3 a, Vector3 b, float e = 1f / 200)
		{
			Vector3 nX = b - a,
							nY = -Vector3.Cross(-Vector3.forward, nX).normalized;

			Debug.DrawLine(a - nY * e, b - nY * e, DRAW_prev.col, DRAW_prev.dt);
			Debug.DrawLine(a + nY * e, b + nY * e, DRAW_prev.col, DRAW_prev.dt);
		}
		#endregion

		#region ARROW
		public static void Arrow(Vector3 a, Vector3 b, float t = 1f, float s = 1f / 15, float e = 1f / 200)
		{
			Vector3 nX = (b - a).normalized,
							nY = -Vector3.Cross(-Vector3.forward, nX).normalized;

			DRAW_prev.Line(a, b, e);
			DRAW_prev.Line(lerp(a, b, t) - nX * (s * 1.6f) + nY * s, lerp(a, b, t), e);
			DRAW_prev.Line(lerp(a, b, t) - nX * (s * 1.6f) - nY * s, lerp(a, b, t), e);
		}

		static Vector3 lerp(Vector3 a, Vector3 b, float t)
		{
			Vector3 n = b - a;
			return a + n * t;
		}
		#endregion



	}
	#endregion
}