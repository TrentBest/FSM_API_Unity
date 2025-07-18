using UnityEngine;

public enum FSMErrorType
{
    Unknown,
    /// <summary>
    /// An error related to the definition of an FSM (e.g., duplicate state name, invalid transition).
    /// </summary>
    DefinitionError,
    /// <summary>
    /// A runtime error that occurred during FSM processing (e.g., an exception in an OnUpdate action).
    /// </summary>
    RuntimeError,
    /// <summary>
    /// An error where an invalid operation was attempted (e.g., trying to transition to a non-existent state).
    /// </summary>
    InvalidOperation,
    /// <summary>
    /// An error related to an FSM instance's context (e.g., context becoming null or invalid during processing).
    /// </summary>
    ContextError,
    // Add more specific types as needed during future development
}
