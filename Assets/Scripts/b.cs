using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BabyControllerrr : MonoBehaviour
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

        var velocity = new Vector2(horizontal, vertical).normalized * Speed;

        LookAt(RoamTarget);

        RigidBody.velocity = velocity;       

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

    private bool DetectObstacles()
    {
        float detectionRange = 3f;
        LayerMask obstacleLayer = LayerMask.GetMask("Walls");

        Vector2 moveDirection = transform.position;

        if (MovementState == MovementStateEnum.FOLLOW)
            moveDirection = PlayerObject.transform.position;
        else if (MovementState == MovementStateEnum.ROAM)
            moveDirection = RoamTarget;

        moveDirection -= (Vector2)transform.position;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, moveDirection, detectionRange, obstacleLayer);
        
        if (hit.collider != null) // If an obstacle is detected
        {
            Debug.DrawRay(transform.position, moveDirection * detectionRange, Color.red);
            return true;
        }
        else 
        {
            Debug.DrawRay(transform.position, moveDirection * detectionRange, Color.green);
            return false;
        }
    }
}
