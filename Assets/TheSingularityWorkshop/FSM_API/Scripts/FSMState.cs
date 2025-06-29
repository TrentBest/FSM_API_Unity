using System;

using Debug = UnityEngine.Debug; // Assuming this Debug alias is intentional for Unity

namespace TheSingularityWorkshop.FSM.API
{
    /// <summary>
    /// Represents a single state within a Finite State Machine (FSM) definition.
    /// It defines actions to be executed when entering, updating within, and exiting the state.
    /// </summary>
    public class FSMState
    {
        /// <summary>
        /// Gets the unique name of this state.
        /// </summary>
        public string Name { get; }

        private readonly Action<IStateContext> _onEnter;
        private readonly Action<IStateContext> _onUpdate;
        private readonly Action<IStateContext> _onExit;

        /// <summary>
        /// Initializes a new instance of the <see cref="FSMState"/> class.
        /// </summary>
        /// <param name="name">The unique name for this state.</param>
        /// <param name="onEnter">An optional action to execute when entering this state. Can be null.</param>
        /// <param name="onUpdate">An optional action to execute periodically while in this state. Can be null.</param>
        /// <param name="onExit">An optional action to execute when exiting this state. Can be null.</param>
        /// <exception cref="ArgumentException">Thrown if the <paramref name="name"/> is null, empty, or whitespace.</exception>
        public FSMState(string name, Action<IStateContext> onEnter = null, Action<IStateContext> onUpdate = null, Action<IStateContext> onExit = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("State name cannot be null or empty.", nameof(name));
            }

            // Removed Debug.Log from constructor as it can be noisy during FSM definition.
            // Logging state creation might be better handled when the FSM is built or registered.

            Name = name;
            // Assign actions directly. The methods (Enter, Update, Exit) will handle null checks.
            _onEnter = onEnter;
            _onUpdate = onUpdate;
            _onExit = onExit;
        }

        /// <summary>
        /// Executes the 'onEnter' action for this state, if defined.
        /// This method is typically called by the FSM when transitioning into this state.
        /// </summary>
        /// <param name="c">The state context providing access to FSM instance data.</param>
        public void Enter(IStateContext c)
        {
            Debug.Log($"{c.Name} Entering State '{Name}'");
            _onEnter?.Invoke(c); // Use null-conditional operator to invoke only if not null
        }

        /// <summary>
        /// Executes the 'onUpdate' action for this state, if defined.
        /// This method is typically called periodically by the FSM while it is in this state.
        /// </summary>
        /// <param name="c">The state context providing access to FSM instance data.</param>
        public void Update(IStateContext c)
        {
            // Only log if there's an actual update action, or if you always want update logs.
            // If onUpdate is often null, this log can be noisy. Decided to keep for consistency with Enter/Exit.
            Debug.Log($"{c.Name} Updating State '{Name}'");
            _onUpdate?.Invoke(c); // Use null-conditional operator to invoke only if not null
        }

        /// <summary>
        /// Executes the 'onExit' action for this state, if defined.
        /// This method is typically called by the FSM when transitioning out of this state.
        /// </summary>
        /// <param name="c">The state context providing access to FSM instance data.</param>
        public void Exit(IStateContext c)
        {
            Debug.Log($"{c.Name} Exiting State '{Name}'");
            _onExit?.Invoke(c); // Use null-conditional operator to invoke only if not null
        }

        /// <summary>
        /// Returns a string representation of the FSMState.
        /// </summary>
        /// <returns>A string in the format "FSMState: [StateName]".</returns>
        public override string ToString()
        {
            return $"FSMState: {Name}";
        }
    }
}