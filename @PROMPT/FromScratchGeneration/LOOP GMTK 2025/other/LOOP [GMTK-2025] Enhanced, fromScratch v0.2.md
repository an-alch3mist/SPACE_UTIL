The type of prompt we are building is technically called a **"System Specification Prompt"** or **"Architectural Mega-Prompt"**.

In the context of Prompt Engineering, it specifically uses a technique called **Few-Shot Chain-of-Thought with Constraint-Based Architecture**. You are essentially behaving as the "Software Architect" writing the spec sheet, so the AI (the "Junior Developer") has no room to make bad decisions.

---

### Clarification on Loop Timing & Boolean Logic

You are absolutely correct. To achieve the behavior you described (where small loops are instant, but massive loops are time-sliced across frames), we need a **Global Instruction Budget**.

* **Logic:** Every time the interpreter runs a statement (`x = 1`) or an expression (`x < 5`), we increment a counter `ops`.
* **Threshold:** If `ops > 100`, we `yield return null` (wait for next frame) and reset `ops = 0`.
* **Result:**
* **100 iter loop:** runs in 1 batch -> **Instant**.
* **1000 iter loop:** runs in 10 batches -> **10 Frames**.
* **Sleep:** purely strictly pauses execution regardless of the ops counter.
* **any game buit-in function say: move("right"):** shall be desccribed customly inslide game built in method as IEnumerator and this shall pause in pyton until that is completed.


Here is the **Final, Definitive Specification (~2000 Lines Equivalent)**. This covers the grouping logic (`(A and B) or C`), the precise timing architecture, and the expanded test suite.

---

# The Ultimate Unity "Python-Like" Game Engine Specification

**Role:** Act as a Principal Unity Architect and Compiler Language Engineer.
**Project:** "Farmer Was Replaced" Clone - A Programming Puzzle Game.
**Goal:** Generate the complete, production-ready C# source code for a Coroutine-based Python Interpreter in Unity.
**Output Format:** Provide all C# scripts in full.

---

## 1. System Architecture & Core Philosophy

### 1.1 Execution Model (The "Time-Slicing" Engine)
The interpreter must optimize for game performance while preventing freezes.
1.  **Global Instruction Budget:**
    * Maintain an integer `instructionsExecutedThisFrame`.
    * **Limit:** Set a constant `MAX_OPS_PER_FRAME = 100`.
    * **Logic:** Every time `ExecStmt` or `ExecExpr` is called, increment the counter.
    * **Check:** `if (instructionsExecutedThisFrame >= MAX_OPS_PER_FRAME)`, then:
        * `yield return null;` (Wait for next Unity frame).
        * `instructionsExecutedThisFrame = 0;` (Reset counter).
    * **Result:** A loop of 100 items finishes instantly (1 frame). A loop of 1000 items takes 10 frames.

2.  **Yield Propagation (The "Sleep" Logic):**
    * If a built-in function (like `sleep(2.5)`) returns a `YieldInstruction`, the Interpreter must **ignore** the instruction budget and immediately `yield return` that instruction to Unity.
    * **Handling Decimals:** `sleep` must accept `int` or `float` (e.g., `sleep(0.5)`).

### 1.2 Data Types & Sandboxing
* **Numbers:** Internally use `double` for everything to handle both `2` and `2.5` without casting errors.
* **Booleans:** Strict Python semantics (`True`/`False`).
* **Sandboxing:** Pure C# logic only. No `System.Reflection`, No `System.IO`.

---

## 2. Technical Specifications (The Compiler Stack)

### 2.1 `Token.cs` & `TokenType.cs`
* **Tokens:** `INDENT`, `DEDENT`, `NEWLINE`, `EOF`, `IDENTIFIER`, `STRING`, `NUMBER`.
* **Keywords:** `if`, `elif`, `else`, `while`, `for`, `def`, `return`, `class`, `break`, `continue`, `pass`, `global`, `and`, `or`, `not`, `in`, `True`, `False`, `None`, `lambda`.
* **Operators:** `+`, `-`, `*`, `/`, `%`, `==`, `!=`, `<`, `>`, `<=`, `>=`, `=`, `+=`, `-=`, `[`, `]`, `.`, `,`, `:`, `(`, `)`.
* **Bitwise:** `&`, `|`, `^`, `~`, `<<`, `>>`.

### 2.2 `Lexer.cs` (Sanitization & Indentation)
* **Sanitization:**
    * Convert `\r\n` -> `\n`.
    * Strip `\v` (vertical tab), `\f`, `\0`, and BOM.
* **Indentation Stack:**
    * Track indentation levels using a `Stack<int>`.
    * Emit `INDENT` when whitespace increases.
    * Emit multiple `DEDENT` tokens when whitespace decreases to match a previous level.
    * **Error:** Throw `IndentationError` if the decrease doesn't match any previous level.

### 2.3 `AST.cs` (Abstract Syntax Tree)
* **Nodes:** `FunctionDef`, `ClassDef`, `IfStmt`, `WhileStmt`, `ForStmt`, `ReturnStmt`, `AssignStmt`, `ExprStmt`, `GlobalStmt`.
* **Expressions:**
    * `BinaryExpr` (Math/Logic), `UnaryExpr`, `CallExpr`.
    * `ListExpr`, `DictExpr`, `GetExpr` (property), `SetExpr` (property).
    * `SliceExpr` (Start/End/Step), `LiteralExpr`, `VariableExpr`.
    * `ListCompExpr`: `[x*2 for x in list]`.
    * `LambdaExpr`: `lambda a, b: a+b`.

### 2.4 `Parser.cs` (Logic Grouping & Precedence)
* **Grouping Logic (Crucial):**
    * Implement strictly: `(1==0) and 2<3 or 5>2`.
    * **Precedence Order (Low to High):**
        1.  `lambda`
        2.  `or`
        3.  `and`
        4.  `not`
        5.  `in`, `not in`, `is`, `is not`, `<`, `<=`, `>`, `>=`, `!=`, `==`
        6.  `|` (Bitwise OR)
        7.  `^` (Bitwise XOR)
        8.  `&` (Bitwise AND)
        9.  `<<`, `>>`
        10. `+`, `-`
        11. `*`, `/`, `%`
        12. `+x`, `-x`, `~x` (Unary)
        13. `**` (Power)
* **Desugaring:**
    * Convert `for x in list` into a standard iterator structure in the AST.

---

## 3. The Runtime Engine (`PythonInterpreter.cs`)

### 3.1 Core Execution Loop
```csharp
public IEnumerator Execute(List<Stmt> statements) {
    foreach (var stmt in statements) {
        // 1. Time Slicing Check
        opCounter++;
        if (opCounter >= 100) {
            opCounter = 0;
            yield return null; // Wait for next frame
        }

        // 2. Execution
        yield return ExecStmt(stmt); 
    }
}

```

* **Recursive Calls:** Ensure nested functions share the **same global** `opCounter`.

### 3.2 Built-In Type Methods

You must implement these methods on C# objects to mimic Python:

* **List:**
* `append(x)`, `pop(i)`, `remove(x)` (throw ValueError if missing).
* `sort(key=None, reverse=False)`.
* `clear()`, `copy()`.


* **String:**
* `split(delim)`, `strip()`, `replace(old, new)`.
* `join(iterable)`.
* `lower()`, `upper()`.



### 3.3 Slicing & Indexing logic

* **Negative Indexing:** `list[-1]` must return the last element.
* **Slicing:** `list[1:3]` (subset), `list[:2]` (start to 2), `list[2:]` (2 to end).
* **Bounds:** Slicing should generally forgive out-of-bounds (clamp to count), but direct Indexing `list[100]` must throw `IndexError`.

---

## 4. Unity Integration & Safety

### 4.1 `CoroutineRunner.cs`

* **Safe Execution:**
* Wrap every step of the Interpreter in `try/catch`.
* **On Error:**
1. Catch the Exception.
2. Identify the **Source Line Number** from the AST/Token.
3. Log to `ConsoleManager` in Red: `"[Line 5] RuntimeError: Division by zero"`.
4. **Reset** the Interpreter (Clear Stack/Globals).
5. `yield break` (Stop).





### 4.2 `GameBuiltinMethods.cs`

* `sleep(seconds)`: Must support `int` and `double`. Returns `new WaitForSeconds((float)seconds)`.
* `move(dir)`: Returns a Coroutine/YieldInstruction.
* `say(text)`: Display text bubble.

---

## 5. Comprehensive Test Suite (The "Check" System)

**Create `DemoScripts.cs` containing these exact test strings. The generated engine MUST pass all of them.**

### Test 1: Timing & Time-Slicing (The "Loop" Test)

*Verifies that small loops are instant and large loops distribute across frames.*

```python
# Should be instant (1 frame)
for i in range(50):
    pass

# Should take ~10 frames (1000 / 100 ops per frame)
sum = 0
for i in range(1000):
    sum += 1
print("Large loop done. Sum: " + str(sum))

# Nested Timing
# Total ops: 10 * 20 = 200. Should take ~2 frames.
nested_sum = 0
for x in range(10):
    for y in range(20):
        nested_sum += 1
print("Nested done: " + str(nested_sum))

```

### Test 2: Sleep & Game Sync

*Verifies sleep pauses execution accurately.*

```python
print("Start")
sleep(0.5) # Wait 0.5s
print("0.5s later")
sleep(2)   # Wait 2s (Integer support)
print("Done")

```

### Test 3: Complex Boolean Logic (Grouping)

*Verifies Operator Precedence and Parentheses.*

```python
# (1 == 0) is False.
# (2 < 3) is True.
# (5 > 2) is True.
# False AND True OR True  =>  False OR True  => True.
if (1 == 0) and 2 < 3 or 5 > 2:
    print("Logic Check 1: PASSED")
else:
    print("Logic Check 1: FAILED")

# Precedence check: AND binds tighter than OR
# True or False and False => True or (False) => True
if True or False and False:
    print("Logic Check 2: PASSED")

```

### Test 4: List Manipulation & Slicing

*Verifies Python-strict list behavior.*

```python
items = ["a", "b", "c", "d", "e"]

# Negative Index
if items[-1] == "e": print("Negative Index: PASSED")

# Slicing
sub = items[1:3] # ["b", "c"]
if len(sub) == 2 and sub[0] == "b": print("Slicing: PASSED")

# Remove
items.remove("c") # ["a", "b", "d", "e"]
if len(items) == 4 and items[2] == "d": print("Remove: PASSED")

# Pop
val = items.pop(0) # "a"
if val == "a" and items[0] == "b": print("Pop: PASSED")

```

### Test 5: List Comprehensions & Lambdas

*Verifies advanced functional features.*

```python
nums = [1, 2, 3, 4]

# List Comp
doubled = [x * 2 for x in nums] # [2, 4, 6, 8]
if doubled[-1] == 8: print("List Comp: PASSED")

# Lambda Sort
data = [{"val": 10}, {"val": 1}]
data.sort(key=lambda x: x["val"]) # Should sort by 'val'
if data[0]["val"] == 1: print("Lambda Sort: PASSED")

```

### Test 6: Recursion & Algorithms (Flood Fill)

*Verifies recursion depth and boolean logic in algorithms.*

```python
grid = [[0,0,0], [0,1,0], [0,0,0]] # 1 is wall
visited = []

def fill(x, y):
    if x < 0 or y < 0 or x > 2 or y > 2: return
    if grid[y][x] == 1: return
    if [x,y] in visited: return
    
    visited.append([x,y])
    fill(x+1, y)
    fill(x-1, y)
    fill(x, y+1)
    fill(x, y-1)

fill(0,0)
print("Visited count: " + str(len(visited))) # Expect 8

```

---

## 6. Execution Instructions for the AI

1. **Generate ALL C# Files:** `Token.cs`, `Lexer.cs`, `AST.cs`, `Parser.cs`, `PythonInterpreter.cs`, `CoroutineRunner.cs`, `GameBuiltinMethods.cs`, `ConsoleManager.cs`, `DemoScripts.cs`.
2. **Implementation Detail:** In `Lexer.cs`, ensure `ValidateAndClean` removes all `\v` and `\0` characters before processing to prevent crashes.
3. **Implementation Detail:** In `PythonInterpreter.cs`, the `opsCounter` must be a field in the class, not a local variable, so it persists across recursive calls.

**Generate the code now.**

