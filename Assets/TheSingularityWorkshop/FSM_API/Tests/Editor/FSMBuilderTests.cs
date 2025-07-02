using System;
using System.Linq;
using NUnit.Framework;
using TheSingularityWorkshop.FSM.API;

namespace TheSingularityWorkshop.FSM.Tests
{
    // Simple test double for IStateContext
    public class BuilderTestContext : IStateContext
    {
        public string Name { get; set; }
        public bool IsValid { get; set; } = true;
    }

    [TestFixture]
    public class FSMBuilderTests
    {
        [SetUp]
        public void SetUp()
        {
            // Optionally clear FSM_API state if needed between tests
        }

        [Test]
        public void State_AddsState_AndThrowsOnDuplicateOrInvalid()
        {
            var builder = new FSMBuilder("TestFSM");
            builder.State("A");
            Assert.Throws<ArgumentException>(() => builder.State("A"));
            Assert.Throws<ArgumentException>(() => builder.State(null));
            Assert.Throws<ArgumentException>(() => builder.State(""));
        }

        [Test]
        public void WithName_SetsName_AndThrowsOnInvalid()
        {
            var builder = new FSMBuilder("TestFSM");
            builder.WithName("NewName");
            Assert.Throws<ArgumentException>(() => builder.WithName(null));
            Assert.Throws<ArgumentException>(() => builder.WithName(""));
        }

        [Test]
        public void WithProcessRate_SetsProcessRate()
        {
            var builder = new FSMBuilder("TestFSM");
            builder.WithProcessRate(5);
            // No direct way to check, but should not throw
        }

        [Test]
        public void WithInitialState_SetsInitialState_AndThrowsOnInvalid()
        {
            var builder = new FSMBuilder("TestFSM");
            builder.State("A");
            builder.WithInitialState("A");
            Assert.Throws<ArgumentException>(() => builder.WithInitialState(null));
            Assert.Throws<ArgumentException>(() => builder.WithInitialState(""));
        }

        [Test]
        public void WithUpdateCategory_SetsCategory_AndThrowsOnInvalid()
        {
            var builder = new FSMBuilder("TestFSM");
            builder.WithUpdateCategory("LateUpdate");
            Assert.Throws<ArgumentException>(() => builder.WithUpdateCategory(null));
            Assert.Throws<ArgumentException>(() => builder.WithUpdateCategory(""));
        }

        [Test]
        public void Transition_AddsTransition_AndThrowsOnInvalid()
        {
            var builder = new FSMBuilder("TestFSM");
            builder.State("A").State("B");
            builder.Transition("A", "B", c => true);
            Assert.Throws<ArgumentException>(() => builder.Transition(null, "B", c => true));
            Assert.Throws<ArgumentException>(() => builder.Transition("A", null, c => true));
            Assert.Throws<ArgumentNullException>(() => builder.Transition("A", "B", null));
        }

        [Test]
        public void BuildDefinition_ThrowsIfNoStates()
        {
            var builder = new FSMBuilder("TestFSM");
            Assert.Throws<InvalidOperationException>(() => builder.BuildDefinition());
        }

        [Test]
        public void BuildDefinition_ThrowsIfInitialStateMissing()
        {
            var builder = new FSMBuilder("TestFSM");
            builder.State("A");
            builder.WithInitialState("B");
            Assert.Throws<ArgumentException>(() => builder.BuildDefinition());
        }

        [Test]
        public void BuildDefinition_UsesFirstStateIfNoInitialState()
        {
            var builder = new FSMBuilder("TestFSM");
            builder.State("A").State("B");
            Assert.DoesNotThrow(() => builder.BuildDefinition());
            // FSM should be registered with initial state "A"
            var fsm = FSM_API.GetDefinition("TestFSM");
            Assert.AreEqual("A", fsm.InitialState);
        }

        [Test]
        public void BuildDefinition_RegistersFSMWithAllStatesAndTransitions()
        {
            var builder = new FSMBuilder("TestFSM");
            builder.State("A").State("B")
                   .WithInitialState("B")
                   .Transition("A", "B", c => true)
                   .Transition("B", "A", c => false)
                   .WithProcessRate(2)
                   .WithUpdateCategory("Update");
            builder.BuildDefinition();

            var fsm = FSM_API.GetDefinition("TestFSM", "Update");
            Assert.AreEqual("TestFSM", fsm.Name);
            Assert.AreEqual("B", fsm.InitialState);
            Assert.AreEqual(2, fsm.ProcessRate);
            Assert.AreEqual("Update", fsm.ProcessingGroup);
            Assert.IsTrue(fsm.HasState("A"));
            Assert.IsTrue(fsm.HasState("B"));
            Assert.AreEqual(2, fsm.GetAllTransitions().Count);
        }

        [Test]
        public void FSMBuilder_CanModifyExistingFSM()
        {
            // Build and register initial FSM
            var builder = new FSMBuilder("TestFSM");
            builder.State("A").State("B").WithInitialState("A").BuildDefinition();

            // Modify using FSMBuilder(FSM)
            var fsm = FSM_API.GetDefinition("TestFSM");
            var builder2 = new FSMBuilder(fsm);
            builder2.WithName("TestFSM2").State("C").WithInitialState("C").BuildDefinition();

            var fsm2 = FSM_API.GetDefinition("TestFSM2");
            Assert.IsTrue(fsm2.HasState("C"));
            Assert.AreEqual("C", fsm2.InitialState);
        }
    }
}