07. Understanding Finite State Machines (FSMs)

Finite State Machines (FSMs) are a powerful tool.
They help organize complex behaviors.
This guide will explain FSMs.
From basic ideas to advanced concepts.
It uses a "college course" structure.
To build your understanding step by step.

FSM 101: What is a Finite State Machine?

Let's start with the basics.
Imagine anything that changes how it acts.
Based on its current "mode" or "stage."

    A game character might be Idle.

    Then Walking, then Jumping.

    A door might be Closed.

    Then Opening, then Open.

    A traffic light might be Green.

    Then Yellow, then Red.

A Finite State Machine (FSM) is simply a system.
That manages these changing "modes."
It can only be in one "state" at a time.
It has a limited, or "finite," number of possible states.
And it has clear rules for "transitions".
This means moving from one state to another.

Think of a Light Switch:

    It has two states: On and Off.

    There's a transition from Off to On.
    (Flipping the switch up).

    There's a transition from On to Off.
    (Flipping the switch down).

    It cannot be "half-on" or "both-on-and-off."
    It's one state at a time.

FSMs help you organize complex behaviors cleanly.
They make it easy to understand what's happening.
And predict what will happen next.

FSM 200: Core Components of an FSM

Now, let's look at the main parts of any FSM.
Understanding these pieces is key.

1. States: The "What's Happening Now"

A State is a named stage or phase.
Your FSM can be in.
It's the "what's happening right now."
For the thing your FSM is controlling.

Each state can have specific actions:

    Entry Action (OnEnter):
    Code that runs once.
    The moment the FSM enters this state.
    Example: Start an animation, play a sound.

    Update Action (OnUpdate):
    Code that runs repeatedly.
    As long as the FSM stays in this state.
    Example: Move a character, check for input.

    Exit Action (OnExit):
    Code that runs once.
    The moment the FSM leaves this state.
    Example: Stop an animation, clean up resources.

Conceptual Code Example (State Actions):
C#

// Imagine this is part of your FSM setup
// (This is generic FSM logic, not FSM_API specific)

public class CharacterFSM
{
    // ... other FSM setup ...

    public void OnEnterIdle()
    {
        Console.WriteLine("Character entered Idle state: Start breathing animation.");
    }

    public void OnUpdateIdle()
    {
        Console.WriteLine("Character is Idle: Waiting for input.");
        // Check for player input to move
    }

    public void OnExitIdle()
    {
        Console.WriteLine("Character exited Idle state: Stop breathing animation.");
    }

    public void OnEnterWalking()
    {
        Console.WriteLine("Character entered Walking state: Start walking animation.");
    }

    public void OnUpdateWalking()
    {
        Console.WriteLine("Character is Walking: Move forward.");
        // Apply movement logic
    }

    public void OnExitWalking()
    {
        Console.WriteLine("Character exited Walking state: Stop walking animation.");
    }
}

2. Transitions: The "How We Change"

A Transition is a rule.
It defines how to move from one state to another.
Transitions are triggered by conditions.

    Condition (Predicate):
    A check that returns true or false.
    If true, the transition happens.
    If false, the FSM stays in its current state.
    Example: "Is health zero?", "Is button pressed?"

Conceptual Code Example (Transitions):
C#

// (This is generic FSM logic, not FSM_API specific)

public class CharacterFSM
{
    public enum CharacterState { Idle, Walking, Running, Jumping }
    public CharacterState CurrentState { get; private set; }

    public bool IsMovingInputPressed { get; set; }
    public bool IsJumpButtonPressed { get; set; }
    public bool IsFalling { get; set; }

    public CharacterFSM()
    {
        CurrentState = CharacterState.Idle; // Initial state
    }

    public void UpdateFSM()
    {
        switch (CurrentState)
        {
            case CharacterState.Idle:
                // Transition from Idle to Walking
                if (IsMovingInputPressed)
                {
                    CurrentState = CharacterState.Walking;
                    // Call OnExitIdle() then OnEnterWalking()
                    Console.WriteLine("Transition: Idle -> Walking");
                }
                // Transition from Idle to Jumping
                else if (IsJumpButtonPressed)
                {
                    CurrentState = CharacterState.Jumping;
                    Console.WriteLine("Transition: Idle -> Jumping");
                }
                break;
            case CharacterState.Walking:
                // Transition from Walking to Idle
                if (!IsMovingInputPressed)
                {
                    CurrentState = CharacterState.Idle;
                    Console.WriteLine("Transition: Walking -> Idle");
                }
                // Transition from Walking to Jumping
                else if (IsJumpButtonPressed)
                {
                    CurrentState = CharacterState.Jumping;
                    Console.WriteLine("Transition: Walking -> Jumping");
                }
                break;
            case CharacterState.Jumping:
                // Transition from Jumping to Idle (when landing)
                if (!IsFalling) // Assuming IsFalling becomes false on landing
                {
                    CurrentState = CharacterState.Idle;
                    Console.WriteLine("Transition: Jumping -> Idle");
                }
                break;
        }
        // In a real FSM, OnUpdate for current state would run here
    }
}

3. Events/Triggers: The "What Causes a Check"

An Event or Trigger is something that happens.
It causes the FSM to check its transition conditions.

    External Events: Player input, enemy spotted, health low.

    Internal Events: Timer finished, animation ended.

In many FSM systems, you "tick" the FSM.
This causes it to check all conditions.
Or you can explicitly "request" a transition.

FSM 300: FSM Types and Patterns

FSMs come in different flavors.
And can be combined in powerful ways.

1. Mealy vs. Moore FSMs

These describe when actions happen.

    Moore FSM:
    Actions are tied only to the state itself.
    Actions happen when you are in a state.
    OnEnter, OnUpdate, OnExit actions are Moore-style.
    This is simpler to understand.

    Mealy FSM:
    Actions are tied to the transitions.
    Actions happen when you move between states.
    Example: "When transitioning from Idle to Walking, play footstep sound."
    This can be more precise for specific moments.
    FSM_API primarily uses a Moore-like approach.
    With OnEnter/OnExit for state-specific actions.
    But you can put transition-specific logic.
    Inside your condition checks or OnEnter methods.

2. Hierarchical FSMs (HFSMs)

Imagine a state that's very complex.
Like "Combat."
Inside "Combat," a character might have many sub-states.
Like "Attacking," "Dodging," "Blocking."
An HFSM allows states to contain other FSMs.
It's like an FSM within an FSM.

Benefits:

    Modularity: Break down complex FSMs into smaller, manageable ones.

    Reusability: Reuse a "Combat" FSM.
    For different enemy types.

    Reduced Complexity: Transitions from a parent state.
    Apply to all its sub-states.
    Reducing "spaghetti" transitions.

Conceptual Code Example (HFSM Structure):
C#

// (This is generic FSM logic, not FSM_API specific)

public class PlayerHFSM
{
    public enum PlayerTopState { Exploring, Combat, InventoryOpen }
    public PlayerTopState CurrentTopState { get; private set; }

    // Sub-FSMs
    private MovementFSM _movementFSM; // Handles Walking, Running, Jumping
    private CombatFSM _combatFSM;     // Handles Attacking, Dodging, Blocking

    public PlayerHFSM()
    {
        _movementFSM = new MovementFSM(); // Initialize sub-FSMs
        _combatFSM = new CombatFSM();
        CurrentTopState = PlayerTopState.Exploring;
    }

    public void Update()
    {
        switch (CurrentTopState)
        {
            case PlayerTopState.Exploring:
                _movementFSM.Update(); // Only update movement when exploring
                if (IsEnemyNearby())
                {
                    CurrentTopState = PlayerTopState.Combat;
                    Console.WriteLine("Transition: Exploring -> Combat");
                    _combatFSM.EnterCombatMode(); // Initial setup for combat FSM
                }
                break;
            case PlayerTopState.Combat:
                _combatFSM.Update(); // Only update combat when in combat
                if (!IsEnemyNearby() && _combatFSM.IsCombatOver())
                {
                    CurrentTopState = PlayerTopState.Exploring;
                    Console.WriteLine("Transition: Combat -> Exploring");
                    _combatFSM.ExitCombatMode(); // Cleanup for combat FSM
                }
                break;
            // ... other top-level states
        }
    }

    private bool IsEnemyNearby() { /* ... logic ... */ return false; }
}

public class MovementFSM // This would be a separate FSM
{
    public enum MovementState { Idle, Walk, Run, Jump }
    public MovementState CurrentState { get; private set; }
    // ... states and transitions for Idle, Walk, Run, Jump
    public void Update() { /* ... */ }
    public void EnterMovementMode() { /* ... */ }
    public void ExitMovementMode() { /* ... */ }
}

public class CombatFSM // This would be another separate FSM
{
    public enum CombatState { Attacking, Dodging, Blocking }
    public CombatState CurrentState { get; private set; }
    // ... states and transitions for Attacking, Dodging, Blocking
    public void Update() { /* ... */ }
    public void EnterCombatMode() { /* ... */ }
    public void ExitCombatMode() { /* ... */ }
    public bool IsCombatOver() { /* ... */ return true; }
}

3. Concurrent FSMs

Sometimes, multiple behaviors need to run.
Independently and simultaneously.
For example, a character might be "Walking."
While also "Reloading" a weapon.
These are not mutually exclusive.
Concurrent FSMs allow multiple FSMs.
To operate in parallel on the same object.

Benefits:

    Orthogonal Behaviors: Manage independent aspects.
    Without complex state combinations.

    Simplicity: Each FSM focuses on one job.

    Flexibility: Easily add or remove behaviors.

Conceptual Example:

    FSM 1 (Movement): States: Idle, Walk, Run.

    FSM 2 (Weapon): States: Ready, Reloading, Firing.

    FSM 3 (Status): States: Normal, Poisoned, Stunned.

All three FSMs update independently.
The character can be "Running," "Reloading," and "Poisoned" at once.
This is where FSM_API's Processing Groups shine.
You can tick different FSMs at different rates.
Or from different parts of your code.

FSM 400: Advanced FSM Concepts

Let's explore more sophisticated FSM features.

1. State Data

Sometimes, a state needs its own unique data.
This data is only relevant while in that state.
Example: A "ChargingAttack" state might need.
A variable for how long it has been charging.

While FSM_API's state actions.
Operate on your IStateContext.
You can manage state-specific data.
Within your context object.
Or through temporary variables.
Passed to state actions.

Conceptual Example (State Data in Context):
C#

// (Part of your IStateContext implementation)
public class PlayerContext // Implements IStateContext
{
    public float CurrentChargeTime { get; set; } = 0f; // Data for Charging state

    public void StartCharging() { CurrentChargeTime = 0f; }
    public void UpdateCharging(float deltaTime) { CurrentChargeTime += deltaTime; }
    public bool IsCharged() { return CurrentChargeTime >= 2.0f; }
}

// (Part of your FSM definition)
// .State("Charging",
//     onEnter: (context) => ((PlayerContext)context).StartCharging(),
//     onUpdate: (context) => ((PlayerContext)context).UpdateCharging(Time.deltaTime))
// .Transition("Charging", "Attack", (context) => ((PlayerContext)context).IsCharged())

2. Global Transitions

A Global Transition is a special rule.
It can be triggered from any state in the FSM.
Regardless of the current state.
Example: A "Die" transition.
If health reaches zero, the character dies.
No matter if they are Idle, Walking, or Attacking.

FSM_API's Transition() method.
Is defined between specific from and to states.
To achieve global transitions, you would define.
A transition from every relevant state.
To the "global" target state (e.g., "Dead").
Or, you can use FSM_API.RequestTransition().
To force a state change from anywhere.

3. History States

Imagine an HFSM.
You are in Combat, then Attacking.
Then you get stunned, transitioning to Stunned (a top-level state).
When the stun wears off, you want to go back.
To Attacking, not just the general Combat state.
A History State remembers the last active sub-state.
When leaving a parent state.
And returns to it upon re-entry.

This feature is typically implemented.
Within the FSM framework itself.
FSM_API does not directly expose "History States."
But you can achieve similar behavior.
By storing the last sub-state in your context.
And setting it as the initial state on re-entry.

4. Entry/Exit Actions vs. Update Actions

It's important to use these correctly.

    OnEnter: For one-time setup.

        Initialize variables.

        Start animations.

        Play sounds.

        Allocate temporary resources.

    OnUpdate: For continuous, per-tick logic.

        Movement calculations.

        AI decision-making.

        Checking for frequent conditions.

    OnExit: For one-time cleanup.

        Stop animations/sounds.

        Reset variables.

        Deallocate temporary resources.

Misusing these can lead to bugs.
Or performance issues.
For example, starting an animation in OnUpdate.
Would restart it every frame.

FSM 500: FSM Design Principles and Pitfalls

Using FSMs effectively requires good design.
Avoid common mistakes.

When to Use FSMs

    Clear, Discrete States:
    When an object has distinct modes.
    With clear boundaries between them.
    (e.g., Open, Closed; Idle, Walking, Running).

    Predictable Flow:
    When behavior follows a defined sequence.
    Or has specific rules for changing modes.

    Behavioral Complexity:
    When if-else chains become unmanageable.
    An FSM provides structure.

    Concurrent Behaviors:
    When different aspects of an object.
    Can operate independently (using multiple FSMs).

When NOT to Use FSMs

    Continuous Processes:
    For things that don't have distinct states.
    (e.g., a simple counter, a continuous particle effect).

    Too Many States/Transitions (State Explosion):
    If your FSM diagram looks like spaghetti.
    With hundreds of states and transitions.
    It might be too complex for a single FSM.
    Consider HFSMs or breaking it into smaller FSMs.

    Pure Data Management:
    If you're just storing data.
    And not managing behavior changes.
    A simple class is better.

Common Pitfalls

    State Explosion:
    Creating too many states.
    For every minor variation.
    Leads to unmanageable FSMs.
    Solution: Use HFSMs, combine similar states.
    Or use parameters within a state.

    Spaghetti Transitions:
    Every state can transition to every other state.
    Makes the FSM hard to follow.
    Solution: Define clear, logical paths.
    Use HFSMs to group transitions.

    Over-Engineering:
    Using an FSM for simple logic.
    That could be handled with an if statement.
    Adds unnecessary complexity.
    Solution: Start simple, add FSMs when needed.

    Mismanaging Context:
    Not properly updating context data.
    Or accessing invalid context objects.
    Leads to errors.
    Solution: Ensure IsValid is correct.
    Keep context data up-to-date.

    Performance Bottlenecks in Actions:
    Putting heavy computations in OnUpdate.
    For FSMs that tick frequently.
    Solution: Optimize state actions.
    Use appropriate processRate and ProcessingGroups.

Best Practices

    Keep States Focused: Each state should have a clear purpose.

    Clear Transitions: Define precise conditions for changing states.

    Use Events: Design your system to trigger transitions.
    Based on events, not constant polling (where possible).

    Test Thoroughly: Test each state and transition path.

    Visualize: Draw your FSMs.
    It helps identify complexity and potential issues.

    Start Simple: Begin with a basic FSM.
    Add complexity as needed.

Conclusion

Understanding FSMs is a fundamental skill.
They provide a structured way to manage behavior.
From simple light switches to complex AI.
By applying these principles.
You can design robust, maintainable systems.
That are easy to understand and debug.
FSM_API provides the tools to build these.
Now you have the theory to master them.