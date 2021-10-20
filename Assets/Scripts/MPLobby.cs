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
    [SerializeField] private Button startGameButton;

    //holds a list of network players
    private NetworkList<MPPlayerInfo> nwPlayers = new NetworkList<MPPlayerInfo>();


    void Start()
    {
        if (IsOwner)
        {
            UpdateConnListServerRpc(NetworkManager.LocalClientId);
        }
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
        Debug.Log("List Updated");
        //update the UI lobby
        int index = 0;
        foreach (MPPlayerInfo connectedplayer in nwPlayers)
        {
            lobbyPlayers[index].playerName.text = connectedplayer.networkPlayerName;
            lobbyPlayers[index].readyButton.SetIsOnWithoutNotify(connectedplayer.networkPlayerReady);
            index++;
        }

        if(IsHost)
        {
            startGameButton.gameObject.SetActive(true);
            startGameButton.interactable = CheckEveryoneReady();
        }
    }

    public void StartGame()
    {
        if (IsServer)
        {
            NetworkSceneManager.OnSceneSwitched += SceneSwitched;
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
        if(IsOwner)
        {
            UpdateConnListServerRpc(clientId);
        }
        Debug.Log("A Player has connected ID:" + clientId);
    }

    [ServerRpc]
    private void UpdateConnListServerRpc(ulong clientId)
    {
        nwPlayers.Add(new MPPlayerInfo(clientId, PlayerPrefs.GetString("PName"), false));
    }

    private void ClientDisconnectedHandle(ulong clientId)
    {
        for(int indx = 0; indx < nwPlayers.Count; indx++)
        {
            if (clientId == nwPlayers[indx].networkClientId)
            {
                nwPlayers.RemoveAt(indx);
                Debug.Log("A Player has left ID: " + clientId);

                break;
            }
        }
    }
    private void ClientConnectedHandle(ulong clientId)
    {
        Debug.Log("TODO: Player Connected");
    }

    [ServerRpc(RequireOwnership = false)]
    private void ReadyUpServerRpc(ServerRpcParams serverRpcParams = default)
    {
        for(int indx = 0; indx < nwPlayers.Count; indx++)
        {
            if(nwPlayers[indx].networkClientId == serverRpcParams.Receive.SenderClientId)
            {
                Debug.Log("Updated with new");
                nwPlayers[indx] = new MPPlayerInfo(nwPlayers[indx].networkClientId, nwPlayers[indx].networkPlayerName, !nwPlayers[indx].networkPlayerReady);
            }
        }
    }

    public void ReadyButtonPressed()
    {
        ReadyUpServerRpc();
        if(IsLocalPlayer)
        {
            Debug.Log("Ready Pressed");
        }
    }

    public void SceneSwitched()
    {
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");

        //spawn a playerprefab for each connected client
        foreach (MPPlayerInfo tmpClient in nwPlayers)
        {
            //random spawn point location
            UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
            int index = UnityEngine.Random.Range(0, spawnPoints.Length);
            GameObject currentPoint = spawnPoints[index];

            GameObject playerSpawn = Instantiate(playerPrefab, currentPoint.transform.position, Quaternion.identity);
            playerSpawn.GetComponent<NetworkObject>().SpawnWithOwnership(tmpClient.networkClientId);
            Debug.Log("Player spawned for: " + tmpClient.networkPlayerName);
        }
    }

    private bool CheckEveryoneReady()
    {
        foreach (MPPlayerInfo players in nwPlayers)
        {
            if (!players.networkPlayerReady)
            {
                return false;
            }
        }
        return true;
    }
}
