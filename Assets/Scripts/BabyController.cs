using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BabyController : MonoBehaviour
{
    
    [SerializeField]
    protected float Speed = 5; // Movement speed of the baby
    [SerializeField]
    protected bool Found = false; // Indicate if the baby has been found by the player

    // Private
    private SpriteRenderer SpriteRenderer; 
    private Rigidbody2D RigidBody; 
    private MovementStateEnum MovementState; 
    private int FacingDirection; // Direction baby is facing (-1 for left, 1 for right)

    [Header("Follow parameters")]

    
    [SerializeField, Tooltip("How far can the player move without the baby following")]
    protected float RangeToFollow = 2; // Distance the player can move away before the baby starts following
    [SerializeField, Tooltip("Once the player leaves the baby following range, how long should the baby wait to follow")]
    protected float DelayToFollow = 1; // Delay before the baby starts following after the player exits the follow range

    private GameObject PlayerObject; 
    private CircleCollider2D RangeToFollowCollider; 
    private float TimerToFollow; 

    [Header("Roam parameters")]
   
   [SerializeField, Tooltip("How far away from the player can the baby roam around")]
    protected float MaxDistanceToRoam = 1; 
    [SerializeField, Tooltip("How long does it take for the baby to roam around")]
    protected float IntervalToRoam = 5; 

    private Vector2 RoamTarget; 
    private float TimerToRoam; 

    void Awake()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>(); 
        RigidBody = GetComponent<Rigidbody2D>(); 
    }


    void Start()
    {
        InstantiateRangeToFollowCollider(); // Create the collider for the follow range
        MovementState = MovementStateEnum.IDLE; 
        RoamTarget = transform.position; // Set the initial roam target to the baby's current position
    }

    void FixedUpdate()
    {
        if (!Found) 
            return;

        if (MovementState != MovementStateEnum.FOLLOW) 
            Roam();

        if (MovementState != MovementStateEnum.ROAM) 
            Follow();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) // If the colliding object is not the player, ignore
            return;

        PlayerObject = collision.gameObject; // Store a reference to the player

        if (!Found) // If the baby hasn't been found yet
        {
            Found = true; // Mark the baby as found
            GameManager.instance.Rescue(gameObject); // Notify the GameManager that the baby has been rescued
        }

        if (MovementState == MovementStateEnum.FOLLOW) // If the baby was following the player
        {
            MovementState = MovementStateEnum.IDLE; // Change to idle
            GetNewRoamTarget(); // Get a new roam target
        }
    }

    /// Called when another collider exits the trigger collider.
    void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) // If the exiting object is not the player, ignore
            return;

        if (MovementState != MovementStateEnum.FOLLOW) // If the baby was not following
        {
            MovementState = MovementStateEnum.FOLLOW; // Change to follow mode
            TimerToFollow = DelayToFollow; // Start the delay timer
        }
    }

    /// Creates the CircleCollider2D for the follow range.
    private void InstantiateRangeToFollowCollider()
    {
        RangeToFollowCollider = gameObject.AddComponent<CircleCollider2D>(); // Add a CircleCollider2D component
        RangeToFollowCollider.radius = RangeToFollow; // Set the radius of the collider
        RangeToFollowCollider.isTrigger = true; // Make it a trigger collider
    }

    /// Makes the baby follow the player.
    private void Follow()
    {
        if (TimerToFollow > 0) // If there's a delay
            TimerToFollow -= Time.deltaTime; // Decrement the timer

        Vector3 playerPosition = PlayerObject.transform.position; // Get the player's position

        float deltaX = playerPosition.x - transform.position.x; // Calculate the difference in x
        float deltaY = playerPosition.y - transform.position.y; // Calculate the difference in y

        float horizontal = Mathf.Abs(deltaX) <= 0.1f ? 0 : deltaX / Mathf.Abs(deltaX); // Calculate horizontal movement
        float vertical = Mathf.Abs(deltaY) <= 0.1f ? 0 : deltaY / Mathf.Abs(deltaY); // Calculate vertical movement

        Vector2 velocity = new Vector2(0, 0); // Initialize velocity

        if (MovementState == MovementStateEnum.FOLLOW && TimerToFollow <= 0) // If in follow mode and the delay is over
        {
            LookAt(playerPosition); // Make the baby look at the player
            velocity = new Vector2(horizontal, vertical).normalized; // Calculate the normalized velocity towards the player
        }

        RigidBody.velocity = velocity * Speed; // Set the baby's velocity
    }

    /// Makes the baby roam around.
    private void Roam()
    {
        if (TimerToRoam > 0) // If there's a roam timer
            TimerToRoam -= Time.deltaTime; // Decrement the timer

        if (MovementState != MovementStateEnum.ROAM && TimerToRoam <= 0) // If not in roam mode and the timer is up
            MovementState = MovementStateEnum.ROAM; // Change to roam mode

        float distanceToTarget = Vector3.Distance(RoamTarget, transform.position); // Calculate the distance to the roam target

        float deltaX = RoamTarget.x - transform.position.x; // Calculate the difference in x
        float deltaY = RoamTarget.y - transform.position.y; // Calculate the difference in y

        float horizontal = Mathf.Abs(deltaX) <= 0.1f ? 0 : deltaX / Mathf.Abs(deltaX); // Calculate horizontal movement
        float vertical = Mathf.Abs(deltaY) <= 0.1f ? 0 : deltaY / Mathf.Abs(deltaY); // Calculate vertical movement

        var velocity = new Vector2(horizontal, vertical).normalized * Speed; // Calculate the normalized velocity towards the roam target

        LookAt(RoamTarget); // Make the baby look at the roam target

        RigidBody.velocity = velocity; // Set the baby's velocity

        if (distanceToTarget <= 0.2f) // If the baby is close to the roam target
        {
            GetNewRoamTarget(); // Get a new roam target
            MovementState = MovementStateEnum.IDLE; // Change to idle mode
            TimerToRoam = IntervalToRoam; // Reset the roam timer
        }
    }

    /// Gets a new random roam target within the allowed distance.
    private void GetNewRoamTarget()
    {
        Vector2 randomOffset = Random.insideUnitCircle * MaxDistanceToRoam; // Get a random point within a circle
        RoamTarget = (Vector2)transform.position + randomOffset; // Set the new roam target
    }

    /// Makes the baby look at a target position.
    private void LookAt(Vector3 target)
    {
        float deltaX = target.x - transform.position.x; // Calculate the difference in x

        float horizontal = Mathf.Abs(deltaX) == 0 ? 0 : deltaX / Mathf.Abs(deltaX); // Calculate horizontal direction

        ChangeDirection(horizontal); // Change the baby's facing direction
    }

    /// Changes the baby's facing direction.
    public void ChangeDirection(float horizontal)
    {
        if (horizontal == 0) // If there's no horizontal movement, do nothing
            return;

        if (horizontal == FacingDirection) // If already facing the correct direction, do nothing
            return;

        FacingDirection = (int)horizontal; // Update the facing direction

        gameObject.transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y); // Flip the sprite
    }

    /// Detects if there are any obstacles in the way.
    
    private bool DetectObstacles()
    {
        float detectionRange = 3f; // Range to detect obstacles
        LayerMask obstacleLayer = LayerMask.GetMask("Walls"); // Layer to check for obstacles

        Vector2 moveDirection = transform.position; // Default to current position

        if (MovementState == MovementStateEnum.FOLLOW) // If following, move towards the player
            moveDirection = PlayerObject.transform.position;
        else if (MovementState == MovementStateEnum.ROAM) // If roaming, move towards the roam target
            moveDirection = RoamTarget;

        moveDirection -= (Vector2)transform.position; // Calculate the direction to move

        RaycastHit2D hit = Physics2D.Raycast(transform.position, moveDirection, detectionRange, obstacleLayer); // Cast a ray to detect obstacles

        if (hit.collider != null) // If an obstacle is detected
        {
            Debug.DrawRay(transform.position, moveDirection * detectionRange, Color.red); // Draw a red ray in the editor
            return true;
        }
        else
        {
            Debug.DrawRay(transform.position, moveDirection * detectionRange, Color.green); // Draw a green ray in the editor
            return false;
        }
    }
}
