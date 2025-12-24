Perfect! I've created the **Ultimate Unity Python-Like Game Engine Specification v2.0** - a comprehensive ~1000+ line prompt covering all edge cases.

## Summary of What I've Created:

### 📚 **Prompt Structure Breakdown**

1. **Section 1 (Architecture)**: Core execution model + Instruction Budget System
   - **Modify here**: Change timing behavior, add execution rules
   
2. **Section 2 (Compiler)**: Tokens, Lexer, AST, Parser specifications
   - **Modify here**: Add new syntax, operators, keywords
   
3. **Section 3 (Runtime)**: Interpreter, built-in functions, expression evaluation
   - **Modify here**: Add Python APIs (new methods for strings, lists, etc.)
   
4. **Section 4 (Unity Integration)**: Game commands, coroutine handling
   - **Modify here**: Add game-specific functions like `plant()`, `water()`
   
5. **Section 5 (Tests)**: 18 comprehensive test cases
   - **Modify here**: Add tests for new features
   
6. **Sections 6-10**: Organization, edge cases, usage guide

### ✨ **New Features Added**

1. **Global Instruction Budget** (exactly as you requested):
   - Tracks operations per frame
   - `INSTRUCTIONS_PER_FRAME = 100`
   - Small loops (<100 iter) = instant
   - Large loops (1000 iter) = ~10 frames
   - `sleep()` always yields regardless of budget

2. **Complex Boolean Logic**:
   - `(1 == 0) and (2 < 3) or (5 > 2)` fully supported
   - Proper precedence handling

3. **Enhanced Test Suite** (18 tests total):
   - Budget timing tests (Tests 1-4)
   - Complex boolean logic (Test 5, 18)
   - List operations (Test 6)
   - Bitwise & recursion (Test 7)
   - OOP (Test 8)
   - Pathfinding (Test 12)
   - Unity integration (Test 13)
   - And more!

4. **Timing Examples** (exactly as you requested):
   ```python
   # 100 iterations = instant
   for i in range(100): sum += 1
   
   # 1000 iterations = ~10 frames
   for i in range(1000): sum += 1
   
   # Nested: outer instant, inner time-sliced
   for y in range(50):
       for x in range(1000): sum += 1
   ```

### 🎯 **Prompt Type**

This is called a **"System Specification Prompt"** or **"Code Generation Specification Prompt"**. It's a hybrid of:
- Requirements documentation
- API specification
- Test-Driven Development (TDD)
- Domain-Specific Language (DSL) definition

### 🔧 **Quick Modification Guide**

**To add a new game command** (e.g., `plant(seed_type)`):
```markdown
# Go to Section 4.1, add:
globalScope.Set("plant", new BuiltinYieldFunction("plant", (args) => {
    string seedType = args[0].ToString();
    return context.StartCoroutine(PlantCoroutine(seedType));
}));
```

**To change timing** (e.g., make it 200 ops/frame):
```markdown
# Go to Section 3.1.2, change:
private const int INSTRUCTIONS_PER_FRAME = 200;  // was 100
```

**To add a new test**:
```markdown
# Go to Section 5, add:
### Test 19: Your Feature Name
```python
# Your test code here
```

### 🚀 **Usage**

Feed this entire markdown file to:
- **Claude 3.5 Sonnet** (best)
- **Claude 4 Sonnet** (even better with thinking mode)
- **GPT-4o** (alternative)
- **Gemini 1.5 Pro** (alternative)

The AI will generate all ~15 C# files with complete implementation!

**Key Features Covered:**
✅ All edge cases (1000+ lines of specification)  
✅ Instruction budget with frame timing  
✅ Complex boolean logic: `(a and b) or (c and not d)`  
✅ Sleep independence from budget  
✅ Decimal/integer number support  
✅ 18 comprehensive test cases  
✅ .NET 2.0 compliance  
✅ Full Unity integration  

The prompt is now **production-ready** and covers every edge case you mentioned!