using Mirror;
using UnityEngine;

[AddComponentMenu("Warlock/Actor/Life")]
public class ActorLife : NetworkBehaviour
{
    public float Health => health;
    public float MaxHealth => maxHealth;
    public bool IsDead => health <= 0;

    [SerializeField, SyncVar] private float health = 1f;
    [SerializeField, SyncVar] private float maxHealth = 100f;

    /// <summary>
    /// Simple tag system, storing the last instigator.
    /// </summary>
    private int lastInstigatorId = -1;
    private Actor actor = null;

    private void Awake()
    {
        actor = GetComponent<Actor>();
    }

    public override void OnStartServer()
    {
        health = maxHealth;
    }

    [Server]
    public void Damage(float value, int instigatorId = -1)
    {
        if (value < 0f)
        {
            Debug.LogWarning("Use Heal(value) to heal the player.");
            return;
        }

        health = Mathf.Max(0f, health - value);

        // We don't want to replace last instigator with null
        if (instigatorId >= 0)
        {
            lastInstigatorId = instigatorId;
        }

        if (health <= 0f)
        {
            Kill();
        }
    }

    [Server]
    public void Heal(float value, Actor instigator = null, bool resurrect = false)
    {
        if (value < 0f)
        {
            Debug.LogWarning("Use Damage(value) to damage the player.");
            return;
        }

        if (IsDead && !resurrect)
        {
            Debug.Log($"Target is dead, to allow resurrection, set resurrect to true.");
            return;
        }

        health = Mathf.Min(health + value, MaxHealth);
    }

    [Server]
    public void Kill()
    {
        // Ensure health is zero
        if (health != 0f)
            health = 0f;

        // TODO: Should the playermanager actually handle this?
        var playerManager = PlayerManager.Instance;
        if (playerManager != null)
        {
            playerManager.PlayerKilled(actor.OwnerId, lastInstigatorId);
        }

        // Reset instigator in case the player is respawned
        lastInstigatorId = -1;

        // Destroy actor, this could be death animation instead or whatever
        NetworkServer.Destroy(gameObject);
    }
}