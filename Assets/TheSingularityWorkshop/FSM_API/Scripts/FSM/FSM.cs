using System;
using System.Collections.Generic;
using System.Linq; // For .Any() and .Contains()

// Removed: using Debug = UnityEngine.Debug; // No longer needed, replace with OnInternalApiError

namespace TheSingularityWorkshop.FSM.API
{
    // Assuming IService interface looks something like this (for context):
    // public interface IService<TContext> { /* maybe some lifecycle methods? */ }

    /// <summary>
    /// Represents a Finite State Machine definition, containing states and transitions.
    /// Instances of this definition are created via FSM_API to manage stateful contexts.
    /// </summary>
    public class FSM
    {
        /// <summary>
        /// Gets or sets the unique name of this FSM definition.
        /// </summary>
        public string Name { get;  set; }

        /// <summary>
        /// Gets or sets the name of the initial state for this FSM.
        /// When an FSM instance is created, it will start in this state.
        /// </summary>
        public string InitialState { get;  set; }

        /// <summary>
        /// Gets or sets the processing rate for instances of this FSM definition.
        /// This determines how often the FSM's <see cref="Step"/> method is called
        /// by the <see cref="FSM_API.TickAll"/> methods.
        /// <list type="bullet">
        ///    <item><term>-1</term><description>Updates every single frame.</description></item>
        ///    <item><term>0</term><description>Never updated automatically by the API's Tick methods (must be driven by events or manual calls).</description></item>
        ///    <item><term>&gt;0</term><description>Updates every Nth frame (e.g., a value of 5 means update every 5th frame).</description></item>
        /// </list>
        /// </summary>
        public int ProcessRate { get; internal set; }

        /// <summary>
        /// Gets or sets the processing group this FSM definition belongs to.
        /// This dictates which <see cref="FSM_API.Update()"/>, <see cref="FSM_API.FixedUpdate()"/>,
        /// or <see cref="FSM_API.LateUpdate()"/> call will process this FSM's instances.
        /// </summary>
        public string ProcessingGroup { get; internal set; }

        /// <summary>
        /// Internal identifier for "Any State" transitions. Transitions from this
        /// pseudo-state can occur from any active state in the FSM.
        /// </summary>
        public const string AnyStateIdentifier = "__ANY_STATE__"; // Using a unique, unlikely string

        // Internal collections for states and transitions
        private readonly Dictionary<string, FSMState> _states = new();
        private readonly List<FSMTransition> _transitions = new(); // Regular transitions
        private readonly List<FSMTransition> _anyStateTransitions = new(); // Any State transitions

        /// <summary>
        /// Initializes a new instance of the <see cref="FSM"/> class.
        /// Default properties will be set by the <see cref="FSMBuilder"/>.
        /// </summary>
        public FSM()
        {
            // Default constructor is fine. Properties will be set by FSMBuilder.
        }

        /// <summary>
        /// Adds or updates a state in the FSM definition. If a state with the same name
        /// already exists, it will be overwritten.
        /// </summary>
        /// <param name="s">The <see cref="FSMState"/> object to add.</param>
        /// <exception cref="ArgumentNullException">Thrown if the provided state is null.</exception>
        public void AddState(FSMState s)
        {
            if (s == null)
            {
                // Corrected: Use FSM_API's internal event invocation helper
                FSM_API.InvokeInternalApiError(FSMErrorType.InvalidOperation, 
                    $"Attempted to add a null state to FSM '{Name}'.", 
                    new ArgumentNullException(nameof(s)));
                return;
            }

            _states[s.Name] = s;
        }

        /// <summary>
        /// Adds a regular transition between two specific states.
        /// If a transition with the same 'from' and 'to' states already exists, it will be replaced.
        /// </summary>
        /// <param name="from">The name of the source state for the transition.</param>
        /// <param name="to">The name of the target state for the transition.</param>
        /// <param name="cond">The condition function that must return true for the transition to occur.</param>
        /// <exception cref="ArgumentNullException">Thrown if the condition function is null.</exception>
        public void AddTransition(string from, string to, Func<IStateContext, bool> cond)
        {
            if (cond == null)
            {
                // Corrected: Use FSM_API's internal event invocation helper
                FSM_API.InvokeInternalApiError(FSMErrorType.InvalidOperation, $"Attempted to add a transition with null condition from '{from}' to '{to}' in FSM '{Name}'.", new ArgumentNullException(nameof(cond)));
                return;
            }

            // Remove existing transition if it matches (from, to) pair for clean updates
            _transitions.RemoveAll(t => t.From == from && t.To == to);
            _transitions.Add(new FSMTransition(from, to, cond));
        }

        /// <summary>
        /// Adds a global "Any State" transition. An "Any State" transition can be
        /// triggered from any state within the FSM if its condition is met.
        /// If an "Any State" transition to the same 'to' state already exists, it will be replaced.
        /// </summary>
        /// <param name="to">The name of the target state for the "Any State" transition.</param>
        /// <param name="cond">The condition function that must return true for the transition to occur.</param>
        /// <exception cref="ArgumentNullException">Thrown if the condition function is null.</exception>
        public void AddAnyStateTransition(string to, Func<IStateContext, bool> cond)
        {
            if (cond == null)
            {
                FSM_API.InvokeInternalApiError(FSMErrorType.InvalidOperation, $"Attempted to add an Any-State transition with null condition to '{to}' in FSM '{Name}'.", new ArgumentNullException(nameof(cond)));
                return;
            }

            // Remove existing any-state transition if it matches 'to' state for clean updates
            _anyStateTransitions.RemoveAll(t => t.To == to);
            _anyStateTransitions.Add(new FSMTransition(AnyStateIdentifier, to, cond));
        }

        /// <summary>
        /// Checks if a state with the given name exists in this FSM definition.
        /// </summary>
        /// <param name="stateName">The name of the state to check.</param>
        /// <returns>True if the state exists, false otherwise.</returns>
        public bool HasState(string stateName)
        {
            return _states.ContainsKey(stateName);
        }

        /// <summary>
        /// Gets a read-only collection of all states defined in this FSM.
        /// </summary>
        /// <returns>A read-only collection of <see cref="FSMState"/> objects.</returns>
        public IReadOnlyCollection<FSMState> GetAllStates()
        {
            return _states.Values.ToList().AsReadOnly();
        }

        /// <summary>
        /// Gets a read-only collection of all transitions (regular and any-state) defined in this FSM.
        /// </summary>
        /// <returns>A read-only collection of <see cref="FSMTransition"/> objects.</returns>
        public IReadOnlyCollection<FSMTransition> GetAllTransitions()
        {
            // Combine regular and any-state transitions
            var allTransitions = new List<FSMTransition>(_transitions);
            allTransitions.AddRange(_anyStateTransitions);
            return allTransitions.AsReadOnly();
        }

        /// <summary>
        /// Enters the initial state of the FSM for a given context.
        /// This method is typically called once when an FSMHandle is created.
        /// </summary>
        /// <param name="ctx">The context object for the FSM instance.</param>
        /// <exception cref="ArgumentException">Thrown if the <see cref="InitialState"/> is not found in the FSM definition.</exception>
        public void EnterInitial(IStateContext ctx)
        {
            if (!_states.TryGetValue(InitialState, out var state))
            {
                
                FSM_API.InvokeInternalApiError(FSMErrorType.RuntimeError,
                    $"Initial state '{InitialState}' not found for FSM '{Name}'. This indicates a corrupted FSM definition.",
                    new ArgumentException($"Initial state '{InitialState}' not found for FSM '{Name}'.",
                    nameof(InitialState))
                );
                // Even though we invoke an error, we still throw to prevent the FSM from operating in an invalid state.
                throw new ArgumentException($"Initial state '{InitialState}' not found for FSM '{Name}'.", nameof(InitialState));
            }
            state.Enter(ctx);
        }

        /// <summary>
        /// Executes one step of the FSM for a given context. This involves checking
        /// "Any State" transitions, executing the current state's update logic,
        /// and then checking regular transitions from the current state.
        /// </summary>
        /// <param name="current">The name of the current state of the FSM instance.</param>
        /// <param name="ctx">The context object for the FSM instance.</param>
        /// <param name="next">Output parameter: The name of the next state after this step.
        /// This will be the same as 'current' if no transition occurs.</param>
        public void Step(string current, IStateContext ctx, out string next)
        {
            next = current; // Assume state doesn't change unless a transition fires

            if (!_states.TryGetValue(current, out var currentState))
            {
                // This indicates a severe internal inconsistency.
                // The FSMHandle thinks it's in a state that doesn't exist in the definition.
                // Corrected: Use FSM_API's internal event invocation helper
                FSM_API.InvokeInternalApiError(FSMErrorType.RuntimeError,
                    $"FSM '{Name}' instance in processing group '{ProcessingGroup}' has an invalid current state '{current}'. Attempting to recover by transitioning to initial state '{InitialState}'.",
                    null
                );
                // Attempt to recover by transitioning to the initial state
                ForceTransition(current, InitialState, ctx);
                next = InitialState;
                return;
            }

            // 1. Check Any-State Transitions First (higher priority for global interrupts)
            foreach (var t in _anyStateTransitions)
            {
                // Check if the target state exists before evaluating condition
                if (!_states.ContainsKey(t.To))
                {
                    
                    FSM_API.InvokeInternalApiError(FSMErrorType.RuntimeError,
                        $"FSM '{Name}' Any-State transition defined to non-existent state '{t.To}'. Transition skipped for safety.",
                        null
                    );
                    continue; // Skip this problematic transition
                }

                try
                {
                    if (t.Condition(ctx))
                    {
                        currentState.Exit(ctx);
                        _states[t.To].Enter(ctx);
                        next = t.To;
                        return; // Transition occurred, exit
                    }
                }
                catch (Exception ex)
                {
                    // Corrected: Use FSM_API's internal event invocation helper
                    FSM_API.InvokeInternalApiError(FSMErrorType.RuntimeError,
                        $"Error evaluating Any-State transition condition from '{current}' to '{t.To}' in FSM '{Name}'. Exception: {ex.Message}",
                        ex
                    );
                    // Continue to next transition or state update
                }
            }

            // 2. Execute current state's Update logic
            try
            {
                currentState.Update(ctx);
            }
            catch (Exception ex)
            {
                FSM_API.InvokeInternalApiError(FSMErrorType.RuntimeError,
                    $"Error during Update logic of state '{current}' in FSM '{Name}'. Exception: {ex.Message}",
                    ex
                );
                // Continue to check transitions, or let the FSMHandle catch this as a fubar
            }


            // 3. Check regular transitions from the current state
            foreach (var t in _transitions)
            {
                if (t.From == current) // Only consider transitions *from* the current state
                {
                    // Check if the target state exists before evaluating condition
                    if (!_states.ContainsKey(t.To))
                    {
                        
                        FSM_API.InvokeInternalApiError(FSMErrorType.RuntimeError,
                            $"FSM '{Name}' regular transition defined from '{current}' to non-existent state '{t.To}'. Transition skipped for safety.",
                            null
                        );
                        continue; // Skip this problematic transition
                    }

                    try
                    {
                        if (t.Condition(ctx))
                        {
                            currentState.Exit(ctx);
                            _states[t.To].Enter(ctx);
                            next = t.To;
                            return; // Transition occurred, exit
                        }
                    }
                    catch (Exception ex)
                    {
                       
                        FSM_API.InvokeInternalApiError(FSMErrorType.RuntimeError,
                            $"Error evaluating regular transition condition from '{current}' to '{t.To}' in FSM '{Name}'. Exception: {ex.Message}",
                            ex
                        );
                        // Continue to next transition
                    }
                }
            }
        }

        /// <summary>
        /// Forces a transition from one state to another for a given context, bypassing any conditions.
        /// This method executes the 'Exit' action of the 'from' state (if it exists)
        /// and the 'Enter' action of the 'to' state.
        /// </summary>
        /// <param name="from">The name of the state to exit from. Can be null or non-existent if no exit action is desired.</param>
        /// <param name="to">The name of the state to enter into.</param>
        /// <param name="ctx">The context object for the FSM instance.</param>
        /// <exception cref="ArgumentException">Thrown if the target 'to' state does not exist in the FSM definition.</exception>
        public void ForceTransition(string from, string to, IStateContext ctx)
        {
            // Check if the "from" state actually exists and handle gracefully if not
            // (e.g., initial state wasn't entered properly or FSM was in an invalid state).
            // We still try to call Exit if possible.
            if (!string.IsNullOrEmpty(from) && _states.TryGetValue(from, out var fromState))
            {
                try
                {
                    fromState.Exit(ctx);
                }
                catch (Exception ex)
                {
                    // Corrected: Use FSM_API's internal event invocation helper
                    FSM_API.InvokeInternalApiError(FSMErrorType.RuntimeError,
                        $"Error during Exit logic of state '{from}' during forced transition to '{to}' in FSM '{Name}'. Exception: {ex.Message}",
                        ex
                    );
                    // Do not re-throw, continue to enter the new state if possible.
                }
            }
            else if (!string.IsNullOrEmpty(from)) // Only log if 'from' was specified but not found
            {
                // Corrected: Use FSM_API's internal event invocation helper
                FSM_API.InvokeInternalApiError(FSMErrorType.RuntimeError,
                    $"Attempted to force transition from non-existent or null state '{from}' for FSM '{Name}'. Exiting original state skipped.",
                    null
                );
            }

            if (!_states.TryGetValue(to, out var toState))
            {
                // Corrected: Use FSM_API's internal event invocation helper
                FSM_API.InvokeInternalApiError(FSMErrorType.RuntimeError,
                    $"Target state '{to}' for forced transition does not exist in FSM '{Name}'. Forced transition failed.",
                    new ArgumentException($"Target state '{to}' for forced transition does not exist in FSM '{Name}'.", nameof(to))
                );
                throw new ArgumentException($"Target state '{to}' for forced transition does not exist in FSM '{Name}'.", nameof(to));
            }

            try
            {
                toState.Enter(ctx);
            }
            catch (Exception ex)
            {
                // Corrected: Use FSM_API's internal event invocation helper
                FSM_API.InvokeInternalApiError(FSMErrorType.RuntimeError,
                    $"Error during Enter logic of state '{to}' during forced transition from '{from}' in FSM '{Name}'. Exception: {ex.Message}",
                    ex
                );
                throw; // Re-throw, as the FSM might now be in a partially entered state.
            }
        }
    }
}