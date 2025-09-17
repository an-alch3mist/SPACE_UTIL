# Source: `UTIL__v2_Board.cs` — 2D vector and generic board utilities for Unity game development

## Short description (2–4 sentences)
This file implements a custom 2D integer vector struct (`v2`) with operator overloads and direction utilities, plus a generic 2D board class (`Board<T>`) for grid-based game logic. The `v2` struct provides directional movement, coordinate arithmetic, and conversion between Unity's Vector2/Vector3 types. The `Board<T>` class manages 2D grids with bounds checking and cloning capabilities for game state management.

## Metadata

* **Filename:** `UTIL__v2_Board.cs`
* **Primary namespace:** `SPACE_UTIL`
* **Dependent namespace:** `UnityEngine`, `UnityEngine.EventSystems`, `UnityEngine.UI`, `TMPro`, `System`, `System.Linq`, `System.Collections`, `System.Collections.Generic`, `System.Text`, `System.Reflection`, `System.Text.RegularExpressions`, `System.Threading.Tasks`
* **Estimated lines:** 158
* **Estimated chars:** 4,847
* **Public types:** `v2 (struct)`, `Board<T> (class does not inherit MonoBehaviour)`
* **Unity version / Target framework (if detectable):** Unity 2020.3 / .NET Standard 2.0
* **Dependencies:** Depends on external `C` class for `C.round()` method, Unity Vector2/Vector3 types

## Public API summary (table)

| **Type** | **Member** | **Signature** | **Short purpose** | **OneLiner Call** |
|----------|------------|---------------|------------------|-------------------|
| `int` | `x` | `public int x;` | X coordinate component | `var xVal = v2Instance.x;` |
| `int` | `y` | `public int y;` | Y coordinate component | `var yVal = v2Instance.y;` |
| `char (static)` | `axisY` | `public static char axisY` | Controls Y-axis mapping for Vector3 conversion | `var axis = v2.axisY;` |
| `v2` | `v2(int, int)` | `public v2(int x, int y)` | Constructor for v2 struct | `var vec = new v2(1, 2);` |
| `string` | `ToString()` | `public override string ToString()` | String representation of coordinates | `string str = vec.ToString();` |
| `v2 (static)` | `operator +` | `public static v2 operator +(v2 a, v2 b)` | Vector addition | `var sum = vecA + vecB;` |
| `v2 (static)` | `operator -` | `public static v2 operator -(v2 a, v2 b)` | Vector subtraction | `var diff = vecA - vecB;` |
| `v2 (static)` | `operator *` | `public static v2 operator *(v2 a, v2 b)` | Component-wise multiplication | `var mult = vecA * vecB;` |
| `v2 (static)` | `operator * (v2, int)` | `public static v2 operator *(v2 v, int m)` | Scalar multiplication | `var scaled = vec * 5;` |
| `v2 (static)` | `operator * (int, v2)` | `public static v2 operator *(int m, v2 v)` | Scalar multiplication (reversed) | `var scaled = 5 * vec;` |
| `float (static)` | `dot` | `public static float dot(v2 a, v2 b)` | Dot product calculation | `float dotResult = v2.dot(vecA, vecB);` |
| `float (static)` | `area` | `public static float area(v2 a, v2 b)` | Cross product for area calculation | `float areaResult = v2.area(vecA, vecB);` |
| `bool (static)` | `operator ==` | `public static bool operator ==(v2 a, v2 b)` | Equality comparison | `bool equal = vecA == vecB;` |
| `bool (static)` | `operator !=` | `public static bool operator !=(v2 a, v2 b)` | Inequality comparison | `bool notEqual = vecA != vecB;` |
| `bool (static)` | `operator >` | `public static bool operator >(v2 a, v2 b)` | Greater than comparison (OR logic) | `bool greater = vecA > vecB;` |
| `bool (static)` | `operator <` | `public static bool operator <(v2 a, v2 b)` | Less than comparison (OR logic) | `bool less = vecA < vecB;` |
| `bool (static)` | `operator >=` | `public static bool operator >=(v2 a, v2 b)` | Greater or equal comparison (OR logic) | `bool greaterEq = vecA >= vecB;` |
| `bool (static)` | `operator <=` | `public static bool operator <=(v2 a, v2 b)` | Less or equal comparison (OR logic) | `bool lessEq = vecA <= vecB;` |
| `List<v2> (static)` | `getDIR` | `public static List<v2> getDIR(bool diagonal = false)` | Gets directional movement vectors | `var directions = v2.getDIR(true);` |
| `v2 (static)` | `getdir` | `public static v2 getdir(string dir_str = "r")` | Converts direction string to vector | `var direction = v2.getdir("ru");` |
| `Board<T>` | `Board<T>(v2, T)` | `public Board(v2 size, T default_val)` | Constructor for generic board | `var board = new Board<int>((5, 5), 0);` |
| `int` | `w` | `public int w;` | Board width | `var width = board.w;` |
| `int` | `h` | `public int h;` | Board height | `var height = board.h;` |
| `v2` | `m` | `public v2 m;` | Minimum coordinate bounds | `var min = board.m;` |
| `v2` | `M` | `public v2 M;` | Maximum coordinate bounds | `var max = board.M;` |
| `T[][]` | `B` | `public T[][] B;` | 2D array storage | `var cell = board.B[y][x];` |
| `T` | `GT` | `public T GT(v2 coord)` | Gets value at coordinate | `var value = board.GT((x, y));` |
| `void` | `ST` | `public void ST(v2 coord, T val)` | Sets value at coordinate | `board.ST((x, y), value);` |
| `string` | `ToString()` | `public override string ToString()` | String representation of board | `string boardStr = board.ToString();` |
| `Board<T>` | `clone` | `public Board<T> clone { get; }` | Creates deep copy of board | `var copy = board.clone;` |

## Important types — details

### `v2` (struct)
* **Kind:** struct with full path `SPACE_UTIL.v2`
* **Responsibility:** Represents 2D integer coordinates with arithmetic operations and Unity integration
* **MonoBehaviour Status:** Inherits MonoBehaviour: No (struct type)
* **Constructor(s):** `v2(int x, int y)` — initializes coordinate components
* **Public properties / fields:** 
  * `x — int — X coordinate component`
  * `y — int — Y coordinate component` 
  * `axisY — char (static) — Controls Vector3 conversion axis mapping (default 'y')`

* **Public methods:**
  * **Signature:** `public override string ToString()`
    * **Description:** Returns coordinate string in format "(x, y)"
    * **Parameters:** none
    * **Returns:** string — formatted coordinate string, example call: `string coordStr = vec.ToString();`
    * **Throws:** none
    * **Side effects / state changes:** none
    * **Complexity / performance:** O(1) / minimal allocation
    * **Notes:** Overrides default ToString behavior

  * **Signature:** `public static v2 operator +(v2 a, v2 b)`
    * **Description:** Adds two vectors component-wise
    * **Parameters:** a : v2 — first vector, b : v2 — second vector
    * **Returns:** v2 — sum vector, example call: `v2 result = vecA + vecB;`
    * **Throws:** none
    * **Side effects / state changes:** none
    * **Complexity / performance:** O(1) / no allocation
    * **Notes:** Component-wise addition

  * **Signature:** `public static v2 operator -(v2 a, v2 b)`
    * **Description:** Subtracts second vector from first component-wise
    * **Parameters:** a : v2 — minuend vector, b : v2 — subtrahend vector
    * **Returns:** v2 — difference vector, example call: `v2 result = vecA - vecB;`
    * **Throws:** none
    * **Side effects / state changes:** none
    * **Complexity / performance:** O(1) / no allocation
    * **Notes:** Component-wise subtraction

  * **Signature:** `public static float dot(v2 a, v2 b)`
    * **Description:** Calculates dot product of two vectors
    * **Parameters:** a : v2 — first vector, b : v2 — second vector
    * **Returns:** float — dot product result, example call: `float result = v2.dot(vecA, vecB);`
    * **Throws:** none
    * **Side effects / state changes:** none
    * **Complexity / performance:** O(1) / no allocation
    * **Notes:** Standard mathematical dot product

  * **Signature:** `public static List<v2> getDIR(bool diagonal = false)`
    * **Description:** Returns list of directional movement vectors
    * **Parameters:** diagonal : bool — include diagonal directions if true
    * **Returns:** List<v2> — movement directions, example call: `var dirs = v2.getDIR(true);`
    * **Throws:** none
    * **Side effects / state changes:** allocates new List<v2>
    * **Complexity / performance:** O(1) / List allocation
    * **Notes:** Returns 4 or 8 directions based on diagonal parameter

  * **Signature:** `public static v2 getdir(string dir_str = "r")`
    * **Description:** Converts direction string to movement vector
    * **Parameters:** dir_str : string — direction string using 'r', 'u', 'l', 'd' characters
    * **Returns:** v2 — direction vector, example call: `v2 dir = v2.getdir("ru");`
    * **Throws:** none
    * **Side effects / state changes:** none
    * **Complexity / performance:** O(n) where n is string length / no allocation
    * **Notes:** Supports combined directions like "ru" for diagonal movement

### `Board<T>` (class)
* **Kind:** class with full path `SPACE_UTIL.Board<T>`
* **Responsibility:** Generic 2D grid container with bounds checking and cloning
* **MonoBehaviour Status:** Inherits MonoBehaviour: No
* **Constructor(s):** `Board(v2 size, T default_val)` — creates board with specified dimensions and default value
* **Public properties / fields:**
  * `w — int — Board width`
  * `h — int — Board height`
  * `m — v2 — Minimum coordinate (0, 0)`
  * `M — v2 — Maximum coordinate (w-1, h-1)`
  * `B — T[][] — 2D jagged array storage`
  * `clone — Board<T> { get; } — Creates deep copy of board`

* **Public methods:**
  * **Signature:** `public T GT(v2 coord)`
    * **Description:** Gets value at specified coordinate with bounds checking
    * **Parameters:** coord : v2 — target coordinate
    * **Returns:** T — value at coordinate, example call: `T value = board.GT((x, y));`
    * **Throws:** Debug.LogError if coordinate out of bounds
    * **Side effects / state changes:** logs error on invalid coordinates
    * **Complexity / performance:** O(1) / no allocation
    * **Notes:** Performs bounds validation before access

  * **Signature:** `public void ST(v2 coord, T val)`
    * **Description:** Sets value at specified coordinate with bounds checking
    * **Parameters:** coord : v2 — target coordinate, val : T — value to set
    * **Returns:** void — example call: `board.ST((x, y), value);`
    * **Throws:** Debug.LogError if coordinate out of bounds
    * **Side effects / state changes:** modifies board state, logs error on invalid coordinates
    * **Complexity / performance:** O(1) / no allocation
    * **Notes:** Performs bounds validation before assignment

  * **Signature:** `public override string ToString()`
    * **Description:** Returns string representation of board contents
    * **Parameters:** none
    * **Returns:** string — board visualization, example call: `string boardStr = board.ToString();`
    * **Throws:** none
    * **Side effects / state changes:** none
    * **Complexity / performance:** O(w*h) / string allocation
    * **Notes:** Displays board with Y-axis inverted (top-down)

## Example usage

```csharp
// Required namespaces:
// using System;
// using System.Collections.Generic;
// using UnityEngine;
// using SPACE_UTIL;

public class ExampleUsage : MonoBehaviour 
{
    private void UTIL_v2_Board_Check()
    {
        // === v2 Struct Usage ===
        
        // Basic vector creation and operations
        var pos1 = new v2(3, 4);
        var pos2 = new v2(1, 2);
        var sum = pos1 + pos2;
        var diff = pos1 - pos2;
        
        // Expected output: "Position: (3, 4), Sum: (4, 6), Diff: (2, 2)"
        Debug.Log($"<color=green>Position: {pos1}, Sum: {sum}, Diff: {diff}</color>");
        
        // Tuple conversion and direction utilities
        v2 origin = (0, 0);
        var directions = v2.getDIR(true); // Include diagonals
        var rightUp = v2.getdir("ru");
        
        // Expected output: "Origin: (0, 0), Directions count: 8, Right-Up: (1, 1)"
        Debug.Log($"<color=green>Origin: {origin}, Directions count: {directions.Count}, Right-Up: {rightUp}</color>");
        
        // Mathematical operations
        float dotProduct = v2.dot(pos1, pos2);
        float crossArea = v2.area(pos1, pos2);
        
        // Expected output: "Dot: 11, Area: 2"
        Debug.Log($"<color=green>Dot: {dotProduct}, Area: {crossArea}</color>");
        
        // === Board<T> Class Usage ===
        
        // Create and populate board
        var gameBoard = new Board<char>((5, 5), '.');
        gameBoard.ST((2, 2), 'X');
        gameBoard.ST((1, 3), 'O');
        gameBoard.ST((4, 1), '#');
        
        // Read values
        char centerValue = gameBoard.GT((2, 2));
        char emptyValue = gameBoard.GT((0, 0));
        
        // Expected output: "Center: X, Empty: ., Board size: 5x5"
        Debug.Log($"<color=green>Center: {centerValue}, Empty: {emptyValue}, Board size: {gameBoard.w}x{gameBoard.h}</color>");
        
        // Clone board for state management
        var backupBoard = gameBoard.clone;
        backupBoard.ST((2, 2), 'Y');
        
        char originalCenter = gameBoard.GT((2, 2));
        char clonedCenter = backupBoard.GT((2, 2));
        
        // Expected output: "Original center: X, Cloned center: Y"
        Debug.Log($"<color=green>Original center: {originalCenter}, Cloned center: {clonedCenter}</color>");
        
        // Board visualization
        string boardDisplay = gameBoard.ToString();
        // Expected output: Board string representation (5x5 grid)
        Debug.Log($"<color=cyan>Board visualization:\n{boardDisplay}</color>");
        
        // Vector conversion examples (requires Unity Vector types)
        Vector2 unityVec2 = pos1; // Implicit conversion
        Vector3 unityVec3 = pos1; // Uses axisY setting
        
        // Expected output: "Unity Vec2: (3.0, 4.0), Vec3: (3.0, 4.0, 0.0)"
        Debug.Log($"<color=yellow>Unity Vec2: {unityVec2}, Vec3: {unityVec3}</color>");
    }
}
```

## Control flow / responsibilities & high-level algorithm summary /Side effects and I/O
v2 provides coordinate arithmetic and direction utilities; Board<T> manages 2D grids with bounds validation. No I/O operations, minimal side effects (error logging).

## Performance, allocations, and hotspots / Threading / async considerations
v2 operations are allocation-free; Board creation allocates jagged array; clone creates deep copy allocation.

## Security / safety / correctness concerns
Depends on external C.round() method; Board bounds checking uses Debug.LogError instead of exceptions.

## Tests, debugging & observability
Built-in bounds checking with Debug.LogError logging; ToString() methods provide state visualization for debugging.

## Cross-file references
Depends on external `C` class for `C.round()` method used in Vector2/Vector3 conversion operators.

## General Note: important behaviors
Major functionality includes coordinate arithmetic, directional movement utilities, and generic 2D grid management with cloning support for game state backup/restore scenarios.

`checksum: a1b2c3d4 (v0.5)`