using Mirror;
using UnityEngine;

/// <summary>
/// Provides movement functionality.
/// </summary>
[RequireComponent(typeof(Rigidbody), typeof(Actor))]
[AddComponentMenu("Warlock/Actor/Movement")]
public class ActorMovement : NetworkBehaviour
{
    [Tooltip("Speed at which the actor moves.")]
    [SerializeField] private float movementSpeed = 3f;
    [Tooltip("Speed at which the actor rotates.")]
    [SerializeField] private float rotationSpeed = 10f;
    [Tooltip("Ground layer mask, decides what layer is considered ground.")]
    [SerializeField] private LayerMask groundMask = 0;
    private Vector3 movement = Vector3.zero;

    private Rigidbody body = null;
    private new CapsuleCollider collider = null;
    private Actor actor = null;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        actor = GetComponent<Actor>();
        collider = GetComponent<CapsuleCollider>();
    }

    private void Update()
    {
        if (!hasAuthority || !CanWalk())
            return;

        movement = new Vector3(Input.GetAxisRaw("Horizontal"), 0f,
            Input.GetAxisRaw("Vertical"));

        if (movement.magnitude > 1f)
            movement.Normalize();
    }

    private void FixedUpdate()
    {
        if (!hasAuthority)
            return;

        var magnitude = movement.magnitude;

        if (!CanWalk() || Mathf.Approximately(magnitude, 0f))
        {
            if (actor != null && actor.Animator != null)
                actor.Animator.SetFloat("Speed", 0f);
            return;
        }
        
        if (actor != null && actor.Animator != null)
            actor.Animator.SetFloat("Speed", magnitude);

        transform.forward = Vector3.Slerp(transform.forward, movement, rotationSpeed * Time.fixedDeltaTime);
        body.MovePosition(transform.position + movement * movementSpeed * Time.fixedDeltaTime);
    }

    private bool CanWalk()
    {
        if (actor.Cast != null && actor.Cast.IsCasting)
            return false;

        return true;
    }

    public void Warp(Vector3 position, bool forceGround = false)
    {
        var connection = netIdentity.connectionToClient;
        if (isServer && !hasAuthority && connection != null)
        {
            TargetRpc_Warp(connection, position, forceGround);
            return;
        }

        if (forceGround && collider != null)
        {
            var top = position + collider.center + collider.height * Vector3.up;

            if (Physics.Raycast(top, Vector3.down, out RaycastHit hit, 1000f))
                position = hit.point;
        }

        transform.position = position;
    }

    [TargetRpc]
    private void TargetRpc_Warp(NetworkConnection connection, Vector3 position, bool forceGround) => Warp(position, forceGround);

    public void Knockback(Vector3 force)
    {
        var connection = netIdentity.connectionToClient;
        if (isServer && !hasAuthority && connection != null)
        {
            TargetRpc_Knockback(connection, force);
            return;
        }

        body.AddForce(force, ForceMode.Impulse);
    }

    [TargetRpc]
    private void TargetRpc_Knockback(NetworkConnection connection, Vector3 force) => Knockback(force);
}