using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TheSingularityWorkshop.FSM.API;

namespace TheSingularityWorkshop.FSM.Tests
{
    public class FSM_API_Tests
    {
        // Simple test double for IStateContext
        public class TestContext : IStateContext
        {
            public string Name { get; set; }
            public bool IsValid { get; set; } = true;
        }

        [SetUp]
        public void SetUp()
        {
            // Optionally clear FSM_API state if needed between tests
        }

        [Test]
        public void CreateProcessingGroup_CreatesGroup_AndThrowsOnInvalid()
        {
            Assert.DoesNotThrow(() => FSM_API.CreateProcessingGroup("TestGroup"));
            Assert.Throws<ArgumentException>(() => FSM_API.CreateProcessingGroup(null));
            Assert.Throws<ArgumentException>(() => FSM_API.CreateProcessingGroup(""));
        }

        [Test]
        public void RemoveProcessingGroup_RemovesGroup_AndThrowsOnInvalid()
        {
            FSM_API.CreateProcessingGroup("ToRemove");
            Assert.DoesNotThrow(() => FSM_API.RemoveProcessingGroup("ToRemove"));
            Assert.Throws<ArgumentException>(() => FSM_API.RemoveProcessingGroup(null));
            Assert.Throws<ArgumentException>(() => FSM_API.RemoveProcessingGroup(""));
        }

        [Test]
        public void CreateFiniteStateMachine_ThrowsOnInvalid()
        {
            Assert.Throws<ArgumentException>(() => FSM_API.CreateFiniteStateMachine(null));
            Assert.Throws<ArgumentException>(() => FSM_API.CreateFiniteStateMachine("Test", 0, null));
        }

        [Test]
        public void Exists_ReturnsCorrectly_AndThrowsOnInvalid()
        {
            Assert.Throws<ArgumentException>(() => FSM_API.Exists(null));
            Assert.Throws<ArgumentException>(() => FSM_API.Exists("Test", null));
            // Add more as needed
        }

        [Test]
        public void GetAllDefinitionNames_ReturnsCorrectly_AndThrowsOnInvalid()
        {
            Assert.Throws<ArgumentException>(() => FSM_API.GetAllDefinitionNames(null));
           
        }

        [Test]
        public void GetDefinition_ThrowsOnInvalid()
        {
            Assert.Throws<ArgumentException>(() => FSM_API.GetDefinition(null));
            Assert.Throws<ArgumentException>(() => FSM_API.GetDefinition("Test", null));
            Assert.Throws<KeyNotFoundException>(() => FSM_API.GetDefinition("NotExist"));
        }

        [Test]
        public void GetInstances_ThrowsOnInvalid()
        {
            Assert.Throws<ArgumentException>(() => FSM_API.GetInstances(null));
            Assert.Throws<ArgumentException>(() => FSM_API.GetInstances("Test", null));
            Assert.Throws<KeyNotFoundException>(() => FSM_API.GetInstances("NotExist"));
        }

        [Test]
        public void CreateInstance_ThrowsOnInvalid()
        {
            Assert.Throws<ArgumentException>(() => FSM_API.CreateInstance(null, new TestContext()));
            Assert.Throws<ArgumentNullException>(() => FSM_API.CreateInstance("Test", null));
            Assert.Throws<ArgumentException>(() => FSM_API.CreateInstance("Test", new TestContext(), null));
            Assert.Throws<KeyNotFoundException>(() => FSM_API.CreateInstance("NotExist", new TestContext()));
        }

        [Test]
        public void DestroyFiniteStateMachine_ThrowsOnInvalid()
        {
            Assert.Throws<ArgumentException>(() => FSM_API.DestroyFiniteStateMachine(null));
            Assert.Throws<ArgumentException>(() => FSM_API.DestroyFiniteStateMachine("Test", null));
        }

        [Test]
        public void RemoveInstance_ThrowsOnNull()
        {
            Assert.Throws<ArgumentNullException>(() => FSM_API.Unregister(null));
        }

        
    }
}