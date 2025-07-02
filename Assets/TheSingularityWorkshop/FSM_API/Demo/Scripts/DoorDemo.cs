using System;

using TheSingularityWorkshop.FSM.API;

using Unity.VisualScripting;

using UnityEngine;

public class DoorDemo : MonoBehaviour
{
    private FSMHandle doorFSM;
    public Door door;

    public string Name => "Door Demo";

    public void DefineFSMs()
    {


    }



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        doorFSM = FSM_API.CreateInstance("DoorFSM", door);
    }

    // Update is called once per frame
    void Update()
    {

    }
}

public class Door : MonoBehaviour, IStateContext
{
    internal bool isOpen;

    public bool IsValid { get; set; }
    public string Name { get; set; }

    void Awake()
    {
        IsValid = true;
        Name = name;
        if (!FSM_API.Exists("DoorFSM"))
        {
            FSM_API.CreateFiniteStateMachine("DoorFSM")
     .State("Closed", OnEnterClosed, OnUpdateClosed, OnExitClosed)
     .State("Open", OnEnterOpen, OnUpdateOpen, OnExitOpen)
     .WithInitialState("Closed")
         .Transition("Open", "Closed", Closing)
         .Transition("Closed", "Open", Opening)
         .BuildDefinition();
        }
    }

    private void OnEnterClosed(IStateContext context)
    {
        throw new NotImplementedException();
    }

    private void OnUpdateClosed(IStateContext context)
    {
        throw new NotImplementedException();
    }

    private void OnExitClosed(IStateContext context)
    {
        throw new NotImplementedException();
    }

    private void OnEnterOpen(IStateContext context)
    {
        throw new NotImplementedException();
    }

    private void OnUpdateOpen(IStateContext context)
    {
        throw new NotImplementedException();
    }

    private void OnExitOpen(IStateContext context)
    {
        throw new NotImplementedException();
    }

    private bool Closing(IStateContext context)
    {
        if (context is Door door)
        {
            return !door.isOpen;
        }
        return false;
    }

    private bool Opening(IStateContext context)
    {
        if (context is Door door)
        {
            return door.isOpen;
        }
        return false;
    }

    public void Open()
    {
        isOpen = true;
    }

    public void Close()
    {
        isOpen = false;
    }
}
