using System;

using Debug = UnityEngine.Debug;

namespace TheSingularityWorkshop.FSM.API
{
    // ——————————
    // 1) A strongly?typed FSM that carries a TContext
    // ——————————
    public class FSMState
    {
        public string Name { get; }
        readonly Action<IStateContext> _onEnter, _onUpdate, _onExit;

        public FSMState(string name, Action<IStateContext> onEnter = null, Action<IStateContext> onUpdate = null, Action<IStateContext> onExit = null)
        {
            Debug.Log($"Creating Generic state '{name}'");
            Name = name;
            _onEnter = onEnter ?? (_ => { Debug.Log($"DefaultOnEnter"); });
            _onUpdate = onUpdate ?? (_ => { Debug.Log($"DefaultOnUpdate"); });
            _onExit = onExit ?? (_ => { Debug.Log($"DefaultOnExit"); });
        }

        public void Enter(IStateContext c)
        {
            Debug.Log($"{c.Name} Entering State '{Name}'");
            _onEnter(c);
        }
        public void Update(IStateContext c)
        {
            Debug.Log($"{c.Name} Updating State '{Name}'");
            _onUpdate(c);
        }
        public void Exit(IStateContext c)
        {
            Debug.Log($"{c.Name} Exiting State '{Name}'");
            _onExit(c);
        }
    }
}