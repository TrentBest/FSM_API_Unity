08. Performance Tips for FSM_API

Optimizing FSM_API usage is key.
It helps build high-performance games and applications.
This guide covers strategies.
They keep your FSMs fast and efficient, even at scale.

Choosing the Right Update Frequency

FSMs can be updated:

    Every Tick (-1):

        Pros: Most responsive. Immediate reaction to changes.

        Cons: Highest CPU cost. Updates every single time.

        Use for: Player input, critical real-time AI, essential UI.

    Event-Driven (0):

        Pros: Zero CPU when idle. Only updates on demand.

        Cons: Must trigger updates manually via RequestTransition.

        Use for: UI panels, turn-based logic, rare state changes.

    Nth Tick (>0):

        Pros: Reduces update frequency, saves CPU cycles.

        Cons: Less responsive; introduces slight delay.

        Use for: Background AI, non-critical NPCs, periodic checks.
        (E.g., 1 skips 1 tick, processes on 2nd; 5 skips 5, processes on 6th).

Example:
C#

using TheSingularityWorkshop.FSM.API;
using UnityEngine; // For MonoBehaviour context

public class FSMExamples
{
    public static void DefineVariousFSMs()
    {
        // Player FSM: Needs immediate response, updates every tick.
        FSM_API.CreateFiniteStateMachine("PlayerInputFSM",
                                         processRate: -1, // Update every single tick
                                         processingGroup: "InputLogic")
            .State("Idle", onUpdate: (ctx) => { /* Check for input continuously */ })
            .BuildDefinition();

        // Door FSM: Only updates when activated by a player or event.
        FSM_API.CreateFiniteStateMachine("DoorStateFSM",
                                         processRate: 0, // Event-driven
                                         processingGroup: "Environment")
            .State("Closed", onEnter: (ctx) => { /* Play closed animation */ })
            .State("Opening", onUpdate: (ctx) => { /* Animate opening */ })
            .State("Open", onEnter: (ctx) => { /* Door is fully open */ })
            .BuildDefinition();

        // Bird Flock FSM: Can afford less frequent updates.
        FSM_API.CreateFiniteStateMachine("BirdFlockAI_FSM",
                                         processRate: 10, // Skip 10 ticks, process on 11th
                                         processingGroup: "BackgroundAI")
            .State("Flying", onUpdate: (ctx) => { /* Update bird positions slowly */ })
            .State("Perched", onUpdate: (ctx) => { /* Occasional checks for nearby food */ })
            .BuildDefinition();
    }
}

Efficient Context Implementation

Your IStateContext implementation is crucial.
Keep its methods and properties lean.

    IsValid Property:

        Make this check as fast as possible.

        Avoid complex logic, physics queries, or allocations.

        For MonoBehaviour contexts, this != null && gameObject.activeInHierarchy is ideal.

    Context Data:

        Store pre-calculated or cached values.

        Do not perform expensive lookups repeatedly.

        Pass necessary data into the context constructor.

Optimizing State Actions and Transitions

Methods like onEnter, onUpdate, onExit, and condition run frequently.
Optimize them like any hot-path code.

    Avoid Allocations:

        Minimize new keyword usage.

        Avoid creating new collections.

        Use object pooling if creating many temporary objects.

        Reduce string operations; string concatenation creates garbage.

    Minimize Expensive Computations:

        Physics queries, pathfinding, GetComponent<T>() are costly.

        Perform these less often (e.g., in onEnter then cache result).

        Or move to less frequent processRate FSMs.

    Cache References:

        Don't call GameObject.Find().

        Or GetComponent() in onUpdate or condition.

        Cache references in Awake(), Start(), or onEnter().

Leveraging Processing Groups

FSM_API.Update(string groupName) gives you control.
It allows granular control over updates.
Organize your FSMs into logical groups.

    Group by Priority/Frequency:

        "Player" for critical, every-frame logic.

        "AI" for common AI, updated periodically.

        "UI" for UI elements, updated as needed.

        "Background" for very infrequent checks.

    Update Only Necessary Groups:

        During gameplay, update Player and AI.

        In a menu, only update UI.

        Pause background groups when not visible.

Example:
C#

using TheSingularityWorkshop.FSM.API;
using UnityEngine; // For MonoBehaviour update loop

public class FSMUpdater : MonoBehaviour
{
    void Update()
    {
        // Update high-priority player and enemy logic every frame
        FSM_API.Update("PlayerInput");
        FSM_API.Update("EnemyCombat");

        // Update background AI only every few frames, controlled by its definition's processRate
        FSM_API.Update("BackgroundAI");

        // Only update UI when a menu is active
        if (GameManager.IsMenuOpen)
        {
            FSM_API.Update("UIMenus");
        }
    }

    // You can define groups that are NEVER automatically updated
    // For example, a save/load system FSM, only triggered by events.
    // FSM_API.Update("SaveLoadSystem"); // Only call this when explicitly saving/loading
}

Minimizing FSM Instances

Use FSMs where state management is genuinely complex.
For very simple behaviors, consider simpler patterns.
Like direct method calls or basic boolean flags.

    Remove Invalid Instances:

        The IsValid property handles this automatically.

        Ensure your IsValid logic correctly detects.

        When an object is no longer active or destroyed.

        This prevents processing dead FSMs.

General Unity Performance Tips (Reminder)

These apply broadly, but especially to FSM actions.

    Avoid GameObject.Find*: Cache references on Awake() or Start().

    Minimize GetComponent*: Cache component references.

    Vector3.Distance vs. sqrMagnitude: Use sqrMagnitude for distance comparisons.
    It avoids a costly square root operation.

    Linq and Foreach: Be mindful of performance.
    foreach can cause allocations on structs.
    Use for loops for arrays and List<T> where possible.

Conclusion

Performance is a continuous process.
Profile your application regularly.
Use Unity's Profiler or other tools.
Identify bottlenecks in your FSM logic.
Applying these tips will help ensure.
Your FSM-driven systems remain fast and responsive.