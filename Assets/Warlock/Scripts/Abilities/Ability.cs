using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

/// <summary>
/// Allows abilities to be synced over network.
/// </summary>
public class SyncListAbility : SyncList<Ability> { }

/// <summary>
/// Contains the dynamic properties of a <see cref="ScriptableAbility"/>.
/// </summary>
[Serializable]
public struct Ability
{
    /// <summary>
    /// Wrapper for getting the related <see cref="ScriptableAbility"/> through caching.
    /// </summary>
    /// <exception cref="KeyNotFoundException"/>
    public ScriptableAbility Data
    {
        get
        {
            if (!ScriptableAbility.Cache.ContainsKey(NetworkHash))
                throw new KeyNotFoundException($"Unable to find {NetworkHash} in ability cache.\n" +
                    $"Ensure abilities are inside the Resources folder.");

            return ScriptableAbility.Cache[NetworkHash];
        }
    }

    /// <summary>
    /// Used to reference the <see cref="ScriptableAbility"/>, computed by the name of the ability.
    /// </summary>
    public int NetworkHash;

    /// <summary>
    /// When the cooldown ends in <see cref="NetworkTime"/>.
    /// <para>On cast, set to <see cref="NetworkTime.time"/> + <see cref="Cooldown"/>.</para>
    /// </summary>
    public double CooldownEnd;

    /// <summary>
    /// When the cast time ends in <see cref="NetworkTime"/>.
    /// <para>On cast, set to <see cref="NetworkTime.time"/> + <see cref="CastTime"/>.</para>
    /// </summary>
    public double CastTimeEnd;

    public string Name => Data.Name;
    public Sprite Icon => Data.Icon;
    public float Cooldown => Data.Cooldown;
    public float CastTime => Data.ChannelTime;
    public float CastRange => Data.Range;

    /// <summary>
    /// Whether the ability is currently on cooldown.
    /// </summary>
    public bool IsCooldown => CooldownLeft > 0f;

    /// <summary>
    /// Whether the ability is currently being cast.
    /// </summary>
    public bool IsCasting => CastTimeLeft > 0f;

    /// <summary>
    /// Seconds until the cooldown ends.
    /// </summary>
    public float CooldownLeft => Mathf.Max(0f, (float)(CooldownEnd - NetworkTime.time));

    /// <summary>
    /// Seconds until the cast time ends.
    /// </summary>
    public float CastTimeLeft => Mathf.Max(0f, (float)(CastTimeEnd - NetworkTime.time));

    public Ability(ScriptableAbility data)
    {
        // Get the network hash related to the ability data
        NetworkHash = data.name.GetStableHashCode();

        // Initially ready to cast
        CooldownEnd = CastTimeEnd = NetworkTime.time;
    }

    public bool CanCast(Actor caster)
    {
        return Data.CanCast(caster) && !IsCooldown && !IsCasting;
    }

    public void Cast(Actor caster, Vector3 position)
    {
        Data.Cast(caster, position);
    }
}