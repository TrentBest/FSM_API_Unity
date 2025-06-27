using System;
using System.Collections.Generic;

using Debug = UnityEngine.Debug;

namespace TheSingularityWorkshop.FSM.API
{
    public class FSM : IService<IStateContext>
    {
        public string name;
        public string initialState;
        readonly Dictionary<string, FSMState> _states = new();
        readonly List<FSMTransition> _transitions = new();
        public int processRate = 0;//0 means purely driven FSM, -1 means every frame, n > 0 executes every n frames
        public string updateCategory = "Update";

        public void AddState(FSMState s)
        {
            Debug.Log($"Adding state '{s.Name}'");
            if (initialState == string.Empty)
            {
                initialState = s.Name;
            }
            _states[s.Name] = s;
        }
        public void AddTransition(string from, string to, Func<IStateContext, bool> cond)
        {
            Debug.Log($"Adding transition from '{from}' to '{to}'");
            _transitions.Add(new FSMTransition(from, to, cond));
        }

        internal void EnterInitial(IStateContext ctx)
        {
            Debug.Log($"Entering FSM '{name}'");
            if (!_states.ContainsKey(initialState)) throw new ArgumentException($"Unknown state '{name}'");
            _states[initialState].Enter(ctx);
        }

        internal void Step(string current, IStateContext ctx, out string next)
        {
            Debug.Log($"Stepping FSM '{current}'");
            _states[current].Update(ctx);
            foreach (var t in _transitions)
                if (t.From == current && t.Condition(ctx))
                {
                    _states[current].Exit(ctx);
                    _states[t.To].Enter(ctx);
                    next = t.To;
                    return;
                }
            next = current;
        }

        internal void ForceTransition(string from, string to, IStateContext ctx)
        {
            Debug.Log($"Force transition from '{from}' to '{to}'");
            if (!_states.ContainsKey(to)) return;
            _states[from].Exit(ctx);
            _states[to].Enter(ctx);
        }
    }
}