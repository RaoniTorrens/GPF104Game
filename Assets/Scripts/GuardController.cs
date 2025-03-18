using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class GuardController : MonoBehaviour
{
    [SerializeField]
    protected float Speed = 5;

    [Header("Patrol parameters")]
    [SerializeField, Tooltip("How long the guard looks in each direction")]
    protected float PatrolDirectionDuration = 3;
    [SerializeField, Tooltip("Every point on the map the guard should cycle through")]
    protected GameObject[] PatrolPoints;

    private Vector2[] PatrolPointsPositions;
    private int CurrentPatrolPointIndex;
    private Vector2 CurrentPatrolPoint;
    private float PatrolDirectionTimer;
    private float PatrolPositionTimer;


    [Header("Line of sight parameters")]
    [SerializeField, Tooltip("How far can the guard see")]
    protected float LineOfSightRange = 5;
    [SerializeField, Tooltip("How wide is the line of sight triangle")]
    protected float LineOfSightWidth = 1f;

    private DirectionEnum LineOfSightDirection;
    
    private Rigidbody2D RigidBody;
    private MovementStateEnum MovementState;
    private GameObject chaseTarget;

    void Awake()
    {
        RigidBody = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        SetPatrolPointsPositions();

        PatrolPositionTimer = PatrolDirectionDuration * 4;
        PatrolDirectionTimer = PatrolDirectionDuration;

        MovementState = MovementStateEnum.IDLE;
        LineOfSightDirection = DirectionEnum.RIGHT;
    }

    void FixedUpdate()
    {
        if (MovementState == MovementStateEnum.CHASE)
            Chase();

        if (MovementState != MovementStateEnum.CHASE)
            Patrol();

        CastLineOfSight();
    }

    private void SetPatrolPointsPositions() 
    {
        PatrolPointsPositions = new Vector2[PatrolPoints.Length + 1];
        PatrolPointsPositions[0] = transform.position;
        for (int i = 0; i < PatrolPoints.Length; i++)
        {
            PatrolPointsPositions[i + 1] = PatrolPoints[i].transform.position;
        }

        CurrentPatrolPointIndex = 0;
        CurrentPatrolPoint = PatrolPointsPositions[CurrentPatrolPointIndex];
    }

    private void Patrol()
    {
        if (MovementState == MovementStateEnum.IDLE)
        {
            if (PatrolPositionTimer > 0)
            {
                PatrolPositionTimer -= Time.deltaTime;
            }
            else
            {
                MovementState = MovementStateEnum.PATROL;
                PatrolPositionTimer = PatrolDirectionDuration * 4;
                SetNextPatrolPoint();
            }

            if (PatrolDirectionTimer > 0)
            {
                PatrolDirectionTimer -= Time.deltaTime;
            }
            else
            {
                SetNewPatrolDirection();
                PatrolDirectionTimer = PatrolDirectionDuration;
            }
        }

        float deltaX = CurrentPatrolPoint.x - transform.position.x;
        float deltaY = CurrentPatrolPoint.y - transform.position.y;

        float horizontal = Mathf.Abs(deltaX) <= 0.1f ? 0 : deltaX / Mathf.Abs(deltaX);
        float vertical = Mathf.Abs(deltaY) <= 0.1f ? 0 : deltaY / Mathf.Abs(deltaY);

        Vector2 velocity = new Vector2(0, 0);

        if (MovementState == MovementStateEnum.PATROL)
        {
            velocity = new Vector2(horizontal, vertical).normalized * Speed;

            if (IsInPatrolPosition(CurrentPatrolPoint)) 
                MovementState = MovementStateEnum.IDLE;
        }

        RigidBody.velocity = velocity;
    }

    private void CastLineOfSight()
    {
        // This method casts three Raycasts and if any hits an object on the "Player" layer the guard starts to chase

        Vector2 originPoint;
        Vector2 destinyVectorA;
        Vector2 destinyVectorB;
        Vector2 destinyVectorC;

        switch (LineOfSightDirection)
        {
            case DirectionEnum.RIGHT:
                originPoint = (Vector2)transform.position + Vector2.right;
                destinyVectorA = Vector2.right;
                destinyVectorB = destinyVectorA + (Vector2.down * LineOfSightWidth);
                destinyVectorC = destinyVectorA + (Vector2.up * LineOfSightWidth);
                break;
            case DirectionEnum.DOWN:
                originPoint = (Vector2)transform.position + Vector2.down;
                destinyVectorA = Vector2.down;
                destinyVectorB = destinyVectorA + (Vector2.right * LineOfSightWidth);
                destinyVectorC = destinyVectorA + (Vector2.left * LineOfSightWidth);
                break;
            case DirectionEnum.LEFT:
                originPoint = (Vector2)transform.position + Vector2.left;
                destinyVectorA = Vector2.left;
                destinyVectorB = destinyVectorA + (Vector2.down * LineOfSightWidth);
                destinyVectorC = destinyVectorA + (Vector2.up * LineOfSightWidth);
                break;
            case DirectionEnum.UP:
                originPoint = (Vector2)transform.position + Vector2.up;
                destinyVectorA = Vector2.up;
                destinyVectorB = destinyVectorA + (Vector2.right * LineOfSightWidth);
                destinyVectorC = destinyVectorA + (Vector2.left * LineOfSightWidth);
                break;
            default:
                originPoint = (Vector2)transform.position + Vector2.right;
                destinyVectorA = Vector2.right;
                destinyVectorB = destinyVectorA + (Vector2.down * LineOfSightWidth);
                destinyVectorC = destinyVectorA + (Vector2.up * LineOfSightWidth);
                break;
        }

        LayerMask layer = LayerMask.GetMask("Player");
        RaycastHit2D hitA = Physics2D.Raycast(originPoint, destinyVectorA, LineOfSightRange, layer);
        RaycastHit2D hitB = Physics2D.Raycast(originPoint, destinyVectorB, LineOfSightRange, layer);
        RaycastHit2D hitC = Physics2D.Raycast(originPoint, destinyVectorC, LineOfSightRange, layer);

        if (hitA.collider != null)
            Debug.DrawRay(originPoint, destinyVectorA * LineOfSightRange, Color.red);
        else
            Debug.DrawRay(originPoint, destinyVectorA * LineOfSightRange, Color.green);

        if (hitB.collider != null)
            Debug.DrawRay(originPoint, destinyVectorB * LineOfSightRange, Color.red);
        else
            Debug.DrawRay(originPoint, destinyVectorB * LineOfSightRange, Color.green);

        if (hitC.collider != null)
            Debug.DrawRay(originPoint, destinyVectorC * LineOfSightRange, Color.red);
        else
            Debug.DrawRay(originPoint, destinyVectorC * LineOfSightRange, Color.green);
        
        bool foundTarget = hitA.collider || hitB.collider || hitC.collider;

        if (foundTarget)
        {
            MovementState = MovementStateEnum.CHASE;
            chaseTarget = hitA.collider ? 
                hitA.collider.gameObject : 
                hitB.collider ? 
                    hitB.collider.gameObject : 
                    hitC ? 
                        hitC.collider.gameObject : 
                        null;
        }

    }

    private void Chase() 
    {
        float deltaX = chaseTarget.transform.position.x - transform.position.x;
        float deltaY = chaseTarget.transform.position.y - transform.position.y;

        float horizontal = Mathf.Abs(deltaX) <= 0.1f ? 0 : deltaX / Mathf.Abs(deltaX);
        float vertical = Mathf.Abs(deltaY) <= 0.1f ? 0 : deltaY / Mathf.Abs(deltaY);

        Vector2 velocity = new Vector2(horizontal, vertical).normalized * Speed;

        RigidBody.velocity = velocity;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject != chaseTarget)
            return;

        if (MovementState == MovementStateEnum.CHASE)
        {
            MovementState = MovementStateEnum.IDLE;
            Vector2 velocity = new Vector2(0, 0);
            RigidBody.velocity = velocity;
            GameManager.instance.Lose();
        }
    }

    private bool IsInPatrolPosition(Vector2 patrolPoint) {
        float acceptableRange = 0.1f;
        bool xInRange = transform.position.x >= patrolPoint.x - acceptableRange && transform.position.x <= patrolPoint.x + acceptableRange;
        bool yInRange = transform.position.y >= patrolPoint.y - acceptableRange && transform.position.y <= patrolPoint.y + acceptableRange;;
        
        return xInRange && yInRange;
    }

    private void SetNextPatrolPoint()
    {
        CurrentPatrolPointIndex++;

        if (CurrentPatrolPointIndex == PatrolPointsPositions.Length)
            CurrentPatrolPointIndex = 0;

        CurrentPatrolPoint = PatrolPointsPositions[CurrentPatrolPointIndex];
        SetNewPatrolDirection(transform.position, CurrentPatrolPoint);
    }

    private void SetNewPatrolDirection()
    {
        switch (LineOfSightDirection)
        {
            case DirectionEnum.RIGHT:
                LineOfSightDirection = DirectionEnum.DOWN;
                return;
            case DirectionEnum.DOWN:
                LineOfSightDirection = DirectionEnum.LEFT;
                return;
            case DirectionEnum.LEFT:
                LineOfSightDirection = DirectionEnum.UP;
                return;
            case DirectionEnum.UP:
                LineOfSightDirection = DirectionEnum.RIGHT;
                return;
            default:
                LineOfSightDirection = DirectionEnum.RIGHT;
                return;
        }
    }

    private void SetNewPatrolDirection(Vector2 currentPosition, Vector2 targetPosition)
    {
        Vector2 direction = (targetPosition - currentPosition).normalized;

        if (direction.y > 0)
            LineOfSightDirection = DirectionEnum.UP;
        else
            LineOfSightDirection = DirectionEnum.DOWN;

        if (direction.x > 0)
            LineOfSightDirection = DirectionEnum.RIGHT;
        else if (direction.x < 0)
            LineOfSightDirection = DirectionEnum.LEFT;
    }
}
