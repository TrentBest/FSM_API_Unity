10. Getting Started: FSMs for Designers & Non-Coders

Welcome! If you've been curious about making your game characters, objects, or UI behave in smart and predictable ways, but found coding intimidating, you're in the right place. This document is designed to bridge the gap, helping you understand how Finite State Machines (FSMs) work with our FSM_API, even if you don't write the code yourself.

Think of this as a way for you, the designer, to speak the language of "behavior" very clearly, so that developers can easily bring your ideas to life.

What is a Finite State Machine (FSM)? - The Core Idea

Imagine anything that can act in different "modes" or "phases." A traffic light, a light switch, or even a character in a game. At any given moment, these things are doing one specific thing. They are in one specific state.

    A traffic light can be Red, Yellow, or Green. It can't be Red and Green at the same time.

    A light switch can be On or Off.

    A game character might be Idle, Walking, Running, Jumping, or Attacking.

A Finite State Machine (FSM) is simply a way to formally describe these behaviors:

    It has a limited (finite) number of "states" it can be in.

    It can only be in one state at any given moment.

    It changes from one state to another based on specific "events" or "conditions."

Think of it like a Flowchart: You draw boxes for each state, and arrows pointing from one box to another showing how to get from State A to State B. That's essentially an FSM!

Why is this helpful?
FSMs make behavior predictable and easy to understand. If you know an object is in the "Attacking" state, you know what it's supposed to be doing and what it can't be doing (like jumping, if jumping isn't allowed during an attack). This clarity is invaluable for both designers and coders.

The Building Blocks of an FSM

Every FSM is built from three core components:

1. States

A State is a distinct "mode" or "activity" that an object can be in.

    Examples: "Door_Closed," "Door_Opening," "Enemy_Patrolling," "Enemy_Chasing," "UI_MainMenu," "UI_Options."

    What happens in a State? When an object enters a state, while it's in that state, and when it leaves it, specific actions can occur:

        On Enter: What should happen immediately when the FSM enters this state?

            Example: For "Door_Opening" state: "Start playing the door opening animation."

        On Update: What should happen continuously or repeatedly while the FSM is in this state?

            Example: For "Door_Opening" state: "Keep moving the door mesh until it's fully open."

        On Exit: What should happen immediately when the FSM leaves this state?

            Example: For "Door_Opening" state: "Stop the door opening sound effect."

2. Transitions

A Transition is the pathway or rule that allows an FSM to move from one state to another. You can only transition from your current state to another connected state.

    Example: From "Door_Closed" to "Door_Opening."

    The Condition: A transition only "fires" (meaning, the FSM changes state) if a specific condition is met. This condition is a simple question that evaluates to "true" or "false."

        Example: To go from "Door_Closed" to "Door_Opening," the condition might be: "Is the player pressing the 'Use' button while near the door?" If true, the transition happens.

3. The Context: Where the FSM Gets its Information and Does its Work

Think of the Context as the "brain" or "data center" for a specific, living instance of your FSM. It's an object that holds all the information the FSM needs to make decisions and perform actions.

    What it holds: This could be anything relevant to the FSM's behavior:

        For a character FSM: its health, position, target, speed.

        For a door FSM: its current open/closed position, whether it's locked.

        For a light bulb FSM: whether its actual light component is currently on or off.

    What it does: The FSM's states and transitions don't directly manipulate your game objects (like moving a character or turning on a light). Instead, they tell the Context to do it. The Context then contains the actual code that performs these actions.

    IsValid Property: Each Context also has a simple IsValid check. This tells the FSM_API if the object connected to this FSM is still alive and active (e.g., if a character hasn't been destroyed). This helps the system automatically clean up FSMs that are no longer needed.

Bringing Your FSM Designs to Life with FSM_API

The FSM_API is the programming tool that developers use to turn your FSM ideas into functional game behavior. It handles the "behind-the-scenes" complexities, letting you focus on the "what" of the behavior, while the developers handle the "how."

1. The Blueprint (Defining an FSM)

Before you can have many enemies or doors behaving a certain way, you first need to describe how they should behave. This is like drawing the original flowchart or blueprint.

    Developers use FSM_API.CreateFiniteStateMachine() to define this blueprint. They list all the states, what happens in each, and all the possible transitions between them.

    Key Idea: This definition happens only once for each type of FSM (e.g., one blueprint for "Enemy_AI," one for "Door_Mechanism").

2. The Live Object (Creating an Instance)

Once you have a blueprint (definition), you can create many live instances of that FSM.

    Developers use FSM_API.CreateInstance() to create a new, independent FSM that follows a specific blueprint.

    Key Idea: Each game object (e.g., every individual enemy character, every door in your level) gets its own FSM instance. Each instance uses its own Context to manage its specific data and actions. So, one door opening doesn't cause all other doors to open!

3. The FSM "Engine" (Processing Groups)

Your FSMs don't just magically run. Something needs to tell them to "think" or "update." This is where Processing Groups come in, managed by FSM_API.Update().

    Separate Channels: You can organize your FSM instances into different "processing groups" (e.g., "PlayerInput," "EnemyAI," "UIElements," "BackgroundStuff").

    Controlled Updates: Developers then decide when and how often each group gets updated.

        FSM_API.Update("PlayerInput"): Might be called every single game frame, because player input needs instant reaction.

        FSM_API.Update("EnemyAI"): Could be called every few frames for less critical enemies, saving computer power.

        FSM_API.Update("UIElements"): Might only be called when a menu is actually open, otherwise it sits idle.

        FSM_API.Update() (No Group Name): If an FSM definition doesn't specify a group, it defaults to the "Update" group, which is usually updated every frame.

    Your Control: This is powerful! You (or the developer) can strategically decide which groups need constant attention and which can "sleep" or only update occasionally. This is key to making large games run smoothly.

Why FSM_API Empowers You, the Designer

You might not be writing the C# code, but FSMs give you a direct and powerful way to define complex behaviors.

    You Define the "What": You focus on sketching out the states, the actions within them, and the rules for transitioning. What does the "Door" do when "Opening"? How does it get from "Closed" to "Opening"?

    Coders Handle the "How": The developers then take your clear FSM design and implement the C# code that tells the FSM_API to build those states and transitions, and how your Context object actually performs the actions (like playing an animation or checking player input).

    Clear Communication: FSM diagrams are a universal language for behavior. This makes it much easier for designers and developers to talk about and build complex systems together.

    Predictable and Organized: Because behavior is defined step-by-step in states, it's easier to understand, test, and find problems.

Key Terms to Google (For a Deeper Dive)

If you want to understand more about these concepts (without necessarily learning to code deeply), here are some terms you can search for online. Just add "game dev" or "programming" to your search for more relevant results!

    Finite State Machine (FSM): The core concept.

    State Diagram / State Chart: Visual ways to represent FSMs.

    Event-Driven Programming: A style of programming where actions are triggered by "events" (like clicking a button or a timer finishing).

    Game Loop: The main cycle (often Update/FixedUpdate in Unity) that drives everything in a game.

    Object-Oriented Programming (OOP): A fundamental programming paradigm that helps organize code using "objects" (like our Contexts).

    C#: The programming language used by FSM_API. You don't need to master it, but understanding its role is helpful.

Conclusion

Don't let the idea of "coding" intimidate you when it comes to designing complex game behaviors. FSM_API is built to make the power of Finite State Machines accessible, allowing you to define clear, predictable, and robust behaviors for your games and applications. You are the architect of these behaviors, and with FSM_API, your designs can come to life more directly than ever before.

[➡️ Continue: 11 FAQ](11_FAQ.md)
