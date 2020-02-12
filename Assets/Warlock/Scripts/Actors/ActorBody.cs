using UnityEngine;


[AddComponentMenu("Warlock/Actor/Actor")]
public class ActorBody : MonoBehaviour
{
    public Vector3 Acceleration
    {
        get => acceleration;
        set => acceleration = value;
    }

    [SerializeField] private float gravity = 9.81f;
    [SerializeField, Range(0f, 1f)] private float groundFriction = 0.96f;
    [SerializeField, Range(0f, 1f)] private float airFriction = 0.99f;
    [SerializeField] private LayerMask groundMask;

    private Vector3 acceleration = Vector3.zero;
    private Vector3 velocity = Vector3.zero;
    [SerializeField] private bool isGrounded = false;
    private Rigidbody body = null;
    private new CapsuleCollider collider = null;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        collider = GetComponent<CapsuleCollider>();
    }

    private void FixedUpdate()
    {
        var position = transform.position;

        velocity += acceleration;
        velocity.y -= gravity;

        isGrounded = Physics.Raycast(position, Vector3.down, out RaycastHit hit, velocity.y - 0.01f, groundMask);

        if (isGrounded)
        {
            position.y = hit.point.y;
        }

        //var position = transform.position;

            //velocity += acceleration;
            //velocity.y -= gravity;
            //position += velocity;

            //if (!Physics.Raycast(vector + Vector3.up, Vector3.down, out RaycastHit hit, 1f, groundMask))
            //    return;

            //var num = vector.y - hit.distance + 1f;
            //isGrounded = (position.y < num + collider.height);

            //if (isGrounded)
            //{
            //    velocity.x *= groundFriction;
            //    velocity.y = Mathf.Max(0f, velocity.y);
            //    velocity.z *= groundFriction;

            //    position.y = num + collider.height;
            //}
            //else
            //{
            //    velocity.x *= airFriction;
            //    velocity.z *= airFriction;
            //}

            //vector.y = position.y;
            //body.MovePosition(vector);
    }

    public void AddForce(Vector3 direction, float strength) => AddForce(direction.normalized * strength);
    public void AddForce(Vector3 velocity) => this.velocity += velocity;
}