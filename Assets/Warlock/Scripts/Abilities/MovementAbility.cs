using UnityEngine;

[CreateAssetMenu(menuName = "Warlock/Abilities/Movement")]
public class MovementAbility : ScriptableAbility
{
    public override void Cast(Actor caster, Vector3 position)
    {
        if (caster == null || caster.Movement == null || caster.Aim == null)
        {
            Debug.LogWarning($"Unable to warp, caster, movement or aim is null.");
            return;
        }

        // Remove the offset from the aim position, this should make the teleport
        // happen from the transform.position, rather than the CastPosition
        // this is important because when teleporting, we want the
        // feet of the player to land on aim position, not the center
        var aimPosition = position - caster.Aim.CastOffset;
        var casterPosition = caster.transform.position;

        var direction = Vector3.Normalize(aimPosition - casterPosition);
        var distance = Vector3.Distance(aimPosition, casterPosition);

        // Teleport towards direction by distance or max range of the ability
        var warpPosition = casterPosition + direction * Mathf.Min(distance, Range);

        if (CastAudio != null)
            AudioManager.Instance.Server_PlayAt(CastAudio, caster.transform.position);

        // Perform the warp!
        caster.Movement.Warp(warpPosition, true);
    }
}