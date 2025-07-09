# FSM_API: Robust Finite State Machine for Unity & Beyond

A high-performance, modular, and **game-engine-agnostic** C#/.NET API for building and managing Finite State Machines (FSMs) in Unity and any C# application. FSM_API empowers developers to model behavior with clarity and efficiency — whether you're handling complex AI, player state transitions, UI flow, or reactive systems.

---

## 🎮 What Is a Finite State Machine (FSM)?

A Finite State Machine allows you to define:

- All possible **states** an object can be in (Idle, Moving, Attacking...)
- The **rules** for transitioning between those states
- The **actions** to perform when entering, updating, or exiting each state

FSMs simplify intricate behaviors into modular, testable logic units. Instead of tangled `if/else` trees, you get structured flow and readable logic.

---

## ✨ Why FSM_API?

- **🔧 Unified Logic Architecture**  
  Design all game logic through a central, fluent FSM definition system.

- **🧩 Engine-Agnostic, Unity-Optimized**  
  Built in pure C#/.NET with no Unity dependencies in the core. Easily integrates into any MonoBehaviour or GameObject via a clean interface.

- **🌀 Context-Driven Execution**  
  FSMs operate on your custom data structures through the `IStateContext` interface — no reflection, no boxing.

- **⚙️ Flexible Update Control**  
  Attach FSMs to Unity's `Update`, `FixedUpdate`, `LateUpdate`, or define custom update groups.

- **🚀 Fine-Tuned Performance**  
  Each FSM can:
  - Run every frame (`-1`)
  - React only to events (`0`)
  - Run every Nth frame (`>0`)

- **🧪 Test-Ready Architecture**  
  Clean separation of definition and instance enables straightforward unit testing — both inside and outside Unity.

- **🛡️ Built-In Error Handling**  
  All exceptions are captured via the `OnInternalApiError` event. Broken FSMs are removed gracefully without crashing your app.

- **♻️ Hot Swappable FSM Logic**  
  Update FSM definitions at runtime — instances will adopt new logic on their next tick.

---

## 🧠 Core Concepts

| Concept        | Description |
|----------------|-------------|
| `FSM_API`      | The global static manager. Defines, updates, and organizes FSMs. |
| `FSMBuilder`   | A fluent interface for creating FSM blueprints. |
| `FSM Definition` | A reusable FSM blueprint shared across instances. |
| `FSMHandle`    | A live instance tied to a specific game object or logic unit. |
| `IStateContext`| Your context class that FSMs operate on. Must implement `IsValid` and `Name`. |

---

## 🛠️ Quick Unity Integration

### 1. Implement a Context

```csharp
public class PlayerController : MonoBehaviour, IStateContext
{
    public float Health;
    public bool IsMoving;
    public bool IsAttacking;

    public string Name { get; set; }
    public bool IsValid => this != null && gameObject.activeInHierarchy;

    void Awake() => Name = gameObject.name;
}
```
2. Define an FSM
```csharp
public static class PlayerFSM
{
    public static void Register()
    {
        FSM_API.CreateFiniteStateMachine("PlayerFSM")
            .State("Idle",
                onEnter: ctx => Debug.Log($"{ctx.Name} is idle"))
            .State("Moving",
                onEnter: ctx => Debug.Log($"{ctx.Name} is moving"))
            .WithInitialState("Idle")
            .Transition("Idle", "Moving", ctx => ctx is PlayerController p && p.IsMoving)
            .Transition("Moving", "Idle", ctx => ctx is PlayerController p && !p.IsMoving)
            .BuildDefinition();
    }
}
```
3. Instantiate FSM on Awake
```csharp
public class PlayerController : MonoBehaviour, IStateContext
{
    private FSMHandle _fsm;

    void Awake()
    {
        Name = gameObject.name;
        _fsm = FSM_API.CreateInstance("PlayerFSM", this, "Update");
    }

    void OnDestroy()
    {
        FSM_API.Unregister(_fsm);
    }
}
```
4. Tick FSMs via a MonoBehaviour
```csharp
public class FSMManager : MonoBehaviour
{
    void Update() => FSM_API.Update("Update");
}
```
🔗 Structure & Portability

    ✅ Unity 2022+ compatible

    ✅ No UnityEngine references in core API

    ✅ Works in any .NET C# app (console, desktop, server, game engine)

    ✅ Designed for integration into ECS, VR, robotics, or simulations

🧪 Testing & Extensibility

    Supports standard C# test runners (NUnit, xUnit, etc.)

    Optional RNG module included for weighted transitions

    Easily extend FSM logic with your own wrappers or serializers

📄 License

This project is licensed under the MIT License.
See LICENSE.md for full terms.
✉️ Contact

Have questions or want to collaborate?

    Email: TheSingularityWorkshop@gmail.com

    GitHub Issues: https://github.com/yourname/FSM_API/issues

    FSM_API is precision-designed for developers who want control, clarity, and performance — without sacrificing flexibility.
