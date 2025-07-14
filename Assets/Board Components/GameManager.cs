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
    [NonSerialized] public Dictionary<int, Card> allCards = new Dictionary<int, Card>();

    [SerializeField] public List<ConnectectionStruct> connectedPlayers = new List<ConnectectionStruct>();
    [SerializeField] public Player[] players;
    [SerializeField] private Camera infoCamera;

    [SerializeField] public GameObject testSphere;

    [System.Serializable]
    public struct ConnectectionStruct
    {
        public readonly ulong clientID;
        public readonly NetworkClient client;
        public ConnectectionStruct(ulong clientID, NetworkClient client)
        {
            this.clientID = clientID;
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
            allCards.Add(cardCount, card);
            cardCount++;
        }

        infoCamera.gameObject.SetActive(true);
    }

    public void OnConnectionOverride(NetworkManager manager, ConnectionEventData data)
    {
        if (manager.IsServer)
        {
            if (data.EventType == ConnectionEvent.ClientConnected)
            {
                var clientID = data.ClientId;
                var client = manager.ConnectedClients[clientID];
                connectedPlayers.Add(new ConnectectionStruct(clientID, client));
                PlayerPuppeteer playerPrefab = client.PlayerObject.GetComponent<PlayerPuppeteer>();
                playerPrefab.SetupPlayerRpc(clientID, connectedPlayers.Count - 1);
                testSphere.transform.Rotate(0f, 0f, -45f);
            }
        }   
    }



    // CARD STATE STRUCT
    // This struct contains the source of truth for cards - namely, how they are positioned.
    public struct CardStateStruct : INetworkSerializable
    {
        public int cardId;
        public int nodeId;
        public bool rest;
        public bool flip;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref cardId);
            serializer.SerializeValue(ref nodeId);
            serializer.SerializeValue(ref rest);
            serializer.SerializeValue(ref flip);
        }
    }

}
