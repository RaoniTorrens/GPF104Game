using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    [SerializeField]
    public float Speed = 5; // Changed to public

    // Private attributes
    private SpriteRenderer SpriteRenderer;
    private Rigidbody2D RigidBody;
    private int FacingDirection;

    
    void Awake()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();
        RigidBody = GetComponent<Rigidbody2D>();
    }

    void Start()
    {

    }

    void FixedUpdate()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        ChangeDirection(horizontal);

        var velocity = new Vector2(horizontal, vertical).normalized * Speed;

        RigidBody.velocity = velocity;
    }

    public void ChangeDirection(float horizontal) 
    {
        Debug.Log($"horizontal {horizontal}");
        if (horizontal == 0)
            return;

        if (horizontal == FacingDirection)
            return;

        FacingDirection = (int)horizontal;

        /*gameObject.transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y);*/

        SpriteRenderer.flipX = FacingDirection < 0;
    }
}
