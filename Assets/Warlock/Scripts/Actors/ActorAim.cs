using Mirror;
using UnityEngine;

/// <summary>
/// Provides mouse-aiming functionality.
/// </summary>
[AddComponentMenu("Warlock/Actor/Aim")]
public class ActorAim : NetworkBehaviour
{
    /// <summary>
    /// Position from which projectiles are launched.
    /// <para>Defaults to <see cref="transform.position"/> if <see cref="castTransform"/> is null.</para>
    /// </summary>
    public Vector3 CastPosition
    {
        get => castTransform == null ? transform.position : castTransform.position;
    }
    
    /// <summary>
    /// Offset of <see cref="CastPosition"/> relative to <see cref="transform.position"/>.
    /// </summary>
    public Vector3 CastOffset
    {
        get => CastPosition - transform.position;
    }

    /// <summary>
    /// Current aim position.
    /// </summary>
    public Vector3 Position { get; private set; } = Vector3.zero;

    /// <summary>
    /// Current aim direction.
    /// </summary>
    public Vector3 Direction { get; private set; } = Vector3.zero;

    [Tooltip("Position where projectiles spawn when casting.")]
    [SerializeField] private Transform castTransform = null;
    [Tooltip("Transform of the aiming reticle.")]
    [SerializeField] private Transform reticleTransform = null;
    private Actor actor = null;

    #region Shared
    
    private void Awake()
    {
        actor = GetComponent<Actor>();
    }
    
    #endregion

    #region Client

    public override void OnStartClient()
    {
        if (hasAuthority)
            return;

        // Disable this component and reticle
        reticleTransform.gameObject.SetActive(false);
        enabled = false;
    }

    [ClientCallback]
    private void Update()
    {
        Position = GetAimPosition();
        Direction = Vector3.Normalize(Position - CastPosition);
    }

    [ClientCallback]
    private void LateUpdate()
    {
        if (reticleTransform == null)
            return;

        // If we're casting, we want to aim towards the cast position
        // indicating we're "busy"
        var isCasting = actor.Cast != null && actor.Cast.IsCasting;
        var direction = isCasting ? transform.forward : Direction;

        reticleTransform.rotation = Quaternion.LookRotation(direction);
    }

    /// <summary>
    /// Returns the current aim position.
    /// </summary>
    [Client]
    private Vector3 GetAimPosition()
    {
        // Create a plane at cast position facing upwards
        var plane = new Plane(Vector3.up, CastPosition);
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // Raycast from the plane
        if (plane.Raycast(ray, out float enter))
        {
            // Get the intersection point
            return ray.GetPoint(enter);
        }

        // Shouldn't be able to fail, but just incase
        return Vector3.zero;
    }

    #endregion
}