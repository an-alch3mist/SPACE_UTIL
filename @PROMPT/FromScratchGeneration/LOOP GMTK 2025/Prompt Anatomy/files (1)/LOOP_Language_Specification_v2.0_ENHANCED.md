# LOOP Language Specification v2.0 - Enhanced with Enums & Game Features

**Role:** Act as a Principal Unity Architect and Compiler Language Engineer.  
**Project:** "Farmer Was Replaced" Clone For Learning - A Programming Puzzle Game.  
**Goal:** Generate complete, production-ready C# source code for a Coroutine-based Python Interpreter in Unity.

---

## QUICK MODIFICATION GUIDE

### 🎯 How to Use This Prompt

This prompt is organized into **clearly marked sections**. Each section has a `[MODIFY HERE]` tag showing where to make changes.

**Common Modifications:**

| **What You Want to Add** | **Go To Section** | **What To Do** |
|---------------------------|-------------------|----------------|
| New game function (like `harvest()`) | Section 4.1 - Game Builtins | Add function to `<game_builtins>` list |
| New operator (like `%`, `**`) | Section 2.1.1 - Token Types | Add token to appropriate category |
| New enum type (like `Ground`, `Items`) | Section 1.2.3 - Enums | Add to `<enum_types>` |
| New built-in constant (like `North`) | Section 4.3 - Built-in Constants | Add to `<built_in_constants>` |
| Indentation rules | Section 2.2.2 - Indentation | Modify `<indentation_rules>` |
| Return-type built-ins | Section 4.1 - Game Builtins | Add with `<return_type>` specified |
| Operator precedence | Section 3.4 - Operator Precedence | Update precedence table |
| Test cases | Section 5 - Test Suite | Add new `<test_case>` block |

---

## ═══════════════════════════════════════════════════════════════
## SECTION 0: META-INSTRUCTIONS & CRITICAL RULES
## ═══════════════════════════════════════════════════════════════

<meta_instruction priority="CRITICAL">
  **Before generating any code, the AI MUST:**
  
  1. ✅ Read the ENTIRE specification carefully
  2. ✅ Review all test cases in Section 5 (Test Suite)
  3. ✅ Verify generated code will pass EVERY test case
  4. ✅ Check operator precedence matches Section 3.4
  5. ✅ Validate indentation rules from Section 2.2.2
  6. ✅ Confirm all game built-in functions are registered (Section 4.1)
  7. ✅ Confirm all enums are registered (Section 1.2.3)
  8. ✅ Confirm all built-in constants are registered (Section 4.3)
  
  **All generated code MUST:**
  - Pass the entire test suite
  - Follow .NET 2.0 constraints (no `yield return` inside try-catch in IEnumerators)
  - Include proper error handling with line numbers
  - Implement instruction budget system
  - Support all features listed in Section 8.1 checklist
  - Support enum member access (e.g., Ground.Soil)
  - Support built-in constants (e.g., North, South)
  
  **If you cannot generate code that passes all tests:**
  - Explain which test case is ambiguous
  - Request clarification before proceeding
</meta_instruction>

---

## ═══════════════════════════════════════════════════════════════
## SECTION 1: SYSTEM ARCHITECTURE & CORE PHILOSOPHY
## ═══════════════════════════════════════════════════════════════

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

**[MODIFY HERE - INSTRUCTION BUDGET]**
```xml
<instruction_budget>
  <default_budget>100</default_budget>
  <description>
    Number of operations allowed per frame before yielding.
    Lower = smoother frame rate but slower script execution.
    Higher = faster script execution but possible frame drops.
  </description>
  <recommended_range>
    <minimum>50</minimum>
    <maximum>500</maximum>
  </recommended_range>
</instruction_budget>
```

* **Increment Logic:** Increment `instructionCount` for:
  - Every statement execution (`x = 1`, `print(x)`)
  - Every expression evaluation (`x < 5`, `a + b`)
  - Every loop iteration check (`for` or `while` condition)
  - Every function call entry
* **Time-Slicing:** When `instructionCount >= INSTRUCTIONS_PER_FRAME`:
  - `yield return null` (pause until next frame)
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

# Example 3: Sleep ALWAYS pauses (independent of budget)
for i in range(50):
    print("iteration:", i)
    if i % 10 == 0:
        sleep(2.0)  # MUST yield WaitForSeconds(2.0)

# Example 4: Built-in function move() ALWAYS pauses (independent of budget)
for i in range(50):
    move(North)  # MUST yield until IEnumerator coroutine completes
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

**[MODIFY HERE - DATA TYPES]**
```xml
<type_system>
  <numeric_type>
    <storage>double</storage>
    <description>
      All numeric values stored as C# double internally.
      Supports both integer and floating-point literals.
    </description>
    <automatic_conversion>
      <context>Array indexing, loop counters</context>
      <method>Cast to (int) when needed</method>
    </automatic_conversion>
  </numeric_type>
  
  <string_type>
    <storage>C# string</storage>
    <mutability>Immutable</mutability>
    <methods>
      split, join, strip, replace, upper, lower, 
      startswith, endswith, find, count
    </methods>
  </string_type>
  
  <boolean_type>
    <storage>C# bool</storage>
    <literals>True, False</literals>
  </boolean_type>
  
  <none_type>
    <storage>C# null</storage>
    <literal>None</literal>
  </none_type>
  
  <list_type>
    <storage>List&lt;object&gt;</storage>
    <description>Dynamic heterogeneous collections</description>
    <methods>
      append, remove, pop, insert, sort, reverse, 
      clear, count, index, extend
    </methods>
  </list_type>
  
  <dictionary_type>
    <storage>Dictionary&lt;object, object&gt;</storage>
    <key_requirements>Must be hashable (immutable types)</key_requirements>
    <methods>
      keys, values, items, get, pop, clear, update
    </methods>
  </dictionary_type>
  
  <class_type>
    <support>Custom user-defined classes</support>
    <features>
      - Methods with 'self' parameter
      - __init__ constructor
      - Instance variables
      - Inheritance (if implemented)
    </features>
  </class_type>
  
  <enum_type>
    <support>Built-in game enums (not user-definable)</support>
    <features>
      - Member access via dot notation (Ground.Soil)
      - Comparison with == and !=
      - Use in if statements and expressions
      - Return values from game functions
    </features>
  </enum_type>
</type_system>
```

#### 1.2.2 Security Constraints (.NET 2.0 Compatibility)

* **No Reflection:** Disable `System.Reflection` access to external assemblies
* **No File I/O:** Block `System.IO` operations
* **No Threading:** Reject `System.Threading` APIs
* **IEnumerator Limitation:** CANNOT use `yield return` inside `try-catch` blocks (Unity 2020.3 .NET 2.0 constraint)
  - Solution: Store yield instructions in variables outside try-catch, then yield after the block

#### 1.2.3 Enum Support (NEW FEATURE)

**[MODIFY HERE - ADD NEW ENUM TYPES]**

```xml
<enum_types>
  <description>
    Enums are built-in, predefined types accessible via dot notation.
    Users CANNOT define custom enums - only use the ones provided by the game.
    Enums are implemented as C# classes with static readonly properties.
  </description>
  
  <enum>
    <n>Grounds</n>
    <description>Types of ground/terrain in the game world</description>
    <members>
      <member>
        <n>Soil</n>
        <value>"soil"</value>
        <description>Regular farmable soil</description>
      </member>
      <member>
        <n>Turf</n>
        <value>"turf"</value>
        <description>Grass-covered ground</description>
      </member>
      <member>
        <n>Grassland</n>
        <value>"grassland"</value>
        <description>Wild grassland terrain</description>
      </member>
    </members>
    <usage_example>
if get_ground_type() == Grounds.Soil:
    print("Standing on soil")
elif get_ground_type() == Grounds.Grassland:
    print("Standing on grassland")
    </usage_example>
  </enum>
  
  <enum>
    <n>Items</n>
    <description>Types of items in the player's inventory</description>
    <members>
      <member>
        <n>Hay</n>
        <value>"hay"</value>
        <description>Harvested hay/grass</description>
      </member>
      <member>
        <n>Wood</n>
        <value>"wood"</value>
        <description>Wood from trees</description>
      </member>
      <member>
        <n>Carrot</n>
        <value>"carrot"</value>
        <description>Harvested carrot</description>
      </member>
      <member>
        <n>Pumpkin</n>
        <value>"pumpkin"</value>
        <description>Harvested pumpkin</description>
      </member>
      <member>
        <n>Power</n>
        <value>"power"</value>
        <description>Power/energy resource</description>
      </member>
      <member>
        <n>Sunflower</n>
        <value>"sunflower"</value>
        <description>Sunflower seed or plant</description>
      </member>
      <member>
        <n>Water</n>
        <value>"water"</value>
        <description>Water resource</description>
      </member>
    </members>
    <usage_example>
if num_items(Items.Hay) >= 20000:
    import Items.Grass
    
use_item(Items.Water)
    </usage_example>
  </enum>
  
  <enum>
    <n>Entities</n>
    <description>Types of plantable entities/crops</description>
    <members>
      <member>
        <n>Grass</n>
        <value>"grass"</value>
        <description>Grass entity</description>
      </member>
      <member>
        <n>Bush</n>
        <value>"bush"</value>
        <description>Bush entity</description>
      </member>
      <member>
        <n>Tree</n>
        <value>"tree"</value>
        <description>Tree entity</description>
      </member>
      <member>
        <n>Carrot</n>
        <value>"carrot"</value>
        <description>Carrot crop</description>
      </member>
      <member>
        <n>Pumpkin</n>
        <value>"pumpkin"</value>
        <description>Pumpkin crop</description>
      </member>
      <member>
        <n>Sunflower</n>
        <value>"sunflower"</value>
        <description>Sunflower crop</description>
      </member>
    </members>
    <usage_example>
plant(Entities.Carrot)
if can_harvest():
    harvest()
    </usage_example>
  </enum>
  
  <!-- TEMPLATE FOR ADDING NEW ENUM TYPE:
  
  <enum>
    <n>EnumName</n>
    <description>Purpose of this enum</description>
    <members>
      <member>
        <n>MemberName</n>
        <value>"internal_value"</value>
        <description>What this member represents</description>
      </member>
      ...more members...
    </members>
    <usage_example>
# Example code using this enum
    </usage_example>
  </enum>
  
  IMPLEMENTATION NOTES:
  1. Create a C# class for each enum in a file like GameEnums.cs
  2. Each enum member is a static readonly property
  3. Example:
     public class Grounds {
         public static readonly string Soil = "soil";
         public static readonly string Turf = "turf";
     }
  4. Register enum in global scope during interpreter initialization
  5. Parse member access (EnumName.MemberName) in Parser
  6. Evaluate member access in Interpreter
  -->
</enum_types>
```

**Implementation Pattern:**

```csharp
// GameEnums.cs - All enum definitions
public static class Grounds {
    public static readonly string Soil = "soil";
    public static readonly string Turf = "turf";
    public static readonly string Grassland = "grassland";
}

public static class Items {
    public static readonly string Hay = "hay";
    public static readonly string Wood = "wood";
    public static readonly string Carrot = "carrot";
    public static readonly string Pumpkin = "pumpkin";
    public static readonly string Power = "power";
    public static readonly string Sunflower = "sunflower";
    public static readonly string Water = "water";
}

public static class Entities {
    public static readonly string Grass = "grass";
    public static readonly string Bush = "bush";
    public static readonly string Tree = "tree";
    public static readonly string Carrot = "carrot";
    public static readonly string Pumpkin = "pumpkin";
    public static readonly string Sunflower = "sunflower";
}
```

**Enum Access Pattern:**

In the interpreter, enum member access must be implemented:

```csharp
// When encountering "Grounds.Soil" in code:
// 1. Parser creates MemberAccessExpr(VariableExpr("Grounds"), "Soil")
// 2. Interpreter evaluates:
//    - Resolve "Grounds" -> Returns Grounds enum object
//    - Access "Soil" member -> Returns "soil" string
// 3. Result: "soil" string value

// For comparison:
if get_ground_type() == Grounds.Soil:
    // Evaluates to: if "soil" == "soil": (both are strings)
```

---

## ═══════════════════════════════════════════════════════════════
## SECTION 2: LEXER & TOKENIZATION
## ═══════════════════════════════════════════════════════════════

### 2.1 Token System (`Token.cs`, `TokenType.cs`)

#### 2.1.1 Token Types (Enum)

**[MODIFY HERE - ADD NEW OPERATORS OR KEYWORDS]**

```xml
<token_types>
  <structural>
    <tokens>INDENT, DEDENT, NEWLINE, EOF</tokens>
    <description>Control indentation-based syntax</description>
  </structural>
  
  <literals>
    <tokens>IDENTIFIER, STRING, NUMBER</tokens>
    <description>Variable names, string literals, numeric literals</description>
  </literals>
  
  <keywords>
    <control_flow>
      IF, ELIF, ELSE, WHILE, FOR, BREAK, CONTINUE, PASS
    </control_flow>
    <function_related>
      DEF, RETURN, LAMBDA
    </function_related>
    <class_related>
      CLASS
    </class_related>
    <scope_related>
      GLOBAL
    </scope_related>
    <logical>
      AND, OR, NOT, IN, IS
    </logical>
    <literals>
      TRUE, FALSE, NONE
    </literals>
    <import_related>
      IMPORT
    </import_related>
  </keywords>
  
  <arithmetic_operators>
    <tokens>PLUS, MINUS, STAR, SLASH, PERCENT, DOUBLE_STAR</tokens>
    <symbols>+, -, *, /, %, **</symbols>
    <description>Basic arithmetic operations including exponentiation</description>
  </arithmetic_operators>
  
  <comparison_operators>
    <tokens>
      EQUAL_EQUAL, BANG_EQUAL, LESS, GREATER, 
      LESS_EQUAL, GREATER_EQUAL
    </tokens>
    <symbols>==, !=, &lt;, &gt;, &lt;=, &gt;=</symbols>
    <description>Comparison and equality operations</description>
  </comparison_operators>
  
  <assignment_operators>
    <tokens>
      EQUAL, PLUS_EQUAL, MINUS_EQUAL, 
      STAR_EQUAL, SLASH_EQUAL
    </tokens>
    <symbols>=, +=, -=, *=, /=</symbols>
    <description>Variable assignment and compound assignment</description>
  </assignment_operators>
  
  <bitwise_operators>
    <tokens>
      AMPERSAND, PIPE, CARET, TILDE, 
      LEFT_SHIFT, RIGHT_SHIFT
    </tokens>
    <symbols>&amp;, |, ^, ~, &lt;&lt;, &gt;&gt;</symbols>
    <description>Bitwise operations on integers</description>
  </bitwise_operators>
  
  <delimiters>
    <tokens>
      LEFT_PAREN, RIGHT_PAREN, LEFT_BRACKET, RIGHT_BRACKET,
      DOT, COMMA, COLON
    </tokens>
    <symbols>(, ), [, ], ., ,, :</symbols>
    <description>Structural delimiters for expressions</description>
  </delimiters>
</token_types>
```

**C# Implementation:**
```csharp
public enum TokenType {
    // Structural
    INDENT, DEDENT, NEWLINE, EOF,
    
    // Literals
    IDENTIFIER, STRING, NUMBER,
    
    // Keywords
    IF, ELIF, ELSE, WHILE, FOR, DEF, RETURN, CLASS,
    BREAK, CONTINUE, PASS, GLOBAL, LAMBDA, IMPORT,
    AND, OR, NOT, IN, IS,
    TRUE, FALSE, NONE,
    
    // Operators (Arithmetic)
    PLUS, MINUS, STAR, SLASH, PERCENT, DOUBLE_STAR,
    
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

**[MODIFY HERE - INDENTATION RULES]**

```xml
<indentation_rules>
  <unit>
    <spaces_per_level>4</spaces_per_level>
    <tabs_allowed>false</tabs_allowed>
    <description>
      Tabs are automatically converted to 4 spaces during input validation.
      Mixing tabs and spaces is not allowed.
    </description>
  </unit>
  
  <validation>
    <rule>All indentation must be multiples of 4 spaces</rule>
    <rule>Dedentation must align with a previous indentation level</rule>
    <rule>Inconsistent indentation throws LexerError</rule>
  </validation>
  
  <examples>
    <valid>
if condition:
    statement1
    if nested:
        statement2
    statement3
    </valid>
    
    <invalid reason="Not multiple of 4">
if condition:
  statement  # Only 2 spaces - ERROR
    </invalid>
  </examples>
</indentation_rules>
```

**C# Implementation:**
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

#### 2.2.3 Comment Handling

**[MODIFY HERE - ADD MULTI-LINE COMMENTS]**

```xml
<comment_syntax>
  <single_line>
    <syntax>//</syntax>
    <description>Extends from // to end of line</description>
    <example>
x = 5  // This is a comment
// This entire line is a comment
    </example>
  </single_line>
  
  <single_line_hash>
    <syntax>#</syntax>
    <description>Python-style single-line comment</description>
    <example>
x = 5  # This is a comment
# This entire line is a comment
    </example>
  </single_line_hash>
</comment_syntax>
```

#### 2.2.4 Number Parsing (Support Integer & Float)

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

#### 2.2.5 String Parsing

```csharp
void ScanString() {
    while (Peek() != '"' && !IsAtEnd()) {
        if (Peek() == '\n') lineNumber++;
        Advance();
    }
    
    if (IsAtEnd()) {
        throw new LexerError($"Line {lineNumber}: Unterminated string");
    }
    
    Advance(); // Closing "
    string value = source.Substring(start + 1, current - start - 2);
    EmitToken(TokenType.STRING, value);
}
```

#### 2.2.6 Operator Scanning (Including \*\*)

```csharp
void ScanOperator() {
    switch (currentChar) {
        case '*':
            // Check for ** (exponentiation)
            if (Peek() == '*') {
                Advance();
                EmitToken(TokenType.DOUBLE_STAR);
            } else {
                EmitToken(TokenType.STAR);
            }
            break;
        // ... other operators
    }
}
```

---

## ═══════════════════════════════════════════════════════════════
## SECTION 3: PARSER & ABSTRACT SYNTAX TREE (AST)
## ═══════════════════════════════════════════════════════════════

### 3.1 AST Node Hierarchy (`AST.cs`)

```csharp
// Base classes
public abstract class ASTNode { }
public abstract class Stmt : ASTNode { }
public abstract class Expr : ASTNode { }

// Statement types
public class ExpressionStmt : Stmt {
    public Expr Expression;
}

public class AssignmentStmt : Stmt {
    public string Target;
    public Expr Value;
    public string Operator; // "=", "+=", "-=", etc.
}

public class IfStmt : Stmt {
    public Expr Condition;
    public List<Stmt> ThenBranch;
    public List<Stmt> ElseBranch; // Can be null
}

public class WhileStmt : Stmt {
    public Expr Condition;
    public List<Stmt> Body;
}

public class ForStmt : Stmt {
    public string Variable;
    public Expr Iterable;
    public List<Stmt> Body;
}

public class FunctionDefStmt : Stmt {
    public string Name;
    public List<string> Parameters;
    public List<Stmt> Body;
}

public class ClassDefStmt : Stmt {
    public string Name;
    public List<FunctionDefStmt> Methods;
}

public class ReturnStmt : Stmt {
    public Expr Value; // Can be null
}

public class BreakStmt : Stmt { }
public class ContinueStmt : Stmt { }
public class PassStmt : Stmt { }

public class GlobalStmt : Stmt {
    public List<string> Variables;
}

public class ImportStmt : Stmt {
    public string EnumName;  // e.g., "Items"
    public string MemberName; // e.g., "Grass" (for import Items.Grass)
}

// Expression types
public class BinaryExpr : Expr {
    public Expr Left;
    public TokenType Operator;
    public Expr Right;
}

public class UnaryExpr : Expr {
    public TokenType Operator;
    public Expr Operand;
}

public class LiteralExpr : Expr {
    public object Value;
}

public class VariableExpr : Expr {
    public string Name;
}

public class CallExpr : Expr {
    public Expr Callee;
    public List<Expr> Arguments;
}

public class IndexExpr : Expr {
    public Expr Object;
    public Expr Index;
}

public class SliceExpr : Expr {
    public Expr Object;
    public Expr Start; // Can be null
    public Expr Stop;  // Can be null
    public Expr Step;  // Can be null
}

public class ListExpr : Expr {
    public List<Expr> Elements;
}

public class DictExpr : Expr {
    public List<Expr> Keys;
    public List<Expr> Values;
}

public class LambdaExpr : Expr {
    public List<string> Parameters;
    public Expr Body;
}

public class ListCompExpr : Expr {
    public Expr Element;
    public string Variable;
    public Expr Iterable;
    public Expr Condition; // Can be null (for if clause)
}

public class MemberAccessExpr : Expr {
    public Expr Object;
    public string Member;
}
```

### 3.2 Parser (`Parser.cs`)

#### 3.2.1 Grammar (Recursive Descent)

```
program        → statement* EOF
statement      → simple_stmt | compound_stmt
simple_stmt    → (expr_stmt | assignment | return_stmt | break_stmt | 
                  continue_stmt | pass_stmt | global_stmt | import_stmt) NEWLINE
compound_stmt  → if_stmt | while_stmt | for_stmt | function_def | class_def

import_stmt    → "import" IDENTIFIER ("." IDENTIFIER)?

expr_stmt      → expression
assignment     → IDENTIFIER (EQUAL | PLUS_EQUAL | ...) expression
return_stmt    → "return" expression?
break_stmt     → "break"
continue_stmt  → "continue"
pass_stmt      → "pass"
global_stmt    → "global" IDENTIFIER ("," IDENTIFIER)*

if_stmt        → "if" expression ":" suite ("elif" expression ":" suite)* 
                 ("else" ":" suite)?
while_stmt     → "while" expression ":" suite
for_stmt       → "for" IDENTIFIER "in" expression ":" suite
function_def   → "def" IDENTIFIER "(" parameters? ")" ":" suite
class_def      → "class" IDENTIFIER ":" suite

suite          → NEWLINE INDENT statement+ DEDENT | simple_stmt
parameters     → IDENTIFIER ("," IDENTIFIER)*

expression     → logical_or
logical_or     → logical_and ("or" logical_and)*
logical_and    → logical_not ("and" logical_not)*
logical_not    → "not" logical_not | comparison
comparison     → bitwise_or (("==" | "!=" | "<" | ">" | "<=" | ">=" | 
                 "in" | "is") bitwise_or)*
bitwise_or     → bitwise_xor ("|" bitwise_xor)*
bitwise_xor    → bitwise_and ("^" bitwise_and)*
bitwise_and    → addition ("&" addition)*
addition       → multiplication (("+" | "-") multiplication)*
multiplication → exponentiation (("*" | "/" | "%") exponentiation)*
exponentiation → unary ("**" unary)*
unary          → ("+" | "-" | "~" | "not") unary | primary
primary        → literal | IDENTIFIER | call | index | slice | 
                 member_access | list | dict | lambda | list_comp | 
                 "(" expression ")"

call           → primary "(" arguments? ")"
index          → primary "[" expression "]"
slice          → primary "[" expression? ":" expression? (":" expression?)? "]"
member_access  → primary "." IDENTIFIER
list           → "[" (expression ("," expression)*)? "]"
dict           → "{" (expression ":" expression ("," expression ":" expression)*)? "}"
lambda         → "lambda" parameters? ":" expression
list_comp      → "[" expression "for" IDENTIFIER "in" expression ("if" expression)? "]"

literal        → NUMBER | STRING | "True" | "False" | "None"
```

### 3.3 Expression Evaluation

**[MODIFY HERE - ADD NEW OPERATORS]**

```csharp
object EvaluateBinaryExpr(BinaryExpr expr) {
    object left = Evaluate(expr.Left);
    object right = Evaluate(expr.Right);
    
    switch (expr.Operator) {
        case TokenType.PLUS:
            // Handle string concatenation
            if (left is string || right is string) {
                return ToString(left) + ToString(right);
            }
            return ToNumber(left) + ToNumber(right);
            
        case TokenType.MINUS:
            return ToNumber(left) - ToNumber(right);
            
        case TokenType.STAR:
            return ToNumber(left) * ToNumber(right);
            
        case TokenType.SLASH:
            double divisor = ToNumber(right);
            if (divisor == 0) throw new RuntimeError("Division by zero");
            return (int)(ToNumber(left) / divisor); // Integer division
            
        case TokenType.PERCENT:
            return ToNumber(left) % ToNumber(right);
            
        case TokenType.DOUBLE_STAR:
            return Math.Pow(ToNumber(left), ToNumber(right));
        
        // Comparison
        case TokenType.EQUAL_EQUAL:
            return IsEqual(left, right);
        case TokenType.BANG_EQUAL:
            return !IsEqual(left, right);
        case TokenType.LESS:
            return ToNumber(left) < ToNumber(right);
        case TokenType.GREATER:
            return ToNumber(left) > ToNumber(right);
        case TokenType.LESS_EQUAL:
            return ToNumber(left) <= ToNumber(right);
        case TokenType.GREATER_EQUAL:
            return ToNumber(left) >= ToNumber(right);
        
        // Logical (with short-circuit)
        case TokenType.AND:
            if (!IsTruthy(left)) return left;  // Short-circuit
            return right;
        case TokenType.OR:
            if (IsTruthy(left)) return left;   // Short-circuit
            return right;
        
        // Bitwise
        case TokenType.AMPERSAND:
            return (int)ToNumber(left) & (int)ToNumber(right);
        case TokenType.PIPE:
            return (int)ToNumber(left) | (int)ToNumber(right);
        case TokenType.CARET:
            return (int)ToNumber(left) ^ (int)ToNumber(right);
        case TokenType.LEFT_SHIFT:
            return (int)ToNumber(left) << (int)ToNumber(right);
        case TokenType.RIGHT_SHIFT:
            return (int)ToNumber(left) >> (int)ToNumber(right);
    }
    
    throw new RuntimeError($"Unknown operator: {expr.Operator}");
}
```

### 3.4 Operator Precedence Table

**[MODIFY HERE - OPERATOR PRECEDENCE]**

```xml
<operator_precedence>
  <instruction>
    Operators are evaluated in this order (highest to lowest priority).
    Use parentheses to override default precedence.
    Parser must build AST respecting this precedence.
  </instruction>
  
  <level priority="1" associativity="left-to-right">
    <n>Grouping</n>
    <operators>( )</operators>
  </level>
  
  <level priority="2" associativity="left-to-right">
    <n>Member Access, Indexing, Calls</n>
    <operators>. [ ] ( )</operators>
  </level>
  
  <level priority="3" associativity="right-to-left">
    <n>Unary</n>
    <operators>+ - ~ not</operators>
  </level>
  
  <level priority="4" associativity="right-to-left">
    <n>Exponentiation</n>
    <operators>**</operators>
    <note>Right-associative: 2**3**4 = 2**(3**4) = 2**81</note>
  </level>
  
  <level priority="5" associativity="left-to-right">
    <n>Multiplicative</n>
    <operators>* / %</operators>
  </level>
  
  <level priority="6" associativity="left-to-right">
    <n>Additive</n>
    <operators>+ -</operators>
  </level>
  
  <level priority="7" associativity="left-to-right">
    <n>Bitwise Shift</n>
    <operators>&lt;&lt; &gt;&gt;</operators>
  </level>
  
  <level priority="8" associativity="left-to-right">
    <n>Bitwise AND</n>
    <operators>&amp;</operators>
  </level>
  
  <level priority="9" associativity="left-to-right">
    <n>Bitwise XOR</n>
    <operators>^</operators>
  </level>
  
  <level priority="10" associativity="left-to-right">
    <n>Bitwise OR</n>
    <operators>|</operators>
  </level>
  
  <level priority="11" associativity="left-to-right">
    <n>Comparison</n>
    <operators>== != &lt; &gt; &lt;= &gt;= in is</operators>
  </level>
  
  <level priority="12" associativity="left-to-right">
    <n>Logical AND</n>
    <operators>and</operators>
  </level>
  
  <level priority="13" associativity="left-to-right">
    <n>Logical OR</n>
    <operators>or</operators>
  </level>
  
  <level priority="14" associativity="right-to-left">
    <n>Lambda</n>
    <operators>lambda</operators>
  </level>
</operator_precedence>
```

---

## ═══════════════════════════════════════════════════════════════
## SECTION 4: GAME BUILT-IN FUNCTIONS, CONSTANTS & RUNTIME
## ═══════════════════════════════════════════════════════════════

### 4.1 Game Built-in Functions (`GameBuiltinMethods.cs`)

**[MODIFY HERE - ADD NEW GAME FUNCTIONS]**

```xml
<game_builtins>
  <description>
    Unity-specific functions based on "Farmer Was Replaced" game.
    Functions that yield (take time) are implemented as IEnumerators.
    Query functions return instantly without yielding.
  </description>
  
  <!-- MOVEMENT FUNCTIONS -->
  
  <function>
    <n>move</n>
    <parameters>direction: constant (North, South, East, West)</parameters>
    <return_type>void</return_type>
    <execution_time>~0.3 seconds (movement animation)</execution_time>
    <yields>true</yields>
    <description>
      Moves player one tile in specified direction.
      Direction can be a built-in constant (North, South, East, West)
      or a string ("up", "down", "left", "right").
    </description>
    <implementation_type>IEnumerator</implementation_type>
    <example>
move(North)  # Move up using constant
move("left") # Move left using string
    </example>
  </function>
  
  <!-- FARMING FUNCTIONS -->
  
  <function>
    <n>harvest</n>
    <parameters>none</parameters>
    <return_type>void</return_type>
    <execution_time>~0.2 seconds depends and customized inside Unity3D C#</execution_time>
    <yields>true</yields>
    <description>
      Harvests the entity at current player position.
      Check can_harvest() first to avoid errors.
    </description>
    <implementation_type>IEnumerator</implementation_type>
    <example>
if can_harvest():
    harvest()
    </example>
  </function>
  
  <function>
    <n>plant</n>
    <parameters>entity: Entities enum</parameters>
    <return_type>void</return_type>
    <execution_time>~0.3 seconds</execution_time>
    <yields>true</yields>
    <description>
      Plants specified entity at current position.
      Entity must be an Entities enum member (e.g., Entities.Carrot).
    </description>
    <implementation_type>IEnumerator</implementation_type>
    <example>
plant(Entities.Carrot)
plant(Entities.Sunflower)
    </example>
  </function>
  
  <function>
    <n>till</n>
    <parameters>none</parameters>
    <return_type>void</return_type>
    <execution_time>~0.1 seconds</execution_time>
    <yields>true</yields>
    <description>
      Tills the ground at current position, converting it to soil.
    </description>
    <implementation_type>IEnumerator</implementation_type>
  </function>
  
  <!-- QUERY FUNCTIONS (Instant - No Yield) -->
  
  <function>
    <n>can_harvest</n>
    <parameters>none</parameters>
    <return_type>bool</return_type>
    <execution_time>Instant</execution_time>
    <yields>false</yields>
    <description>
      Returns True if entity at current position can be harvested.
    </description>
    <implementation_type>Regular method</implementation_type>
    <example>
if can_harvest():
    harvest()
    </example>
  </function>
  
  <function>
    <n>get_ground_type</n>
    <parameters>none</parameters>
    <return_type>Grounds enum (string internally)</return_type>
    <execution_time>Instant</execution_time>
    <yields>false</yields>
    <description>
      Returns the ground type at current position.
      Returns Grounds.Soil, Grounds.Turf, or Grounds.Grassland.
    </description>
    <implementation_type>Regular method</implementation_type>
    <possible_returns>
      Grounds.Soil, Grounds.Turf, Grounds.Grassland
    </possible_returns>
    <example>
if get_ground_type() == Grounds.Soil:
    plant(Entities.Carrot)
elif get_ground_type() == Grounds.Grassland:
    till()
    </example>
  </function>
  
  <function>
    <n>get_entity_type</n>
    <parameters>none</parameters>
    <return_type>Entities enum or None</return_type>
    <execution_time>Instant</execution_time>
    <yields>false</yields>
    <description>
      Returns the entity type at current position, or None if empty.
    </description>
    <implementation_type>Regular method</implementation_type>
  </function>
  
  <function>
    <n>get_pos_x</n>
    <parameters>none</parameters>
    <return_type>int</return_type>
    <execution_time>Instant</execution_time>
    <yields>false</yields>
    <description>Returns current X position of player</description>
    <implementation_type>Regular method</implementation_type>
  </function>
  
  <function>
    <n>get_pos_y</n>
    <parameters>none</parameters>
    <return_type>int</return_type>
    <execution_time>Instant</execution_time>
    <yields>false</yields>
    <description>Returns current Y position of player</description>
    <implementation_type>Regular method</implementation_type>
  </function>
  
  <function>
    <n>get_world_size</n>
    <parameters>none</parameters>
    <return_type>int</return_type>
    <execution_time>Instant</execution_time>
    <yields>false</yields>
    <description>
      Returns the size of the world grid (width and height are same).
    </description>
    <implementation_type>Regular method</implementation_type>
    <example>
for i in range(get_world_size()):
    for j in range(get_world_size()):
        # Process entire grid
    </example>
  </function>
  
  <function>
    <n>get_water</n>
    <parameters>none</parameters>
    <return_type>float</return_type>
    <execution_time>Instant</execution_time>
    <yields>false</yields>
    <description>
      Returns current water level at player position (0.0 to 1.0).
    </description>
    <implementation_type>Regular method</implementation_type>
  </function>
  
  <!-- INVENTORY FUNCTIONS -->
  
  <function>
    <n>num_items</n>
    <parameters>item: Items enum</parameters>
    <return_type>int</return_type>
    <execution_time>Instant</execution_time>
    <yields>false</yields>
    <description>
      Returns quantity of specified item in inventory.
      Item must be an Items enum member (e.g., Items.Carrot).
    </description>
    <implementation_type>Regular method</implementation_type>
    <example>
carrots = num_items(Items.Carrot)
if num_items(Items.Hay) >= 20000:
    # Do something
    </example>
  </function>
  
  <function>
    <n>use_item</n>
    <parameters>item: Items enum</parameters>
    <return_type>void</return_type>
    <execution_time>~0.1 seconds</execution_time>
    <yields>true</yields>
    <description>
      Uses/consumes one unit of specified item from inventory.
      Throws error if item not available.
    </description>
    <implementation_type>IEnumerator</implementation_type>
    <example>
if num_items(Items.Water) > 0:
    use_item(Items.Water)
    </example>
  </function>
  
  <!-- UTILITY FUNCTIONS -->
  
  <function>
    <n>do_a_flip</n>
    <parameters>none</parameters>
    <return_type>void</return_type>
    <execution_time>~1.0 second (animation)</execution_time>
    <yields>true</yields>
    <description>
      Player performs a flip animation. Easter egg function.
    </description>
    <implementation_type>IEnumerator</implementation_type>
  </function>
  
  <function>
    <n>is_even</n>
    <parameters>x: int, y: int</parameters>
    <return_type>bool</return_type>
    <execution_time>Instant depends until animation complete which the speed can be configured inside C#</execution_time>
    <yields>false</yields>
    <description>
      Returns True if (x + y) is even. Helper utility function.
    </description>
    <implementation_type>Regular method</implementation_type>
    <example>
def is_even(x, y):
    return (x + y) % 2 == 0
    </example>
  </function>
  
  <function>
    <n>is_odd</n>
    <parameters>x: int, y: int</parameters>
    <return_type>bool</return_type>
    <execution_time>Instant</execution_time>
    <yields>false</yields>
    <description>
      Returns True if (x + y) is odd. Helper utility function.
    </description>
    <implementation_type>Regular method</implementation_type>
  </function>
</game_builtins>
```

### 4.2 Standard Built-in Functions

```xml
<standard_builtins>
  <function>
    <n>print</n>
    <parameters>*args</parameters>
    <description>
      Prints values to in-game console.
      Automatically converts all arguments to strings and joins with spaces.
    </description>
  </function>
  
  <function>
    <n>sleep</n>
    <parameters>seconds: float</parameters>
    <yields>true</yields>
    <description>
      Pauses script execution for specified duration.
      ALWAYS yields WaitForSeconds, independent of instruction budget.
    </description>
  </function>
  
  <function>
    <n>range</n>
    <parameters>start, stop, step (Python-style)</parameters>
    <return_type>list</return_type>
    <description>
      Generates a list of numbers.
      range(5) → [0, 1, 2, 3, 4]
      range(2, 8) → [2, 3, 4, 5, 6, 7]
      range(0, 10, 2) → [0, 2, 4, 6, 8]
    </description>
  </function>
  
  <function>
    <n>len</n>
    <parameters>obj: list | string | dict</parameters>
    <return_type>int</return_type>
  </function>
  
  <function>
    <n>str</n>
    <parameters>obj: any</parameters>
    <return_type>string</return_type>
  </function>
  
  <function>
    <n>int</n>
    <parameters>value: any</parameters>
    <return_type>int</return_type>
  </function>
  
  <function>
    <n>float</n>
    <parameters>value: any</parameters>
    <return_type>float</return_type>
  </function>
  
  <function>
    <n>abs</n>
    <parameters>x: number</parameters>
    <return_type>number</return_type>
  </function>
  
  <function>
    <n>min</n>
    <parameters>*args or list</parameters>
    <return_type>number</return_type>
  </function>
  
  <function>
    <n>max</n>
    <parameters>*args or list</parameters>
    <return_type>number</return_type>
  </function>
  
  <function>
    <n>sum</n>
    <parameters>list: list</parameters>
    <return_type>number</return_type>
  </function>
  
  <function>
    <n>sorted</n>
    <parameters>list: list, key: function (optional)</parameters>
    <return_type>list</return_type>
  </function>
</standard_builtins>
```

### 4.3 Built-in Constants

**[MODIFY HERE - ADD NEW BUILT-IN CONSTANTS]**

```xml
<built_in_constants>
  <description>
    Predefined constant variables accessible globally.
    These are registered during interpreter initialization.
    Users cannot modify these values.
  </description>
  
  <constant_group name="Directions">
    <description>Cardinal direction constants for movement</description>
    
    <constant>
      <n>North</n>
      <value>"up"</value>
      <description>Upward direction (decreases Y)</description>
      <usage>move(North)</usage>
    </constant>
    
    <constant>
      <n>South</n>
      <value>"down"</value>
      <description>Downward direction (increases Y)</description>
      <usage>move(South)</usage>
    </constant>
    
    <constant>
      <n>East</n>
      <value>"right"</value>
      <description>Rightward direction (increases X)</description>
      <usage>move(East)</usage>
    </constant>
    
    <constant>
      <n>West</n>
      <value>"left"</value>
      <description>Leftward direction (decreases X)</description>
      <usage>move(West)</usage>
    </constant>
  </constant_group>
  
  <example>
# Using directional constants
for i in range(5):
    move(North)
move(East)
for i in range(5):
    move(South)
move(West)
  </example>
  
  <!-- TEMPLATE FOR ADDING NEW CONSTANT:
  
  <constant>
    <n>CONSTANT_NAME</n>
    <value>"string_value" or numeric_value</value>
    <description>What this constant represents</description>
    <usage>Example usage</usage>
  </constant>
  
  IMPLEMENTATION:
  1. In PythonInterpreter initialization:
     globalScope.Set("North", "up");
     globalScope.Set("South", "down");
     globalScope.Set("East", "right");
     globalScope.Set("West", "left");
  
  2. Make constants read-only (optional but recommended):
     - Track constant names in HashSet<string> readOnlyVariables
     - In assignment, check if variable is read-only
     - Throw error if attempting to modify: "Cannot assign to constant 'North'"
  -->
</built_in_constants>
```

---

## ═══════════════════════════════════════════════════════════════
## SECTION 5: TEST SUITE & VALIDATION
## ═══════════════════════════════════════════════════════════════

**[MODIFY HERE - ADD NEW TEST CASES]**

```xml
<test_suite>
  <meta_instruction>
    All generated code MUST pass these test cases.
    Before generating final code:
    1. Review each test case
    2. Validate your implementation handles each scenario
    3. If any test fails, fix the issue before delivering code
    4. btw the execution time for built-in funciton that are coroutine say harvest(), do_flip() depends on animation complete, since later these animation speed are adjusted via slider inside game as you seen in tycoon games, to change speed of entire code.
  </meta_instruction>
  
  <test_category name="Enum Support">
    <test_case id="ENUM-1">
      <description>Enum member access and comparison</description>
      <code>
# Enum member access
if get_ground_type() == Grounds.Soil:
    print("Standing on soil")

# Multiple comparisons
ground = get_ground_type()
if ground == Grounds.Grassland:
    till()
elif ground == Grounds.Soil:
    plant(Entities.Carrot)
      </code>
      <expected_behavior>
        1. Grounds.Soil evaluates to "soil" string
        2. get_ground_type() returns "soil" string
        3. Comparison "soil" == "soil" returns True
        4. Should print "Standing on soil"
      </expected_behavior>
    </test_case>
    
    <test_case id="ENUM-2">
      <description>Using enum in function calls</description>
      <code>
plant(Entities.Carrot)
if can_harvest():
    harvest()

if num_items(Items.Carrot) > 0:
    print("Have carrots!")
      </code>
      <expected_behavior>
        plant() receives "carrot" string from Entities.Carrot
        num_items() receives "carrot" string from Items.Carrot
      </expected_behavior>
    </test_case>
    
    <test_case id="ENUM-3">
      <description>Import statement for enum members</description>
      <code>
import Items.Grass
import Items.Carrot

# After import, can use without Items prefix (if implemented)
# This is OPTIONAL advanced feature
      </code>
      <expected_behavior>
        Import statement is recognized and parsed.
        Implementation is optional (can just parse without error).
      </expected_behavior>
    </test_case>
  </test_category>
  
  <test_category name="Built-in Constants">
    <test_case id="CONST-1">
      <description>Directional constants in movement</description>
      <code>
move(North)
move(East)
move(South)
move(West)
      </code>
      <expected_behavior>
        North evaluates to "up"
        East evaluates to "right"
        South evaluates to "down"
        West evaluates to "left"
        move() receives these string values
      </expected_behavior>
    </test_case>
    
    <test_case id="CONST-2">
      <description>Constants are read-only (optional feature)</description>
      <code>
# This should throw error if read-only enforcement is implemented
# North = "something"  # Error: Cannot assign to constant
      </code>
      <expected_behavior>
        If read-only enforcement implemented: throw error
        If not implemented: allow assignment (less ideal but acceptable)
      </expected_behavior>
    </test_case>
  </test_category>
  
  <test_category name="Farmer Was Replaced - Full Scripts">
    <test_case id="FWR-1">
      <description>Basic farming loop from screenshot</description>
      <code>
while True:
    for i in range(get_world_size()):
        for j in range(get_world_size()):
            if get_ground_type() == Grounds.Soil:
                till()
            if can_harvest():
                harvest()
                plant(Entities.Carrot)
            if get_water() < 0.5:
                use_item(Items.Water)
            move(North)
        move(East)
      </code>
      <expected_behavior>
        Should execute without errors
        All enum accesses work correctly
        All function calls succeed
        Movement and farming logic flows properly
      </expected_behavior>
    </test_case>
    
    <test_case id="FWR-2">
      <description>Priority farming script from screenshot</description>
      <code>
import Grass
import Trees

farming_targets = [
    (Items.Hay, 20000, Grass, Entities.Grass),
    (Items.Wood, 2000, Trees, Entities.Tree),
    (Items.Carrot, 1500, Carrot, Entities.Carrot)
]

if num_items(Items.Hay) >= 20000:
    import Grass
elif num_items(Items.Wood) >= 2000:
    import Trees
elif num_items(Items.Carrot) >= 1500:
    import Carrot
      </code>
      <expected_behavior>
        Tuple creation works
        List of tuples works
        Enum members in tuples work
        Import statements parse without error
      </expected_behavior>
    </test_case>
  </test_category>
  
  <test_category name="Operator Precedence">
    <test_case id="OP-1">
      <description>Exponentiation operator precedence</description>
      <code>
result = 2 ** 3 ** 2  # Should be 2 ** (3 ** 2) = 2 ** 9 = 512
print(result)

result2 = 2 * 3 ** 2  # Should be 2 * (3 ** 2) = 2 * 9 = 18
print(result2)
      </code>
      <expected_output>512, then 18</expected_output>
      <expected_behavior>
        ** is right-associative
        ** has higher precedence than *
      </expected_behavior>
    </test_case>
    
    <test_case id="OP-2">
      <description>Complex expression from earlier</description>
      <code>
a = True
b = 3
c = 5
if a and b & c or 2 == 5 // 2:
    print("PASS")
      </code>
      <expected_result>Should print "PASS"</expected_result>
    </test_case>
  </test_category>
  
  <test_category name="String Concatenation">
    <test_case id="STR-CONCAT-1">
      <description>String concatenation with + operator</description>
      <code>
name = "Player"
greeting = "Hello, " + name
print(greeting)  # "Hello, Player"

# Concatenation in expressions
msg = "Position: " + str(get_pos_x()) + "," + str(get_pos_y())
print(msg)
      </code>
      <expected_output>"Hello, Player" then position string</expected_output>
    </test_case>
  </test_category>
  
  <test_category name="Instruction Budget">
    <test_case id="BUDGET-1">
      <description>Small loop executes instantly</description>
      <code>
sum = 0
for i in range(50):
    sum += i
print("Sum:", sum)
      </code>
      <expected_behavior>
        ~50 operations (under budget)
        Executes in single frame
      </expected_behavior>
    </test_case>
    
    <test_case id="BUDGET-2">
      <description>Large loop time-slices</description>
      <code>
sum = 0
for i in range(500):
    sum += i
print("Sum:", sum)
      </code>
      <expected_behavior>
        ~500 operations (5x budget)
        Yields ~5 times
        Produces correct sum (124750)
      </expected_behavior>
    </test_case>
  </test_category>
  
  <test_category name="List Operations">
    <test_case id="LIST-1">
      <description>Negative indexing</description>
      <code>
items = [10, 20, 30, 40, 50]
print(items[-1])   # 50
print(items[-2])   # 40
      </code>
    </test_case>
    
    <test_case id="LIST-2">
      <description>List slicing</description>
      <code>
nums = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9]
print(nums[2:5])    # [2, 3, 4]
print(nums[:3])     # [0, 1, 2]
print(nums[7:])     # [7, 8, 9]
      </code>
    </test_case>
    
    <test_case id="LIST-3">
      <description>List comprehension</description>
      <code>
nums = [1, 2, 3, 4, 5]
doubled = [x * 2 for x in nums]
print(doubled)  # [2, 4, 6, 8, 10]

evens = [x for x in nums if x % 2 == 0]
print(evens)  # [2, 4]
      </code>
    </test_case>
  </test_category>
  
  <test_category name="Functions">
    <test_case id="FUNC-1">
      <description>Function definition and call</description>
      <code>
def add(a, b):
    return a + b

result = add(5, 3)
print("Result:", result)  # 8
      </code>
    </test_case>
    
    <test_case id="FUNC-2">
      <description>Lambda functions</description>
      <code>
double = lambda x: x * 2
print(double(5))  # 10

add = lambda a, b: a + b
print(add(3, 4))  # 7
      </code>
    </test_case>
  </test_category>
  
  <test_category name="Error Handling">
    <test_case id="ERR-1">
      <description>Index out of range with line number</description>
      <code>
items = [1, 2, 3]
print(items[5])  # Line 2
      </code>
      <expected_error>
        RuntimeError (Line 2): IndexError - list index out of range
      </expected_error>
    </test_case>
    
    <test_case id="ERR-2">
      <description>Undefined variable error</description>
      <code>
x = 5
print(unknown_var)  # Line 2
      </code>
      <expected_error>
        RuntimeError (Line 2): NameError - undefined variable 'unknown_var'
      </expected_error>
    </test_case>
  </test_category>
</test_suite>
```

---

## ═══════════════════════════════════════════════════════════════
## SECTION 6: FILE ORGANIZATION & CODE STRUCTURE
## ═══════════════════════════════════════════════════════════════

### 6.1 File Naming Convention

All C# files must follow these conventions:
* **Classes:** `ClassName.cs` (PascalCase)
* **Regions:** Use `#region` to organize code sections

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
10. **GameEnums.cs** - Enum definitions (Grounds, Items, Entities)
11. **CoroutineRunner.cs** - Safe coroutine execution wrapper
12. **ConsoleManager.cs** - UI console for print() output
13. **DemoScripts.cs** - All test scripts as string constants
14. **Exceptions.cs** - Custom exception classes

### 6.3 Code Quality Requirements

* **Comments:** Every public method must have XML documentation
* **Error Messages:** All errors must include context (line number, variable name)
* **Validation:** All user input must be validated
* **Null Checks:** All object access must check for null
* **.NET 2.0 Compliance:** No `yield return` inside try-catch in IEnumerators

---

## ═══════════════════════════════════════════════════════════════
## SECTION 7: IMPLEMENTATION CHECKLIST
## ═══════════════════════════════════════════════════════════════

### 7.1 Complete Feature Checklist

When generating code, ensure you:

**Core Language Features:**
- ✅ Create separate `.cs` files for each class
- ✅ Use `#region` tags to organize code
- ✅ Add XML documentation for all public methods
- ✅ Implement instruction budget (INSTRUCTIONS_PER_FRAME = 100)
- ✅ Support decimal and integer numbers (store as `double`)
- ✅ Implement `sleep()` to always yield
- ✅ Handle negative list indices: `items[-1]`
- ✅ Support list slicing: `items[1:4]`
- ✅ Implement list comprehensions
- ✅ Implement lambda functions
- ✅ Support classes with `__init__` and `self`
- ✅ Support dictionaries
- ✅ Handle all exceptions with line numbers
- ✅ Reset interpreter state on error
- ✅ Comply with .NET 2.0 restrictions

**NEW - Enum Features:**
- ✅ Implement enum types (Grounds, Items, Entities)
- ✅ Support enum member access (Grounds.Soil)
- ✅ Support enum comparison (== and !=)
- ✅ Register enums in global scope
- ✅ Parse member access expressions (MemberAccessExpr)
- ✅ Evaluate enum members to their string values

**NEW - Built-in Constants:**
- ✅ Implement directional constants (North, South, East, West)
- ✅ Register constants in global scope during initialization
- ✅ (Optional) Make constants read-only

**NEW - Operators:**
- ✅ Implement exponentiation operator (**)
- ✅ Ensure ** is right-associative
- ✅ Implement string concatenation with +
- ✅ Support integer division with //

**NEW - Game Functions:**
- ✅ Implement all Farmer Was Replaced functions from Section 4.1
- ✅ Distinguish between yielding (IEnumerator) and instant (regular) functions
- ✅ Support enum parameters in functions (plant, num_items, etc.)
- ✅ Support constant parameters in functions (move)

**Import Statement:**
- ✅ Parse import statements (import Items.Grass)
- ✅ (Optional) Implement actual import functionality

### 7.2 Error Reporting Template

Every error must follow this format:
```
[ERROR TYPE] (Line X): [Detailed message]
```

Examples:
```
LexerError (Line 5): Indentation mismatch
ParserError (Line 12): Expected ')' after function arguments
RuntimeError (Line 23): IndexError - list index out of range
RuntimeError (Line 45): NameError - undefined variable 'player_pos'
RuntimeError (Line 67): Cannot access member 'Invalid' on enum 'Grounds'
```

---

## ═══════════════════════════════════════════════════════════════
## SECTION 8: FINAL GENERATION COMMAND
## ═══════════════════════════════════════════════════════════════

**GENERATE NOW:**

Create **all** C# files as separate artifacts with:

1. ✅ Proper `#region` organization
2. ✅ XML documentation on public methods
3. ✅ Complete error handling with line numbers
4. ✅ Full instruction budget implementation
5. ✅ All test cases from Section 5 validated
6. ✅ .NET 2.0 compliance
7. ✅ Unity 2020.3+ compatibility
8. ✅ Enum support (Grounds, Items, Entities)
9. ✅ Built-in constants (North, South, East, West)
10. ✅ All Farmer Was Replaced game functions
11. ✅ Exponentiation operator (**)
12. ✅ String concatenation with +
13. 
**Target Unity Version:** 2020.3+  
**Target .NET:** 2.0 Standard  
**Estimated Lines of Code:** 4000-6000 total

---

## APPENDIX: QUICK REFERENCE

### Modification Quick Reference

| **Want to add...** | **Go to section...** |
|--------------------|---------------------|
| New game function | Section 4.1 |
| New enum type | Section 1.2.3 |
| New built-in constant | Section 4.3 |
| New operator | Sections 2.1.1, 3.3, 3.4 |
| New test case | Section 5 |
| Change instruction budget | Section 1.1.2 |

---

**END OF SPECIFICATION - Ready for code generation**
**Generate the code now (covering all edge cases, and .Net 2.0 limitation that i mentioned at the begining) all in seperate files as required**