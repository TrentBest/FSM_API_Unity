# FSM_API: Robust Finite State Machine for Unity & Beyond

A powerful, high-performance, and **game-engine-agnostic** C# API for designing, managing, and running Finite State Machines in your Unity projects — and beyond. FSM_API gives you a clean, safe, and flexible way to drive everything from AI behaviors to UI flows, event systems, and gameplay logic.

Built for **clarity**, **modularity**, and **runtime resilience**, FSM_API is equally at home in Unity or any other .NET/C# application.

---

## ✨ Key Features

- 🧠 **Intuitive FSM Design**  
  Define FSMs fluently via builder syntax. Easy to read, easy to maintain.

- 🧩 **Game-Engine Agnostic Core**  
  While Unity-ready out of the box, FSM_API runs in **any C#/.NET environment** with zero dependencies on UnityEngine.

- 🌀 **Context-Based Logic**  
  FSMs operate directly on your own custom classes via the `IStateContext` interface. No inheritance chains. No boxing. No limitations.

- ⚙️ **Update Groups with Granular Control**  
  Choose `Update`, `FixedUpdate`, `LateUpdate`, or define your own named update groups. Set update rates per FSM:  
  - `-1`: every frame  
  - `0`: event-driven only  
  - `>0`: every N frames

- 🛡️ **Robust Error Handling**  
  All exceptions are captured and raised via `OnInternalApiError` for clean logging and runtime stability.

- 🧼 **Automatic Cleanup**  
  Invalid or stale FSM instances are cleaned up automatically, keeping your scene efficient and bug-free.

- 🎲 **Built-in RNG**  
  Includes `TheSingularityWorkshop.FSM.API.Utilities.RNG` — a compact, fast, and deterministic random number generator for AI transitions and more.

---

## 🚀 Getting Started in Unity

### 1. 📦 Import the Package  
After downloading from the Unity Asset Store, import the FSM_API package.  
The core files live in:  
Assets/TheSingularityWorkshop/FSM_API/


### 2. 🧠 Create a Context

Your FSMs operate on data classes that implement `IStateContext`. Example:

''''csharp
using TheSingularityWorkshop.FSM.API;

public class PlayerContext : IStateContext
{
    public bool IsValid { get; set; } = true;
    public string Name { get; set; }

    public float Health = 100f;
    public bool IsMoving = false;
    public bool IsAttacking = false;
    public int Ammo = 10;

    public PlayerContext(string name) => Name = name;
    public void Invalidate() => IsValid = false;
}
