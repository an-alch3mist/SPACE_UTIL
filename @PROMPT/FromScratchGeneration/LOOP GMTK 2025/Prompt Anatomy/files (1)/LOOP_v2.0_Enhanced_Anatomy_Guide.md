# LOOP Language v2.0 - Enhanced Anatomy & Modification Guide

## 📚 Table of Contents

1. [What's New in v2.0](#whats-new-in-v20)
2. [Enum System Architecture](#enum-system-architecture)
3. [Built-in Constants System](#built-in-constants-system)
4. [How to Add New Enums](#how-to-add-new-enums)
5. [How to Add New Constants](#how-to-add-new-constants)
6. [How to Add Game Functions That Use Enums](#how-to-add-game-functions-that-use-enums)
7. [Complete Workflow Examples](#complete-workflow-examples)
8. [Understanding the Cascading Updates](#understanding-the-cascading-updates)

---

## 🆕 What's New in v2.0

### New Features Added

Based on the "Farmer Was Replaced" game screenshot, v2.0 adds:

1. **✨ Enum Support**
   - `Grounds.Soil`, `Grounds.Turf`, `Grounds.Grassland`
   - `Items.Hay`, `Items.Wood`, `Items.Carrot`, etc.
   - `Entities.Grass`, `Entities.Bush`, `Entities.Tree`, etc.

2. **✨ Built-in Constants**
   - `North`, `South`, `East`, `West` for movement
   - Pre-registered global variables

3. **✨ New Operators**
   - `**` (exponentiation operator)
   - String concatenation with `+`
   - `//` (integer division)

4. **✨ Complete Farmer Was Replaced API**
   - 15+ game functions from the screenshot
   - Proper enum parameter support
   - Distinction between yielding and instant functions

---

## 🏗️ Enum System Architecture

### How Enums Work in LOOP

Unlike Python, LOOP enums are **built-in** and **not user-definable**.

**Key Design Decisions:**

```
User writes:  if get_ground_type() == Grounds.Soil:
              
What happens:
1. Lexer sees "Grounds" → IDENTIFIER token
2. Lexer sees "." → DOT token
3. Lexer sees "Soil" → IDENTIFIER token
4. Parser creates: MemberAccessExpr(VariableExpr("Grounds"), "Soil")
5. Interpreter evaluates:
   a. Resolve "Grounds" → Returns Grounds enum object
   b. Access "Soil" member → Returns "soil" string
6. Comparison: "soil" == "soil" → True
```

**Why strings internally?**
- Simplicity: No need for complex enum equality logic
- Unity compatibility: Easy to serialize and debug
- Function parameters: Can pass strings directly to Unity methods

### Enum Implementation Pattern

**In C# (GameEnums.cs):**
```csharp
public static class Grounds {
    public static readonly string Soil = "soil";
    public static readonly string Turf = "turf";
    public static readonly string Grassland = "grassland";
}

public static class Items {
    public static readonly string Hay = "hay";
    public static readonly string Wood = "wood";
    public static readonly string Carrot = "carrot";
    // ... etc
}
```

**Registration (in PythonInterpreter.cs initialization):**
```csharp
// Create enum objects that support member access
var groundsEnum = new EnumObject();
groundsEnum.AddMember("Soil", "soil");
groundsEnum.AddMember("Turf", "turf");
groundsEnum.AddMember("Grassland", "grassland");

globalScope.Set("Grounds", groundsEnum);

// Repeat for Items, Entities, etc.
```

**EnumObject class:**
```csharp
public class EnumObject {
    private Dictionary<string, string> members = new Dictionary<string, string>();
    
    public void AddMember(string name, string value) {
        members[name] = value;
    }
    
    public string GetMember(string name) {
        if (!members.ContainsKey(name)) {
            throw new RuntimeError($"Enum member '{name}' not found");
        }
        return members[name];
    }
}
```

**In Interpreter evaluation:**
```csharp
object EvaluateMemberAccess(MemberAccessExpr expr) {
    object obj = Evaluate(expr.Object);
    
    if (obj is EnumObject enumObj) {
        return enumObj.GetMember(expr.Member);
    }
    
    // Handle other member access (class instances, etc.)
    // ...
}
```

---

## 🎯 Built-in Constants System

### How Constants Work

Built-in constants are **pre-registered global variables** with (optional) read-only protection.

**Implementation Pattern:**

```csharp
// In PythonInterpreter initialization
globalScope.Set("North", "up");
globalScope.Set("South", "down");
globalScope.Set("East", "right");
globalScope.Set("West", "left");

// Optional: Track read-only variables
HashSet<string> readOnlyVariables = new HashSet<string> {
    "North", "South", "East", "West"
};
```

**Read-only enforcement (optional but recommended):**
```csharp
void ExecuteAssignment(AssignmentStmt stmt) {
    if (readOnlyVariables.Contains(stmt.Target)) {
        throw new RuntimeError($"Cannot assign to constant '{stmt.Target}'");
    }
    
    // Normal assignment logic
    // ...
}
```

**Why this approach?**
- Simple: Just pre-populate global scope
- Flexible: Easy to add more constants
- Optional protection: Read-only enforcement can be added later

---

## 📝 How to Add New Enums

### Step-by-Step Guide

**Example: Adding a `Colors` enum**

#### Step 1: Add to Specification (Section 1.2.3)

```xml
<enum>
  <n>Colors</n>
  <description>Color options for customization</description>
  <members>
    <member>
      <n>Red</n>
      <value>"red"</value>
      <description>Red color</description>
    </member>
    <member>
      <n>Green</n>
      <value>"green"</value>
      <description>Green color</description>
    </member>
    <member>
      <n>Blue</n>
      <value>"blue"</value>
      <description>Blue color</description>
    </member>
  </members>
  <usage_example>
set_color(Colors.Red)
if get_color() == Colors.Blue:
    print("Blue!")
  </usage_example>
</enum>
```

#### Step 2: Create C# Class (in GameEnums.cs)

```csharp
public static class Colors {
    public static readonly string Red = "red";
    public static readonly string Green = "green";
    public static readonly string Blue = "blue";
}
```

#### Step 3: Register in Interpreter

```csharp
// In PythonInterpreter.RegisterEnums()
var colorsEnum = new EnumObject();
colorsEnum.AddMember("Red", "red");
colorsEnum.AddMember("Green", "green");
colorsEnum.AddMember("Blue", "blue");

globalScope.Set("Colors", colorsEnum);
```

#### Step 4: Add Test Case (Section 5)

```xml
<test_case id="ENUM-COLORS">
  <description>Colors enum usage</description>
  <code>
set_color(Colors.Red)
if get_color() == Colors.Red:
    print("Color is red!")
  </code>
  <expected_behavior>
    Colors.Red evaluates to "red"
    Comparison works correctly
  </expected_behavior>
</test_case>
```

#### Step 5: (Optional) Add Functions That Use It

See [How to Add Game Functions That Use Enums](#how-to-add-game-functions-that-use-enums)

---

## 🔧 How to Add New Constants

### Step-by-Step Guide

**Example: Adding clock constants (Morning, Noon, Evening, Night)**

#### Step 1: Add to Specification (Section 4.3)

```xml
<constant_group name="Time of Day">
  <description>Constants representing times of day</description>
  
  <constant>
    <n>Morning</n>
    <value>0</value>
    <description>Morning time (sunrise)</description>
  </constant>
  
  <constant>
    <n>Noon</n>
    <value>6</value>
    <description>Midday</description>
  </constant>
  
  <constant>
    <n>Evening</n>
    <value>12</value>
    <description>Evening time (sunset)</description>
  </constant>
  
  <constant>
    <n>Night</n>
    <value>18</value>
    <description>Night time</description>
  </constant>
</constant_group>

<example>
if get_time() >= Evening:
    print("Time to rest")
    
wait_until(Morning)
</example>
```

#### Step 2: Register in Interpreter

```csharp
// In PythonInterpreter initialization
globalScope.Set("Morning", 0);
globalScope.Set("Noon", 6);
globalScope.Set("Evening", 12);
globalScope.Set("Night", 18);

// Optional: Make read-only
readOnlyVariables.Add("Morning");
readOnlyVariables.Add("Noon");
readOnlyVariables.Add("Evening");
readOnlyVariables.Add("Night");
```

#### Step 3: Add Test Case

```xml
<test_case id="CONST-TIME">
  <description>Time constants usage</description>
  <code>
if get_time() >= Evening:
    print("Evening or later")

# Test that constants have correct values
print(Morning)  # 0
print(Noon)     # 6
print(Evening)  # 12
print(Night)    # 18
  </code>
  <expected_output>0, 6, 12, 18</expected_output>
</test_case>
```

---

## 🎮 How to Add Game Functions That Use Enums

### Pattern: Query Function Returning Enum

**Example: `get_weather()` returning Weather enum**

#### Step 1: Add Enum (if doesn't exist)

```xml
<enum>
  <n>Weather</n>
  <members>
    <member><n>Sunny</n><value>"sunny"</value></member>
    <member><n>Rainy</n><value>"rainy"</value></member>
    <member><n>Cloudy</n><value>"cloudy"</value></member>
  </members>
</enum>
```

#### Step 2: Add Function (Section 4.1)

```xml
<function>
  <n>get_weather</n>
  <parameters>none</parameters>
  <return_type>Weather enum (string internally)</return_type>
  <execution_time>Instant</execution_time>
  <yields>false</yields>
  <description>
    Returns the current weather condition.
    Returns Weather.Sunny, Weather.Rainy, or Weather.Cloudy.
  </description>
  <implementation_type>Regular method</implementation_type>
  <possible_returns>
    Weather.Sunny, Weather.Rainy, Weather.Cloudy
  </possible_returns>
  <example>
weather = get_weather()
if weather == Weather.Rainy:
    print("Better stay inside!")
elif weather == Weather.Sunny:
    print("Great day for farming!")
  </example>
</function>
```

#### Step 3: Implement in C# (GameBuiltinMethods.cs)

```csharp
public string GetWeather() {
    // Unity logic to determine weather
    int weatherCode = weatherSystem.GetCurrentWeather();
    
    // Return enum string value
    switch (weatherCode) {
        case 0: return "sunny";    // Weather.Sunny
        case 1: return "rainy";    // Weather.Rainy
        case 2: return "cloudy";   // Weather.Cloudy
        default: return "sunny";
    }
}
```

#### Step 4: Register Function

```csharp
void RegisterGameFunctions() {
    // ...
    RegisterBuiltin("get_weather", args => GetWeather());
    // ...
}
```

#### Step 5: Add Test

```xml
<test_case id="FUNC-WEATHER">
  <description>Weather query function</description>
  <code>
weather = get_weather()
if weather == Weather.Sunny:
    print("Sunny day")
  </code>
  <expected_behavior>
    get_weather() returns "sunny"
    Weather.Sunny evaluates to "sunny"
    Comparison succeeds
  </expected_behavior>
</test_case>
```

---

### Pattern: Action Function Taking Enum Parameter

**Example: `plant(entity)` taking Entities enum**

#### Step 1: Add Function (Section 4.1)

```xml
<function>
  <n>plant</n>
  <parameters>entity: Entities enum</parameters>
  <return_type>void</return_type>
  <execution_time>~0.3 seconds</execution_time>
  <yields>true</yields>
  <description>
    Plants specified entity at current position.
    Entity must be Entities enum member (e.g., Entities.Carrot).
  </description>
  <implementation_type>IEnumerator</implementation_type>
  <example>
plant(Entities.Carrot)
plant(Entities.Sunflower)
  </example>
  <error_handling>
    Throws RuntimeError if:
    - Parameter is not a valid entity string
    - Current position already occupied
  </error_handling>
</function>
```

#### Step 2: Implement in C# (GameBuiltinMethods.cs)

```csharp
public IEnumerator PlantCrop(object entityObj) {
    // Convert to string (enum members evaluate to strings)
    string entityType = entityObj as string;
    
    if (entityType == null) {
        throw new RuntimeError("plant() requires entity type (e.g., Entities.Carrot)");
    }
    
    // Validate entity type
    string[] validEntities = { "grass", "bush", "tree", "carrot", "pumpkin", "sunflower" };
    if (!validEntities.Contains(entityType)) {
        throw new RuntimeError($"Unknown entity type: {entityType}");
    }
    
    // Plant in Unity
    GameObject entityPrefab = GetEntityPrefab(entityType);
    GameObject planted = Instantiate(entityPrefab, playerPosition, Quaternion.identity);
    
    // Play animation
    yield return new WaitForSeconds(0.3f);
}
```

#### Step 3: Register Function

```csharp
void RegisterGameFunctions() {
    // ...
    RegisterBuiltin("plant", args => {
        if (args.Count != 1) {
            throw new RuntimeError("plant() takes 1 argument");
        }
        return PlantCrop(args[0]);  // Returns IEnumerator
    });
    // ...
}
```

#### Step 4: Add Test

```xml
<test_case id="FUNC-PLANT">
  <description>Plant function with enum</description>
  <code>
plant(Entities.Carrot)
if can_harvest():
    harvest()
  </code>
  <expected_behavior>
    plant() receives "carrot" string from Entities.Carrot
    Planting animation plays (~0.3 seconds)
    Entity is created at player position
  </expected_behavior>
</test_case>
```

---

## 🔄 Complete Workflow Examples

### Example 1: Adding Complete "Trading" Feature

**Goal:** Add trading system with `Traders` enum and `trade()` function.

#### Step 1: Design the Feature

```
Enums needed:
- Traders (Villager, Merchant, Blacksmith)

Constants needed:
- (none for this feature)

Functions needed:
- get_trader() - returns current trader or None
- trade(trader, give_item, receive_item, quantity)
```

#### Step 2: Add to Specification

**In Section 1.2.3 (Enums):**
```xml
<enum>
  <n>Traders</n>
  <members>
    <member><n>Villager</n><value>"villager"</value></member>
    <member><n>Merchant</n><value>"merchant"</value></member>
    <member><n>Blacksmith</n><value>"blacksmith"</value></member>
  </members>
</enum>
```

**In Section 4.1 (Functions):**
```xml
<function>
  <n>get_trader</n>
  <parameters>none</parameters>
  <return_type>Traders enum or None</return_type>
  <yields>false</yields>
  <example>
trader = get_trader()
if trader == Traders.Merchant:
    trade(trader, Items.Carrot, Items.Gold, 10)
  </example>
</function>

<function>
  <n>trade</n>
  <parameters>
    trader: Traders enum,
    give_item: Items enum,
    receive_item: Items enum,
    quantity: int
  </parameters>
  <return_type>bool (success)</return_type>
  <yields>true</yields>
  <execution_time>~0.5 seconds</execution_time>
</function>
```

#### Step 3: Implement in C#

**GameEnums.cs:**
```csharp
public static class Traders {
    public static readonly string Villager = "villager";
    public static readonly string Merchant = "merchant";
    public static readonly string Blacksmith = "blacksmith";
}
```

**GameBuiltinMethods.cs:**
```csharp
public string GetTrader() {
    // Check if trader is nearby
    Collider2D trader = Physics2D.OverlapCircle(
        playerPosition, 
        interactionRadius, 
        traderLayer
    );
    
    if (trader == null) return null;
    
    return trader.GetComponent<Trader>().TraderType; // Returns "villager", etc.
}

public IEnumerator Trade(object traderObj, object giveItemObj, 
                        object receiveItemObj, object quantityObj) {
    string trader = traderObj as string;
    string giveItem = giveItemObj as string;
    string receiveItem = receiveItemObj as string;
    int quantity = (int)ToNumber(quantityObj);
    
    // Validation
    if (trader == null || giveItem == null || receiveItem == null) {
        throw new RuntimeError("trade() requires valid enum parameters");
    }
    
    // Check inventory
    if (inventory.GetCount(giveItem) < quantity) {
        yield return false;
        yield break;
    }
    
    // Perform trade
    inventory.Remove(giveItem, quantity);
    inventory.Add(receiveItem, quantity);
    
    // Animation
    yield return new WaitForSeconds(0.5f);
    
    yield return true;
}
```

**Registration:**
```csharp
void RegisterGameFunctions() {
    // ...
    
    // Register Traders enum
    var tradersEnum = new EnumObject();
    tradersEnum.AddMember("Villager", "villager");
    tradersEnum.AddMember("Merchant", "merchant");
    tradersEnum.AddMember("Blacksmith", "blacksmith");
    globalScope.Set("Traders", tradersEnum);
    
    // Register functions
    RegisterBuiltin("get_trader", args => GetTrader());
    RegisterBuiltin("trade", args => {
        if (args.Count != 4) {
            throw new RuntimeError("trade() takes 4 arguments");
        }
        return Trade(args[0], args[1], args[2], args[3]);
    });
}
```

#### Step 4: Add Tests (Section 5)

```xml
<test_case id="TRADE-1">
  <description>Trading system</description>
  <code>
trader = get_trader()
if trader == Traders.Merchant:
    success = trade(trader, Items.Carrot, Items.Gold, 10)
    if success:
        print("Trade successful!")
  </code>
  <expected_behavior>
    Enums evaluate correctly
    trade() receives proper string parameters
    Returns boolean result
  </expected_behavior>
</test_case>
```

---

### Example 2: Adding "Weather System" with Constants

**Goal:** Add weather system with seasonal constants.

#### Step 1: Design

```
Enums:
- Weather (Sunny, Rainy, Cloudy, Stormy)

Constants:
- Spring, Summer, Fall, Winter (seasonal constants)

Functions:
- get_weather() - returns Weather enum
- get_season() - returns season constant
- wait_for_weather(weather) - waits until weather changes
```

#### Step 2-4: Follow the same pattern as Example 1

**Key points:**
- Constants are just `globalScope.Set("Spring", 0)` etc.
- Enum follows standard enum pattern
- Functions use both enum returns and constant comparisons

#### Test Example:
```python
season = get_season()
if season == Spring:
    if get_weather() == Weather.Rainy:
        print("April showers!")
        
wait_for_weather(Weather.Sunny)
plant(Entities.Sunflower)
```

---

## 🧩 Understanding the Cascading Updates

### When You Add an Enum

**What needs to be updated:**

```
1. Section 1.2.3 (Enum Definition)
   ↓
2. GameEnums.cs (C# class)
   ↓
3. PythonInterpreter.cs (Registration)
   ↓
4. (Optional) Functions that use it
   ↓
5. Section 5 (Test cases)
```

### When You Add a Constant

**What needs to be updated:**

```
1. Section 4.3 (Constant Definition)
   ↓
2. PythonInterpreter.cs (Registration)
   ↓
3. (Optional) readOnlyVariables HashSet
   ↓
4. Section 5 (Test cases)
```

### When You Add a Function That Uses Enums

**What needs to be updated:**

```
1. Section 4.1 (Function Definition)
   ↓
2. GameBuiltinMethods.cs (Implementation)
   ↓
3. GameBuiltinMethods.cs (Registration)
   ↓
4. Section 5 (Test cases)
   
Note: The enum itself should already exist!
```

---

## 💡 Design Patterns & Best Practices

### Pattern 1: Enum Validation in Functions

**Always validate enum parameters:**

```csharp
public IEnumerator PlantCrop(object entityObj) {
    string entity = entityObj as string;
    
    // Validation
    if (entity == null) {
        throw new RuntimeError("plant() requires Entities enum parameter");
    }
    
    // Additional validation: check if valid entity
    if (!IsValidEntity(entity)) {
        throw new RuntimeError($"Unknown entity: {entity}");
    }
    
    // Implementation...
}
```

### Pattern 2: Enum-Returning Functions

**Return the string value directly:**

```csharp
public string GetGroundType() {
    // Unity logic
    TerrainType terrain = GetTerrainAt(playerPosition);
    
    // Map to enum string values
    switch (terrain) {
        case TerrainType.Soil: return "soil";        // Grounds.Soil
        case TerrainType.Grass: return "grassland";  // Grounds.Grassland
        case TerrainType.Turf: return "turf";        // Grounds.Turf
        default: return "soil";
    }
}
```

### Pattern 3: Optional Parameters with Enums

**Handle both enum and string:**

```csharp
public IEnumerator Move(object directionObj) {
    string direction = directionObj as string;
    
    // direction could be:
    // - "up" (from North constant)
    // - "down" (from South constant)
    // - "left" (from move("left") string literal)
    // - "right" (from move("right") string literal)
    
    // All handled the same way!
    yield return MoveInDirection(direction);
}
```

### Pattern 4: Read-Only Constants (Optional but Recommended)

**Enforce immutability:**

```csharp
void ExecuteAssignment(AssignmentStmt stmt) {
    // Check if trying to modify constant
    if (readOnlyVariables.Contains(stmt.Target)) {
        throw new RuntimeError(
            $"Cannot assign to constant '{stmt.Target}'. " +
            "Built-in constants (North, South, East, West) are read-only."
        );
    }
    
    // Normal assignment
    currentScope.Set(stmt.Target, Evaluate(stmt.Value));
}
```

---

## 🎯 Quick Cheat Sheet

### Adding an Enum

1. **Define** in Section 1.2.3
2. **Create class** in GameEnums.cs
3. **Register** in PythonInterpreter
4. **Test** in Section 5

### Adding a Constant

1. **Define** in Section 4.3
2. **Register** in PythonInterpreter: `globalScope.Set("Name", value)`
3. **(Optional)** Add to `readOnlyVariables`
4. **Test** in Section 5

### Adding a Function with Enum Parameter

1. **Define** in Section 4.1 (specify enum parameter)
2. **Implement** in GameBuiltinMethods.cs
3. **Cast parameter:** `string value = paramObj as string`
4. **Validate** the string value
5. **Register** in RegisterGameFunctions()
6. **Test** in Section 5

### Adding a Function Returning Enum

1. **Define** in Section 4.1 (specify enum return type)
2. **Implement** in GameBuiltinMethods.cs
3. **Return string value** directly (e.g., return "soil")
4. **Register** in RegisterGameFunctions()
5. **Test** in Section 5

---

## 🚀 You're Ready!

You now understand:

✅ How enums work in LOOP (member access → string values)
✅ How constants are pre-registered globals
✅ How to add new enums (4-step process)
✅ How to add new constants (3-step process)
✅ How to add functions that use enums
✅ Complete workflow examples
✅ Design patterns and best practices

**The v2.0 specification is production-ready for generating a complete Farmer Was Replaced clone!** 🎉

---

**End of Enhanced Anatomy Guide**