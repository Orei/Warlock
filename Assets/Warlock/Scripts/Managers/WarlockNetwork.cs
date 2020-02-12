using Mirror;
using UnityEngine;

public class ChangeReadyMessage : MessageBase { }

[AddComponentMenu("Warlock/Managers/Network")]
public class WarlockNetwork : NetworkManager
{
    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        // Copied from base.OnServerAddPlayer, needed to inject code between this and AddPlayer
        var startPos = GetStartPosition();
        var player = startPos != null
            ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
            : Instantiate(playerPrefab);

        var playerComponent = player.GetComponent<Player>();

        if (playerComponent != null)
        {
            // We have to call this before AddPlayerForConnection
            // Otherwise player values will not be initialized, and we'll have annoying issues
            PlayerManager.Instance.Register(conn, playerComponent);
        }

        NetworkServer.AddPlayerForConnection(conn, player);
    }

    public override void OnServerRemovePlayer(NetworkConnection conn, NetworkIdentity player)
    {
        PlayerManager.Instance.Unregister(conn);

        base.OnServerRemovePlayer(conn, player);
    }

    public override void OnStartServer()
    {
        NetworkServer.RegisterHandler<ChangeReadyMessage>((conn, msg) => 
        {
            var player = conn.identity.GetComponent<Player>();

            // This should never happen, but you never know.
            if (player == null)
                return;

            player.IsReady = !player.IsReady;
        });
    }

    public override void OnStartClient()
    {
        // Forces resources to load
        var abilities = ScriptableAbility.Cache.Count;
        var audio = AudioManager.Cache.Count;
    }

    public override void OnStopServer()
    {
        if (PlayerManager.Instance != null)
            PlayerManager.Instance.Clear();
    }

    public override void OnStopClient()
    {
        if (PlayerManager.Instance != null)
            PlayerManager.Instance.Clear();
    }
}