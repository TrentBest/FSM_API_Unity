using Debug = UnityEngine.Debug;

namespace TheSingularityWorkshop.FSM.API
{
    public class FSMHandle
    {
        readonly FSM _def;
        public  IStateContext Context;
        public string currentState;
        public string Name => _def.name;
        
        public FSMHandle(FSM def, IStateContext ctx)
        {
            Debug.Log($"Creating FSM handle '{def.name}'");
            _def = def;
            Context = ctx;
            currentState = _def.initialState;
            _def.EnterInitial(ctx);
        }

        public void Update()
        {
            Debug.Log($"Updating FSM '{_def.name}'");
            _def.Step(currentState, Context, out currentState);
        }

        public void TransitionTo(string next)
        {
            _def.ForceTransition(currentState, next, Context);
            currentState = next;
        }
    }
}