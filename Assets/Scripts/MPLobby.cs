using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using System;
using MLAPI.NetworkVariable.Collections;
using MLAPI.Connection;
using MLAPI.SceneManagement;
using MLAPI.Messaging;
using UnityEngine.UI;


public class MPLobby : NetworkBehaviour
{
    [SerializeField] private LobbyPanel[] lobbyPlayers;
    [SerializeField] private GameObject playerPrefab;
    //holds a list of network players
    private NetworkList<MPPlayerInfo> nwPlayers = new NetworkList<MPPlayerInfo>();


    void Start()
    {

        UpdateConnListServerRpc(NetworkManager.LocalClientId);
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
    }

    public override void NetworkStart()
    {
        Debug.Log("StartingServer");
        if(IsClient)
        {
            nwPlayers.OnListChanged += PlayersInfoChanged;
        }
        if(IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += ClientConnectedHandle;
            NetworkManager.Singleton.OnClientDisconnectCallback += ClientDisconnectedHandle;
            //handle for people connected
            foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
            {
                ClientConnectedHandle(client.ClientId);
            }
        }
    }
    
    private void OnDestroy()
    {
        nwPlayers.OnListChanged -= PlayersInfoChanged;
        if (NetworkManager.Singleton)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= ClientConnectedHandle;
            NetworkManager.Singleton.OnClientDisconnectCallback -= ClientDisconnectedHandle;
        }
    }

    private void PlayersInfoChanged(NetworkListEvent<MPPlayerInfo> changeEvent)
    {
        //update the UI lobby
        int index = 0;
        foreach (MPPlayerInfo connectedplayer in nwPlayers)
        {
            lobbyPlayers[index].playerName.text = connectedplayer.networkPlayerName;
            index++;
        }
    }

    public void StartGame()
    {
        if (IsServer)
        {
            //spawn a playerprefab for each connected client
            foreach (MPPlayerInfo tmpClient in nwPlayers)
            {
                GameObject playerSpawn = Instantiate(playerPrefab, new Vector3(2f, 1f, 7f), Quaternion.identity);
                playerSpawn.GetComponent<NetworkObject>().SpawnWithOwnership(tmpClient.networkClientId);
                Debug.Log("Player spawned for: " + tmpClient.networkPlayerName);
            }
            NetworkSceneManager.SwitchScene("MainGame");
        }
        else
        {
            Debug.Log("You are not the host");
        }
    }

    /* HANDLES*/

    private void HandleClientConnected(ulong clientId)
    {
        UpdateConnListServerRpc(clientId);
        Debug.Log("A Player has connected ID:" + clientId);
    }

    [ServerRpc]
    private void UpdateConnListServerRpc(ulong clientId)
    {
        nwPlayers.Add(new MPPlayerInfo(clientId, PlayerPrefs.GetString("PName"), false));
    }

    private void ClientDisconnectedHandle(ulong clientId)
    {
        Debug.Log("TODO: Player Disconnected");
    }
    private void ClientConnectedHandle(ulong clientId)
    {
        Debug.Log("TODO: Player Connected");
    }
}
