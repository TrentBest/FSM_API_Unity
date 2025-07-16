using NUnit.Framework;

using System;

using TheSingularityWorkshop.FSM.API;

using UnityEngine;

namespace TheSingularityWorkshop.FSM.Tests
{
    [TestFixture]
    public class OscillatorTests
    {
        [SetUp]
        public void SetUp()
        {
            var groups = FSM_API.GetAllProcessGroups();
            foreach (var pg in groups)
            {
                if(pg != "Update")
                {
                    FSM_API.RemoveProcessingGroup(pg);
                }
            }
        }

        [Test]
        public void OscillatorInitializesCorrectly()
        {
            // Arrange
            var gameObject = new GameObject("TestOscillator");
            var oscillator = gameObject.AddComponent<Oscillator>();
            oscillator.Name = "TestOscillator";

            // Act
            oscillator.Awake();

            // Assert
            Assert.IsNotNull(oscillator.OscillatorFSM, "Oscillator FSM should be initialized.");
            Assert.AreEqual("TestOscillator", oscillator.Name, "Oscillator Name should be set correctly.");
            Assert.IsTrue(oscillator.IsValid, "Oscillator should be valid after initialization.");
        }

        [Test]
        public void IsMinReturnsTrueWhenMinimumValueReached()
        {
            // Arrange
            var gameObject = new GameObject("TestOscillator");
            var oscillator = gameObject.AddComponent<Oscillator>();
            oscillator.MinimumValue = 1f;
            oscillator.MaximumValue = 2f;
            oscillator.dx = -0.01f;
            oscillator.floatAccesorSetDelegate = fasd;
            oscillator.floatAccessorGetDelegate = fagd;
            oscillator.Awake();
            FSM_API.CreateInstance("OscillatorFSM", oscillator, "OscillatorPG");
            // Act
            for (int i = 0; i < 100; i++)
            {
                FSM_API.Update("OscillatorPG");
            }
            // Assert
            Assert.IsTrue(oscillator.OscillatorFSM.CurrentState == "Minimum", "Oscillator should be in Minimum state when minimum value is reached.");
        }

        [Test]
        public void IsMaxReturnsTrueWhenMaximumValueReached()
        {
            // Arrange
            var gameObject = new GameObject("TestOscillator");
            var oscillator = gameObject.AddComponent<Oscillator>();
            oscillator.MinimumValue = 1f;
            oscillator.MaximumValue = 2f;
            oscillator.dx = 0.01f;
            oscillator.floatAccesorSetDelegate = fasd;
            oscillator.floatAccessorGetDelegate = fagd;
            oscillator.Awake();
            FSM_API.CreateInstance("OscillatorFSM", oscillator, "OscillatorPG");
            // Act
            for (int i = 0; i < 100; i++)
            {
                FSM_API.Update("OscillatorPG");
            }
            // Assert
            Assert.IsTrue(oscillator.OscillatorFSM.CurrentState == "Maximum", "Oscillator should be in Maximum state when maximum value is reached.");
        }

        [Test]
        public void OscillatorChangesDirectionAfterReachingMax()
        {
            // Arrange
            var gameObject = new GameObject("TestOscillator");
            var oscillator = gameObject.AddComponent<Oscillator>();
            oscillator.MinimumValue = -1f;
            oscillator.MaximumValue = 1f;
            oscillator.dx = .5f;
            oscillator.floatAccesorSetDelegate = fasd;
            oscillator.floatAccessorGetDelegate = fagd;
            oscillator.floatAccesorSetDelegate(gameObject, 0f);
            oscillator.Awake();
            FSM_API.CreateInstance("OscillatorFSM", oscillator, "OscillatorPG");
            // Act
            for (int i = 0; i < 3; i++)
            {
                FSM_API.Update("OscillatorPG");
            }
            // Assert
            Assert.IsTrue(oscillator.dx < 0);
        }

        private float fagd(GameObject go)
        {
            //Debug.Log($"fagd:  {go.transform.localScale.x}");
            return go.transform.localScale.x;
        }

        private void fasd(GameObject go, float value)
        {
            //Debug.Log($"fasd {value}");
            var current = go.transform.localScale;
            go.transform.localScale =  new Vector3(value, current.y, current.z);
        }
    }
}