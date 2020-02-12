using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Provides references to common actor components, doesn't ensure they exist.
/// </summary>
[AddComponentMenu("Warlock/Actor/Actor")]
public class Actor : NetworkBehaviour
{
    /// <summary>
    /// Global access to the local actor.
    /// </summary>
    public static Actor Local { get; private set; } = null;
    public static List<Actor> Actors { get; private set; } = new List<Actor>();

    public Player Owner
    {
        get
        {
            if (PlayerManager.Instance == null || ownerId < 0)
                return null;

            return PlayerManager.Instance.GetPlayer(ownerId);
        }
        set
        {
            // Invalidate if null
            if (value == null)
            {
                ownerId = -1;
                return;
            }

            ownerId = value.LobbyIndex;
        }
    }
    public int OwnerId => ownerId;
    [SyncVar(hook = "Hook_Owner")] private int ownerId = -1;
    
    public ActorMovement Movement { get; private set; } = null;
    public ActorCast Cast { get; private set; } = null;
    public ActorLife Life { get; private set; } = null;
    public ActorAim Aim { get; private set; } = null;
    public Animator Animator { get; private set; } = null;
    public AudioSource Audio { get; private set; } = null;

    public EventHandler OwnerChanged;

    private void Awake()
    {
        Movement = GetComponent<ActorMovement>();
        Cast = GetComponent<ActorCast>();
        Life = GetComponent<ActorLife>();
        Aim = GetComponent<ActorAim>();
        Animator = GetComponentInChildren<Animator>();
        Audio = GetComponent<AudioSource>();
    }

    public override void OnStartClient()
    {
        if (!Actors.Contains(this))
            Actors.Add(this);

        if (hasAuthority)
            Local = this;
    }

    public override void OnNetworkDestroy()
    {
        if (Actors.Contains(this))
            Actors.Remove(this);
    }

    private void Hook_Owner(int oldValue, int newValue)
    {
        OwnerChanged?.Invoke(this, null);

        // Ensure new id is valid
        if (newValue >= 0)
        {
            var player = Owner;
            
            if (player != null)
            {
                player.Actor = this;
            }
        }
    }
}