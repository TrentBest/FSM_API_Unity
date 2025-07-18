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
            Debug.Log("[SETUP] Starting SetUp for test.");

            // Get all FSM definitions (assuming FSM_API has a way to iterate/get these)
            // If FSM_API.GetFiniteStateMachineNames() exists:
            var fsmDefinitions = FSM_API.GetAllDefinitionNames();
            foreach (var fsmName in fsmDefinitions)
            {
                FSM_API.DestroyFiniteStateMachine(fsmName); 
                Debug.Log($"[SETUP] Removed FSM Definition: {fsmName}");
            }


            //var groups = FSM_API.GetAllProcessGroups();
            //foreach (var pg in groups)
            //{
            //    if (pg != "Update") // Keep "Update" if it's a special, always-present group
            //    {
            //        FSM_API.RemoveProcessingGroup(pg);
            //        Debug.Log($"[SETUP] Removed Processing Group: {pg}");
            //    }
            //}

            // Ensure no lingering instances or definitions
            // If FSM_API has a general reset/clear method, call it here. E.g., FSM_API.ResetStatics();

            Debug.Log("[SETUP] Finished SetUp for test.");
        }

        //[Test]
        //public void OscillatorInitializesCorrectly()
        //{
        //    // Arrange
        //    var gameObject = new GameObject("TestOscillator");
        //    var oscillator = gameObject.AddComponent<Oscillator>();
        //    oscillator.Name = "TestOscillator";

        //    // Act
        //    oscillator.Awake();

        //    // Assert
        //    Assert.IsNotNull(oscillator.OscillatorFSM, "Oscillator FSM should be initialized.");
        //    Assert.AreEqual("TestOscillator", oscillator.Name, "Oscillator Name should be set correctly.");
        //    Assert.IsTrue(oscillator.IsValid, "Oscillator should be valid after initialization.");
        //}

        //[Test]
        //public void IsMinReturnsTrueWhenMinimumValueReached()
        //{
        //    // Arrange
        //    var gameObject = new GameObject("TestOscillator");
        //    var oscillator = gameObject.AddComponent<Oscillator>();
        //    oscillator.MinimumValue = 1f;
        //    oscillator.MaximumValue = 2f;
        //    oscillator.dx = -0.01f; // dx is negative, so it will decrease towards MinimumValue
        //    oscillator.floatAccesorSetDelegate = fasd;
        //    oscillator.floatAccessorGetDelegate = fagd;

        //    // Start at MaximumValue so it can decrease towards MinimumValue
        //    oscillator.floatAccesorSetDelegate(gameObject, oscillator.MaximumValue); // ADDED THIS LINE

        //    oscillator.Awake();
        //    FSM_API.CreateInstance("OscillatorFSM", oscillator, "OscillatorPG");
        //    // Act
        //    for (int i = 0; i < 100; i++) // 100 updates should be enough to reach from 2f down to 1f with dx of -0.01f
        //    {
        //        FSM_API.Update("OscillatorPG");
        //    }
        //    // Assert
        //    Assert.IsTrue(oscillator.OscillatorFSM.CurrentState == "Minimum", "Oscillator should be in Minimum state when minimum value is reached.");
        //    // Optionally, add an assertion to confirm the value is also at the minimum
        //    Assert.That(oscillator.floatAccessorGetDelegate(gameObject), Is.EqualTo(oscillator.MinimumValue).Within(0.001f)); // Added for robustness
        //}

        //[Test]
        //public void IsMaxReturnsTrueWhenMaximumValueReached()
        //{
        //    // Arrange
        //    var gameObject = new GameObject("TestOscillator");
        //    var oscillator = gameObject.AddComponent<Oscillator>();
        //    oscillator.MinimumValue = 1f;
        //    oscillator.MaximumValue = 2f;
        //    oscillator.dx = 0.01f;
        //    oscillator.floatAccesorSetDelegate = fasd;
        //    oscillator.floatAccessorGetDelegate = fagd;
        //    oscillator.Awake();
        //    FSM_API.CreateInstance("OscillatorFSM", oscillator, "OscillatorPG");
        //    // Act
        //    for (int i = 0; i < 100; i++)
        //    {
        //        FSM_API.Update("OscillatorPG");
        //    }
        //    // Assert
        //    Assert.IsTrue(oscillator.OscillatorFSM.CurrentState == "Maximum", "Oscillator should be in Maximum state when maximum value is reached.");
        //}

        //[Test]
        //public void OscillatorChangesDirectionAfterReachingMax()
        //{
        //    // Arrange
        //    var gameObject = new GameObject("TestOscillator");
        //    var oscillator = gameObject.AddComponent<Oscillator>();
        //    oscillator.MinimumValue = -1f;
        //    oscillator.MaximumValue = 1f;
        //    oscillator.dx = .5f;
        //    oscillator.floatAccesorSetDelegate = fasd;
        //    oscillator.floatAccessorGetDelegate = fagd;
        //    oscillator.floatAccesorSetDelegate(gameObject, 0f);
        //    oscillator.Awake();
        //    FSM_API.CreateInstance("OscillatorFSM", oscillator, "OscillatorPG");
        //    // Act
        //    for (int i = 0; i < 3; i++)
        //    {
        //        FSM_API.Update("OscillatorPG");
        //    }
        //    // Assert
        //    Assert.IsTrue(oscillator.dx < 0);
        //}

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