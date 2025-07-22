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
    🔧 Core Concepts
     [FSMBuilder]([FSMBuilder.md](https://github.com/TrentBest/FSM_API_Unity/blob/master/Assets/TheSingularityWorkshop/FSM_API/Documentation/UserGuide/04_FSM_Builder_Deep_Dive.md)) - Fluently define states, transitions, enter/exit actions.
     [FSMHandle]([FSMBuilder.md](https://github.com/TrentBest/FSM_API_Unity/blob/master/Assets/TheSingularityWorkshop/FSM_API/Documentation/UserGuide/04_FSM_Builder_Deep_Dive.md)) - Runtime instance of an FSM with full control.
     [IStateContext]([FSMBuilder.md](https://github.com/TrentBest/FSM_API_Unity/blob/master/Assets/TheSingularityWorkshop/FSM_API/Documentation/UserGuide/04_FSM_Builder_Deep_Dive.md)) - Any data model the FSM will operate upon.
    [Processing Groups]([FSMBuilder.md](https://github.com/TrentBest/FSM_API_Unity/blob/master/Assets/TheSingularityWorkshop/FSM_API/Documentation/UserGuide/04_FSM_Builder_Deep_Dive.md)) - Organize FSMs by update loop or system.
    [Error Handling]([FSMBuilder.md](https://github.com/TrentBest/FSM_API_Unity/blob/master/Assets/TheSingularityWorkshop/FSM_API/Documentation/UserGuide/04_FSM_Builder_Deep_Dive.md)) - Thresholds prevent runaway logic or invalid state contexts.
    [Thread-Safe by Design]([FSMBuilder.md](https://github.com/TrentBest/FSM_API_Unity/blob/master/Assets/TheSingularityWorkshop/FSM_API/Documentation/UserGuide/04_FSM_Builder_Deep_Dive.md)) - All modifications are deferred and processed safely post-update.

---
## 📦 Features at a Glance

| Capability                | Description                                                                                             |
| :------------------------ | :------------------------------------------------------------------------------------------------------ |
| 🔄 **State Transition Logic** | Effortlessly define state changes based on **dynamic conditions** or **explicit triggers**, ensuring your FSMs react precisely. |
| 🎭 **Context-Driven Behavior**| Your FSM logic directly operates on **any custom C# object (POCO)** that implements `IStateContext`, allowing clean separation of concerns. |
| 🧪 **Flexible Update Control** | Choose how FSMs are processed: **event-driven**, **tick-based** (every N frames), or **manual**, adapting to any application loop. |
| 🧯 **Robust Error Escalation**| Benefit from **per-instance and per-definition error tracking**, providing insights to prevent runaway logic or invalid states without crashing. |
| 🔁 **Runtime Redefinition** | Adapt your application on the fly! FSM definitions can be **redefined while actively running**, enabling dynamic updates and live patching. |
| 🎯 **Lightweight & Fast** | Engineered for **minimal memory allocations** and **optimized performance**, ensuring your FSMs are efficient even in demanding scenarios. |
| 🌐 **Seamless Unity Integration** | Utilize **dedicated helper methods** and **clear examples** tailored for Unity's lifecycle, making FSM management a breeze within the engine. |
| 🔬 **Core API Link** | This Unity integration is **built on the robust [FSM_API Core Library](https://github.com/TrentBest/FSM_API)**, providing a solid, platform-agnostic foundation. |
 
   
    	
    

    

    

    

    

    

    


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
