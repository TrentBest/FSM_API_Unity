using System;

namespace TheSingularityWorkshop.FSM.API
{
    public class FSMTransition
    {
        public string From, To;
        public Func<IStateContext, bool> Condition;
        public FSMTransition(string f, string t, Func<IStateContext, bool> c) { From = f; To = t; Condition = c; }
    }
}