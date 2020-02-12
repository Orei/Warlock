using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[AddComponentMenu("Warlock/Managers/Audio")]
public class AudioManager : NetworkBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Tooltip("Number of audio sources to create.")]
    [SerializeField] private int numSources = 12;
    private AudioSource[] sources = null;

    private void Awake()
    {
        Debug.Assert(Instance == null, $"An instance of {nameof(AudioManager)} has already been created.");
        Instance = this;

        sources = new AudioSource[numSources];
        for (var i = 0; i < sources.Length; i++)
        {
            var obj = new GameObject($"Audio Source ({i + 1})");
            obj.transform.SetParent(transform, true);
            obj.SetActive(false);

            var source = obj.AddComponent<AudioSource>();
            source.playOnAwake = false;

            sources[i] = source;
        }
    }

    private void Update()
    {
        for (var i = 0; i < sources.Length; i++)
        {
            var source = sources[i];

            if (!source.gameObject.activeSelf || source.isPlaying)
                continue;

            // Disable the object, indicating it's not being used
            source.gameObject.SetActive(false);
        }
    }

    [Server]
    public void Server_Play(AudioClip clip)
    {
        Rpc_Play(clip.name.GetStableHashCode());
    }

    [Server]
    public void Server_PlayAt(AudioClip clip, Vector3 position)
    {
        Rpc_PlayAt(clip.name.GetStableHashCode(), position);
    }

    [ClientRpc]
    public void Rpc_Play(int hash)
    {
        var clip = Cache[hash];

        Client_Play(clip);
    }

    [ClientRpc]
    public void Rpc_PlayAt(int hash, Vector3 position)
    {
        var clip = Cache[hash];

        Client_PlayAt(clip, position);
    }

    [Client]
    public void Client_Play(AudioClip clip)
    {
        var source = GetAvailableSource();
        source.gameObject.SetActive(true);
        source.spatialBlend = 0f;
        source.PlayOneShot(clip);
    }

    [Client]
    public void Client_PlayAt(AudioClip clip, Vector3 position)
    {
        var source = GetAvailableSource();
        source.gameObject.SetActive(true);
        source.transform.position = position;
        source.spatialBlend = 1f;
        source.PlayOneShot(clip);
    }

    private AudioSource GetAvailableSource()
    {
        for (var i = 0; i < sources.Length; i++)
        {
            var source = sources[i];

            if (!source.gameObject.activeSelf)
                return source;
        }

        return null;
    }

    public static Dictionary<int, AudioClip> Cache
    {
        get
        {
            if (cache == null)
            {
                // Load all audio clips from resources
                var abilities = Resources.LoadAll<AudioClip>("");

                // Create dictionary from the loaded resources
                // Key is the hash of the name and the value is the audio clip
                cache = abilities.ToDictionary(x => x.name.GetStableHashCode(), y => y);

                Debug.Log($"Audio Cache: {string.Join(", ", cache.Keys.ToArray())}");
            }

            return cache;
        }
    }
    private static Dictionary<int, AudioClip> cache;
}