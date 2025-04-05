using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BabyController : MonoBehaviour
{
    // General Vars

    // Editable on Editor
    [SerializeField]
    protected float Speed = 5;
    [SerializeField]
    protected bool Found = false;

    // Private
    private SpriteRenderer SpriteRenderer;
    private Rigidbody2D RigidBody;
    private MovementStateEnum MovementState;
    private int FacingDirection;

    [Header("Follow parameters")]

    // Editable on Editor
    [SerializeField, Tooltip("How far can the player move without the baby following")]
    protected float RangeToFollow = 2;
    [SerializeField, Tooltip("Once the player leaves the baby following range, how long should the baby wait to follow")]
    protected float DelayToFollow = 1;

    // Private
    private GameObject PlayerObject;
    private CircleCollider2D RangeToFollowCollider;
    private float TimerToFollow;

    [Header("Roam parameters")]

    // Editable on Editor
    [SerializeField, Tooltip("How far away from the player can the baby roam around")]
    protected float MaxDistanceToRoam = 1;
    [SerializeField, Tooltip("How long does it take for the baby to roam around")]
    protected float IntervalToRoam = 5;

    // Private
    private Vector2 RoamTarget;
    private float TimerToRoam;

    [Header("Obstacle Avoidance")]
    [SerializeField]
    private float obstacleDetectionRange = 1f;
    [SerializeField]
    private LayerMask obstacleLayer;
    [SerializeField]
    private float avoidanceForce = 2f;

    void Awake()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();
        RigidBody = GetComponent<Rigidbody2D>();
    }


    void Start()
    {
        InstantiateRangeToFollowCollider();
        MovementState = MovementStateEnum.IDLE;
        RoamTarget = transform.position;
    }

    void FixedUpdate()
    {
        //DetectObstacles();

        if (!Found)
            return;

        if (MovementState != MovementStateEnum.FOLLOW)
            Roam();

        if (MovementState != MovementStateEnum.ROAM)
            Follow();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
            return;

        PlayerObject = collision.gameObject;

        if (!Found)
        {
            Found = true;
            GameManager.instance.Rescue(gameObject);
        }

        if (MovementState == MovementStateEnum.FOLLOW)
        {
            MovementState = MovementStateEnum.IDLE;
            GetNewRoamTarget();
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
            return;

        if (MovementState != MovementStateEnum.FOLLOW)
        {
            MovementState = MovementStateEnum.FOLLOW;
            TimerToFollow = DelayToFollow;
        }
    }

    private void InstantiateRangeToFollowCollider()
    {
        RangeToFollowCollider = gameObject.AddComponent<CircleCollider2D>();
        RangeToFollowCollider.radius = RangeToFollow;
        RangeToFollowCollider.isTrigger = true;
    }

    private void Follow()
    {
        if (TimerToFollow > 0)
            TimerToFollow -= Time.deltaTime;

        Vector3 playerPosition = PlayerObject.transform.position;

        float deltaX = playerPosition.x - transform.position.x;
        float deltaY = playerPosition.y - transform.position.y;

        float horizontal = Mathf.Abs(deltaX) <= 0.1f ? 0 : deltaX / Mathf.Abs(deltaX);
        float vertical = Mathf.Abs(deltaY) <= 0.1f ? 0 : deltaY / Mathf.Abs(deltaY);

        Vector2 velocity = new Vector2(0, 0);

        if (MovementState == MovementStateEnum.FOLLOW && TimerToFollow <= 0)
        {
            LookAt(playerPosition);
            velocity = new Vector2(horizontal, vertical).normalized;
        }

        // Obstacle Avoidance
        Vector2 avoidanceDirection = CalculateAvoidanceDirection(velocity);
        velocity += avoidanceDirection * avoidanceForce;

        RigidBody.velocity = velocity * Speed;
    }

    private void Roam()
    {
        if (TimerToRoam > 0)
            TimerToRoam -= Time.deltaTime;

        if (MovementState != MovementStateEnum.ROAM && TimerToRoam <= 0)
            MovementState = MovementStateEnum.ROAM;

        float distanceToTarget = Vector3.Distance(RoamTarget, transform.position);

        float deltaX = RoamTarget.x - transform.position.x;
        float deltaY = RoamTarget.y - transform.position.y;

        float horizontal = Mathf.Abs(deltaX) <= 0.1f ? 0 : deltaX / Mathf.Abs(deltaX);
        float vertical = Mathf.Abs(deltaY) <= 0.1f ? 0 : deltaY / Mathf.Abs(deltaY);

        var velocity = new Vector2(horizontal, vertical).normalized;

        LookAt(RoamTarget);

        // Obstacle Avoidance
        Vector2 avoidanceDirection = CalculateAvoidanceDirection(velocity);
        velocity += avoidanceDirection * avoidanceForce;

        RigidBody.velocity = velocity * Speed;

        if (distanceToTarget <= 0.2f)
        {
            GetNewRoamTarget();
            MovementState = MovementStateEnum.IDLE;
            TimerToRoam = IntervalToRoam;
        }
    }

    private void GetNewRoamTarget()
    {
        Vector2 randomOffset = Random.insideUnitCircle * MaxDistanceToRoam;

        RoamTarget = (Vector2)transform.position + randomOffset;
    }

    private void LookAt(Vector3 target)
    {
        float deltaX = target.x - transform.position.x;

        float horizontal = Mathf.Abs(deltaX) == 0 ? 0 : deltaX / Mathf.Abs(deltaX);

        ChangeDirection(horizontal);
    }

    public void ChangeDirection(float horizontal)
    {
        if (horizontal == 0)
            return;

        if (horizontal == FacingDirection)
            return;

        FacingDirection = (int)horizontal;

        gameObject.transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y);
    }

    private Vector2 CalculateAvoidanceDirection(Vector2 currentVelocity)
    {
        Vector2 avoidanceDirection = Vector2.zero;

        // Check for obstacles in front of the baby
        RaycastHit2D hitForward = Physics2D.Raycast(transform.position, currentVelocity.normalized, obstacleDetectionRange, obstacleLayer);
        if (hitForward.collider != null)
        {
            Debug.DrawRay(transform.position, currentVelocity.normalized * obstacleDetectionRange, Color.red);
            // Obstacle detected, calculate avoidance direction
            Vector2 hitNormal = hitForward.normal;
            avoidanceDirection += hitNormal;
        }
        else
        {
            Debug.DrawRay(transform.position, currentVelocity.normalized * obstacleDetectionRange, Color.green);
        }

        // Check for obstacles to the left
        Vector2 leftDirection = Quaternion.Euler(0, 0, 45) * currentVelocity.normalized;
        RaycastHit2D hitLeft = Physics2D.Raycast(transform.position, leftDirection, obstacleDetectionRange, obstacleLayer);
        if (hitLeft.collider != null)
        {
            Debug.DrawRay(transform.position, leftDirection * obstacleDetectionRange, Color.red);
            avoidanceDirection += hitLeft.normal;
        }
        else
        {
            Debug.DrawRay(transform.position, leftDirection * obstacleDetectionRange, Color.green);
        }

        // Check for obstacles to the right
        Vector2 rightDirection = Quaternion.Euler(0, 0, -45) * currentVelocity.normalized;
        RaycastHit2D hitRight = Physics2D.Raycast(transform.position, rightDirection, obstacleDetectionRange, obstacleLayer);
        if (hitRight.collider != null)
        {
            Debug.DrawRay(transform.position, rightDirection * obstacleDetectionRange, Color.red);
            avoidanceDirection += hitRight.normal;
        }
        else
        {
            Debug.DrawRay(transform.position, rightDirection * obstacleDetectionRange, Color.green);
        }

        return avoidanceDirection.normalized;
    }
}
