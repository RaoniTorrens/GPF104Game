using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class AnimatorController : MonoBehaviour
{
    private Animator Animator { get; set; }
    private Rigidbody2D RigidBody;

    void Awake()
    {
        RigidBody = GetComponent<Rigidbody2D>();
        Animator = GetComponent<Animator>();
    }

    void Start()
    {
        
    }

    void FixedUpdate()
    {
        float velocityX = RigidBody.velocity.x;
        float velocityY = RigidBody.velocity.y;

        float horizontal = Mathf.Abs(velocityX);
        float vertical = Mathf.Abs(velocityY);

        Animator.SetFloat("horizontal", horizontal);
        Animator.SetFloat("vertical", vertical);
    }
}
