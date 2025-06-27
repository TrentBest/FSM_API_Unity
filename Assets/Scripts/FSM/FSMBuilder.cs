using System;
using System.Collections.Generic;

using Debug = UnityEngine.Debug;

namespace TheSingularityWorkshop.FSM.API
{
    public class FSMBuilder
    {
        private string _fsmName = "UnNamedFSM";
        private int _processRate;
        private readonly List<FSMState> _states = new();
        private readonly List<FSMTransition> _transitions = new();
        private FSM _alreadyBuilt = null;
        private string initialState;
        private string _updateCategory = "Update";

        // now internal, and takes the processRate
        internal FSMBuilder(string fsmName, int processRate = 0, string updateCategory = "Update")
        {
            _fsmName = fsmName;
            _processRate = processRate;
            _updateCategory = updateCategory;
        }
        internal FSMBuilder(FSM fsm)
        {
            _alreadyBuilt = fsm;
            _fsmName = _alreadyBuilt.name;
            _updateCategory = fsm.updateCategory;
        }
        public FSMBuilder State(
            string name,
            Action<IStateContext> onEnter = null,
            Action<IStateContext> onUpdate = null,
            Action<IStateContext> onExit = null)
        {
            Debug.Log($"[FSMBuilder] Adding state '{name}'");
            _states.Add(new FSMState(name, onEnter, onUpdate, onExit));
            return this;
        }

        public FSMBuilder WithProcessRate(int rate)
        {
            Debug.Log($"[FSMBuilder] With Process Rate:  {rate}");
            if (_alreadyBuilt != null)
            {
                return this;
            }
            _processRate = rate;
            return this;
        }

        public FSMBuilder WithName(string name)
        {
            Debug.Log($"[FSMBuilder] With Name:  {name}");
            if (_alreadyBuilt != null)
            {
                return this;
            }
            _fsmName = name;
            return this;
        }

        public FSMBuilder WithInitialState(string name)
        {
            Debug.Log($"[FSMBuilder] With Initial State:  {name}");
            if (_alreadyBuilt != null)
            {
                return this;
            }
            initialState = name;
            return this;
        }
        public FSMBuilder Transition(
            string from,
            string to,
            Func<IStateContext, bool> condition)
        {
            Debug.Log($"[FSMBuilder] Adding transition '{from}' ? '{to}'");
            if (_alreadyBuilt != null)
            {
                return this;
            }
            _transitions.Add(new FSMTransition(from, to, condition));
            return this;
        }

        public FSMBuilder WithUpdateCategory(string category)
        {
            _updateCategory = category;
            return this;
        }

        public void BuildDefinition()
        {
            Debug.Log($"[FSMBuilder] Building FSM '{_fsmName}' (rate={_processRate})");
            if (_alreadyBuilt != null)
            {
                return;
            }
            // 1) Build the strongly?typed machine
            var machine = new FSM();
            foreach (var s in _states) machine.AddState(s);
            foreach (var t in _transitions) machine.AddTransition(t.From, t.To, t.Condition);
            if (initialState == string.Empty)
            {
                machine.initialState = _states[0].Name;
            }
            else
            {
                machine.initialState = initialState;
            }
            machine.processRate = _processRate;
            machine.name = _fsmName;
            // 2) Register under the looser IStateContext API:
            //    cast is safe because TCtx : IStateContext
            FSM_API.Register(
                    _fsmName,
                    machine as FSM,
                    _processRate);
            _alreadyBuilt = machine;
            //// 3) Clear so double?builds are no?ops
            //_states.Clear();
            //_transitions.Clear();
        }
    }
}