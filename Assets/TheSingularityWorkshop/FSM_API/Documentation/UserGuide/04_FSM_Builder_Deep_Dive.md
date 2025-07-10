04. Deep Dive into FSMBuilder

FSMBuilder provides a fluent API to construct FSM definitions. Using chained method calls, you define:

    Named states and their behaviors

    Transitions between states based on conditions

    The initial state where the FSM begins

    Its default processing group and rate of update

    Other FSM-wide configurations before finalizing it via .BuildDefinition()

Think of it as your entire FSM logic encoded as one powerful, readable block of code that clearly lays out the behavior blueprint.

✨ What is a Fluent API?

A fluent API is designed for readability, flow, and chaining. Every method returns the builder itself, allowing you to string multiple commands together:
C#
```csharp
FSM_API.CreateFiniteStateMachine("MyFSM")
    .State("Idle", ...)
    .State("Walking", ...)
    .WithInitialState("Idle")
    .Transition("Idle", "Walking", ctx => ...)
    .BuildDefinition();
```
Each call refines the FSM’s structure in a step-by-step manner. The result is both clear and declarative, resembling a natural language description of your FSM.

🏁 Starting the Build

You initiate the FSM definition process using FSM_API.CreateFiniteStateMachine(...). This factory method returns a FSMBuilder instance, ready for you to configure.
C#
```csharp
FSM_API.CreateFiniteStateMachine(
    fsmName: "EnemyAI",
    processRate: 1, // Updates every 2nd tick (0-indexed)
    processingGroup: "AI"
)
```
Parameters:

    fsmName (string): Required. A unique name for this FSM definition. This name is used to retrieve and instantiate the FSM later.

    processRate (int): Optional. Defaults to 0 (event-driven). This controls how often instances of this FSM definition will process their onUpdate logic.

        -1: Every tick of its assigned processing group. For highly reactive behaviors.

        0: Only when manually stepped or driven by external events. Zero CPU consumption when idle, perfect for reactive systems.

        >0: Every (processRate + 1) ticks (e.g., 1 means every 2nd tick, 5 means every 6th tick). Useful for optimizing performance of less critical behaviors.

    processingGroup (string): Optional. Defaults to "Update". This categorizes when and how this FSM definition's instances are updated within your application's main loop.

        ⚠️ Important: You must register the processing group (e.g., FSM_API.CreateProcessingGroup("AI")) and ensure you are calling its corresponding update method (e.g., FSM_API.Update("AI")) for FSMs in that group to process.

🧱 FSMBuilder Methods

Once you have a FSMBuilder instance, you use its methods to define the FSM's structure and behavior. For optimal readability and understanding, we recommend defining your FSM in the following order:

    Define all your States first.

    Set the Initial State.

    Define your Transitions.

State(...)

Defines a single state within your FSM and its optional actions that run when entering, updating, or exiting that state.
C#
```csharp
.State("Idle",
    onEnter: ctx => Debug.Log("Entering Idle"),
    onUpdate: ctx => Debug.Log("Still Idle"),
    onExit:  ctx => Debug.Log("Leaving Idle")
)
```
    name (string): The unique name of this state within the FSM definition.

    onEnter: (Optional) An action that runs once, the moment the FSM enters this state. Perfect for starting animations or playing sounds.

    onUpdate: (Optional) An action that runs repeatedly, each time the FSM ticks while in this state (based on its processRate). Good for continuous actions like movement or input checks.

    onExit: (Optional) An action that runs once, the moment the FSM leaves this state. Useful for stopping animations or cleaning up resources.

You can define as many .State(...) calls as your FSM requires.

WithInitialState(...)

Specifies the exact state name where any new instance of this FSM definition will begin.
C#
```chsarp
.WithInitialState("Idle")
```
    If omitted, the very first .State(...) call you defined will automatically become the initial state. However, explicitly setting it improves clarity.

Transition(...)

Defines a specific conditional path for the FSM to move from one state to another.
C#
```csharp
.Transition("Idle", "Attack", ctx => ((EnemyContext)ctx).IsPlayerVisible())
```
    from (string): The name of the state the FSM must currently be in.

    to (string): The name of the state the FSM will transition to if the condition is met.

    condition: A function that returns true or false. If true, the transition will occur.

    ➡️ Important: Transition Priority ⬅️
    Transitions defined for a given "from" state are evaluated in the order they are provided within the builder. The first transition whose condition evaluates to true will be taken. This allows you to implicitly define priority among your transitions. Place more critical or specific transitions before more general ones.

You can define multiple transitions from a single state, and they will be checked in the order you list them.

AnyTransition(...)

Defines a transition that can occur from any state to a specified target state, based on a condition. Useful for global events like "death" or "pause."
C#
```csharp
.AnyTransition("Dead", ctx => ((PlayerContext)ctx).Health <= 0)
```
    This transition will be checked before any specific transitions from the current state.

WithProcessRate(...)

Allows you to override the processRate initially set in FSM_API.CreateFiniteStateMachine(...) for this specific definition.
C#
```csharp
.WithProcessRate(5) // Instances of this FSM will update every 6th tick
```
WithName(...)

Lets you rename the FSM definition before it's finalized and registered.
C#
```csharp
.WithName("FinalEnemyAI")
```
This is particularly useful if you are reusing or deriving a definition dynamically and want to save it under a new name.

WithProcessingGroup(...)

Allows you to change the processingGroup for this FSM definition. This method aligns with the processingGroup concept used throughout the API.
C#
```csharp
.WithProcessingGroup("LateUpdate")
```
    Processing groups define "tick domains." When you call FSM_API.Update("LateUpdate"), only FSMs assigned to the "LateUpdate" group will process their logic. This gives you precise control over when different FSMs in your game update.

BuildDefinition()

This is the crucial final step! You must call .BuildDefinition() to finalize your FSM's blueprint and register it with the FSM_API system.
C#
```csharp
.BuildDefinition();
```
Calling BuildDefinition() performs several important actions:

    It internally validates all the states and transitions you've defined, ensuring they are correctly structured.

    It registers the complete FSM definition with FSM_API, making it available for instantiation.

    If an FSM definition with the same name already exists, BuildDefinition() will update that existing definition with the new blueprint you've just created.

        🔁 Runtime Adaptability: This means you can re-define FSMs by calling .BuildDefinition() again, even while your application is running. Existing FSM instances will seamlessly adopt the new logic at their next update cycle, enabling powerful hot-swapping and live iteration.

🎯 Full Example: Player Combat FSM

This example demonstrates how to define a complete FSM for a player character's combat behavior, adhering to the recommended structure:
C#
```csharp
using UnityEngine; // Required for Debug.Log in Unity
using TheSingularityWorkshop.FSM.API;

public static class PlayerCombatFSM
{
    // Define state names as constants for cleaner code and less typos
    public const string Idle = "Idle";
    public const string Attacking = "Attacking";
    public const string Dodging = "Dodging";
    public const string Stunned = "Stunned"; // Added a new state for demonstration

    public static void DefinePlayerCombatFSM() // Renamed for clarity
    {
        // Best practice: Only define FSMs once at application start
        if (!FSM_API.Exists("PlayerCombatFSM"))
        {
            FSM_API.CreateFiniteStateMachine("PlayerCombatFSM", processRate: -1, processingGroup: "Combat")
                // 1. Define all your States first
                .State(Idle,
                    onEnter: ctx => Debug.Log($"{ctx.Name} entered Idle state."),
                    onUpdate: ctx => ((PlayerContext)ctx).CheckForCombatInput(), // Assumes PlayerContext has this method
                    onExit:  ctx => Debug.Log($"{ctx.Name} exiting Idle state."))
                .State(Attacking,
                    onEnter: ctx => ((PlayerContext)ctx).StartAttack(),
                    onUpdate: ctx => ((PlayerContext)ctx).DoAttack(),
                    onExit:  ctx => ((PlayerContext)ctx).EndAttack())
                .State(Dodging,
                    onEnter: ctx => ((PlayerContext)ctx).StartDodge(),
                    onUpdate: ctx => ((PlayerContext)ctx).DoDodge(),
                    onExit:  ctx => Debug.Log($"{ctx.Name} finished Dodging."))
                .State(Stunned,
                    onEnter: ctx => ((PlayerContext)ctx).ApplyStunEffect(),
                    onUpdate: ctx => Debug.Log($"{ctx.Name} is Stunned..."),
                    onExit: ctx => ((PlayerContext)ctx).RemoveStunEffect())

                // 2. Set the Initial State
                .WithInitialState(Idle)

                // 3. Define your Transitions (order matters for priority!)
                // From Idle:
                .Transition(Idle, Attacking, ctx => ((PlayerContext)ctx).HasAttackInput())
                .Transition(Idle, Dodging, ctx => ((PlayerContext)ctx).HasDodgeInput())

                // From Attacking:
                .Transition(Attacking, Stunned, ctx => ((PlayerContext)ctx).IsStunned()) // High priority: if attacked while attacking, go to stunned
                .Transition(Attacking, Idle, ctx => ((PlayerContext)ctx).AttackComplete())

                // From Dodging:
                .Transition(Dodging, Stunned, ctx => ((PlayerContext)ctx).IsStunned()) // High priority: if stunned during dodge
                .Transition(Dodging, Idle, ctx => ((PlayerContext)ctx).DodgeComplete())

                // From Stunned:
                .Transition(Stunned, Idle, ctx => ((PlayerContext)ctx).StunDurationElapsed())

                // Global transitions (checked from any state, usually after state-specific transitions unless it's an AnyTransition)
                // AnyTransition is checked from ANY state.
                .AnyTransition(Stunned, ctx => ((PlayerContext)ctx).TookMassiveDamage()) // If massive damage, go to stunned from anywhere
                .BuildDefinition(); // Finalize and register this FSM blueprint
        }
    }
}
```
Note: The PlayerContext in this example would be your custom class (e.g., a MonoBehaviour) that implements IStateContext and holds all the game-specific data and methods (CheckForCombatInput, StartAttack, IsStunned, etc.) that the FSM needs to interact with.

🧠 Best Practice: Use FSM_API.Exists(...)

FSM definitions are typically created once at the application's startup. Using FSM_API.Exists() prevents accidental duplicate definitions if your FSM setup code might be called multiple times (e.g., from different Awake() or Start() methods).
C#
```csharp
if (!FSM_API.Exists("MyFSM"))
{
    // Define your FSM here using FSM_API.CreateFiniteStateMachine(...)
    // and then .BuildDefinition();
}
```
🧰 Advanced: Modifying FSMs & Creating Variants

FSM_API allows for powerful runtime modification and creation of FSM variants:

    Modifying an Existing Definition:
    To update an FSM definition, simply call FSM_API.CreateFiniteStateMachine with the same unique name as an existing definition, configure your changes using the builder methods, and then call BuildDefinition(). This will overwrite the old definition with the new one. Existing FSM instances will automatically adapt to the new logic on their next update cycle.
    C#
```csharp
// Example: Dynamically update an FSM definition at runtime
FSM_API.CreateFiniteStateMachine("PlayerCombatFSM") // Use the same name to overwrite
    .State(Idle, onEnter: ctx => Debug.Log("NEW Idle Behavior!")) // Modify an existing state
    .Transition(Idle, Attacking, ctx => ((PlayerContext)ctx).HasAttackInput() && ((PlayerContext)ctx).CanAttack()) // Add a new condition
    // ... continue defining the rest of your FSM, or only changed parts ...
    .BuildDefinition(); // This will replace the old "PlayerCombatFSM" definition
```
Creating Variants:
To create a new FSM definition that is a slight variation of another, you would define it with a new, unique name. You can copy relevant logic from existing definitions conceptually into your new builder.
C#
```csharp
    // Example: Create an "EliteEnemyAI" variant based on a conceptual "EnemyAI"
    FSM_API.CreateFiniteStateMachine("EliteEnemyAI", processRate: -1, processingGroup: "AI")
        .State("Idle", ...) // Define states for EliteEnemyAI
        .State("EliteAttack", onEnter: ctx => Debug.Log("Elite Attack Initiated!")) // Add unique states/logic
        .WithInitialState("Idle")
        .Transition("Idle", "EliteAttack", ctx => ((EnemyContext)ctx).IsEliteTargetVisible())
        // ... add other states and transitions specific to EliteEnemyAI ...
        .BuildDefinition();
```
🧼 Processing Group Clean-Up

FSM_API is designed for runtime flexibility and efficient memory management. If you no longer need a specific set of FSMs (e.g., when transitioning between game levels), you can clean up their processing group:
C#
```csharp
FSM_API.RemoveProcessingGroup("Combat") // Remove the "Combat" group
```
When RemoveProcessingGroup() is called:

    It calls the onExit() action for all FSM instances currently in that group.

    It unregisters all FSMHandle instances associated with that group.

    It removes the group from being ticked by the FSM_API.Update() calls.

    It reclaims all memory associated with that group, ensuring no leaks.

This feature is excellent for managing game state across levels or different game modes, allowing for clean creation and destruction of FSM-driven logic.

🛠 Manual Transitions & Handle Overrides

For finer control, you can directly interact with individual FSM instances to force a state change:
C#
```csharp
// Assuming 'myPlayerFSMHandle' is an FSMHandle instance for a player's FSM
// This will force the FSM into the "Dead" state, bypassing its normal transitions.
myPlayerFSMHandle.TransitionTo("Dead");

    FSMHandle.TransitionTo(string nextStateName): Forces the FSM to immediately transition to nextStateName, executing the onExit of the current state and onEnter of the new state. Use with caution as it bypasses the defined conditions.
```
You can also override the default processing group for an individual FSM instance when you create it:
C#
```csharp
// This instance will be processed by "BossLogic" group, regardless of its definition's default group.
FSM_API.CreateInstance("EnemyAI", context, "BossLogic");
```
    FSMHandle instances may live in processing groups different from their definition's default.

    If you specify a new processing group that doesn't yet exist, FSM_API will create it automatically.

    All FSM features work seamlessly across custom groups.

🧩 Summary

Method
	

Purpose

State(...)
	

Define a state and its onEnter, onUpdate, onExit logic

WithInitialState(...)
	

Set the starting state for new FSM instances

Transition(...)
	

Define a conditional path from a specific state to another

AnyTransition(...)
	

Define a global conditional path from any state to another

WithProcessRate(...)
	

Control the update frequency for instances of this FSM definition

WithName(...)
	

Rename the FSM definition before it's built

WithProcessingGroup(...)
	

Set the processing group name for update loop synchronization

BuildDefinition()
	

Finalize and register the FSM blueprint. Essential last step.

FSMBuilder is your FSM factory — write once, reuse everywhere, and iterate rapidly.

➡️ Continue to: 05_Context_Implementation.md
