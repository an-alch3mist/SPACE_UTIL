# Unity Life Cycle - Complete Reference Guide (seek Claude(4.5 sonnet) think)

## prompt
Unity Life Cycle
    - Awake()
    - Enable()
    - Start()
    - FixedUpdate()
    - Update()
    - LateUpdate() -> move camera after everything done, cinemachine movement also made in lateUpdate
    - OnColliderEnter()
    - OnTriggerEnter()
    - animation events
    - animation rig calculation(especially UnityEngine.AnimationRigging), 
        the rig caclulation or rig weight alter should always be done in lateUpdate(), 
        since in past i try to alter .rig.weight inside animationEvent(in animation editor of unity) that reiggers a public method and 
        it didnt alter instantly later when i execute it via 
        Coroutine with a frame delay at start through same animation event(i,e with yield yield return null; at start of IEnumerator) it worked.
    
    - Disable()
    - Destroy()
    - and many more
    
    - OnApplicationQuit() // global for a game
    critique/rewrite/improve as you seem fit, into a UnityLifeCycle.md, with exact flow after a thourough research on Unity Life Cycle. along with Practical Code Examples(where a certain action to be decided, eg: camera movement should be done in lateUpdate to avoid jitter, and rig.rigWeight should be done at end of frame or IEnumerator delay as i mentioned before etc as you seem fit) - Included your exact use case showing what doesn't work vs. what does Visual Reference Chart - Added a quick-reference diagram showing the complete frame loop
    could you also mention about how enabling and disabling a gameobject or certain monoBehaviour script checkbox disabled in inspector, shall impact on certain lifeCycle method, as far im aware Awake() gets called regardless of a gameObject is enabled or not and so on correct ?

## Visual Reference Chart

```
INITIALIZATION PHASE
├─ Awake()           → Called when script instance is loaded (even if disabled)
├─ OnEnable()        → Called when object/component becomes enabled
└─ Start()           → Called before first frame update (only if enabled)

PHYSICS LOOP (Fixed Timestep - Default 0.02s)
├─ FixedUpdate()     → Called at fixed intervals for physics
└─ Internal Physics Update

GAME LOOP (Per Frame)
├─ Update()          → Called once per frame (main logic)
├─ LateUpdate()      → Called after all Updates (cameras, follow systems)
├─ Animation Events  → Triggered during animation playback
└─ Animation Rigging → IK and rig calculations

RENDERING
├─ OnWillRenderObject()
├─ OnPreCull()
├─ OnBecameVisible() / OnBecameInvisible()
├─ OnPreRender()
├─ OnRenderObject()
└─ OnPostRender()

COLLISION/TRIGGER DETECTION
├─ OnCollisionEnter/Stay/Exit()
├─ OnTriggerEnter/Stay/Exit()
├─ OnCollisionEnter2D/Stay2D/Exit2D()
└─ OnTriggerEnter2D/Stay2D/Exit2D()

DEINITIALIZATION PHASE
├─ OnDisable()       → Called when object/component becomes disabled
├─ OnDestroy()       → Called when object is destroyed
└─ OnApplicationQuit() → Global, called before application quits
```

---
## Complete Execution Order

### 1. Initialization Methods

#### `Awake()`
- **When**: Called when the script instance is being loaded
- **Important**: Called even if the GameObject is **inactive** or the script component is **disabled**
- **Use For**: Setting up references between scripts, initializing variables that don't depend on other objects

```csharp
public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    
    void Awake()
    {
        // ✓ WORKS: Called even if GameObject is inactive
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
```

#### `OnEnable()`
- **When**: Called every time the object/component is enabled
- **Important**: Called **after** `Awake()`, can be called multiple times
- **Not Called If**: Component checkbox is disabled in inspector or GameObject is inactive

```csharp
public class PooledObject : MonoBehaviour
{
    void OnEnable()
    {
        // ✓ Perfect for object pooling - reset state when reused
        transform.localScale = Vector3.one;
        GetComponent<Rigidbody>().velocity = Vector3.zero;
    }
}
```

#### `Start()`
- **When**: Called before the first frame update, only if enabled
- **Important**: Called only once, after all `Awake()` calls complete
- **Use For**: Initialization that depends on other objects being set up

```csharp
public class PlayerController : MonoBehaviour
{
    private GameManager gameManager;
    
    void Start()
    {
        // ✓ Safe to reference other objects - all Awake() calls are done
        gameManager = GameManager.Instance;
        gameManager.RegisterPlayer(this);
    }
}
```

---

### 2. Physics Loop

#### `FixedUpdate()`
- **When**: Called at fixed time intervals (default 0.02s = 50 FPS)
- **Use For**: All physics calculations, Rigidbody movements

```csharp
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float moveSpeed = 5f;
    
    void FixedUpdate()
    {
        // ✓ CORRECT: Physics operations in FixedUpdate
        Vector3 movement = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        rb.AddForce(movement * moveSpeed);
    }
    
    void Update()
    {
        // ✗ WRONG: Don't move Rigidbody here - causes jittery movement
        // rb.AddForce(movement * moveSpeed);
    }
}
```

---

### 3. Game Loop (Per Frame)

#### `Update()`
- **When**: Called once per frame
- **Use For**: Main game logic, input handling, non-physics movement

```csharp
public class InputManager : MonoBehaviour
{
    void Update()
    {
        // ✓ CORRECT: Input handling in Update
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }
        
        // ✓ Transform-based movement (non-physics)
        float moveX = Input.GetAxis("Horizontal") * Time.deltaTime * speed;
        transform.Translate(moveX, 0, 0);
    }
}
```

#### `LateUpdate()`
- **When**: Called after all `Update()` methods complete
- **Use For**: Camera following, procedural animations, final adjustments

```csharp
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset;
    
    void LateUpdate()
    {
        // ✓ CORRECT: Camera movement in LateUpdate avoids jitter
        // Target has already moved in Update(), so camera follows smoothly
        transform.position = target.position + offset;
    }
    
    void Update()
    {
        // ✗ WRONG: Camera moves before target, causing one-frame delay jitter
        // transform.position = target.position + offset;
    }
}
```

**Why LateUpdate for Cameras?**
```
Frame Timeline:
1. Update()      → Player moves to position (10, 0, 0)
2. LateUpdate()  → Camera follows to (10, 5, 10)

If camera was in Update():
1. Update()      → Camera moves to old position (9, 5, 9)
2. Update()      → Player moves to new position (10, 0, 0)
3. Next Frame    → Camera catches up, but 1 frame late = JITTER
```

---

### 4. Animation Events & Rigging

#### Animation Events
- **When**: Triggered at specific keyframes in animation timeline
- **Caution**: Execute immediately during animation update

```csharp
public class CharacterAnimator : MonoBehaviour
{
    // Called from Animation Event in Unity's Animation window
    public void OnFootstep()
    {
        // ✓ Instantiate effects immediately
        Instantiate(footstepParticle, transform.position, Quaternion.identity);
        AudioSource.PlayClipAtPoint(footstepSound, transform.position);
    }
}
```

#### Animation Rigging (Your Specific Use Case)

```csharp
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class WeaponRigController : MonoBehaviour
{
    [SerializeField] private Rig weaponRig;
    
    // ✗ DOESN'T WORK: Changing rig weight immediately in animation event
    public void EnableAimingRig_AnimEvent()
    {
        // Called from animation event
        weaponRig.weight = 1f; // ✗ Change doesn't apply this frame!
        // Rig calculations happen AFTER animation events in the frame
    }
    
    // ✓ SOLUTION 1: Use Coroutine with frame delay
    public void EnableAimingRig_AnimEvent()
    {
        StartCoroutine(EnableRigNextFrame());
    }
    
    private IEnumerator EnableRigNextFrame()
    {
        yield return null; // Wait one frame
        weaponRig.weight = 1f; // ✓ Now it works!
    }
    
    // ✓ SOLUTION 2: Set weight in LateUpdate (best practice)
    private bool shouldEnableRig = false;
    
    public void RequestRigEnable_AnimEvent()
    {
        shouldEnableRig = true;
    }
    
    void LateUpdate()
    {
        if (shouldEnableRig)
        {
            // ✓ CORRECT: Rig calculations happen at end of frame
            weaponRig.weight = Mathf.Lerp(weaponRig.weight, 1f, Time.deltaTime * 5f);
            if (weaponRig.weight > 0.99f)
                shouldEnableRig = false;
        }
    }
}
```

**Why This Happens:**
```
Frame Execution Order:
1. Update()
2. Animation State Machine updates
3. Animation Events fire           ← You try to change rig.weight here
4. LateUpdate()
5. Animation Rigging calculations  ← Rig actually calculates here
6. Final IK/Rig solver pass

Solution: Either wait one frame, or change weight in LateUpdate/Coroutine
```

---

### 5. Collision & Trigger Detection

#### Physics Collision
```csharp
public class DamageOnCollision : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        // ✓ Requires Rigidbody + Collider (not trigger)
        if (collision.gameObject.CompareTag("Enemy"))
        {
            collision.gameObject.GetComponent<Health>().TakeDamage(10);
        }
    }
}
```

#### Trigger Detection
```csharp
public class PickupItem : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        // ✓ Requires Collider with "Is Trigger" checked
        if (other.CompareTag("Player"))
        {
            Inventory.Instance.AddItem(itemData);
            Destroy(gameObject);
        }
    }
}
```

**Execution Order:**
```
FixedUpdate() → Physics Simulation → OnCollision/OnTrigger → Next FixedUpdate()
```

---

### 6. Deinitialization

#### `OnDisable()`
- **When**: Called when object/component becomes disabled
- **Use For**: Unsubscribe from events, cleanup listeners

```csharp
public class EventListener : MonoBehaviour
{
    void OnEnable()
    {
        GameEvents.OnPlayerDied += HandlePlayerDeath;
    }
    
    void OnDisable()
    {
        // ✓ CRITICAL: Prevent memory leaks
        GameEvents.OnPlayerDied -= HandlePlayerDeath;
    }
    
    void HandlePlayerDeath() { /* ... */ }
}
```

#### `OnDestroy()`
- **When**: Called when object is destroyed
- **Use For**: Final cleanup, releasing resources

```csharp
public class ResourceManager : MonoBehaviour
{
    private Texture2D dynamicTexture;
    
    void OnDestroy()
    {
        // ✓ Release unmanaged resources
        if (dynamicTexture != null)
            Destroy(dynamicTexture);
    }
}
```

#### `OnApplicationQuit()`
- **When**: Called before application quits (global event)
- **Use For**: Save game state, close connections

```csharp
public class SaveManager : MonoBehaviour
{
    void OnApplicationQuit()
    {
        // ✓ Save before closing
        SaveGame();
        CloseNetworkConnection();
    }
}
```

---

## Enable/Disable Behavior Matrix

| Method | GameObject Inactive | Component Disabled | Both Active |
|--------|-------------------|-------------------|-------------|
| `Awake()` | ✓ **CALLED** | ✓ **CALLED** | ✓ CALLED |
| `OnEnable()` | ✗ Not called | ✗ Not called | ✓ CALLED |
| `Start()` | ✗ Not called | ✗ Not called | ✓ CALLED |
| `Update()` | ✗ Not called | ✗ Not called | ✓ CALLED |
| `FixedUpdate()` | ✗ Not called | ✗ Not called | ✓ CALLED |
| `LateUpdate()` | ✗ Not called | ✗ Not called | ✓ CALLED |
| `OnDisable()` | ✗ Not called* | ✓ **CALLED** | ✗ Not called |
| `OnDestroy()` | ✓ **CALLED** | ✓ **CALLED** | ✓ CALLED |

* OnDisable is called when GameObject becomes inactive if component was previously enabled

### Practical Example

```csharp
public class LifecycleTest : MonoBehaviour
{
    void Awake()
    {
        Debug.Log("Awake - Always called when script loads");
        // ✓ Called even if GameObject is inactive or component is disabled
    }
    
    void OnEnable()
    {
        Debug.Log("OnEnable - Called when enabled");
        // Toggle GameObject or component checkbox to trigger this
    }
    
    void Start()
    {
        Debug.Log("Start - Only if enabled");
        // NOT called if GameObject inactive or component disabled
    }
    
    void OnDisable()
    {
        Debug.Log("OnDisable - Called when disabled");
        // Toggle off to trigger
    }
}
```

**Testing Scenarios:**
```csharp
// Scenario 1: GameObject starts inactive in Inspector
// Result: Awake() called, OnEnable() NOT called, Start() NOT called

// Scenario 2: Component checkbox disabled in Inspector
// Result: Awake() called, OnEnable() NOT called, Start() NOT called

// Scenario 3: GameObject starts active, then disabled via script
void DisableGameObject()
{
    gameObject.SetActive(false);
    // Triggers: OnDisable() → (object disabled)
}

// Scenario 4: Component enabled/disabled via script
void ToggleComponent()
{
    this.enabled = false;  // Triggers OnDisable()
    this.enabled = true;   // Triggers OnEnable()
}
```

---

## Common Mistakes & Solutions

### ❌ Problem: Jittery Camera
```csharp
// WRONG
void Update()
{
    transform.position = player.position + offset; // Camera updates before player!
}
```

### ✅ Solution
```csharp
// CORRECT
void LateUpdate()
{
    transform.position = player.position + offset; // Camera updates after player
}
```

---

### ❌ Problem: Physics Not Working
```csharp
// WRONG
void Update()
{
    rb.AddForce(Vector3.forward * speed); // Inconsistent with physics timestep
}
```

### ✅ Solution
```csharp
// CORRECT
void FixedUpdate()
{
    rb.AddForce(Vector3.forward * speed); // Synced with physics updates
}
```

---

### ❌ Problem: Rig Weight Not Changing (Your Case)
```csharp
// WRONG - Called from Animation Event
public void ChangeRigWeight()
{
    rig.weight = 1f; // Changes too early in frame, gets overridden
}
```

### ✅ Solution
```csharp
// CORRECT - Option 1: Coroutine
public void ChangeRigWeight()
{
    StartCoroutine(ChangeRigWeightNextFrame());
}

private IEnumerator ChangeRigWeightNextFrame()
{
    yield return null; // Wait one frame
    rig.weight = 1f;
}

// CORRECT - Option 2: LateUpdate (Best Practice)
private float targetRigWeight;

public void RequestRigWeightChange(float weight)
{
    targetRigWeight = weight;
}

void LateUpdate()
{
    rig.weight = Mathf.Lerp(rig.weight, targetRigWeight, Time.deltaTime * 5f);
}
```

---

## Quick Reference: When to Use Each Method

| Method | Best For |
|--------|----------|
| `Awake()` | Initialize references, singleton pattern, setup that doesn't depend on other objects |
| `OnEnable()` | Subscribe to events, reset pooled objects, enable systems |
| `Start()` | Initialize after all Awake() calls, safe to reference other objects |
| `FixedUpdate()` | Physics calculations, Rigidbody movement, forces |
| `Update()` | Input handling, game logic, non-physics movement |
| `LateUpdate()` | Camera movement, following systems, animation rigging, final adjustments |
| `OnDisable()` | Unsubscribe from events, pause systems |
| `OnDestroy()` | Cleanup, release resources, save data |
| `OnApplicationQuit()` | Global cleanup, save game state, close connections |

---

## Additional Notes

### Cinemachine
Cinemachine updates in `LateUpdate()` by default, which is why custom camera scripts should also use `LateUpdate()` to avoid conflicts.

### Animation Rigging
All IK and rig calculations happen **after** `LateUpdate()` but **before** rendering. To ensure rig weights apply correctly:
- Set rig weights in `LateUpdate()`
- Or use coroutines with `yield return null` to wait one frame
- Avoid setting rig weights in animation events directly

### Performance Tips
- Minimize work in `Update()` - cache references in `Awake()` or `Start()`
- Use `FixedUpdate()` only for physics - it's called more frequently than `Update()` on high-end systems
- `LateUpdate()` is slightly more expensive - use only when necessary

---

**Last Updated**: Based on Unity 2021.3 LTS and later versions