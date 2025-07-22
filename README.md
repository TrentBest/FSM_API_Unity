# FSM_API (Unity Integration)

**Seamlessly integrate the blazing-fast, software-agnostic Finite State Machine system into your Unity projects.**

> Designed for flexibility. Built for robustness. Ready for anything.


---

## 🔍 Overview

**FSM_API** is a modular, runtime-safe, and fully event-aware Finite State Machine (FSM) system designed to plug directly into **any C# application**—from enterprise software to games, simulations, robotics, or reactive systems.

- ✅ **Thread-safe operations** (main thread only, deferred mutation handling)
- 🧠 **Decoupled state logic from data**
- 🏗️ **Define once, instantiate many**
- 🛠️ **Error-tolerant FSM lifecycle management**
- 🧪 **Dynamic update ticking with frame/process throttling**

**No Unity dependency. No frameworks required. No boilerplate setup.**

---

## 💡 Why FSM_API?

Typical FSM systems are tightly coupled to their target environment or force you into rigid coding patterns. FSM_API flips the paradigm:

| Feature                          | FSM_API ✅ | Traditional FSM ❌ |
|----------------------------------|------------|--------------------|
| Framework agnostic               | ✅         | ❌                 |
| Runtime-modifiable definitions   | ✅         | ❌                 |
| Deferred mutation safety         | ✅         | ❌                 |
| Named FSMs & Processing Groups   | ✅         | ❌                 |
| Built-in diagnostics & thresholds| ✅         | ❌                 |
| Pure C# with no external deps    | ✅         | ❌                 |

---

## 🚀 Quickstart (Non-Unity Example)

Define a simple context:

```csharp
public class LightSwitch : IStateContext
{
    public bool IsOn = false;
    public bool IsValid => true;
    public string Name { get; set; } = "KitchenLight";
}
 ````
Define and build your FSM:
```csharp
FSM_API.CreateProcessingGroup("MainLoop");

var fsm = FSM_API.CreateFiniteStateMachine("LightSwitchFSM", processRate: 1, processingGroup: "MainLoop")
    .State("Off")
        .OnEnter(ctx => { if (ctx is LightSwitch l) l.IsOn = false; })
        .TransitionIf("On", ctx => ctx is LightSwitch)
    .State("On")
        .OnEnter(ctx => { if (ctx is LightSwitch l) l.IsOn = true; })
        .TransitionIf("Off", ctx => ctx is LightSwitch l && !l.IsOn)
    .BuildDefinition();
 ````
Create an instance:
```csharp
var kitchenLight = new LightSwitch();
var handle = FSM_API.CreateInstance("LightSwitchFSM", kitchenLight, "MainLoop");
 ````
Tick the FSM from your main loop:
```csharp
FSM_API.Update("MainLoop");
 ````


| 🔧 Core Concepts        | Brief Summary                                                    |
|--------------------------|-----------------------------------------------------------------|
| FSMBuilder               | Fluently define states, transitions, enter/exit actions.        |
| FSMHandle                | Runtime instance of an FSM with full control.                   |
| IStateContext            | Any data model the FSM will operate upon.                       |
| Processing Groups        | Organize FSMs by update loop or system.                         |
| Error Handling           | Thresholds prevent runaway logic or invalid state contexts.     |
| Thread-Safe by Design    | All modifications are deferred and processed safely post-update.|
   

 
    📦 Features at a Glance
    Capability	Description
    🔄 State Transition Logic	Conditional or forced

    🎭 Context-Driven Behavior	FSM logic operates on your POCOs

    🧪 Update Control	Process every frame, every Nth frame, or event-driven

    🧯 Error Escalation	Per-instance and per-definition error tracking

    🔁 Runtime Redefinition	FSMs can be redefined while running

    🎯 Lightweight & Fast	Minimal allocations, optimized for performance

    🌐 Unity Integration	Dedicated helper methods and examples for Unity lifecycle


🔬 Core API Link	Built on the [FSM_API Core Library](https://github.com/TrentBest/FSM_API/README.md)

 

    📘 What’s Next?

    📖 Full Documentation & Wiki (TBD)

    🧪 Unit Tests & Benchmarks (Currently Under Development)

    🌐 Unity Package (.unitypackage) distribution 

    🎮 Unity Integration Examples (as a secondary layer)

    🔌 Plugins & Extension Framework

🤝 Contributing

Contributions welcome! Whether you're integrating into your engine, designing new extensions, or just fixing typos, PRs and issues are appreciated.
📄 [License](LICENSE.txt)

MIT License. Use it, hack it, build amazing things with it.
🧠 Brought to you by

The Singularity Workshop — Tools for the curious, the bold, and the systemically inclined.

<img src="https://github.com/user-attachments/assets/b94a9412-29f3-4b55-9d07-ddef3b57e082" width="200">    

    Because state shouldn't be a mess.
