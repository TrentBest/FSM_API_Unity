07. Error Handling and Debugging in FSM_API

Effective error handling is crucial.
It helps build reliable FSM-driven systems.
FSM_API provides built-in tools.
These help you catch, log, and diagnose issues quickly.

The OnInternalApiError Event

This is FSM_API's primary error notification mechanism.
It is a static event you can subscribe to.

What It Catches

    Internal API Errors:
    Captures unexpected exceptions.
    Thrown by FSM_API's core operations.
    Examples: registration issues, update cycle problems.
    Or invalid internal configurations.

    Not State Logic Errors:
    Does not catch errors in your own code.
    This includes your state actions (onEnter, onUpdate, onExit).
    Also your transition condition predicates.
    You must handle those errors within your own methods.

How to Subscribe

Subscribe to OnInternalApiError once.
Do this for centralized error logging and monitoring.
It's often done in an Awake() method for Unity.
Or in your application's initialization logic.

C#
C#

using TheSingularityWorkshop.FSM.API;
using UnityEngine; // For Debug.LogError, if in Unity

public class FsmErrorHandler : MonoBehaviour
{
    // Subscribe to the event when this object is enabled.
    void OnEnable()
    {
        FSM_API.OnInternalApiError += HandleFsmApiError;
        Debug.Log("Subscribed to FSM_API.OnInternalApiError.");
    }

    // Unsubscribe when this object is disabled.
    void OnDisable()
    {
        FSM_API.OnInternalApiError -= HandleFsmApiError;
        Debug.Log("Unsubscribed from FSM_API.OnInternalApiError.");
    }

    // This method will be called when an internal FSM_API error occurs.
    private void HandleFsmApiError(object sender, FsmErrorEventArgs e)
    {
        Debug.LogError($"FSM_API Internal Error! Type: {e.ErrorType}, Message: {e.Message}");
        Debug.LogError($"Affected FSM: {e.FsmName}, Context: {e.Context?.Name ?? "N/A"}");
        Debug.LogError($"Current State: {e.StateName ?? "N/A"}");

        if (e.Exception != null)
        {
            Debug.LogError($"Exception Details: {e.Exception}");
        }

        // You can add custom logic here:
        // - Send error reports
        // - Display an error message to the user
        // - Attempt to gracefully recover (if possible)
        // - Log to a persistent file
    }
}

// FsmErrorEventArgs (for reference, defined internally by FSM_API)
// This structure passes detailed error information.
/*
namespace TheSingularityWorkshop.FSM.API
{
    public class FsmErrorEventArgs : System.EventArgs
    {
        public FsmErrorType ErrorType { get; } // Enum for classification (e.g., Configuration, Runtime)
        public string Message { get; }         // Human-readable error description
        public System.Exception Exception { get; } // The actual exception, if any
        public IStateContext Context { get; }  // The context object related to the error
        public string FsmName { get; }         // Name of the FSM blueprint
        public string StateName { get; }       // Name of the state being processed (if applicable)

        // Constructor would be internal to FSM_API
        internal FsmErrorEventArgs(...) { ... }
    }

    public enum FsmErrorType
    {
        Unknown,
        ConfigurationError,
        RuntimeError,
        InvalidOperation,
        // ... more types as needed
    }
}
*/

Understanding FsmErrorEventArgs

The FsmErrorEventArgs class provides rich details.
This helps you diagnose the problem.

    ErrorType: Categorizes the error.
    Such as ConfigurationError or RuntimeError.

    Message: A human-readable description.

    Exception: The actual exception object, if one occurred.
    This provides the full stack trace.

    Context: The IStateContext instance involved.
    This helps identify which game object or system.
    Caused or was affected by the error.

    FsmName: The name of the FSM blueprint.

    StateName: The name of the current state.
    Where the error originated (if applicable).