using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Static data-object for abilities. <see cref="Ability"/> contains the dynamic functionality.
/// <para>
/// We can't pass this through Mirror, and we can't give the user authority on the properties.
/// It has to be recognized over the network by some means, and has to run on the server.
/// </para>
/// </summary>
public abstract class ScriptableAbility : ScriptableObject
{
    /// <summary>
    /// Name of the ability.
    /// </summary>
    public string Name => name;

    [Header("Properties")]
    [Tooltip("Sprite to be displayed on the UI.")]
    public Sprite Icon;
    [Tooltip("Time before the ability can be re-cast in seconds.")]
    public float Cooldown = 1f;
    [Tooltip("Time the ability takes to cast in seconds.\nEffectively the amount of time the player is locked in place.")]
    public float ChannelTime = 1f;
    [Tooltip("Range of the ability in units.")]
    public float Range = 1f;

    [Header("Audio")]
    [Tooltip("Audio played when the ability is cast.")]
    public AudioClip CastAudio;

    /// <summary>
    /// Determines whether the ability actually can be cast.
    /// </summary>
    public virtual bool CanCast(Actor caster)
    {
        return true;
    }

    /// <summary>
    /// Casts the ability, this is where the ability specific cast-logic goes.
    /// <para>Should only be called on the server.</para>
    /// </summary>
    public abstract void Cast(Actor caster, Vector3 position);

    /// <summary>
    /// Called when the ability starts casting.
    /// <para>Used to perform client-side effects, and should therefore only be called on client.</para>
    /// </summary>
    public virtual void OnChannelBegin(Actor caster)
    {
    }

    /// <summary>
    /// Called when the ability has finished casting.
    /// <para>Used to perform client-side effects, and should therefore only be called on client.</para>
    /// </summary>
    /// <remarks>
    /// Called <see cref="ChannelTime"/> seconds after <see cref="OnChannelBegin(Warlock)"/>.
    /// </remarks>
    public virtual void OnChannelEnd(Actor caster)
    {
    }

    /// <summary>
    /// Caches all abilities in <see cref="Resources"/> on first access attempt.
    /// <see cref="Resources.LoadAll{T}(string)"/> must be called from the main thread, hence why we initialize it on first get.
    /// <para>Key is <see cref="GetNetworkHash"/>.</para>
    /// </summary>
    public static Dictionary<int, ScriptableAbility> Cache
    {
        get
        {
            if (cache == null)
            {
                // Load all abilities in resources
                var abilities = Resources.LoadAll<ScriptableAbility>("");

                // Create dictionary from the loaded resources
                // Key is the hash of the name and the value is the ability
                cache = abilities.ToDictionary(x => x.name.GetStableHashCode(), y => y);

                Debug.Log($"Ability Cache: {string.Join(", ", cache.Keys.ToArray())}");
            }

            return cache;
        }
    }
    private static Dictionary<int, ScriptableAbility> cache;
}