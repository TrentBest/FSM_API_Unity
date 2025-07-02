using System;
using System.Collections.Generic;
using System.Linq;

namespace TheSingularityWorkshop.FSM.API
{
    /// <summary>
    /// Provides a fluent API for defining Finite State Machines (FSMs).
    /// Use this builder to specify states, transitions, and initial settings
    /// for your FSM definition before registering it with the <see cref="FSM_API"/>.
    /// </summary>
    public class FSMBuilder
    {
        private string _fsmName = "UnNamedFSM";
        private int _processRate;
        private readonly List<FSMState> _states = new();
        private readonly List<FSMTransition> _transitions = new();
        private string _initialState;
        private string _processGroup = "Update";

        /// <summary>
        /// Initializes a new FSMBuilder for creating a new FSM definition.
        /// This constructor is intended for internal API use via <see cref="FSM_API.CreateFiniteStateMachine"/>.
        /// </summary>
        /// <param name="fsmName">The unique name for the FSM definition.</param>
        /// <param name="processRate">The default process rate for instances of this FSM.</param>
        /// <param name="updateCategory">The update category this FSM belongs to.</param>
        public FSMBuilder(string fsmName, int processRate = 0, string updateCategory = "Update")
        {
            _fsmName = fsmName;
            _processRate = processRate;
            _processGroup = updateCategory;
        }

        /// <summary>
        /// Initializes an FSMBuilder for an already existing FSM definition, allowing it to be modified.
        /// This constructor is intended for internal API use via <see cref="FSM_API.CreateFiniteStateMachine"/>.
        /// </summary>
        /// <param name="fsm">The existing FSM definition to load and potentially modify.</param>
        public FSMBuilder(FSM fsm)
        {
            if (fsm == null)
            {
                throw new ArgumentNullException(nameof(fsm), "Cannot initialize FSMBuilder with a null FSM definition.");
            }

            _fsmName = $"{fsm.Name}2";
            _processRate = fsm.ProcessRate;
            _initialState = fsm.InitialState;
            _processGroup = fsm.ProcessingGroup;

            // Load existing states and transitions into the builder for modification
            foreach (var stateKvp in fsm.GetAllStates())
            {
                _states.Add(stateKvp);
            }
            foreach (var transition in fsm.GetAllTransitions())
            {
                _transitions.Add(transition);
            }
        }

        /// <summary>
        /// Adds a state to the FSM definition.
        /// </summary>
        // ... (no change to logic, just remove _alreadyBuilt checks if they were there)
        public FSMBuilder State(string name, Action<IStateContext> onEnter = null, Action<IStateContext> onUpdate = null, Action<IStateContext> onExit = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("State name cannot be null or empty.", nameof(name));
            }
            if (_states.Any(s => s.Name == name))
            {
                throw new ArgumentException($"State with name '{name}' already exists in this FSM definition.", nameof(name));
            }

            _states.Add(new FSMState(name, onEnter, onUpdate, onExit));
            return this;
        }

        /// <summary>
        /// Sets the processing rate for instances of this FSM definition.
        /// </summary>
        // ... (no change to logic)
        public FSMBuilder WithProcessRate(int rate)
        {
            _processRate = rate;
            return this;
        }

        /// <summary>
        /// Sets the unique name for this FSM definition.
        /// </summary>
        // ... (no change to logic)
        public FSMBuilder WithName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("FSM name cannot be null or empty.", nameof(name));
            }
            _fsmName = name;
            return this;
        }

        /// <summary>
        /// Sets the initial state for the FSM definition.
        /// This state must be added using the <see cref="State"/> method before calling <see cref="BuildDefinition"/>.
        /// </summary>
        // ... (no change to logic)
        public FSMBuilder WithInitialState(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Initial state name cannot be null or empty.", nameof(name));
            }
            _initialState = name;
            return this;
        }

        /// <summary>
        /// Adds a transition between two states.
        /// </summary>
        // ... (no change to logic)
        public FSMBuilder Transition(string from, string to, Func<IStateContext, bool> condition)
        {
            if (string.IsNullOrWhiteSpace(from))
            {
                throw new ArgumentException("'From' state name cannot be null or empty.", nameof(from));
            }
            if (string.IsNullOrWhiteSpace(to))
            {
                throw new ArgumentException("'To' state name cannot be null or empty.", nameof(to));
            }
            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition), "Transition condition cannot be null.");
            }

            _transitions.Add(new FSMTransition(from, to, condition));
            return this;
        }

        /// <summary>
        /// Sets the update category for this FSM definition.
        /// </summary>
        // ... (no change to logic)
        public FSMBuilder WithUpdateCategory(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                throw new ArgumentException("Update category cannot be null or empty.", nameof(category));
            }
            _processGroup = category;
            return this;
        }

        /// <summary>
        /// Finalizes the FSM definition and registers it with the <see cref="FSM_API"/>.
        /// This method ensures the FSM is ready to have instances created from it.
        /// If an FSM definition with the same name already exists, it will be updated.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if no states have been defined or if the initial state is invalid.</exception>
        /// <exception cref="ArgumentException">Thrown if the specified initial state does not exist.</exception>
        public void BuildDefinition()
        {
            // --- Validation before building ---
            if (_states.Count == 0)
            {
                throw new InvalidOperationException($"FSM '{_fsmName}' cannot be built: No states have been defined. Use .State() to add states.");
            }

            string finalInitialState;
            if (string.IsNullOrWhiteSpace(_initialState))
            {
                // If no initial state explicitly set, use the first added state
                finalInitialState = _states[0].Name;
            }
            else
            {
                // Validate that the specified initial state actually exists
                if (!_states.Any(s => s.Name == _initialState))
                {
                    throw new ArgumentException($"Initial state '{_initialState}' specified for FSM '{_fsmName}' does not exist. Ensure you add it with .State() before building.", nameof(_initialState));
                }
                finalInitialState = _initialState;
            }

            // --- Build the FSM ---
            var machine = new FSM
            {
                Name = _fsmName,
                ProcessRate = _processRate,
                InitialState = finalInitialState,
                ProcessingGroup = _processGroup
            };

            foreach (var s in _states)
            {
                machine.AddState(s);
            }
            foreach (var t in _transitions)
            {
                machine.AddTransition(t.From, t.To, t.Condition);
            }

            // Register with FSM_API. This handles new registration or updating an existing one.
            FSM_API.Register(
                _fsmName,
                machine,
                _processRate, 
                _processGroup);

            // Clear builder state to prevent accidental reuse of internal lists for a new build operation.
            _states.Clear();
            _transitions.Clear();
            _initialState = null; 
            _processRate = 0;
            _processGroup = "Update"; 
        }
    }
}