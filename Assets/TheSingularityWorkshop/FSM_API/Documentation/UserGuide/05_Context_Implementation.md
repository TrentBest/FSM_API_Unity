05. Understanding and Implementing Your Context (IStateContext)

In the FSM_API system, the Context is a vital component. It serves as the essential link between your Finite State Machine (FSM) logic and the specific game objects or systems it controls. This document dives deep into the IStateContext interface, and its foundational base, IContext, guiding you on how to properly implement them for various types of objects within your application.

What is a Context in FSM_API?

An FSM is designed to manage complex behaviors, but it requires a clear understanding of what it is managing. Your Context object provides this "what." It is the specific character, door, enemy AI, or backend system whose state the FSM is controlling.

When an FSM's state actions run (such as onEnter or onUpdate), they receive a direct reference to this Context object. This allows your FSM to seamlessly read data from, or call methods directly on, the specific object it is responsible for. This direct connection ensures tight integration between your FSM logic and your game's runtime data.

Understanding IContext: The Basic Identity

IContext is the most fundamental interface within the FSM_API for identifying any object that the system might interact with.

What it requires:

    string Name { get; set; }:
    Every object implementing IContext must have a name. This Name property is primarily used by FSM_API for internal logging, debugging, and error reporting. It helps you efficiently track which FSM instance belongs to which object in your game, especially useful in complex scenes or systems.

Example of IContext definition (for reference):
C#
```csharp
namespace TheSingularityWorkshop.FSM.API
{
    public interface IContext
    {
        // Every context needs a name for identification in logs and debugging.
        string Name { get; set; }
    }
}
```
Understanding IStateContext: The FSM Connection

IStateContext builds directly upon IContext, adding a crucial requirement specifically for objects that will be managed by an FSM. Any object you intend for an FSM to control must implement IStateContext.

What it requires (in addition to IContext's Name):

    bool IsValid { get; }:
    This property is a critical safety and lifecycle check. It informs FSM_API whether your context object is still active, relevant, and should continue to be processed.

Example of IStateContext definition (for reference):
C#
```csharp
namespace TheSingularityWorkshop.FSM.API
{
    public interface IStateContext : IContext // It inherits from IContext!
    {
        // This tells FSM_API if your object is still valid and active.
        // If false, the FSM instance will be automatically cleaned up.
        bool IsValid { get; }
    }
}
```
Why IsValid is Important

The IsValid property is a powerful, built-in safety feature of FSM_API. The system checks IsValid before each processing tick of an FSM instance. If IsValid returns false, FSM_API will automatically:

    Stop updating that FSM instance.

    Remove it from its processing group.

    Perform necessary cleanup.

Benefits of IsValid:

    Prevents Errors: If a GameObject is destroyed in Unity, or a pure C# object becomes irrelevant or disposed, IsValid prevents FSMs from attempting to access non-existent or invalid resources, averting NullReferenceExceptions and other runtime errors.

    Automatic Cleanup: FSM_API handles the automatic cleanup of FSM instances tied to invalid contexts. This significantly reduces potential memory usage and prevents resource leaks as objects are dynamically created and destroyed.

    Robustness: Your application remains stable and resilient, even in complex scenarios where objects are frequently added or removed from the game world.

You define the specific logic for IsValid based on the lifecycle and nature of your object type.

Implementing IStateContext

Here are comprehensive examples demonstrating how to implement IStateContext for common types of objects in both pure C# environments and within Unity. FSM_API is highly flexible and can seamlessly intermix FSMs controlling different context types within the same processing group.

1. IStateContext with a Pure C# Class

This implementation is ideal for FSMs managing backend logic, data processing, or game systems that are not directly tied to Unity's GameObject hierarchy. These FSMs exist independently of the visual scene.

Example: GameLogicContext.cs
C#
```csharp
using System; // For Console.WriteLine
using TheSingularityWorkshop.FSM.API; // Needed for IStateContext

public class GameLogicContext : IStateContext
{
    // --- YOUR GAME'S DATA (Private fields with public properties for encapsulation) ---
    private bool _isGameRunning;
    private int _score;
    // A custom flag to control the lifecycle of this pure C# context.
    private bool _isActive = true; 

    public bool IsGameRunning { get { return _isGameRunning; } private set { _isGameRunning = value; } }
    public int Score { get { return _score; } private set { _score = value; } }

    // --- IContext Requirement: Name property ---
    // For pure C# contexts, the Name is typically set via the constructor.
    public string Name { get; set; }

    // --- IStateContext Requirement: IsValid property ---
    // For pure C# objects, IsValid depends entirely on your custom logic.
    // Here, it's controlled by the internal _isActive flag.
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

    // Method to manually set the active state of this context from outside.
    public void SetContextActive(bool active)
    {
        _isActive = active;
        Console.WriteLine($"{Name}: Context active status set to {active}");
    }
}
```
Key Points for Pure C# Class:

    Inherits solely from IStateContext. Does not inherit from MonoBehaviour or ScriptableObject.

    Name is typically assigned when the class instance is created, often via its constructor.

    IsValid is entirely defined by your custom logic (e.g., an internal boolean flag, a check against resource loading status, or whether dependent systems are running).

    Fields are often private with public properties to control access, adhering to good programming practices.

    Outputs use Console.WriteLine() for non-Unity environments.

2. IStateContext with a Unity MonoBehaviour

This is the most common way to integrate FSM_API within Unity projects. Your context script attaches directly to a GameObject in your scene, representing a character, prop, or interactive element.

Example: PlayerCharacterContext.cs
C#
```csharp
using UnityEngine; // Required for MonoBehaviour and Debug.Log
using TheSingularityWorkshop.FSM.API; // Needed for IStateContext

public class PlayerCharacterContext : MonoBehaviour, IStateContext
{
    // --- YOUR GAME'S DATA (Public fields for easy Inspector access) ---
    public float currentHealth = 100f; // Player's health.
    public bool isMoving = false;      // Is player currently moving?

    // --- IContext Requirement: Name property ---
    // FSM_API uses this name for logs and debugging.
    public string Name { get; set; }

    // --- IStateContext Requirement: IsValid property ---
    // For a MonoBehaviour, IsValid checks if the script instance itself exists
    // and if its GameObject is currently active in the scene hierarchy.
    public bool IsValid => this != null && gameObject.activeInHierarchy;

    // --- Unity Lifecycle Method for Setup ---
    void Awake()
    {
        // Set the context's name to match the GameObject's name for easy identification.
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
```
Key Points for MonoBehaviour:

    Inherits from both MonoBehaviour and IStateContext.

    Name is typically set in Awake() to gameObject.name to link the FSM instance to its visual representation.

    IsValid generally checks this != null (ensuring the script instance hasn't been destroyed) and gameObject.activeInHierarchy (ensuring its associated GameObject is enabled and active in the scene).

    public fields (like currentHealth, isMoving) will automatically appear in the Unity Inspector for easy tweaking.

    Outputs use Debug.Log() for Unity's console.

3. IStateContext with a Unity ScriptableObject

ScriptableObjects are Unity assets that exist independently of the scene hierarchy but can hold data and logic. They are excellent for managing global settings, shared data, or AI "brains" that don't need a direct GameObject presence in the scene.

Example: EnemyBrainContext.cs
C#
```csharp
using UnityEngine; // Required for ScriptableObject and Debug.Log
using TheSingularityWorkshop.FSM.API; // Needed for IStateContext

// This attribute allows you to create instances of this asset directly
// from Unity's Project window (Assets > Create > FSM > Enemy Brain Context).
[CreateAssetMenu(fileName = "NewEnemyBrain", menuName = "FSM/Enemy Brain Context")]
public class EnemyBrainContext : ScriptableObject, IStateContext
{
    // --- YOUR GAME'S DATA (Public fields for Inspector access when asset is selected) ---
    public float aggressionLevel = 0.5f; // How aggressive this enemy brain is.
    public bool seesPlayer = false;      // Does this brain currently see the player?

    // --- IContext Requirement: Name property ---
    // ScriptableObjects already have a built-in 'name' property (the asset's file name).
    public string Name { get; set; } // We'll set this to the asset's name.

    // --- IStateContext Requirement: IsValid property ---
    // For ScriptableObjects, 'IsValid' is often simpler, as they don't have GameObjects.
    // The simplest check is if the asset object itself exists.
    public bool IsValid => this != null; 

    // --- Unity Lifecycle Method for Setup ---
    // OnEnable is called when the ScriptableObject is loaded or enabled (e.g., when selected in Project view).
    void OnEnable()
    {
        // Set the context's name to match the ScriptableObject asset's name for clarity.
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
```
Key Points for ScriptableObject:

    Inherits from ScriptableObject and IStateContext.

    The [CreateAssetMenu(...)] attribute allows you to easily create instances of this class as assets in your Unity Project window.

    Name can be set in OnEnable() to this.name, which corresponds to the asset's file name in the project.

    IsValid is typically simpler (e.g., this != null) as ScriptableObjects do not have gameObject.activeInHierarchy status.

    public fields will appear in the Inspector when you select the ScriptableObject asset in your Project window.

    Outputs use Debug.Log() for Unity's console.

Conclusion

Implementing IStateContext correctly is fundamental to effectively using FSM_API. It establishes the necessary communication bridge between your FSMs and your game's underlying objects and systems. By providing the Name for identification and the IsValid status for lifecycle management, you empower FSM_API to robustly manage state-driven behaviors across diverse object types—whether they are pure C# classes, Unity MonoBehaviours, or ScriptableObjects. Choose the context type that best fits the nature of the entity or system your FSM is intended to control, and enjoy the flexibility and power of FSM_API.

➡️ Continue to: 06_FSM_API_Update_and_Management.md
