04. Deep Dive into FSMBuilder

In previous guides, you learned to define and create FSMs.
This document explores the FSMBuilder class in detail.
It's the core tool to construct FSM definitions.
With a clear, readable, and powerful fluent API.

The FSMBuilder is your primary tool.
It describes states, transitions, and FSM behavior.
Before FSM_API.CreateInstance() brings them to life.

What is a Fluent API?

A fluent API designs object-oriented APIs.
It provides a more readable and expressive syntax.
This is done by allowing method calls to be "chained."
Each method returns the object it operates on.
Enabling the next method in the chain immediately.

In FSMBuilder, this means you define your entire FSM.
Blueprint in a single, flowing statement.
Making FSM definitions highly legible and maintainable.

Entering the FSMBuilder: FSM_API.CreateFiniteStateMachine()

Your journey into defining an FSM always begins here.
It starts with FSM_API.CreateFiniteStateMachine().
This static method on the main FSM_API class.
It returns an FSMBuilder instance.
Ready for you to start chaining methods.
C#

// The entry point to FSMBuilder
FSM_API.CreateFiniteStateMachine(
    "MyPlayerFSM", // Unique name for this FSM blueprint
    processRate: -1, // How often instances of this FSM will update (-1 = every tick)
    processingGroup: "PlayerLogic" // The group this FSM belongs to for updating
);

Parameters:

    fsmName (string): This is the unique identifier.
    For your FSM blueprint. You'll use this name later.
    To create instances (FSM_API.CreateInstance("MyPlayerFSM", context)).
    Choose a descriptive name.

    processRate (int, optional, default: 0):
    Controls how often instances of this FSM definition.
    Will process their onUpdate actions.
    And check for transitions when their processingGroup is updated.

        -1: Processes every single tick. No frames are skipped.

        0: Processes only when explicitly told to (event-driven).
        It will not automatically update each tick of its group.
        Perfect for traffic lights: when the FSM is told to switch.

        N (any integer greater than 0): Skips N ticks, then processes.
        This means it updates on tick N+1, 2(N+1), 3(N+1), etc.
        (e.g., 1 skips 1 tick, processes on 2nd, 4th, etc. - every other tick).
        (e.g., 5 skips 5 ticks, processes on 6th, 12th, etc. - every 6th tick).

    processingGroup (string, optional, default: "Update"):
    The name of the group this FSM definition belongs to.
    When you call FSM_API.Update("YourGroupName").
    All FSM instances associated with "YourGroupName" will be processed.
    Using specific groups allows for granular control.
    Over when different parts of your game logic update.

Composing Your FSM: Key FSMBuilder Methods

Once you have an FSMBuilder instance.
You can chain the following methods.
To define your FSM's structure and behavior.

1. State(string name, Action<IStateContext> onEnter = null, Action<IStateContext> onUpdate = null, Action<IStateContext> onExit = null)

This is how you define each state in your FSM.

    name (string): The unique name for this state.
    Within your FSM.

    onEnter (Action<IStateContext>, optional):
    An action that is executed once.
    When the FSM enters this state.
    Perfect for initialization, playing entry animations.
    Or setting up state-specific conditions.

    onUpdate (Action<IStateContext>, optional):
    An action that is executed repeatedly.
    While the FSM is in this state.
    According to its processRate.
    Use this for continuous logic, like movement.
    AI decision-making, or checking for conditions.
    That might trigger transitions.

    onExit (Action<IStateContext>, optional):
    An action that is executed once.
    When the FSM leaves this state.
    Ideal for cleanup, stopping animations.
    Or resetting values.

Example:
C#

FSM_API.CreateFiniteStateMachine("EnemyAI")
    .State("Idle",
        onEnter: (context) => ((EnemyContext)context).PlayIdleAnimation(),
        onUpdate: (context) => ((EnemyContext)context).LookForPlayer(),
        onExit: (context) => ((EnemyContext)context).StopIdleAnimation())
    .State("Patrol",
        onEnter: (context) => ((EnemyContext)context).StartPatrolRoute(),
        onUpdate: (context) => ((EnemyContext)context).AdvancePatrol(),
        onExit: (context) => ((EnemyContext)context).ResetPatrol());

2. WithInitialState(string name)

Specifies the state an FSM instance will start in.
When it is first created from this definition.

    name (string): The name of the state.
    That should be the initial state.
    This state must be defined using State()
    before you call BuildDefinition().
    If not explicitly set, BuildDefinition() will automatically.
    Set the first State() added as the initial state.

Example:
C#

FSM_API.CreateFiniteStateMachine("EnemyAI")
    .State("Idle", /* ... actions ... */)
    .State("Patrol", /* ... actions ... */)
    .WithInitialState("Idle"); // Enemy will start in "Idle" state.

3. Transition(string from, string to, Func<IStateContext, bool> condition)

Defines a possible transition path between two states.
And the condition under which it occurs.

    from (string): The name of the state.
    The FSM must currently be in.
    For this transition to be considered.

    to (string): The name of the state.
    The FSM will transition to.
    If the condition is met.

    condition (Func<IStateContext, bool>):
    A delegate (or lambda expression).
    That returns true if the transition should occur.
    And false otherwise.
    This function receives the IStateContext as an argument.
    Allowing you to check properties or call methods.
    On your context object.

Example:
C#

FSM_API.CreateFiniteStateMachine("EnemyAI")
    .State("Idle", /* ... */)
    .State("Chase", /* ... */)
    .State("Attack", /* ... */)
    .WithInitialState("Idle")
    .Transition("Idle", "Chase", (context) => ((EnemyContext)context).IsPlayerVisible())
    .Transition("Chase", "Attack", (context) => ((EnemyContext)context).IsPlayerInRangeForAttack())
    .Transition("Chase", "Idle", (context) => !((EnemyContext)context).IsPlayerVisible());

Important: An FSM can have multiple transitions.
From a single "from" state.
FSM_API will evaluate these transitions.
In the order they were defined.
The first transition whose condition is true will be taken.

4. WithProcessRate(int rate)

Allows you to override the processRate.
Specified in CreateFiniteStateMachine() for this blueprint.

    rate (int): The new process rate.
    See CreateFiniteStateMachine() for details on rate values.

Example:
C#

FSM_API.CreateFiniteStateMachine("SlowMovingObject", processRate: -1) // Default to every tick
    .WithProcessRate(5) // But this builder overrides to skip 5 ticks (process every 6th)
    .State("Moving", /* ... */);

5. WithName(string name)

Allows you to rename the FSM definition.
Typically set in CreateFiniteStateMachine().
This method provides flexibility for complex builder patterns.

    name (string): The new unique name for the FSM definition.

Example:
C#

FSM_API.CreateFiniteStateMachine("TemporaryName")
    .WithName("FinalPlayerStateMachine") // Renames the FSM blueprint
    .State("Run", /* ... */);

6. WithUpdateCategory(string category) (or processingGroup in constructor)

Allows you to override the processingGroup.
Specified in CreateFiniteStateMachine() for this blueprint.

    category (string): The new processing group name.
    See CreateFiniteStateMachine() for details.

Example:
C#

FSM_API.CreateFiniteStateMachine("UI_FSM", processingGroup: "DefaultUI")
    .WithUpdateCategory("HighPriorityUI") // Moves it to a different processing group
    .State("Opened", /* ... */);

7. BuildDefinition()

This is the final and crucial step.
In defining your FSM.
It takes all states, transitions, initial state, and settings.
And compiles them into a complete FSM blueprint.
This blueprint is then registered internally with FSM_API.
Making it available for creating live FSM instances.

    Validation: BuildDefinition() performs checks.
    Such as ensuring at least one state is defined.
    And that the specified initial state actually exists.

    Registration: If an FSM definition with the same name.
    Already exists, BuildDefinition() will update it.
    With the new configuration provided by the builder.
    If it's a new name, it will register it.

    Important: You must call BuildDefinition()
    For your FSM blueprint to be registered and usable.

Example of a complete FSM definition:
C#

using TheSingularityWorkshop.FSM.API;
// Assuming MyContext is your IStateContext implementation
// public class MyContext : MonoBehaviour, IStateContext { ... } OR
// public class MyContext : IStateContext { ... }

public class FSMDefinitions
{
    public static void DefinePlayerCombatFSM()
    {
        // Define player combat FSM only if it doesn't already exist
        if (!FSM_API.Exists("PlayerCombatFSM"))
        {
            FSM_API.CreateFiniteStateMachine("PlayerCombatFSM",
                                             processRate: -1, // Updates every tick of its group
                                             processingGroup: "PlayerCombat") // Custom group for combat logic
                .State("Idle",
                    onEnter: (context) => Console.WriteLine($"{((MyContext)context).Name} entered Idle Combat."),
                    onUpdate: (context) => ((MyContext)context).CheckForCombatInput(),
                    onExit: (context) => Console.WriteLine($"{((MyContext)context).Name} exited Idle Combat."))
                .State("Attacking",
                    onEnter: (context) => ((MyContext)context).StartAttackAnimation(),
                    onUpdate: (context) => ((MyContext)context).ExecuteAttack(),
                    onExit: (context) => ((MyContext)context).EndAttackAnimation())
                .State("Dodging",
                    onEnter: (context) => ((MyContext)context).PlayDodgeAnimation(),
                    onUpdate: (context) => ((MyContext)context).PerformDodge(),
                    onExit: (context) => Console.WriteLine($"{((MyContext)context).Name} exited Dodging."))
                .WithInitialState("Idle")
                .Transition("Idle", "Attacking", (context) => ((MyContext)context).HasAttackInput())
                .Transition("Idle", "Dodging", (context) => ((MyContext)context).HasDodgeInput())
                .Transition("Attacking", "Idle", (context) => ((MyContext)context).AttackFinished())
                .Transition("Dodging", "Idle", (context) => ((MyContext)context).DodgeFinished())
                .BuildDefinition(); // Call this to finalize and register the FSM!
            Console.WriteLine("PlayerCombatFSM definition built and registered.");
        }
        else
        {
            Console.WriteLine("PlayerCombatFSM already exists, skipping definition.");
        }
    }
}

Best Practice: Always Check for Existence (FSM_API.Exists)

As demonstrated in the examples.
It's highly recommended to wrap your FSM definition logic.
Within an if (!FSM_API.Exists("YourFSMName")) check.

Why is this important?

    Preventing Redefinition Errors:
    While BuildDefinition() handles updates gracefully.
    Explicitly checking for existence makes your intent clearer.
    And can prevent unintentional re-definitions.
    Especially if defining FSMs in Unity's Awake() or OnEnable().
    Where they might be called multiple times.

    Performance:
    Defining an FSM blueprint involves some setup.
    (Object creation, list population).
    If you have many game objects.
    Or parts of your application that might trigger FSM definitions.
    Performing this check ensures the definition work only happens once.

    Clarity:
    It clearly communicates that this block of code.
    Is responsible for the initial definition.
    Of a unique FSM blueprint.

This pattern ensures that your FSM blueprint is set up.
Exactly once for the entire application lifetime.
Regardless of how many times the setup code might be invoked.

Conclusion

The FSMBuilder provides a clean, extensible way.
To declaratively define your Finite State Machines.
By mastering its fluent API and understanding each method.
You can construct complex and maintainable state logic.
For any part of your application.
Whether in Unity or a pure C# environment.
The practice of checking for FSM existence before defining.
Ensures robust and efficient setup.

Continue to explore how you can manage multiple FSMs.
Nested states, and other advanced features.
To build sophisticated state-driven behaviors.