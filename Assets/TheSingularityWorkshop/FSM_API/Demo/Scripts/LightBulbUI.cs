using UnityEngine;
using UnityEngine.UIElements;
using System;

public class LightBulbUI
{
    // Events to notify external systems (like SimpleLightBulbDemo) of user input
    public event Action OnBulbRequestedOn;
    public event Action OnBulbRequestedOff;

    // Event to update UI based on actual light state in the world
    public event Action<bool> OnBulbStateChangedInWorld;


    public VisualElement RootUIElement { get; private set; }
    private Label _bulbStatusLabel;
    private Button _onButton;
    private Button _offButton;

    public LightBulbUI(VisualTreeAsset bulbUXML)
    {
        RootUIElement = bulbUXML.Instantiate();

        _bulbStatusLabel = RootUIElement.Q<Label>("bulbStatusLabel");
        _onButton = RootUIElement.Q<Button>("onButton");
        _offButton = RootUIElement.Q<Button>("offButton");

        _onButton.clicked += () => OnBulbRequestedOn?.Invoke();
        _offButton.clicked += () => OnBulbRequestedOff?.Invoke();

        // Initial UI update (will be overridden by SimpleLightBulbDemo after bulb component is spawned)
        UpdateStatusLabel(false);
    }

    /// <summary>
    /// Updates the UI label and styles based on the actual state of the light in the world.
    /// </summary>
    /// <param name="isOn">True if the light is currently ON, false if OFF.</param>
    public void UpdateStatusLabel(bool isOn)
    {
        _bulbStatusLabel.text = isOn ? "ON" : "OFF";
        _bulbStatusLabel.EnableInClassList("on", isOn);
        _bulbStatusLabel.EnableInClassList("off", !isOn);

        // Control button interactability and visual state
        _onButton.SetEnabled(!isOn);
        _offButton.SetEnabled(isOn);
        _onButton.pickingMode = isOn ? PickingMode.Ignore : PickingMode.Position;
        _offButton.pickingMode = isOn ? PickingMode.Position : PickingMode.Ignore;
    }

    // Call this method when the UI controller is no longer needed (e.g., when a bulb is removed)
    public void Dispose()
    {
        // Unsubscribe from UI button events to prevent memory leaks
        _onButton.clicked -= () => OnBulbRequestedOn?.Invoke();
        _offButton.clicked -= () => OnBulbRequestedOff?.Invoke();

        // Clear references
        RootUIElement = null;
        _bulbStatusLabel = null;
        _onButton = null;
        _offButton = null;
    }
}