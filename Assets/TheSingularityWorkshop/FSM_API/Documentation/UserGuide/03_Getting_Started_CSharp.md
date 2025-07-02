Getting Started with FSM_API in Pure C#

This guide shows you how to use FSM_API with pure C# classes, entirely decoupled from Unity's MonoBehaviour lifecycle. This approach is ideal for managing application logic, AI, or data states in non-Unity contexts, or as a powerful backend for your Unity projects.

Follow these steps to get running quickly.

1. Define Your Pure C# Context

First, create a standard C# class. This class represents the "thing" your FSM will manage, such as a game character's internal state, a network connection, or a data processing pipeline.

Crucially, this pure C# class must implement IStateContext.

What is a "Context" in Pure C#?

In a pure C# environment, your context is simply an instance of a class that your FSM operates on. Unlike Unity's MonoBehaviour, this class doesn't automatically attach to anything or have built-in update loops.

Your context class will hold all the relevant data and methods for the entity it represents. For example, a GameCharacter class might manage health, inventory, or behavior methods.

How to Create Your Pure C# Context Class

    Create a New C# Class File:
    In your C# project (or even a simple console application), create a new .cs file. Name it something descriptive, like GameCharacterContext.cs.

    Define Your Class and Implement IStateContext:
    Open the new file and define your class. It needs to implement IStateContext and include using TheSingularityWorkshop.FSM.API; at the top. Since it's not a MonoBehaviour, there's no UnityEngine namespace needed here.
    C#

    // Add this line at the very top of your script.
    using System; // For Console.WriteLine and other basic C# features
    using TheSingularityWorkshop.FSM.API;

    // This is a pure C# class, not inheriting from MonoBehaviour.
    public class GameCharacterContext : IStateContext
    {
        // --- YOUR GAME'S DATA ---
        // These are private fields, typical for pure C# classes.
        private float _currentHealth = 100f; // Example: Character's health.
        private int _currentAmmo = 10;     // Example: Character's bullets.
        private float _moveSpeed = 5f;     // Example: How fast character moves.
        private bool _isActive = true;     // Custom flag for IsValid.
        private bool _isReviveCommandIssued = false; // For demonstration of input.

        // Public properties to access data from outside the class.
        public float CurrentHealth { get { return _currentHealth; } private set { _currentHealth = value; } }
        public int CurrentAmmo { get { return _currentAmmo; } private set { _currentAmmo = value; } }
        public float MoveSpeed { get { return _moveSpeed; } private set { _moveSpeed = value; } }

        // --- REQUIREMENTS FOR IContext (inherited by IStateContext) ---
        // This is a name for your object, useful for debugging FSMs.
        public string Name { get; set; }

        // --- REQUIREMENTS FOR IStateContext ---
        // FSM_API checks this to know if your object is still valid.
        // For a pure C# context, 'IsValid' typically reflects its operational status.
        public bool IsValid => _isActive; // Custom logic: true if character is conceptually active.

        // Constructor: Called when you create a new instance of this class.
        public GameCharacterContext(string name)
        {
            Name = name; // Set the FSM_API's context name.
            Console.WriteLine($"{Name} context created.");
        }

        // --- YOUR GAME'S ACTIONS (Methods) ---
        // These are actions your FSM might tell this object to do.
        public void TakeDamage(float amount)
        {
            _currentHealth -= amount;
            Console.WriteLine($"{Name} took {amount} damage. Health: {CurrentHealth}");
            // Add more game logic here, like triggering an event.
        }

        public void SetActive(bool active)
        {
            _isActive = active;
            Console.WriteLine($"{Name} active status set to {_isActive}.");
        }

        public void RestoreHealth(float amount)
        {
            _currentHealth += amount;
            if (_currentHealth > 100f) _currentHealth = 100f; // Cap health
            Console.WriteLine($"{Name} health restored to {CurrentHealth}.");
        }

        public void SimulateMovement()
        {
            // Example: Simulate movement for a character.
            Console.WriteLine($"{Name} is moving at speed {MoveSpeed}.");
        }

        public void PlaySound(string soundName)
        {
            // Example: Code to simulate playing a sound.
            Console.WriteLine($"{Name} playing sound: {soundName}");
        }

        // Method to simulate an "input" or external command for revival.
        public void IssueReviveCommand()
        {
            _isReviveCommandIssued = true;
            Console.WriteLine($"{Name}: Revive command issued.");
        }

        // Internal method to consume the revive command.
        public void ClearReviveCommand()
        {
            _isReviveCommandIssued = false;
        }

        // Property to check if a revive command is pending.
        public bool IsReviveCommandPending => _isReviveCommandIssued;
    }

    What's Happening Here?

        using System;: This is commonly included for console applications to use Console.WriteLine().

        using TheSingularityWorkshop.FSM.API;: This provides access to FSM_API interfaces and classes.

        public class GameCharacterContext : IStateContext: Notice there is no MonoBehaviour. This is a plain C# class implementing the IStateContext interface.

        private float _currentHealth;: Fields are often private in pure C# classes, with public properties (CurrentHealth) providing controlled access. This is good practice.

        public bool IsValid => _isActive;: Since there's no GameObject.activeInHierarchy, you define your own logic for IsValid. Here, it relies on a custom _isActive boolean flag which can be set by other parts of your application logic.

        public GameCharacterContext(string name): This is a constructor. It's a special method that runs automatically when you create a new instance of your class (e.g., new GameCharacterContext("Hero")). It's a perfect place to set initial properties like Name.

        public void TakeDamage(...), public void SimulateMovement(), etc.: These are regular C# methods that your FSM will call. Instead of Debug.Log, we use Console.WriteLine for output in a non-Unity environment.

2. Define and Create Your FSM

Now, let's define your FSM's blueprint and then create a live instance for your context. This logic would typically reside in your main application class or a dedicated manager.

    Add FSM_API Reference:
    Ensure using TheSingularityWorkshop.FSM.API; is at the top of the file where you define your FSM.

    FSMHandle Variable and Definition:
    You'll store the FSMHandle in a variable, just like in Unity. The FSM blueprint definition will also be similar, but using your pure C# context.
    C#

    // This could be within your main program class (e.g., Program.cs) or a game manager.
    // We'll put it in a conceptual "GameLoop" class for demonstration.

    public class PureCSharpGameLoop
    {
        private FSMHandle characterHealthFSM;
        private GameCharacterContext playerCharacter;

        public void SetupGame()
        {
            playerCharacter = new GameCharacterContext("Hero");

            // --- Define the FSM Blueprint (only once!) ---
            // We check if the FSM blueprint "CharacterHealthFSM" already exists.
            if (!FSM_API.Exists("CharacterHealthFSM"))
            {
                FSM_API.CreateFiniteStateMachine("CharacterHealthFSM",
                                                 processRate: 1, // Update every tick
                                                 processingGroup: "GameLogic") // Custom group name
                    .State("Healthy",
                        onEnter: (context) => ((GameCharacterContext)context).PlaySound("HappyTune"),
                        onUpdate: (context) => Console.WriteLine($"{((GameCharacterContext)context).Name} is Healthy. Health: {((GameCharacterContext)context).CurrentHealth}"),
                        onExit: (context) => ((GameCharacterContext)context).PlaySound("SadTune"))
                    .State("Damaged",
                        onEnter: (context) => ((GameCharacterContext)context).PlaySound("OuchSound"),
                        onUpdate: (context) => Console.WriteLine($"{((GameCharacterContext)context).Name} is Damaged. Health: {((GameCharacterContext)context).CurrentHealth}"),
                        onExit: null) // No specific action on exit
                    .State("CriticallyInjured",
                        onEnter: (context) => ((GameCharacterContext)context).PlaySound("WarningAlarm"),
                        onUpdate: (context) => Console.WriteLine($"{((GameCharacterContext)context).Name} is Critically Injured! Health: {((GameCharacterContext)context).CurrentHealth}"),
                        onExit: null)
                    .State("Dead",
                        onEnter: (context) => ((GameCharacterContext)context).PlaySound("DeathRattle"),
                        onUpdate: (context) => Console.WriteLine($"{((GameCharacterContext)context).Name} is Dead."),
                        onExit: (context) => ((GameCharacterContext)context).ClearReviveCommand()) // Clear command on exit from Dead

                    // Set "Healthy" as the starting state.
                    .WithInitialState("Healthy")

                    // Transitions based on health thresholds
                    .Transition("Healthy", "Damaged",
                        (context) => ((GameCharacterContext)context).CurrentHealth <= 75 && ((GameCharacterContext)context).CurrentHealth > 25)
                    .Transition("Damaged", "Healthy",
                        (context) => ((GameCharacterContext)context).CurrentHealth > 75)
                    .Transition("Damaged", "CriticallyInjured",
                        (context) => ((GameCharacterContext)context).CurrentHealth <= 25 && ((GameCharacterContext)context).CurrentHealth > 0)
                    .Transition("CriticallyInjured", "Damaged",
                        (context) => ((GameCharacterContext)context).CurrentHealth > 25 && ((GameCharacterContext)context).CurrentHealth <= 75)
                    .Transition("CriticallyInjured", "Dead",
                        (context) => ((GameCharacterContext)context).CurrentHealth <= 0)
                    .Transition("Dead", "Healthy",
                        (context) => ((GameCharacterContext)context).IsReviveCommandPending && ((GameCharacterContext)context).CurrentHealth > 0)

                    // IMPORTANT: Finalize and register this FSM blueprint.
                    .BuildDefinition();
            }

            // --- Create a Live FSM Instance for THIS Character ---
            // "playerCharacter" is our pure C# context instance.
            characterHealthFSM = FSM_API.CreateInstance("CharacterHealthFSM", playerCharacter);
            Console.WriteLine($"FSM for {playerCharacter.Name} initialized to state: {characterHealthFSM.CurrentState}");
        }

        // This method will simulate a game loop update.
        public void UpdateGameLogic()
        {
            // FSM_API.Update() advances all FSMs in the "GameLogic" group.
            FSM_API.Update("GameLogic");
        }

        public GameCharacterContext GetPlayerCharacter()
        {
            return playerCharacter;
        }

        public string GetCharacterFSMState()
        {
            return characterHealthFSM.CurrentState;
        }
    }

        What's Happening Here?

            playerCharacter = new GameCharacterContext("Hero");: We explicitly create an instance of our pure C# context using the new keyword. There's no GameObject involved.

            if (!FSM_API.Exists("CharacterHealthFSM")): This remains the same; we define the blueprint only once.

            processRate: 1, processingGroup: "GameLogic": These work identically to the Unity version, controlling how often this FSM updates when its group is "ticked."

            onEnter: (context) => ((GameCharacterContext)context).PlaySound("HappyTune"): The state actions and transition conditions still use lambda expressions to call methods on your context. You still need to cast the generic IStateContext to your specific GameCharacterContext type.

            characterHealthFSM = FSM_API.CreateInstance("CharacterHealthFSM", playerCharacter);: This line creates the live FSM instance, binding it to our specific playerCharacter object. The playerCharacter variable now holds the reference to our pure C# context.

            PureCSharpGameLoop class: This demonstrates how you might encapsulate your game's main logic. Its SetupGame method initializes the context and FSM, and UpdateGameLogic will be called repeatedly to simulate time passing.

3. "Tick" Your FSMs Manually

Since there are no built-in Unity Update() or FixedUpdate() methods, you must explicitly call FSM_API.Update() to advance your FSMs. This gives you complete control over when your state machines process.

You will typically have a main application loop or a manager class that orchestrates these updates.

Example: A Simple C# Game Loop

Let's create a Program.cs (or similar main entry point) that simulates a game loop, ticking the FSMs:
C#

using System;
using System.Threading; // For Thread.Sleep

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("--- Starting Pure C# FSM Demo ---");

        PureCSharpGameLoop game = new PureCSharpGameLoop();
        game.SetupGame(); // Initialize the character and its FSM

        GameCharacterContext player = game.GetPlayerCharacter();

        Console.WriteLine("\nSimulating game progression:");

        // Simulate some game ticks and events
        for (int i = 0; i < 20; i++)
        {
            Console.WriteLine($"\n--- Tick {i + 1} ---");

            // Simulate taking damage
            if (i == 2) player.TakeDamage(30); // Health: 70 -> Damaged
            if (i == 5) player.TakeDamage(20); // Health: 50 -> Still Damaged
            if (i == 8) player.TakeDamage(30); // Health: 20 -> CriticallyInjured
            if (i == 10) player.TakeDamage(20); // Health: 0 -> Dead

            // Simulate revive attempt
            if (i == 12) player.IssueReviveCommand(); // Issue command
            if (i == 13) player.RestoreHealth(100);    // Restore health

            game.UpdateGameLogic(); // Tick the FSM group

            Console.WriteLine($"Current FSM State: {game.GetCharacterFSMState()}");

            Thread.Sleep(500); // Wait for 500 milliseconds to slow down output
        }

        Console.WriteLine("\n--- Pure C# FSM Demo Finished ---");
    }
}

    What's Happening Here?

        PureCSharpGameLoop game = new PureCSharpGameLoop();: We create an instance of our game loop class.

        game.SetupGame();: This calls the setup method, which in turn creates our GameCharacterContext and defines/creates its FSM.

        for (int i = 0; i < 20; i++): This for loop simulates game "ticks" or frames. Each iteration is one step in our game's timeline.

        player.TakeDamage(amount);: We directly call methods on our player context instance to change its internal state, which will then be picked up by the FSM's transition conditions.

        player.IssueReviveCommand(); and player.RestoreHealth(100);: These are custom methods on our GameCharacterContext that simulate external actions (like player input or a healing spell) that might trigger FSM transitions.

        game.UpdateGameLogic();: This is the key line. It calls FSM_API.Update("GameLogic"), telling FSM_API to process all FSMs associated with the "GameLogic" group (which includes our CharacterHealthFSM). This is how your FSM advances.

        Thread.Sleep(500);: This is just to slow down the console output so you can observe the state changes more easily.

Running Your Pure C# FSM

To run this example:

    Create a New C# Console Application Project:
    In Visual Studio or your preferred C# IDE, create a new "Console Application" project.

    Add FSM_API Reference:
    You'll need to add a reference to the FSM_API library. This is usually done via NuGet package manager or by directly referencing the compiled FSM_API DLL.

    Add Your Code Files:
    Copy the GameCharacterContext.cs and Program.cs (containing PureCSharpGameLoop and Main method) into your project.

    Run the Application:
    Execute your console application. You will see the output in the console demonstrating the FSM transitioning between states based on the simulated health changes and revive commands.

Congratulations! You've successfully implemented and observed an FSM using FSM_API in a pure C# environment, proving its versatility beyond Unity-specific contexts.

[Continue to: Next Document (e.g., Advanced FSM Concepts)]