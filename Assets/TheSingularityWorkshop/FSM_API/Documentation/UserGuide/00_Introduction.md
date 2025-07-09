# 00. Introduction to FSM_API

---

## 📚 Table of Contents

1. [00. Introduction to FSM_API](#00-introduction-to-fsm_api)
2. [01. Core Concepts: Your Guide to FSM_API](#)
3. [02. Getting Started with Unity](#)
4. [03. Getting Started with C# (Non-Unity)](#)
5. [04. FSMBuilder Deep Dive: Building Your FSMs](#)
6. [05. Understanding and Implementing Your Context (`IStateContext`)](#)
7. [06. RNG Utility: Adding Randomness to Your FSMs](#)
8. [07. Robust Error Handling: The Fubar System](#)
9. [08. Performance Tips & Best Practices](#)
10. [09. Common Use Cases & Examples](#)
11. [10. FSM_API for Non-Coders: A Big Picture Overview](#)
12. [11. Frequently Asked Questions (FAQ)](#)

---

## 🧠 What is FSM_API?

**FSM_API** is a robust, modular, and game-engine-agnostic C#/.NET API designed to make building and managing Finite State Machines (FSMs) effortless — whether you're working in **Unity**, **Godot**, a **custom engine**, or any other **C#/.NET application**.

Whether you're creating complex AI, dynamic character behaviors, interactive UI states, or intricate game logic, **FSM_API empowers you to do so with clarity and confidence.**

---

## ✅ Why Choose FSM_API?

### 🔧 Simplifies Complexity
FSM_API streamlines the process of designing and managing state-driven systems. You can focus on your **creative vision** instead of boilerplate code.

### 🔗 Versatile & Modular – Truly Engine-Agnostic
Built for **maximum flexibility**, FSM_API allows you to define **custom processing groups** (e.g., `"AI_Tick"`, `"UI_Animation"`, `"Physics_Update"`).  
You control exactly when and where each group of FSMs updates via a single method:

```csharp
FSM_API.Update("MyProcessingGroup");
```

This gives you seamless integration with **game loops**, **custom simulations**, and even **nested FSM hierarchies** — all with **total control**.

### 🚀 Performance-Driven
FSM_API is designed for **efficiency** and **responsiveness**, even when managing **hundreds or thousands** of active state machines.

### 🧼 Clarity & Maintainability
Designed for **readability and elegance**, FSM_API helps teams collaborate with ease and maintain codebases confidently.

### 🧪 Testability
FSM_API’s clean **separation of logic and context** makes it easy to write **automated unit tests** for your state machines.

---

## 🔍 Core Components of FSM_API

Here's a breakdown of the main tools you'll use when working with FSM_API:

### 🧠 `FSM_API` — The Central Manager
The master orchestrator.

- Use `FSM_API.CreateFiniteStateMachine()` to define FSM blueprints.
- Use `FSM_API.CreateInstance()` to spawn live instances.
- Use `FSM_API.Update("YourGroup")` to drive your FSMs.

💡 Think of it as: **The big boss** that organizes and runs everything.

---

### ✏️ `FSMBuilder` — The Blueprint Designer
Use this fluent, chainable interface to:

- Define states
- Create transitions
- Configure behavior
- Set initial state

💡 Think of it as: **Your drawing board** for building logic.

---

### 🧬 FSM Definition — The Blueprint
Once you've called `BuildDefinition()` on an FSMBuilder, you get an **FSM Definition**:

- Reusable logic for one behavior pattern
- Shared across multiple live instances

💡 Think of it as: **Your master plan**, reusable and consistent.

---

### ⚙️ `FSMHandle` — The Live Instance
Each FSMHandle represents a **live FSM instance**, bound to a specific data context:

- Maintains `CurrentState`
- Updates logic and transitions
- Operates on your `IStateContext` object

💡 Think of it as: **The live performer** of your FSM script.

---

### 🧍 `IStateContext` — Your Game Object / Script
This is **your class**, where the actual game data and logic lives.

- Implements `IStateContext` (and `IContext`)
- Represents characters, UI panels, doors, anything
- FSMs use this to read/write data and trigger behavior

💡 Think of it as: **The actor’s brain and body**, where things happen.

---

## 🔁 Flow of Interaction

```text
FSM_API
  └── CreateFiniteStateMachine() → FSMBuilder
        └── BuildDefinition() → FSM Definition
              └── CreateInstance() → FSMHandle
                    └── Interacts with → IStateContext (Your game object)
```

---

## 🧭 State Management Modes

FSM_API supports **two primary paradigms** for updating FSMs:

### 🕰 Frame-Based FSMs

- **Every frame**: `processRate = -1`
- **Every N frames**: `processRate = N`
- **Only when triggered manually**: `processRate = 0`

### 🔔 Event-Driven FSMs

FSMs that are **idle until you trigger them**. Great for highly efficient UI logic or reactive AI.

---

## 🎲 Special Features

### 🎛 RNG Utility

A powerful `RNG.cs` class is included for:

- Probabilistic transitions
- Weighted choices
- Random behavior control

### 💣 Robust Error Handling: The FUBAR System

> FUBAR = **F**\*\*ed **U**p **B**eyond **A**ll **R**ecognition

FSM_API includes an internal safeguard system to:

- Catch and log **all exceptions**
- Report errors via `OnInternalApiError`
- Automatically remove **broken instances**
- Automatically remove **unstable definitions** after repeated failures

✅ Ensures **runtime resilience** and **application stability**  
❌ Prevents hidden bugs from silently breaking your logic

---

## ⚡ High-Level Flow: Getting Started

Here’s a simplified walkthrough:

### 1. 🧱 Define Your FSM

```csharp
var builder = FSM_API.CreateFiniteStateMachine("MyFSM");
builder
    .State("Idle")
        .TransitionIf("Moving", ctx => /* some condition */)
    .End()
    .State("Moving")
        .TransitionIf("Idle", ctx => /* some other condition */)
    .End()
    .BuildDefinition();
```

---

### 2. 🚶 Create a Live Instance

```csharp
var context = new MyGameObjectContext();
var instance = FSM_API.CreateInstance("MyFSM", context);
```

---

### 3. 🧠 Tell FSMs to “Think”

```csharp
FSM_API.Update("Update"); // Call this from Unity’s Update(), or your own loop
```

---

## ⏭ Continue to:

➡️ **[01. Core Concepts: Your Guide to FSM_API](01_Core_Concepts.md)**
