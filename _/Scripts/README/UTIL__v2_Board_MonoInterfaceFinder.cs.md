# SPACE_UTIL Documentation: v2 & Board<T> Builtin General Utility for 2D Grid Based
*Source: UTIL.cs*

## v2 Struct (require namespace: SPACE_UTIL)
2D integer vector with implicit tuple conversion and comprehensive operators.

```csharp
// Construction
v2 pos = new v2(5, 3);
v2 pos = (5, 3);  // implicit from tuple

// Operations
v2 a = (1, 2);
v2 b = (3, 4);
v2 sum = a + b;      // (4, 6)
v2 diff = a - b;     // (-2, -2)
v2 mult = a * b;     // (3, 8)
v2 scaled = a * 3;   // (3, 6)

// Dot product and cross product area
float dot = v2.dot(a, b);    // 11
float area = v2.area(a, b);  // -2

// Comparisons (component-wise OR logic)
bool greater = a > b;  // false (neither x>x nor y>y)
bool equal = a == b;   // false

// Direction utilities
List<v2> dirs = v2.getDIR(diagonal: true);  // 8 directions
v2 right = v2.getdir("r");     // (1, 0)
v2 upRight = v2.getdir("ur");  // (1, 1)

// Unity Vector conversion (depends on v2.axisY setting)
v2.axisY = 'y';  // default, uses XY plane
Vector3 vec3 = pos;  // converts to Vector3
Vector2 vec2 = pos;  // converts to Vector2
```

## Board<T> Class (require namespace: SPACE_UTIL)
Generic 2D grid with bounds checking and coordinate access.

```csharp
// Construction
Board<int> grid = new Board<int>((5, 5), defaultValue: 0);
// Creates 5x5 grid filled with zeros

// Access (with bounds checking)
grid.ST((2, 3), 42);    // Set value at (2,3)
int val = grid.GT((2, 3));  // Get value at (2,3)

// Properties
int width = grid.w;    // 5
int height = grid.h;   // 5
v2 min = grid.m;       // (0, 0)
v2 max = grid.M;       // (4, 4)

// Display (Y-axis flipped for visual consistency)
string display = grid.ToString();

// Cloning
Board<int> copy = grid.clone;
```

## MonoInterfaceFinder Class (require namespace: SPACE_UTIL)
Find MonoBehaviours implementing specific interfaces in the scene.

```csharp
// Find first MonoBehaviour implementing interface
var manager = MonoInterfaceFinder.FindInterface<IGameManager>();

// Find all MonoBehaviours implementing interface
var allManagers = MonoInterfaceFinder.FindAllInterfaces<IGameManager>();

// Multiple interface casting
var testable = manager as ITestable;
testable?.RunAllTests();
```

**Usage Notes:**
- All classes require `using SPACE_UTIL;` or full qualification
- v2 depends on C.round() for Unity Vector conversions
- Board<T> depends on v2 for coordinate system
- MonoInterfaceFinder only finds active GameObjects
- Returns null/empty if no implementations found

**Strict Requirements:**
- **For 2D coordinates:** Always use v2 instead of Vector2/Vector3 or int tuples
- **For 2D grid systems:** Always use Board<T> instead of native arrays
- **For interface discovery:** Always use MonoInterfaceFinder instead of manual GameObject searches