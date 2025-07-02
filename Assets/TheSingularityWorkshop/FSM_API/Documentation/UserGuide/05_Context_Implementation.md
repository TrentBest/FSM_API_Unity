05. Understanding and Implementing Your Context (IStateContext)

In FSM_API, the Context is vital.
It's the link between your FSM and your game.
This document dives deep into IStateContext.
And its base, IContext.
You'll learn how to properly implement them.
For various types of objects.

What is a Context in FSM_API?

An FSM is designed to manage behavior.
But it needs to know what it's managing.
Your Context object is that "what."
It's the specific character, door, or system.
Whose state the FSM is controlling.

When an FSM's state actions run.
(Like onEnter or onUpdate).
They receive a reference to this Context.
This allows the FSM to read data.
Or call methods directly on your object.

Understanding IContext: The Basic Identity

IContext is the most fundamental interface.
It provides a basic way to identify any object.
That FSM_API might interact with.

What it requires:

    string Name { get; set; }:
    Every object implementing IContext must have a name.
    This name is used by FSM_API for logging.
    And for debugging purposes.
    It helps you track which FSM instance.
    Belongs to which object in your game.

Example of IContext definition (for reference):
C#

namespace TheSingularityWorkshop.FSM.API
{
    public interface IContext
    {
        // Every context needs a name for identification.
        string Name { get; set; }
    }
}

Understanding IStateContext: The FSM Connection

IStateContext builds upon IContext.
It adds a crucial requirement specifically for FSMs.
Any object you want an FSM to manage.
Must implement IStateContext.

What it requires (in addition to IContext's Name):

    bool IsValid { get; }:
    This property tells FSM_API if your context object.
    Is still active and should be processed.
    It's a critical safety check.

Example of IStateContext definition (for reference):
C#

namespace TheSingularityWorkshop.FSM.API
{
    public interface IStateContext : IContext // It inherits from IContext!
    {
        // This tells FSM_API if your object is still valid and active.
        bool IsValid { get; }
    }
}

Why IsValid is Important

The IsValid property is a powerful safety feature.
FSM_API checks IsValid before processing an FSM instance.
If IsValid returns false, FSM_API will automatically.
Stop updating that FSM instance.
And remove it from its processing group.

Benefits of IsValid:

    Prevents Errors: If a GameObject is destroyed.
    Or a pure C# object is no longer relevant.
    IsValid prevents FSMs from trying to access.
    Non-existent or disposed resources.

    Automatic Cleanup: FSM_API automatically cleans up.
    FSM instances tied to invalid contexts.
    Reducing memory usage and preventing leaks.

    Robustness: Your application remains stable.
    Even if objects are dynamically created or destroyed.

You define the logic for IsValid based on your object type.

Implementing IStateContext

Here are examples of how to implement IStateContext.
For common types of objects in Unity and pure C#.

1. IStateContext with a Unity MonoBehaviour

This is the most common way to use FSM_API in Unity.
Your script attaches to a GameObject.

Example PlayerCharacterContext.cs:
C#

// Add this line at the very top of your script.
using UnityEngine;
using TheSingularityWorkshop.FSM.API; // Needed for IStateContext

public class PlayerCharacterContext : MonoBehaviour, IStateContext
{
    // --- YOUR GAME'S DATA (Public fields show in Inspector) ---
    public float currentHealth = 100f; // Player's health.
    public bool isMoving = false;      // Is player currently moving?

    // --- IContext Requirement ---
    // The FSM_API uses this name for logs and debugging.
    public string Name { get; set; }

    // --- IStateContext Requirement ---
    // For a MonoBehaviour, we check if the script is active
    // and if its GameObject is active in the scene hierarchy.
    public bool IsValid => this != null && gameObject.activeInHierarchy;

    // --- Unity Lifecycle Method for Setup ---
    void Awake()
    {
        // Set the context's name to match the GameObject's name.
        Name = gameObject.name;
    }

    // --- Example Game Actions (Methods called by FSM states) ---
    public void StartMovement()
    {
        isMoving = true;
        Debug.Log($"{Name} started moving.");
    }

    public void StopMovement()
    {
        isMoving = false;
        Debug.Log($"{Name} stopped moving.");
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log($"{Name} took {amount} damage. Health: {currentHealth}");
    }
}

    Key Points for MonoBehaviour:

        Inherits from MonoBehaviour and IStateContext.

        Name is often set in Awake() to gameObject.name.

        IsValid checks this != null (if script exists) and gameObject.activeInHierarchy (if GameObject is active).

        Public fields (currentHealth, isMoving) appear in Unity Inspector.

2. IStateContext with a Pure C# Class

This is for FSMs managing logic not tied to Unity GameObjects.
Ideal for backend systems, data processing, or game logic.
That exists outside the visual scene.

Example GameLogicContext.cs:
C#

using System; // For Console.WriteLine
using TheSingularityWorkshop.FSM.API; // Needed for IStateContext

public class GameLogicContext : IStateContext
{
    // --- YOUR GAME'S DATA (Private fields with public properties) ---
    private bool _isGameRunning;
    private int _score;
    private bool _isActive = true; // Custom flag to control IsValid.

    public bool IsGameRunning { get { return _isGameRunning; } private set { _isGameRunning = value; } }
    public int Score { get { return _score; } private set { _score = value; } }

    // --- IContext Requirement ---
    // Name is set via the constructor.
    public string Name { get; set; }

    // --- IStateContext Requirement ---
    // For pure C#, IsValid depends on your custom logic.
    // Here, it's controlled by the _isActive flag.
    public bool IsValid => _isActive;

    // Constructor: Called when creating a new instance of this class.
    public GameLogicContext(string name, bool initialActiveState = true)
    {
        Name = name;
        _isActive = initialActiveState;
        Console.WriteLine($"{Name} context initialized. IsValid: {IsValid}");
    }

    // --- Example Game Actions (Methods called by FSM states) ---
    public void StartGame()
    {
        _isGameRunning = true;
        _score = 0;
        Console.WriteLine($"{Name}: Game Started. Score: {Score}");
    }

    public void EndGame()
    {
        _isGameRunning = false;
        Console.WriteLine($"{Name}: Game Ended. Final Score: {Score}");
    }

    public void AddScore(int points)
    {
        _score += points;
        Console.WriteLine($"{Name}: Score updated to {Score}");
    }

    // Method to manually set the active state of this context.
    public void SetContextActive(bool active)
    {
        _isActive = active;
        Console.WriteLine($"{Name}: Context active status set to {active}");
    }
}

    Key Points for Pure C# Class:

        Inherits only from IStateContext. No MonoBehaviour.

        Name is typically set in the class's constructor.

        IsValid is defined by your custom logic (e.g., an internal boolean flag, or checking if resources are loaded).

        Fields are often private with public properties for access.

        Output uses Console.WriteLine() instead of Debug.Log().

3. IStateContext with a Unity ScriptableObject

ScriptableObjects are Unity assets.
They exist outside the scene hierarchy.
But can hold data and logic.
They are great for global settings, data, or AI brains.
That don't need a GameObject.

Example EnemyBrainContext.cs:
C#

// Add this line at the very top of your script.
using UnityEngine;
using TheSingularityWorkshop.FSM.API; // Needed for IStateContext

// This attribute allows you to create this asset in Unity's Project window.
[CreateAssetMenu(fileName = "NewEnemyBrain", menuName = "FSM/Enemy Brain Context")]
public class EnemyBrainContext : ScriptableObject, IStateContext
{
    // --- YOUR GAME'S DATA (Public fields show in Inspector) ---
    public float aggressionLevel = 0.5f; // How aggressive this enemy brain is.
    public bool seesPlayer = false;      // Does this brain currently see the player?

    // --- IContext Requirement ---
    // ScriptableObjects have a 'name' property already.
    public string Name { get; set; } // We'll set this to the asset's name.

    // --- IStateContext Requirement ---
    // For ScriptableObjects, 'IsValid' might always be true,
    // or depend on a custom flag, or if the asset is loaded.
    public bool IsValid => this != null; // Simplest check: if the asset object exists.

    // --- Unity Lifecycle Method for Setup ---
    // OnEnable is called when the ScriptableObject is loaded or enabled.
    void OnEnable()
    {
        // Set the context's name to match the ScriptableObject asset's name.
        Name = this.name;
        Debug.Log($"EnemyBrainContext '{Name}' enabled. IsValid: {IsValid}");
    }

    // --- Example Game Actions (Methods called by FSM states) ---
    public void StartAggressiveBehavior()
    {
        Debug.Log($"{Name}: Initiating aggressive behavior!");
        // Logic to make enemy aggressive.
    }

    public void Retreat()
    {
        Debug.Log($"{Name}: Retreating to safety.");
        // Logic to make enemy retreat.
    }
}

    Key Points for ScriptableObject:

        Inherits from ScriptableObject and IStateContext.

        [CreateAssetMenu(...)]: Allows creating instances as assets in Unity.

        Name can be set in OnEnable() to this.name (the asset's name).

        IsValid might be simpler (this != null) as ScriptableObjects don't have gameObject.activeInHierarchy.

        Public fields appear in the Inspector when you select the asset.

Conclusion

Implementing IStateContext correctly is fundamental.
It ensures your FSMs can interact with your game objects.
Whether they are MonoBehaviours, pure C# classes, or ScriptableObjects.
By providing Name and IsValid, you give FSM_API.
The necessary information for robust state management.
Choose the context type that best fits your needs.
And enjoy the flexibility of FSM_API.