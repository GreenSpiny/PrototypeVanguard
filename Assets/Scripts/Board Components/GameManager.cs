using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : NetworkBehaviour
{
    public static GameManager instance;

    [SerializeField] public NetworkManager networkManager;
    [SerializeField] public DragManager dragManager;
    [SerializeField] public LetterboxedCanvas letterboxedCanvas;

    [NonSerialized] public Dictionary<int, Node> allNodes = new Dictionary<int, Node>();
    [NonSerialized] public Dictionary<int, Card> allCards = new Dictionary<int, Card>();

    [NonSerialized] public int turnPlayer;
    [NonSerialized] public List<ConnectectionStruct> connectedPlayers = new List<ConnectectionStruct>();
    [SerializeField] public Player[] players;
    [SerializeField] private Camera infoCamera;
    [SerializeField] private Chatbox chatbox;

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

        foreach (var card in FindObjectsByType<Card>(FindObjectsSortMode.None))
        {
            int cardID = Convert.ToInt32(card.name.Split('_')[1]);
            card.cardID = cardID;
            allCards.Add(cardID, card);
        }

        foreach (var node in FindObjectsByType<Node>(FindObjectsSortMode.None))
        {
            int nodeID = Convert.ToInt32(node.name.Split('_')[2]);
            node.Init(nodeID);
            allNodes.Add(nodeID, node);
        }
        foreach (var node in allNodes.Values)
        {
            node.AlignCards(true);
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
            }
        }   
    }

    // === CARD NETWORK REQUESTS === //

    [Rpc(SendTo.Everyone)]
    public void RequestRecieveCardRpc(int nodeID, int cardID, string parameters)
    {
        Node targetNode = allNodes[nodeID];
        Card targetCard = allCards[cardID];
        targetNode.RecieveCard(targetCard, parameters);
    }

    [Rpc(SendTo.Everyone)]
    public void RequestRetireCardsRpc(int nodeID, string parameters)
    {
        Node targetNode = allNodes[nodeID];
        targetNode.RetireCards();
    }

    [Rpc(SendTo.Everyone)]
    public void RequestSetOrientationRpc(int cardID, bool flip, bool rest)
    {
        Card targetCard = allCards[cardID];
        targetCard.SetOrientation(flip, rest);
    }

    [Rpc(SendTo.Everyone)]
    public void RequestEditPowerRpc(int cardID, int powerModifier, int critModifier, int driveModifier)
    {
        Card targetCard = allCards[cardID];
        targetCard.EditPower(powerModifier, critModifier, driveModifier);
    }

    [Rpc(SendTo.Everyone)]
    public void RequestResetPowerRpc(int cardID)
    {
        Card targetCard = allCards[cardID];
        targetCard.ResetPower();
    }

    [Rpc(SendTo.Everyone)]
    public void RequestDisplayCardsRpc(int playerID, int nodeID, int cardCount, bool revealCards)
    {
        Node targetNode = allNodes[nodeID];
        DragManager.instance.OpenDisplay(playerID, targetNode, cardCount, revealCards);
    }

    [Rpc(SendTo.Everyone)]
    public void RequestCloseDisplayRpc(int playerID)
    {
        DragManager.instance.CloseDisplay(playerID);
    }

    [Rpc(SendTo.Everyone)]
    public void RequestRevealCardRpc(int cardID, float revealDuration)
    {
        Card targetCard = allCards[cardID];
        targetCard.SetRevealed(true, revealDuration);
    }

    [Rpc(SendTo.Everyone)]
    public void RequestSendChatMessageRpc(int playerID, string message)
    {
        chatbox.RecieveMessage(playerID, message);
    }

    // CARD STATE STRUCT
    // This struct contains the source of truth for cards - namely, how they are positioned.
    // (not used yet)
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

    private void Update()
    {
        // Temporary deck initialization test
        if (Input.GetKeyDown(KeyCode.P))
        {
            players[0].AssignDeck(CardInfo.CreateRandomDeck());
            players[1].AssignDeck(CardInfo.CreateRandomDeck());
        }
    }

}
