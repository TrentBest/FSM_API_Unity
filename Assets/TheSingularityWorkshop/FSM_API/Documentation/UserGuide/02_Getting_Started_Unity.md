Getting Started with FSM_API in Unity

This guide walks you through FSM_API, helping you integrate it into your Unity project. You'll learn everything from defining your context to creating your first state machine. Follow these steps to get up and running quickly.

1. Define Your Context

First, create a C# class in Unity. This class represents the "thing" your FSM will manage, like your Player, an Enemy, or a Door. This class must implement IStateContext.

What is a "Context" in Unity?

In Unity, your context is the object your FSM operates on. This can be a MonoBehaviour script you attach to a GameObject, a Scriptable Object, or even a pure C# class. Your context holds all the data and actions for that "thing." For example, a Player script would hold health and movement code.

How to Create Your Context Class

    Create a New C# Script:
    In your Unity Project window, right-click and choose Create > C# Script. Name it something descriptive, like PlayerCharacterContext. Make sure the script name matches the file name exactly.

    Open the Script:
    Double-click the new script to open it in your code editor (like Visual Studio).

    Add IStateContext:
    Modify the script to "implement" IStateContext. This is like signing a contract; it tells FSM_API your script is ready to be a context. Your script also needs using UnityEngine; at the top.
    C#

    // Add this line at the very top of your script.
    using UnityEngine;
    using TheSingularityWorkshop.FSM.API; // Add this for FSM_API interfaces

    // Your script's name should match your file name!
    // It must inherit from MonoBehaviour AND IStateContext.
    public class PlayerCharacterContext : MonoBehaviour, IStateContext
    {
        // --- YOUR GAME'S DATA ---
        // These are public "fields" shown in Unity's Inspector.
        public float currentHealth = 100f; // Example: Player's health.
        public int currentAmmo = 10;     // Example: Player's bullets.
        public float moveSpeed = 5f;     // Example: How fast player moves.

        // --- REQUIREMENTS FOR IContext (inherited by IStateContext) ---
        // This is a name for your object, useful for debugging FSMs.
        public string Name { get; set; }

        // --- REQUIREMENTS FOR IStateContext ---
        // FSM_API checks this to know if your object is still valid.
        // If the GameObject is destroyed, or script is inactive, this should be false.
        public bool IsValid => this != null && gameObject.activeInHierarchy;

        // --- UNITY SPECIFIC SETUP ---
        // This runs when your GameObject first wakes up.
        void Awake()
        {
            // Set the FSM_API's context name to your GameObject's name.
            Name = gameObject.name;
        }

        // --- YOUR GAME'S ACTIONS (Methods) ---
        // These are actions your FSM might tell this object to do.
        public void TakeDamage(float amount)
        {
            currentHealth -= amount;
            Debug.Log(Name + " took " + amount + " damage. Health: " + currentHealth);
            // Add more game logic here, like playing a hit sound.
        }

        public void MoveForward()
        {
            // Example: Make the character move.
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
            // You might play a "walking" animation here too.
        }

        public void PlayAnimation(string animName)
        {
            // Example: Code to play an animation by name.
            Debug.Log(Name + " playing animation: " + animName);
        }
    }

    What's Happening Here?

        using UnityEngine;: This line tells your script to use Unity's tools.

        : MonoBehaviour, IStateContext: This means your script is a MonoBehaviour (so you can attach it to GameObjects), and it agrees to provide the IStateContext information (like Name and IsValid).

        public float currentHealth;: These are fields. public means other parts of your code can see them. They also show up in Unity's Inspector window, letting you change values easily.

        Name { get; set; }: This is a property. It's like a field but with more control. IContext requires your object to have a Name.

        IsValid => this != null && gameObject.activeInHierarchy;: This is a shorthand property. It quickly checks if your script exists and if its GameObject is active in the game. If it's not valid, FSM_API knows to stop trying to use this context.

        void Awake(): A special Unity method. It runs once when your GameObject becomes active. We use it to set the Name property.

        public void TakeDamage(...), public void MoveForward(), etc.: These are methods. They are actions your object can perform. Your FSM will "tell" your context to run these actions.

2. Define and Create Your FSM

Now, let's define your FSM's blueprint and then create a live instance for your context. You'll do this inside your PlayerCharacterContext script.

    Add FSM_API Reference:
    First, add using TheSingularityWorkshop.FSM.API; to the very top of your script. This line tells your script where to find FSM_API's tools.

    Add an FSMHandle Variable:
    Inside your PlayerCharacterContext class, add a new line to hold your live FSM instance.
    C#

// Add this inside your PlayerCharacterContext class,
// usually near your other public fields.
private FSMHandle playerFSM; // This will hold our player's FSM.

    private FSMHandle playerFSM;: This creates a storage space. It will hold the FSMHandle (your live FSM). private means only this script can see it.

Define and Create FSM in Awake():
Modify your Awake() method. This is where you'll define your FSM's states and transitions, and create its live instance.
C#

    // Add this line at the very top of your script, with using UnityEngine;
    using TheSingularityWorkshop.FSM.API;

    public class PlayerCharacterContext : MonoBehaviour, IStateContext
    {
        // ... (your public fields like currentHealth, currentAmmo, moveSpeed) ...

        // ... (your Name and IsValid properties) ...

        private FSMHandle playerFSM; // Our FSM instance variable

        void Awake() // This runs when your GameObject starts.
        {
            Name = gameObject.name; // Set the context's name.

            // --- Define the FSM Blueprint (only once!) ---
            // We check if the FSM blueprint "PlayerHealthFSM" already exists.
            // This prevents defining it multiple times if you have many players.
            if (!FSM_API.Exists("PlayerHealthFSM"))
            {
                // Start creating our FSM blueprint.
                FSM_API.CreateFiniteStateMachine("PlayerHealthFSM",
                                                 processRate: 1, // Update every frame
                                                 processingGroup: "PlayerLogic") // Custom group name
                    // Define the "Alive" state.
                    // When entering "Alive", we might log a message.
                    .State("Alive",
                        onEnter: (context) => ((PlayerCharacterContext)context).OnEnterAlive(),
                        onUpdate: (context) => ((PlayerCharacterContext)context).OnUpdateAlive(),
                        onExit: (context) => ((PlayerCharacterContext)context).OnExitAlive())
                    // Define the "Dead" state.
                    // When entering "Dead", the player might stop moving.
                    .State("Dead",
                        onEnter: (context) => ((PlayerCharacterContext)context).OnEnterDead(),
                        onUpdate: (context) => ((PlayerCharacterContext)context).OnUpdateDead(),
                        onExit: (context) => ((PlayerCharacterContext)context).OnExitDead())
                    // Set "Alive" as the starting state when FSM begins.
                    .WithInitialState("Alive")
                    // Define a rule: From "Alive" to "Dead" if player health is zero.
                    .Transition("Alive", "Dead",
                        (context) => ((PlayerCharacterContext)context).IsDead())
                    // Define a rule: From "Dead" to "Alive" if player is revived.
                    .Transition("Dead", "Alive",
                        (context) => ((PlayerCharacterContext)context).IsRevived())
                    // IMPORTANT: Finalize and register this FSM blueprint.
                    .BuildDefinition();
            }

            // --- Create a Live FSM Instance for THIS Player ---
            // We create a new FSMHandle for this specific PlayerCharacterContext.
            // "this" means "this current PlayerCharacterContext object."
            playerFSM = FSM_API.CreateInstance("PlayerHealthFSM", this);
        }

        // --- FSM STATE ACTION METHODS (called by the FSM) ---
        // These methods are what your FSM will run.
        // They take IStateContext, but you cast it to your specific type.

        void OnEnterAlive()
        {
            Debug.Log(Name + " entered Alive state. Health: " + currentHealth);
            // Example: Enable player controls, play happy music.
        }

        void OnUpdateAlive()
        {
            // Example: Check for input, move player.
            // Debug.Log(Name + " is still Alive.");
        }

        void OnExitAlive()
        {
            Debug.Log(Name + " exited Alive state.");
            // Example: Disable player controls, stop music.
        }

        void OnEnterDead()
        {
            Debug.Log(Name + " entered Dead state!");
            // Example: Play death animation, show game over screen.
        }

        void OnUpdateDead()
        {
            // Example: Wait for revive, or countdown to respawn.
            // Debug.Log(Name + " is still Dead.");
        }

        void OnExitDead()
        {
            Debug.Log(Name + " exited Dead state. Revived!");
            // Example: Hide game over screen, reset health.
            currentHealth = 100f; // Reset health on revive.
        }

        // --- FSM TRANSITION CONDITION METHODS ---
        // These methods return true or false to decide if a transition happens.

        bool IsDead()
        {
            // Example: Player is dead if health is zero or less.
            return currentHealth <= 0;
        }

        bool IsRevived()
        {
            // Example: Player is revived if health is greater than zero
            // and perhaps a "revive" command was given.
            return currentHealth > 0 && Input.GetKeyDown(KeyCode.R); // Example revive input
        }

        // ... (your TakeDamage, MoveForward, PlayAnimation methods) ...
    }

        What's Happening Here?

            if (!FSM_API.Exists("PlayerHealthFSM")): This line checks if our FSM blueprint named "PlayerHealthFSM" has already been created. We only want to create the blueprint once, even if many PlayerCharacterContext objects exist.

            FSM_API.CreateFiniteStateMachine(...): This starts the process of building our FSM blueprint.

                "PlayerHealthFSM": This is the unique name for our blueprint.

                processRate: 1: This controls how often this FSM updates.

                    -1: Updates every single time its ProcessingGroup is ticked (most frequent).

                    0: Updates only when explicitly told to (event-driven). It won't update automatically each tick.

                    N (any number greater than 0): Updates every N ticks (e.g., 5 means it updates every 5th tick).

                processingGroup: "PlayerLogic": We're putting this FSM into a custom group named "PlayerLogic." We'll "tick" this group later.

            .State("Alive", ...) and .State("Dead", ...): These lines define our two states.

                "Alive" and "Dead" are the names of our states.

                onEnter: (context) => ((PlayerCharacterContext)context).OnEnterAlive(): This is how you tell FSM_API what code to run when entering a state. We're pointing to a method (OnEnterAlive) within this script. We ((PlayerCharacterContext)context) to tell the system that context is our PlayerCharacterContext so we can use its methods.

            .WithInitialState("Alive"): This tells FSM_API that when a new FSM instance is created from this blueprint, it should start in the "Alive" state.

            .Transition("Alive", "Dead", ...): This defines a rule for moving between states.

                "Alive" is the state we are coming from.

                "Dead" is the state we are going to.

                (context) => ((PlayerCharacterContext)context).IsDead(): This is the condition for the transition. The FSM will check this IsDead() method. If IsDead() returns true, the FSM will move from "Alive" to "Dead."

            .BuildDefinition(): This is very important! It finalizes your FSM blueprint and registers it with FSM_API, making it ready to be used. Don't forget this line!

            playerFSM = FSM_API.CreateInstance("PlayerHealthFSM", this);: After the blueprint is defined, this line creates a live FSM instance.

                "PlayerHealthFSM": We tell it which blueprint to use.

                this: We tell it that this specific PlayerCharacterContext script is the object the FSM will control. The FSM will then use this script's data and methods.

3. Tick Your FSMs with Processing Groups

Your FSMs won't do anything unless you tell them to "think" or "update." FSM_API uses Processing Groups to give you control. You decide when and which FSMs update.

The FSM_API.Update() Method

You advance FSMs by calling FSM_API.Update(). This method has a default processingGroup of "Update", so FSM_API.Update() is the same as FSM_API.Update("Update").

You can call FSM_API.Update() from any part of your code. In Unity, you typically call it from MonoBehaviour methods, like Update(), FixedUpdate(), or OnGUI().

Example: Ticking Multiple Processing Groups

Let's imagine an AnyApp script that manages different FSMs. It defines FSMs for application flow and user interface.
C#

using TheSingularityWorkshop.FSM.API;
using UnityEngine; // Needed for MonoBehaviour

public class AnyApp : MonoBehaviour, IStateContext
{
    // --- IContext requirement ---
    // This script's name will be its context name.
    public string Name { get; set; }

    // --- IStateContext requirement ---
    // This app context is always valid if the GameObject is active.
    public bool IsValid => this != null && gameObject.activeInHierarchy;

    // FSMHandle for our main application flow FSM.
    private FSMHandle appFlowFSM;
    // FSMHandle for our user interface FSM.
    private FSMHandle uiFSM;

    void Awake()
    {
        Name = gameObject.name; // Set the context name.

        // --- Define the Application Flow FSM (updates every frame) ---
        if (!FSM_API.Exists("ApplicationFlowFSM"))
        {
            FSM_API.CreateFiniteStateMachine("ApplicationFlowFSM",
                                             processRate: -1, // Updates every tick of its group.
                                             processingGroup: "AppCore") // Custom group for app flow.
                .State("SplashScreen",
                    onEnter: (context) => Debug.Log("Entered Splash Screen."),
                    onUpdate: (context) => Debug.Log("Splash Screen active..."),
                    onExit: (context) => Debug.Log("Exited Splash Screen."))
                .State("Initializing",
                    onEnter: (context) => Debug.Log("Entered Initializing."),
                    onUpdate: (context) => Debug.Log("Initializing app..."),
                    onExit: (context) => Debug.Log("Exited Initializing."))
                .State("Initialized",
                    onEnter: (context) => Debug.Log("App Initialized!"),
                    onUpdate: (context) => Debug.Log("App running."),
                    onExit: (context) => Debug.Log("App shutting down."))
                .WithInitialState("SplashScreen")
                .Transition("SplashScreen", "Initializing", (context) => ((AnyApp)context).SplashComplete())
                .Transition("Initializing", "Initialized", (context) => ((AnyApp)context).InitializingComplete())
                .BuildDefinition();
        }
        appFlowFSM = FSM_API.CreateInstance("ApplicationFlowFSM", this);

        // --- Define the User Interface FSM (updates every 5 frames) ---
        if (!FSM_API.Exists("UserInterfaceFSM"))
        {
            FSM_API.CreateFiniteStateMachine("UserInterfaceFSM",
                                             processRate: 5, // Updates every 5 ticks of its group.
                                             processingGroup: "GUI") // Custom group for UI.
                .State("MainMenu",
                    onEnter: (context) => Debug.Log("Entered Main Menu UI."),
                    onUpdate: (context) => Debug.Log("Main Menu active."),
                    onExit: (context) => Debug.Log("Exited Main Menu UI."))
                .State("SettingsMenu",
                    onEnter: (context) => Debug.Log("Entered Settings UI."),
                    onUpdate: (context) => Debug.Log("Settings active."),
                    onExit: (context) => Debug.Log("Exited Settings UI."))
                .WithInitialState("MainMenu")
                .Transition("MainMenu", "SettingsMenu", (context) => Input.GetKeyDown(KeyCode.S))
                .Transition("SettingsMenu", "MainMenu", (context) => Input.GetKeyDown(KeyCode.Escape))
                .BuildDefinition();
        }
        uiFSM = FSM_API.CreateInstance("UserInterfaceFSM", this);
    }

    // --- Methods for ApplicationFlowFSM transitions ---
    bool SplashComplete() => Time.time > 3f; // Example: Splash screen lasts 3 seconds.
    bool InitializingComplete() => Time.time > 5f; // Example: Init lasts 5 seconds.

    // --- Unity Lifecycle Methods to Tick FSMs ---

    void Update() // Runs once per frame.
    {
        // Tick our main application flow FSMs.
        FSM_API.Update("AppCore");

        // Example: If we had FSMs in the default "Update" group:
        // FSM_API.Update(); // Same as FSM_API.Update("Update");
    }

    void OnGUI() // Runs for GUI events.
    {
        // Tick our user interface FSMs here.
        // This ensures UI logic updates only when GUI events occur.
        FSM_API.Update("GUI");

        // Example: Display current FSM states
        GUI.Label(new Rect(10, 10, 200, 20), "App State: " + appFlowFSM.CurrentState);
        GUI.Label(new Rect(10, 30, 200, 20), "UI State: " + uiFSM.CurrentState);
    }

    // ... (Other methods like TakeDamage, MoveForward, PlayAnimation, IsDead, IsRevived) ...
    // You would typically put these in a separate context script,
    // but shown here for a complete example.
}

    What's Happening Here?

        processRate: -1: This FSM will update every single time its AppCore group is ticked. It's for very frequent, essential logic.

        processingGroup: "AppCore": This FSM belongs to a custom group named "AppCore."

        processRate: 5: This FSM will update only every 5th time its GUI group is ticked. This is good for less frequent UI logic, saving performance.

        processingGroup: "GUI": This FSM belongs to a custom group named "GUI."

        void Update(): This Unity method runs every frame. FSM_API.Update("AppCore") tells FSM_API to process all FSMs in the "AppCore" group.

        void OnGUI(): This Unity method runs when GUI events happen. FSM_API.Update("GUI") tells FSM_API to process all FSMs in the "GUI" group. This shows you can tick groups from any Unity method.

Ordering Your FSM Updates

You can control the order FSMs update by calling FSM_API.Update() for different groups. This is useful for complex game logic.

Example: Ensure parent FSMs update before child FSMs.
C#

void Update()
{
    // First, update all FSMs in the "ParentGroup".
    FSM_API.Update("ParentGroup");

    // Then, update all FSMs in the "ChildGroup".
    // This ensures child FSMs react to parent FSM changes in the same frame.
    FSM_API.Update("ChildGroup");

    // You could also have the default "Update" group.
    // FSM_API.Update(); // Same as FSM_API.Update("Update");
}

    What's Happening Here?

        FSM_API.Update("ParentGroup");: All FSMs defined for "ParentGroup" run first.

        FSM_API.Update("ChildGroup");: Then, all FSMs defined for "ChildGroup" run.

        This guarantees a specific execution order, essential for dependencies between FSMs.

4. Attach Script to a GameObject

Now that your script is ready, attach it in Unity!

    Create a 3D Object:
    In Unity, go to GameObject > 3D Object > Cube. This will create a new cube in your scene.

    Rename the GameObject:
    Select the Cube in the Hierarchy window. In the Inspector, change its name to PlayerCharacter.

    Attach Your Script:
    With PlayerCharacter selected, drag your PlayerCharacterContext script from the Project window onto the PlayerCharacter GameObject in the Inspector. Alternatively, click Add Component in the Inspector and search for PlayerCharacterContext.

    Run Your Game!
    Press the Play button in Unity. Watch the Console window for messages. Press Spacebar to damage your player. Press R to revive after death. You will see the FSM transition between "Alive" and "Dead" states!

Congratulations! You've successfully set up your first FSM in Unity using FSM_API.

[Continue to: 03. Getting Started with C# (Non-Unity)]