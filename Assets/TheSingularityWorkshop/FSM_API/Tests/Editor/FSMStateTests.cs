using System;
using NUnit.Framework;
using TheSingularityWorkshop.FSM.API;
using UnityEngine;
using UnityEngine.TestTools; // For LogAssert

namespace TheSingularityWorkshop.FSM.Tests
{
    // Simple test double for IStateContext
    public class StateTestContext : IStateContext
    {
        public string Name { get; set; }
        public bool IsValid { get; set; } = true;
    }

    [TestFixture]
    public class FSMStateTests
    {
        private StateTestContext _context;

        [SetUp]
        public void SetUp()
        {
            _context = new StateTestContext { Name = "TestContext", IsValid = true };
        }

        [Test]
        public void Constructor_ThrowsArgumentException_WhenNameIsNullOrWhitespace()
        {
            Assert.Throws<ArgumentException>(() => new FSMState(null));
            Assert.Throws<ArgumentException>(() => new FSMState(""));
            Assert.Throws<ArgumentException>(() => new FSMState("   "));
        }

        [Test]
        public void Name_Property_ReturnsConstructorValue()
        {
            var state = new FSMState("MyState");
            Assert.AreEqual("MyState", state.Name);
        }

        [Test]
        public void Enter_InvokesOnEnterAction_AndLogs()
        {
            bool called = false;
            var state = new FSMState("EnterState", c => called = true);

           // LogAssert.Expect(LogType.Log, $"{_context.Name} Entering State 'EnterState'");
            state.Enter(_context);

            Assert.IsTrue(called);
        }

        [Test]
        public void Enter_DoesNotThrow_WhenOnEnterIsNull()
        {
            var state = new FSMState("NoEnterAction");
           // LogAssert.Expect(LogType.Log, $"{_context.Name} Entering State 'NoEnterAction'");
            Assert.DoesNotThrow(() => state.Enter(_context));
        }

        [Test]
        public void Update_InvokesOnUpdateAction_AndLogs()
        {
            bool called = false;
            var state = new FSMState("UpdateState", null, c => called = true);

           // LogAssert.Expect(LogType.Log, $"{_context.Name} Updating State 'UpdateState'");
            state.Update(_context);

            Assert.IsTrue(called);
        }

        [Test]
        public void Update_DoesNotThrow_WhenOnUpdateIsNull()
        {
            var state = new FSMState("NoUpdateAction");
           // LogAssert.Expect(LogType.Log, $"{_context.Name} Updating State 'NoUpdateAction'");
            Assert.DoesNotThrow(() => state.Update(_context));
        }

        [Test]
        public void Exit_InvokesOnExitAction_AndLogs()
        {
            bool called = false;
            var state = new FSMState("ExitState", null, null, c => called = true);

           // LogAssert.Expect(LogType.Log, $"{_context.Name} Exiting State 'ExitState'");
            state.Exit(_context);

            Assert.IsTrue(called);
        }

        [Test]
        public void Exit_DoesNotThrow_WhenOnExitIsNull()
        {
            var state = new FSMState("NoExitAction");
           // LogAssert.Expect(LogType.Log, $"{_context.Name} Exiting State 'NoExitAction'");
            Assert.DoesNotThrow(() => state.Exit(_context));
        }

        [Test]
        public void ToString_ReturnsExpectedFormat()
        {
            var state = new FSMState("TestState");
            Assert.AreEqual("FSMState: TestState", state.ToString());
        }
    }
}