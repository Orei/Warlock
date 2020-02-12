using Mirror;
using System.Collections.Generic;
using UnityEngine;

public struct LavaVictim
{
    public ActorLife Life;
    public double TimeStamp;
}

[AddComponentMenu("Warlock/Lava")]
public class Lava : NetworkBehaviour
{
    [Tooltip("Initial height.")]
    [SerializeField] private float defaultHeight = 0f;
    [Tooltip("Final height.")]
    [SerializeField] private float maxHeight = 1f;
    [Tooltip("Seconds until the lava reaches it's final height.")]
    [SerializeField] private float raiseTime = 30f;
    [Tooltip("Damage dealt per tick.")]
    [SerializeField] private float damage = 5f;
    [Tooltip("Seconds between damage tick.")]
    [SerializeField] private float damageInterval = 1f;
    [Tooltip("Audio played when damage is dealt.")]
    [SerializeField] private AudioClip burnAudio = null;
    private bool isMoving = false;

    /// <summary>
    /// Timestamp set when <see cref="Enable"/> is called, used to track progress.
    /// </summary>
    private double startTime = 0f;

    /// <summary>
    /// A list of all actors currently in the lava.
    /// </summary>
    private List<LavaVictim> victims;
    
    /// <summary>
    /// Normalized value, progress between <see cref="startTime"/> and end time.
    /// </summary>
    [SyncVar(hook = "Hook_CurrentProgress")] private float currentProgress = 0f;

    public override void OnStartServer()
    {
        startTime = NetworkTime.time;
        victims = new List<LavaVictim>();
    }

    public void Enable()
    {
        isMoving = true;
        startTime = NetworkTime.time;
    }

    public void Disable()
    {
        isMoving = false;
        currentProgress = 0f;
    }

    [ServerCallback]
    private void Update()
    {
        if (!isMoving)
            return;

        currentProgress = Mathf.Clamp01((float)(NetworkTime.time - startTime) / raiseTime);

        for (var i = 0; i < victims.Count; i++)
        {
            var victim = victims[i];

            if (victim.Life == null || victim.Life.IsDead)
            {
                victims.RemoveAt(i);
                i--;
                continue;
            }

            if ((NetworkTime.time - victim.TimeStamp) >= damageInterval)
            {
                victim.Life.Damage(damage);
                victim.TimeStamp = NetworkTime.time;

                AudioManager.Instance.Server_PlayAt(burnAudio, victim.Life.transform.position);

                victims[i] = victim;
            }
        }
    }

    private void Hook_CurrentProgress(float oldValue, float newValue)
    {
        transform.position = Vector3.up * (defaultHeight + currentProgress * maxHeight);
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        var actor = other.GetComponent<Actor>();

        if (actor == null || actor.Life == null)
            return;

        victims.Add(new LavaVictim()
        {
            Life = actor.Life,
            TimeStamp = NetworkTime.time
        });

        Debug.Log($"Lava victim added: {actor.name}.");
    }

    [ServerCallback]
    private void OnTriggerExit(Collider other)
    {
        var actor = other.GetComponent<Actor>();

        if (actor == null || actor.Life == null)
            return;

        var index = victims.FindIndex(x => x.Life == actor.Life);
        
        if (index >= 0)
        {
            victims.RemoveAt(index);

            Debug.Log($"Lava victim removed: {actor.name}.");
        }
    }
}