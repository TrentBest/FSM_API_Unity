using System;
using System.Linq; // For GetComponentsInChildren().First()

using UnityEngine;
using TheSingularityWorkshop.FSM.API; // Ensure this namespace is correct for your FSM API

public class TrafficLight : MonoBehaviour, IStateContext
{
    private const string FSM_NAME = "TrafficLight";

    // =====================================================================================
    // IStateContext Properties (Required by IStateContext)
    // =====================================================================================
    public bool IsValid { get; set; } = true; // Set to false in OnDestroy for FSM cleanup
    public string Name { get; set; } // Now has a setter as required by IStateContext

    // =====================================================================================
    // Exposed Bulbs (Assign in Inspector or find dynamically) - Now uses LightBulb
    // =====================================================================================
    public LightBulb redBulb;
    public LightBulb yellowBulb;
    public LightBulb greenBulb;

    // =====================================================================================
    // Timers (in seconds)
    // =====================================================================================
    public float greenLightDuration = 5f;
    public float yellowLightDuration = 2f;
    public float redLightDuration = 5f; // Added for completeness and consistency

    // =====================================================================================
    // Working state
    // =====================================================================================
    private float _currentLightTimer; // Consolidated timer for the current light state

    // =====================================================================================
    // FSM related members
    // =====================================================================================
    public FSMHandle trafficLightFSM;

    void Awake()
    {
        Name = gameObject.name; // Assign Name for IStateContext

        // Dynamically find LightBulb components in children if not assigned in inspector.
        // Assumes child LightBulb GameObjects are named with "Red", "Yellow", "Green".
        var bulbs = GetComponentsInChildren<LightBulb>();
        redBulb = bulbs.First(b => b.name.Contains("Red"));
        yellowBulb = bulbs.First(b => b.name.Contains("Yellow"));
        greenBulb = bulbs.First(b => b.name.Contains("Green"));

        // --- Define FSM Blueprint (if not already defined globally) ---
        // This ensures the FSM definition exists regardless of scene load order.
        if (!FSM_API.Exists(FSM_NAME))
        {
            Debug.Log($"[{nameof(TrafficLight)}.Awake] Defining hardcoded FSM: {FSM_NAME}");
            DefineTrafficLightFSM();
        }
        else
        {
            Debug.Log($"[{nameof(TrafficLight)}.Awake] FSM '{FSM_NAME}' already defined.");
        }

        // Create FSM instance for this TrafficLight
        trafficLightFSM = FSM_API.CreateInstance(FSM_NAME, this, "TrafficLights"); // Use the same updateCategory as defined
        Debug.Log($"[{nameof(TrafficLight)}.Awake] Created instance of {FSM_NAME}. Initial state: {trafficLightFSM.CurrentState}");
    }

    /// <summary>
    /// Defines the Finite State Machine for the Traffic Light.
    /// This method is called once to register the FSM blueprint with the FSM_API.
    /// </summary>
    private void DefineTrafficLightFSM()
    {
        FSM_API
            .CreateFiniteStateMachine(FSM_NAME, processRate: 30, "TrafficLights") // Ensure updateCategory matches where it will be ticked
            .WithInitialState("Red") // Explicitly setting initial state
            .State("Red",
                onEnter: ctx =>
                {
                    if (ctx is TrafficLight self) // Use pattern matching for cast
                    {
                        self.redBulb.ShouldBeOn = true;
                        self.yellowBulb.ShouldBeOn = false;
                        self.greenBulb.ShouldBeOn = false;
                        self._currentLightTimer = self.redLightDuration; // Initialize red light timer
                        Debug.Log($"{self.Name} - Entering RED");
                    }
                },
                onUpdate: ctx =>
                {
                    if (ctx is TrafficLight self)
                    {
                        self._currentLightTimer -= Time.deltaTime;
                    }
                }
            )
            .State("Green",
                onEnter: ctx =>
                {
                    if (ctx is TrafficLight self)
                    {
                        self.greenBulb.ShouldBeOn = true;
                        self.redBulb.ShouldBeOn = false;
                        self.yellowBulb.ShouldBeOn = false;
                        self._currentLightTimer = self.greenLightDuration; // Initialize green light timer
                        Debug.Log($"{self.Name} - Entering GREEN");
                    }
                },
                onUpdate: ctx =>
                {
                    if (ctx is TrafficLight self)
                    {
                        self._currentLightTimer -= Time.deltaTime;
                    }
                }
            )
            .State("Yellow",
                onEnter: ctx =>
                {
                    if (ctx is TrafficLight self)
                    {
                        self.yellowBulb.ShouldBeOn = true;
                        self.redBulb.ShouldBeOn = false;
                        self.greenBulb.ShouldBeOn = false;
                        self._currentLightTimer = self.yellowLightDuration; // Initialize yellow light timer
                        Debug.Log($"{self.Name} - Entering YELLOW");
                    }
                },
                onUpdate: ctx =>
                {
                    if (ctx is TrafficLight self)
                    {
                        self._currentLightTimer -= Time.deltaTime;
                    }
                }
            )
            // Define Transitions for a standard traffic light cycle
            .Transition("Red", "Green", ctx => (ctx as TrafficLight)?._currentLightTimer <= 0f)
            .Transition("Green", "Yellow", ctx => (ctx as TrafficLight)?._currentLightTimer <= 0f)
            .Transition("Yellow", "Red", ctx => (ctx as TrafficLight)?._currentLightTimer <= 0f)
            .BuildDefinition();
    }

    void OnDestroy()
    {
        // Mark context as invalid so FSM_API can clean up this instance
        IsValid = false;
        Debug.Log($"[{nameof(TrafficLight)}.OnDestroy] TrafficLight context '{Name}' destroyed. Instance will be cleaned up.");

        // Explicitly unregister the FSM instance.
        if (trafficLightFSM != null)
        {
            FSM_API.Unregister(trafficLightFSM);
            trafficLightFSM = null;
        }
    }
}