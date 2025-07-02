FSM_API: Robust Finite State Machine for Unity & Beyond

A powerful, high-performance, and Game-Engine-Agnostic C#/.NET API for building and managing Finite State Machines (FSMs) in your Unity projects and other C# applications. FSM_API simplifies complex AI, character behaviors, UI logic, and game flow, allowing you to focus on creativity.

Designed with performance, clarity, and flexibility in mind, FSM_API integrates seamlessly into your workflow, supporting both event-driven and frame-based state management paradigms.

✨ Key Features

    Effortless FSM Management: Centralized FSM_API provides a single, intuitive point to define, register, and update all your state machines.

    Unity-Native, Core-Flexible: Built purely in C#/.NET, FSM_API's core is engine-agnostic, making it highly portable. Ready-to-use with Unity (via simple MonoBehaviour integration), yet usable in any C# environment.

    Integrated High-Quality RNG: Includes TheSingularityWorkshop.FSM.API.Utilities.RNG – a robust, optimized random number generator. Perfectly suited for probabilistic FSM transitions, AI decision-making, and other game logic requiring reliable randomness.

    Flexible Update Control: Organize FSMs into Update, FixedUpdate, LateUpdate, or custom processing groups for precise control over execution timing.

    Granular Performance Options: Each FSM definition supports configurable update frequencies:

        Every Frame (-1): For highly reactive and immediate behaviors.

        Event-Driven (0): Zero CPU consumption when idle, perfect for reactive systems.

        Nth Frame (>0): Optimize performance by updating FSMs only every N frames.

    Context-Driven Behavior: FSMHandle instances are tightly coupled with your custom IStateContext objects, enabling FSMs to directly interact with and control your specific game entities (e.g., player, enemy, UI panel).

    Robust Error Handling: Catch and log all internal API exceptions via the OnInternalApiError event, allowing for centralized debugging and monitoring without crashing.

    Automatic Cleanup: Intelligent monitoring automatically removes stale FSM instances, preventing null references and maintaining a clean processing queue.

    Runtime Adaptability: Modify FSM definitions on the fly. Existing instances seamlessly adopt new state logic at their next update cycle.

🚀 Getting Started

Integrating FSM_API into your Unity project is quick and easy.

    Import the Package: After downloading from the Unity Asset Store, import the FSM_API package into your project. The core files will be organized under Assets/TheSingularityWorkshop/FSM_API/.

    Define Your Context:
    Create a C# class that represents the entity your FSM will manage. This class must implement the IStateContext interface (and IContext which it inherits from):
    C#

using TheSingularityWorkshop.FSM.API;

// Example: A simple player context
public class PlayerContext : IStateContext
{
    public bool IsValid { get; set; } = true;
    public string Name { get; set; }

    public float Health { get; set; } = 100f;
    public bool IsMoving { get; set; } = false;
    public bool IsAttacking { get; set; } = false;
    public int AmmoCount { get; set; } = 10;

    public PlayerContext(string name)
    {
        Name = name;
    }

    public void Invalidate() => IsValid = false;
}

Define Your FSM:
Use FSM_API.CreateFiniteStateMachine to construct your FSM definition.
C#

using UnityEngine;
using TheSingularityWorkshop.FSM.API;

public static class PlayerFSMDefinitions
{
    public const string Idle = "Idle";
    public const string Moving = "Moving";
    public const string Attacking = "Attacking";
    public const string Dead = "Dead";

    public static void RegisterPlayerFSM()
    {
        FSM_API.CreateFiniteStateMachine("PlayerFSM", processRate: -1, processingGroup: "Update")
            .WithInitialState(Idle)
            .State(Idle,
                onEnter: ctx => Debug.Log($"{ctx.Name} entered Idle state."),
                onUpdate: ctx => { },
                onExit: ctx => Debug.Log($"{ctx.Name} exiting Idle state.")
            )
            .Transition(Idle, Moving, ctx => (ctx as PlayerContext)?.IsMoving == true)

            .State(Moving,
                onEnter: ctx => Debug.Log($"{ctx.Name} entered Moving state."),
                onUpdate: ctx => Debug.Log($"{ctx.Name} is moving."),
                onExit: ctx => Debug.Log($"{ctx.Name} stopped moving.")
            )
            .Transition(Moving, Idle, ctx => (ctx as PlayerContext)?.IsMoving == false)
            .Transition(Moving, Attacking, ctx => (ctx as PlayerContext)?.IsAttacking == true)

            .State(Attacking,
                onEnter: ctx => Debug.Log($"{ctx.Name} started attacking."),
                onUpdate: ctx => { },
                onExit: ctx => Debug.Log($"{ctx.Name} finished attacking.")
            )
            .Transition(Attacking, Idle, ctx => (ctx as PlayerContext)?.IsAttacking == false)

            .State(Dead,
                onEnter: ctx => Debug.LogError($"{ctx.Name} has died!")
            )

            .AnyTransition(Dead, ctx => (ctx as PlayerContext)?.Health <= 0)
            .BuildDefinition();
    }
}

Integrate with Unity (FSMManager MonoBehaviour):
Create a simple MonoBehaviour to tick your FSMs. Add this to an empty GameObject in your scene.
C#

using UnityEngine;
using TheSingularityWorkshop.FSM.API;

public class FSMManager : MonoBehaviour
{
    void Awake()
    {
        FSM_API.OnInternalApiError += (msg, ex) => Debug.LogError($"FSM_API Internal Error: {msg}\n{ex}", this);

        PlayerFSMDefinitions.RegisterPlayerFSM();
    }

    void Update()
    {
        FSM_API.Update("Update");
    }

    void FixedUpdate()
    {
        FSM_API.FixedUpdate();
    }

    void LateUpdate()
    {
        FSM_API.LateUpdate();
    }

    void OnApplicationQuit()
    {
        FSM_API.ClearAllFSMs();
        Debug.Log("[FSMManager.OnApplicationQuit] All FSMs cleared.");
    }
}

Instantiate an FSM:
Attach this script to your player GameObject (or any GameObject that represents your PlayerContext).
C#

    using UnityEngine;
    using TheSingularityWorkshop.FSM.API;

    public class PlayerController : MonoBehaviour
    {
        private PlayerContext _playerContext;
        private FSMHandle _playerFSM;

        void Awake()
        {
            _playerContext = new PlayerContext(gameObject.name);

            _playerFSM = FSM_API.CreateInstance("PlayerFSM", _playerContext, "Update");
            Debug.Log($"[{_playerContext.Name}] FSM created. Current state: {_playerFSM.CurrentState}");
        }

        void Update()
        {
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
            _playerContext.Invalidate();
            Debug.Log($"[{_playerContext.Name}] PlayerController destroyed. FSM context marked invalid.");

            if (_playerFSM != null)
            {
                FSM_API.Unregister(_playerFSM);
                _playerFSM = null;
                Debug.Log($"[{_playerContext.Name}] FSM instance unregistered.");
            }
        }
    }

🎮 Demo Scene

A comprehensive demo scene, Assets/TheSingularityWorkshop/FSM_API/Demo/FSM_API_DemoScene.unity, is included to help you quickly grasp the core concepts and see FSM_API in action.

    Location: Assets/TheSingularityWorkshop/FSM_API/Demo/FSM_API_DemoScene.unity

    Highlights: Explore practical FSM implementations including a Light Bulb Demo, a Traffic Light Demo, and a Traffic Intersection Demo, showcasing basic and complex use cases of the API.

    Guide: Refer to the Assets/TheSingularityWorkshop/FSM_API/Demo/README_Demo.md for specific instructions on interacting with the demo.

📄 Documentation & Support

    Full Documentation: For detailed API reference, advanced usage patterns, and best practices, refer to the full documentation within this package at Assets/TheSingularityWorkshop/FSM_API/Documentation/.

    Support: Encountered an issue or have a question?

        Email: TheSingularityWorkshop@gmail.com

        Discord: [Your Discord Link Here]

        GitHub Issues: [Link to your GitHub Issues for bug reports/feature requests]

🙏 Acknowledgements

We believe in celebrating the contributions of others. This project utilizes the following third-party assets, which greatly enhance the demonstration experience:

    Light Bulb 3D Model

        Creator: Poly Art 3D

        Source: Unity Asset Store - Lightbulb

        Usage: Used to visually represent the light bulbs in the Light Bulb Demo and Traffic Light Demos, showcasing the FSM-driven behavior.

(Add more entries here as you incorporate other assets, following the same format.)

❤️ About The Singularity Workshop

FSM_API is the foundational technology from The Singularity Workshop, a collective dedicated to crafting robust, modular, and performance-driven tools for game and application development. Our mission is to empower creators with the highest quality building blocks, enabling rapid prototyping and scalable solutions for any project, from complex AI to self-assembling applications.

Stay tuned for future tools from The Singularity Workshop, including powerful editor extensions for visualizing and debugging your FSMs, and the revolutionary "AnyApp" system.

©️ License

This asset is provided under the MIT License. See Assets/TheSingularityWorkshop/FSM_API/LICENSE.md for full details.