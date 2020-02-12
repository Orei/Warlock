using Mirror;
using System.Collections;
using UnityEngine;

/// <summary>
/// Provides casting functionality.
/// </summary>
[AddComponentMenu("Warlock/Actor/Cast")]
public class ActorCast : NetworkBehaviour
{
    public bool IsCasting => activeAbility >= 0;

    public ScriptableAbility[] Templates => templates;
    public SyncListAbility Abilities { get; } = new SyncListAbility();

    [SerializeField] private ScriptableAbility[] templates = null;

    [SyncVar(hook = "Hook_ActiveAbility")] private int activeAbility = -1;
    private Actor actor;

    #region Shared

    private void Awake()
    {
        actor = GetComponent<Actor>();
    }

    #endregion

    #region Server

    /// <summary>
    /// Loads all abilities from templates on the server.
    /// Automatically synced to clients.
    /// </summary>
    public override void OnStartServer()
    {
        foreach (var data in Templates)
            Abilities.Add(new Ability(data));
    }

    /// <summary>
    /// Starts casting an ability.
    /// </summary>
    [Server]
    private void Server_CastBegin(Ability ability, Vector3 position)
    {
        // Set cast end time to now + cast time
        // cooldown is applied after the ability has finished
        ability.CastTimeEnd = NetworkTime.time + ability.CastTime;

        // We have updated values on a struct, so we need to replace it
        Abilities[activeAbility] = ability;

        // TODO: Figure out where this should be
        ability.Cast(actor, position);

        // Call client-side effects
        Rpc_CastBegin(activeAbility, position);

        StartCoroutine(CastCoroutine(ability, position));
    }

    /// <summary>
    /// Ends casting an ability.
    /// </summary>
    [Server]
    private void Server_CastEnd(Ability ability, Vector3 position)
    {
        // Set cooldown end to now + cooldown time
        ability.CooldownEnd = NetworkTime.time + ability.Cooldown;

        // Struct, we need to replace with new state
        Abilities[activeAbility] = ability;

        // Call client-side effects
        Rpc_CastEnd(activeAbility);

        // Invalidate active ability
        activeAbility = -1;
    }

    // HACK: fast way to do this, but stupid
    /// <summary>
    /// Server coroutine ran by <see cref="Server_CastBegin(Ability)"/>, triggers <see cref="Server_CastEnd(Ability)"/> when it ends.
    /// </summary>
    private IEnumerator CastCoroutine(Ability ability, Vector3 position)
    {
        while (ability.IsCasting)
            yield return new WaitForEndOfFrame();

        Server_CastEnd(ability, position);
    }

    #endregion

    #region Client

    [ClientCallback]
    private void Update()
    {
        if (!hasAuthority || IsCasting)
            return;

        var aimPosition = actor.Aim.Position;

        if (Input.GetButtonDown("Primary") && Abilities.Count > 0)
            Client_TryCast(0, aimPosition);

        if (Input.GetButtonDown("Secondary") && Abilities.Count > 1)
            Client_TryCast(1, aimPosition);

        if (Input.GetButtonDown("Movement") && Abilities.Count > 2)
            Client_TryCast(2, aimPosition);
    }

    /// <summary>
    /// Attempts to cast an ability.
    /// The server has authority on whether it happens.
    /// </summary>
    /// <param name="abilityIndex">Index of the ability in <see cref="Abilities"/>.</param>
    [Client]
    private void Client_TryCast(int abilityIndex, Vector3 position)
    {
        // Ability doesn't exist
        if (abilityIndex >= Abilities.Count || abilityIndex < 0)
            return;

        var ability = Abilities[abilityIndex];

        // Verify we can cast this ability
        if (!ability.CanCast(actor))
            return;

        // Ask the server to cast, it still has authority on whether it happens though
        Cmd_Cast(abilityIndex, position);
    }

    /// <summary>
    /// Hooks <see cref="activeAbility"/>, triggers cast animation on clients when casting.
    /// </summary>
    [Client]
    public void Hook_ActiveAbility(int oldValue, int newValue)
    {
        if (newValue == -1)
            return;

        // Force speed to zero in the animator, makes sure the player isn't walking
        actor.Animator.SetFloat("Speed", 0f);
        actor.Animator.SetTrigger("Cast");
    }
    
    #endregion

    #region RPC

    /// <summary>
    /// Tell the server that we are casting an ability.
    /// Verify that the user can actually cast here, make sure the skill is not on cooldown etc.
    /// </summary>
    /// <param name="abilityIndex">Index of the ability in <see cref="Abilities"/>.</param>
    [Command]
    private void Cmd_Cast(int abilityIndex, Vector3 position)
    {
        var ability = Abilities[abilityIndex];

        if (!ability.CanCast(actor))
            return;

        activeAbility = abilityIndex;

        Server_CastBegin(ability, position);
    }
 
    /// <summary>
    /// Tell all clients to call client-side begin effects.
    /// </summary>
    [ClientRpc]
    private void Rpc_CastBegin(int abilityIndex, Vector3 position)
    {
        var ability = Abilities[abilityIndex];
        ability.Data.OnChannelBegin(actor);

        // If we're have authority of the casting actor
        if (hasAuthority)
        {
            // Rotate towards target position, the server can't do this for us
            // and since it's just a visual cue, it's fine if it isn't secure
            transform.rotation = Quaternion.LookRotation(actor.Aim.Direction);
        }
    }

    /// <summary>
    /// Tell all clients to call client-side end effects.
    /// </summary>
    [ClientRpc]
    private void Rpc_CastEnd(int abilityIndex)
    {
        var ability = Abilities[abilityIndex];
        ability.Data.OnChannelEnd(actor);
    }
    
    #endregion
}