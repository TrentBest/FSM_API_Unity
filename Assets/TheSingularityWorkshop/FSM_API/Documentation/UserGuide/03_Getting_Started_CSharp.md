# 03. Getting Started with FSM_API in Pure C#

> Use FSM_API with pure C# classes — completely decoupled from Unity — for backend logic, simulations, AI, networking, or any state-based system.

This guide shows how to use FSM_API in non-Unity environments (console, services, backends, robotics, etc.) with clean, minimal dependencies.

---

## 🎯 What This Example Will Demonstrate

We’ll simulate a basic character health lifecycle using FSM_API — all in **pure C#**.

This example illustrates:

- How a game character transitions through health-related states (`Healthy`, `Damaged`, `Critically Injured`, `Dead`)
- How FSM_API makes these transitions declarative, clean, and maintainable
- How to manually control the "ticking" of FSMs without relying on Unity’s game loop
- How FSMs can be revived and continue operating even after simulated "death"

### 💡 Simulation Flow

1. The player starts in the **Healthy** state.
2. Over several ticks, they take damage, triggering transitions to:
   - **Damaged**
   - **Critically Injured**
   - **Dead**
3. After death, a revive command is issued.
4. Health is restored.
5. The FSM automatically transitions back to **Healthy**.

All output is printed to the console to simulate animation, audio, and event feedback — ideal for unit tests or backend game systems.

---

## ✅ Step 1: Define Your Pure C# Context

Create a standard C# class to represent the "thing" your FSM will manage — like a character, connection, pipeline, or subsystem.

It must implement the `IStateContext` interface.

### 🛠 Example: `GameCharacterContext.cs`

```csharp
using System;
using TheSingularityWorkshop.FSM.API;

public class GameCharacterContext : IStateContext
{
    private float _currentHealth = 100f;
    private int _currentAmmo = 10;
    private float _moveSpeed = 5f;
    private bool _isActive = true;
    private bool _isReviveCommandIssued = false;

    public float CurrentHealth => _currentHealth;
    public int CurrentAmmo => _currentAmmo;
    public float MoveSpeed => _moveSpeed;

    public string Name { get; set; }
    public bool IsValid => _isActive;

    public GameCharacterContext(string name)
    {
        Name = name;
        Console.WriteLine($"{Name} context created.");
    }

    public void TakeDamage(float amount)
    {
        _currentHealth -= amount;
        Console.WriteLine($"{Name} took {amount} damage. Health: {CurrentHealth}");
    }

    public void SetActive(bool active)
    {
        _isActive = active;
        Console.WriteLine($"{Name} active status set to {_isActive}.");
    }

    public void RestoreHealth(float amount)
    {
        _currentHealth += amount;
        if (_currentHealth > 100f) _currentHealth = 100f;
        Console.WriteLine($"{Name} health restored to {CurrentHealth}.");
    }

    public void SimulateMovement()
    {
        Console.WriteLine($"{Name} is moving at speed {MoveSpeed}.");
    }

    public void PlaySound(string soundName)
    {
        Console.WriteLine($"{Name} playing sound: {soundName}");
    }

    public void IssueReviveCommand() => _isReviveCommandIssued = true;
    public void ClearReviveCommand() => _isReviveCommandIssued = false;
    public bool IsReviveCommandPending => _isReviveCommandIssued;
}
```
✅ Step 2: Define and Create the FSM
🎯 Setup FSM and Link to Context
```csharp
using System;
using TheSingularityWorkshop.FSM.API;

public class PureCSharpGameLoop
{
    private FSMHandle characterHealthFSM;
    private GameCharacterContext playerCharacter;

    public void SetupGame()
    {
        playerCharacter = new GameCharacterContext("Hero");

        if (!FSM_API.Exists("CharacterHealthFSM"))
        {
            FSM_API.CreateFiniteStateMachine("CharacterHealthFSM", processRate: 1, processingGroup: "GameLogic")
                .State("Healthy",
                    onEnter: ctx => ((GameCharacterContext)ctx).PlaySound("HappyTune"),
                    onUpdate: ctx => Console.WriteLine($"{ctx.Name} is Healthy. Health: {((GameCharacterContext)ctx).CurrentHealth}"),
                    onExit: ctx => ((GameCharacterContext)ctx).PlaySound("SadTune"))
                .State("Damaged",
                    onEnter: ctx => ((GameCharacterContext)ctx).PlaySound("OuchSound"),
                    onUpdate: ctx => Console.WriteLine($"{ctx.Name} is Damaged. Health: {((GameCharacterContext)ctx).CurrentHealth}"))
                .State("CriticallyInjured",
                    onEnter: ctx => ((GameCharacterContext)ctx).PlaySound("WarningAlarm"),
                    onUpdate: ctx => Console.WriteLine($"{ctx.Name} is Critically Injured!"))
                .State("Dead",
                    onEnter: ctx => ((GameCharacterContext)ctx).PlaySound("DeathRattle"),
                    onUpdate: ctx => Console.WriteLine($"{ctx.Name} is Dead."),
                    onExit: ctx => ((GameCharacterContext)ctx).ClearReviveCommand())
                .WithInitialState("Healthy")
                .Transition("Healthy", "Damaged", ctx => ((GameCharacterContext)ctx).CurrentHealth <= 75 && ((GameCharacterContext)ctx).CurrentHealth > 25)
                .Transition("Damaged", "Healthy", ctx => ((GameCharacterContext)ctx).CurrentHealth > 75)
                .Transition("Damaged", "CriticallyInjured", ctx => ((GameCharacterContext)ctx).CurrentHealth <= 25 && ((GameCharacterContext)ctx).CurrentHealth > 0)
                .Transition("CriticallyInjured", "Damaged", ctx => ((GameCharacterContext)ctx).CurrentHealth > 25 && ((GameCharacterContext)ctx).CurrentHealth <= 75)
                .Transition("CriticallyInjured", "Dead", ctx => ((GameCharacterContext)ctx).CurrentHealth <= 0)
                .Transition("Dead", "Healthy", ctx => ((GameCharacterContext)ctx).IsReviveCommandPending && ((GameCharacterContext)ctx).CurrentHealth > 0)
                .BuildDefinition();
        }

        characterHealthFSM = FSM_API.CreateInstance("CharacterHealthFSM", playerCharacter);
        Console.WriteLine($"FSM for {playerCharacter.Name} initialized to state: {characterHealthFSM.CurrentState}");
    }

    public void UpdateGameLogic() => FSM_API.Update("GameLogic");
    public GameCharacterContext GetPlayerCharacter() => playerCharacter;
    public string GetCharacterFSMState() => characterHealthFSM.CurrentState;
}
```
✅ Step 3: Manual Update Loop

Since you don’t have Unity’s Update(), you'll tick the FSM manually in your main application loop.
🕹 Example: Simulated Game Loop
```csharp
using System;
using System.Threading;

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("--- Starting Pure C# FSM Demo ---");

        var game = new PureCSharpGameLoop();
        game.SetupGame();
        var player = game.GetPlayerCharacter();

        for (int i = 0; i < 20; i++)
        {
            Console.WriteLine($"\n--- Tick {i + 1} ---");

            if (i == 2) player.TakeDamage(30);   // Health: 70 → Damaged
            if (i == 5) player.TakeDamage(20);   // Health: 50 → Still Damaged
            if (i == 8) player.TakeDamage(30);   // Health: 20 → Critically Injured
            if (i == 10) player.TakeDamage(20);  // Health: 0 → Dead
            if (i == 12) player.IssueReviveCommand();
            if (i == 13) player.RestoreHealth(100);

            game.UpdateGameLogic();

            Console.WriteLine($"Current FSM State: {game.GetCharacterFSMState()}");
            Thread.Sleep(500); // Optional: slow output
        }

        Console.WriteLine("\n--- FSM Demo Finished ---");
    }
}
```
🧪 Running the Example

    Create a Console App Project in your IDE (e.g., Visual Studio)

    Add FSM_API Reference

        via NuGet (if published) or manually via .dll reference

    Add Code Files

        GameCharacterContext.cs

        PureCSharpGameLoop.cs

        Program.cs

    Run the App and observe the FSM transitions and output

✅ Summary

    FSM_API works seamlessly outside of Unity

    You define a class that implements IStateContext

    FSM logic and transitions are identical to Unity usage

    You control when FSMs update via FSM_API.Update("Group")

    This makes FSM_API ideal for servers, simulations, AI systems, and custom engines

[➡️ Continue to: 04 FSM Builder Deep Dive](04_FSM_Builder_Deep_Dive.md)
