using Mirror;
using UnityEngine;

[CreateAssetMenu(menuName = "Warlock/Abilities/Projectile")]
public class ProjectileAbility : ScriptableAbility
{
    [Header("Projectile")]
    [Tooltip("Spawned projectile.")]
    [SerializeField] private ProjectileController projectilePrefab = null;

    public override void Cast(Actor caster, Vector3 position)
    {
        var direction = Vector3.Normalize(position - caster.Aim.CastPosition);
        var controller = Instantiate(projectilePrefab, caster.Aim.CastPosition, Quaternion.LookRotation(direction));
        var finalPosition = caster.Aim.CastPosition + direction * Range;

        controller.Owner = caster.Owner;
        controller.Initialize(finalPosition);

        if (CastAudio != null)
            AudioManager.Instance.Server_PlayAt(CastAudio, caster.transform.position);

        NetworkServer.Spawn(controller.gameObject);
    }
}