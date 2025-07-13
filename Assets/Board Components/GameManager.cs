using UnityEngine;
using System.Collections.Generic;
using System;
using Unity.Netcode;

public class GameManager : NetworkBehaviour
{
    public static GameManager instance;

    [SerializeField] public NetworkManager networkManager;
    [SerializeField] public DragManager dragManager;
    [SerializeField] public LetterboxedCanvas letterboxedCanvas;

    [NonSerialized] public Dictionary<int, Node> allNodes = new Dictionary<int, Node>();
    [NonSerialized] public List<Card> allCards = new List<Card>();

    [SerializeField] public List<ConnectectionStruct> connectedPlayers = new List<ConnectectionStruct>();
    [SerializeField] public Player[] players;
    [SerializeField] private Camera infoCamera;

    [System.Serializable]
    public struct ConnectectionStruct
    {
        public readonly ulong clientId;
        public readonly NetworkClient client;
        public ConnectectionStruct(ulong clientId, NetworkClient client)
        {
            this.clientId = clientId;
            this.client = client;
        }
    }

    private void Awake()
    {
        networkManager.OnConnectionEvent += OnConnectionOverride;

        if (instance == null)
        {
            instance = this;
        }
        else
        {
            DestroyImmediate(gameObject);
            return;
        }

        dragManager.Init();

        int nodeCount = 0;
        foreach (var node in FindObjectsByType<Node>(FindObjectsSortMode.None))
        {
            allNodes.Add(nodeCount, node);
            node.Init(nodeCount);
            nodeCount++;
        }

        int cardCount = 0;
        foreach (var card in FindObjectsByType<Card>(FindObjectsSortMode.None))
        {
            allCards.Add(card);
            cardCount++;
        }

        infoCamera.gameObject.SetActive(true);
    }

    public void OnConnectionOverride(NetworkManager manager, ConnectionEventData data)
    {
        
        if (manager.IsHost)
        {
            var clientId = data.ClientId;
            var client = manager.ConnectedClients[clientId];
            connectedPlayers.Add(new ConnectectionStruct(clientId, client));

            PlayerPuppeteer playerPrefab = client.PlayerObject.GetComponent<PlayerPuppeteer>();
            playerPrefab.playerIndex.Value = connectedPlayers.Count - 1;
            playerPrefab.SetupPlayerRpc();
        }
        
    }

}
