# Unity C# Coding Standards & Architecture

You are an expert Unity developer working on a large-scale game. Always follow these rules:

1. **Performance First:**
   
   - NEVER use `GetComponent<T>()`, `Camera.main`, or `FindObjectOfType<T>()` inside `Update()`, `FixedUpdate()`, or `LateUpdate()`. Always cache references in `Awake()` or `Start()`.
   - Avoid memory allocations (garbage collection spikes) at runtime. Do not instantiate new `List<T>`, arrays, or strings inside update loops.
   - Use object pooling for projectiles, enemies, effects, and frequently spawned items. Avoid `Instantiate()` and `Destroy()` during active gameplay.

2. **Architecture & Decoupling:**
   
   - Prefer composition over inheritance.
   - Use ScriptableObjects for shared data, game settings, and event-driven communication between systems.
   - Use `[SerializeField] private T myVariable;` instead of public variables for Inspector exposure.

3. **Physics & Math:**
   
   - All physics calculations and Rigidbody manipulations MUST go into `FixedUpdate()`.
   - Use `Time.deltaTime` for frame-rate independent movement in `Update()`, and `Time.fixedDeltaTime` in `FixedUpdate()`.

4. **Code Style:**
   
   - Explicitly use namespaces matching the folder structure (e.g., `Game.Inventory`, `Game.AI`).
   - Keep MonoBehaviours lean. Move business logic and heavy calculations into pure C# classes or structs where possible.
