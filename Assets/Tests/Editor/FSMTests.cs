using NUnit.Framework;

using System;

using TheSingularityWorkshop.FSM.API;

using UnityEngine;

/*
 * ? 1. FSM Creation & Registration
Tests

CreateFiniteStateMachine returns a builder for new name.

CreateFiniteStateMachine returns same builder for duplicate name (already in _buckets).

Register stores FSM definition correctly in _buckets.

    Register throws InvalidOperationException on duplicate FSM name.

? 2. Lookup & Existence
Tests

Exists returns true for known FSM.

Exists returns false for unknown FSM.

GetAllDefinitionNames returns all registered names.

GetDefinition returns correct FSM definition.

GetDefinition throws KeyNotFoundException for unknown FSM.

GetInstances returns correct instances list.

    GetInstances throws KeyNotFoundException for unknown FSM.

? 3. FSM Instance Management
Tests

CreateInstance successfully adds a handle to a known FSM.

CreateInstance throws KeyNotFoundException for unknown FSM.

Handle returned has correct Name, Context, and initialState.

Handle transitions update currentState.

TransitionTo() forces state transition and updates state.

    Update() steps FSM with correct current state logic.

? 4. Ticking / Updating
Process Rates:

FSM with processRate == 0 is skipped.

FSM with processRate > 0 counts down and ticks on zero.

FSM with processRate == -1 ticks every call.

    FSM handle inside tick is updated correctly.

? 5. Performance Watchdog
Tests

    Update() times the update and only logs if elapsed > 5ms.

        Can mock/stub Stopwatch or inject a test version if needed.

? 6. FSM Removal

(Once DestroyFiniteStateMachine is complete — include these:)

FSM is removed from _buckets.

Subsequent Exists returns false.

DestroyFiniteStateMachine on unknown FSM throws.

FSM instance list is also removed or emptied (depending on implementation).

    Removed FSM no longer ticks or updates.

? 7. Edge Cases & Safety
Tests

Register FSM with empty string name.

Create instance with null context (if allowed).

Create FSM with unusual characters or very long name.

    Tick when no FSMs exist (safe no-op).

? 8. Thread Safety (Optional / Advanced)

If you ever plan multithreaded FSMs:

Register and CreateInstance on different threads.

Tick from separate thread.
 */

public class FSMTests
{
    [Test]
    public void CreateFiniteStateMachine_ShouldCreateNewFSM_WhenCalledWithValidName()
    {
        string fsmName = "TestFSM";

        FSM_API.CreateFiniteStateMachine(fsmName).BuildDefinition();

        Assert.IsTrue(FSM_API.Exists(fsmName));
        FSM_API.DestroyFiniteStateMachine(fsmName);
    }

    [Test]
    public void CreateFiniteStateMachine_With_States_ShouldCreateNewFSM_WithStates()
    {
        string fsmName = "TestFSMWithStates";
        FSM_API.CreateFiniteStateMachine(fsmName)
        .State("State1", OnEnterState1, OnUpdateState1, OnExitState1)
               .State("State2", OnEnterState2, OnUpdateState2, OnExitState2)
               .WithInitialState("State1")
               .Transition("State1", "State2", ShouldTransitionFromState1ToState2)
               .Transition("State2", "State1", ShouldTransitionFromState2ToState1)
               .BuildDefinition();
        Assert.IsTrue(FSM_API.Exists(fsmName));
        FSM_API.DestroyFiniteStateMachine(fsmName);
    }

    [Test]
    public void CreateFiniteStateMachine_With_States_HasFSMInstance()
    {
        string fsmName = "TestFSMWithStates";
        FSM_API.CreateFiniteStateMachine(fsmName)
        .State("State1", OnEnterState1, OnUpdateState1, OnExitState1)
               .State("State2", OnEnterState2, OnUpdateState2, OnExitState2)
               .WithInitialState("State1")
               .Transition("State1", "State2", ShouldTransitionFromState1ToState2)
               .Transition("State2", "State1", ShouldTransitionFromState2ToState1)
               .BuildDefinition();
        var context = new TestStateContext { Name = "TestContext" };
        var fsm = FSM_API.CreateInstance(fsmName,context);
        Assert.IsNotNull(fsm);
        FSM_API.DestroyFiniteStateMachine(fsmName);
    }

    [Test]
    public void CreateFiniteStateMachine_With_States_ChangesStateWhenStepped()
    {
        string fsmName = "TestFSMWithStates2";
        FSM_API.CreateFiniteStateMachine(fsmName,-1)
        .State("State1", OnEnterState1, OnUpdateState1, OnExitState1)
               .State("State2", OnEnterState2, OnUpdateState2, OnExitState2)
               .WithInitialState("State1")
               .Transition("State1", "State2", ShouldTransitionFromState1ToState2)
               .Transition("State2", "State1", ShouldTransitionFromState2ToState1)
               .BuildDefinition();
        var context = new TestStateContext { Name = "TestContext" };
        var fsm = FSM_API.CreateInstance(fsmName, context);
        FSM_API.Update();

        Assert.IsTrue(fsm.currentState == "State2");
        FSM_API.DestroyFiniteStateMachine(fsmName);
    }


    private class TestStateContext : IStateContext
    {
        public string Name { get; set; }
        public object UserData { get; set; }
        public bool EnteredState { get; } = false;
        public bool ShouldTransition { get; } = false;
    }

    private void OnEnterState1(IStateContext context)
    {
        Debug.Log("Entering State 1");
    }

    private void OnUpdateState1(IStateContext context)
    {
        Debug.Log("Updating State 1");
    }

    private void OnExitState1(IStateContext context)
    {
        Debug.Log("Exiting State 1");
    }

    private void OnEnterState2(IStateContext context)
    {
        Debug.Log("Entering State 2");
    }

    private void OnUpdateState2(IStateContext context)
    {
        Debug.Log("Updating State 2");
    }

    private void OnExitState2(IStateContext context)
    {
        Debug.Log("Exiting State 2");
    }

    private bool ShouldTransitionFromState1ToState2(IStateContext context)
    {
        Debug.Log("Checking transition from State 1 to State 2");
        return true;
    }

    private bool ShouldTransitionFromState2ToState1(IStateContext context)
    {
        Debug.Log("Checking transition from State 2 to State 1");
        return true;
    }
}

public class FSMInstanceTests
{

}

public class FSMTickingTests
{

}

public class  FSMRemovalTests
{
    
}

[TestFixture]
public class FSMEdgeCaseTests
{
    class TestContext : IStateContext
    {
        public string Name { get; set; }
        public int TickCount = 0;

        public bool EnteredState { get; } = false;
        public bool ShouldTransition { get; } = false;
    }

    //private FSM<IStateContext> BuildSimpleMachine(string name)
    //{
    //    return new FSMBuilder<IStateContext>(name, processRate: -1)
    //        .State("Idle", onUpdate: ctx => ((TestContext)ctx).TickCount++)
    //        .BuildDefinition();
    //}

    [SetUp]
    public void Clear()
    {
        // Not part of your current API, but ideally you'd have:
        // GenericStateMachineAPI.ClearAll(); 
        // For now, just destroy a known test FSM
        if (FSM_API.Exists("TestFSM"))
            FSM_API.DestroyFiniteStateMachine("TestFSM");
    }
}