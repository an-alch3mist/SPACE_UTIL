# The Ultimate Unity "Python-Like" Game Engine Specification v2.0

**Role:** Act as a Principal Unity Architect and Compiler Language Engineer.  
**Project:** "Farmer Was Replaced" Clone For Learning - A Programming Puzzle Game.  
**Goal:** Generate complete, production-ready C# source code for a Coroutine-based Python Interpreter in Unity.

---

## 1. System Architecture & Core Philosophy

### 1.1 Execution Model (The "Golden Rule")

#### 1.1.1 Coroutine-Based VM
* **Single-Threaded Execution:** The Interpreter (`PythonInterpreter.cs`) runs strictly within a Unity `IEnumerator`.
* **Thread-Safety:** No background threads - ensures compatibility with Unity's API (`Transform`, `GameObject`).
* **Yield Propagation:** When user scripts call "Game Commands" (like `move()`), those commands return `YieldInstruction` objects. The Interpreter must pause and yield these to Unity.

#### 1.1.2 Global Instruction Budget System (CRITICAL)

**Purpose:** Prevent frame drops while maintaining instant execution for small loops.

**Implementation:**
* **Counter:** Maintain a global `int instructionCount` that tracks operations executed in the current frame.
* **Budget:** Set `INSTRUCTIONS_PER_FRAME = 100` (configurable).
* **Increment Logic:** Increment `instructionCount` for:
  - Every statement execution (`x = 1`, `print(x)`)
  - Every expression evaluation (`x < 5`, `a + b`)
  - Every loop iteration check (`for` or `while` condition)
  - Every function call entry
* **Time-Slicing:** When `instructionCount >= INSTRUCTIONS_PER_FRAME`:
  - `yield return null` (pause until next frame) or yield return wait till next frame as you seem fit.
  - Reset `instructionCount = 0`
  - Continue execution

**Behavior Examples:**
```python
# Example 1: 100 iterations - INSTANT (1 frame)
sum = 0
for i in range(100):
    sum += 1  # ~100 ops -> stays under budget
print(sum)

# Example 2: 1000 iterations - TIME-SLICED (~10 frames)
sum = 0
for i in range(1000):
    sum += 1  # ~1000 ops -> yields 10 times

# Example 3: Nested loops - INTELLIGENT SLICING
sum = 0
for y in range(50):      # Outer: 50 iterations
    for x in range(1000): # Inner: 1000 iterations each
        sum += 1          # Total: 50,000 ops -> yields ~500 times

# Example 4: Sleep ALWAYS pauses (independent of budget)
for i in range(50):
    print("iteration:", i)
    if i % 10 == 0:
        sleep(2.0)  # MUST yield WaitForSeconds(2.0)

# Example 5: InBuilt function Say move() ALWAYS pauses (independent of budget)
for i in range(50):
    print("iteration:", i)
    if i % 10 == 0:
        move("right")  # MUST yield until the built-in function IEnumerator is completed , (this is customised inside unity IEnumerator, it may include animation event and many more all can be customised) when move("right") is called it pauses until that IEnumerator corutine complete.
```

**Critical Rules:**
1. **`sleep(seconds)` Override:** Sleep ALWAYS yields `WaitForSeconds`, regardless of instruction budget.
2. **Game Commands:** Functions like `move()`, `harvest()` ALWAYS yield their `YieldInstruction`, pausing Python execution until complete.
3. **Budget Independence:** Instruction budget only controls computational time-slicing, not external waits.

#### 1.1.3 Error Context & Recovery
* **Line Tracking:** Maintain `currentLineNumber` during execution.
* **Exception Wrapping:** When ANY exception occurs:
  - Catch the original exception
  - Create `RuntimeError` with message: `"Line {currentLineNumber}: {originalMessage}"`
  - Re-throw the wrapped error
* **State Reset:** After error, call `Interpreter.Reset()` to clear:
  - All scopes (global + local)
  - Instruction counter
  - Call stack

### 1.2 Data Types & Sandboxing

#### 1.2.1 Type System
* **Numbers:** 
  - Store all numeric values as `double` internally
  - Perform automatic conversions when needed (e.g., array indexing uses `(int)value`)
  - Support both integer and floating-point literals (`42`, `3.14`)
* **Strings:** Immutable C# strings with Python-like methods
* **Booleans:** Use C# `bool` for `True`, `False`
* **None:** Represent as C# `null`
* **Lists:** `List<object>` for dynamic heterogeneous collections
* **Dictionaries:** `Dictionary<object, object>` with proper equality handling
* **Classes:** Support custom user-defined classes with methods and `self`

#### 1.2.2 Security Constraints (.NET 2.0 Compatibility)
* **No Reflection:** Disable `System.Reflection` access to external assemblies
* **No File I/O:** Block `System.IO` operations
* **No Threading:** Reject `System.Threading` APIs
* **IEnumerator Limitation:** CANNOT use `yield return` inside `try-catch` blocks (Unity 2020.3 .NET 2.0 constraint)
  - Solution: Store yield instructions in variables outside try-catch, then yield after the block

---

## 2. Technical Specifications (The Compiler Stack)

### 2.1 Token System (`Token.cs`, `TokenType.cs`)

#### 2.1.1 Token Types (Enum)
```csharp
public enum TokenType {
    // Structural
    INDENT, DEDENT, NEWLINE, EOF,
    
    // Literals
    IDENTIFIER, STRING, NUMBER,
    
    // Keywords
    IF, ELIF, ELSE, WHILE, FOR, DEF, RETURN, CLASS,
    BREAK, CONTINUE, PASS, GLOBAL, LAMBDA,
    AND, OR, NOT, IN, IS,
    TRUE, FALSE, NONE,
    
    // Operators (Arithmetic)
    PLUS, MINUS, STAR, SLASH, PERCENT,
    
    // Operators (Comparison)
    EQUAL_EQUAL, BANG_EQUAL, LESS, GREATER, LESS_EQUAL, GREATER_EQUAL,
    
    // Operators (Assignment)
    EQUAL, PLUS_EQUAL, MINUS_EQUAL, STAR_EQUAL, SLASH_EQUAL,
    
    // Operators (Bitwise)
    AMPERSAND, PIPE, CARET, TILDE, LEFT_SHIFT, RIGHT_SHIFT,
    
    // Delimiters
    LEFT_PAREN, RIGHT_PAREN, LEFT_BRACKET, RIGHT_BRACKET,
    DOT, COMMA, COLON
}
```

#### 2.1.2 Token Class
```csharp
public class Token {
    public TokenType Type;
    public string Lexeme;
    public object Literal;  // For NUMBER, STRING
    public int LineNumber;
    
    public Token(TokenType type, string lexeme, object literal, int line) {
        Type = type;
        Lexeme = lexeme;
        Literal = literal;
        LineNumber = line;
    }
}
```

### 2.2 Lexer (`Lexer.cs`) - The Input Sanitizer

#### 2.2.1 Input Validation (CRITICAL)
```csharp
public string ValidateAndClean(string input) {
    // Step 1: Normalize line endings
    input = input.Replace("\r\n", "\n");
    input = input.Replace("\r", "\n");
    
    // Step 2: Convert tabs to 4 spaces (consistency)
    input = input.Replace("\t", "    ");
    
    // Step 3: Remove invisible characters (prevent crashes)
    input = input.Replace("\v", "");  // Vertical tab
    input = input.Replace("\f", "");  // Form feed
    input = input.Replace("\uFEFF", "");  // BOM (Byte Order Mark)
    
    // Step 4: Ensure ends with newline
    if (!input.EndsWith("\n")) {
        input += "\n";
    }
    
    return input;
}
```

#### 2.2.2 Indentation Logic (Python-Style)
```csharp
Stack<int> indentStack = new Stack<int>();
// Initialize with 0
indentStack.Push(0);

void ProcessIndentation(int leadingSpaces) {
    int current = leadingSpaces;
    int previous = indentStack.Peek();
    
    if (current > previous) {
        // INDENT
        indentStack.Push(current);
        EmitToken(TokenType.INDENT);
    } else if (current < previous) {
        // DEDENT (possibly multiple)
        while (indentStack.Count > 0 && indentStack.Peek() > current) {
            indentStack.Pop();
            EmitToken(TokenType.DEDENT);
        }
        
        // Validation: Must match a level
        if (indentStack.Peek() != current) {
            throw new LexerError($"Line {lineNumber}: Indentation mismatch");
        }
    }
    // If current == previous, no indent change
}
```

#### 2.2.3 Number Parsing (Support Integer & Float)
```csharp
void ScanNumber() {
    while (IsDigit(Peek())) Advance();
    
    // Check for decimal point
    if (Peek() == '.' && IsDigit(PeekNext())) {
        Advance(); // Consume '.'
        while (IsDigit(Peek())) Advance();
    }
    
    string numStr = source.Substring(start, current - start);
    double value = double.Parse(numStr);
    EmitToken(TokenType.NUMBER, value);
}
```

### 2.3 Abstract Syntax Tree (`AST.cs`)

#### 2.3.1 Node Hierarchy
```csharp
public abstract class AstNode {
    public int LineNumber;  // For error reporting
}

// Statements (do not produce values)
public abstract class Stmt : AstNode { }

// Expressions (produce values)
public abstract class Expr : AstNode { }
```

#### 2.3.2 Statement Types
```csharp
// Variable assignment: x = 10
public class AssignStmt : Stmt {
    public Expr Target;  // LHS (can be variable, index, property)
    public Expr Value;   // RHS
}

// Expression statement: print(x)
public class ExprStmt : Stmt {
    public Expr Expression;
}

// If-elif-else chain
public class IfStmt : Stmt {
    public Expr Condition;
    public List<Stmt> ThenBranch;
    public List<Stmt> ElifBranches;  // List of (Expr condition, List<Stmt> body)
    public List<Stmt> ElseBranch;
}

// While loop
public class WhileStmt : Stmt {
    public Expr Condition;
    public List<Stmt> Body;
}

// For loop: for x in iterable:
public class ForStmt : Stmt {
    public string Variable;  // Loop variable name
    public Expr Iterable;    // range(), list, etc.
    public List<Stmt> Body;
}

// Function definition
public class FunctionDef : Stmt {
    public string Name;
    public List<string> Parameters;
    public List<Stmt> Body;
}

// Class definition
public class ClassDef : Stmt {
    public string Name;
    public List<FunctionDef> Methods;  // Including __init__
}

// Return statement
public class ReturnStmt : Stmt {
    public Expr Value;  // null for bare 'return'
}

// Global declaration
public class GlobalStmt : Stmt {
    public List<string> Names;
}

// Control flow
public class BreakStmt : Stmt { }
public class ContinueStmt : Stmt { }
public class PassStmt : Stmt { }
```

#### 2.3.3 Expression Types
```csharp
// Binary operations: a + b, x < 5
public class BinaryExpr : Expr {
    public Expr Left;
    public Token Operator;  // Stores operator type
    public Expr Right;
}

// Unary operations: -x, not flag
public class UnaryExpr : Expr {
    public Token Operator;
    public Expr Right;
}

// Function/method calls: func(a, b)
public class CallExpr : Expr {
    public Expr Callee;  // Function/method to call
    public List<Expr> Arguments;
}

// Property access: obj.property
public class GetExpr : Expr {
    public Expr Object;
    public string Name;
}

// Property assignment: obj.property = value
public class SetExpr : Expr {
    public Expr Object;
    public string Name;
    public Expr Value;
}

// Indexing/slicing: list[0], list[1:3]
public class IndexExpr : Expr {
    public Expr Object;      // The list/dict being indexed
    public Expr Index;       // Single index or slice start
    public Expr SliceEnd;    // null for single index, otherwise slice end
}

// List literal: [1, 2, 3]
public class ListExpr : Expr {
    public List<Expr> Elements;
}

// Dict literal: {"a": 1, "b": 2}
public class DictExpr : Expr {
    public List<Expr> Keys;
    public List<Expr> Values;
}

// List comprehension: [x*2 for x in nums if x > 0]
public class ListCompExpr : Expr {
    public Expr Element;       // Expression to compute (x*2)
    public string Variable;    // Loop variable (x)
    public Expr Iterable;      // Source (nums)
    public Expr Condition;     // Optional filter (x > 0), null if absent
}

// Lambda: lambda x, y: x + y
public class LambdaExpr : Expr {
    public List<string> Parameters;
    public Expr Body;  // Single expression (no statements)
}

// Literals
public class LiteralExpr : Expr {
    public object Value;  // string, double, bool, null
}

// Variable reference: x
public class VariableExpr : Expr {
    public string Name;
}

// Self reference (in methods): self
public class SelfExpr : Expr { }
```

### 2.4 Parser (`Parser.cs`) - The Syntax Analyzer

#### 2.4.1 Operator Precedence (Lowest to Highest)
```
1. Lambda         (lambda x: ...)
2. Logic OR       (or)
3. Logic AND      (and)
4. Logic NOT      (not)
5. Comparison     (==, !=, <, >, <=, >=, in, is)
6. Bitwise OR     (|)
7. Bitwise XOR    (^)
8. Bitwise AND    (&)
9. Bitwise Shift  (<<, >>)
10. Addition      (+, -)
11. Multiplication(*, /, %)
12. Unary         (-, not, ~)
13. Power         (**)  [if supported]
14. Call/Index    (func(), list[0], obj.prop)
15. Primary       (literals, identifiers, parentheses)
```

#### 2.4.2 Key Parsing Methods
```csharp
// Entry point for statements
List<Stmt> ParseProgram() {
    List<Stmt> statements = new List<Stmt>();
    while (!IsAtEnd()) {
        statements.Add(ParseStatement());
    }
    return statements;
}

// Parse single statement
Stmt ParseStatement() {
    if (Match(TokenType.DEF)) return ParseFunctionDef();
    if (Match(TokenType.CLASS)) return ParseClassDef();
    if (Match(TokenType.IF)) return ParseIfStmt();
    if (Match(TokenType.WHILE)) return ParseWhileStmt();
    if (Match(TokenType.FOR)) return ParseForStmt();
    if (Match(TokenType.RETURN)) return ParseReturnStmt();
    if (Match(TokenType.GLOBAL)) return ParseGlobalStmt();
    if (Match(TokenType.BREAK)) return new BreakStmt();
    if (Match(TokenType.CONTINUE)) return new ContinueStmt();
    if (Match(TokenType.PASS)) return new PassStmt();
    
    // Assignment or expression statement
    return ParseAssignmentOrExpr();
}

// Parse expressions with precedence
Expr ParseExpression() {
    return ParseLambda();
}

Expr ParseLambda() {
    if (Match(TokenType.LAMBDA)) {
        List<string> params = ParseParameterList();
        Consume(TokenType.COLON, "Expected ':' after lambda parameters");
        Expr body = ParseLogicOr();
        return new LambdaExpr { Parameters = params, Body = body };
    }
    return ParseLogicOr();
}

Expr ParseLogicOr() {
    Expr left = ParseLogicAnd();
    while (Match(TokenType.OR)) {
        Token op = Previous();
        Expr right = ParseLogicAnd();
        left = new BinaryExpr { Left = left, Operator = op, Right = right };
    }
    return left;
}

// Continue for AND, NOT, Comparison, Bitwise, etc...
```

#### 2.4.3 List Comprehension Parsing
```csharp
Expr ParseListCompOrLiteral() {
    Consume(TokenType.LEFT_BRACKET, "Expected '['");
    
    if (Check(TokenType.RIGHT_BRACKET)) {
        Advance();
        return new ListExpr { Elements = new List<Expr>() };
    }
    
    Expr first = ParseExpression();
    
    // Check for 'for' keyword (list comprehension)
    if (Match(TokenType.FOR)) {
        string var = Consume(TokenType.IDENTIFIER, "Expected variable").Lexeme;
        Consume(TokenType.IN, "Expected 'in'");
        Expr iterable = ParseExpression();
        
        Expr condition = null;
        if (Match(TokenType.IF)) {
            condition = ParseExpression();
        }
        
        Consume(TokenType.RIGHT_BRACKET, "Expected ']'");
        return new ListCompExpr {
            Element = first,
            Variable = var,
            Iterable = iterable,
            Condition = condition
        };
    }
    
    // Regular list literal
    List<Expr> elements = new List<Expr> { first };
    while (Match(TokenType.COMMA)) {
        if (Check(TokenType.RIGHT_BRACKET)) break;
        elements.Add(ParseExpression());
    }
    Consume(TokenType.RIGHT_BRACKET, "Expected ']'");
    return new ListExpr { Elements = elements };
}
```

#### 2.4.4 Complex Boolean Logic Parsing
```csharp
// Handles: if (1 == 0) and (2 < 3) or (5 > 2):
Expr ParseComparison() {
    Expr left = ParseBitwiseOr();
    
    // Support chaining: a < b < c becomes (a < b) and (b < c)
    while (Match(TokenType.LESS, TokenType.GREATER, TokenType.LESS_EQUAL,
                 TokenType.GREATER_EQUAL, TokenType.EQUAL_EQUAL,
                 TokenType.BANG_EQUAL, TokenType.IN, TokenType.IS)) {
        Token op = Previous();
        Expr right = ParseBitwiseOr();
        left = new BinaryExpr { Left = left, Operator = op, Right = right };
    }
    return left;
}
```

---

## 3. The Runtime Engine (`PythonInterpreter.cs`)

### 3.1 Core State Management

#### 3.1.1 Scope System
```csharp
public class Scope {
    public Dictionary<string, object> Variables = new Dictionary<string, object>();
    public Scope Parent;  // For nested scopes
    
    public object Get(string name) {
        if (Variables.ContainsKey(name)) return Variables[name];
        if (Parent != null) return Parent.Get(name);
        throw new RuntimeError($"Undefined variable '{name}'");
    }
    
    public void Set(string name, object value) {
        Variables[name] = value;
    }
}

// In Interpreter
Stack<Scope> scopeStack = new Stack<Scope>();
Scope globalScope = new Scope();
```

#### 3.1.2 Instruction Budget Tracking
```csharp
public class PythonInterpreter {
    private int instructionCount = 0;
    private const int INSTRUCTIONS_PER_FRAME = 100;
    
    private IEnumerator CheckBudget() {
        instructionCount++;
        if (instructionCount >= INSTRUCTIONS_PER_FRAME) {
            instructionCount = 0;
            yield return null;  // Pause until next frame
        }
    }
    
    public IEnumerator Execute(List<Stmt> statements) {
        foreach (Stmt stmt in statements) {
            // Budget check before each statement
            yield return CheckBudget();
            
            var result = ExecuteStatement(stmt);
            while (result.MoveNext()) {
                yield return result.Current;
            }
        }
    }
}
```

### 3.2 Built-in Functions & Methods

#### 3.2.1 String Methods
```csharp
// Implement when string.Method() is called
object InvokeStringMethod(string str, string methodName, List<object> args) {
    switch (methodName) {
        case "split":
            string delimiter = args.Count > 0 ? (string)args[0] : " ";
            string[] parts = str.Split(new[] { delimiter }, StringSplitOptions.None);
            return new List<object>(parts);
            
        case "strip":
            return str.Trim();
            
        case "replace":
            if (args.Count < 2) throw new RuntimeError("replace() requires 2 arguments");
            return str.Replace((string)args[0], (string)args[1]);
            
        case "join":
            if (args.Count < 1) throw new RuntimeError("join() requires 1 argument");
            List<object> items = (List<object>)args[0];
            List<string> strings = items.ConvertAll(x => x.ToString());
            return string.Join(str, strings.ToArray());
            
        case "upper":
            return str.ToUpper();
            
        case "lower":
            return str.ToLower();
            
        case "startswith":
            return str.StartsWith((string)args[0]);
            
        case "endswith":
            return str.EndsWith((string)args[0]);
            
        default:
            throw new RuntimeError($"String has no method '{methodName}'");
    }
}
```

#### 3.2.2 List Methods (CRITICAL)
```csharp
object InvokeListMethod(List<object> list, string methodName, List<object> args) {
    switch (methodName) {
        case "append":
            if (args.Count < 1) throw new RuntimeError("append() requires 1 argument");
            list.Add(args[0]);
            return null;
            
        case "remove":
            if (args.Count < 1) throw new RuntimeError("remove() requires 1 argument");
            if (!list.Remove(args[0])) {
                throw new RuntimeError($"ValueError: {args[0]} not in list");
            }
            return null;
            
        case "pop":
            int index = args.Count > 0 ? (int)(double)args[0] : -1;
            if (index < 0) index += list.Count;
            if (index < 0 || index >= list.Count) {
                throw new RuntimeError($"IndexError: pop index out of range");
            }
            object item = list[index];
            list.RemoveAt(index);
            return item;
            
        case "insert":
            if (args.Count < 2) throw new RuntimeError("insert() requires 2 arguments");
            int idx = (int)(double)args[0];
            list.Insert(idx, args[1]);
            return null;
            
        case "clear":
            list.Clear();
            return null;
            
        case "sort":
            // Support key parameter: list.sort(key=lambda x: x.val)
            if (args.Count > 0 && args[0] != null) {
                // Key function provided
                object keyFunc = args[0];
                list.Sort((a, b) => {
                    object keyA = CallFunction(keyFunc, new List<object> { a });
                    object keyB = CallFunction(keyFunc, new List<object> { b });
                    return CompareValues(keyA, keyB);
                });
            } else {
                // Default sort
                list.Sort((a, b) => CompareValues(a, b));
            }
            
            // Check for reverse parameter
            bool reverse = false;
            if (args.Count > 1 && args[1] is bool) {
                reverse = (bool)args[1];
            }
            if (reverse) list.Reverse();
            
            return null;
            
        default:
            throw new RuntimeError($"List has no method '{methodName}'");
    }
}

int CompareValues(object a, object b) {
    if (a is double && b is double) {
        return ((double)a).CompareTo((double)b);
    }
    if (a is string && b is string) {
        return ((string)a).CompareTo((string)b);
    }
    throw new RuntimeError("Cannot compare incompatible types");
}
```

#### 3.2.3 Global Functions
```csharp
void RegisterBuiltins() {
    globalScope.Set("print", new BuiltinFunction("print", (args) => {
        string output = string.Join(" ", args.ConvertAll(x => x?.ToString() ?? "None"));
        ConsoleManager.Log(output);
        return null;
    }));
    
    globalScope.Set("len", new BuiltinFunction("len", (args) => {
        if (args.Count < 1) throw new RuntimeError("len() requires 1 argument");
        object obj = args[0];
        if (obj is List<object>) return (double)((List<object>)obj).Count;
        if (obj is string) return (double)((string)obj).Length;
        if (obj is Dictionary<object, object>) return (double)((Dictionary<object, object>)obj).Count;
        throw new RuntimeError("Object has no len()");
    }));
    
    globalScope.Set("range", new BuiltinFunction("range", (args) => {
        int start = 0, stop, step = 1;
        if (args.Count == 1) {
            stop = (int)(double)args[0];
        } else if (args.Count == 2) {
            start = (int)(double)args[0];
            stop = (int)(double)args[1];
        } else if (args.Count == 3) {
            start = (int)(double)args[0];
            stop = (int)(double)args[1];
            step = (int)(double)args[2];
        } else {
            throw new RuntimeError("range() requires 1-3 arguments");
        }
        
        List<object> result = new List<object>();
        for (int i = start; i < stop; i += step) {
            result.Add((double)i);
        }
        return result;
    }));
    
    globalScope.Set("min", new BuiltinFunction("min", (args) => {
        if (args.Count == 0) throw new RuntimeError("min() requires at least 1 argument");
        List<object> items = args.Count == 1 && args[0] is List<object> 
            ? (List<object>)args[0] : args;
        object minVal = items[0];
        foreach (object item in items) {
            if (CompareValues(item, minVal) < 0) minVal = item;
        }
        return minVal;
    }));
    
    globalScope.Set("max", new BuiltinFunction("max", (args) => {
        if (args.Count == 0) throw new RuntimeError("max() requires at least 1 argument");
        List<object> items = args.Count == 1 && args[0] is List<object> 
            ? (List<object>)args[0] : args;
        object maxVal = items[0];
        foreach (object item in items) {
            if (CompareValues(item, maxVal) > 0) maxVal = item;
        }
        return maxVal;
    }));
    
    globalScope.Set("abs", new BuiltinFunction("abs", (args) => {
        if (args.Count < 1) throw new RuntimeError("abs() requires 1 argument");
        return Math.Abs((double)args[0]);
    }));
    
    globalScope.Set("str", new BuiltinFunction("str", (args) => {
        if (args.Count < 1) return "";
        return args[0]?.ToString() ?? "None";
    }));
    
    globalScope.Set("int", new BuiltinFunction("int", (args) => {
        if (args.Count < 1) throw new RuntimeError("int() requires 1 argument");
        if (args[0] is double) return (double)(int)(double)args[0];
        if (args[0] is string) return (double)int.Parse((string)args[0]);
        throw new RuntimeError("Cannot convert to int");
    }));
    
    globalScope.Set("float", new BuiltinFunction("float", (args) => {
        if (args.Count < 1) throw new RuntimeError("float() requires 1 argument");
        if (args[0] is double) return args[0];
        if (args[0] is string) return double.Parse((string)args[0]);
        throw new RuntimeError("Cannot convert to float");
    }));
}
```

### 3.3 Statement Execution

#### 3.3.1 Loop Execution with Budget
```csharp
IEnumerator ExecuteWhileStmt(WhileStmt stmt) {
    while (true) {
        // Budget check for condition evaluation
        yield return CheckBudget();
        
        object condValue = EvaluateExpression(stmt.Condition);
        if (!IsTruthy(condValue)) break;
        
        foreach (Stmt bodyStmt in stmt.Body) {
            yield return CheckBudget();  // Check before each iteration
            
            var result = ExecuteStatement(bodyStmt);
            while (result.MoveNext()) {
                yield return result.Current;
            }
            
            if (breakFlag) {
                breakFlag = false;
                yield break;
            }
            if (continueFlag) {
                continueFlag = false;
                break;
            }
        }
    }
}

IEnumerator ExecuteForStmt(ForStmt stmt) {
    object iterableValue = EvaluateExpression(stmt.Iterable);
    List<object> items = GetIterableItems(iterableValue);
    
    foreach (object item in items) {
        yield return CheckBudget();  // Check for each iteration
        
        currentScope.Set(stmt.Variable, item);
        
        foreach (Stmt bodyStmt in stmt.Body) {
            var result = ExecuteStatement(bodyStmt);
            while (result.MoveNext()) {
                yield return result.Current;
            }
            
            if (breakFlag) {
                breakFlag = false;
                yield break;
            }
            if (continueFlag) {
                continueFlag = false;
                break;
            }
        }
    }
}
```

### 3.4 Expression Evaluation

#### 3.4.1 Binary Operations (All Operators)
```csharp
object EvaluateBinaryExpr(BinaryExpr expr) {
    object left = EvaluateExpression(expr.Left);
    object right = EvaluateExpression(expr.Right);
    
    switch (expr.Operator.Type) {
        // Arithmetic
        case TokenType.PLUS:
            if (left is string || right is string) {
                return left.ToString() + right.ToString();
            }
            return (double)left + (double)right;
        case TokenType.MINUS:
            return (double)left - (double)right;
        case TokenType.STAR:
            return (double)left * (double)right;
        case TokenType.SLASH:
            if ((double)right == 0) throw new RuntimeError("Division by zero");
            return (double)left / (double)right;
        case TokenType.PERCENT:
            return (double)left % (double)right;
            
        // Comparison
        case TokenType.EQUAL_EQUAL:
            return AreEqual(left, right);
        case TokenType.BANG_EQUAL:
            return !AreEqual(left, right);
        case TokenType.LESS:
            return (double)left < (double)right;
        case TokenType.GREATER:
            return (double)left > (double)right;
        case TokenType.LESS_EQUAL:
            return (double)left <= (double)right;
        case TokenType.GREATER_EQUAL:
            return (double)left >= (double)right;
            
        // Logical
        case TokenType.AND:
            if (!IsTruthy(left)) return left;
            return right;
        case TokenType.OR:
            if (IsTruthy(left)) return left;
            return right;
            
        // Membership
        case TokenType.IN:
            if (right is List<object>) {
                return ((List<object>)right).Contains(left);
            }
            if (right is string) {
                return ((string)right).Contains(left.ToString());
            }
            throw new RuntimeError("'in' requires iterable");
            
        // Bitwise
        case TokenType.AMPERSAND:
            return (double)((long)(double)left & (long)(double)right);
        case TokenType.PIPE:
            return (double)((long)(double)left | (long)(double)right);
        case TokenType.CARET:
            return (double)((long)(double)left ^ (long)(double)right);
        case TokenType.LEFT_SHIFT:
            return (double)((long)(double)left << (int)(double)right);
        case TokenType.RIGHT_SHIFT:
            return (double)((long)(double)left >> (int)(double)right);
            
        default:
            throw new RuntimeError($"Unknown operator {expr.Operator.Lexeme}");
    }
}
```

#### 3.4.2 List Comprehension Evaluation
```csharp
object EvaluateListComp(ListCompExpr expr) {
    List<object> result = new List<object>();
    object iterableValue = EvaluateExpression(expr.Iterable);
    List<object> items = GetIterableItems(iterableValue);
    
    Scope compScope = new Scope { Parent = currentScope };
    Scope previousScope = currentScope;
    currentScope = compScope;
    
    foreach (object item in items) {
        compScope.Set(expr.Variable, item);
        
        // Check optional condition
        if (expr.Condition != null) {
            object condResult = EvaluateExpression(expr.Condition);
            if (!IsTruthy(condResult)) continue;
        }
        
        // Compute element
        object element = EvaluateExpression(expr.Element);
        result.Add(element);
    }
    
    currentScope = previousScope;
    return result;
}
```

#### 3.4.3 Indexing & Slicing
```csharp
object EvaluateIndexExpr(IndexExpr expr) {
    object obj = EvaluateExpression(expr.Object);
    
    if (expr.SliceEnd != null) {
        // Slicing: list[start:end]
        if (!(obj is List<object>)) throw new RuntimeError("Can only slice lists");
        List<object> list = (List<object>)obj;
        
        int start = expr.Index != null ? (int)(double)EvaluateExpression(expr.Index) : 0;
        int end = (int)(double)EvaluateExpression(expr.SliceEnd);
        
        // Handle negative indices
        if (start < 0) start += list.Count;
        if (end < 0) end += list.Count;
        
        // Clamp to bounds
        start = Math.Max(0, Math.Min(start, list.Count));
        end = Math.Max(0, Math.Min(end, list.Count));
        
        List<object> slice = new List<object>();
        for (int i = start; i < end; i++) {
            slice.Add(list[i]);
        }
        return slice;
    } else {
        // Single index: list[i] or dict[key]
        if (obj is List<object>) {
            List<object> list = (List<object>)obj;
            int index = (int)(double)EvaluateExpression(expr.Index);
            if (index < 0) index += list.Count;
            if (index < 0 || index >= list.Count) {
                throw new RuntimeError($"IndexError: list index out of range");
            }
            return list[index];
        }
        if (obj is Dictionary<object, object>) {
            Dictionary<object, object> dict = (Dictionary<object, object>)obj;
            object key = EvaluateExpression(expr.Index);
            if (!dict.ContainsKey(key)) {
                throw new RuntimeError($"KeyError: {key}");
            }
            return dict[key];
        }
        throw new RuntimeError("Object is not indexable");
    }
}
```

---

## 4. Unity Integration & Safety

### 4.1 Game Built-in Methods (`GameBuiltinMethods.cs`)

```csharp
public class GameBuiltinMethods {
    public static void RegisterGameFunctions(Scope globalScope, MonoBehaviour context) {
        // Movement (yields WaitForSeconds until that coroutine is complete)
        globalScope.Set("move", new BuiltinYieldFunction("move", (args) => {
            if (args.Count < 1) throw new RuntimeError("move() requires direction");
            string direction = args[0].ToString();
            return context.StartCoroutine(MoveCoroutine(direction));
        }));
        
        // Harvest (yields)
        globalScope.Set("harvest", new BuiltinYieldFunction("harvest", (args) => {
            return context.StartCoroutine(HarvestCoroutine());
        }));
        
        // Sleep (yields) - ALWAYS YIELDS REGARDLESS OF BUDGET
        globalScope.Set("sleep", new BuiltinYieldFunction("sleep", (args) => {
            if (args.Count < 1) throw new RuntimeError("sleep() requires time");
            float seconds = (float)(double)args[0];  // Support decimal values
            return new WaitForSeconds(seconds);
        }));
        
        // Non-yielding queries
        globalScope.Set("get_pos_x", new BuiltinFunction("get_pos_x", (args) => {
            return (double)PlayerController.Instance.Position.x;
        }));
        
        globalScope.Set("get_pos_y", new BuiltinFunction("get_pos_y", (args) => {
            return (double)PlayerController.Instance.Position.y;
        }));
        
        globalScope.Set("get_grid_size", new BuiltinFunction("get_grid_size", (args) => {
            return new List<object> { 
                (double)GridManager.Instance.Width, 
                (double)GridManager.Instance.Height 
            };
        }));
        
        globalScope.Set("is_passable", new BuiltinFunction("is_passable", (args) => {
            if (args.Count < 2) throw new RuntimeError("is_passable() requires x, y");
            int x = (int)(double)args[0];
            int y = (int)(double)args[1];
            return GridManager.Instance.IsPassable(x, y);
        }));
        
        globalScope.Set("say", new BuiltinFunction("say", (args) => {
            if (args.Count < 1) throw new RuntimeError("say() requires text");
            string text = args[0].ToString();
            DialogueManager.Instance.ShowBubble(text);
            return null;
        }));
        
        // Additional game-specific functions
        globalScope.Set("can_move", new BuiltinFunction("can_move", (args) => {
            string dir = args[0].ToString();
            return PlayerController.Instance.CanMove(dir);
        }));
        
        globalScope.Set("is_goal", new BuiltinFunction("is_goal", (args) => {
            int x = (int)(double)args[0];
            int y = (int)(double)args[1];
            return GridManager.Instance.IsGoal(x, y);
        }));
        
        globalScope.Set("is_block", new BuiltinFunction("is_block", (args) => {
            int x = (int)(double)args[0];
            int y = (int)(double)args[1];
            return GridManager.Instance.IsBlocked(x, y);
        }));
        
        globalScope.Set("submit", new BuiltinFunction("submit", (args) => {
            string code = args[0].ToString();
            return LevelManager.Instance.CheckSolution(code);
        }));
    }
    
    static IEnumerator MoveCoroutine(string direction) {
        // Example: smooth movement over 0.3 seconds
        yield return PlayerController.Instance.Move(direction);
    }
    
    static IEnumerator HarvestCoroutine() {
        yield return PlayerController.Instance.Harvest();
    }
}
```

### 4.2 Coroutine Runner (`CoroutineRunner.cs`)

```csharp
public class CoroutineRunner : MonoBehaviour {
    private PythonInterpreter interpreter;
    private Coroutine currentScript;
    
    void Start() {
        interpreter = new PythonInterpreter(this);
    }
    
    public void RunScript(string sourceCode) {
        if (currentScript != null) {
            StopCoroutine(currentScript);
        }
        
        currentScript = StartCoroutine(SafeExecute(sourceCode));
    }
    
    public void StopScript() {
        if (currentScript != null) {
            StopCoroutine(currentScript);
            interpreter.Reset();
            ConsoleManager.Instance.Log("Script stopped.");
        }
    }
    
    IEnumerator SafeExecute(string sourceCode) {
        // Step 1: Parse
        List<Stmt> program;
        try {
            Lexer lexer = new Lexer(sourceCode);
            List<Token> tokens = lexer.Tokenize();
            Parser parser = new Parser(tokens);
            program = parser.Parse();
        } catch (LexerError e) {
            ConsoleManager.Instance.LogError($"Lexer Error (Line {e.LineNumber}): {e.Message}");
            yield break;
        } catch (ParserError e) {
            ConsoleManager.Instance.LogError($"Parser Error (Line {e.LineNumber}): {e.Message}");
            yield break;
        }
        
        // Step 2: Execute
        IEnumerator execution = interpreter.Execute(program);
        
        // Wrap in try-catch to handle runtime errors
        bool hasError = false;
        while (!hasError) {
            object current = null;
            try {
                if (!execution.MoveNext()) break;
                current = execution.Current;
            } catch (RuntimeError e) {
                ConsoleManager.Instance.LogError($"Runtime Error (Line {e.LineNumber}): {e.Message}");
                hasError = true;
                interpreter.Reset();
                yield break;
            } catch (System.Exception e) {
                ConsoleManager.Instance.LogError($"Unexpected Error: {e.Message}\n{e.StackTrace}");
                hasError = true;
                interpreter.Reset();
                yield break;
            }
            
            if (current != null) {
                yield return current;
            }
        }
        
        if (!hasError) {
            ConsoleManager.Instance.Log("Script completed successfully.");
        }
    }
}
```

### 4.3 Console Manager (`ConsoleManager.cs`)

```csharp
public class ConsoleManager : MonoBehaviour {
    public static ConsoleManager Instance;
    public Text consoleText;
    public ScrollRect scrollRect;
    
    private List<string> lines = new List<string>();
    private const int MAX_LINES = 100;
    
    void Awake() {
        Instance = this;
    }
    
    public void Log(string message) {
        AddLine($"<color=white>{message}</color>");
    }
    
    public void LogError(string message) {
        AddLine($"<color=red>[ERROR] {message}</color>");
    }
    
    void AddLine(string line) {
        lines.Add(line);
        if (lines.Count > MAX_LINES) {
            lines.RemoveAt(0);
        }
        
        consoleText.text = string.Join("\n", lines.ToArray());
        
        // Auto-scroll to bottom
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }
    
    public void Clear() {
        lines.Clear();
        consoleText.text = "";
    }
}
```

---

## 5. Comprehensive Test Suite

### Test 1: Instruction Budget (Small Loop - Instant)
```python
# Should execute in 1 frame (100 ops)
sum = 0
for i in range(100):
    sum += 1
print("Sum (100):", sum)  # Expect: 100
```

### Test 2: Instruction Budget (Large Loop - Time-Sliced)
```python
# Should execute across ~10 frames (1000 ops)
sum = 0
for i in range(1000):
    sum += 1
print("Sum (1000):", sum)  # Expect: 1000
```

### Test 3: Nested Loops (Budget Interaction)
```python
# Outer: 50 iterations (instant)
# Inner: 1000 iterations each (time-sliced)
# Total: 50,000 ops = ~500 frames
sum = 0
for y in range(50):
    for x in range(1000):
        sum += 1
print("Nested sum:", sum)  # Expect: 50000
```

### Test 4: Sleep vs Budget (Independence)
```python
# Sleep ALWAYS pauses, regardless of ops
for i in range(50):
    print("Iteration:", i)
    if i % 10 == 0:
        sleep(2)  # Pauses for 2 seconds
print("Done")
```

### Test 5: Complex Boolean Logic
```python
# Test: (condition1) and (condition2) or (condition3)
if (1 == 0) and (2 < 3) or (5 > 2):
    print("Logic works!")  # Should print

if (True and False) or (10 > 5 and 3 < 7):
    print("Complex logic!")  # Should print

x = 10
if not (x < 5) and (x % 2 == 0 or x > 100):
    print("X is valid")  # Should print
```

### Test 6: List Operations (Negative Indexing & Slicing)
```python
items = [0, 1, 2, 3, 4, 5]
print("Original:", items)

# Slicing
print("Slice [1:4]:", items[1:4])   # [1, 2, 3]
print("Slice [:3]:", items[:3])     # [0, 1, 2]
print("Slice [3:]:", items[3:])     # [3, 4, 5]
print("Slice [-2:]:", items[-2:])   # [4, 5]

# Negative indexing
print("Last item:", items[-1])      # 5
print("Second last:", items[-2])    # 4

# Modification
items.append(6)
items.remove(2)
items.pop()
items[0] = 99
print("Modified:", items)  # [99, 1, 3, 4, 5]
```

### Test 7: Bitwise Operations & Recursion
```python
# Bitwise operations
a = 60   # 0011 1100
b = 13   # 0000 1101

print("AND:", a & b)      # 12 (0000 1100)
print("OR:", a | b)       # 61 (0011 1101)
print("XOR:", a ^ b)      # 49 (0011 0001)
print("NOT:", ~a)         # -61 (two's complement)
print("Left shift:", a << 2)   # 240 (1111 0000)
print("Right shift:", a >> 2)  # 15 (0000 1111)

# Recursion (Fibonacci)
def fib(n):
    if n <= 1:
        return n
    return fib(n - 1) + fib(n - 2)

print("Fib(10):", fib(10))  # Expect: 55

# Factorial
def factorial(n):
    if n <= 1:
        return 1
    return n * factorial(n - 1)

print("Factorial(5):", factorial(5))  # Expect: 120
```

### Test 8: Object-Oriented Programming
```python
class Robot:
    def __init__(self, name, battery):
        self.name = name
        self.battery = battery
        self.position = [0, 0]
    
    def move_to(self, x, y):
        cost = abs(x - self.position[0]) + abs(y - self.position[1])
        if self.battery >= cost:
            self.battery -= cost
            self.position = [x, y]
            print(self.name + " moved to " + str(self.position))
        else:
            print(self.name + " has insufficient battery!")
    
    def recharge(self, amount):
        self.battery += amount
        print(self.name + " recharged. Battery: " + str(self.battery))

bot1 = Robot("FarmerBot", 100)
bot2 = Robot("HarvesterBot", 50)

bot1.move_to(5, 3)  # Cost: 8, Battery: 92
bot2.move_to(10, 10)  # Cost: 20, Battery: 30
bot2.recharge(50)  # Battery: 80
```

### Test 9: Dictionary & Nested Data Structures
```python
# Simple dictionary
player = {"name": "Alice", "score": 100, "level": 5}
print("Player:", player["name"], "Level:", player["level"])

# Modifying dict
player["score"] += 50
player["level"] = 6
print("Updated score:", player["score"])

# Nested structures (dict in list, list in dict)
game_state = {
    "players": [
        {"id": 1, "name": "Alice", "inventory": ["sword", "potion"]},
        {"id": 2, "name": "Bob", "inventory": ["bow", "arrow", "shield"]}
    ],
    "settings": {"difficulty": "hard", "sound": True}
}

# Deep access
print("First player:", game_state["players"][0]["name"])
print("Inventory:", game_state["players"][1]["inventory"][2])  # "shield"

# Modification
game_state["players"][0]["inventory"].append("helmet")
print("Alice inventory:", game_state["players"][0]["inventory"])
```

### Test 10: List Comprehensions & Lambdas
```python
# Basic list comprehension
nums = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10]
squares = [x * x for x in nums]
print("Squares:", squares)

# With condition
evens = [x for x in nums if x % 2 == 0]
print("Evens:", evens)  # [2, 4, 6, 8, 10]

odds = [x for x in nums if x % 2 == 1]
print("Odds:", odds)  # [1, 3, 5, 7, 9]

# Nested comprehension
pairs = [[i, j] for i in range(3) for j in range(3)]
print("Pairs:", pairs)  # [[0,0], [0,1], [0,2], [1,0], ...]

# Lambda functions
double = lambda x: x * 2
print("Double 5:", double(5))  # 10

add = lambda a, b: a + b
print("Add:", add(10, 20))  # 30

# Sorting with lambda
class Item:
    def __init__(self, name, price):
        self.name = name
        self.price = price

items = [Item("apple", 5), Item("banana", 2), Item("cherry", 8)]
items.sort(key=lambda item: item.price)
print("Sorted by price:", [item.name for item in items])  # ['banana', 'apple', 'cherry']

# Reverse sort
items.sort(key=lambda item: item.price, reverse=True)
print("Reverse:", [item.name for item in items])  # ['cherry', 'apple', 'banana']
```

### Test 11: String Manipulation
```python
text = "  Hello, World!  "

# Basic operations
print("Original:", text)
print("Stripped:", text.strip())  # "Hello, World!"
print("Upper:", text.upper())
print("Lower:", text.lower())

# Split and join
sentence = "apple,banana,cherry,date"
fruits = sentence.split(",")
print("Split:", fruits)  # ['apple', 'banana', 'cherry', 'date']

joined = " | ".join(fruits)
print("Joined:", joined)  # "apple | banana | cherry | date"

# Replace
replaced = sentence.replace(",", " and ")
print("Replaced:", replaced)  # "apple and banana and cherry and date"

# Startswith/endswith
if text.strip().startswith("Hello"):
    print("Starts with Hello!")

if text.strip().endswith("!"):
    print("Ends with exclamation!")
```

### Test 12: Pathfinding Algorithm (A* Simulation)
```python
# Grid: 0 = passable, 1 = wall
grid = [
    [0, 0, 0, 0, 0],
    [0, 1, 1, 1, 0],
    [0, 0, 0, 1, 0],
    [0, 1, 0, 0, 0],
    [0, 0, 0, 1, 0]
]

start = [0, 0]
goal = [4, 4]

def get_neighbors(pos):
    neighbors = []
    x = pos[0]
    y = pos[1]
    
    # Four directions
    directions = [[0, 1], [1, 0], [0, -1], [-1, 0]]
    for d in directions:
        nx = x + d[0]
        ny = y + d[1]
        if nx >= 0 and nx < 5 and ny >= 0 and ny < 5:
            if grid[ny][nx] == 0:  # Passable
                neighbors.append([nx, ny])
    
    return neighbors

def heuristic(a, b):
    # Manhattan distance
    return abs(a[0] - b[0]) + abs(a[1] - b[1])

def list_to_str(lst):
    return str(lst[0]) + "," + str(lst[1])

# A* algorithm
open_list = [start]
came_from = {}
g_score = {list_to_str(start): 0}
f_score = {list_to_str(start): heuristic(start, goal)}

found = False
while len(open_list) > 0:
    # Find node with lowest f_score
    current = open_list[0]
    best_f = f_score.get(list_to_str(current), 9999)
    
    for node in open_list:
        f = f_score.get(list_to_str(node), 9999)
        if f < best_f:
            current = node
            best_f = f
    
    # Check if reached goal
    if current[0] == goal[0] and current[1] == goal[1]:
        print("Path found!")
        found = True
        break
    
    open_list.remove(current)
    
    # Explore neighbors
    for neighbor in get_neighbors(current):
        tentative_g = g_score.get(list_to_str(current), 9999) + 1
        neighbor_key = list_to_str(neighbor)
        
        if tentative_g < g_score.get(neighbor_key, 9999):
            came_from[neighbor_key] = current
            g_score[neighbor_key] = tentative_g
            f_score[neighbor_key] = tentative_g + heuristic(neighbor, goal)
            
            # Add to open list if not present
            in_open = False
            for node in open_list:
                if node[0] == neighbor[0] and node[1] == neighbor[1]:
                    in_open = True
                    break
            
            if not in_open:
                open_list.append(neighbor)

if found:
    # Reconstruct path
    path = [goal]
    current_key = list_to_str(goal)
    while current_key in came_from:
        current = came_from[current_key]
        path.append(current)
        current_key = list_to_str(current)
    
    path.reverse()
    print("Path:", path)
else:
    print("No path found!")
```

### Test 13: Unity Game Integration
```python
say("Starting mission...")
sleep(1.0)

# Get initial position
x = get_pos_x()
y = get_pos_y()

print("Starting at:", x, y)

# Simple movement with obstacle detection
steps = 0
while not is_goal(x, y) and steps < 20:
    if can_move("right"):
        move("right")
        x = get_pos_x()
        print("Moved right to:", x, y)
    elif can_move("down"):
        move("down")
        y = get_pos_y()
        print("Moved down to:", x, y)
    else:
        say("Stuck! Finding alternate route...")
        if can_move("up"):
            move("up")
        elif can_move("left"):
            move("left")
    
    steps += 1
    sleep(0.5)

if is_goal(x, y):
    say("Goal reached!")
    submit("mission_complete")
else:
    say("Could not reach goal")
```

### Test 14: Error Handling Edge Cases
```python
# Division by zero
try:
    result = 10 / 0
except:
    print("Caught division by zero")

# List index out of bounds
items = [1, 2, 3]
# This should throw IndexError:
# print(items[10])

# Undefined variable
# This should throw RuntimeError:
# print(undefined_var)

# Invalid list operation
numbers = [1, 2, 3]
# This should throw ValueError:
# numbers.remove(99)
```

### Test 15: Mixed Data Types & Type Coercion
```python
# String concatenation
name = "Alice"
age = 25
message = "Hello, " + name + "! Age: " + str(age)
print(message)

# Number operations
a = 10
b = 3.5
print("Int + Float:", a + b)  # 13.5
print("Division:", a / b)  # 2.857...
print("Modulo:", a % b)  # 3.0

# List with mixed types
mixed = [1, "two", 3.0, True, None]
print("Mixed list:", mixed)

# Dict with mixed types
data = {
    "count": 42,
    "name": "test",
    "values": [1, 2, 3],
    "nested": {"a": 1, "b": 2}
}
print("Dict:", data["name"], data["count"])
```

### Test 16: Advanced Loop Control
```python
# Break statement
for i in range(100):
    if i == 10:
        break
    print("i:", i)
print("Broke at 10")

# Continue statement
count = 0
for i in range(20):
    if i % 3 == 0:
        continue
    count += 1
print("Non-multiples of 3:", count)  # 14

# Nested loops with break
found = False
for i in range(5):
    for j in range(5):
        if i * j == 12:
            print("Found at:", i, j)
            found = True
            break
    if found:
        break
```

### Test 17: Function Definitions & Scoping
```python
# Global variable
counter = 0

def increment():
    global counter
    counter += 1
    print("Counter:", counter)

increment()  # 1
increment()  # 2
increment()  # 3

# Local vs global
x = 10

def test_scope():
    x = 20  # Local x
    print("Local x:", x)  # 20

test_scope()
print("Global x:", x)  # 10

# Function with multiple parameters
def calculate(a, b, operation):
    if operation == "add":
        return a + b
    elif operation == "subtract":
        return a - b
    elif operation == "multiply":
        return a * b
    elif operation == "divide":
        if b == 0:
            return None
        return a / b
    return None

print("10 + 5 =", calculate(10, 5, "add"))
print("10 * 5 =", calculate(10, 5, "multiply"))
```

### Test 18: Comprehensive Boolean Logic
```python
# Complex conditions
x = 10
y = 20
z = 30

# AND chain
if x < y and y < z and x < z:
    print("All conditions true")

# OR chain
if x > 100 or y > 15 or z < 0:
    print("At least one true")

# Mixed AND/OR
if (x < 5 and y > 10) or (z > 25 and x < 15):
    print("Complex logic works")

# NOT with grouping
if not (x > 20 or y < 10):
    print("Negation works")

# In operator
numbers = [1, 2, 3, 4, 5]
if 3 in numbers and not (10 in numbers):
    print("Membership test works")

# Comparison chaining (if supported)
if x < y < z:
    print("Chained comparison works")
```

---

## 6. File Organization & Code Structure

### 6.1 File Naming Convention
All C# files must follow these conventions:
* **Classes:** `ClassName.cs` (PascalCase)
* **Regions:** Use `#region` to organize code sections:
  - `#region Public API` - Public methods/properties
  - `#region Private API` - Private/protected methods
  - `#region Unity Lifecycle` - Start, Update, OnDestroy, etc.
  - `#region Enums` - Enum definitions
  - `#region Events` - Event declarations
  - `#region Nested Classes` - Inner class definitions

### 6.2 Required Files (Complete List)
1. **Token.cs** - Token class and TokenType enum
2. **Lexer.cs** - Tokenizer with indentation handling
3. **AST.cs** - All AST node classes (Stmt, Expr, and subclasses)
4. **Parser.cs** - Recursive descent parser
5. **PythonInterpreter.cs** - Main execution engine with instruction budget
6. **Scope.cs** - Variable scope management
7. **BuiltinFunction.cs** - Wrapper for built-in functions
8. **ClassInstance.cs** - Runtime class instance representation
9. **GameBuiltinMethods.cs** - Unity-specific game commands
10. **CoroutineRunner.cs** - Safe coroutine execution wrapper
11. **ConsoleManager.cs** - UI console for print() output
12. **DemoScripts.cs** - All test scripts as string constants
13. **Exceptions.cs** - Custom exception classes (LexerError, ParserError, RuntimeError)

### 6.3 Code Quality Requirements
* **Comments:** Every public method must have XML documentation
* **Error Messages:** All errors must include context (line number, variable name)
* **Validation:** All user input (indices, keys, arguments) must be validated
* **Null Checks:** All object access must check for null
* **.NET 2.0 Compliance:** No `yield return` inside try-catch in IEnumerators

---

## 7. Advanced Features & Edge Cases

### 7.1 Operator Precedence Edge Cases
```python
# Test mixed precedence
result = 2 + 3 * 4  # Should be 14, not 20
print("Precedence:", result)

# Bitwise vs comparison
a = 5
b = 3
if a & b == 1:  # & should evaluate before ==
    print("Bitwise precedence works")

# Unary minus with exponentiation
x = -2 * -3  # Should be 6
print("Unary:", x)
```

### 7.2 Edge Cases for Indexing
```python
# Empty list slicing
empty = []
print("Empty slice:", empty[0:10])  # Should be []

# Out of bounds slicing (should not error)
nums = [1, 2, 3]
print("OOB slice:", nums[10:20])  # Should be []

# Negative step (if supported)
# nums = [1, 2, 3, 4, 5]
# print("Reverse:", nums[::-1])  # [5, 4, 3, 2, 1]
```

### 7.3 Recursion Depth
```python
# Test deep recursion (should handle up to 1000 calls)
def deep_recurse(n):
    if n == 0:
        return "done"
    return deep_recurse(n - 1)

print(deep_recurse(100))  # Should succeed
# print(deep_recurse(2000))  # May hit stack limit
```

### 7.4 Type Coercion in Comparisons
```python
# Number to string comparison (should handle gracefully)
if str(42) == "42":
    print("Type coercion works")

# List equality
list1 = [1, 2, 3]
list2 = [1, 2, 3]
# Deep equality (should compare contents, not references)
# if list1 == list2:
#     print("List equality works")
```

---

## 8. Implementation Checklist & Final Instructions

### 8.1 AI Generation Checklist
When generating code, ensure you:
- ✅ Create **separate** `.cs` files for each class
- ✅ Use `#region` tags to organize code
- ✅ Add XML documentation (`/// <summary>`) for all public methods
- ✅ Implement **all** test scripts in `DemoScripts.cs`
- ✅ Handle **all** exceptions with line numbers
- ✅ Implement instruction budget with `INSTRUCTIONS_PER_FRAME = 100`
- ✅ Support **decimal and integer** numbers (store as `double`)
- ✅ Implement `sleep()` to **always** yield, independent of budget
- ✅ Support complex boolean logic: `(a and b) or (c and not d)`
- ✅ Handle negative list indices: `items[-1]`
- ✅ Support list slicing: `items[1:4]`, `items[:3]`, `items[3:]`
- ✅ Implement list comprehensions: `[x*2 for x in nums if x > 0]`
- ✅ Implement lambda functions: `lambda x, y: x + y`
- ✅ Support sorting with key: `list.sort(key=lambda x: x.val)`
- ✅ Implement all string methods: `split`, `join`, `strip`, `replace`
- ✅ Implement all list methods: `append`, `remove`, `pop`, `sort`
- ✅ Support classes with `__init__` and `self`
- ✅ Support dictionaries with any hashable key
- ✅ Handle nested data structures (lists in dicts, dicts in lists)
- ✅ Implement recursion (up to 1000 depth)
- ✅ Validate all array access (throw IndexError/KeyError)
- ✅ Reset interpreter state on error (`Interpreter.Reset()`)
- ✅ Comply with .NET 2.0 restrictions (no yield in try-catch)

### 8.2 Error Reporting Template
Every error must follow this format:
```
[ERROR TYPE] (Line X): [Detailed message]
```

Examples:
```
LexerError (Line 5): Indentation mismatch - expected 4 spaces, found 2
ParserError (Line 12): Expected ')' after function arguments
RuntimeError (Line 23): IndexError - list index out of range
RuntimeError (Line 45): NameError - undefined variable 'player_pos'
```

### 8.3 Performance Targets
* **Small loops** (<100 iterations): Execute in 1 frame (instant)
* **Medium loops** (100-1000 iterations): Time-slice across 1-10 frames
* **Large loops** (>1000 iterations): Time-slice proportionally
* **Sleep calls**: Always yield for exact duration, independent of ops
* **Game commands**: Always yield and pause Python until Unity action completes

---

## 9. Prompt Usage Guide

### 9.1 How to Modify This Prompt

**To add a new Python feature:**
1. Add token types in Section 2.1 (if new syntax)
2. Add AST node in Section 2.3
3. Add parsing logic in Section 2.4
4. Add execution/evaluation in Section 3.3 or 3.4
5. Add test case in Section 5

**To add a new game command:**
1. Go to Section 4.1 (`GameBuiltinMethods.cs`)
2. Add function registration in `RegisterGameFunctions()`
3. Implement coroutine if it needs to yield
4. Add test case in Section 5

**To change timing behavior:**
1. Modify `INSTRUCTIONS_PER_FRAME` in Section 3.1.2
2. Adjust `CheckBudget()` logic
3. Update timing examples in Section 5

**To add a new built-in function:**
1. Go to Section 3.2.3
2. Add registration in `RegisterBuiltins()`
3. Implement function logic
4. Add test case in Section 5

### 9.2 Prompt Classification
This type of prompt is called:
* **"System Specification Prompt"** (formal name)
* **"Code Generation Specification"** (industry term)
* **"Technical Blueprint Prompt"** (descriptive name)

It combines:
* Requirements documentation
* API specification
* Test-driven development (TDD)
* Domain-specific language (DSL) definition

---

## 10. Final Generation Command

**GENERATE NOW:**
Create **all** C# files as separate artifacts with:
1. Proper `#region` organization
2. XML documentation on public methods
3. Complete error handling with line numbers
4. Full instruction budget implementation
5. All 18 test cases validated
6. .NET 2.0 compliance
7. Unity 2020.3+ compatibility

**Target Unity Version:** 2020.3+  
**Target .NET:** 2.0 Standard  
**Estimated Lines of Code:** As you seem right.

Begin generation of all files now.
**Generate the code now (covering all edge cases, and .Net 2.0 limitation that i mentioned at the begining) all in seperate files as required**