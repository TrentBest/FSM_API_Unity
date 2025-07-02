06. RNG Utility (TheSingularityWorkshop.Squirrel.RNG)

The RNG utility is fast, deterministic, stateless.
It provides random number generation.
Included with FSM_API for your needs.
Use it in games and FSMs.

Why a Custom RNG?

    Performance: Optimized for speed and low overhead.

    Determinism: Same input always gives same output.

    Control: Fine-grained seeding for reproducible results.

    FSM Integration: Use randomness in transitions and actions.

Core Features: Deterministic Randomness

The RNG class is stateless.
This means it holds no internal changing values.
Each method call is independent.
Its output solely depends on the inputs provided.

It is also deterministic.
Give it the same inputs (pos, seed).
You will always get the exact same output.
This is crucial for procedural generation.
And for reproducible game behaviors.

You provide a pos (position/index).
And an optional seed (random universe).
This combination generates unique, fixed values.

Key Methods and Usage

To use RNG, simply call its static methods.
You will need to import its namespace.
C#

using TheSingularityWorkshop.Squirrel; // Add this using directive
// ...
// Then call methods like: RNG.Value01(...)

1. Hash(int pos, int seed = 0) and UHash(uint pos, uint seed = 0)

These are the core hashing functions.
They generate a deterministic 32-bit integer hash.
From an integer position and optional seed.
UHash is the unsigned integer version.
Most other RNG methods use these internally.
C#

int hashValue1 = RNG.Hash(0);     // First hash in default universe
int hashValue2 = RNG.Hash(10, 123); // Hash at pos 10, seed 123

2. Value01(int pos, int seed = 0)

Generates a deterministic float value.
Between 0.0 (inclusive) and 1.0 (inclusive).
Based on an integer position and optional seed.
This is perfect for probabilities.
C#

float randomFloat = RNG.Value01(myObject.GetHashCode()); // Use object's hash
// In an FSM transition:
// .Transition("Idle", "Wander", (ctx) => RNG.Value01(ctx.GetHashCode()) < 0.5f)

3. Range(int pos, float min, float max, int seed = 0)

Generates a deterministic float value.
Within a specified range [min, max].
Based on position and optional seed.
C#

float randomAngle = RNG.Range(transform.GetInstanceID(), 0f, 360f);

4. RangeInt(int pos, int min, int max, int seed = 0)

Generates a deterministic integer value.
Within a specified range [min, max].
Based on position and optional seed.
The max value is inclusive.
C#

int enemyCount = RNG.RangeInt(currentLevelId, 5, 10); // Between 5 and 10 enemies

5. Seeding Methods (Seed)

These methods help you generate consistent seeds.
For different random "universes."
Or to combine multiple factors into one seed.

    Seed(uint seed): Generates a master seed from an unsigned integer.

    Seed(int seed): Generates a master seed from a signed integer.
    Incorporates a hashing salt for better distribution.

    Seed(string seed): Generates a seed from a string.
    Useful for level names or unique identifiers.
    C#

int levelSeed = RNG.Seed("ForestLevelA");
float treeDensity = RNG.Value01(0, levelSeed);

Seed(params object[] args): Combines multiple objects into one seed.
Useful for complex situations.
C#

    // Combine level ID and player ID for a unique seed
    int combinedSeed = RNG.Seed(levelID, playerID, "QuestEvent");

6. HashFromVec3(Vector3 v)

Computes a hash from a Vector3 position.
Allows for seeding random generation.
Based on spatial coordinates.
C#

int spatialHash = RNG.HashFromVec3(transform.position);
Vector3 spawnPos = RNG.WithinCircle(spatialHash, 5f);

7. SelectIndexByProbability(float[] weights, int pos, int seed = 0)

Selects an index from an array of probabilities (weights).
Deterministically, based on pos and seed.
Useful for weighted loot drops or AI decisions.
C#

float[] itemWeights = { 0.7f, 0.2f, 0.1f }; // Common, Uncommon, Rare
int itemIndex = RNG.SelectIndexByProbability(itemWeights, enemyID);

8. WithinCircle(int pos, float radius, int seed = 0)

Generates a deterministic Vector3 point.
Within a 2D circle (on the XZ plane).
Based on position and optional seed.
The Y-component is always 0.
C#

Vector3 randomTarget = RNG.WithinCircle(agentID, 10f, currentLevelSeed);

FSM Integration Example

RNG seamlessly integrates into FSM states and transitions.
Use an FSM context's unique identifier.
(Like GetHashCode() or GetInstanceID() for Unity objects).
As the pos parameter.
Use a consistent game or level seed as the seed.
C#

// Example using PlayerCharacterContext from previous document
using TheSingularityWorkshop.FSM.API;
using TheSingularityWorkshop.Squirrel; // For RNG utilities
using UnityEngine; // For GetInstanceID()

public class PlayerFSMDefinitions
{
    private static int _globalGameSeed = RNG.Seed("MyGameWorld"); // Consistent world seed

    public static void DefinePlayerMovementFSM()
    {
        if (!FSM_API.Exists("PlayerMovementFSM"))
        {
            FSM_API.CreateFiniteStateMachine("PlayerMovementFSM")
                .State("Idle",
                    onUpdate: (context) => {
                        // 10% chance to start wandering every update (using context hash as position)
                        if (RNG.Value01(context.GetHashCode(), _globalGameSeed) < 0.1f)
                        {
                            ((PlayerCharacterContext)context).StartMovement();
                            // FSM_API.RequestTransition will process immediately
                            FSM_API.RequestTransition(context, "PlayerMovementFSM", "Wandering");
                        }
                    })
                .State("Wandering",
                    onEnter: (context) => {
                        Debug.Log("Player starts wandering.");
                        // Determine wander duration based on context instance ID
                        float wanderDuration = RNG.Range(((MonoBehaviour)context).GetInstanceID(), 2f, 5f, _globalGameSeed);
                        // Store this duration in the context or a temporary variable
                        ((PlayerCharacterContext)context).SetWanderTimer(wanderDuration);
                    },
                    onUpdate: (context) => {
                        // Move character, check if wander timer is up
                        if (((PlayerCharacterContext)context).UpdateWander())
                        {
                            FSM_API.RequestTransition(context, "PlayerMovementFSM", "Idle");
                        }
                    })
                .WithInitialState("Idle")
                .BuildDefinition();
        }
    }
}

// Inside PlayerCharacterContext (updated)
public class PlayerCharacterContext : MonoBehaviour, IStateContext
{
    public string Name { get; set; }
    public bool IsValid => this != null && gameObject.activeInHierarchy;

    private float _currentWanderTimer;

    void Awake() { Name = gameObject.name; }

    public void SetWanderTimer(float duration)
    {
        _currentWanderTimer = duration;
    }

    // Returns true if wander finished
    public bool UpdateWander()
    {
        _currentWanderTimer -= Time.deltaTime;
        Debug.Log($"Wandering... {_currentWanderTimer:F2}");
        return _currentWanderTimer <= 0f;
    }

    public void StartMovement() { /* ... */ }
    public void StopMovement() { /* ... */ }
    public void TakeDamage(float amount) { /* ... */ }
}

Acknowledgments

This RNG utility is deeply inspired by the work of Squirrel Eiserloh.
His "Squirrel Noise" functions revolutionized deterministic randomness.
Particularly for procedural generation in games.
We are grateful for his innovative approaches.
And for sharing his elegant bit-mangling techniques.