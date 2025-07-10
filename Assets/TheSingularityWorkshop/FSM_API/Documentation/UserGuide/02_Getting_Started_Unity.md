# 02. Getting Started with FSM_API in Unity

> Use FSM_API with your MonoBehaviours to define clean, maintainable state machines directly within Unity's component lifecycle.

FSM_API is engine-agnostic, but Unity users get the full benefit of its modular power. In this guide, we’ll walk through setting up a **character health state machine** using MonoBehaviour, managing transitions like **Alive → Dead → Revived**, and structuring update timing using Unity’s `Update()` flow — including full control over **processing groups**.

---

## 🎯 What This Example Will Demonstrate

- How to define and consume FSMs using MonoBehaviours
- How to use FSM_API’s **processing groups** to control update timing
- How to create FSMs that react to health, death, and revival
- How Unity lifecycle methods can align with FSM_API update strategies
- How cleanup and flexibility are handled automatically

---

## ✅ Step 1: Define Your Context (MonoBehaviour + IStateContext)

Every FSM operates on a **context** — in Unity, this is typically a `MonoBehaviour`. The FSM reads and modifies data from the context, and reacts to its lifecycle.

### 🧱 Example: `PlayerCharacterContext.cs`

```csharp
using UnityEngine;
using TheSingularityWorkshop.FSM.API;

public class PlayerCharacterContext : MonoBehaviour, IStateContext
{
    public float currentHealth = 100f;
    public float moveSpeed = 5f;

    public string Name { get; set; }
    public bool IsValid { get; private set; } = true;

    private FSMHandle playerFSM;

    void Awake()
    {
        Name = gameObject.name;

        // Only define once
        if (!FSM_API.Exists("PlayerHealthFSM"))
        {
            FSM_API.CreateFiniteStateMachine("PlayerHealthFSM", processRate: 1, processingGroup: "Gameplay")
                .State("Alive",
                    onEnter: ctx => ((PlayerCharacterContext)ctx).OnEnterAlive(),
                    onUpdate: ctx => ((PlayerCharacterContext)ctx).OnUpdateAlive(),
                    onExit: ctx => ((PlayerCharacterContext)ctx).OnExitAlive())
                .State("Dead",
                    onEnter: ctx => ((PlayerCharacterContext)ctx).OnEnterDead(),
                    onUpdate: ctx => ((PlayerCharacterContext)ctx).OnUpdateDead(),
                    onExit: ctx => ((PlayerCharacterContext)ctx).OnExitDead())
                .WithInitialState("Alive")
                .Transition("Alive", "Dead", ctx => ((PlayerCharacterContext)ctx).IsDead())
                .Transition("Dead", "Alive", ctx => ((PlayerCharacterContext)ctx).IsRevived())
                .BuildDefinition();
        }
    }

    void Start()
    {
        // FSMHandle can override default processing group if desired
        playerFSM = FSM_API.CreateInstance("PlayerHealthFSM", this); // Optionally: , "Gameplay"
    }

    void Update()
    {
        FSM_API.Update("Gameplay");
    }

    void OnEnable() => IsValid = true;
    void OnDisable() => IsValid = false;

    void OnDestroy()
    {
        IsValid = false;
        if (playerFSM != null)
        {
            FSM_API.Unregister(playerFSM);
            playerFSM = null;
        }
    }

    void OnEnterAlive() => Debug.Log($"{Name} entered Alive.");
    void OnUpdateAlive() { }
    void OnExitAlive() => Debug.Log($"{Name} exited Alive.");
    void OnEnterDead() => Debug.Log($"{Name} entered Dead.");
    void OnUpdateDead() { }
    void OnExitDead()
    {
        Debug.Log($"{Name} exited Dead.");
        currentHealth = 100f;
    }

    bool IsDead() => currentHealth <= 0;
    bool IsRevived() => currentHealth > 0 && Input.GetKeyDown(KeyCode.R);
}
```
✅ Step 2: Register and Drive Processing Groups

FSM_API uses named processing groups to organize how and when FSMs update.

Before ticking a group, you must register it with:
```csharp
FSM_API.CreateProcessingGroup("Gameplay");
```
This can be done anytime, but it’s common to do this in a dedicated driver MonoBehaviour.
🔁 Group Lifecycle and Cleanup

    Creating a group: Simply call FSM_API.CreateProcessingGroup("GroupName")

    Assigning FSMs: FSMs created with .CreateFiniteStateMachine(..., processingGroup: "GroupName") or FSMHandles instantiated with group overrides will be linked automatically

    Destroying a group: Calling FSM_API.RemoveProcessingGroup("GroupName") will:

        Call onExit() on all FSMs in that group

        Unregister and clean up their handles

        Remove the group from the ticking queue

        Prevent further updates or leaks — it’s safe and clean

✅ Step 3: The FSM Driver MonoBehaviour

Use a central MonoBehaviour to register your groups and tick them using Unity's lifecycle methods.
🧭 Example: FSMDriver.cs
```csharp
using UnityEngine;
using TheSingularityWorkshop.FSM.API;

public class FSMDriver : MonoBehaviour
{
    void Awake()
    {
        // Register desired update groups
        FSM_API.CreateProcessingGroup("Gameplay");
        FSM_API.CreateProcessingGroup("Physics");
        FSM_API.CreateProcessingGroup("LateUpdate");
    }

    void Update()
    {
        FSM_API.Update("Gameplay");
    }

    void FixedUpdate()
    {
        FSM_API.Update("Physics");
    }

    void LateUpdate()
    {
        FSM_API.Update("LateUpdate");
    }
}
```
    ⚠️ You can create or remove processing groups at any point during runtime. Just don’t forget to call CreateProcessingGroup(...) before ticking it.

✅ Optional: FSMHandle Group Override

FSM instances (FSMHandles) can override the processing group specified by the FSM definition.
```csharp
// Uses same FSM definition, but adds it to a different processing group
var handle = FSM_API.CreateInstance("MyFSM", myContext, "CustomUIGroup");
```
    If the group doesn’t exist, it’s automatically created

    Multiple FSMHandles can reference the same definition, but live in different groups

    Groups can be used to organize logic semantically (e.g., "Enemies", "BossAI", "Ambient") and control timing or performance individually

🧠 Tip: Manual Stepping and Compound Timing

FSM_API allows fine-grained control:
```csharp
FSM_API.Step(myFSMHandle); // Directly advances this single FSM
```
You could even throttle a group manually:
```csharp
if (Time.frameCount % 5 == 0)
    FSM_API.Update("AmbientAI");
```
This can be layered atop individual FSM processRate settings — allowing deeply nested, compound throttling for performance or time dilation logic.
✅ Summary

    Define FSMs in Awake(), instantiate in Start() for MonoBehaviours

    Use FSM_API.CreateProcessingGroup(...) to register groups before update

    Use FSM_API.Update("Group") to drive updates from Unity lifecycle methods

    Destroying a group cleans up all associated FSMs — no leaks

    FSMHandles can override their group assignment

    Manual stepping and tick throttling allow tight runtime control

➡️ Continue to: 03_Pure_CSharp_Getting_Started.md
