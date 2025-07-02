using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class SimpleLightBulbDemo : MonoBehaviour
{
    // Assign these in the Inspector
    public UIDocument demoUIDocument; // The UIDocument for the main demo control panel
    public VisualTreeAsset lightbulbUIUXML; // The UXML for individual lightbulbs
    public StyleSheet lightbulbUIUSS; // The USS for individual lightbulbs

    // Reference to the Prefab of your LightBulb GameObject
    public GameObject lightBulbPrefab;

    private Button _addBulbButton;
    private Button _removeBulbButton;
    private Button _leaveDemoButton;

    // A VisualElement to host all the individual lightbulb UIs dynamically
    private VisualElement _bulbContainer;

    // Lists to manage both the LightBulb components and their corresponding UI Controllers
    private List<LightBulb> _activeBulbComponents = new List<LightBulb>();
    private List<LightBulbUI> _activeBulbUIControllers = new List<LightBulbUI>();

    private const int MAX_BULBS = 5;
    private const int MIN_BULBS = 1; // Minimum number of bulbs allowed

    void OnEnable()
    {
        // Basic Inspector assignment checks
        if (demoUIDocument == null) { Debug.LogError("Demo UIDocument is not assigned!", this); enabled = false; return; }
        if (lightbulbUIUXML == null) { Debug.LogError("Lightbulb UI UXML is not assigned!", this); enabled = false; return; }
        if (lightBulbPrefab == null) { Debug.LogError("LightBulb Prefab is not assigned! Please assign a prefab with a LightBulb component.", this); enabled = false; return; }

        VisualElement root = demoUIDocument.rootVisualElement;

        // Get references to buttons from the DemoControlPanel.uxml
        _addBulbButton = root.Q<Button>("addBulbButton");
        _removeBulbButton = root.Q<Button>("removeBulbButton");
        _leaveDemoButton = root.Q<Button>("leaveDemoButton");

        // Create the container for dynamically added bulb UIs
        _bulbContainer = new VisualElement();
        _bulbContainer.name = "BulbContainer";
        _bulbContainer.style.flexDirection = FlexDirection.Row;
        _bulbContainer.style.flexWrap = Wrap.Wrap;
        _bulbContainer.style.justifyContent = Justify.Center;
        _bulbContainer.style.alignItems = Align.FlexStart;
        _bulbContainer.style.flexGrow = 1;
        _bulbContainer.style.paddingBottom = 10;
        _bulbContainer.style.paddingLeft = 10;
        _bulbContainer.style.paddingRight = 10;
        _bulbContainer.style.paddingTop = 10;
        _bulbContainer.style.height = 300; // Give it a fixed height or set flex-grow

        // Add the bulb container to the main UI Document's root
        root.Add(_bulbContainer);

        // Add the USS for individual lightbulbs to the main UIDocument's style sheets
        if (lightbulbUIUSS != null)
        {
            root.styleSheets.Add(lightbulbUIUSS);
        }

        // Register callbacks for demo control buttons
        _addBulbButton.clicked += AddBulb;
        _removeBulbButton.clicked += RemoveLastBulb;
        _leaveDemoButton.clicked += LeaveDemo;

        // Start with the minimum number of bulbs
        for (int i = 0; i < MIN_BULBS; i++)
        {
            AddBulb();
        }
        UpdateControlButtons(); // Update button states based on initial bulb count
    }

    void OnDisable()
    {
        // Unregister callbacks
        if (_addBulbButton != null) _addBulbButton.clicked -= AddBulb;
        if (_removeBulbButton != null) _removeBulbButton.clicked -= RemoveLastBulb;
        if (_leaveDemoButton != null) _leaveDemoButton.clicked -= LeaveDemo;

        // Clean up all active bulb instances and their UI controllers
        foreach (var controller in _activeBulbUIControllers)
        {
            controller.Dispose(); // Unregister UI events
        }
        _activeBulbUIControllers.Clear();
        // Destroy the GameObjects
        foreach (var bulbComponent in _activeBulbComponents)
        {
            if (bulbComponent != null && bulbComponent.gameObject != null)
            {
                Destroy(bulbComponent.gameObject);
            }
        }
        _activeBulbComponents.Clear();
        _bulbContainer?.Clear();
    }

    private void AddBulb()
    {
        if (_activeBulbComponents.Count >= MAX_BULBS)
        {
            Debug.Log("Maximum number of bulbs reached.");
            return;
        }

        // 1. Instantiate the 3D LightBulb GameObject from prefab
        // Position them side-by-side or in a grid in the 3D world as well
        Vector3 spawnPosition = new Vector3(_activeBulbComponents.Count * 1.5f, 0, 0); // Example spacing
        GameObject newBulbGO = Instantiate(lightBulbPrefab, spawnPosition, Quaternion.identity);
        newBulbGO.name = $"LightBulb_{_activeBulbComponents.Count + 1}";
        LightBulb bulbComponent = newBulbGO.GetComponent<LightBulb>();
        if (bulbComponent == null)
        {
            Debug.LogError($"Prefab '{lightBulbPrefab.name}' does not have a LightBulb component! Cannot add bulb.", this);
            Destroy(newBulbGO);
            return;
        }

        _activeBulbComponents.Add(bulbComponent);

        // 2. Create a new UI Controller for this bulb
        LightBulbUI newUIController = new LightBulbUI(lightbulbUIUXML);
        _activeBulbUIControllers.Add(newUIController);

        // 3. Connect the UI Controller's actions to the bulb's FSM input
        newUIController.OnBulbRequestedOn += () => bulbComponent.SetShouldBeOn(true);
        newUIController.OnBulbRequestedOff += () => bulbComponent.SetShouldBeOn(false);

        // 4. Connect the bulb's FSM state (via LightBulb.ShouldBeOn) to update the UI
        // This is a simple direct link. For more complex FSMs, you might use FSM_API's
        // state change events or an OnUpdate action within LightBulb to update a UI-specific delegate.
        // For now, let's have the UI poll the actual state if needed, or rely on the ShouldBeOn flag
        // when the UI buttons are pressed. The LightBulb itself *reacts* to ShouldBeOn via FSM.
        // The UI should update its label based on the lightComponent.enabled state for accuracy.
        // We'll update the UI controller's label from here for simplicity in this demo.
        newUIController.OnBulbStateChangedInWorld += (isOn) => {
            newUIController.UpdateStatusLabel(isOn);
        };


        // Initial UI state update from actual bulb state
        newUIController.UpdateStatusLabel(bulbComponent.ShouldBeOn);

        // Add its UI element to our container
        _bulbContainer.Add(newUIController.RootUIElement);

        UpdateControlButtons();
    }

    private void RemoveLastBulb()
    {
        if (_activeBulbComponents.Count <= MIN_BULBS)
        {
            Debug.Log("Cannot remove the last bulb.");
            return;
        }

        // Get the last bulb
        int lastIndex = _activeBulbComponents.Count - 1;
        LightBulb bulbComponentToRemove = _activeBulbComponents[lastIndex];
        LightBulbUI uiControllerToRemove = _activeBulbUIControllers[lastIndex];

        // Remove its UI from the container
        _bulbContainer.Remove(uiControllerToRemove.RootUIElement);

        // Dispose of the UI controller to unregister its internal events
        uiControllerToRemove.Dispose();

        // Remove from our lists
        _activeBulbComponents.RemoveAt(lastIndex);
        _activeBulbUIControllers.RemoveAt(lastIndex);

        // Destroy the actual GameObject in the scene
        Destroy(bulbComponentToRemove.gameObject);

        UpdateControlButtons();
    }

    private void LeaveDemo()
    {
        Debug.Log("Leaving demo. Cleaning up.");
        // Best practice: if you're loading a new scene, this GameObject will be destroyed,
        // and OnDisable will handle cleanup. If staying in scene, manually clear.
        if (demoUIDocument != null && demoUIDocument.rootVisualElement != null)
        {
            demoUIDocument.rootVisualElement.Clear(); // Clear the entire UI
        }
        // Destroy this GameObject or disable its component if it manages the scene flow
        Destroy(gameObject);
    }

    private void UpdateControlButtons()
    {
        _addBulbButton.SetEnabled(_activeBulbComponents.Count < MAX_BULBS);
        _removeBulbButton.SetEnabled(_activeBulbComponents.Count > MIN_BULBS);
    }

    void Update()
    {
        // Continuously update the UI labels based on the actual light's state.
        // This is necessary because the FSM can change the state independently of button clicks
        // (e.g., if another FSM or external logic drives the ShouldBeOn flag).
        for (int i = 0; i < _activeBulbComponents.Count; i++)
        {
            // Assuming LightBulb has a way to report its current *visual* state (e.g. via _lightComponent.enabled)
            // or by checking its FSM's current state.
            // For simplicity, let's rely on the LightBulb's internal light component enabled state.
            if (_activeBulbComponents[i] != null && _activeBulbComponents[i].gameObject != null)
            {
                // This checks the actual light component's state, not just the FSM's input flag.
                bool actualLightState = _activeBulbComponents[i].GetComponentInChildren<Light>()?.enabled ?? false;
                _activeBulbUIControllers[i].UpdateStatusLabel(actualLightState);
            }
        }
    }
}