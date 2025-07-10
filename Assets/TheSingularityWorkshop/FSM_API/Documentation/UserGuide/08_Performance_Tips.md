08. Performance Tips for FSM_API

Optimizing your FSM_API usage is paramount for building high-performance games and applications, especially as your project scales. This guide covers essential strategies to keep your Finite State Machines fast and efficient, balancing responsiveness with CPU cost.

Choosing the Right Update Frequency (processRate)

The processRate parameter in FSM_API.CreateFiniteStateMachine() is your primary tool for controlling how often an FSM's onUpdate actions and transition conditions are evaluated. Selecting the appropriate frequency for each FSM definition is critical for performance.

    Every Tick (processRate: -1)

        Pros: Maximum responsiveness, immediate reaction to changes in your game state.

        Cons: Highest CPU cost, as the FSM will be processed every single time its ProcessingGroup is updated.

        Use for: Core gameplay elements requiring constant monitoring, such as player input, critical real-time AI (e.g., enemy targeting), or essential UI elements that update constantly.

    Event-Driven (processRate: 0)

        Pros: Zero CPU consumption when idle. The FSM only processes its onUpdate logic and checks transitions when explicitly told to do so via FSMHandle.RequestTransition() or when TransitionTo() is called. Its internal onUpdate will not run unless manually stepped.

        Cons: Requires manual triggering of updates or transitions from external code.

        Use for: UI panels that only change on user interaction, turn-based game logic, non-visual backend systems, or any FSM with rare, explicit state changes.

    Nth Tick (processRate: >0)

        Pros: Significantly reduces update frequency, leading to substantial CPU savings, especially with many FSM instances. An FSM with processRate: N will update approximately every (N + 1) ticks. For example, processRate: 1 means it updates every 2nd tick, and processRate: 5 means it updates every 6th tick.

        Cons: Introduces a slight, controlled delay in responsiveness proportional to N.

        Use for: Background AI (e.g., ambient wildlife, distant NPCs), non-critical AI behaviors, periodic data checks (e.g., checking for resource availability), or any system where a fractional delay is acceptable.

Example: Defining FSMs with Various Update Frequencies
C#
```csharp
using TheSingularityWorkshop.FSM.API;
using UnityEngine; // For MonoBehaviour context

public class FSMExamples
{
    public static void DefineVariousFSMs()
    {
        // Player FSM: Needs immediate response, updates every tick.
        FSM_API.CreateFiniteStateMachine("PlayerInputFSM",
                                         processRate: -1, // Update every single tick when its group is updated
                                         processingGroup: "InputLogic")
            .State("Idle", onUpdate: (ctx) => { /* Check for input continuously */ })
            // ... other states and transitions
            .BuildDefinition();

        // Door FSM: Only updates when activated by a player or event (event-driven).
        FSM_API.CreateFiniteStateMachine("DoorStateFSM",
                                         processRate: 0, // Event-driven
                                         processingGroup: "Environment")
            .State("Closed", onEnter: (ctx) => { /* Play closed animation */ })
            .State("Opening", onUpdate: (ctx) => { /* Animate opening */ })
            .State("Open", onEnter: (ctx) => { /* Door is fully open */ })
            // ... transitions
            .BuildDefinition();

        // Bird Flock FSM: Can afford less frequent updates to save CPU.
        FSM_API.CreateFiniteStateMachine("BirdFlockAI_FSM",
                                         processRate: 10, // Skip 10 ticks, process on 11th
                                         processingGroup: "BackgroundAI")
            .State("Flying", onUpdate: (ctx) => { /* Update bird positions slowly */ })
            .State("Perched", onUpdate: (ctx) => { /* Occasional checks for nearby food */ })
            // ... transitions
            .BuildDefinition();
    }
}
```
Efficient Context Implementation

Your IStateContext implementation serves as the data backbone for your FSM. The efficiency of its properties and methods is paramount, as they are frequently accessed by the FSM's internal logic and your state actions.

    IsValid Property: Make it Lightning Fast

        The IsValid property is checked by FSM_API for every active FSM instance, every time its processing group is updated. It is a critical hot-path.

        Avoid: Complex logic, physics queries, expensive allocations, or iterating over collections within IsValid.

        Ideal: For MonoBehaviour contexts, this != null && gameObject.activeInHierarchy is highly optimized. For pure C# contexts, a simple boolean flag or a check against a disposed state is best.

    Context Data: Cache and Pre-calculate

        Store pre-calculated values or cache references to frequently accessed components/data within your context.

        Avoid performing expensive lookups (like GameObject.Find(), GetComponent<T>(), or complex calculations) repeatedly in onUpdate or condition callbacks. Instead, perform these once in Awake(), Start(), or onEnter() and store the result in a private field of your context.

        Pass necessary data into the context's constructor when creating pure C# contexts to ensure they are fully initialized and self-contained.

Optimizing State Actions and Transitions

The code you write within onEnter, onUpdate, onExit, and condition callbacks runs frequently and directly impacts performance. Treat these methods as hot-path code and optimize them rigorously.

    Avoid Allocations (Minimize GC Pressure):

        Minimize the use of the new keyword within these methods. Frequent object creation leads to garbage collection (GC) pauses, which can cause frame rate hitches.

        Avoid creating new collections (e.g., new List<T>(), new Dictionary<T, T>()) or implicitly creating new objects (e.g., by boxing value types).

        If you need temporary objects or collections, explore object pooling to reuse existing instances rather than creating and destroying new ones.

        Reduce string operations, especially string concatenation with +, as these create new string objects (garbage). Use StringBuilder for complex string building or string interpolation for simple cases where it's optimized by the compiler.

    Minimize Expensive Computations:

        Operations like physics queries (e.g., Raycast, OverlapSphere), complex pathfinding, or iterating over large collections are costly.

        If a computation is expensive but doesn't need to be run every time onUpdate is called, consider:

            Performing it only in onEnter() and caching the result.

            Implementing a timer or counter within onUpdate to run the expensive check only periodically (e.g., if (Time.frameCount % 10 == 0) for an every-10-frame check). While processRate handles overall FSM throttling, a per-method timer can further optimize specific heavy computations within an already active FSM.

    Cache References:

        Never call GameObject.Find(), FindObjectOfType(), or GetComponent<T>() in onUpdate or condition methods. These are extremely slow.

        Instead, cache all necessary component and object references in your Awake(), Start(), or onEnter() methods.

Leveraging Processing Groups for Extreme Optimization

FSM_API.Update(string groupName) is your most powerful tool for granular control over FSM performance. It allows you to define exactly when and how FSMs are processed. This enables advanced orchestration and significant performance gains.

    Granular Control is Absolute:

        An FSM will only process its onUpdate logic and check transitions if its assigned ProcessingGroup is explicitly updated by FSM_API.Update(groupName). If FSM_API.Update("BackgroundAI") is never called, any FSMs in that group, regardless of their processRate, will never run.

        This gives you the power to completely stop or resume FSM processing for entire categories of game logic.

        Default Group: Remember that FSM_API.Update() (without a groupName argument) implicitly updates the default "Update" processing group. FSMs defined without specifying a processingGroup will belong to this default group.

    Throttling External to FSM:

        Beyond the processRate defined in the FSM blueprint, you can apply an additional layer of throttling by controlling how often you call FSM_API.Update() for a given group in your main game loop.

        Example: For a "VeryBackgroundAI" group, instead of calling FSM_API.Update("VeryBackgroundAI") every frame, you could call it only every 30 frames (or even less frequently) in your MonoBehaviour.Update() method. This compounds the processRate savings, leading to immense performance gains for non-critical systems.
    C#
```csharp
    using TheSingularityWorkshop.FSM.API;
    using UnityEngine;

    public class AdvancedFSMUpdater : MonoBehaviour
    {
        private int _frameCount = 0;

        void Update()
        {
            // Always update critical gameplay logic
            FSM_API.Update("PlayerInput");
            FSM_API.Update("EnemyCombat");

            // Update less critical AI every 5 frames
            if (_frameCount % 5 == 0)
            {
                FSM_API.Update("BackgroundAI");
            }

            // Only update UI when a menu is explicitly active
            if (GameManager.IsMenuOpen) // Assuming GameManager manages game state
            {
                FSM_API.Update("UIMenus");
            }
            else
            {
                // Ensure UI FSMs are not processing when menu is closed
                // (though they won't process if Update("UIMenus") isn't called)
            }

            _frameCount++;
        }

        void FixedUpdate()
        {
            // Example for physics-related FSMs
            FSM_API.Update("FixedPhysicsGroup"); // Update FSMs specifically tied to FixedUpdate
        }

        // Example: Only update a "SaveLoadSystem" group when a save/load operation is actually happening.
        public void PerformSave()
        {
            // FSM_API.CreateInstance("SaveLoadFSM", new SaveLoadContext());
            // ...
            FSM_API.Update("SaveLoadSystem"); // Only call this when explicitly saving or loading
            // ...
        }
    }
```
    Derived Clocks and Custom Time Tracking:

        FSM_API.Update(groupName) doesn't implicitly use Time.deltaTime or Unity's fixed timestep. This allows you to implement custom "clocks" for different parts of your game logic.

        Example: In a turn-based strategy game, you might have a "TurnBasedLogic" processing group where FSM_API.Update("TurnBasedLogic") is only called once per game turn, or after specific player actions. This decouples your FSMs from the rendering framerate, enabling fully deterministic or event-driven simulations.

        You could even have FSMs that track their own internal time, unrelated to real-world time, by using a custom IStateContext that holds a "game-tick" counter.

    Orchestrating FSMs Across Groups (Parent/Child Concepts):

        While FSM_API doesn't enforce a direct "parent-child" relationship between FSM definitions in its internal structure, you can orchestrate complex behaviors by having FSMs in one group influence those in another.

        Mechanism: A "parent" FSM in one group might set a flag or value in the IStateContext of a "child" FSM instance, or it might call FSMHandle.RequestTransition() or FSMHandle.TransitionTo() directly on the child FSM's handle.

        Execution Flow: Crucially, the "child" FSM will only react to these changes (e.g., perform onUpdate or check transitions) when its own dedicated processing group is subsequently updated by an FSM_API.Update(groupName) call in your main game loop. This allows for highly modular and controlled execution.

    Intermixing Context Types:

        FSM_API doesn't differentiate between MonoBehaviour, pure C#, or ScriptableObject contexts within a ProcessingGroup. This is a performance benefit, as you don't need to create separate processing groups just for different context implementations. All FSMs, regardless of their context type, are efficiently managed together if they belong to the same group.

    Conditional FSM Definition:

        Avoid re-defining FSMs unnecessarily. Use if (!FSM_API.Exists("FSMName")) to ensure FSM definitions are created only once at application startup. While BuildDefinition() handles overwriting, initial creation checks save a minor amount of overhead and prevent potential logical issues if definitions are mutable at runtime in ways not intended.

Minimizing FSM Instances

While FSM_API is highly optimized, every FSM instance still carries a small overhead.

    Use FSMs Where Truly Needed: Implement FSMs for behaviors that genuinely benefit from explicit state management (e.g., distinct states, complex transitions, clear actions per state). For very simple behaviors (e.g., a light that just turns on/off based on a single boolean), a direct method call or a simple conditional might be more performant than an FSM.

    Trust IsValid for Cleanup: The IsValid property on your IStateContext automatically handles the removal of FSM instances associated with invalid or destroyed objects. Ensure your IsValid logic correctly detects when an object is no longer active or destroyed, preventing dead FSMs from being needlessly processed.

General Unity Performance Tips (Reminder)

These broader Unity optimization principles are especially crucial when applied within your FSM actions and conditions.

    Avoid GameObject.Find* Methods: Methods like GameObject.Find(), GameObject.FindWithTag(), and FindObjectsOfType() are extremely slow. Always cache references to GameObjects or components in Awake(), Start(), or OnEnable().

    Minimize GetComponent<T>(): Repeated calls to GetComponent<T>() are costly. Cache component references in a field after the first retrieval.

    Vector3.Distance vs. sqrMagnitude: When comparing distances (e.g., IsPlayerWithinRange), use (transform.position - target.position).sqrMagnitude instead of Vector3.Distance(). sqrMagnitude avoids a computationally expensive square root operation, which is unnecessary when only comparing relative magnitudes.

    LINQ and foreach Allocation Awareness: While convenient, LINQ queries and foreach loops on certain collection types (especially older ones or when used on structs causing boxing) can generate garbage. For performance-critical code on arrays and List<T>, consider traditional for loops.

    Profile Regularly: Performance optimization is an iterative process. Use Unity's Profiler (or similar tools for non-Unity applications) to identify actual bottlenecks in your FSM logic. Don't optimize blindly; let the profiler guide your efforts.

Conclusion

Performance is a continuous consideration throughout your development process. By thoughtfully choosing update frequencies, implementing efficient contexts, optimizing state actions, and mastering the powerful ProcessingGroup system, you can leverage FSM_API to its fullest potential, building highly responsive, scalable, and performant state-driven systems. Regular profiling will be your best friend in ensuring your FSMs remain fast and efficient, even at scale.
