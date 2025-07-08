# FSM_API: Robust, Game-Engine-Agnostic Finite State Machine for C#/.NET

FSM_API is a powerful, high-performance, and **100% Game-Engine-Agnostic C#/.NET API** for building and managing Finite State Machines (FSMs). While designed to integrate seamlessly with Unity (and distributed as a Unity package), its core functionality is pure C#, making it ideal for:

* **Unity**
* **Godot**
* **.NET desktop applications**
* **Console applications**
* **Custom game engines**
* **Any C#/.NET environment**

FSM_API simplifies complex AI, character behaviors, UI logic, and game flow, allowing you to focus on creativity, regardless of your target platform.

Designed with performance, clarity, and flexibility in mind, FSM_API supports both event-driven and frame-based state management paradigms.

✨ Key Features

* **Game-Engine-Agnostic Core:** Built purely in C#/.NET, FSM_API's core is entirely independent of any game engine or framework, ensuring maximum portability and reusability across projects.
* **Effortless FSM Management:** Centralized FSM_API provides a single, intuitive point to define, register, and update all your state machines.
* **Integrated High-Quality RNG:** Includes TheSingularityWorkshop.FSM.API.Utilities.RNG – a robust, optimized random number generator. Perfectly suited for probabilistic FSM transitions, AI decision-making, and other game logic requiring reliable randomness.
* **Flexible Update Control:** Organize FSMs into custom processing groups for precise control over execution timing (e.g., "AI_Updates", "UI_Logic", "Physics_Related").
* **Granular Performance Options:** Each FSM definition supports configurable update frequencies:
    * **Every Tick (-1):** For highly reactive and immediate behaviors.
    * **Event-Driven (0):** Zero CPU consumption when idle, perfect for reactive systems where updates are explicitly triggered.
    * **Nth Tick (>0):** Optimize performance by updating FSMs only every N ticks/frames.
* **Context-Driven Behavior:** `FSMHandle` instances are tightly coupled with your custom `IStateContext` objects, enabling FSMs to directly interact with and control your specific entities (e.g., player, enemy, UI panel, backend service).
* **Robust Error Handling:** Catch and log all internal API exceptions via the `OnInternalApiError` event, allowing for centralized debugging and monitoring without crashing your application.
* **Automatic Cleanup:** Intelligent monitoring automatically removes stale FSM instances when their contexts become invalid, preventing null references and maintaining a clean processing queue.
* **Runtime Adaptability:** Modify FSM definitions on the fly. Existing instances seamlessly adopt new state logic at their next update cycle.

🚀 Getting Started (Engine-Agnostic)

FSM_API's core is pure C#. Here's how you define and manage FSMs in any .NET environment.

* **Define Your Context:**
    Create a C# class that represents the entity your FSM will manage. This class must implement the `IStateContext` interface (which inherits from `IContext`). This is where your FSM interacts with your game object's data and methods.

    ```csharp
    using TheSingularityWorkshop.FSM.API;
    using System; // For Console.WriteLine

    // Example: A simple character context
    public class CharacterContext : IStateContext
    {
        // IContext requirement: A name for logging and identification
        public string Name { get; set; }

        // IStateContext requirement: Indicates if the context is still valid and active
        public bool IsValid { get; set; } = true;

        // Your custom data for the character
        public float Health { get; set; } = 100f;
        public bool IsMoving { get; set; } = false;
        public bool IsAttacking { get; set; } = false;

        public CharacterContext(string name)
        {
            Name = name;
        }

        // Method to invalidate the context when the entity it represents is no longer active
        public void Invalidate() => IsValid = false;

        // Example: Actions your FSM might trigger
        public void PerformAttack()
        {
            Console.WriteLine($"{Name} performs an attack!");
            IsAttacking = true;
        }
    }
    ```

* **Define Your FSM:**
    Use `FSM_API.CreateFiniteStateMachine` to construct your FSM definition.

    ```csharp
    using TheSingularityWorkshop.FSM.API;
    using System; // For Console.WriteLine

    public static class CharacterFSMDefinitions
    {
        public const string Idle = "Idle";
        public const string Moving = "Moving";
        public const string Attacking = "Attacking";
        public const string Dead = "Dead";

        public static void RegisterCharacterFSM()
        {
            // Define an FSM named "CharacterFSM"
            // It will be part of the "GameLogic" processing group
            // Instances of this FSM will update every tick (-1)
            FSM_API.CreateFiniteStateMachine("CharacterFSM", processRate: -1, processingGroup: "GameLogic")
                .WithInitialState(Idle)
                .State(Idle,
                    onEnter: ctx => Console.WriteLine($"{ctx.Name} entered Idle state."),
                    onUpdate: ctx => { /* Maybe check for input or environmental changes */ },
                    onExit: ctx => Console.WriteLine($"{ctx.Name} exiting Idle state.")
                )
                .Transition(Idle, Moving, ctx => (ctx as CharacterContext)?.IsMoving == true)

                .State(Moving,
                    onEnter: ctx => Console.WriteLine($"{ctx.Name} entered Moving state."),
                    onUpdate: ctx => Console.WriteLine($"{ctx.Name} is currently moving."),
                    onExit: ctx => Console.WriteLine($"{ctx.Name} stopped moving.")
                )
                .Transition(Moving, Idle, ctx => (ctx as CharacterContext)?.IsMoving == false)
                .Transition(Moving, Attacking, ctx => (ctx as CharacterContext)?.IsAttacking == true)

                .State(Attacking,
                    onEnter: ctx => {
                        Console.WriteLine($"{ctx.Name} started attacking.");
                        (ctx as CharacterContext)?.PerformAttack();
                    },
                    onUpdate: ctx => { /* Attack animation/cooldown logic */ },
                    onExit: ctx => Console.WriteLine($"{ctx.Name} finished attacking.")
                )
                .Transition(Attacking, Idle, ctx => (ctx as CharacterContext)?.IsAttacking == false)

                .State(Dead,
                    onEnter: ctx => Console.WriteLine($"{ctx.Name} has died!")
                )
                // AnyTransition applies regardless of the current state
                .AnyTransition(Dead, ctx => (ctx as CharacterContext)?.Health <= 0)
                .BuildDefinition(); // Finalize the FSM definition
        }
    }
    ```

* **Create and Tick FSM Instances:**
    You'll instantiate an FSM with a `CharacterContext` and then periodically call `FSM_API.Update` for its processing group.

    ```csharp
    using TheSingularityWorkshop.FSM.API;
    using System;
    using System.Threading; // For simulating a game loop

    public class FsmApplication
    {
        public static void Main(string[] args)
        {
            // Subscribe to internal errors for debugging
            FSM_API.OnInternalApiError += (e) =>
            {
                Console.Error.WriteLine($"FSM_API Error [{e.ErrorType}]: {e.Message}");
                if (e.Exception != null) Console.Error.WriteLine($"  Exception: {e.Exception}");
                if (e.FsmName != null) Console.Error.WriteLine($"  FSM: {e.FsmName}");
                if (e.Context != null) Console.Error.WriteLine($"  Context: {e.Context.Name}");
                if (e.StateName != null) Console.Error.WriteLine($"  State: {e.StateName}");
            };

            // Register your FSM definition once at application startup
            CharacterFSMDefinitions.RegisterCharacterFSM();

            // Create an instance of your FSM, linked to a specific context
            var heroContext = new CharacterContext("Hero");
            var heroFSM = FSM_API.CreateInstance("CharacterFSM", heroContext, "GameLogic");

            Console.WriteLine($"Hero FSM created. Initial state: {heroFSM.CurrentState}");

            // Simulate a game loop
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine($"--- Tick {i+1} ---");

                // Simulate input
                if (i == 2) heroContext.IsMoving = true;
                if (i == 4) heroContext.IsAttacking = true;
                if (i == 6) heroContext.IsAttacking = false;
                if (i == 8) heroContext.Health = 0;

                // Tell the FSM_API to update FSMs in the "GameLogic" group
                FSM_API.Update("GameLogic");

                Thread.Sleep(100); // Simulate time passing
            }

            // Cleanup when done (optional for most app lifecycles, but good practice)
            FSM_API.Unregister(heroFSM);
            FSM_API.DestroyFiniteStateMachine("CharacterFSM", "GameLogic");
        }
    }
    ```

---

### 🚀 Getting Started (Unity Integration)

FSM_API is designed for seamless integration into Unity. If you are developing in Unity, follow these steps in addition to the core FSM definition.

* **Import the Package:**
    After downloading from the Unity Asset Store, import the FSM_API package into your project. The core files will be organized under `Assets/TheSingularityWorkshop/FSM_API/`.

* **Unity-Specific Context (`MonoBehaviour` or `ScriptableObject`):**
    Your `IStateContext` implementation will typically be a `MonoBehaviour` attached to a `GameObject`, or a `ScriptableObject` asset.

    ```csharp
    using UnityEngine;
    using TheSingularityWorkshop.FSM.API;

    // Example: A Player MonoBehaviour context
    public class PlayerContext : MonoBehaviour, IStateContext
    {
        // IContext requirement: Name for logging (use GameObject's name)
        public string Name { get; set; }

        // IStateContext requirement: IsValid indicates if the GameObject is active
        public bool IsValid => this != null && gameObject.activeInHierarchy;

        // Custom player-specific data
        public float Health { get; set; } = 100f;
        public bool IsMoving { get; set; } = false;
        public bool IsAttacking { get; set; } = false;

        void Awake()
        {
            Name = gameObject.name; // Automatically sets the context name
        }

        // Method to explicitly invalidate context, e.g., OnDestroy
        public void Invalidate() => IsValid = false;

        // Example: Actions your FSM might trigger for Unity specific things
        public void PlayAnimation(string animName)
        {
            Debug.Log($"{Name} playing animation: {animName}");
            // Your Unity animation logic here
        }
    }
    ```

* **Integrate with Unity Game Loop (`FSMManager` MonoBehaviour):**
    Create a simple MonoBehaviour (e.g., `FSMManager`) to handle global FSM initialization and `FSM_API.Update` calls from Unity's `Update()`, `FixedUpdate()`, or `LateUpdate()` methods. Attach this script to an empty GameObject in your scene.

    ```csharp
    using UnityEngine;
    using TheSingularityWorkshop.FSM.API;

    public class FSMManager : MonoBehaviour
    {
        void Awake()
        {
            // Subscribe to internal API errors for centralized logging in Unity's console
            FSM_API.OnInternalApiError += (e) =>
            {
                string errorMsg = $"FSM_API Internal Error: {e.Message}";
                if (e.Exception != null) errorMsg += $"\nException: {e.Exception.Message}";
                if (e.FsmName != null) errorMsg += $"\nFSM: {e.FsmName}";
                if (e.Context != null) errorMsg += $"\nContext: {e.Context.Name}";
                if (e.StateName != null) errorMsg += $"\nState: {e.StateName}";
                Debug.LogError(errorMsg, this); // 'this' attaches error to the FSMManager GameObject
            };

            // Register your FSM definitions here, typically once at startup
            PlayerFSMDefinitions.RegisterPlayerFSM(); // Assuming you have a PlayerFSMDefinitions class
        }

        // Call FSM_API.Update for your chosen processing groups
        void Update()
        {
            FSM_API.Update("Update"); // Process FSMs in the "Update" group
        }

        void FixedUpdate()
        {
            FSM_API.FixedUpdate(); // Process FSMs in the "FixedUpdate" group
        }

        void LateUpdate()
        {
            FSM_API.LateUpdate(); // Process FSMs in the "LateUpdate" group
        }

        // Clean up all FSMs when the application quits
        void OnApplicationQuit()
        {
            FSM_API.ClearAllFSMs();
            Debug.Log("[FSMManager] All FSMs cleared on application quit.");
        }
    }
    ```

* **Instantiate an FSM in Unity:**
    Use `FSM_API.CreateInstance` to create a live FSM linked to your MonoBehaviour `IStateContext`.

    ```csharp
    using UnityEngine;
    using TheSingularityWorkshop.FSM.API;

    public class PlayerController : MonoBehaviour
    {
        private PlayerContext _playerContext; // This MonoBehaviour itself is the context
        private FSMHandle _playerFSM;

        void Awake()
        {
            _playerContext = GetComponent<PlayerContext>(); // Get the context from this GameObject
            if (_playerContext == null)
            {
                Debug.LogError("PlayerContext MonoBehaviour not found on this GameObject.", this);
                return;
            }

            // Create an instance of the "PlayerFSM" for this specific PlayerContext
            _playerFSM = FSM_API.CreateInstance("PlayerFSM", _playerContext, "Update");
            Debug.Log($"[{_playerContext.Name}] FSM created. Current state: {_playerFSM.CurrentStateName}");
        }

        void Update()
        {
            // Example: Simulate context data changes based on Unity input
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _playerContext.IsMoving = !_playerContext.IsMoving;
                Debug.Log($"[{_playerContext.Name}] IsMoving set to: {_playerContext.IsMoving}");
            }
            if (Input.GetKeyDown(KeyCode.A))
            {
                _playerContext.IsAttacking = true;
            }
            else if (Input.GetKeyUp(KeyCode.A))
            {
                _playerContext.IsAttacking = false;
            }

            if (Input.GetKeyDown(KeyCode.D))
            {
                _playerContext.Health -= 30;
                Debug.Log($"[{_playerContext.Name}] Health: {_playerContext.Health}");
            }
        }

        void OnDestroy()
        {
            // Mark the context as invalid when the GameObject is destroyed
            _playerContext?.Invalidate();
            Debug.Log($"[{_playerContext?.Name}] PlayerController destroyed. FSM context marked invalid.");

            // Explicitly unregistering ensures immediate cleanup, though automatic cleanup exists
            if (_playerFSM != null)
            {
                FSM_API.Unregister(_playerFSM);
                _playerFSM = null;
                Debug.Log($"[{_playerContext?.Name}] FSM instance unregistered.");
            }
        }
    }
    ```

---

🎮 Demo Scene (Unity Specific)

A comprehensive demo scene, `Assets/TheSingularityWorkshop/FSM_API/Demo/FSM_API_DemoScene.unity`, is included to help you quickly grasp the core concepts and see FSM_API in action within a Unity environment.

* **Location:** `Assets/TheSingularityWorkshop/FSM_API/Demo/FSM_API_DemoScene.unity`
* **Highlights:** Explore practical FSM implementations including a Light Bulb Demo, a Traffic Light Demo, and a Traffic Intersection Demo, showcasing basic and complex use cases of the API.
* **Guide:** Refer to the `Assets/TheSingularityWorkshop/FSM_API/Demo/README_Demo.md` for specific instructions on interacting with the demo.

📄 Documentation & Support

* **Full Documentation:** For detailed API reference, advanced usage patterns, and best practices, refer to the full documentation within this package at `Assets/TheSingularityWorkshop/FSM_API/Documentation/`.
* **Support:** Encountered an issue or have a question?
    * Email: TheSingularityWorkshop@gmail.com
    * Discord: [Your Discord Link Here]
    * GitHub Issues: [Link to your GitHub Issues for bug reports/feature requests]

🙏 Acknowledgements

We believe in celebrating the contributions of others. This project utilizes the following third-party assets, which greatly enhance the demonstration experience:

* **Light Bulb 3D Model**
    * Creator: Poly Art 3D
    * Source: Unity Asset Store - Lightbulb
    * Usage: Used to visually represent the light bulbs in the Light Bulb Demo and Traffic Light Demos, showcasing the FSM-driven behavior.



❤️ About The Singularity Workshop

FSM_API is the foundational technology from The Singularity Workshop, a collective dedicated to crafting robust, modular, and performance-driven tools for game and application development. Our mission is to empower creators with the highest quality building blocks, enabling rapid prototyping and scalable solutions for any project, from complex AI to self-assembling applications.

Stay tuned for future tools from The Singularity Workshop, including powerful editor extensions for visualizing and debugging your FSMs, and the revolutionary "AnyApp" system.

©️ License

This asset is provided under the MIT License. See `Assets/TheSingularityWorkshop/FSM_API/LICENSE.md` for full details.
