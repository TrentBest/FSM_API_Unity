using System;
using System.Linq;
using NUnit.Framework;
using TheSingularityWorkshop.FSM.API;

namespace TheSingularityWorkshop.FSM.Tests
{
    // Simple test double for IStateContext
    public class FSMTestContext : IStateContext
    {
        public string Name { get; set; }
        public bool IsValid { get; set; } = true;
    }

    [TestFixture]
    public class FSMTests
    {
        private API.FSM _fsm;
        private FSMTestContext _context;

        [SetUp]
        public void SetUp()
        {
            _fsm = new API.FSM { Name = "TestFSM", InitialState = "Idle" };
            _context = new FSMTestContext { Name = "TestContext" };
        }

        [Test]
        public void AddState_AddsAndOverwritesStates()
        {
            var state1 = new FSMState("Idle");
            var state2 = new FSMState("Idle", c => { }); // Overwrite
            _fsm.AddState(state1);
            Assert.IsTrue(_fsm.HasState("Idle"));
            _fsm.AddState(state2);
            Assert.AreEqual(state2, _fsm.GetAllStates().First(s => s.Name == "Idle"));
        }

        [Test]
        public void AddState_NullState_ReportsError()
        {
            bool errorCalled = false;
            FSM_API.OnInternalApiError += (msg, ex) => errorCalled = true;
            _fsm.AddState(null);
            FSM_API.OnInternalApiError -= (msg, ex) => errorCalled = true;
            Assert.IsFalse(_fsm.HasState("null"));
            Assert.IsTrue(errorCalled);
        }

        [Test]
        public void AddTransition_AddsAndReplacesTransition()
        {
            _fsm.AddState(new FSMState("A"));
            _fsm.AddState(new FSMState("B"));
            Func<IStateContext, bool> cond1 = c => true;
            Func<IStateContext, bool> cond2 = c => false;
            _fsm.AddTransition("A", "B", cond1);
            Assert.AreEqual(1, _fsm.GetAllTransitions().Count);
            _fsm.AddTransition("A", "B", cond2); // Should replace
            Assert.AreEqual(1, _fsm.GetAllTransitions().Count);
            Assert.AreEqual(cond2, _fsm.GetAllTransitions().First().Condition);
        }

        [Test]
        public void AddTransition_NullCondition_ReportsError()
        {
            bool errorCalled = false;
            FSM_API.OnInternalApiError += (msg, ex) => errorCalled = true;
            _fsm.AddTransition("A", "B", null);
            FSM_API.OnInternalApiError -= (msg, ex) => errorCalled = true;
            Assert.IsTrue(errorCalled);
        }

        [Test]
        public void AddAnyStateTransition_AddsAndReplacesAnyStateTransition()
        {
            _fsm.AddState(new FSMState("B"));
            Func<IStateContext, bool> cond1 = c => true;
            Func<IStateContext, bool> cond2 = c => false;
            _fsm.AddAnyStateTransition("B", cond1);
            Assert.AreEqual(1, _fsm.GetAllTransitions().Count);
            _fsm.AddAnyStateTransition("B", cond2); // Should replace
            Assert.AreEqual(1, _fsm.GetAllTransitions().Count);
            Assert.AreEqual(cond2, _fsm.GetAllTransitions().First().Condition);
        }

        [Test]
        public void AddAnyStateTransition_NullCondition_ReportsError()
        {
            bool errorCalled = false;
            FSM_API.OnInternalApiError += (msg, ex) => errorCalled = true;
            _fsm.AddAnyStateTransition("B", null);
            FSM_API.OnInternalApiError -= (msg, ex) => errorCalled = true;
            Assert.IsTrue(errorCalled);
        }

        [Test]
        public void HasState_ReturnsCorrectly()
        {
            Assert.IsFalse(_fsm.HasState("Idle"));
            _fsm.AddState(new FSMState("Idle"));
            Assert.IsTrue(_fsm.HasState("Idle"));
        }

        [Test]
        public void GetAllStates_ReturnsAllStates()
        {
            _fsm.AddState(new FSMState("A"));
            _fsm.AddState(new FSMState("B"));
            var states = _fsm.GetAllStates();
            Assert.AreEqual(2, states.Count);
            Assert.IsTrue(states.Any(s => s.Name == "A"));
            Assert.IsTrue(states.Any(s => s.Name == "B"));
        }

        [Test]
        public void GetAllTransitions_ReturnsAllTransitions()
        {
            _fsm.AddState(new FSMState("A"));
            _fsm.AddState(new FSMState("B"));
            _fsm.AddTransition("A", "B", c => true);
            _fsm.AddAnyStateTransition("B", c => false);
            var transitions = _fsm.GetAllTransitions();
            Assert.AreEqual(2, transitions.Count);
        }

        [Test]
        public void EnterInitial_ThrowsAndReportsError_IfInitialStateMissing()
        {
            _fsm.InitialState = "Missing";
            bool errorCalled = false;
            FSM_API.OnInternalApiError += (msg, ex) => errorCalled = true;
            Assert.Throws<ArgumentException>(() => _fsm.EnterInitial(_context));
            FSM_API.OnInternalApiError -= (msg, ex) => errorCalled = true;
            Assert.IsTrue(errorCalled);
        }

        [Test]
        public void EnterInitial_CallsEnterOnInitialState()
        {
            bool entered = false;
            _fsm.AddState(new FSMState("Idle", c => entered = true));
            _fsm.EnterInitial(_context);
            Assert.IsTrue(entered);
        }

        [Test]
        public void Step_RecoversIfCurrentStateMissing()
        {
            _fsm.AddState(new FSMState("Idle"));
            _fsm.InitialState = "Idle";
            bool errorCalled = false;
            FSM_API.OnInternalApiError += (msg, ex) => errorCalled = true;
            string next;
            _fsm.Step("Missing", _context, out next);
            FSM_API.OnInternalApiError -= (msg, ex) => errorCalled = true;
            Assert.AreEqual("Idle", next);
            Assert.IsTrue(errorCalled);
        }

        [Test]
        public void Step_AnyStateTransition_TakesPriority()
        {
            bool exited = false, entered = false;
            _fsm.AddState(new FSMState("A", null, null, c => exited = true));
            _fsm.AddState(new FSMState("B", c => entered = true));
            _fsm.AddAnyStateTransition("B", c => true);
            string next;
            _fsm.Step("A", _context, out next);
            Assert.IsTrue(exited);
            Assert.IsTrue(entered);
            Assert.AreEqual("B", next);
        }

        [Test]
        public void Step_RegularTransition_IfNoAnyState()
        {
            bool exited = false, entered = false;
            _fsm.AddState(new FSMState("A", null, null, c => exited = true));
            _fsm.AddState(new FSMState("B", c => entered = true));
            _fsm.AddTransition("A", "B", c => true);
            string next;
            _fsm.Step("A", _context, out next);
            Assert.IsTrue(exited);
            Assert.IsTrue(entered);
            Assert.AreEqual("B", next);
        }

        [Test]
        public void Step_UpdatesState_IfNoTransition()
        {
            bool updated = false;
            _fsm.AddState(new FSMState("A", null, c => updated = true));
            string next;
            _fsm.Step("A", _context, out next);
            Assert.IsTrue(updated);
            Assert.AreEqual("A", next);
        }

        [Test]
        public void Step_TransitionToNonExistentState_ReportsError()
        {
            _fsm.AddState(new FSMState("A"));
            _fsm.AddAnyStateTransition("Missing", c => true);
            bool errorCalled = false;
            FSM_API.OnInternalApiError += (msg, ex) => errorCalled = true;
            string next;
            _fsm.Step("A", _context, out next);
            FSM_API.OnInternalApiError -= (msg, ex) => errorCalled = true;
            Assert.IsTrue(errorCalled);
            Assert.AreEqual("A", next);
        }

        [Test]
        public void Step_TransitionConditionThrows_ReportsError()
        {
            _fsm.AddState(new FSMState("A"));
            _fsm.AddAnyStateTransition("A", c => throw new Exception("Condition failed"));
            bool errorCalled = false;
            FSM_API.OnInternalApiError += (msg, ex) => errorCalled = true;
            string next;
            _fsm.Step("A", _context, out next);
            FSM_API.OnInternalApiError -= (msg, ex) => errorCalled = true;
            Assert.IsTrue(errorCalled);
            Assert.AreEqual("A", next);
        }

        [Test]
        public void Step_UpdateThrows_ReportsError()
        {
            _fsm.AddState(new FSMState("A", null, c => throw new Exception("Update failed")));
            bool errorCalled = false;
            FSM_API.OnInternalApiError += (msg, ex) => errorCalled = true;
            string next;
            _fsm.Step("A", _context, out next);
            FSM_API.OnInternalApiError -= (msg, ex) => errorCalled = true;
            Assert.IsTrue(errorCalled);
            Assert.AreEqual("A", next);
        }

        [Test]
        public void ForceTransition_ExitsAndEntersStates()
        {
            bool exited = false, entered = false;
            _fsm.AddState(new FSMState("A", null, null, c => exited = true));
            _fsm.AddState(new FSMState("B", c => entered = true));
            _fsm.ForceTransition("A", "B", _context);
            Assert.IsTrue(exited);
            Assert.IsTrue(entered);
        }

        [Test]
        public void ForceTransition_ThrowsAndReportsError_IfTargetStateMissing()
        {
            _fsm.AddState(new FSMState("A"));
            bool errorCalled = false;
            FSM_API.OnInternalApiError += (msg, ex) => errorCalled = true;
            Assert.Throws<ArgumentException>(() => _fsm.ForceTransition("A", "Missing", _context));
            FSM_API.OnInternalApiError -= (msg, ex) => errorCalled = true;
            Assert.IsTrue(errorCalled);
        }

        [Test]
        public void ForceTransition_ReportsError_IfFromStateMissing()
        {
            _fsm.AddState(new FSMState("B"));
            bool errorCalled = false;
            FSM_API.OnInternalApiError += (msg, ex) => errorCalled = true;
            _fsm.ForceTransition("Missing", "B", _context);
            FSM_API.OnInternalApiError -= (msg, ex) => errorCalled = true;
            Assert.IsTrue(errorCalled);
        }

        [Test]
        public void ForceTransition_ExitThrows_ReportsError_AndStillEnters()
        {
            bool entered = false;
            _fsm.AddState(new FSMState("A", null, null, c => throw new Exception("Exit failed")));
            _fsm.AddState(new FSMState("B", c => entered = true));
            bool errorCalled = false;
            FSM_API.OnInternalApiError += (msg, ex) => errorCalled = true;
            _fsm.ForceTransition("A", "B", _context);
            FSM_API.OnInternalApiError -= (msg, ex) => errorCalled = true;
            Assert.IsTrue(errorCalled);
            Assert.IsTrue(entered);
        }

        [Test]
        public void ForceTransition_EnterThrows_ReportsError_AndThrows()
        {
            _fsm.AddState(new FSMState("A"));
            _fsm.AddState(new FSMState("B", c => throw new Exception("Enter failed")));
            bool errorCalled = false;
            FSM_API.OnInternalApiError += (msg, ex) => errorCalled = true;
            Assert.Throws<Exception>(() => _fsm.ForceTransition("A", "B", _context));
            FSM_API.OnInternalApiError -= (msg, ex) => errorCalled = true;
            Assert.IsTrue(errorCalled);
        }
    }
}