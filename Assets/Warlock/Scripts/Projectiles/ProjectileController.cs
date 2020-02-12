using Mirror;
using UnityEngine;

[AddComponentMenu("Warlock/Projectile Controller")]
public class ProjectileController : NetworkBehaviour
{
    public Player Owner
    {
        get
        {
            if (PlayerManager.Instance == null || owner < 0)
                return null;

            return PlayerManager.Instance.GetPlayer(owner);
        }
        set
        {
            // Invalidate if null
            if (value == null)
            {
                owner = -1;
                return;
            }

            owner = value.LobbyIndex;
        }
    }
    [SyncVar] private int owner = -1;

    [Header("Properties")]
    [Tooltip("Speed at which the projectile moves.")]
    [SerializeField] private float speed = 3f;
    [Tooltip("Damage done by the projectile.")]
    [SerializeField] private float damage = 10f;
    [Tooltip("Knockback strength of the projectile.")]
    [SerializeField] private float strength = 15f;

    [Header("Audio")]
    [SerializeField] private AudioClip impactAudio = null;

    private Vector3 targetPosition = Vector3.zero;
    private Rigidbody body = null;
    private bool isDying = false;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// </summary>
    /// <param name="position">Target position, projectile will be destroyed after reaching here.</param>
    [Server]
    public void Initialize(Vector3 position)
    {
        targetPosition = position;
    }

    [ServerCallback]
    private void FixedUpdate()
    {
        var direction = Vector3.Normalize(targetPosition - transform.position);
        var distance = Vector3.Distance(targetPosition, transform.position);
        var movement = speed * Time.fixedDeltaTime;

        if (distance <= movement)
        {
            // We don't want to remove this right now
            // but we need to flag it, removal should happen after collision has had a chance to run
            isDying = true;
        }

        body.MovePosition(transform.position + direction * Mathf.Min(distance, movement));
    }

    [ServerCallback]
    private void Update()
    {
        if (isDying)
            Kill();
    }

    /// <summary>
    /// Handles collision exclusively on the server.
    /// </summary>
    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        var actor = other.GetComponent<Actor>();

        // Actor collision
        if (actor != null)
        {
            // Don't collide against owner
            if (actor.Owner == Owner)
                return;

            // Get direction from rotation and remove height
            var direction = transform.forward;
            direction.y = 0f;

            var force = direction * strength;

            // Add force to the rigid body
            if (actor.Movement != null)
            {
                actor.Movement.Knockback(force);
            }

            if (actor.Life != null)
            {
                // Deal damage to actor
                actor.Life.Damage(damage, owner);
            }

            if (impactAudio != null)
                AudioManager.Instance.Server_PlayAt(impactAudio, actor.transform.position);
        }

        Kill();
    }

    [Server]
    private void Kill()
    {
        // Destroy the projectile for all clients
        NetworkServer.Destroy(gameObject);
    }
}