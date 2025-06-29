using System;

namespace TheSingularityWorkshop.FSM.API
{
    /// <summary>
    /// Represents a single transition rule within a Finite State Machine (FSM) definition.
    /// Defines a source state, a target state, and a condition that must be met for the transition to occur.
    /// </summary>
    public class FSMTransition
    {
        /// <summary>
        /// Gets the name of the source state from which this transition originates.
        /// </summary>
        public string From { get; }

        /// <summary>
        /// Gets the name of the target state to which this transition leads.
        /// </summary>
        public string To { get; }

        /// <summary>
        /// Gets the condition function that must return true for this transition to be taken.
        /// The function receives an <see cref="IStateContext"/> to provide context-specific data.
        /// </summary>
        public Func<IStateContext, bool> Condition { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FSMTransition"/> class.
        /// </summary>
        /// <param name="from">The name of the source state.</param>
        /// <param name="to">The name of the target state.</param>
        /// <param name="condition">The function that defines the condition for this transition.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if 'from' or 'to' state names are null, empty, or whitespace.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the 'condition' function is null.
        /// </exception>
        public FSMTransition(string from, string to, Func<IStateContext, bool> condition)
        {
            if (string.IsNullOrWhiteSpace(from))
            {
                throw new ArgumentException("Transition 'from' state name cannot be null or empty.", nameof(@from));
            }
            if (string.IsNullOrWhiteSpace(to))
            {
                throw new ArgumentException("Transition 'to' state name cannot be null or empty.", nameof(to));
            }
            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition), "Transition condition cannot be null.");
            }

            From = from;
            To = to;
            Condition = condition;
        }

        /// <summary>
        /// Returns a string representation of the FSMTransition.
        /// </summary>
        /// <returns>A string in the format "FromState --[Condition]--> ToState".</returns>
        public override string ToString()
        {
            // We can't easily print the condition's internal details for Func<T, bool>,
            // but we can indicate its presence.
            return $"{From} --[Condition]--> {To}";
        }
    }
}