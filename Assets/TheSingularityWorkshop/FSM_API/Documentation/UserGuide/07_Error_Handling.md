07. Error Handling and Debugging in FSM_API

Effective error handling is crucial for building robust and reliable FSM-driven systems. The FSM_API provides built-in mechanisms to help you catch, log, and diagnose issues quickly, specifically those originating from the API's core operations.

The OnInternalApiError Event

FSM_API's primary error notification mechanism is the static OnInternalApiError event. You can subscribe to this event to receive notifications when unexpected issues occur within the FSM system itself.

What It Catches:

    Internal API Errors: This event specifically captures exceptions thrown by FSM_API's core operations. Examples include issues during FSM registration, problems within the internal update cycle, or invalid internal configurations detected by the API.

    Not Your State Logic Errors: It is crucial to understand that OnInternalApiError does not catch errors in your own application code. This includes exceptions thrown within your FSM's state actions (onEnter, onUpdate, onExit) or your transition condition predicates. You are responsible for implementing error handling (e.g., try-catch blocks) within your own state logic and context methods.

How to Subscribe:

You should subscribe to OnInternalApiError once, typically during your application's initialization (e.g., in an Awake() method for Unity MonoBehaviours, or within your application's bootstrap logic for pure C# applications). This allows for centralized error logging and monitoring.

Example: FsmErrorHandler.cs (Unity MonoBehaviour)
C#
```csharp
using TheSingularityWorkshop.FSM.API;
using UnityEngine; // For Debug.LogError in Unity

public class FsmErrorHandler : MonoBehaviour
{
    // Subscribe to the event when this object is enabled.
    void OnEnable()
    {
        // The event provides a string message and the Exception object.
        FSM_API.OnInternalApiError += HandleFsmApiError;
        Debug.Log("Subscribed to FSM_API.OnInternalApiError.");
    }

    // Unsubscribe when this object is disabled to prevent memory leaks.
    void OnDisable()
    {
        FSM_API.OnInternalApiError -= HandleFsmApiError;
        Debug.Log("Unsubscribed from FSM_API.OnInternalApiError.");
    }

    // This method will be called when an internal FSM_API error occurs.
    // It receives a descriptive message and the caught Exception.
    private void HandleFsmApiError(string errorMessage, System.Exception exception)
    {
        Debug.LogError($"FSM_API Internal Error! Message: {errorMessage}");
        if (exception != null)
        {
            Debug.LogError($"Exception Details: {exception.ToString()}"); // Log full exception details including stack trace
        }

        // --- Custom Error Handling Logic ---
        // Here you can add your custom logic, such as:
        // - Displaying an error message to the user (e.g., a "fatal error" popup)
        // - Sending automated error reports to a telemetry system
        // - Logging the error to a persistent file for later analysis
        // - Attempting to gracefully recover or disable affected systems (if applicable and safe)
    }
}
```
Built-in Error Thresholds and Automatic Cleanup

FSM_API incorporates internal error tracking mechanisms that automatically respond to repeated errors from FSM instances or definitions, ensuring system stability and preventing resource accumulation from faulty logic.

Error Tracking Mechanisms:
```csharp
    FSM_API.ErrorCountThreshold: This static property (default: 5) defines how many times a single FSM instance (represented by an FSMHandle) can throw an unhandled exception during its update cycle before FSM_API automatically removes and cleans up that specific instance.
```
        Behavior: If an FSM instance repeatedly encounters errors, it's considered "faulty." Once its error count reaches ErrorCountThreshold, FSM_API will call onExit for its current state, unregister the FSMHandle, and release its associated resources. This prevents a single problematic FSM from continually causing errors and consuming processing time.
```csharp
    FSM_API.DefinitionErrorThreshold: This static property (default: 3) defines how many different instances created from the same FSM definition can fail and be removed (due to reaching ErrorCountThreshold) before FSM_API schedules the complete destruction of the FSM definition itself.
```
        Behavior: If multiple instances derived from the same FSM definition consistently fail, it suggests a fundamental problem with the definition's blueprint. Once DefinitionErrorThreshold is met, FSM_API will queue a deferred modification to call DestroyFiniteStateMachine for that definition. This means no new instances can be created from it, and existing instances will cease to function or be tracked by the API.

Benefits of Thresholds:

    Self-Healing: The system automatically prunes problematic FSM instances and definitions, reducing the impact of bugs in your state logic.

    Resource Management: Prevents memory leaks and CPU waste by actively removing non-functional FSMs.

    Early Warning: Repeated errors hitting these thresholds can serve as an early warning sign of significant issues in your FSM designs, even if you don't explicitly handle OnInternalApiError.

These thresholds are configurable, allowing you to fine-tune the system's sensitivity to errors based on your application's stability requirements.

Conclusion

Implementing proper error handling and understanding FSM_API's debugging tools are vital steps towards building stable and maintainable FSM-driven systems. By subscribing to OnInternalApiError, you gain insight into the API's internal health, and by understanding the built-in error thresholds, you can rely on the system's self-correcting mechanisms to maintain runtime robustness.

➡️ Continue to: 08_FSM_API_Examples_and_Best_Practices.md
