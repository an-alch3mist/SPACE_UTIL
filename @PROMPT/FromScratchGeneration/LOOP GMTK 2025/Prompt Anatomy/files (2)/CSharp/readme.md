# LOOP Language Interpreter for Unity

A complete Python-like interpreter implementation for Unity, designed for the "Farmer Was Replaced" game clone. Supports advanced features like lambda expressions, list comprehensions, tuples, enums, and coroutine-based execution.

## 📁 File Structure

All files are in the `LOOPLanguage` namespace:

### Core Interpreter Files
- **Token.cs** - Token types and Token class
- **Lexer.cs** - Tokenizer with indentation handling
- **AST.cs** - Abstract Syntax Tree node definitions
- **Parser.cs** - Recursive descent parser
- **PythonInterpreter.cs** - Main execution engine (Parts 1-3 combined)
- **Exceptions.cs** - Custom exception classes

### Runtime Support
- **Scope.cs** - Variable scope management
- **BuiltinFunction.cs** - Built-in function wrapper
- **LambdaFunction.cs** - Runtime lambda representation
- **ClassInstance.cs** - User-defined class support

### Game Integration
- **GameBuiltinMethods.cs** - Game-specific functions
- **GameEnums.cs** - Enum definitions (Grounds, Items, Entities)
- **CoroutineRunner.cs** - Safe coroutine execution
- **ConsoleManager.cs** - In-game console for output

### Demo & Testing
- **DemoScripts.cs** - Collection of test scripts
- **LOOPController.cs** - Main controller MonoBehaviour

## 🚀 Setup Instructions

### 1. Create Unity Project
- Unity 2020.3 or later (LTS recommended)
- Create a new 3D or 2D project

### 2. Add All Scripts
1. Create a folder: `Assets/Scripts/LOOPLanguage/`
2. Add all `.cs` files to this folder
3. **Important**: Combine the three PythonInterpreter parts into ONE file:
   - Open a new file: `PythonInterpreter.cs`
   - Copy the content from Part 1 (class definition, fields, initialization)
   - Then add Part 2 content (Expression Evaluation region)
   - Then add Part 3 content (Built-in Functions & Helpers regions)
   - Close the class and namespace properly

### 3. Setup the Scene

#### Create UI Canvas
1. Create: `GameObject > UI > Canvas`
2. Set Canvas Scaler to "Scale With Screen Size"

#### Add Code Input
1. Create: `UI > InputField` (child of Canvas)
2. Rename to "CodeInput"
3. Set to multiline
4. Adjust size (e.g., 800x400)
5. Add placeholder text: "Enter your code here..."

#### Add Console Output
1. Create: `UI > Scroll View` (child of Canvas)
2. Inside Content, add a `Text` component
3. Rename to "ConsoleText"
4. Set text color, font size
5. Set Text alignment to Top-Left

#### Add Buttons
1. Create: `UI > Button` for "Run"
2. Create: `UI > Button` for "Stop"
3. Create: `UI > Button` for "Clear Console"

#### Add Error Display
1. Create: `UI > Text`
2. Rename to "ErrorText"
3. Set color to red
4. Set to inactive by default

#### Add Demo Script Dropdown (Optional)
1. Create: `UI > Dropdown`
2. Will be populated by LOOPController

### 4. Setup LOOPController

1. Create empty GameObject: "LOOPController"
2. Add `LOOPController` component
3. Assign references in inspector:
   - Code Input → CodeInput InputField
   - Run Button → Run Button
   - Stop Button → Stop Button
   - Clear Button → Clear Button
   - Error Text → ErrorText
   - Demo Scripts Dropdown → Dropdown (optional)

### 5. Setup ConsoleManager

1. Create empty GameObject: "ConsoleManager"
2. Add `ConsoleManager` component
3. Assign references:
   - Console Text → ConsoleText
   - Scroll Rect → Scroll View

## ✨ Features

### Language Support
- ✅ Variables and assignments
- ✅ If/elif/else statements
- ✅ While and for loops
- ✅ Functions (def) with parameters
- ✅ Classes with methods
- ✅ Lambda expressions (including IIFE)
- ✅ List comprehensions
- ✅ Tuples and indexing
- ✅ Dictionaries
- ✅ Enums (Grounds, Items, Entities)
- ✅ Built-in constants (North, South, East, West)

### Operators
- Arithmetic: `+`, `-`, `*`, `/`, `%`, `**`
- Comparison: `==`, `!=`, `<`, `>`, `<=`, `>=`
- Logical: `and`, `or`, `not`, `in`, `is`
- Bitwise: `&`, `|`, `^`, `~`, `<<`, `>>`

### Built-in Functions
- `print()`, `sleep()`, `range()`, `len()`
- `str()`, `int()`, `float()`, `abs()`
- `min()`, `max()`, `sum()`, `sorted()`

### Game Functions
- Movement: `move(direction)`
- Farming: `harvest()`, `plant(entity)`, `till()`
- Queries: `can_harvest()`, `get_ground_type()`, `get_entity_type()`
- Position: `get_pos_x()`, `get_pos_y()`, `get_world_size()`
- Inventory: `num_items(item)`, `use_item(item)`

## 🧪 Testing

### Run Demo Scripts
1. Click the Demo Scripts dropdown
2. Select a test case
3. Click "Run"
4. Watch the console output

### Example Test Cases

**Lambda with List Comprehension:**
```python
nums = [1, 2, 3, 4, 5, 6, 7, 8]
result = (lambda x: [i*i for i in x if i % 2 == 0 and i > 3])(nums)
print(result)  # [16, 36, 64]
```

**Sorted with Lambda:**
```python
data = [(1, 'b'), (3, 'a'), (2, 'c')]
sorted_data = sorted(data, key=lambda x: x[1])
print(sorted_data)  # [(3, 'a'), (1, 'b'), (2, 'c')]
```

**Game Integration:**
```python
for i in range(5):
    move(North)
    if can_harvest():
        harvest()

plant(Entities.Carrot)
```

## ⚙️ Configuration

### Instruction Budget
In `PythonInterpreter.cs`:
```csharp
private const int INSTRUCTIONS_PER_FRAME = 100;
```
- Lower = smoother framerate, slower execution
- Higher = faster execution, possible frame drops
- Recommended range: 50-500

### Console Settings
In `ConsoleManager.cs`:
```csharp
private const int MAX_LINES = 100;
```
- Maximum number of console lines to keep

## 🔧 .NET 2.0 Compliance

This implementation respects Unity's .NET 2.0 limitations:
- ✅ No `yield return` inside `try-catch` in IEnumerators
- ✅ Yields stored in variables before returning
- ✅ Exception handling done before yielding

## 📋 Test Suite Status

All test cases from the specification should pass:
- ✅ Advanced lambda expressions
- ✅ Tuple support
- ✅ Enum member access
- ✅ Built-in constants
- ✅ Operator precedence (** is right-associative)
- ✅ List operations (negative indexing, slicing)
- ✅ Instruction budget system
- ✅ Error handling with line numbers

## 🐛 Common Issues

### Issue: "Indentation mismatch"
**Solution**: Ensure all indentation uses 4 spaces (tabs are auto-converted)

### Issue: "Lambda expects N arguments, got M"
**Solution**: Check lambda parameter count matches call arguments

### Issue: Script runs slowly
**Solution**: Increase `INSTRUCTIONS_PER_FRAME` constant

### Issue: Console not showing output
**Solution**: Verify ConsoleManager references are assigned in inspector

## 📝 Notes

- Enums are represented as C# strings internally
- Query functions (like `can_harvest()`) have negligible budget cost
- Sleep always yields, regardless of instruction budget
- Numbers with decimals (1.0) are floats, without (1) are integers
- All scripts must end with a newline (auto-added by lexer)

## 🎮 Game Development Tips

1. **Use Enums**: Access ground types with `Grounds.Soil` instead of strings
2. **Lambda in sorted()**: Sort complex data with `sorted(data, key=lambda x: x[1])`
3. **List Comprehensions**: Filter and transform efficiently
4. **Budget Management**: Keep loops under 100 iterations per frame for smoothness

## 📚 Additional Resources

- See `DemoScripts.cs` for complete working examples
- All test cases from specification are included
- Check Unity console for detailed error messages with line numbers

---

**Version**: 2.1  
**Unity**: 2020.3+  
**Namespace**: LOOPLanguage  
**Status**: Production Ready ✅
