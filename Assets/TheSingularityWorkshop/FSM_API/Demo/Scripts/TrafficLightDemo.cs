using TheSingularityWorkshop.FSM.API;
using UnityEngine;
using System.Collections.Generic; // For Dictionary

public class TrafficLightDemo : MonoBehaviour, IStateContext
{
    // =====================================================================================
    // FSM API Context Properties (Required by IStateContext)
    // =====================================================================================
    public bool IsValid { get; set; } = true; // Set to false in OnDestroy
    public string Name { get; set; }

    // =====================================================================================
    // Unity References (Assign in Inspector) - NOW REFERENCES LightBulb MONOBEHAVIOURS
    // =====================================================================================
    [Header("Traffic Light Bulbs (LightBulb Script Instances)")]
    public LightBulb redBulb;
    public LightBulb yellowBulb;
    public LightBulb greenBulb;

    // =====================================================================================
    // FSM Related Members
    // =====================================================================================
    private FSMHandle _trafficLightFSMInstance;

    [Header("Traffic Light Timing")]
    [Tooltip("Duration in seconds for the Red light state.")]
    public float RedDuration = 5.0f;
    [Tooltip("Duration in seconds for the Yellow light state.")]
    public float YellowDuration = 2.0f;
    [Tooltip("Duration in seconds for the Green light state.")]
    public float GreenDuration = 5.0f;

    private float _currentLightTimer; // Timer for the current light state

    // =====================================================================================
    // MonoBehaviour Lifecycle
    // =====================================================================================

    void Awake()
    {
        Name = gameObject.name; // Set context name for debugging

        // --- 1. Define FSM Blueprints ---
        // We only define TrafficLightFSM here.
        // The "LightBulbFSM" should be defined by the LightBulb.cs script itself.
        if (!FSM_API.Exists("TrafficLightFSM"))
        {
            FSM_API.CreateFiniteStateMachine("TrafficLightFSM")
                .WithInitialState("Red") // Traffic lights typically start red
                .State("Red", OnEnterRed, OnUpdateRed, OnExitRed)
                .State("Green", OnEnterGreen, OnUpdateGreen, OnExitGreen)
                .State("Yellow", OnEnterYellow, OnUpdateYellow, OnExitYellow)

                // Define Transitions for a standard traffic light cycle
                .Transition("Red", "Green", IsRedDurationMet)
                .Transition("Green", "Yellow", IsGreenDurationMet)
                .Transition("Yellow", "Red", IsYellowDurationMet)
                .BuildDefinition();

            Debug.Log($"[TrafficLightDemo.Awake] Defined TrafficLightFSM.");
        }
        else
        {
            Debug.Log($"[TrafficLightDemo.Awake] TrafficLightFSM already defined.");
        }

        // No need to check for LightBulbFSM here; the LightBulb.cs script handles its own FSM definition.
        // We just need to ensure the LightBulb instances exist in the scene and are properly configured.
    }

    void Start()
    {
        // --- 2. Create FSM Instance ---
        // Create the primary FSM instance for *this* TrafficLightDemo.
        _trafficLightFSMInstance = FSM_API.CreateInstance("TrafficLightFSM", this, "TrafficLightGroup");
        Debug.Log($"[TrafficLightDemo.Start] Created instance of TrafficLightFSM on group '{_trafficLightFSMInstance.Definition.ProcessingGroup}'. Initial state: {_trafficLightFSMInstance.CurrentState}");

        // Ensure LightBulb instances are initially off
        SetBulbShouldBeOn(redBulb, false);
        SetBulbShouldBeOn(yellowBulb, false);
        SetBulbShouldBeOn(greenBulb, false);
    }

    void Update()
    {
        // No explicit FSM_API.Update call here for individual bulbs,
        // as each LightBulb MonoBehaviour's Update or its FSM_API group handling
        // (if it puts itself in a specific group that gets ticked by Demo.cs)
        // will handle its own FSM.
        // The main Demo.cs script will tick "TrafficLightGroup".
    }

    void OnDestroy()
    {
        // --- 4. Clean up FSM Instance ---
        // Set IsValid to false to signal FSM_API to automatically remove this instance.
        IsValid = false;
        Debug.Log($"[TrafficLightDemo.OnDestroy] TrafficLightDemo context '{Name}' destroyed. Instance will be cleaned up.");

        if (_trafficLightFSMInstance != null)
        {
            FSM_API.Unregister(_trafficLightFSMInstance);
            _trafficLightFSMInstance = null;
        }

        // Individual LightBulb FSMs are handled by their respective LightBulb.cs scripts.
        // No need to unregister them from here.
    }

    // =====================================================================================
    // TrafficLightFSM State Action Methods
    // =====================================================================================

    private void OnEnterRed(IStateContext context)
    {
        if (context is TrafficLightDemo trafficLightDemo) // Cast context to TrafficLightDemo
        {
            Debug.Log($"{trafficLightDemo.Name} - Entering Red state.");
            // Turn Red bulb ON, others OFF via their ShouldBeOn property
            trafficLightDemo.SetBulbShouldBeOn(trafficLightDemo.redBulb, true);
            trafficLightDemo.SetBulbShouldBeOn(trafficLightDemo.yellowBulb, false);
            trafficLightDemo.SetBulbShouldBeOn(trafficLightDemo.greenBulb, false);
            trafficLightDemo._currentLightTimer = trafficLightDemo.RedDuration; // Start timer
        }
    }

    private void OnUpdateRed(IStateContext context)
    {
        if (context is TrafficLightDemo trafficLightDemo) // Cast context to TrafficLightDemo
        {
            trafficLightDemo._currentLightTimer -= Time.deltaTime;
        }
    }

    private void OnExitRed(IStateContext context)
    {
        if (context is TrafficLightDemo trafficLightDemo) // Cast context to TrafficLightDemo
        {
            Debug.Log($"{trafficLightDemo.Name} - Exiting Red state.");
        }
    }

    private bool IsRedDurationMet(IStateContext context)
    {
        if (context is TrafficLightDemo trafficLightDemo) // Cast context to TrafficLightDemo
        {
            return trafficLightDemo._currentLightTimer <= 0;
        }
        return false; // Should not happen if context is valid
    }

    private void OnEnterGreen(IStateContext context)
    {
        if (context is TrafficLightDemo trafficLightDemo) // Cast context to TrafficLightDemo
        {
            Debug.Log($"{trafficLightDemo.Name} - Entering Green state.");
            // Turn Green bulb ON, others OFF
            trafficLightDemo.SetBulbShouldBeOn(trafficLightDemo.redBulb, false);
            trafficLightDemo.SetBulbShouldBeOn(trafficLightDemo.yellowBulb, false);
            trafficLightDemo.SetBulbShouldBeOn(trafficLightDemo.greenBulb, true);
            trafficLightDemo._currentLightTimer = trafficLightDemo.GreenDuration; // Start timer
        }
    }

    private void OnUpdateGreen(IStateContext context)
    {
        if (context is TrafficLightDemo trafficLightDemo) // Cast context to TrafficLightDemo
        {
            trafficLightDemo._currentLightTimer -= Time.deltaTime;
        }
    }

    private void OnExitGreen(IStateContext context)
    {
        if (context is TrafficLightDemo trafficLightDemo) // Cast context to TrafficLightDemo
        {
            Debug.Log($"{trafficLightDemo.Name} - Exiting Green state.");
        }
    }

    private bool IsGreenDurationMet(IStateContext context)
    {
        if (context is TrafficLightDemo trafficLightDemo) // Cast context to TrafficLightDemo
        {
            return trafficLightDemo._currentLightTimer <= 0;
        }
        return false; // Should not happen if context is valid
    }

    private void OnEnterYellow(IStateContext context)
    {
        if (context is TrafficLightDemo trafficLightDemo) // Cast context to TrafficLightDemo
        {
            Debug.Log($"{trafficLightDemo.Name} - Entering Yellow state.");
            // Turn Yellow bulb ON, others OFF
            trafficLightDemo.SetBulbShouldBeOn(trafficLightDemo.redBulb, false);
            trafficLightDemo.SetBulbShouldBeOn(trafficLightDemo.yellowBulb, true);
            trafficLightDemo.SetBulbShouldBeOn(trafficLightDemo.greenBulb, false);
            trafficLightDemo._currentLightTimer = trafficLightDemo.YellowDuration; // Start timer
        }
    }

    private void OnUpdateYellow(IStateContext context)
    {
        if (context is TrafficLightDemo trafficLightDemo) // Cast context to TrafficLightDemo
        {
            trafficLightDemo._currentLightTimer -= Time.deltaTime;
        }
    }

    private void OnExitYellow(IStateContext context)
    {
        if (context is TrafficLightDemo trafficLightDemo) // Cast context to TrafficLightDemo
        {
            Debug.Log($"{trafficLightDemo.Name} - Exiting Yellow state.");
        }
    }

    private bool IsYellowDurationMet(IStateContext context)
    {
        if (context is TrafficLightDemo trafficLightDemo) // Cast context to TrafficLightDemo
        {
            return trafficLightDemo._currentLightTimer <= 0;
        }
        return false; // Should not happen if context is valid
    }

    // =====================================================================================
    // Helper Method to Interact with LightBulb MonoBehaviours
    // =====================================================================================

    /// <summary>
    /// Sets the 'ShouldBeOn' property of a LightBulb MonoBehaviour, which its FSM reacts to.
    /// </summary>
    /// <param name="bulb">The LightBulb MonoBehaviour instance.</param>
    /// <param name="turnOn">True to request the bulb to turn on, false to turn off.</param>
    private void SetBulbShouldBeOn(LightBulb bulb, bool turnOn)
    {
        if (bulb != null)
        {
            bulb.ShouldBeOn = turnOn;
            Debug.Log($"[TrafficLightDemo] Setting {bulb.name}.ShouldBeOn to {turnOn}");
        }
        else
        {
            Debug.LogWarning($"[TrafficLightDemo] LightBulb reference is null. Cannot set state.");
        }
    }
}