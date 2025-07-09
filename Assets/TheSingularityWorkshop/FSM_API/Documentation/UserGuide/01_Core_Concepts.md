What is a Finite State Machine (FSM)?

Imagine anything in a game or app. It acts differently based on its current "mode" or "stage."

    A character might be Idle, then Walking, then Jumping.

    A door might be Closed, then Opening, then Open.

    A UI button might be Enabled, then Pressed, then Disabled.

A Finite State Machine (FSM) is simply a way to manage these modes. It's a system that can only be in one "state" at a time. It has a limited, or "finite," number of possible states. And it has clear rules for "transitions" (moving from one state to another).

Think of it like a light switch:

    It has two states: On and Off.

    There's a transition from Off to On (flipping the switch up).

    There's a transition from On to Off (flipping the switch down).

    It cannot be "half-on" or "both-on-and-off." It's one state at a time.

FSMs help you organize complex behaviors cleanly. They make it easy to understand what's happening and predict what will happen next.

Understanding the Core Concepts of FSMs

Now that you know what an FSM is, let's look at its pieces. Each concept below plays a distinct role in an FSM's structure.

FSM Definition: The Blueprint

Think of an FSM Definition as the complete plan or blueprint for how a certain behavior should work.

    What it is: This blueprint lays out all the "stages" your character, item, or game logic can be in (like "Walking," "Sleeping," or "Door Open"). It also defines the rules for moving between these stages and what actions happen in each stage.

    Why it matters: Once you have this blueprint, you can use it many times! For example, you can design a "Basic Enemy Behavior" blueprint once, then use that same plan to control hundreds of different enemies. Each acts on its own, but they all follow the same underlying design, saving you from repeating work.

FSM Instance: The Live Version

If an FSM Definition is the blueprint, an FSM Instance is the actual, live "thing" that's running in your game or application. It's a specific character actually walking, a specific door actually opening, or specific game logic actually deciding what to do next.

    What it is: This is a live, active version of your FSM blueprint. It's always connected to a specific "thing" in your game (like a player character, a monster, or even a button).

    Purpose: The FSM Instance is your main way to observe and, if needed, directly influence one of these live FSMs. It tells you what state the FSM is currently in (e.g., "walking").

    Key Point: Multiple FSM Instances can share the same FSM Definition but operate entirely independently. Each manages its own current stage and actions, even though they all follow the same initial blueprint.

State: The "What's Happening Now"

A State is simply a named stage or phase that your FSM can be in. It's the "what's happening right now" for the thing your FSM is controlling.

    What it is: A single, distinct phase like "Idle," "Moving," "Attacking," "Door_Closed," or "UI_Enabled."

    How you define its actions (what happens): When you design your FSM blueprint, you specify what should happen for each named state:

        On Enter: This is action that runs once, the moment the FSM enters this state. Perfect for starting an animation or playing a sound.

        On Update: This is action that runs repeatedly, as long as the FSM stays in this state. Good for things that happen over time, like moving a character.

        On Exit: This is action that runs once, the moment the FSM leaves this state. Useful for stopping animations or cleaning up.

State Context: Your Data Connection

The State Context is simply your object – the actual thing in your game that the FSM is controlling. It's how the FSM can "talk" to your game's data and features.

    What it is: This is your own character, item, or piece of game logic. The FSM needs this context to know what it's supposed to be influencing.

    Purpose: The FSM itself is generic. It doesn't inherently know about "player health" or "door position." It needs your context object to get that information. When your FSM is in a certain state, its actions will use this context to read data (e.g., "is the door open?") or make changes (e.g., "open the door").

    How it works: To allow the FSM to interact with your object, your object must provide certain basic information, like a way to identify itself (a Name) and whether it's still active or relevant in the game (Valid). This ensures the FSM can safely work with your game's elements. For example, if a game character is removed from the game, the FSM can be informed that its associated object is no longer valid.