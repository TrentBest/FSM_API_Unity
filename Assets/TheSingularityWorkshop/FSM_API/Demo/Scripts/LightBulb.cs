using System;

using TheSingularityWorkshop.FSM.API;

using UnityEngine;

// Ensure a Light component is present on the GameObject or a child.
// For visible bulb effect, ensure a MeshRenderer is present and its material is emissive.

public class LightBulb : MonoBehaviour, IStateContext
{
    // The FSM definition name for this type of bulb.
    private const string FSM_NAME = "LightBulbFSM";

    // Handle to this specific bulb's FSM instance.
    private FSMHandle _bulbFSMInstance;

    // References to the actual light and mesh renderer components.
    private Light _lightComponent;
    private MeshRenderer _meshRenderer; // Used to control material emission

    // The current material of the bulb's mesh.
    private Material _bulbMaterial;
    private Color _emissionColorOn = Color.yellow; // Default ON emission color
    private Color _emissionColorOff = Color.black; // Default OFF emission color (no emission)
    private float _emissionIntensityOn = 5f; // Default ON emission intensity
    private float _emissionIntensityOff = 0f; // Default OFF emission intensity

    // This public flag is the external control point, intended to be set by the UI.
    public bool ShouldBeOn = false;

    // IStateContext implementation
    public string Name { get; set; }
    public bool IsValid { get; set; } = true; // Set to false when object is destroyed.

    void Awake()
    {
        Name = name; // Initialize context name with GameObject's name.

        // Get references to components
        _lightComponent = GetComponentInChildren<Light>();
        _meshRenderer = GetComponentInChildren<MeshRenderer>();

        // Basic error checking
        if (_lightComponent == null)
        {
            Debug.LogError($"LightBulb '{name}': No Light component found on this GameObject or its children. Disabling script.", this);
            enabled = false;
            return;
        }
        if (_meshRenderer == null)
        {
            Debug.LogWarning($"LightBulb '{name}': No MeshRenderer found on this GameObject or its children. Bulb visual emission will not work.", this);
        }
        else
        {
            // Get the material instance to modify its properties (important for multiple bulbs)
            _bulbMaterial = _meshRenderer.material;
        }

        // Define the Finite State Machine (FSM) if it hasn't been defined already.
        if (!FSM_API.Exists(FSM_NAME))
        {
            FSM_API.CreateFiniteStateMachine(FSM_NAME, processRate: 15) // Match your desired process rate
                .State("Off", OnEnterOff) // Define "Off" state and its enter action
                .State("On", OnEnterOn)   // Define "On" state and its enter action
                                          // Transitions based on the 'ShouldBeOn' flag from this context.
                .Transition("Off", "On", condition: ctx => ((LightBulb)ctx).ShouldBeOn)
                .Transition("On", "Off", condition: ctx => !((LightBulb)ctx).ShouldBeOn)
                .WithInitialState("Off") // Set the initial state for new instances
                .BuildDefinition();

            Debug.Log($"[LightBulb] '{FSM_NAME}' FSM Definition Built successfully.");
        }

        // Create a unique FSM instance for *this specific* LightBulb GameObject.
        _bulbFSMInstance = FSM_API.CreateInstance(FSM_NAME, this);
        if (_bulbFSMInstance == null)
        {
            Debug.LogError($"[LightBulb] Failed to create FSM instance for '{name}'. Disabling script.", this);
            enabled = false;
            return;
        }
    }

    void OnDestroy()
    {
        // When this GameObject is destroyed, mark its context as invalid for FSM API cleanup.
        IsValid = false;
        // Explicitly destroy the FSM instance associated with this bulb.
        if (_bulbFSMInstance != null)
        {
            FSM_API.Unregister(_bulbFSMInstance);
            // Debug.Log($"[LightBulb] FSM instance for '{name}' unregistered.");
        }
        // Clean up the material instance if we created one
        if (_bulbMaterial != null)
        {
            Destroy(_bulbMaterial);
        }
    }

    // --- FSM State Actions ---
    // These methods are called by the FSM when it enters the "Off" or "On" state.

    private void OnEnterOff(IStateContext context)
    {
        if (context is LightBulb bulb)
        {
            bulb.SetLightState(false); // Turn off the actual light source and emission
            // Debug.Log($"Light Bulb '{bulb.name}': Switched to OFF state via FSM");
        }
    }

    private void OnEnterOn(IStateContext context)
    {
        if (context is LightBulb bulb)
        {
            bulb.SetLightState(true); // Turn on the actual light source and emission
            // Debug.Log($"Light Bulb '{bulb.name}': Switched to ON state via FSM");
        }
    }

    /// <summary>
    /// Controls the actual light component and the bulb's material emission.
    /// This method is called by the FSM state actions.
    /// </summary>
    /// <param name="isOn">True to turn light ON, false to turn OFF.</param>
    private void SetLightState(bool isOn)
    {
        if (_lightComponent != null)
        {
            _lightComponent.enabled = isOn; // Enable/disable the Light component
        }

        if (_bulbMaterial != null)
        {
            if (isOn)
            {
                // Enable emission for the material
                _bulbMaterial.EnableKeyword("_EMISSION");
                _bulbMaterial.SetColor("_EmissionColor", _emissionColorOn * _emissionIntensityOn);
                // For URP/HDRP, you might also need: _bulbMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }
            else
            {
                // Disable emission
                _bulbMaterial.DisableKeyword("_EMISSION");
                _bulbMaterial.SetColor("_EmissionColor", _emissionColorOff * _emissionIntensityOff);
            }
        }

        // The ShouldBeOn flag should only be set by external input (e.g., UI or game logic)
        // It should NOT be set by the FSM's internal state changes.
        // This method should *react* to the FSM state, not *change* the FSM's input.
    }

    // Optional: Public method to manually set the state, primarily for external FSMs
    // that might drive this bulb directly, or for initial setup.
    public void SetShouldBeOn(bool value)
    {
        ShouldBeOn = value;
    }
}