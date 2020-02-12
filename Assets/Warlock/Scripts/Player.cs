using Mirror;
using UnityEngine;

[AddComponentMenu("Warlock/Player")]
public class Player : NetworkBehaviour
{
    public static Player Local { get; private set; } = null;
    
    /// <summary>
    /// Not synced, we attach it manually on each instance.
    /// </summary>
    public Actor Actor { get; set; } = null;

    [SyncVar] public int ConnectionId = -1;
    [SyncVar] public int LobbyIndex = -1;
    [SyncVar] public string Name = "Undefined";
    [SyncVar] public Color Color = Color.white;
    [SyncVar] public int Score = -1;
    [SyncVar] public bool IsReady = false;

    public override void OnStartClient() => PlayerManager.Instance.Register(this);
    public override void OnNetworkDestroy() => PlayerManager.Instance.Unregister(this);
    public override void OnStartLocalPlayer() => Local = this;
}