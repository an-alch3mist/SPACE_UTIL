# LOOP Language Specification v2.1

**Role:** Act as a Principal Unity Architect and Compiler Language Engineer.  
**Project:** "Farmer Was Replaced" Clone For Learning - A Programming Puzzle Game.  
**Goal:** Generate complete, production-ready C# source code for a Coroutine-based Python Interpreter in Unity.

---

## 🆕 NEW IN v2.1: Advanced Lambda Features

This version adds comprehensive lambda support including:
- ✨ Lambda with list comprehensions inside
- ✨ Immediately invoked lambda expressions (IIFE)
- ✨ Tuple/list indexing in lambda expressions
- ✨ Complex nested lambda expressions
- ✨ Lambda with multiple parameters and conditions

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
| Lambda patterns | Section 3.5 - Lambda Expressions | Add patterns to `<lambda_support>` |
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
  9. ✅ Validate lambda expression support (Section 3.5)
  
  **All generated code MUST:**
  - Pass the entire test suite
  - Follow .NET 2.0 constraints (no `yield return` inside try-catch in IEnumerators)
  - Include proper error handling with line numbers
  - Implement instruction budget system
  - Support all features listed in Section 8.1 checklist
  - Support enum member access (e.g., Grounds.Soil)
  - Support built-in constants (e.g., North, South)
  - Support advanced lambda patterns (IIFE, nested comprehensions, tuple indexing)
  
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
  - Every lambda evaluation
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

# Example 2: Lambda with list comprehension - counts operations
nums = [1, 2, 3, 4, 5, 6, 7, 8]
result = (lambda x: [i*i for i in x if i % 2 == 0 and i > 3])(nums)
# This counts as: lambda eval + list comp iterations + filtering

# Example 3: Sleep ALWAYS pauses (independent of budget)
for i in range(50):
    print("iteration:", i)
    if i % 10 == 0:
        sleep(2.0)  # MUST yield WaitForSeconds(2.0)
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
  
  <tuple_type>
    <storage>List&lt;object&gt; (immutable after creation)</storage>
    <description>Immutable ordered collections</description>
    <syntax>(1, 2, 3) or (1, 'a') or ('x',)</syntax>
    <indexing>Supports indexing: tuple[0], tuple[1]</indexing>
    <usage>Often used with lambda: lambda x: x[1]</usage>
  </tuple_type>
  
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
      - Member access via dot notation (Grounds.Soil)
      - Comparison with == and !=
      - Use in if statements and expressions
      - Return values from game functions
    </features>
  </enum_type>
  
  <lambda_type>
    <support>First-class function support</support>
    <features>
      - Can be assigned to variables
      - Can be passed as arguments (e.g., to sorted())
      - Can be immediately invoked (IIFE pattern)
      - Can contain expressions including comprehensions
      - Can access outer scope (closure support)
    </features>
  </lambda_type>
</type_system>
```

#### 1.2.2 Security Constraints (.NET 2.0 Compatibility)

* **No Reflection:** Disable `System.Reflection` access to external assemblies
* **No File I/O:** Block `System.IO` operations
* **No Threading:** Reject `System.Threading` APIs
* **IEnumerator Limitation:** CANNOT use `yield return` inside `try-catch` blocks (Unity 2020.3 .NET 2.0 constraint)
  - Solution: Store yield instructions in variables outside try-catch, then yield after the block

#### 1.2.3 Enum Support

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

public class TupleExpr : Expr {
    public List<Expr> Elements;
}

public class DictExpr : Expr {
    public List<Expr> Keys;
    public List<Expr> Values;
}

public class LambdaExpr : Expr {
    public List<string> Parameters;
    public Expr Body; // Can be any expression, including list comprehension!
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
                 member_access | list | tuple | dict | lambda | list_comp | 
                 "(" expression ")"

call           → primary "(" arguments? ")"
index          → primary "[" expression "]"
slice          → primary "[" expression? ":" expression? (":" expression?)? "]"
member_access  → primary "." IDENTIFIER
list           → "[" (expression ("," expression)*)? "]"
tuple          → "(" expression ("," expression)* ","? ")"
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
    <note>Lambda has lowest precedence, allowing complex expressions in body</note>
  </level>
</operator_precedence>
```

### 3.5 Lambda Expression Support (NEW SECTION)

**[MODIFY HERE - LAMBDA PATTERNS]**

```xml
<lambda_support>
  <description>
    Lambda expressions are first-class citizens in LOOP, supporting:
    - Simple expressions: lambda x: x * 2
    - Multiple parameters: lambda x, y: x + y
    - Complex expressions: lambda x: x[0] + x[1]
    - List comprehensions in body: lambda x: [i*i for i in x if i > 0]
    - Immediately invoked: (lambda x: x * 2)(5)
    - Passed as arguments: sorted(data, key=lambda x: x[1])
    - Closure support: Accessing outer scope variables
  </description>
  
  <pattern name="Simple Lambda">
    <syntax>lambda param: expression</syntax>
    <example>
double = lambda x: x * 2
print(double(5))  # 10
    </example>
  </pattern>
  
  <pattern name="Multi-Parameter Lambda">
    <syntax>lambda param1, param2, ...: expression</syntax>
    <example>
add = lambda a, b: a + b
print(add(3, 4))  # 7

multiply = lambda x, y, z: x * y * z
print(multiply(2, 3, 4))  # 24
    </example>
  </pattern>
  
  <pattern name="Lambda with Indexing">
    <syntax>lambda x: x[index]</syntax>
    <description>
      Lambda can access tuple/list elements by index.
      This is commonly used with sorted() for sorting tuples/lists.
    </description>
    <example>
# Sort tuples by second element
data = [(1, 'b'), (3, 'a'), (2, 'c')]
sorted_data = sorted(data, key=lambda x: x[1])
print(sorted_data)  # [(3, 'a'), (1, 'b'), (2, 'c')]

# Sort by first element (descending)
sorted_desc = sorted(data, key=lambda x: -x[0])
print(sorted_desc)  # [(3, 'a'), (2, 'c'), (1, 'b')]
    </example>
  </pattern>
  
  <pattern name="Lambda with List Comprehension">
    <syntax>lambda x: [expr for i in x if condition]</syntax>
    <description>
      Lambda body can contain a full list comprehension.
      This allows complex filtering and transformation in one expression.
    </description>
    <example>
# Filter and square even numbers greater than 3
nums = [1, 2, 3, 4, 5, 6, 7, 8]
result = (lambda x: [i*i for i in x if i % 2 == 0 and i > 3])(nums)
print(result)  # [16, 36, 64] (squares of 4, 6, 8)

# Filter and transform with complex condition
process = lambda lst: [x * 2 for x in lst if x % 2 == 1 and x > 2]
print(process([1, 2, 3, 4, 5, 6, 7]))  # [6, 10, 14] (3*2, 5*2, 7*2)
    </example>
  </pattern>
  
  <pattern name="Immediately Invoked Lambda (IIFE)">
    <syntax>(lambda params: expression)(arguments)</syntax>
    <description>
      Lambda can be immediately invoked by wrapping in parentheses
      and calling with arguments. This is the IIFE pattern.
    </description>
    <example>
# Simple IIFE
result = (lambda x: x * 2)(5)
print(result)  # 10

# IIFE with list comprehension
result = (lambda x: [i*i for i in x if i % 2 == 0 and i > 3])([1, 2, 3, 4, 5, 6, 7, 8])
print(result)  # [16, 36, 64]

# IIFE with multiple parameters
result = (lambda a, b: a + b)(10, 20)
print(result)  # 30
    </example>
  </pattern>
  
  <pattern name="Lambda in sorted() with key">
    <syntax>sorted(iterable, key=lambda x: expression)</syntax>
    <description>
      Most common use case: sorting by a specific field or computed value.
    </description>
    <example>
# Sort list of tuples by second element
data = [(1, 'b'), (3, 'a'), (2, 'c')]
sorted_data = sorted(data, key=lambda x: x[1])
# Result: [(3, 'a'), (1, 'b'), (2, 'c')]

# Sort list of dicts by value
items = [{"name": "apple", "price": 3}, 
         {"name": "banana", "price": 1},
         {"name": "cherry", "price": 2}]
sorted_items = sorted(items, key=lambda x: x["price"])
# Result: [banana, cherry, apple] (sorted by price)

# Sort with complex key
words = ["apple", "pie", "zoo", "at"]
sorted_words = sorted(words, key=lambda x: (len(x), x))
# Result: ['at', 'pie', 'zoo', 'apple'] (by length, then alphabetically)
    </example>
  </pattern>
  
  <pattern name="Lambda with Multiple Conditions">
    <syntax>lambda x: x[index] if complex_condition else default</syntax>
    <example>
# Lambda with conditional expression
get_grade = lambda score: "A" if score >= 90 else "B" if score >= 80 else "C"
print(get_grade(95))  # "A"
print(get_grade(85))  # "B"

# Lambda with multiple boolean conditions
filter_func = lambda x: x > 0 and x % 2 == 0 and x < 100
nums = [-5, 2, 15, 50, 101, 88]
result = [x for x in nums if filter_func(x)]
# Result: [2, 50, 88]
    </example>
  </pattern>
  
  <pattern name="Nested Lambda (Advanced)">
    <syntax>lambda x: (lambda y: expression)(x)</syntax>
    <description>
      Lambda can return another lambda (currying pattern).
      While supported, this is advanced and rarely used.
    </description>
    <example>
# Currying example
add = lambda x: lambda y: x + y
add_5 = add(5)
print(add_5(3))  # 8

# More practical: function generator
multiplier = lambda factor: lambda x: x * factor
double = multiplier(2)
triple = multiplier(3)
print(double(10))  # 20
print(triple(10))  # 30
    </example>
  </pattern>
  
  <implementation_notes>
    <note>
      Lambda body is stored as an Expr (not Stmt) in AST.
      This means lambda can only contain expressions, not statements.
    </note>
    
    <note>
      When evaluating lambda:
      1. Create new scope (closure) capturing outer variables
      2. Store parameters and body expression
      3. Return LambdaFunction object (callable)
    </note>
    
    <note>
      When calling lambda:
      1. Create new local scope
      2. Bind arguments to parameters
      3. Evaluate body expression in this scope
      4. Return result
    </note>
    
    <note>
      For IIFE pattern (lambda x: ...)(args):
      1. Parser sees CallExpr with LambdaExpr as callee
      2. Evaluate lambda first (creates callable)
      3. Immediately call with provided arguments
    </note>
    
    <note>
      For list comprehension in lambda body:
      1. Lambda body is ListCompExpr
      2. Evaluate comprehension in lambda's scope
      3. Return resulting list
    </note>
  </implementation_notes>
</lambda_support>
```

**C# Implementation Pattern:**

```csharp
// LambdaFunction.cs - Runtime representation
public class LambdaFunction {
    public List<string> Parameters;
    public Expr Body;
    public Scope ClosureScope;  // Captured outer scope
    
    public object Call(PythonInterpreter interpreter, List<object> arguments) {
        // Validate argument count
        if (arguments.Count != Parameters.Count) {
            throw new RuntimeError(
                $"Lambda expects {Parameters.Count} arguments, got {arguments.Count}"
            );
        }
        
        // Create new local scope with closure as parent
        Scope lambdaScope = new Scope(ClosureScope);
        
        // Bind parameters to arguments
        for (int i = 0; i < Parameters.Count; i++) {
            lambdaScope.Set(Parameters[i], arguments[i]);
        }
        
        // Evaluate body expression in this scope
        Scope previousScope = interpreter.currentScope;
        interpreter.currentScope = lambdaScope;
        
        try {
            object result = interpreter.Evaluate(Body);
            return result;
        } finally {
            interpreter.currentScope = previousScope;
        }
    }
}

// In PythonInterpreter.cs
object EvaluateLambda(LambdaExpr expr) {
    return new LambdaFunction {
        Parameters = expr.Parameters,
        Body = expr.Body,
        ClosureScope = currentScope  // Capture current scope
    };
}

object EvaluateCall(CallExpr expr) {
    object callee = Evaluate(expr.Callee);
    List<object> arguments = expr.Arguments.Select(arg => Evaluate(arg)).ToList();
    
    if (callee is LambdaFunction lambda) {
        return lambda.Call(this, arguments);
    }
    
    // Handle other callables (built-in functions, user functions, etc.)
    // ...
}
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
    <execution_time>~0.2 seconds</execution_time>
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
    <execution_time>Instant</execution_time>
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
    <execution_time>based on parameter</execution_time>
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
    <parameters>obj: list | string | dict | tuple</parameters>
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
    <parameters>iterable: list/tuple, key: function (optional), reverse: bool (optional)</parameters>
    <return_type>list</return_type>
    <description>
      Returns a new sorted list (does not modify original).
      
      Parameters:
      - iterable: The list/tuple to sort
      - key: Optional lambda/function to extract comparison key
      - reverse: If True, sort in descending order
    </description>
    <example>
# Simple sorting
nums = [3, 1, 4, 1, 5]
sorted_nums = sorted(nums)  # [1, 1, 3, 4, 5]

# Sort tuples by second element
data = [(1, 'b'), (3, 'a'), (2, 'c')]
sorted_data = sorted(data, key=lambda x: x[1])
# Result: [(3, 'a'), (1, 'b'), (2, 'c')]

# Sort descending
sorted_desc = sorted(nums, reverse=True)  # [5, 4, 3, 1, 1]

# Sort dicts by value
items = [{"name": "apple", "price": 3}, 
         {"name": "banana", "price": 1}]
sorted_items = sorted(items, key=lambda x: x["price"])
    </example>
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
  </meta_instruction>
  
  <test_category name="Advanced Lambda Expressions">
    <test_case id="LAMBDA-ADV-1">
      <description>Lambda with list comprehension inside</description>
      <code>
nums = [1, 2, 3, 4, 5, 6, 7, 8]
result = (lambda x: [i*i for i in x if i % 2 == 0 and i > 3])(nums)
print(result)  # [16, 36, 64]
      </code>
      <expected_output>[16, 36, 64]</expected_output>
      <expected_behavior>
        1. Lambda is defined with parameter x
        2. Body is a list comprehension
        3. Lambda is immediately invoked with nums
        4. Comprehension filters: i % 2 == 0 and i > 3
        5. Filtered values: 4, 6, 8
        6. Squares: 16, 36, 64
      </expected_behavior>
    </test_case>
    
    <test_case id="LAMBDA-ADV-2">
      <description>Sorted with lambda accessing tuple elements</description>
      <code>
data = [(1, 'b'), (3, 'a'), (2, 'c')]
sorted_data = sorted(data, key=lambda x: x[1])
print(sorted_data)  # [(3, 'a'), (1, 'b'), (2, 'c')]
      </code>
      <expected_output>[(3, 'a'), (1, 'b'), (2, 'c')]</expected_output>
      <expected_behavior>
        1. sorted() calls lambda for each tuple
        2. Lambda extracts second element (x[1])
        3. Sorting by: 'a', 'b', 'c'
        4. Result ordered alphabetically by second element
      </expected_behavior>
    </test_case>
    
    <test_case id="LAMBDA-ADV-3">
      <description>Lambda with multiple conditions</description>
      <code>
nums = [-5, 2, 15, 50, 101, 88]
filter_func = lambda x: x > 0 and x % 2 == 0 and x < 100
result = [x for x in nums if filter_func(x)]
print(result)  # [2, 50, 88]
      </code>
      <expected_output>[2, 50, 88]</expected_output>
      <expected_behavior>
        Lambda evaluates three conditions:
        1. x > 0 (positive)
        2. x % 2 == 0 (even)
        3. x < 100 (less than 100)
        All three must be true
      </expected_behavior>
    </test_case>
    
    <test_case id="LAMBDA-ADV-4">
      <description>Complex IIFE with nested comprehension</description>
      <code>
# Immediately invoked lambda with complex logic
result = (lambda nums: [
    x * 2 for x in nums 
    if x % 2 == 1 and x > 2
])([1, 2, 3, 4, 5, 6, 7, 8, 9])

print(result)  # [6, 10, 14, 18]
      </code>
      <expected_output>[6, 10, 14, 18]</expected_output>
      <expected_behavior>
        Filters odd numbers > 2: [3, 5, 7, 9]
        Doubles each: [6, 10, 14, 18]
      </expected_behavior>
    </test_case>
    
    <test_case id="LAMBDA-ADV-5">
      <description>Lambda sorting dict list by computed value</description>
      <code>
items = [
    {"name": "apple", "price": 3, "qty": 10},
    {"name": "banana", "price": 1, "qty": 20},
    {"name": "cherry", "price": 2, "qty": 5}
]

# Sort by total value (price * qty)
sorted_items = sorted(items, key=lambda x: x["price"] * x["qty"])

for item in sorted_items:
    print(item["name"])
# Output: cherry, banana, apple
      </code>
      <expected_output>cherry, banana, apple (on separate lines)</expected_output>
      <expected_behavior>
        Computed values:
        - apple: 3 * 10 = 30
        - banana: 1 * 20 = 20
        - cherry: 2 * 5 = 10
        Sorted by value: 10, 20, 30
      </expected_behavior>
    </test_case>
    
    <test_case id="LAMBDA-ADV-6">
      <description>Lambda with nested indexing</description>
      <code>
# List of lists (2D array)
matrix = [[1, 2, 3], [4, 5, 6], [7, 8, 9]]

# Sort rows by their middle element
sorted_matrix = sorted(matrix, key=lambda row: row[1])

for row in sorted_matrix:
    print(row)
# [[1, 2, 3], [4, 5, 6], [7, 8, 9]]
      </code>
      <expected_output>
        [1, 2, 3]
        [4, 5, 6]
        [7, 8, 9]
      </expected_output>
      <expected_behavior>
        Lambda accesses middle element (index 1) of each row
        Middle elements: 2, 5, 8
        Already sorted, so order unchanged
      </expected_behavior>
    </test_case>
    
    <test_case id="LAMBDA-ADV-7">
      <description>Multiple lambda parameters with tuple unpacking</description>
      <code>
# Lambda with multiple params
combine = lambda a, b, c: a + b * c
result = combine(10, 5, 2)
print(result)  # 20

# Lambda returning tuple  
make_pair = lambda x, y: (x, y)
pair = make_pair(5, 10)
print(pair)  # (5, 10)
      </code>
      <expected_output>20, then (5, 10)</expected_output>
    </test_case>
    
    <test_case id="LAMBDA-ADV-8">
      <description>Lambda with descending sort</description>
      <code>
data = [(1, 'b'), (3, 'a'), (2, 'c')]
sorted_data = sorted(data, key=lambda x: x[0], reverse=True)
print(sorted_data)  # [(3, 'a'), (2, 'c'), (1, 'b')]
      </code>
      <expected_output>[(3, 'a'), (2, 'c'), (1, 'b')]</expected_output>
      <expected_behavior>
        Sorts by first element (x[0]) in descending order
      </expected_behavior>
    </test_case>
  </test_category>
  
  <test_category name="Tuple Support">
    <test_case id="TUPLE-1">
      <description>Tuple creation and indexing</description>
      <code>
t = (1, 'a', 3.14)
print(t[0])  # 1
print(t[1])  # 'a'
print(t[2])  # 3.14
print(t[-1])  # 3.14
      </code>
      <expected_output>1, 'a', 3.14, 3.14</expected_output>
    </test_case>
    
    <test_case id="TUPLE-2">
      <description>Tuple in list, iteration</description>
      <code>
data = [(1, 2), (3, 4), (5, 6)]
for pair in data:
    print(pair[0] + pair[1])
# Output: 3, 7, 11
      </code>
      <expected_output>3, 7, 11 (on separate lines)</expected_output>
    </test_case>
    
    <test_case id="TUPLE-3">
      <description>Single element tuple</description>
      <code>
t = (42,)  # Note the comma
print(len(t))  # 1
print(t[0])    # 42
      </code>
      <expected_output>1, then 42</expected_output>
    </test_case>
  </test_category>
  
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
        Grounds.Soil evaluates to "soil"
        Comparison works correctly
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
        Constants evaluate to direction strings
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
      <description>Lambda with wrong argument count</description>
      <code>
add = lambda a, b: a + b
result = add(5)  # Missing second argument
      </code>
      <expected_error>
        RuntimeError: Lambda expects 2 arguments, got 1
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
8. **LambdaFunction.cs** - Runtime lambda representation
9. **ClassInstance.cs** - Runtime class instance representation
10. **GameBuiltinMethods.cs** - Unity-specific game commands
11. **GameEnums.cs** - Enum definitions (Grounds, Items, Entities)
12. **CoroutineRunner.cs** - Safe coroutine execution wrapper
13. **ConsoleManager.cs** - UI console for print() output
14. **DemoScripts.cs** - All test scripts as string constants
15. **Exceptions.cs** - Custom exception classes

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
- ✅ Support tuples: `(1, 2, 3)`
- ✅ Support classes with `__init__` and `self`
- ✅ Support dictionaries
- ✅ Handle all exceptions with line numbers
- ✅ Reset interpreter state on error
- ✅ Comply with .NET 2.0 restrictions

**Lambda Features (NEW IN v2.1):**
- ✅ Basic lambda: `lambda x: x * 2`
- ✅ Multi-parameter lambda: `lambda a, b: a + b`
- ✅ Lambda with indexing: `lambda x: x[1]`
- ✅ Lambda with list comprehension: `lambda x: [i*i for i in x if i > 0]`
- ✅ Immediately invoked lambda (IIFE): `(lambda x: x * 2)(5)`
- ✅ Lambda in sorted(): `sorted(data, key=lambda x: x[1])`
- ✅ Lambda with multiple conditions
- ✅ Lambda with nested indexing
- ✅ Closure support (capturing outer scope)

**Enum Features:**
- ✅ Implement enum types (Grounds, Items, Entities)
- ✅ Support enum member access (Grounds.Soil)
- ✅ Support enum comparison (== and !=)
- ✅ Register enums in global scope

**Built-in Constants:**
- ✅ Implement directional constants (North, South, East, West)
- ✅ Register constants in global scope

**Operators:**
- ✅ Implement exponentiation operator (**)
- ✅ Ensure ** is right-associative
- ✅ Implement string concatenation with +
- ✅ Support integer division with //

**Game Functions:**
- ✅ Implement all Farmer Was Replaced functions
- ✅ Distinguish yielding (IEnumerator) vs instant functions
- ✅ Support enum parameters in functions

### 7.2 Error Reporting Template

Every error must follow this format:
```
[ERROR TYPE] (Line X): [Detailed message]
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
13. ✅ **Advanced lambda support (all patterns in Section 3.5)**
14. ✅ **Tuple support**
15. ✅ **LambdaFunction.cs** with closure support

**Target Unity Version:** 2020.3+  
**Target .NET:** 2.0 Standard

---

## APPENDIX: QUICK REFERENCE

### Modification Quick Reference

| **Want to add...** | **Go to section...** |
|--------------------|---------------------|
| New game function | Section 4.1 |
| New enum type | Section 1.2.3 |
| New built-in constant | Section 4.3 |
| New lambda pattern | Section 3.5 |
| New operator | Sections 2.1.1, 3.3, 3.4 |
| New test case | Section 5 |

---

**END OF SPECIFICATION v2.1 - Ready for code generation**
**Generate the code now (covering all edge cases, and .Net 2.0 limitation that i mentioned at the begining) all in seperate files as required**