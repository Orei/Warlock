using Mirror;
using System;
using UnityEngine;

[Serializable]
public struct PlayerDefault
{
    public string Name;
    public Color Color;

    public PlayerDefault(string name, Color color)
    {
        Name = name;
        Color = color;
    }
}

[AddComponentMenu("Warlock/Managers/Player")]
public class PlayerManager : NetworkBehaviour
{
    public static PlayerManager Instance { get; private set; } = null;
    public Player[] Players { get; private set; } = null;

    [Header("Setup")]
    [SerializeField] private PlayerDefault[] defaults =
    {
        new PlayerDefault("Red", Color.red),
        new PlayerDefault("Blue", Color.blue),
        new PlayerDefault("Green", Color.green),
        new PlayerDefault("Yellow", Color.yellow),
        new PlayerDefault("Cyan", Color.cyan),
        new PlayerDefault("Grey", Color.grey),
    };
    [SyncVar] private int maxPlayers = 0;

    [Server]
    public override void OnStartServer()
    {
        // Sets max players on server, which will be synced to clients
        maxPlayers = NetworkManager.singleton.maxConnections;

        // Slots for each connection we support
        Players = new Player[maxPlayers];
    }

    [Client]
    public override void OnStartClient()
    {
        Players = new Player[maxPlayers];
    }

    private void Awake()
    {
        Debug.Assert(Instance == null, $"An instance of {nameof(AudioManager)} has already been created.");
        Instance = this;
    }

    [Server]
    public void Register(NetworkConnection connection, Player player)
    {
        var index = GetEmptyIndex();

        if (index < 0)
        {
            Debug.LogError($"Not enough slots to support another connection, client disconnected.");
            connection.Disconnect();
            return;
        }

        var defaults = GetDefaults(index);

        player.ConnectionId = connection.connectionId;
        player.LobbyIndex = index;
        player.Name = defaults.Name;
        player.Color = defaults.Color;
        player.Score = 0;
        player.IsReady = false;

        Players[index] = player;

        Debug.Log($"Player {player.Name} registered at {index}.");
    }

    [Server]
    public void Unregister(NetworkConnection connection)
    {
        var player = connection.identity.GetComponent<Player>();
        var index = player.LobbyIndex;
        
        if (player.LobbyIndex < 0)
        {
            Debug.LogWarning($"Unable to find player object for connection {connection.connectionId}.");
            return;
        }

        var name = Players[index].Name;
        Players[index] = null;

        Debug.Log($"Player {name} unregistered from {index}.");
    }

    [Server]
    public void PlayerKilled(int victimId, int instigatorId)
    {
        // Reward instigator for victim death
        if (instigatorId != -1)
        {
            var player = GetPlayer(instigatorId);

            if (player != null)
                player.Score++;
        }
    }

    [Client]
    public void Register(Player player)
    {
        if (player.LobbyIndex < 0)
            return;

        Players[player.LobbyIndex] = player;
    }

    [Client]
    public void Unregister(Player player)
    {
        if (player.LobbyIndex < 0)
            return;

        Players[player.LobbyIndex] = null;
    }
    
    public void Clear()
    {
        for (var i = 0; i < Players.Length; i++)
            Players[i] = null;
    }

    public Player GetPlayer(int lobbyIndex)
    {
        if (lobbyIndex < 0 || lobbyIndex > Players.Length)
            return null;

        return Players[lobbyIndex];
    }

    private int GetEmptyIndex()
    {
        for (var i = 0; i < Players.Length; i++)
            if (Players[i] == null)
                return i;
        
        return -1;
    }

    private PlayerDefault GetDefaults(int index)
    {
        if (index < 0 || index > defaults.Length)
            return new PlayerDefault("Undefined", Color.white);

        return defaults[index];
    }
}