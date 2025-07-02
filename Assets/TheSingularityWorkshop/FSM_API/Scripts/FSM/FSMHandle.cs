using System;
// Removed: using UnityEngine; // Assuming this is no longer needed after FSM_API refactor

namespace TheSingularityWorkshop.FSM.API
{
    /// <summary>
    /// Represents a live instance of a Finite State Machine (FSM) definition.
    /// This handle provides the means to interact with a specific FSM instance,
    /// manage its context, trigger manual transitions, and receive updates.
    /// </summary>
    public class FSMHandle
    {
        /// <summary>
        /// Gets the underlying Finite State Machine definition this handle refers to.
        /// This provides access to the FSM's structure (states, transitions).
        /// </summary>
        /// <remarks>
        /// This is a read-only field set at the time of instance creation.
        /// </remarks>
        public readonly FSM Definition;

        /// <summary>
        /// Gets or sets the context object associated with this FSM instance.
        /// The context object holds the data and methods that the FSM operates on.
        /// It must implement the <see cref="IStateContext"/> interface.
        /// </summary>
        public IStateContext Context { get; set; }

        /// <summary>
        /// Gets the current state of this FSM instance.
        /// </summary>
        /// <remarks>
        /// This property is updated internally by the FSM's <see cref="Update"/> and <see cref="TransitionTo"/> methods.
        /// </remarks>
        public string CurrentState { get; private set; }

        /// <summary>
        /// Gets the name of the FSM definition that this handle is an instance of.
        /// This is a convenience property that retrieves the name from the underlying FSM definition.
        /// </summary>
        public string Name => Definition.Name;

        /// <summary>
        /// Gets whether this FSMHandle is valid (i.e., its context is valid and not null).
        /// </summary>
        public bool IsValid => Context?.IsValid ?? false;

        /// <summary>
        /// Initializes a new instance of the <see cref="FSMHandle"/> class.
        /// </summary>
        /// <param name="definition">The <see cref="FSM"/> definition this handle will represent.</param>
        /// <param name="context">The <see cref="IStateContext"/> object that this FSM instance will operate on.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="definition"/> or <paramref name="context"/> is null.</exception>
        public FSMHandle(FSM definition, IStateContext context)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition), "FSM definition cannot be null for FSMHandle.");
            Context = context ?? throw new ArgumentNullException(nameof(context), "Context cannot be null for FSMHandle.");

            CurrentState = Definition.InitialState;
            Definition.EnterInitial(Context);
        }

        /// <summary>
        /// Advances the FSM by one step, executing the current state's logic
        /// and potentially transitioning to a new state based on defined rules.
        /// This method is typically called by the FSM_API's internal tick loop.
        /// </summary>
        public void Update()
        {
            try
            {
                // The Step method will internally handle state transitions and update CurrentState
                Definition.Step(CurrentState, Context, out string nextState);
                CurrentState = nextState;
            }
            catch (Exception ex)
            {
                // If an exception occurs during the FSM's internal step, report it as a "fubar".
                // This allows the FSM_API to track and potentially remove problematic instances.
                FSM_API.ReportError(this, ex);
            }
        }

        /// <summary>
        /// Forces an immediate transition of the FSM to a specified state,
        /// bypassing normal transition conditions.
        /// </summary>
        /// <param name="nextStateName">The name of the state to transition to.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="nextStateName"/> is null or empty,
        /// or if the target state does not exist within the FSM definition.</exception>
        /// <remarks>
        /// Use with caution, as forcing a transition may bypass intended FSM logic.
        /// This method will execute the 'Exit' action of the current state and
        /// the 'Enter' action of the target state.
        /// </remarks>
        public void TransitionTo(string nextStateName)
        {
            if (string.IsNullOrWhiteSpace(nextStateName))
            {
                throw new ArgumentException("Target state name cannot be null or empty.", nameof(nextStateName));
            }

            try
            {
                Definition.ForceTransition(CurrentState, nextStateName, Context);
                //CurrentState = nextStateName;
            }
            catch (Exception ex)
            {
                // Report any issues during a forced transition as a "fubar".
                FSM_API.ReportError(this, ex);
                throw; // Re-throw the exception as this is a direct user-invoked method.
            }
        }

        public void ResetFSM()
        {
            Definition.ForceTransition(CurrentState, Definition.InitialState, Context);
        }
    }
}