using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
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

    [NonSerialized] private int dieRollWinner;
    [NonSerialized] public int turnPlayer;

    [NonSerialized] public List<ConnectectionStruct> connectedPlayers = new List<ConnectectionStruct>();
    [SerializeField] public Player[] players;
    [SerializeField] private Camera infoCamera;
    [SerializeField] private Chatbox chatbox;
    [SerializeField] private AnimationProperties animationProperties;

    private enum GameState { setup, dieroll, mulligan, draw, ride, battle, end, finished }
    private GameState gameState = GameState.setup;

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
    public void RequestDisplayCardsRpc(int playerID, int nodeID, int cardCount, bool revealCards, bool sortCards)
    {
        Node targetNode = allNodes[nodeID];
        DragManager.instance.OpenDisplay(playerID, targetNode, cardCount, revealCards, sortCards);
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

    // === SETUP NETWORK REQUESTS === //

    [Rpc(SendTo.Everyone)]
    public void RequestDieRollEventRpc(int result)
    {
        dieRollWinner = result;
        animationProperties.UIAnimator.gameObject.SetActive(true);

        int gzoneCount = DragManager.instance.OpposingPlayer.gzone.cards.Count;
        string gzoneResult;
        if (gzoneCount == 1)
        {
            gzoneResult = gzoneCount.ToString() + " card";
        }
        else
        {
            gzoneResult = gzoneCount.ToString() + " cards";
        }
        animationProperties.gZoneCountText.text = animationProperties.gZoneCountText.text.Replace("[x]", gzoneResult);
        animationProperties.UIAnimator.anim.SetBool("WinRoll", dieRollWinner == DragManager.instance.controllingPlayer.playerIndex);
        animationProperties.UIAnimator.anim.Play("Game Start");
    }

    [Rpc(SendTo.Everyone)]
    public void RequestGoFirstSecondRpc(int choice)
    {
        if (choice == 0)
        {
            turnPlayer = dieRollWinner;
        }
        else
        {
            turnPlayer = (dieRollWinner + 1) % 2;
        }

        if (turnPlayer == DragManager.instance.controllingPlayer.playerIndex)
        {
            animationProperties.firstSecondText.text = animationProperties.firstSecondText.text.Replace("[x]", "first");
        }
        else
        {
            animationProperties.firstSecondText.text = animationProperties.firstSecondText.text.Replace("[x]", "second");
        }

        animationProperties.UIAnimator.anim.SetBool("RollChoice", turnPlayer == DragManager.instance.controllingPlayer.playerIndex);
        animationProperties.UIAnimator.anim.SetBool("RollDecided", true);
    }

    [Rpc(SendTo.Everyone)]
    public void RequestGameStartRpc()
    {
        gameState = GameState.mulligan;
        DragManager.instance.ChangeDMstate(DragManager.DMstate.open);
        for (int i = 0; i < 2; i++)
        {
            players[i].VC.cards[0].SetOrientation(false, false);
            for (int j = 1; j <= 5; j++)
            {
                Node targetDeck = players[i].deck;
                players[i].hand.RecieveCard(targetDeck.cards[targetDeck.cards.Count - j], string.Empty);
            }
        }
        animationProperties.UIAnimator.Close();
    }

    [Rpc(SendTo.Server)]
    public void SubmitDeckListToServerRpc(int playerIndex, string deckName, int cardSleeves, int[] mainDeck, int[] rideDeck, int[] strideDeck, int[] toolbox)
    {
        CardInfo.DeckList submittedDeck = new CardInfo.DeckList(deckName, cardSleeves, mainDeck, rideDeck, strideDeck, toolbox);
        players[playerIndex].AssignDeck(submittedDeck);
        for (int i = 0; i < 2; i++)
        {
            CardInfo.DeckList targetList = players[i].deckList;
            if (targetList != null)
            {
                BroadcastDeckListToClientRpc(i, targetList.deckName, targetList.cardSleeves, targetList.mainDeck, targetList.rideDeck, targetList.strideDeck, targetList.toolbox);
            }
        }
    }

    [Rpc(SendTo.NotServer)]
    public void BroadcastDeckListToClientRpc(int playerIndex, string deckName, int cardSleeves, int[] mainDeck, int[] rideDeck, int[] strideDeck, int[] toolbox)
    {
        CardInfo.DeckList broadcastedDeck = new CardInfo.DeckList(deckName, cardSleeves, mainDeck, rideDeck, strideDeck, toolbox);
        players[playerIndex].AssignDeck(broadcastedDeck);
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
        if (gameState == GameState.setup)
        {
            bool readyToStart = false;
            int dieRoll = 0;
            if (Application.isEditor)
            {
                animationProperties.UIAnimator.Close();
                gameState = GameState.mulligan;
                DragManager.instance.ChangeDMstate(DragManager.DMstate.open);
            }
            else
            {
                readyToStart = players[0].deckList != null && players[1].deckList != null;
                dieRoll = Mathf.RoundToInt(UnityEngine.Random.Range(0f, 1f));
            }
            if (readyToStart)
            {
                gameState = GameState.dieroll;
                RequestDieRollEventRpc(dieRoll);
            }
        }
    }

    [System.Serializable]
    private class AnimationProperties
    {
        public AnimatorEventUtility UIAnimator;
        public TextMeshProUGUI gZoneCountText;
        public TextMeshProUGUI firstSecondText;
    }

}
