# Source: `UTIL__v2_Board.cs` — 2D vector operations and generic board grid utilities

## Short description
Provides a custom 2D integer vector struct (v2) with comprehensive operators and direction utilities, plus a generic Board<T> class for 2D grid operations. The v2 struct handles Unity Vector2/Vector3 conversions and supports directional movement calculations commonly used in grid-based games.

## Metadata
* **Filename:** `UTIL__v2_Board.cs`
* **Primary namespace:** `SPACE_UTIL`
* **Dependencies:** `System`, `System.Linq`, `System.Collections`, `System.Collections.Generic`, `System.Text`, `System.Reflection`, `System.Text.RegularExpressions`, `UnityEngine`, `UnityEngine.EventSystems`, `UnityEngine.UI`, `TMPro`, `System.Threading.Tasks`
* **Public types:** `v2 (struct)`, `Board<T> (class)`
* **Unity version:** (Unity-dependent due to Vector2/Vector3 conversions)

## Public API Summary
| Class | Member Type | Member | Signature | Short Purpose | OneLiner Call |
|-------|-------------|---------|-----------|---------------|---------------|
| v2 | Field | x | public int x | X coordinate | var xVal = coord.x; |
| v2 | Field | y | public int y | Y coordinate | var yVal = coord.y; |
| v2 | Constructor | v2 | public v2(int x, int y) | Creates vector | var coord = new v2(5, 3); |
| v2 | Method | ToString | public override string ToString() | String representation | var str = coord.ToString(); |
| v2 | Method | + | public static v2 operator +(v2 a, v2 b) | Vector addition | var sum = coordA + coordB; |
| v2 | Method | - | public static v2 operator -(v2 a, v2 b) | Vector subtraction | var diff = coordA - coordB; |
| v2 | Method | * | public static v2 operator *(v2 a, v2 b) | Component-wise multiply | var mult = coordA * coordB; |
| v2 | Method | * | public static v2 operator *(v2 v, int m) | Scalar multiply | var scaled = coord * 3; |
| v2 | Method | * | public static v2 operator *(int m, v2 v) | Scalar multiply (reverse) | var scaled = 3 * coord; |
| v2 | Method | dot | public static float dot(v2 a, v2 b) | Dot product | var dotProduct = v2.dot(coordA, coordB); |
| v2 | Method | area | public static float area(v2 a, v2 b) | Cross product area | var area = v2.area(coordA, coordB); |
| v2 | Method | == | public static bool operator ==(v2 a, v2 b) | Equality comparison | bool equal = coordA == coordB; |
| v2 | Method | != | public static bool operator !=(v2 a, v2 b) | Inequality comparison | bool notEqual = coordA != coordB; |
| v2 | Method | > | public static bool operator >(v2 a, v2 b) | Greater than (OR logic) | bool greater = coordA > coordB; |
| v2 | Method | < | public static bool operator <(v2 a, v2 b) | Less than (OR logic) | bool less = coordA < coordB; |
| v2 | Method | >= | public static bool operator >=(v2 a, v2 b) | Greater or equal (OR logic) | bool greaterEq = coordA >= coordB; |
| v2 | Method | <= | public static bool operator <=(v2 a, v2 b) | Less or equal (OR logic) | bool lessEq = coordA <= coordB; |
| v2 | Method | getDIR | public static List<v2> getDIR(bool diagonal = false) | Gets direction vectors | var dirs = v2.getDIR(true); |
| v2 | Method | getdir | public static v2 getdir(string dir_str = "r") | Direction from string | var dir = v2.getdir("ru"); |
| v2 | Field | axisY | public static char axisY | Unity Y-axis mode | v2.axisY = 'z'; |
| Board<T> | Field | w | public int w | Board width | var width = board.w; |
| Board<T> | Field | h | public int h | Board height | var height = board.h; |
| Board<T> | Field | m | public v2 m | Minimum bounds | var min = board.m; |
| Board<T> | Field | M | public v2 M | Maximum bounds | var max = board.M; |
| Board<T> | Field | B | public T[][] B | Raw 2D array | var cell = board.B[y][x]; |
| Board<T> | Constructor | Board | public Board(v2 size, T default_val) | Creates board | var board = new Board<int>((10, 10), 0); |
| Board<T> | Method | GT | public T GT(v2 coord) | Gets value at coordinate | var value = board.GT((5, 3)); |
| Board<T> | Method | ST | public void ST(v2 coord, T val) | Sets value at coordinate | board.ST((5, 3), newValue); |
| Board<T> | Method | ToString | public override string ToString() | String representation | var str = board.ToString(); |
| Board<T> | Property | clone | public Board<T> clone { get; } | Deep copy of board | var copy = board.clone; |
## Important Types

### `v2`
* **Kind:** struct with implicit conversion operators
* **Responsibility:** Represents 2D integer coordinates with comprehensive operators and Unity integration
* **Constructor(s):** `public v2(int x, int y)` — Creates coordinate vector
* **Public Properties:**
  * `x` — `int` — X coordinate (`get/set`)
  * `y` — `int` — Y coordinate (`get/set`)
  * `axisY` — `static char` — Unity Y-axis mapping ('y' or 'z') (`get/set`)
* **Public Methods:**
  * **`public static List<v2> getDIR(bool diagonal = false)`**
    * Description: Returns list of direction vectors for movement
    * Parameters: `diagonal : bool — Include diagonal directions`
    * Returns: `List<v2> — Direction vectors` + call example: `var dirs = v2.getDIR(true);`
    * Notes: Returns 4 or 8 directions based on diagonal parameter
  * **`public static v2 getdir(string dir_str = "r")`**
    * Description: Converts direction string to vector
    * Parameters: `dir_str : string — Direction letters (r/u/l/d combinations)`
    * Returns: `v2 — Direction vector` + call example: `var dir = v2.getdir("ru");`
    * Notes: Supports compound directions like "ru" for right-up

### `Board<T>`
* **Kind:** generic class for 2D grid storage
* **Responsibility:** Manages 2D array with coordinate-based access and bounds checking
* **Constructor(s):** `public Board(v2 size, T default_val)` — Creates board with specified size and default value
* **Public Properties:**
  * `w` — `int` — Board width (`get`)
  * `h` — `int` — Board height (`get`)
  * `m` — `v2` — Minimum coordinate bounds (0,0) (`get`)
  * `M` — `v2` — Maximum coordinate bounds (w-1,h-1) (`get`)
  * `B` — `T[][]` — Raw 2D jagged array access (`get`)
  * `clone` — `Board<T>` — Deep copy of entire board (`get`)
* **Public Methods:**
  * **`public T GT(v2 coord)`**
    * Description: Gets value at coordinate with bounds checking
    * Parameters: `coord : v2 — Target coordinate`
    * Returns: `T — Value at coordinate` + call example: `var value = board.GT((5, 3));`
    * Notes: Logs error if coordinate out of bounds
  * **`public void ST(v2 coord, T val)`**
    * Description: Sets value at coordinate with bounds checking
    * Parameters: `coord : v2 — Target coordinate`, `val : T — Value to set`
    * Returns: `void — No return value` + call example: `board.ST((5, 3), newValue);`
    * Notes: Logs error if coordinate out of bounds

## Example Usage
**Required namespaces:**
```csharp
// using System;
// using System.Collections.Generic;
// using UnityEngine;
// using SPACE_UTIL;
```

**For files with only Non-MonoBehaviour root classes:**
```csharp
public class ExampleUsage : MonoBehaviour 
{
    private void v2_Board_Check()
    {
        // Test v2 struct APIs
        var coordA = new v2(3, 4);
        var coordB = new v2(1, 2);
        var sum = coordA + coordB;
        var diff = coordA - coordB;
        var dotProduct = v2.dot(coordA, coordB);
        var directions = v2.getDIR(true);
        var rightUp = v2.getdir("ru"); // right + up
        
        // Test Board<T> APIs
        var board = new Board<char>((10, 10), '.');
        board.ST((5, 3), 'Z');
        var value = board.GT((5, 3));
        var copy = board.clone;
        var boardStr = board.ToString();
        
        Debug.Log($"API Results: Sum:{sum}, Diff:{diff}, Dot:{dotProduct}, Dirs:{directions.Count}, RightUp:{rightUp}, Value:{value}, Board copied, String generated");
    }
}
```

## Control Flow & Responsibilities
v2 handles coordinate math and Unity conversions; Board manages 2D grid storage with bounds checking.

## Performance & Threading
Board clone creates full deep copy; all operations main-thread safe.

## Cross-file Dependencies
Depends on class C for rounding operations in Unity vector conversions.

## Major Functionality
Coordinate arithmetic, direction utilities, 2D grid management, Unity Vector2/Vector3 integration.

`checksum: 7a2b9c1f v0.8.min`