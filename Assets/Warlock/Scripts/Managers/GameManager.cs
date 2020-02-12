using Mirror;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    Lobby,
    Game,
    End
}

public class GameManager : NetworkBehaviour
{
    public GameState State => state;

    [SerializeField] private Actor warlockPrefab = null;
    [SyncVar] private GameState state = GameState.Lobby;
    private List<Actor> warlocks = new List<Actor>();
    private bool isSinglePlayer = false;

    #region Shared

    private void Update()
    {
        // Host is both server & client, so no else
        if (isServer) Server_Update();
        if (isClient) Client_Update();
    }

    #endregion

    #region Server

    [Server]
    private void Server_Update()
    {
        if (state == GameState.Lobby)
        {
            var players = PlayerManager.Instance.Players;
            var numPlayers = 0;
            var readyPlayers = 0;

            for (var i = 0; i < players.Length; i++)
            {
                var player = players[i];

                if (player == null)
                    continue;

                numPlayers++;

                if (player.IsReady)
                    readyPlayers++;
            }

            // If all players are ready, start
            if (readyPlayers >= numPlayers)
            {
                // Set single player if only one player is connected
                // this is mostly for debug reasons
                isSinglePlayer = numPlayers == 1;

                // Unready all players, because otherwise we'll respawn endlessly
                // which might be good for later, but not for now
                for (var i = 0; i < players.Length; i++)
                {
                    var player = players[i];

                    if (player == null)
                        continue;

                    player.IsReady = false;
                }

                StartGame();
            }
        }
        else if (state == GameState.Game)
        {
            for (var i = 0; i < warlocks.Count; i++)
            {
                var warlock = warlocks[i];

                if (warlock == null || (warlock.Life != null && warlock.Life.IsDead))
                {
                    warlocks.RemoveAt(i);
                    i--;
                }
            }

            if (warlocks.Count <= 0)
                EnterLobby();
        }
    }

    [Server]
    private void EnterLobby()
    {
        // Remove all remaining warlocks
        if (warlocks.Count > 0)
        {
            warlocks.ForEach(x => NetworkServer.Destroy(x.gameObject));
            warlocks.Clear();
        }

        FindObjectOfType<Lava>().Disable();

        // Set state to lobby
        state = GameState.Lobby;
    }

    [Server]
    public void StartGame()
    {
        var players = PlayerManager.Instance.Players;

        for (var i = 0; i < players.Length; i++)
        {
            var player = players[i];

            if (player == null)
                continue;

            var warlock = Instantiate(warlockPrefab, player.transform.position, player.transform.rotation);

            warlock.name = $"Warlock ({player.Name})";
            warlock.Owner = player;
            warlocks.Add(warlock);

            NetworkServer.Spawn(warlock.gameObject, player.connectionToClient);
        }

        FindObjectOfType<Lava>().Enable();

        state = GameState.Game;
    }
    
    #endregion

    #region Client

    [Client]
    private void Client_Update()
    {
        if (state == GameState.Lobby)
        {
            // Send ready message to server
            if (Input.GetButtonDown("Submit"))
                NetworkClient.Send(new ChangeReadyMessage());
        }
    }

    #endregion
}