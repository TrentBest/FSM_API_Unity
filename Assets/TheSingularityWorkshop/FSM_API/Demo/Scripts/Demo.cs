using TheSingularityWorkshop.FSM.API;
using System.Collections.Generic; // For List<string>
using UnityEngine.SceneManagement; // For scene loading
using UnityEngine;
using UnityEngine.InputSystem; // For the new Unity Input System

public class Demo : MonoBehaviour, IStateContext
{
    // =====================================================================================
    // FSM API Context Properties (Required by IStateContext)
    // =====================================================================================
    public bool IsValid { get; set; } = true; // Set to false in OnDestroy for automatic cleanup
    public string Name { get; set; }

    // =====================================================================================
    // Demo Flow Control (Context-Specific Data)
    // =====================================================================================
    [Header("Demo Scene Management")]
    [Tooltip("List of all demo scene names, including Introduction. Ensure these are in Build Settings!")]
    public List<string> DemoSceneNames; // Populate this list in the Inspector in Unity

    // Flags to trigger transitions from UI or demo logic
    [HideInInspector] public bool LightBulbDemoSelected { get; set; } = false;
    [HideInInspector] public bool TrafficLightDemoSelected { get; set; } = false;
    [HideInInspector] public bool TrafficIntersectionDemoSelected { get; set; } = false;
    [HideInInspector] public bool DoorDemoSelected { get; set; } = false;
    [HideInInspector] public bool ParticlesDemoSelected { get; set; } = false;
    [HideInInspector] public bool UIDemoSelected { get; set; } = false;
    [HideInInspector] public bool GameInGameDemoSelected { get; set; } = false;

    // Flag to signal demo completion and return to Introduction
    [HideInInspector] public bool CurrentDemoCompleted { get; set; } = false;


    // =====================================================================================
    // Internal FSM Reference
    // =====================================================================================
    private FSMHandle _demoFSMInstance; // This will hold the running instance of our FSM

    // =====================================================================================
    // Unity Input System References
    // =====================================================================================
    [Header("Input System")]
    [Tooltip("Reference to your Input Actions Asset (e.g., 'DemoInputActions').")]
    public InputActionAsset DemoInputActions;

    private InputAction _returnToIntroAction; // Example: An escape key to always go back to intro
    private string _lastLoadedSceneName; // Changed from int to string, and made private


    // =====================================================================================
    // MonoBehaviour Lifecycle
    // =====================================================================================

    void Awake()
    {
        Name = gameObject.name; // Assign the GameObject's name as the context name for debugging

        // --- 1. Define the FSM Blueprint (only once, typically at app startup) ---
        if (!FSM_API.Exists("DemoFSM"))
        {
            FSM_API.CreateFiniteStateMachine("DemoFSM", -1, "App") // -1: updates every tick in "App" group
                                                                   // Define States with their corresponding actions
                .State("Initializing", OnEnterInitializing, OnUpdateInitializing, OnExitInitializing)
                .State("Introduction", OnEnterIntroduction, OnUpdateIntroduction, OnExitIntroduction)
                .State("LightBulbDemo", OnEnterLightBulbDemo, OnUpdateLightBulbDemo, OnExitLightBulbDemo)
                .State("TrafficLightDemo", OnEnterTrafficLightDemo, OnUpdateTrafficLightDemo, OnExitTrafficLightDemo)
                .State("TrafficIntersectionDemo", OnEnterTrafficIntersectionDemo, OnUpdateTrafficIntersectionDemo, OnExitTrafficIntersectionDemo)
                .State("DoorDemo", OnEnterDoorDemo, OnUpdateDoorDemo, OnExitDoorDemo)
                .State("ParticlesDemo", OnEnterParticlesDemo, OnUpdateParticlesDemo, OnExitParticlesDemo)
                .State("UIDemo", OnEnterUIDemo, OnUpdateUIDemo, OnExitUIDemo)
                .State("GameInGameDemo", OnEnterGameInGameDemo, OnUpdateGameInGameDemo, OnExitGameInGameDemo)

                // Define Transitions
                // Initial Flow after startup
                .Transition("Initializing", "Introduction", Initialized)
                // Introduction -> Demos (Player selects via UI button, which sets a flag)
                .Transition("Introduction", "LightBulbDemo", PlayerSelectedLightBulbDemo)
                .Transition("Introduction", "TrafficLightDemo", PlayerSelectedTrafficLightDemo)
                .Transition("Introduction", "TrafficIntersectionDemo", PlayerSelectedTrafficIntersectionDemo)
                .Transition("Introduction", "DoorDemo", PlayerSelectedDoorDemo)
                .Transition("Introduction", "ParticlesDemo", PlayerSelectedParticlesDemo)
                .Transition("Introduction", "UIDemo", PlayerSelectedUIDemo)
                .Transition("Introduction", "GameInGameDemo", PlayerSelectedGameInGameDemo)
                // Demo -> Introduction (Demo completes or player requests return)
                // Using one generic predicate 'CurrentDemoComplete' simplifies this
                .Transition("LightBulbDemo", "Introduction", CurrentDemoComplete)
                .Transition("TrafficLightDemo", "Introduction", CurrentDemoComplete)
                .Transition("TrafficIntersectionDemo", "Introduction", CurrentDemoComplete)
                .Transition("DoorDemo", "Introduction", CurrentDemoComplete)
                .Transition("ParticlesDemo", "Introduction", CurrentDemoComplete)
                .Transition("UIDemo", "Introduction", CurrentDemoComplete)
                .Transition("GameInGameDemo", "Introduction", CurrentDemoComplete)

                .WithInitialState("Initializing") // FSM starts in this state
                .BuildDefinition();

            Debug.Log($"DemoFSM definition for '{Name}' created.");
        }

        // --- 2. Create the FSM Instance for THIS Context ---
        _demoFSMInstance = FSM_API.CreateInstance("DemoFSM", this);
        if (_demoFSMInstance == null)
        {
            Debug.LogError($"Failed to create DemoFSM instance for '{Name}'.");
        }
        else
        {
            Debug.Log($"DemoFSM instance created for '{Name}'. Initial state: {_demoFSMInstance.CurrentState}");
        }

        // --- 3. Subscribe to Input Actions ---
        if (DemoInputActions != null)
        {
            // Example: An action mapped to 'Escape' or a dedicated 'Back' button
            _returnToIntroAction = DemoInputActions.FindAction("General/ReturnToIntro"); // Adjust "General/ReturnToIntro" to your actual action path

            if (_returnToIntroAction != null)
            {
                _returnToIntroAction.performed += ctx => OnReturnToIntroPerformed();
            }
            else
            {
                Debug.LogWarning("Input action 'General/ReturnToIntro' not found in DemoInputActions. Please check your Input Actions asset.");
            }
        }
        else
        {
            Debug.LogError("DemoInputActions asset is not assigned in the Inspector! Input integration will not work.");
        }

        // Add a global listener for FSM_API internal errors (highly recommended for debugging)
        FSM_API.OnInternalApiError += e => Debug.LogError($"[FSM_API Error] {e.Message}");

    }

    void OnEnable()
    {
        // Enable the Input Action Asset when this GameObject is enabled
        DemoInputActions?.Enable();
    }

    void OnDisable()
    {
        // Disable the Input Action Asset when this GameObject is disabled
        DemoInputActions?.Disable();
    }

    void Update()
    {
        // --- 4. Tick the FSM ---
        // Since DemoFSM has processRate: -1, it will be updated every time FSM_API.Update("App") is called.
        // This is crucial for the FSM to evaluate transitions and run OnUpdate actions.
        FSM_API.Update("App");
    }

    void OnDestroy()
    {
        // Set IsValid to false to signal FSM_API to automatically remove this instance.
        // This is part of the "Automatic Instance Shedding" feature for MonoBehaviour contexts.
        IsValid = false;
        Debug.Log($"Demo FSM Context '{Name}' destroyed. Instance will be cleaned up by FSM_API.");

        // Unsubscribe from input actions to prevent memory leaks if this script is destroyed
        if (_returnToIntroAction != null) _returnToIntroAction.performed -= ctx => OnReturnToIntroPerformed();
    }


    // =====================================================================================
    // State Action Methods (Called by FSM_API)
    // These methods correspond to the OnEnter, OnUpdate, OnExit delegates in FSMBuilder.State()
    // =====================================================================================

    // --- Initializing State ---
    private void OnEnterInitializing(IStateContext context)
    {
        Debug.Log("Entering Initializing state. Performing startup tasks...");
        // This is where you might perform one-time global setup for the demo app,
        // like loading global assets, initializing services, etc.
        // For simplicity, we'll quickly transition to Introduction.
    }
    private void OnUpdateInitializing(IStateContext context)
    {
        // You could check for completion of async initialization tasks here
    }
    private void OnExitInitializing(IStateContext context)
    {
        Debug.Log("Exiting Initializing state. Initialization complete.");
    }

    // --- Introduction State (Our Landing Page/Hub) ---
    private void OnEnterIntroduction(IStateContext context)
    {
        Debug.Log("Entering Introduction state. Loading '00_Introduction' Scene.");
        LoadScene("00_Introduction"); // Ensure this scene exists and is in Build Settings!
        // Reset any demo selection flags when returning to the introduction
        ResetDemoSelectionFlags();
        CurrentDemoCompleted = false; // Reset the completion flag for coming back from a demo
    }
    private void OnUpdateIntroduction(IStateContext context)
    {
        // In the Introduction scene, UI buttons will set the demo selection flags.
        // The FSM will then transition based on these flags in the next FSM.Update() cycle.
    }
    private void OnExitIntroduction(IStateContext context)
    {
        Debug.Log("Exiting Introduction state.");
        // Unload the Introduction scene if it was loaded additively and we are moving to another additive scene
        UnloadCurrentDemoScene();
    }

    // --- Demo States (Pattern) ---
    // Make sure each of these corresponds to a unique scene name in your Build Settings!
    // Implement specific logic for each demo within its OnUpdate/OnExit.

    private void OnEnterLightBulbDemo(IStateContext context) { Debug.Log("Entering LightBulbDemo state. Loading '01_LightBulbDemo' Scene."); LoadScene("01_LightBulbDemo"); CurrentDemoCompleted = false; }
    private void OnUpdateLightBulbDemo(IStateContext context) { /* Add specific logic for the Light Bulb Demo if needed */ }
    private void OnExitLightBulbDemo(IStateContext context) { Debug.Log("Exiting LightBulbDemo state."); UnloadCurrentDemoScene(); }

    private void OnEnterTrafficLightDemo(IStateContext context) { Debug.Log("Entering TrafficLightDemo state. Loading '02_TrafficLightDemo' Scene."); LoadScene("02_TrafficLightDemo"); CurrentDemoCompleted = false; }
    private void OnUpdateTrafficLightDemo(IStateContext context) { /* Add specific logic for the Traffic Light Demo if needed */ }
    private void OnExitTrafficLightDemo(IStateContext context) { Debug.Log("Exiting TrafficLightDemo state."); UnloadCurrentDemoScene(); }

    private void OnEnterTrafficIntersectionDemo(IStateContext context) { Debug.Log("Entering TrafficIntersectionDemo state. Loading '03_TrafficIntersectionDemo' Scene."); LoadScene("03_TrafficIntersectionDemo"); CurrentDemoCompleted = false; }
    private void OnUpdateTrafficIntersectionDemo(IStateContext context) { /* Add specific logic for the Traffic Intersection Demo if needed */ }
    private void OnExitTrafficIntersectionDemo(IStateContext context) { Debug.Log("Exiting TrafficIntersectionDemo state."); UnloadCurrentDemoScene(); }

    private void OnEnterDoorDemo(IStateContext context)
    {
        Debug.Log("Entering DoorDemo state. Loading '04_DoorDemo' Scene.");
        LoadScene("04_DoorDemo");
        CurrentDemoCompleted = false;
    }

    private void OnUpdateDoorDemo(IStateContext context) { /* Add specific logic for the Door Demo if needed */ }
    private void OnExitDoorDemo(IStateContext context)
    {
        Debug.Log("Exiting DoorDemo state.");
        UnloadCurrentDemoScene();
    }

    private void OnEnterParticlesDemo(IStateContext context) { Debug.Log("Entering ParticlesDemo state. Loading '05_ParticlesDemo' Scene."); LoadScene("05_ParticlesDemo"); CurrentDemoCompleted = false; }
    private void OnUpdateParticlesDemo(IStateContext context) { /* Add specific logic for the Particles Demo if needed */ }
    private void OnExitParticlesDemo(IStateContext context) { Debug.Log("Exiting ParticlesDemo state."); UnloadCurrentDemoScene(); }

    private void OnEnterUIDemo(IStateContext context) { Debug.Log("Entering UIDemo state. Loading '06_UIDemo' Scene."); LoadScene("06_UIDemo"); CurrentDemoCompleted = false; }
    private void OnUpdateUIDemo(IStateContext context) { /* Add specific logic for the UI Demo if needed */ }
    private void OnExitUIDemo(IStateContext context) { Debug.Log("Exiting UIDemo state."); UnloadCurrentDemoScene(); }

    private void OnEnterGameInGameDemo(IStateContext context) { Debug.Log("Entering GameInGameDemo state. Loading '07_GameInGameDemo' Scene."); LoadScene("07_GameInGameDemo"); CurrentDemoCompleted = false; }
    private void OnUpdateGameInGameDemo(IStateContext context) { /* Add specific logic for the Game In Game Demo if needed */ }
    private void OnExitGameInGameDemo(IStateContext context) { Debug.Log("Exiting GameInGameDemo state."); UnloadCurrentDemoScene(); }


    // =====================================================================================
    // Transition Condition Methods (Called by FSM_API)
    // These methods return true when the FSM should transition.
    // =====================================================================================

    private bool Initialized(IStateContext context)
    {
        // For this demo, initialization is assumed to be instantaneous.
        // In a real application, this would check if essential startup tasks are done.
        return true;
    }

    // --- Player Selection Predicates (Set by UI Buttons in Introduction Scene) ---
    private bool PlayerSelectedLightBulbDemo(IStateContext context) { return LightBulbDemoSelected; }
    private bool PlayerSelectedTrafficLightDemo(IStateContext context) { return TrafficLightDemoSelected; }
    private bool PlayerSelectedTrafficIntersectionDemo(IStateContext context) { return TrafficIntersectionDemoSelected; }
    private bool PlayerSelectedDoorDemo(IStateContext context) { return DoorDemoSelected; }
    private bool PlayerSelectedParticlesDemo(IStateContext context) { return ParticlesDemoSelected; }
    private bool PlayerSelectedUIDemo(IStateContext context) { return UIDemoSelected; }
    private bool PlayerSelectedGameInGameDemo(IStateContext context) { return GameInGameDemoSelected; }

    // --- Demo Completion Predicate (Set by "Back to Intro" Button in Demo Scenes) ---
    private bool CurrentDemoComplete(IStateContext context) { return CurrentDemoCompleted; }


    // =====================================================================================
    // Helper Methods (for internal script logic)
    // =====================================================================================

    /// <summary>
    /// Loads a Unity scene by name. Ensures the scene is in the 'DemoSceneNames' list
    /// and added to Unity's Build Settings.
    /// Stores the loaded scene name for later unloading.
    /// </summary>
    /// <param name="sceneName">The name of the scene to load.</param>
    private void LoadScene(string sceneName)
    {
        if (DemoSceneNames.Contains(sceneName))
        {
            // Unload the previous additive scene if one was loaded
            if (!string.IsNullOrEmpty(_lastLoadedSceneName))
            {
                // Only unload if it's not the scene we are about to load (prevents trying to unload itself)
                // And if the scene is actually loaded.
                if (SceneManager.GetSceneByName(_lastLoadedSceneName).isLoaded)
                {
                    SceneManager.UnloadSceneAsync(_lastLoadedSceneName);
                    Debug.Log($"Unloaded previous scene: {_lastLoadedSceneName}");
                }
            }

            SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
            _lastLoadedSceneName = sceneName; // Store the name of the newly loaded scene
            Debug.Log($"Loaded scene: {sceneName}");
        }
        else
        {
            Debug.LogError($"Scene '{sceneName}' not found in 'DemoSceneNames' list or Unity's Build Settings! Please add it.");
        }
    }

    /// <summary>
    /// Unloads the scene that was last loaded additively.
    /// </summary>
    private void UnloadCurrentDemoScene()
    {
        if (!string.IsNullOrEmpty(_lastLoadedSceneName))
        {
            if (SceneManager.GetSceneByName(_lastLoadedSceneName).isLoaded)
            {
                SceneManager.UnloadSceneAsync(_lastLoadedSceneName);
                Debug.Log($"Unloaded scene: {_lastLoadedSceneName}");
            }
            _lastLoadedSceneName = null; // Clear the stored scene name after unloading
        }
    }

    /// <summary>
    /// Resets all demo selection flags. Call this when entering the Introduction state
    /// to ensure a clean slate for new selections.
    /// </summary>
    private void ResetDemoSelectionFlags()
    {
        LightBulbDemoSelected = false;
        TrafficLightDemoSelected = false;
        TrafficIntersectionDemoSelected = false;
        DoorDemoSelected = false;
        ParticlesDemoSelected = false;
        UIDemoSelected = false;
        GameInGameDemoSelected = false;
        // Do NOT reset CurrentDemoCompleted here, as it's used for transitions *out* of demos.
    }

    // =====================================================================================
    // Unity Input System Event Handlers & Public UI Button Methods
    // =====================================================================================

    /// <summary>
    /// Event handler for the global "Return to Intro" input action (e.g., Escape key).
    /// This forces a transition back to the Introduction state from any active demo.
    /// </summary>
    private void OnReturnToIntroPerformed()
    {
        Debug.Log("Global 'Return to Intro' input detected. Requesting transition to Introduction state.");
        // We directly request a transition here, bypassing normal predicates for a universal escape
        _demoFSMInstance.TransitionTo("Introduction");
    }

    // --- Public Methods to be hooked up to UI Buttons in the "00_Introduction" Scene ---
    public void SelectLightBulbDemo() { LightBulbDemoSelected = true; Debug.Log("LightBulb Demo selected via UI."); }
    public void SelectTrafficLightDemo() { TrafficLightDemoSelected = true; Debug.Log("TrafficLight Demo selected via UI."); }
    public void SelectTrafficIntersectionDemo() { TrafficIntersectionDemoSelected = true; Debug.Log("TrafficIntersection Demo selected via UI."); }
    public void SelectDoorDemo() { DoorDemoSelected = true; Debug.Log("Door Demo selected via UI."); }
    public void SelectParticlesDemo() { ParticlesDemoSelected = true; Debug.Log("Particles Demo selected via UI."); }
    public void SelectUIDemo() { UIDemoSelected = true; Debug.Log("UI Demo selected via UI."); }
    public void SelectGameInGameDemo() { GameInGameDemoSelected = true; Debug.Log("GameInGame Demo selected via UI."); }

    // --- Public Method to be hooked up to a "Back to Intro" button in any individual demo scene ---
    public void CompleteCurrentDemoAndReturnToIntro()
    {
        CurrentDemoCompleted = true;
        Debug.Log("Current Demo flagged as complete. FSM will transition back to Introduction.");
    }
}