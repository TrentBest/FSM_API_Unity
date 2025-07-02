using System;
using TheSingularityWorkshop.FSM.API;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements; // Added for IStateContext.Actions


public class LightBulbDemo : MonoBehaviour, IStateContext, IDemonstration
{
    // Public reference to the LightBulb component on a child GameObject
    [Header("Demo Specific References")]
    public LightBulb lightBulb;

    // UI elements for demonstration (assign in inspector)
    [Header("UI")]
    public UIDocument lightBulbDemoUI;

    // IStateContext implementation
    public bool IsValid { get; set; } = false;
    public string Name { get; set; }
    


    // FSM-related fields
    private const string DEMO_FSM_NAME = "LightBulbDemoFSM";
    private const string BULB_FSM_NAME = "LightBulbIndividualFSM";
    private const string DEMO_PROCESSING_GROUP = "LightBulbDemoGroup"; 
    private FSMHandle _demoFSMHandle;

    // Internal state flags
    private bool _isInternalSetupComplete = false;
    private bool _interactionRequested = false; // Flag for starting interaction
    private bool _doneInteractingRequested = false; // Flag for stopping interaction

    // Flag to control FSM_API.Update() calls
    private bool _isHubControlled = false;
    private Button addLightButton;
    private Button removeLightButton;
    private Button quitLightDemoButton;

    // --- MonoBehaviour Lifecycle ---

    void Awake()
    {
        // 1. Set context properties (always do this in Awake for MonoBehaviour contexts)
        IsValid = true;
        Name = gameObject.name;

        // Ensure references are set
        if (lightBulb == null)
        {
            Debug.LogError($"[LightBulbDemo.Awake] 'lightBulb' reference is not set. Please assign the LightBulb component in the Inspector.", this);
            enabled = false; // Disable to prevent NullReferenceExceptions
            return;
        }
        var root = lightBulbDemoUI.rootVisualElement;
        addLightButton = root.Q<Button>("addBulbButton");
        removeLightButton = root.Q<Button>("removeBulbButton");
        quitLightDemoButton = root.Q<Button>("leaveDemoButton");
        addLightButton.clicked += AddLightButton_clicked;
        removeLightButton.clicked += RemoveLightButton_clicked;
        quitLightDemoButton.clicked += QuitLightDemoButton_clicked;

        
        // 2. Define FSMs if they don't already exist globally.
        // This should only happen ONCE across all scenes/instances.
        DefineFSMs();
    }

    private void QuitLightDemoButton_clicked()
    {
        
    }

    private void RemoveLightButton_clicked()
    {
        
    }

    private void AddLightButton_clicked()
    {
        
    }

    void Start()
    {
        // If this script is running in its *own scene* (not loaded by the hub),
        // we'll treat it as not hub-controlled and initialize it directly.
        // In a hub scenario, the hub would call Initialize.
        // This check is a common way to have a MonoBehaviour double as a standalone and a hub-integrated component.
        if (FindObjectsByType<LightBulbDemo>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Length == 1 
            && Application.isPlaying && !transform.root.CompareTag("DemoHub")) 
        {
            Debug.Log("[LightBulbDemo.Start] Running in standalone mode. Initializing demo directly.");
            Initialize(false); // Not hub-controlled, so this script will manage its FSM updates
            StartDemo(); // Start immediately if standalone
        }
        else
        {
            Debug.Log("[LightBulbDemo.Start] Running under a hub or multiple instances. Waiting for hub to call Initialize.");
        }
    }

    void Update()
    {
        // Only call FSM_API.Update if NOT hub-controlled.
        // If hub-controlled, the hub's manager will call UpdateDemoFSM().
        if (!_isHubControlled && _demoFSMHandle != null)
        {
            // You might need to update a specific group here, or all groups.
            // For a demo, updating its own group is typical.
            FSM_API.Update(DEMO_PROCESSING_GROUP);
            FSM_API.Update("Unit"); // If LightBulb also creates FSMs in "Unit" group
        }
    }

    void OnDestroy()
    {
        // Clean up the FSM instance when the GameObject is destroyed.
        StopDemo(); // Ensure proper cleanup through the interface method
        IsValid = false; // Mark context as invalid
    }

    // --- IDemonstration Interface Implementation ---

    public void Initialize(bool isHubControlled)
    {
        Debug.Log($"[LightBulbDemo.Initialize] Initializing LightBulbDemo. Hub Controlled: {isHubControlled}");
        _isHubControlled = isHubControlled;

        // Create the FSM instance for *this specific demo's lifecycle*.
        if (_demoFSMHandle == null)
        {
            _demoFSMHandle = FSM_API.CreateInstance(DEMO_FSM_NAME, this, DEMO_PROCESSING_GROUP);
            if (_demoFSMHandle == null)
            {
                Debug.LogError($"[LightBulbDemo.Initialize] Failed to create FSM instance for '{gameObject.name}'. Disabling demo.", this);
                enabled = false;
            }
            else
            {
                Debug.Log($"[LightBulbDemo.Initialize] Created instance of {DEMO_FSM_NAME} on group '{_demoFSMHandle.Definition.ProcessingGroup}'.");
            }
        }
        else
        {
            // If already initialized (e.g., re-initializing), ensure it's reset
            _demoFSMHandle.ResetFSM();
        }

        // Ensure the light bulb's own FSM instance is ready or created if it's managed by this demo directly
        // The LightBulb.cs script already creates its own FSM instance in its Awake, which is good.
        // We just need to make sure its initial state matches.
        lightBulb.ShouldBeOn = false; // Ensure it starts off
    }

    public void StartDemo()
    {
        Debug.Log($"[LightBulbDemo.StartDemo] Activating LightBulbDemo.");
        
    }

    public void StopDemo()
    {
        Debug.Log($"[LightBulbDemo.StopDemo] Deactivating LightBulbDemo.");
        if (_demoFSMHandle != null)
        {
            FSM_API.Unregister(_demoFSMHandle);
            _demoFSMHandle = null;
        }

        // Reset demo state
        _isInternalSetupComplete = false;
        _interactionRequested = false;
        _doneInteractingRequested = false;

        // Reset the light bulb
        if (lightBulb != null)
        {
            lightBulb.ShouldBeOn = false; // Force it off
        }

        // Hide all UI
      
    }

    public void UpdateDemoFSM()
    {
        // This method is called by the hub's main update loop when _isHubControlled is true.
        if (_demoFSMHandle != null)
        {
            // This implicitly updates the FSM group specified when the FSM instance was created.
            // If you had multiple groups for the demo (e.g., individual lights in a separate group),
            // you'd call FSM_API.Update() for each relevant group here.
            FSM_API.Update(DEMO_PROCESSING_GROUP);
            // Assuming LightBulb.cs creates its FSM in a group named "Unit" or similar, update it too
            // FSM_API.Update("Unit"); // Uncomment if LightBulb's FSM is in a different group and needs explicit update
        }
    }


    // --- FSM Definition Helper ---
    private void DefineFSMs()
    {
        if (!FSM_API.Exists(DEMO_FSM_NAME))
        {
            FSM_API.CreateFiniteStateMachine(DEMO_FSM_NAME, -1, DEMO_PROCESSING_GROUP) // Process every frame
                .WithInitialState("Initializing")
                .State("Initializing", OnEnterInitializing, OnUpdateInitializing, OnExitInitializing)
                .State("Ready", OnEnterReady, OnUpdateReady, OnExitReady)
                .State("Interacting", OnEnterInteracting, OnUpdateInteracting, OnExitInteracting)
                .Transition("Initializing", "Ready", IsReady)
                .Transition("Ready", "Interacting", IsInteracting)
                .Transition("Interacting", "Ready", IsDoneInteracting)
                .BuildDefinition();
            Debug.Log($"[LightBulbDemo.DefineFSMs] Defined {DEMO_FSM_NAME}.");
        }

        // The LightBulb.cs script handles its own FSM definition, which is generally cleaner.
        // So, we don't need LightBulbIndividualFSM defined here anymore.
        // However, if you wanted the demo to control the bulb's FSM definition, it would be here.
    }

    // --- LightBulbDemoFSM State Implementations ---

    private void OnEnterInitializing(IStateContext context)
    {
        Debug.Log($"{context.Name} - Entering Initializing state.");
        _isInternalSetupComplete = false;
        _interactionRequested = false;
        _doneInteractingRequested = false;
      

        StartCoroutine(SimulateAsyncSetup());
    }

    private void OnUpdateInitializing(IStateContext context) { } // No specific action, transition condition handles it

    private void OnExitInitializing(IStateContext context)
    {
        Debug.Log($"{context.Name} - Exiting Initializing state.");
    }

    private bool IsReady(IStateContext context)
    {
        return _isInternalSetupComplete;
    }

    private void OnEnterReady(IStateContext context)
    {
        Debug.Log($"{context.Name} - Entering Ready state. Demo is prepared and awaiting interaction.");
      
    }

    private void OnUpdateReady(IStateContext context) { } // Await player input

    private void OnExitReady(IStateContext context)
    {
        Debug.Log($"{context.Name} - Exiting Ready state.");
       
    }

    private bool IsInteracting(IStateContext context)
    {
        // This is triggered by a UI button click (see Public Methods below)
        return _interactionRequested;
    }

    private void OnEnterInteracting(IStateContext context)
    {
        Debug.Log($"{context.Name} - Entering Interacting state. Player can now interact with lights.");
    
        _interactionRequested = false; // Reset flag
        lightBulb.ShouldBeOn = true; // Turn light on as part of interaction start
    }

    private void OnUpdateInteracting(IStateContext context)
    {
        // While here, the lightBulb's own FSM is running and controlled by its ShouldBeOn flag.
        // You'd have UI events or other logic changing lightBulb.ShouldBeOn.
        // For instance, a button for "Toggle Light" would just flip lightBulb.ShouldBeOn.
    }

    private void OnExitInteracting(IStateContext context)
    {
        Debug.Log($"{context.Name} - Exiting Interacting state.");
        _doneInteractingRequested = false; // Reset flag
      
        lightBulb.ShouldBeOn = false; // Turn light off when exiting interaction
    }

    private bool IsDoneInteracting(IStateContext context)
    {
        // This is triggered by a UI button click (see Public Methods below)
        return _doneInteractingRequested;
    }

    // --- Helper Coroutine for Initialization ---
    private IEnumerator SimulateAsyncSetup()
    {
        yield return new WaitForSeconds(2.0f); // Simulate 2 seconds of loading
        _isInternalSetupComplete = true;
        Debug.Log($"{Name} - Internal setup simulated completion.");
    }

    // --- Public Methods for UI Interaction ---
    // Call these from Unity UI Buttons (OnClick events)

    public void OnClick_StartInteraction()
    {
        Debug.Log($"[LightBulbDemo] Start Interaction button clicked.");
        _interactionRequested = true;
    }

    public void OnClick_StopInteraction()
    {
        Debug.Log($"[LightBulbDemo] Stop Interaction button clicked.");
        _doneInteractingRequested = true;
    }

    public void OnClick_ToggleLight()
    {
        if (lightBulb != null)
        {
            lightBulb.ShouldBeOn = !lightBulb.ShouldBeOn;
            Debug.Log($"[LightBulbDemo] Toggled light to: {lightBulb.ShouldBeOn}");
        }
    }
}