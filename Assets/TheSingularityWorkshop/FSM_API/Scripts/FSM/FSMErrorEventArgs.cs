using TheSingularityWorkshop.FSM.API;

using UnityEngine;

public class FSMErrorEventArgs : System.EventArgs
{
    /// <summary>
    /// Categorizes the type of error that occurred.
    /// </summary>
    public FSMErrorType ErrorType { get; }

    /// <summary>
    /// A human-readable message describing the error.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// The actual exception object that was caught, if any.
    /// </summary>
    public System.Exception Exception { get; }

    /// <summary>
    /// The IStateContext instance involved in the error, if applicable.
    /// </summary>
    public IStateContext Context { get; }

    /// <summary>
    /// The unique name of the FSM blueprint definition involved in the error.
    /// </summary>
    public string FsmName { get; }

    /// <summary>
    /// The name of the state being processed when the error occurred, if applicable.
    /// </summary>
    public string StateName { get; }

    // Internal constructor to ensure instances are created only within FSM_API
    internal FSMErrorEventArgs(FSMErrorType errorType, string message, System.Exception exception = null,
                               IStateContext context = null, string fsmName = null, string stateName = null)
    {
        ErrorType = errorType;
        Message = message;
        Exception = exception;
        Context = context;
        FsmName = fsmName;
        StateName = stateName;
    }
}
