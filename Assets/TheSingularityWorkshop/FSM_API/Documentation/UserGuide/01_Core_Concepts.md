Core Concepts: Your Guide to FSM_API

What is a Finite State Machine (FSM)?

Imagine anything in a game or app. It acts differently based
on its current "mode" or "stage."

    A character might be Idle, then Walking, then Jumping.

    A door might be Closed, then Opening, then Open.

    A UI button might be Enabled, then Pressed, then Disabled.

A Finite State Machine (FSM) is simply a way to manage these modes.
It's a system that can only be in one "state" at a time.
It has a limited, or "finite," number of possible states.
And it has clear rules for "transitions" (moving from one state to another).

Think of it like a light switch:

    It has two states: On and Off.

    There's a transition from Off to On (flipping the switch up).

    There's a transition from On to Off (flipping the switch down).

    It cannot be "half-on" or "both-on-and-off." It's one state at a time.

FSMs help you organize complex behaviors cleanly.
They make it easy to understand what's happening.
And predict what will happen next.

FSM_API helps you build and manage these powerful systems.

Understanding FSM_API's Core Concepts

Now that you know what an FSM is, let's look at FSM_API's pieces.
Each concept below plays a distinct role in FSM_API's architecture.

FSM Definition: The Blueprint

Think of an FSM Definition as the complete plan or blueprint.
It defines how a certain behavior should work.

    What it is:
    This blueprint lays out all the "stages" your character, item, or
    game logic can be in (like "Walking," "Sleeping," or "Door Open").
    It also defines the rules for moving between these stages.
    And what actions happen in each stage.

    How you create it:
    You create this blueprint using a special tool called the FSMBuilder.
    You get the FSMBuilder by simply typing FSM_API.CreateFiniteStateMachine("YourFSMName").
    Then, you use easy, chained commands.
    Add your stages (states), describe what happens in them.
    And set up the rules for moving between them.

    Why it matters:
    Once you have this blueprint, you can use it many times!
    For example, define "Enemy AI" once.
    Then use that same blueprint to control hundreds of different enemies.
    Each acts on its own. It saves you from repeating work.

FSM Instance (FSMHandle): The Live Version

If an FSM Definition is the blueprint, an FSM Instance is
the actual, live "thing" that's running in your game or application.
It's a specific character actually walking.
A specific door actually opening.
Or specific game logic actually deciding what to do next.

    What it is:
    This is a live, active version of your FSM blueprint.
    It's always connected to a specific "thing" in your game.
    (Like a player character, a monster, or even a button).

    Purpose:
    The FSMHandle is your main way to talk to.
    And control one of these live FSMs.
    It tells you what state the FSM is currently in (e.g., "walking").
    And you can use it to force the FSM to change its state if needed.

    How it's created:
    Once you've built your FSM Definition (the blueprint),
    you create a live instance by calling FSM_API.CreateInstance("YourFSMName", yourObject).
    The yourObject part is very important.
    It's the specific thing in your game that this FSM will control.

    Key Point:
    Multiple FSM Instances can share the same FSM Definition.
    But operate entirely independently.
    Each manages its own current stage and actions.
    Even though they all follow the same initial blueprint.

State: The "What's Happening Now"

A State is simply a named stage or phase that your FSM can be in.
It's the "what's happening right now" for the thing your FSM is controlling.

    What it is:
    A single, distinct phase like "Idle," "Moving," "Attacking,"
    "Door_Closed," or "UI_Enabled."

    How you define its actions (what happens):
    This is where FSM_API is really easy!
    You don't need to create separate "State" files or classes.
    Instead, when you use the FSMBuilder to create your FSM Definition,
    you directly tell it what code should run for each named state:

        onEnter: This is code that runs once.
        The moment the FSM enters this state.
        Perfect for starting an animation or playing a sound.

        onUpdate: This is code that runs repeatedly.
        As long as the FSM stays in this state.
        Good for things that happen over time, like moving a character.

        onExit: This is code that runs once.
        The moment the FSM leaves this state.
        Useful for stopping animations or cleaning up.

    You provide these actions as small pieces of code.
    (Often called "lambdas" or "methods") directly when building your FSM.

    Example of defining state actions (you'll see more in the FSMBuilder guide):
    C#
```csharp
    // Imagine this is part of your FSM setup
    .State("Idle",
        onEnter: (context) => { /* Code to start Idle animation */ },
        onUpdate: (context) => { /* Code to check for player input */ },
        onExit: (context) => { /* Code to stop Idle animation */ })
```
    Don't worry about context for now; we'll explain it next!

State Context (IStateContext): Your Data Connection

The State Context is simply your object.
The actual thing in your game that the FSM is controlling.
It's how the FSM can "talk" to your game's data and features.

    What it is:
    This is your own class or script from your game.
    (Like a PlayerController script on your player character, or a DoorManager script on a door).
    The FSM needs this context to know what it's supposed to be influencing.

    Purpose:
    The FSM itself is generic.
    It doesn't know about "player health" or "door position."
    It needs your context object to get that information.
    Your state actions (onEnter, onUpdate, onExit) will use this context.
    To read data (e.g., "is the door open?") or make changes (e.g., "open the door").

    How you use it: Implementing the IStateContext Interface
    For your game object or script to be used as a context,
    it must follow a small "contract" by implementing the IStateContext interface.
    This interface ensures your object provides two key pieces of information
    that FSM_API needs: its Name and whether it is Valid.

    The IStateContext actually builds upon an even simpler contract, IContext,
    which just provides a Name. IStateContext then adds the IsValid check.

    Interface Details (What your object must provide):
    C#
```csharp
public interface IContext
{
    // Your object must provide a 'Name'. This helps debugging!
    string Name { get; set; }
}

public interface IStateContext : IContext // It inherits from IContext!
{
    // Your object must tell FSM_API if it's still valid.
    // For example, if a game character is destroyed, IsValid might become 'false'.
    bool IsValid { get; }
}
```
Example of what your class looks like (you'll fill in your own game details):
C#
```csharp
// You would add this to YOUR script/class in Unity
using UnityEngine; // Needed for MonoBehaviour

public class MyPlayerScript : MonoBehaviour, IStateContext
{
    // --- Your game-specific data and features ---
    // Public FIELDS like these will show up directly in the Unity Inspector.
    public float PlayerHealth; // Example: Current health of the player.
    public bool HasJumpInput;  // Example: Set this when player presses jump button.

    // --- IContext requires this ---
    // FSM_API will use this name for debugging and identifying this object.
    public string Name { get; set; }

    // --- IStateContext requires this ---
    // This tells FSM_API if your object is still active and okay to process.
    // For Unity, we check if the GameObject is active in the game world.
    public bool IsValid => this != null && gameObject.activeInHierarchy;

    // Your specific game methods that states will call
    public void PerformJump() { /* ... jump logic here ... */ }
    public void PlayAnimation(string animName) { /* ... */ }

    void Awake() // Unity specific method to set up the Name when the object loads
    {
        Name = gameObject.name; // Automatically sets the name to your GameObject's name.
    }

    // Example: Method you might call from Unity's Update() or an input system.
    public void SetJumpInput(bool input)
    {
        HasJumpInput = input;
    }
}
```
When writing your state actions, you'll often "tell" the context
what kind of object it really is (e.g., (MyPlayerScript)context).
This lets you access all your custom stuff, like ((MyPlayerScript)context).PerformJump().
We'll show this in examples in later documents.

Continue:  [Getting Started with Unity](02_Getting_Started_Unity.md)
