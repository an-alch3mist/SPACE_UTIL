# C# to .cs.md Documentation Generation Prompt

## Task
Convert a provided C# files (`*.cs`) into a documentation file (`*.cs.md`) that preserves architectural structure while removing implementation complexity.

## Conversion Rules

### PRESERVE (Keep Method Bodies):
1. **Unity Lifecycle Methods** - Show initialization patterns
   - `Awake()`, `Start()`, `Update()`, `FixedUpdate()`
   - `OnEnable()`, `OnDisable()`, `OnDestroy()`
   - Collision/Trigger: `OnCollisionEnter()`, `OnTriggerEnter()`, etc.
   - UI Events: `OnPointerEnter()`, `OnPointerClick()`, etc.

2. **Property Implementations** - Show backing field patterns
   - Auto-properties: `{ get; set; }`, `{ get; private set; }`
   - Simple computed properties (1-2 lines)
   - Property setters with validation

3. **Constructors & Initialization** - Show setup requirements
   - All constructors with parameter assignments
   - Static constructors
   - Field initializers

4. **Event Handlers** - Show response patterns
   - Event subscription/unsubscription patterns
   - Simple event handler implementations

5. **Short Utility Methods** - Keep if ≤ 3 lines
   - Simple getters/setters
   - Basic calculations
   - One-line conversions

### REMOVE (Replace with Semicolon):
1. **Complex Business Logic** - Methods with > 3 lines of logic
2. **Large Calculation Functions** - Mathematical algorithms
3. **Long Procedural Methods** - Step-by-step processes
4. **Private Implementation Details** - Complex internal logic
5. **Coroutines** - Keep signature, remove `yield return` implementations
6. **Switch Statements** - Large switch blocks
7. **Loop Bodies** - Keep loop structure, remove complex body content

### DOCUMENTATION PRESERVATION:
- **ALWAYS KEEP**: All `<summary>`, `<param>`, `<returns>` XML documentation
- **ALWAYS KEEP**: All `/// <summary>` comment blocks
- **ALWAYS KEEP**: Region markers `#region` and `#endregion`
- **ALWAYS KEEP**: Public/private/protected access modifiers
- **ALWAYS KEEP**: All using statements and namespace declarations
- **ALWAYS KEEP**: Class inheritance and interface implementations

## Output Format

```csharp
// Original file header comments (if any)
using System;
using System.Collections;
// ... all original using statements

namespace OriginalNamespace
{
    /// <summary>
    /// Original class summary documentation
    /// </summary>
    public class ComplexSample : MonoBehaviour, IInterface
    {
        #region FIELDS
        [SerializeField] private float someField;
        private bool isInitialized;
        #endregion

        #region PROPERTIES
        /// <summary>
        /// Property documentation preserved
        /// </summary>
        public bool IsActive { get; private set; }
        
        public float Timer 
        { 
            get => timer; 
            private set => timer = Mathf.Max(0, value); 
        }
        #endregion

        #region UNITY LIFECYCLE
        /// <summary>
        /// Awake documentation preserved
        /// </summary>
        void Awake()
        {
            // Initialize components and variables
            // Setup initial state
            isInitialized = true;
        }

        void Update()
        {
            // Update timer and process input
            // Handle frame-based logic
        }
        #endregion

        #region PUBLIC METHODS
        /// <summary>
        /// Public method documentation preserved
        /// </summary>
        public void ComplexBusinessLogic(int parameter); // Body removed - complex logic

        /// <summary>
        /// Simple utility kept
        /// </summary>
        public bool IsValidInput(string input) => !string.IsNullOrEmpty(input);
        #endregion

        #region PRIVATE METHODS
        /// <summary>
        /// Private method documentation preserved
        /// </summary>
        private void ComplexCalculation(); // Body removed
        
        private void OnSomeEvent(EventArgs args)
        {
            // Event handling pattern shown
            if (args != null) ProcessEvent(args);
        }
        #endregion

        #region EVENT HANDLERS
        void OnCollisionEnter(Collision collision)
        {
            // Collision response pattern
            if (collision.gameObject.CompareTag("Player"))
                TriggerInteraction();
        }
        #endregion
    }
}
```

## Quality Checklist

Before outputting the `.cs.md` file, verify:

- [ ] All `<summary>` documentation blocks are preserved
- [ ] Unity lifecycle methods show their initialization/update patterns
- [ ] Properties show their backing field patterns
- [ ] Complex method bodies are replaced with `;`
- [ ] Simple utility methods (≤3 lines) are kept intact
- [ ] All using statements and namespace are preserved
- [ ] Class inheritance and interface implementations are maintained
- [ ] Region organization is preserved
- [ ] Access modifiers are maintained
- [ ] Event subscription patterns are visible in lifecycle methods

## Example Input Processing

**Input Method:**
```csharp
public void ProcessComplexUserInput(InputData data)
{
    if (data == null || !data.IsValid) return;
    
    var normalizedInput = NormalizeInput(data);
    var validationResult = ValidateInput(normalizedInput);
    
    if (validationResult.HasErrors)
    {
        ShowErrorMessage(validationResult.Errors);
        return;
    }
    
    ExecuteUserAction(normalizedInput);
    UpdateUIState();
    LogUserAction(data.ActionType);
}
```

**Output Method:**
```csharp
/// <summary>
/// Processes complex user input with validation and execution
/// </summary>
/// <param name="data">Input data to process</param>
public void ProcessComplexUserInput(InputData data);
```

**Input Simple Method:**
```csharp
public bool IsInputValid(string input) => !string.IsNullOrEmpty(input) && input.Length > 2;
```

**Output Simple Method (KEPT):**
```csharp
/// <summary>
/// Validates input string length and content
/// </summary>
public bool IsInputValid(string input) => !string.IsNullOrEmpty(input) && input.Length > 2;
```

## Instructions

1. Analyze the provided C# file structure
2. Identify which methods fall into PRESERVE vs REMOVE categories
3. Generate the `.cs.md` file following the format above
4. Ensure all architectural information is maintained
5. Verify documentation completeness using the quality checklist

**Output only the `.cs.md` file content without additional explanation unless errors are found.**

**End every generated `.cs.md` file with:**
```markdown
---
`checksum: claude-4 2025-sept-20 v0.8.5.min`
```

<!-- 
  { } -> ; 

  get/set -> preserve
  Unity3D LifeCycle -> preserve
  Event -> Preserve
  Interface -> Preserve
  Enum -> Preserve
  Constructor -> Preserve
  simple method oneliner -> preserve
  <summary> </summary>  -> preserve in-between
-->