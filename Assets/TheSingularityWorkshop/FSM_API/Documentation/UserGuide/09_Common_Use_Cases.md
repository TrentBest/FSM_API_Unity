09. Common Use Cases for FSM_API

FSM_API is designed to be flexible.
It is powerful for many game and application scenarios.
Below are practical examples.
They should inspire your own FSM designs.

1. Enemy AI FSM

Concept:
An enemy that patrols, chases the player, attacks, and retreats.
Its behavior changes dynamically.
Based on player proximity and health.

States & Transitions:

    Patrol: Enemy moves between predefined points.

        ? Chase (if player detected within vision range)

    Chase: Enemy actively pursues the player.

        ? Attack (if player within attack range)

        ? Patrol (if player lost or too far)

    Attack: Enemy performs offensive actions.

        ? Retreat (if enemy health is low)

        ? Chase (if player moves out of attack range)

    Retreat: Enemy moves to a safe location.

        ? Patrol (if enemy health recovered and safe)

        ? Chase (if player gets too close during retreat)

IStateContext Properties & Methods:

The enemy context will hold its state data.
And provide actions for the FSM to call.

    Vector3 Position: Enemy's current location.

    Transform Target: The player's transform.

    float Health: Current hit points.

    float Speed: Movement speed.

    float VisionRange: How far enemy can see.

    float AttackRange: How close to attack.

    float RetreatThreshold: Health to start retreating.

    bool IsTargetVisible(): Line of sight check.

    bool IsTargetInAttackRange(): Distance check.

    void MoveTowardsTarget(): Logic to move.

    void MoveToSafeSpot(): Logic to retreat.

    void PerformAttack(): Attack action.

    bool IsAttackOnCooldown(): Check attack timing.

    bool IsSafeSpotReached(): Retreat goal achieved.

    void RestoreHealth(): Healing over time.

A. EnemyAIContext (MonoBehaviour)

This context will be a component.
It attaches to your enemy GameObject.
It holds data and performs basic actions.

C#
C#

using UnityEngine;
using TheSingularityWorkshop.FSM.API; // For IStateContext

public class EnemyAIContext : MonoBehaviour, IStateContext
{
    [Header("Enemy Stats")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public float retreatSpeed = 3f;
    public float attackDamage = 10f;
    public float attackCooldown = 1.5f;
    public float visionRange = 10f;
    public float attackRange = 2f;
    public float retreatHealthThreshold = 30f;
    public float safeDistance = 15f; // Distance from player to be "safe" during retreat

    [Header("References")]
    public Transform playerTransform; // Assign player in Inspector
    public Transform[] patrolWaypoints; // Assign waypoints in Inspector

    // Internal FSM context properties
    public string Name { get; set; }
    public bool IsValid => this != null && gameObject.activeInHierarchy;

    // AI-specific internal state
    private int _currentWaypointIndex = 0;
    private float _lastAttackTime;
    private Vector3 _retreatTargetPosition; // Where the enemy is retreating to

    void Awake()
    {
        Name = gameObject.name;
        currentHealth = maxHealth;
        _lastAttackTime = -attackCooldown; // Allow immediate attack
    }

    void Update()
    {
        // Simple health regeneration when safe (for retreat transition)
        if (IsSafe() && currentHealth < maxHealth)
        {
            currentHealth += Time.deltaTime * 5f; // Regenerate 5 health per second
            currentHealth = Mathf.Min(currentHealth, maxHealth);
        }
    }

    // --- FSM Condition Methods ---

    public bool IsTargetVisible()
    {
        if (playerTransform == null) return false;
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        // Simple line of sight: just distance for this example
        return distance <= visionRange;
    }

    public bool IsTargetInAttackRange()
    {
        if (playerTransform == null) return false;
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        return distance <= attackRange;
    }

    public bool IsHealthLow()
    {
        return currentHealth <= retreatHealthThreshold;
    }

    public bool IsSafe()
    {
        if (playerTransform == null) return true; // Treat as safe if no player
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        // Safe if far enough from player AND health is good
        return distance > safeDistance && currentHealth >= maxHealth;
    }
    
    // --- FSM Action Methods ---

    public void StartPatrol()
    {
        Debug.Log($"{Name} entered Patrol. Moving to waypoint {_currentWaypointIndex}.");
        // Ensure waypoints exist
        if (patrolWaypoints.Length > 0)
        {
            _currentWaypointIndex = 0; // Start from first waypoint
            SetDestination(patrolWaypoints[_currentWaypointIndex].position);
        }
    }

    public void PatrolUpdate()
    {
        if (patrolWaypoints.Length == 0) return;

        // Move towards current waypoint
        transform.position = Vector3.MoveTowards(transform.position,
                                                 patrolWaypoints[_currentWaypointIndex].position,
                                                 patrolSpeed * Time.deltaTime);

        // If reached waypoint, move to next
        if (Vector3.Distance(transform.position, patrolWaypoints[_currentWaypointIndex].position) < 0.1f)
        {
            _currentWaypointIndex = (_currentWaypointIndex + 1) % patrolWaypoints.Length;
            SetDestination(patrolWaypoints[_currentWaypointIndex].position);
        }
    }

    public void StartChase()
    {
        Debug.Log($"{Name} entered Chase. Pursuing player.");
        // Stop any previous movement to patrol waypoint
        SetDestination(playerTransform.position); // Initially aim at player
    }

    public void ChaseUpdate()
    {
        if (playerTransform == null) return;
        // Keep moving towards the player
        transform.position = Vector3.MoveTowards(transform.position,
                                                 playerTransform.position,
                                                 chaseSpeed * Time.deltaTime);
        // Optional: Look at player
        transform.LookAt(playerTransform.position);
    }

    public void StartAttack()
    {
        Debug.Log($"{Name} entered Attack. Ready to strike!");
        _lastAttackTime = Time.time; // Reset attack timer for immediate attack
    }

    public void AttackUpdate()
    {
        if (playerTransform == null) return;
        // Look at player while attacking
        transform.LookAt(playerTransform.position);

        if (Time.time - _lastAttackTime >= attackCooldown)
        {
            PerformAttack();
            _lastAttackTime = Time.time; // Reset for next attack
        }
    }
    
    private void PerformAttack()
    {
        Debug.Log($"{Name} attacked player for {attackDamage} damage!");
        // Placeholder for applying damage to player
        // playerTransform.GetComponent<PlayerHealth>()?.TakeDamage(attackDamage);
    }

    public void StartRetreat()
    {
        Debug.Log($"{Name} entered Retreat. Finding safety.");
        // Find a safe spot away from the player
        // For simplicity, just move away in opposite direction
        if (playerTransform != null)
        {
            _retreatTargetPosition = transform.position + (transform.position - playerTransform.position).normalized * safeDistance * 2;
        }
        else
        {
            _retreatTargetPosition = transform.position + transform.forward * safeDistance * 2; // Just move forward
        }
        SetDestination(_retreatTargetPosition);
    }

    public void RetreatUpdate()
    {
        // Move towards the retreat target
        transform.position = Vector3.MoveTowards(transform.position,
                                                 _retreatTargetPosition,
                                                 retreatSpeed * Time.deltaTime);
        // Optional: Look away from player
        if (playerTransform != null)
        {
             Vector3 lookAway = (transform.position - playerTransform.position).normalized;
             transform.rotation = Quaternion.LookRotation(lookAway);
        }
    }

    public bool HasReachedRetreatSpot()
    {
        return Vector3.Distance(transform.position, _retreatTargetPosition) < 1f; // Within 1 unit of target
    }

    // Helper for movement (if using pathfinding, replace this)
    private void SetDestination(Vector3 target)
    {
        // In a real game, this might use a NavMeshAgent.
        // For this example, just a target for MoveTowards.
        Debug.Log($"{Name} setting destination to: {target}");
    }
}

B. EnemyAIFSMDefinition (Static Class)

This class will define the FSM blueprint.
It should be called once at game startup.
For example, in a GameInitializer script.

C#
C#

using TheSingularityWorkshop.FSM.API;
using UnityEngine; // For Debug.Log (in a Unity project)

public static class EnemyAIFSMDefinition
{
    public static void DefineFSM()
    {
        // Check if the FSM already exists to prevent re-definition
        if (FSM_API.Exists("EnemyAI"))
        {
            Debug.Log("EnemyAI FSM already defined.");
            return;
        }

        FSM_API.CreateFiniteStateMachine("EnemyAI",
                                         processRate: -1, // Update every tick for responsive AI
                                         processingGroup: "EnemyLogic") // Custom processing group
            // Define States
            .State("Patrol",
                onEnter: (context) => ((EnemyAIContext)context).StartPatrol(),
                onUpdate: (context) => ((EnemyAIContext)context).PatrolUpdate())
            .State("Chase",
                onEnter: (context) => ((EnemyAIContext)context).StartChase(),
                onUpdate: (context) => ((EnemyAIContext)context).ChaseUpdate())
            .State("Attack",
                onEnter: (context) => ((EnemyAIContext)context).StartAttack(),
                onUpdate: (context) => ((EnemyAIContext)context).AttackUpdate())
            .State("Retreat",
                onEnter: (context) => ((EnemyAIContext)context).StartRetreat(),
                onUpdate: (context) => ((EnemyAIContext)context).RetreatUpdate())

            // Define Transitions
            // Patrol -> Chase: If player becomes visible
            .Transition("Patrol", "Chase", (context) => ((EnemyAIContext)context).IsTargetVisible())

            // Chase -> Attack: If player is within attack range
            .Transition("Chase", "Attack", (context) => ((EnemyAIContext)context).IsTargetInAttackRange())
            // Chase -> Patrol: If player is no longer visible
            .Transition("Chase", "Patrol", (context) => !((EnemyAIContext)context).IsTargetVisible())

            // Attack -> Retreat: If health is low
            .Transition("Attack", "Retreat", (context) => ((EnemyAIContext)context).IsHealthLow())
            // Attack -> Chase: If player moves out of attack range during attack
            .Transition("Attack", "Chase", (context) => !((EnemyAIContext)context).IsTargetInAttackRange())

            // Retreat -> Patrol: If safe spot reached AND health is good
            .Transition("Retreat", "Patrol", (context) => ((EnemyAIContext)context).HasReachedRetreatSpot() && ((EnemyAIContext)context).IsSafe())
            // Retreat -> Chase: If player gets too close again during retreat
            .Transition("Retreat", "Chase", (context) => ((EnemyAIContext)context).IsTargetVisible() && !((EnemyAIContext)context).IsSafe())


            // Set the initial state for new FSM instances
            .WithInitialState("Patrol")
            .BuildDefinition(); // Finalize and register the FSM blueprint

        Debug.Log("EnemyAI FSM definition built and registered successfully.");
    }
}

C. Instantiating the FSM in Unity

Attach this script to your enemy GameObject.
It will create and manage the FSM instance.

C#
C#

using UnityEngine;
using TheSingularityWorkshop.FSM.API; // For FSM_API

[RequireComponent(typeof(EnemyAIContext))] // Ensures context component exists
public class EnemyAIController : MonoBehaviour
{
    private EnemyAIContext _enemyContext; // Reference to our context
    private IFSM _enemyFSM;               // The FSM instance for this enemy

    void Awake()
    {
        _enemyContext = GetComponent<EnemyAIContext>();
        if (_enemyContext == null)
        {
            Debug.LogError("EnemyAIContext not found on this GameObject.", this);
            return;
        }

        // Define the FSM blueprint ONCE (e.g., at game start from a dedicated initializer)
        // For demonstration, we'll call it here, but ideally from a central place.
        EnemyAIFSMDefinition.DefineFSM();

        // Create an FSM instance for THIS enemy, using its context.
        _enemyFSM = FSM_API.CreateInstance("EnemyAI", _enemyContext);
        if (_enemyFSM == null)
        {
            Debug.LogError($"Failed to create FSM instance for {_enemyContext.Name}.", this);
        }
        else
        {
            Debug.Log($"FSM '{_enemyFSM.FsmName}' created for '{_enemyContext.Name}'. Initial state: {_enemyFSM.CurrentStateName}");
        }
    }

    void OnDestroy()
    {
        // When the enemy GameObject is destroyed, the FSM instance
        // will automatically be removed by FSM_API due to IsValid.
        // No explicit Dispose or RemoveInstance call is strictly needed here.
        Debug.Log($"Enemy GameObject destroyed. FSM for '{_enemyContext?.Name}' will be cleaned up.");
    }

    // Remember to call FSM_API.Update("EnemyLogic") somewhere in your main game loop (e.g., GameManager's Update)
}

This example shows how FSM_API enables robust AI.
It combines state logic within the context.
And flexible transitions defined by the FSM builder.
This structure keeps your code clean.
It is easy to understand and extend.

[➡️ Continue: 10 Non Coder Overview](10_Non_Coder_Overview.md)
