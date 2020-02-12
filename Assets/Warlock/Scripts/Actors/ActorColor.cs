using Mirror;
using UnityEngine;

// TODO: This is hot garbage and should be replaced asap
[AddComponentMenu("Warlock/Actor/Color")]
public class ActorColor : NetworkBehaviour
{
    private Actor actor = null;

    private void Awake()
    {
        actor = GetComponent<Actor>();

        actor.OwnerChanged += (s, e) =>
        {
            UpdateColor();
        };
    }

    public override void OnStartClient() => UpdateColor();

    private void UpdateColor()
    {
        if (actor == null)
            return;

        var owner = actor.Owner;

        if (owner == null)
            return;

        var renderer = GetComponentInChildren<SkinnedMeshRenderer>();

        if (renderer != null && renderer.materials != null && renderer.materials.Length >= 2 && renderer.materials[1] != null)
            renderer.materials[1].SetColor("_BaseColor", owner.Color);
    }
}