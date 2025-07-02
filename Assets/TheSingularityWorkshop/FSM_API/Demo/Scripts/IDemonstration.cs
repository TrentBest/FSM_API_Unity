using TheSingularityWorkshop.FSM.API;

using UnityEngine;

public interface IDemonstration
{
    // Initializes the demo, setting up FSMs, contexts, etc.
    // `isHubControlled` indicates if the hub will handle FSM_API.Update() calls.
    void Initialize(bool isHubControlled);

    // Called to start the main logic of the demo.
    void StartDemo();

    // Called to stop and clean up the demo's active logic.
    void StopDemo();

    // Used by the hub to explicitly update the FSM group associated with this demo.
    // This is only called if isHubControlled was true during Initialize.
    void UpdateDemoFSM();
}
