using System.Collections.Generic;

namespace TheSingularityWorkshop.FSM.API
{

    /// <summary>
    /// Base interface for all FSM contexts.
    /// </summary>
    public interface IStateContext : IContext
    {
        bool EnteredState { get; }
        bool ShouldTransition { get; }
    }

    public interface IContext
    {
        string Name { get; }

    }


    public interface IProvider<T> where T : IContext
    {
        List<string> ListServices();
        IService<T> GetProvider(string name);
    }

    public interface IService<T> where T : IContext
    {

    }
}