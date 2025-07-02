namespace TheSingularityWorkshop.FSM.API
{

    /// <summary>
    /// Represents the context object for an FSM instance.
    /// Implement this interface on any object that you want an FSM to control.
    /// </summary>
    public interface IStateContext : IContext
    {
        /// <summary>
        /// Gets a value indicating whether this context object is currently valid and active.
        /// This is crucial for the FSM system to determine if an FSM instance should continue to be processed.
        /// For Unity, this might involve checking if the GameObject or MonoBehaviour is still active and not destroyed.
        /// For plain C# objects, this might involve checking if it has been explicitly "disposed" or marked as invalid.
        /// </summary>
        bool IsValid { get; set; }
    }
}