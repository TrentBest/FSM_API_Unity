namespace TheSingularityWorkshop.FSM.API
{
    /// <summary>
    /// Base interface for all FSM contexts.
    /// Provides a common contract for objects that FSMs operate upon.
    /// </summary>
    public interface IContext
    {
        /// <summary>
        /// Gets or sets the name of the context object.
        /// Useful for debugging and identification.
        /// </summary>
        string Name { get; set; }
    }
}