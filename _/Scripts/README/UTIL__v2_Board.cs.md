# SPACE_UTIL Documentation: v2 & Board<T> Builtin General Utility for 2D Grid Based
*Source: UTIL.cs*

## v2 Struct (require namespace: SPACE_UTIL)

A 2D integer coordinate struct with comprehensive operators and utility methods.

### Constructor & Basic Usage
```cs
using SPACE_UTIL;

v2 a = new v2(3, 4);
v2 b = (1, 2);  // implicit tuple conversion support
```

### Operators
```cs
v2 sum = a + b;      // (4, 6)
v2 diff = a - b;     // (2, 2)
v2 mult = a * b;     // component-wise: (3, 8)
v2 scale = a * 2;    // (6, 8)
v2 scale2 = 3 * a;   // (9, 12)

bool equal = a == b;     // false
bool greater = a > b;    // true (either x OR y greater)
bool less = a <= b;      // false
```

### Static Methods
```cs
// Dot product and cross product area
float dot = v2.dot(a, b);      // 11
float area = v2.area(a, b);    // 2

// Direction utilities
List<v2> dirs = v2.getDIR(diagonal: true);  // 8 directions
v2 right = v2.getdir("r");     // (1, 0)
v2 upRight = v2.getdir("ur");  // (1, 1)
```

### Unity Integration
```cs
// Axis configuration (default 'y')
v2.axisY = 'y';  // or 'z' for XZ plane

// Automatic conversions
Vector3 pos3d = new Vector3(5, 3, 0);
v2 coord = pos3d;  // (5, 3)

Vector2 pos2d = new Vector2(2.7f, 1.3f);
v2 rounded = pos2d;  // (3, 1) - uses C.round()

// Convert back
Vector3 world = coord;  // depends on axisY setting
Vector2 screen = coord;
```

## Board<T> Class (require namespace: SPACE_UTIL)

A 2D grid container with bounds checking and convenient access methods.

### Constructor
```cs
Board<char> grid = new Board<char>((10, 8), '.');
// Creates 10x8 grid filled with '.'
// Access bounds: (0,0) to (9,7)
```

### Properties
```cs
int width = grid.w;    // 10
int height = grid.h;   // 8
v2 min = grid.m;       // (0, 0)
v2 max = grid.M;       // (9, 7)
```

### Access Methods
```cs
// Get/Set with automatic bounds checking
grid.ST((3, 4), 'X');    // Set position
char value = grid.GT((3, 4));  // Get 'X'

// Direct array access (no bounds check)
char direct = grid.B[4][3];  // B[y][x] format
```

### Cloning
```cs
Board<char> copy = grid.clone;  // Deep copy
```

### String Representation
```cs
Debug.Log(grid.ToString());
// Prints grid with Y-axis flipped (top-to-bottom)
// Origin (0,0) appears at bottom-left
```

### Typical Usage Patterns
```cs
// Initialize game board
Board<int> gameGrid = new Board<int>((20, 15), 0);

// Place objects
gameGrid.ST((5, 7), 1);  // Player at (5,7)
gameGrid.ST((12, 3), 2); // Enemy at (12,3)

// Check positions
if (gameGrid.GT(playerPos) == 0) {
    // Empty space, can move
}

// Iterate through grid
for (int y = 0; y < gameGrid.h; y++) {
    for (int x = 0; x < gameGrid.w; x++) {
        v2 pos = (x, y);
        ProcessCell(gameGrid.GT(pos));
    }
}
```

### Error Handling
Both GT() and ST() methods log errors for out-of-bounds access but don't throw exceptions. Always ensure coordinates are within (0,0) to (board.w-1, board.h-1) range, 

---
<!-- `checksum: 2020-sept-20 v3k` -->