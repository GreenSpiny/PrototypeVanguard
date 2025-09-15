using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static CardInfo;

public class MultiplayerManagerV2 : MonoBehaviour
{
    // Static elements
    public static MultiplayerManagerV2 instance;
    public static Lobby hostedLobby;
    public static Lobby leechedLobby;
    public static string relayCode;

    // Control elements
    public enum MultiplayerState { none, hosting, leeching, blocked };
    private MultiplayerState multiplayerState;
    private bool initialized = false;
    [SerializeField] private SceneLoadCanvas sceneLoadCanvas;
    [SerializeField] private RectTransform roomInstantiationArea;
    [SerializeField] private RectTransform playerInstantiationArea;
    [SerializeField] private AvatarSelector avatarSelector;

    // User fields
    [SerializeField] private Image userAvatar;
    [SerializeField] private TMP_InputField displayNameInputField;
    [SerializeField] private Toggle goFirstToggle;
    [SerializeField] private DeckSelectContainer player1DeckContainer;
    [SerializeField] private DeckSelectContainer player2DeckContainer;
    [SerializeField] private TMP_InputField roomCodeInputField;
    [SerializeField] private TMP_InputField roomNameFilter;
    [SerializeField] private TMP_InputField roomCodeFilter;
    private DeckSelectContainer[] deckSelectContainers;

    // User status displays
    [SerializeField] private TextMeshProUGUI gameVersionText;
    [SerializeField] private TextMeshProUGUI cardsVersionText;
    [SerializeField] private TextMeshProUGUI onlineStatusText;
    [SerializeField] private TextMeshProUGUI connectionStatusText;
    private List<RoomResult> roomResults;
    private List<PlayerResult> playerResults;

    // Blocking areas
    [SerializeField] private CanvasGroup optionsArea;
    [SerializeField] private CanvasGroup userLobbyArea;
    [SerializeField] private CanvasGroup browseLobbiesArea;
    [SerializeField] private Image grayOutImage;
    private CanvasGroup grayOutCanvas;

    // Major buttons
    [SerializeField] private Button startSingleplayerButton;
    [SerializeField] private Button startMultiplayerButton;
    [SerializeField] private Button stopMultiplayerButton;
    private Transform startMultiplayerParent;
    private Transform stopMultiplayerParent;

    // Prefabs
    [SerializeField] private RoomResult roomResultPrefab;
    [SerializeField] private PlayerResult playerResultPrefab;

    // Containers
    [Serializable]
    public class DeckSelectContainer
    {
        public TMP_Dropdown deckSelectDropdown;
        public TMP_Text deckValidationText;
        [NonSerialized] public CardInfo.DeckList deckList;
    }

    // === MAIN FUNCTIONS === //
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            DestroyImmediate(gameObject);
        }
        deckSelectContainers = new DeckSelectContainer[2] { player1DeckContainer, player2DeckContainer };
        startMultiplayerParent = startMultiplayerButton.transform.parent;
        stopMultiplayerParent = stopMultiplayerButton.transform.parent;
        grayOutCanvas = grayOutImage.GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        relayCode = null;
        roomResults = new List<RoomResult>();
        playerResults = new List<PlayerResult>();

        sceneLoadCanvas.Hide();
        connectionStatusText.text = string.Empty;
        ChangeMultiplayerState(MultiplayerState.none);
        StopHostingAndLeeching();

        string lastProfileName = PlayerPrefs.GetString(SaveDataManager.lastProfileNameKey);
        if (!string.IsNullOrWhiteSpace(lastProfileName))
        {
            displayNameInputField.text = lastProfileName;
        }

        StartCoroutine(LoadInitialDisplay());
    }

    private bool debuglimit = false;
    private void Update()
    {
        if (!debuglimit && !Application.isEditor)
        {
            if (Input.GetKey(KeyCode.Alpha0) && Input.GetKey(KeyCode.Alpha1))
            {
                debuglimit = true;
                UnityGameServices.instance.SwitchProfileAsync();
            }
            if (Input.GetKey(KeyCode.Alpha2) && Input.GetKey(KeyCode.Alpha8))
            {
                debuglimit = true;
                Caching.ClearCache();
            }
        }
    }

    private void AssignLocalPlayerData()
    {
        if (!string.IsNullOrWhiteSpace(displayNameInputField.text))
        {
            string sanitizedName = GameManager.SanitizeString(displayNameInputField.text);
            if (sanitizedName.Length > displayNameInputField.characterLimit)
            {
                sanitizedName = sanitizedName.Substring(0, displayNameInputField.characterLimit);
            }
            GameManager.localPlayerName = sanitizedName;
            PlayerPrefs.SetString(SaveDataManager.lastProfileNameKey, sanitizedName);
        }
        else
        {
            GameManager.localPlayerName = "Player";
        }
        if (userAvatar.sprite != null)
        {
            GameManager.localAvatar = userAvatar.sprite.name;
        }

        if (player1DeckContainer.deckList != null)
        {
            PlayerPrefs.SetString(SaveDataManager.player1DecklistKey, player1DeckContainer.deckList.deckName);
        }
        if (player2DeckContainer.deckList != null)
        {
            PlayerPrefs.SetString(SaveDataManager.player2DecklistKey, player2DeckContainer.deckList.deckName);
        }

        GameManager.localPlayerDecklist1 = player1DeckContainer.deckList;
        GameManager.localPlayerDecklist2 = player2DeckContainer.deckList;
        GameManager.player2starts = !goFirstToggle.isOn;

        GameManager.remotePlayerName = string.Empty;
        GameManager.remoteAvatar = string.Empty;
    }

    public void OnPlayerNameChanged(string playerName)
    {
        if (!string.IsNullOrWhiteSpace(playerName))
        {
            PlayerPrefs.SetString(SaveDataManager.lastProfileNameKey, playerName);
        }
    }

    public void SwapPlayer1Deck(int deckIndex)
    {
        SwapDecks(deckIndex, 0);
    }

    public void SwapPlayer2Deck(int deckIndex)
    {
        SwapDecks(deckIndex, 1);
    }

    private void SwapDecks(int deckIndex, int playerIndex)
    {
        if (initialized)
        {
            DeckSelectContainer targetContainer = deckSelectContainers[playerIndex];

            if (deckIndex >= 0 && deckIndex < targetContainer.deckSelectDropdown.options.Count)
            {
                string targetDeck = targetContainer.deckSelectDropdown.options[deckIndex].text;
                targetContainer.deckSelectDropdown.value = deckIndex;
                targetContainer.deckSelectDropdown.RefreshShownValue();
                targetContainer.deckList = SaveDataManager.LoadDeck(targetDeck);
            }

            CheckValidity();
        }
    }
    private void ClearRooms()
    {
        int count = roomResults.Count;
        for (int i = count - 1; i >= 0; i--)
        {
            if (roomResults[i] != null)
            {
                Destroy(roomResults[i].gameObject);
            }
        }
        roomResults.Clear();
    }

    private void ClearPlayers()
    {
        int count = playerResults.Count;
        for (int i = count - 1; i >= 0; i--)
        {
            if (playerResults[i] != null)
            {
                Destroy(playerResults[i].gameObject);
            }
        }
        playerResults.Clear();
    }

    private IEnumerator LoadInitialDisplay()
    {
        // Manually transition the scene in
        while (!CardLoader.CardsLoaded)
        {
            yield return null;
        }
        yield return null;

        gameVersionText.text = Application.version;
        cardsVersionText.text = CardLoader.instance.dataVersionObject.cardsFileVersion.ToString();

        string lastAvatarName = PlayerPrefs.GetString(SaveDataManager.lastAvatarKey);
        if (string.IsNullOrWhiteSpace(lastAvatarName))
        {
            lastAvatarName = AvatarBank.defaultAvatar;
        }
        SetAvatar(lastAvatarName);

        sceneLoadCanvas.TransitionIn();

        // Populate deck dropdown options
        player1DeckContainer.deckSelectDropdown.ClearOptions();
        player1DeckContainer.deckSelectDropdown.ClearOptions();

        List<string> deckOptions = SaveDataManager.GetAvailableDecks();
        if (deckOptions.Count == 0)
        {
            CardInfo.DeckList exampleDeck = SaveDataManager.GenerateExampleDeck();
            deckOptions.Add(exampleDeck.deckName);
        }
        foreach (string deckOption in deckOptions)
        {
            player1DeckContainer.deckSelectDropdown.options.Add(new TMP_Dropdown.OptionData(deckOption));
            player2DeckContainer.deckSelectDropdown.options.Add(new TMP_Dropdown.OptionData(deckOption));
        }
        if (player1DeckContainer.deckSelectDropdown.options.Count > 0)
        {
            int targetPlayer1DeckIndex = 0;
            string previousPlayer1Decklist = PlayerPrefs.GetString(SaveDataManager.player1DecklistKey);
            if (!string.IsNullOrEmpty(previousPlayer1Decklist))
            {
                for (int i = 0; i < player1DeckContainer.deckSelectDropdown.options.Count; i++)
                {
                    if (player1DeckContainer.deckSelectDropdown.options[i].text == previousPlayer1Decklist)
                    {
                        targetPlayer1DeckIndex = i;
                        break;
                    }
                }
            }
            string targetPlayer1Deck = player1DeckContainer.deckSelectDropdown.options[targetPlayer1DeckIndex].text;
            player1DeckContainer.deckSelectDropdown.value = targetPlayer1DeckIndex;
            player1DeckContainer.deckSelectDropdown.RefreshShownValue();
            player1DeckContainer.deckList = SaveDataManager.LoadDeck(targetPlayer1Deck);

            int targetPlayer2DeckIndex = 0;
            string previousPlayer2Decklist = PlayerPrefs.GetString(SaveDataManager.player2DecklistKey);
            if (!string.IsNullOrEmpty(previousPlayer2Decklist))
            {
                for (int i = 0; i < player2DeckContainer.deckSelectDropdown.options.Count; i++)
                {
                    if (player2DeckContainer.deckSelectDropdown.options[i].text == previousPlayer2Decklist)
                    {
                        targetPlayer2DeckIndex = i;
                        break;
                    }
                }
            }
            string targetPlayer2Deck = player2DeckContainer.deckSelectDropdown.options[targetPlayer2DeckIndex].text;
            player2DeckContainer.deckSelectDropdown.value = targetPlayer2DeckIndex;
            player2DeckContainer.deckSelectDropdown.RefreshShownValue();
            player2DeckContainer.deckList = SaveDataManager.LoadDeck(targetPlayer2Deck);
        }

        // Complete initialization
        initialized = true;
        CheckValidity();
    }

    public void SetAvatar(string avatarName)
    {
        PlayerPrefs.SetString(SaveDataManager.lastAvatarKey, avatarName);
        userAvatar.sprite = CardLoader.instance.avatarBank.GetSprite(avatarName);
    }

    public void OpenAvatarWindow()
    {
        avatarSelector.gameObject.SetActive(true);
        ChangeMultiplayerState(MultiplayerState.blocked);
    }

    private void CheckValidity()
    {
        string error;
        bool[] decksValid = new bool[2] { false, false };

        for (int i = 0; i < deckSelectContainers.Length; i++)
        {
            DeckSelectContainer targetContainer = deckSelectContainers[i];
            if (targetContainer.deckSelectDropdown.options.Count == 0)
            {
                targetContainer.deckValidationText.text = "No decks found.";
                targetContainer.deckSelectDropdown.interactable = false;
            }
            else if (!targetContainer.deckList.IsValid(out error))
            {
                targetContainer.deckValidationText.text = "Deck is invalid.";
            }
            else
            {
                targetContainer.deckValidationText.text = "Deck is valid.";
                decksValid[i] = true;
            }
        }
        
        startSingleplayerButton.interactable = decksValid[0] && decksValid[1];
        startMultiplayerButton.interactable = decksValid[0];
    }

    public void StartSingleplayerGame()
    {
        AssignLocalPlayerData();
        GameManager.singlePlayer = true;
        SceneManager.LoadScene("FightScene");
    }

    public void ChangeMultiplayerState(MultiplayerState state)
    {
        multiplayerState = state;
        switch (multiplayerState)
        {
            case MultiplayerState.none:
                optionsArea.interactable = true;
                userLobbyArea.interactable = true;
                browseLobbiesArea.interactable = true;
                grayOutImage.gameObject.SetActive(false);
                startMultiplayerParent.gameObject.SetActive(true);
                stopMultiplayerParent.gameObject.SetActive(false);
                break;

            case MultiplayerState.hosting:
                optionsArea.interactable = false;
                userLobbyArea.interactable = true;
                browseLobbiesArea.interactable = false;
                grayOutImage.gameObject.SetActive(false);
                startMultiplayerParent.gameObject.SetActive(false);
                stopMultiplayerParent.gameObject.SetActive(true);
                break;

            case MultiplayerState.leeching:
                optionsArea.interactable = false;
                userLobbyArea.interactable = false;
                browseLobbiesArea.interactable = false;
                grayOutImage.gameObject.SetActive(true);
                grayOutCanvas.interactable = true;
                startMultiplayerParent.gameObject.SetActive(true);
                stopMultiplayerParent.gameObject.SetActive(false);
                break;

            case MultiplayerState.blocked:
                optionsArea.interactable = false;
                userLobbyArea.interactable = false;
                browseLobbiesArea.interactable = false;
                grayOutCanvas.interactable = false;
                break;
        }
    }

    public async void StartHostingAsync()
    {
        if (multiplayerState == MultiplayerState.none)
        {
            ChangeMultiplayerState(MultiplayerState.blocked);
            AssignLocalPlayerData();
            connectionStatusText.text = string.Empty;

            string lobbyName = GameManager.localPlayerName + "'s Lobby";
            int maxPlayers = 5;
            CreateLobbyOptions options = new CreateLobbyOptions();
            options.IsPrivate = false;
            options.Player = new Unity.Services.Lobbies.Models.Player();
            options.Player.Data = new Dictionary<string, PlayerDataObject>()
            {
                {
                "Name", new PlayerDataObject(
                    visibility: PlayerDataObject.VisibilityOptions.Public,
                    value: GameManager.localPlayerName)
                },
                {
                "Avatar", new PlayerDataObject(
                    visibility: PlayerDataObject.VisibilityOptions.Public,
                    value: userAvatar.sprite.name)
                },
                {
                "Host", new PlayerDataObject(
                    visibility: PlayerDataObject.VisibilityOptions.Public,
                    value: "true")
                }
            };
            options.Data = new Dictionary<string, DataObject>()
            {
                {
                    "CardsVersion", new DataObject(
                        visibility: DataObject.VisibilityOptions.Public,
                        value: CardLoader.instance.dataVersionObject.cardsFileVersion.ToString(),
                        index: DataObject.IndexOptions.N1)
                },
                {
                    "GameVersion", new DataObject(
                        visibility: DataObject.VisibilityOptions.Public,
                        value: Application.version,
                        index: DataObject.IndexOptions.S1)
                },
                {
                    "RoomCode", new DataObject(
                        visibility: DataObject.VisibilityOptions.Public,
                        value: "abcd", //roomCodeInputField.text,
                        index: DataObject.IndexOptions.S2)
                }
            };

            try
            {
                hostedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);

                var callbacks = new LobbyEventCallbacks();
                callbacks.LobbyChanged += OnLobbyChanged;
                callbacks.PlayerJoined += OnPlayerJoined;
                callbacks.PlayerLeft += OnPlayerLeft;
                callbacks.PlayerDataAdded += OnPlayerDataAdded;
                callbacks.LobbyEventConnectionStateChanged += OnLobbyEventConnectionStateChanged;
                var events = await LobbyService.Instance.SubscribeToLobbyEventsAsync(hostedLobby.Id, callbacks);

                connectionStatusText.text = "Waiting for players to join...";
                ChangeMultiplayerState(MultiplayerState.hosting);
            }
            catch (LobbyServiceException e)
            {
                connectionStatusText.text = "Lobby error: " + e.Message;
                ChangeMultiplayerState(MultiplayerState.none);
            }

            QueryLobbies();
        }
    }

    public async void StopHostingAsync()
    {
        if (hostedLobby != null)
        {
            string hostedLobbyId = hostedLobby.Id;
            hostedLobby = null;
            try
            {
                await LobbyService.Instance.DeleteLobbyAsync(hostedLobbyId);
            }
            catch (LobbyServiceException e)
            {
                connectionStatusText.text = "Lobby error: " + e.Message;
            }
        }
        if (multiplayerState == MultiplayerState.hosting)
        {
            ChangeMultiplayerState(MultiplayerState.none);
        }
        ClearRooms();
        ClearPlayers();
    }

    public async void StartLeechingAsync(RoomResult room)
    {
        if (multiplayerState == MultiplayerState.none)
        {
            GameManager.singlePlayer = false;
            ChangeMultiplayerState(MultiplayerState.blocked);
            AssignLocalPlayerData();
            connectionStatusText.text = string.Empty;

            try
            {
                JoinLobbyByIdOptions options = new JoinLobbyByIdOptions();
                options.Player = new Unity.Services.Lobbies.Models.Player();
                options.Player.Data = new Dictionary<string, PlayerDataObject>()
                {
                    {
                    "Name", new PlayerDataObject(
                        visibility: PlayerDataObject.VisibilityOptions.Public,
                        value: GameManager.localPlayerName)
                    },
                    {
                    "Avatar", new PlayerDataObject(
                        visibility: PlayerDataObject.VisibilityOptions.Public,
                        value: userAvatar.sprite.name)
                    }
                };

                Lobby targetLobby = await LobbyService.Instance.JoinLobbyByIdAsync(room.lobby.Id, options);
                var callbacks = new LobbyEventCallbacks();
                callbacks.LobbyChanged += OnLobbyChanged;
                callbacks.PlayerLeft += OnPlayerLeft;
                callbacks.PlayerDataAdded += OnPlayerDataAdded;
                callbacks.LobbyEventConnectionStateChanged += OnLobbyEventConnectionStateChanged;
                var events = await LobbyService.Instance.SubscribeToLobbyEventsAsync(targetLobby.Id, callbacks);

                leechedLobby = targetLobby;
                ChangeMultiplayerState(MultiplayerState.leeching);
            }
            catch (LobbyServiceException e)
            {
                connectionStatusText.text = e.Message;
                ChangeMultiplayerState(MultiplayerState.none);
            }
            catch (AuthenticationException e)
            {
                connectionStatusText.text = e.Message;
                // ChangeMultiplayerState(MultiplayerState.none);
            }
        }
    }

    public async void StopLeechingAsync()
    {
        if (leechedLobby != null)
        {
            string leechedLobbyId = leechedLobby.Id;
            leechedLobby = null;
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(leechedLobbyId, AuthenticationService.Instance.PlayerId);
            }
            catch (LobbyServiceException e)
            {
                connectionStatusText.text = "Lobby error: " + e.Message;
            }
            catch (AuthenticationException e)
            {
                connectionStatusText.text = "Authentication error: " + e.Message;
            }
        }
        if (multiplayerState == MultiplayerState.leeching)
        {
            ChangeMultiplayerState(MultiplayerState.none);
        }
        ClearRooms();
    }

    public void StopHostingAndLeeching()
    {
        StopHostingAsync();
        StopLeechingAsync();
    }

    public async void StartPlaying(string playerId, string remotePlayerName, string remotePlayerAvatar)
    {
        GameManager.singlePlayer = false;
        connectionStatusText.text = "Joining game!";
        ChangeMultiplayerState(MultiplayerState.blocked);
        if (hostedLobby != null)
        {
            try
            {
                UpdateLobbyOptions options = new UpdateLobbyOptions();
                options.IsPrivate = true;
                options.IsLocked = true;
                options.Data = new Dictionary<string, DataObject>()
                {
                    {
                    "MatchedPlayer", new DataObject(
                        visibility: DataObject.VisibilityOptions.Member,
                        value: playerId,
                        index: DataObject.IndexOptions.S4

                    )}
                };

                await LobbyService.Instance.UpdateLobbyAsync(hostedLobby.Id, options);
                GameManager.remotePlayerName = remotePlayerName;
                GameManager.remoteAvatar = remotePlayerAvatar;
                SceneManager.LoadScene("FightScene");
                return;
            }
            catch (LobbyServiceException e)
            {
                connectionStatusText.text = "Lobby error: " + e.Message;
            }
        }
        ChangeMultiplayerState(MultiplayerState.none);
        StopHostingAsync();
    }

    public async void KickPlayer(string playerId)
    {
        if (hostedLobby != null)
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(hostedLobby.Id, playerId);
            }
            catch (LobbyServiceException e)
            {
                connectionStatusText.text = "Lobby error: " + e.Message;
            }
        }
    }

    public async void QueryLobbies()
    {
        ClearRooms();
        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions();
            options.Count = 50;

            // Filter for open lobbies only
            options.Filters = new List<QueryFilter>()
            {
                new QueryFilter(
                field: QueryFilter.FieldOptions.AvailableSlots,
                op: QueryFilter.OpOptions.GT,
                value: "0")
            };

            // Order by newest lobbies first
            options.Order = new List<QueryOrder>()
            {
            new QueryOrder(
                asc: false,
                field: QueryOrder.FieldOptions.Created)
            };

            QueryResponse lobbies = await LobbyService.Instance.QueryLobbiesAsync(options);
            foreach (Lobby result in lobbies.Results)
            {
                {
                    RoomResult roomResult = Instantiate(roomResultPrefab, roomInstantiationArea.transform);
                    roomResult.Initialize(result);
                    roomResults.Add(roomResult);
                }
            }
        }
        catch (LobbyServiceException e)
        {
            connectionStatusText.text = e.Message;
        }
        FilterLobbies();
    }

    public void FilterLobbies()
    {
        string nameFilter = roomNameFilter.text.ToLower().Trim();
        string codeFilter = roomCodeFilter.text.ToLower().Trim();

        foreach (RoomResult room in roomResults)
        {
            bool nameMatch = true;
            if (!string.IsNullOrWhiteSpace(nameFilter) && !room.lobby.Name.ToLower().Contains(nameFilter))
            {
                nameMatch = false;
            }
            bool codeMatch = true;
            if (!string.IsNullOrWhiteSpace(codeFilter) && room.Code.ToLower() != codeFilter.ToLower())
            {
                codeMatch = false;
            }
            room.SetInteractable(multiplayerState == MultiplayerState.none);
            room.gameObject.SetActive(nameMatch && codeMatch);
        }
    }

    // === LOBBY SUBSCRIPTION EVENTS === //

    private void OnLobbyChanged(ILobbyChanges changes)
    {
        if (changes.LobbyDeleted)
        {
            leechedLobby = null;
            hostedLobby = null;
            StopHostingAndLeeching();
            connectionStatusText.text = "Room was closed or kicked.";
        }
        else
        {
            if (hostedLobby != null)
            {
                changes.ApplyToLobby(hostedLobby);
            }
            else if (leechedLobby != null)
            {
                changes.ApplyToLobby(leechedLobby);
                if (string.IsNullOrEmpty(relayCode))
                {
                    if (multiplayerState == MultiplayerState.leeching && leechedLobby.Data.ContainsKey("MatchedPlayer"))
                    {
                        if (leechedLobby.Data["MatchedPlayer"].Value == AuthenticationService.Instance.PlayerId)
                        {
                            connectionStatusText.text = "Joining game!";
                            ChangeMultiplayerState(MultiplayerState.blocked);
                            SceneManager.LoadScene("FightScene");
                        }
                        else
                        {
                            StopLeechingAsync();
                        }
                    }
                    if (leechedLobby.Data.ContainsKey("RelayCode") && !string.IsNullOrEmpty(leechedLobby.Data["RelayCode"].Value))
                    {
                        relayCode = leechedLobby.Data["RelayCode"].Value;
                        GameManager.remotePlayerName = leechedLobby.Players.Count.ToString();
                        foreach (var player in leechedLobby.Players)
                        {
                            if (player.Data.ContainsKey("Host") && player.Data.ContainsKey("Name"))
                            {
                                GameManager.remotePlayerName = player.Data["Name"].Value;
                                GameManager.remoteAvatar = player.Data["Avatar"].Value;
                            }
                        }
                    }
                }
            }
        }
    }

    private void OnPlayerJoined(List<LobbyPlayerJoined> joined)
    {
        if (multiplayerState == MultiplayerState.hosting)
        {
            try
            {
                foreach (var player in joined)
                {
                    if (player.Player.Id != AuthenticationService.Instance.PlayerId)
                    {
                        PlayerResult result = Instantiate(playerResultPrefab, playerInstantiationArea.transform);
                        result.Initialize(player);
                        playerResults.Add(result);
                    }
                }
            }
            catch (AuthenticationException e)
            {
                connectionStatusText.text = "Authentication error: " + e.Message;
            }
        }
    }

    private void OnPlayerLeft(List<int> left)
    {
        if (multiplayerState == MultiplayerState.hosting)
        {
            for (int i = playerResults.Count - 1; i >= 0; i--)
            {
                connectionStatusText.text = left[left.Count - 1].ToString();
                PlayerResult currentResult = playerResults[i];
                if (left.Contains(currentResult.playerIndex))
                {
                    playerResults.RemoveAt(i);
                    Destroy(currentResult.gameObject);
                }
            }
        }
    }

    private void OnPlayerDataAdded(Dictionary<int, Dictionary<string, ChangedOrRemovedLobbyValue<PlayerDataObject>>> dictionary)
    {
        if (multiplayerState == MultiplayerState.hosting)
        {
            foreach (PlayerResult result in playerResults)
            {
                if (dictionary.ContainsKey(result.playerIndex))
                {
                    Dictionary<string, ChangedOrRemovedLobbyValue<PlayerDataObject>> innerDict = dictionary[result.playerIndex];
                    result.UpdateData(innerDict);
                }
            }
        }
        
    }

    private void OnLobbyEventConnectionStateChanged(LobbyEventConnectionState state)
    {
        switch (state)
        {
            case LobbyEventConnectionState.Unsubscribed:
                connectionStatusText.text = "Room was closed or kicked.";
                hostedLobby = null;
                leechedLobby = null;
                StopHostingAndLeeching();
                break;
            case LobbyEventConnectionState.Subscribing:
                connectionStatusText.text = "Connecting to room...";
                break;
            case LobbyEventConnectionState.Subscribed:
                connectionStatusText.text = "Connected to room.";
                break;
            case LobbyEventConnectionState.Unsynced:
                hostedLobby = null;
                leechedLobby = null;
                StopHostingAndLeeching();
                break;
            case LobbyEventConnectionState.Error:
                hostedLobby = null;
                leechedLobby = null;
                StopHostingAndLeeching();
                connectionStatusText.text = "There was a connection error.";
                break;
            default: return;
        }
    }

}
