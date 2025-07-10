Introduction to FSM_API

Table of Contents

[00. Introduction to FSM_API](00_Introduction.md)

[01. Core Concepts: Your Guide to FSM_API](01_Core_Concepts.md)

[02. Getting Started with Unity](02_Getting_Started_Unity.md)

[03. Getting Started with C# (Non-Unity)](03_Getting_Started_CSharp.md)

[04. FSMBuilder Deep Dive: Building Your FSMs](04_FSM_Builder_Deep_Dive.md)

[05. Understanding and Implementing Your Context (IStateContext)](05_Context_Implementation.md)

[06. RNG Utility: Adding Randomness to Your FSMs](06_RNG_Utility.md)

[07. Robust Error Handling: The Fubar System](07_Error_Handling.md)

[08. Performance Tips & Best Practices](08_Performance_Tips.md)

[09. Common Use Cases & Examples](09_Common_Use_Cases.md)

[10. FSM_API for Non-Coders: A Big Picture Overview](10_Non_Coder_Overview.md)

[11. Frequently Asked Questions (FAQ)](11_FAQ.md)

FSM_API is a robust, modular, and game-engine-agnostic C#/.NET
API. It is designed to make building and managing Finite State Machines
(FSMs) effortless for Unity, Godot, custom engines, and any C#/.NET
application developers.

Whether you're creating complex AI, dynamic character behaviors, interactive UI
states, or intricate game logic, FSM_API empowers you to do so
with clarity and confidence.

Why Choose FSM_API?

    Simplifies Complexity:
    FSM_API streamlines the process of designing and managing state-driven systems.
    You can focus on your creative vision instead of boilerplate code.

    Versatile & Modular - Truly Engine-Agnostic:
    Built with unparalleled flexibility, FSM_API allows you to define custom
    processing groups (e.g., "AI_Tick", "UI_Animation", "Physics_Update").
    You control precisely when and where each group of FSMs updates
    by simply calling a single, central FSM_API.Update(string processingGroup)
    method from any part of your application. This empowers you to integrate
    FSMs seamlessly into any game loop, custom simulation, or even nested
    state-machine hierarchies, providing ultimate control and adaptability.

    Performance-Driven:
    Enjoy high efficiency and responsiveness, even in projects with many
    active state machines, thanks to its optimized internal processing.

    Clarity & Maintainability:
    The API is designed for readability and ease of use, helping teams
    collaborate and maintain codebases with less friction.

    Testability:
    FSM_API’s clean separation of logic and context makes it easy to write
    reliable, automated tests for your state machines.

How We Are Structured: Your Main Tools

FSM_API provides a few main tools that you will interact with.
Understanding these relationships will help you use the API effectively.

    FSM_API (The Central Manager)

        This is the primary hub for all FSM operations.

        You interact with it to CreateFiniteStateMachine() (to start making a blueprint).

        You also use it to CreateInstance() (to bring a blueprint to life).

        Crucially, you tell it to Update() specific processing groups.

        Think: This is the big boss that organizes and runs everything.

    FSMBuilder (The Blueprint Designer)

        You obtain an instance of this tool by calling FSM_API.CreateFiniteStateMachine().

        It's a fluent, chainable tool that lets you define your states.

        You also define transitions, and set initial properties for your FSM blueprint.

        Think: This is your drawing board for designing how your behaviors work.

    FSM Definition (The Blueprint Itself)

        This is the completed, reusable plan for a state machine.

        It's created when you finish using the FSMBuilder and call BuildDefinition().

        It holds all the rules, states, and actions for a particular behavior.

        Think: This is the master copy of your behavior design, ready for use.

    FSMHandle (The Live Behavior Instance)

        This represents a single, live, running copy of an FSM Definition.

        You get an FSMHandle by calling FSM_API.CreateInstance().

        It manages the CurrentState and processes logic for one specific entity.

        Think: This is the actual character performing actions or the door opening.

    IStateContext (Your Game's Data & Features)

        This is your custom object or script from your game (e.g., your player character, a monster, a door).

        It holds all the specific data and functions that your FSM needs to interact with.

        Your game object implements (which means it agrees to follow the rules of) this IStateContext interface.

        States within your FSM will use this context to read information or trigger actions on your object.

        Think: This is like your character's brain and body, giving the FSM access.

Flow of Interaction (How the parts connect):

FSM_API
-- (calls) CreateFiniteStateMachine() --> FSMBuilder
|
| (you design with builder, then call BuildDefinition())
V
FSM Definition
|
| (you call CreateInstance() on FSM_API with the definition's name)
V
FSMHandle
|
| (the FSMHandle continuously interacts with...)
V
IStateContext (THIS IS YOUR GAME OBJECT/SCRIPT)

Understanding State Management

FSM_API allows you to manage the way your states behave in two primary paradigms.

    Frame-based FSMs:
    These are FSMs that update on a regular, timed basis.
    You can configure them to update every single frame (processRate = -1).
    Or every Nth frame (e.g., every 5th frame, processRate = 5).
    Or only when you explicitly tell them to (by setting processRate = 0).

    Event-driven FSMs:
    These FSMs react instantly to specific events that you send to them.
    This allows for very responsive and efficient logic execution, 
    as they only "wake up" when something relevant happens.

FSM_API's Special Features

    RNG Utility:
    A powerful RNG.cs utility is included with FSM_API.
    

    Robust Error Handling: The "Fubar" System:
    (F***ed Up Beyond All Recognition)
    FSM_API features a unique and exceptionally robust internal system.
    This proactive mechanism automatically detects and reports malfunctions.
    It uses the OnInternalApiError event to tell you when problems occur.
    If errors repeat, it will gracefully remove malfunctioning FSM instances.
    It can even remove entire FSM definitions that cause consistent errors.
    This ensures the stability and resilience of your application.
    It helps prevent crashes, even from unexpected runtime issues.

High-Level Flow: Getting Started

Here's a quick overview of the main steps to start using FSM_API:

    Define Your FSM:
    You begin by using FSM_API.CreateFiniteStateMachine().
    Then, you use the fluent FSMBuilder to describe your FSM's states.
    You also define its transitions and associated behaviors.
    This action creates a reusable FSM blueprint (definition).

    Create Live Instances:
    Next, you bring your defined FSMs to life.
    You instantiate them using FSM_API.CreateInstance().
    You link them to specific objects in your game or application.
    These objects must implement the IStateContext interface.
    This step effectively activates your FSMs.

    Tell FSMs to "Think":
    Finally, you integrate calls to FSM_API.Update("YourProcessingGroup").
    You'll place these calls into your application's main game loop(s).
    Or, you can put them into your custom update cycles.
    This action drives the FSMs forward.
    It allows them to process their logic, evaluate transitions, and react to events.

[Continue to: 01. Core Concepts: Your Guide to FSM_API](01_Core_Concepts.md)
