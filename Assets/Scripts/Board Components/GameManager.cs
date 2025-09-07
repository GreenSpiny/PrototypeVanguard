using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Matchmaker.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : NetworkBehaviour
{
    public static GameManager instance;                     // Static instance.
    public static CardInfo.DeckList localPlayerDecklist1;   // The primary decklist assigned for the game.
    public static CardInfo.DeckList localPlayerDecklist2;   // The secondary decklist assigned for the game. Only relevant in singleplayer mode.
    public static bool singlePlayer;                        // Flag for whether the game should be run in singleplayer mode or multiplayer mode.
    public static bool player2starts;
    public static string localPlayerName;

    [SerializeField] public NetworkManager networkManager;
    [SerializeField] public DragManager dragManager;
    [SerializeField] public LetterboxedCanvas letterboxedCanvas;
    [SerializeField] public Canvas boardOverlayCanvas;
    [SerializeField] public Canvas boardUnderlayCanvas;
    [SerializeField] private CardDetailUI cardDetailUI;
    [SerializeField] private SceneLoadCanvas sceneLoadCanvas;

    [NonSerialized] public Dictionary<int, Node> allNodes = new Dictionary<int, Node>();
    [NonSerialized] public Dictionary<int, Card> allCards = new Dictionary<int, Card>();

    [NonSerialized] private int dieRollWinner;
    [NonSerialized] public int turnPlayer;
    [NonSerialized] public int turnCount = 0;
    [NonSerialized] private bool drewForTurn = false;

    [NonSerialized] public List<ConnectectionStruct> connectedPlayers = new List<ConnectectionStruct>();
    [SerializeField] public Player[] players;
    [SerializeField] private Camera infoCamera;
    [SerializeField] private Chatbox chatbox;
    [SerializeField] private AnimationProperties animationProperties;
    
    [SerializeField] public PhaseIndicator phaseIndicator;

    [SerializeField] private TextMeshProUGUI debugString;

    private const int maxConnections = 2;
    private const string connectionType = "udp";
    private bool exiting;

    public enum GameState { setup, dieroll, gaming, finished }
    public enum Phase { none = 0, mulligan = 1, ride = 2, main = 3, battle = 4, end = 5 }
    public static string[] phaseNames = { "none", "Mulligan", "Ride", "Main", "Battle", "End" };

    [NonSerialized] public GameState gameState = GameState.setup;
    [NonSerialized] public Phase phase = Phase.none;

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
        if (Application.isEditor)
        {
            singlePlayer = true;
        }
        networkManager.SetSingleton();
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
        exiting = false;
    }

    private int NextPlayer(int input)
    {
        return (input + 1) % 2;
    }

    private void Start()
    {
        if (singlePlayer)
        {
            UnityTransport transport = networkManager.GetComponent<UnityTransport>();
            transport.SetConnectionData("127.0.0.1", 7777);
            bool startedHost = networkManager.StartHost();
            animationProperties.UIAnimator.anim.Play("Game Static");
            cardDetailUI.DisableChat();
        }
        else if (MultiplayerManagerV2.hostedLobby != null)
        {
            StartHostingAsync(MultiplayerManagerV2.hostedLobby);
        }
        else if (MultiplayerManagerV2.leechedLobby != null)
        {
            StartCoroutine(ListenForHostMessage());
        }
    }

    private async void LeaveGameAsync()
    {
        exiting = true;
        try
        {
            if (MultiplayerManagerV2.hostedLobby != null)
            {
                await LobbyService.Instance.DeleteLobbyAsync(MultiplayerManagerV2.hostedLobby.Id);
            }
            else if (MultiplayerManagerV2.leechedLobby != null)
            {
                await LobbyService.Instance.RemovePlayerAsync(MultiplayerManagerV2.leechedLobby.Id, AuthenticationService.Instance.PlayerId);
                
            }
        }
        catch (AuthenticationException e)
        {
            debugString.text = e.Message;
        }
        catch (LobbyServiceException e)
        {
            debugString.text = e.Message;
        }
        networkManager.Shutdown();
        sceneLoadCanvas.TransitionOut();
    }

    private async void StartHostingAsync(Lobby lobby)
    {
        try
        {
            var allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
            networkManager.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, connectionType));
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            MultiplayerManagerV2.relayCode = networkManager.StartHost() ? joinCode : null;

            if (!string.IsNullOrEmpty(MultiplayerManagerV2.relayCode))
            {
                UpdateLobbyOptions options = new UpdateLobbyOptions();
                options.IsPrivate = true;
                options.IsLocked = true;
                options.Data = new Dictionary<string, DataObject>()
                {
                    {
                    "RelayCode", new DataObject(
                        visibility: DataObject.VisibilityOptions.Member,
                        value: MultiplayerManagerV2.relayCode,
                        index: DataObject.IndexOptions.S5

                    )}
                };
                await LobbyService.Instance.UpdateLobbyAsync(lobby.Id, options);
            }
        }
        catch (RelayServiceException e)
        {
            debugString.text = "Relay error: " + e.Message;
        }
        catch (LobbyServiceException e)
        {
            debugString.text = "Lobby error: " + e.Message;
        }
    }
    private async void StartLeechingAsync()
    {
        var allocation = await RelayService.Instance.JoinAllocationAsync(MultiplayerManagerV2.relayCode);
        networkManager.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, connectionType));
        NetworkManager.Singleton.StartClient();
    }

    private IEnumerator ListenForHostMessage()
    {
        while (string.IsNullOrEmpty(MultiplayerManagerV2.relayCode))
        {
            yield return null;
        }
        StartLeechingAsync();
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

    private void AssignActionFlags(int playerIndex)
    {
        foreach (Card card in players[playerIndex].cards)
        {
            foreach (CardInfo.ActionFlag flag in card.cardInfo.playerActionFlags)
            {
                players[playerIndex].playerActionFlags.Add(flag);
            }
            foreach (CardInfo.ActionFlag flag in card.cardInfo.globalActionFlags)
            {
                players[playerIndex].playerActionFlags.Add(flag);
                players[NextPlayer(playerIndex)].playerActionFlags.Add(flag);
            }
        }
    }

    public void ChangePhase(bool forward)
    {
        // Change the phase
        Phase currentPhase = phase;
        if (forward)
        {
            switch (currentPhase)
            {
                case Phase.mulligan:
                    RequestStandAndDrawRpc(turnPlayer);
                    break;
                case Phase.ride:
                    RequestChangePhaseRpc(turnCount, (int)Phase.main);
                    break;
                case Phase.main:
                    RequestChangePhaseRpc(turnCount, (int)Phase.battle);
                    break;
                case Phase.battle:
                    RequestChangePhaseRpc(turnCount, (int)Phase.end);
                    break;
                case Phase.end:
                    RequestStandAndDrawRpc(NextPlayer(turnPlayer));
                    break;
            }
        }
        else
        {
            switch(currentPhase)
            {
                case Phase.ride:
                    if (turnCount == 0)
                    {
                        RequestChangePhaseRpc(turnCount, (int)Phase.mulligan);
                    }
                    break;
                case Phase.main:
                    RequestChangePhaseRpc(turnCount, (int)Phase.ride);
                    break;
                case Phase.battle:
                    RequestChangePhaseRpc(turnCount, (int)Phase.main);
                    break;
                case Phase.end:
                    RequestChangePhaseRpc(turnCount, (int)Phase.battle);
                    break;
            }
        }
    }

    public static string SanitizeString(string inputString)
    {
        HashSet<char> badChars = new HashSet<char>() { '\n', '\r', '\t', '\\', '`', '{', '}' };
        StringBuilder builder = new StringBuilder(inputString.Length);
        foreach (char c in inputString)
        {
            if (!badChars.Contains(c))
            {
                builder.Append(c);
            }
        }
        string result = builder.ToString();
        return result.Trim();
    }

    public static string SimplifyString(string inputString)
    {
        HashSet<char> badChars = new HashSet<char>() { '\n', '\r', '\t', ' ' };
        StringBuilder builder = new StringBuilder(inputString.Length);
        foreach (char c in inputString)
        {
            if (!badChars.Contains(c))
            {
                builder.Append(c);
            }
        }
        string result = builder.ToString();
        return result.Trim();
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
    public void RequestShuffleCardsRpc(int nodeID, int randomSeed)
    {
        Node targetNode = allNodes[nodeID];
        targetNode.Shuffle(randomSeed, false);
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
    public void RequestSetEnergyRpc(int playerID, int amount)
    {
        players[playerID].SetEnergy(amount);
    }

    [Rpc(SendTo.Everyone)]
    public void RequestStandAndDrawRpc(int newTurnPlayer)
    {
        bool turnChange = turnPlayer != newTurnPlayer;
        if (turnChange)
        {
            drewForTurn = false;
            turnPlayer = newTurnPlayer;
            turnCount++;
            phaseIndicator.phaseAnimator.Play("phase turn " + turnPlayer.ToString(), 0, 0f);
            players[turnPlayer].IncrementEnergy(3);
        }
        else
        {
            phaseIndicator.phaseAnimator.Play("phase pulse", 0, 0f);
        }
        phaseIndicator.overlayPhaseAnimator.Play("phase pulse", 0, 0f);

        Player targetPlayer = players[turnPlayer];
        if (!drewForTurn)
        {
            Node targetDeck = targetPlayer.deck;
            if (targetDeck.HasCard)
            {
                targetPlayer.hand.RecieveCard(targetDeck.cards[targetDeck.cards.Count - 1], string.Empty);
            }
            drewForTurn = true;
            Node targetVC = targetPlayer.VC;
            if (targetVC.HasCard)
            {
                Card topCard = targetVC.cards[targetVC.cards.Count - 1];
                topCard.SetOrientation(false, false);
            }
            foreach (Node targetRC in targetPlayer.RC)
            {
                if (targetRC.HasCard)
                {
                    Card topCard = targetRC.cards[targetRC.cards.Count - 1];
                    topCard.SetOrientation(topCard.flip, false);
                }
            }
        }

        phase = Phase.ride;
        phaseIndicator.phaseText.text = phaseNames[(int)phase] + " Phase";

        foreach (Player player in players)
        {
            player.OnPhaseChanged();
        }
    }

    [Rpc(SendTo.Everyone)]
    public void RequestChangePhaseRpc(int currentTurn, int phase)
    {
        if (currentTurn >= turnCount)
        {
            this.phase = (Phase)phase;
            phaseIndicator.phaseText.text = phaseNames[(int)this.phase] + " Phase";
            foreach (Player player in players)
            {
                player.OnPhaseChanged();
            }
            phaseIndicator.phaseAnimator.Play("phase pulse", 0, 0f);
            phaseIndicator.overlayPhaseAnimator.Play("phase pulse", 0, 0f);
        }
    }

    [Rpc(SendTo.Everyone)]
    public void RequestDisplayCardsRpc(int playerID, int nodeID, int startIndex, int cardCount, bool revealCards, bool sortCards)
    {
        Node targetNode = allNodes[nodeID];
        DragManager.instance.OpenDisplay(playerID, targetNode, startIndex, cardCount, revealCards, sortCards);
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
            turnPlayer = NextPlayer(dieRollWinner);
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
        AssignActionFlags(0);
        AssignActionFlags(1);

        gameState = GameState.gaming;
        phase = Phase.mulligan;
        DragManager.instance.ChangeDMstate(DragManager.DMstate.open);
        for (int i = 0; i < 2; i++)
        {
            if (singlePlayer)
            {
                players[i].VC.cards[0].SetOrientation(false, false);
                players[i].VC.AlignCards(true);
            }
            for (int j = 1; j <= 5; j++)
            {
                Node targetDeck = players[i].deck;
                players[i].hand.RecieveCard(targetDeck.cards[targetDeck.cards.Count - j], string.Empty);
            }
        }
        animationProperties.UIAnimator.Close();
        phaseIndicator.phaseAnimator.Play("phase neutral " + turnPlayer.ToString());
        foreach (Player player in players)
        {
            player.OnPhaseChanged();
        }
    }
    private IEnumerator RequestGameStartDelayed() { yield return null;  RequestGameStartRpc(); }

    [Rpc(SendTo.Server)]
    public void SubmitDeckListToServerRpc(int playerIndex, string playerName, string nation, int[] mainDeck, int[] rideDeck, int[] strideDeck, int[] toolbox)
    {
        CardInfo.DeckList submittedDeck = new CardInfo.DeckList("default", nation, mainDeck, rideDeck, strideDeck, toolbox);
        submittedDeck.ShuffleMainDeck();
        players[playerIndex].AssignDeck(submittedDeck);
        //animationProperties.playerNames[playerIndex].text = playerName;

        // Singleplayer
        if (singlePlayer)
        {
            players[playerIndex].AssignDeck(submittedDeck);
            CardInfo.DeckList player2Deck;
            if (localPlayerDecklist2 != null)
            {
                player2Deck = localPlayerDecklist2;
            }
            else
            {
                player2Deck = CardInfo.CreateRandomDeck();
            }
            int nextPlayerIndex = NextPlayer(playerIndex);
            players[nextPlayerIndex].AssignDeck(player2Deck);
            animationProperties.playerNames[nextPlayerIndex].text = "Shadowboxer";

            turnPlayer = 0;
            if (player2starts)
            {
                turnPlayer = 1;
                Debug.Log("not me!");
            }

            StartCoroutine(RequestGameStartDelayed());
        }
        // Multiplayer
        else
        {
            for (int i = 0; i < 2; i++)
            {
                CardInfo.DeckList targetList = players[i].deckList;
                if (targetList != null)
                {
                    BroadcastDeckListToClientRpc(i, targetList.deckName, targetList.nation, targetList.mainDeck, targetList.rideDeck, targetList.strideDeck, targetList.toolbox);
                }
            }
        }
    }

    [Rpc(SendTo.NotServer)]
    public void BroadcastDeckListToClientRpc(int playerIndex, string deckName, string nation, int[] mainDeck, int[] rideDeck, int[] strideDeck, int[] toolbox)
    {
        if (players[playerIndex].deckList == null)
        {
            CardInfo.DeckList broadcastedDeck = new CardInfo.DeckList(deckName, nation, mainDeck, rideDeck, strideDeck, toolbox);
            players[playerIndex].AssignDeck(broadcastedDeck);
        }
    }

    private void Update()
    {
        if (!singlePlayer && gameState == GameState.setup)
        {
            bool readyToStart = players[0].deckList != null && players[1].deckList != null;
            if (readyToStart)
            {
                gameState = GameState.dieroll;
                int dieRoll = Mathf.RoundToInt(UnityEngine.Random.Range(0f, 1f));
                RequestDieRollEventRpc(dieRoll);
            }
        }
        if (Input.GetKey(KeyCode.Escape) && !exiting)
        {
            LeaveGameAsync();
        }
    }

    [System.Serializable]
    private class AnimationProperties
    {
        public AnimatorEventUtility UIAnimator;
        public TextMeshProUGUI gZoneCountText;
        public TextMeshProUGUI firstSecondText;

        public TextMeshProUGUI[] playerNames;
        public Image[] playerImages;
    }

    [System.Serializable]
    public class PhaseIndicator
    {
        public GameObject root;
        public TextMeshProUGUI phaseText;
        public Animator phaseAnimator;
        public Animator overlayPhaseAnimator;
    }

}
