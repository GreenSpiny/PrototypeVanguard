using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MultiplayerManagerV2 : MonoBehaviour
{
    // Static elements
    public static MultiplayerManagerV2 instance;
    public static Lobby hostedLobby;
    public static Lobby leechedLobby;

    // Control elements
    public enum MultiplayerState { none, hosting, leeching, blocked };
    private MultiplayerState multiplayerState;
    private bool initialized = false;
    [SerializeField] private SceneLoadCanvas sceneLoadCanvas;
    [SerializeField] private RectTransform roomInstantiationArea;
    [SerializeField] private RectTransform playerInstantiationArea;

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

    // Major buttons
    [SerializeField] private Button startSingleplayerButton;
    [SerializeField] private Button startMultiplayerButton;
    [SerializeField] private Button stopMultiplayerButton;

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
    }

    private void Start()
    {
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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            UnityGameServices.instance.SwitchProfileAsync();
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
        }
        else
        {
            GameManager.localPlayerName = "Player";
        }
        GameManager.localPlayerDecklist1 = player1DeckContainer.deckList;
        GameManager.localPlayerDecklist2 = player2DeckContainer.deckList;
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
        while (CardLoader.instance == null || !CardLoader.instance.CardsLoaded)
        {
            yield return null;
        }
        yield return null;

        gameVersionText.text = Application.version;
        cardsVersionText.text = CardLoader.instance.dataVersionObject.cardsFileVersion.ToString();

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
            int targetDeckIndex = 0;
            string lastViewedDecklist = PlayerPrefs.GetString(SaveDataManager.lastViewedDecklistKey);
            if (!string.IsNullOrEmpty(lastViewedDecklist))
            {
                for (int i = 0; i < player1DeckContainer.deckSelectDropdown.options.Count; i++)
                {
                    if (player1DeckContainer.deckSelectDropdown.options[i].text == lastViewedDecklist)
                    {
                        targetDeckIndex = i;
                        break;
                    }
                }
            }
            string targetDeck = player1DeckContainer.deckSelectDropdown.options[targetDeckIndex].text;
            player1DeckContainer.deckSelectDropdown.value = targetDeckIndex;
            player2DeckContainer.deckSelectDropdown.value = targetDeckIndex;
            player1DeckContainer.deckSelectDropdown.RefreshShownValue();
            player2DeckContainer.deckSelectDropdown.RefreshShownValue();
            player1DeckContainer.deckList = SaveDataManager.LoadDeck(targetDeck);
            player2DeckContainer.deckList = SaveDataManager.LoadDeck(targetDeck);
        }

        // Complete initialization
        initialized = true;
        CheckValidity();
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

    private void ChangeMultiplayerState(MultiplayerState state)
    {
        multiplayerState = state;
        switch (multiplayerState)
        {
            case MultiplayerState.none:
                optionsArea.interactable = true;
                userLobbyArea.interactable = true;
                browseLobbiesArea.interactable = true;
                grayOutImage.gameObject.SetActive(false);
                startMultiplayerButton.gameObject.SetActive(true);
                stopMultiplayerButton.gameObject.SetActive(false);
                break;

            case MultiplayerState.hosting:
                optionsArea.interactable = false;
                userLobbyArea.interactable = true;
                browseLobbiesArea.interactable = false;
                grayOutImage.gameObject.SetActive(false);
                startMultiplayerButton.gameObject.SetActive(false);
                stopMultiplayerButton.gameObject.SetActive(true);
                break;

            case MultiplayerState.leeching:
                optionsArea.interactable = false;
                userLobbyArea.interactable = false;
                browseLobbiesArea.interactable = false;
                grayOutImage.gameObject.SetActive(true);
                startMultiplayerButton.gameObject.SetActive(true);
                stopMultiplayerButton.gameObject.SetActive(false);
                break;

            case MultiplayerState.blocked:
                optionsArea.interactable = false;
                userLobbyArea.interactable = false;
                browseLobbiesArea.interactable = false;
                break;
        }
    }

    public async void StartHostingAsync()
    {
        if (multiplayerState == MultiplayerState.none)
        {
            ChangeMultiplayerState(MultiplayerState.blocked);
            AssignLocalPlayerData();
            GameManager.singlePlayer = false;

            string lobbyName = GameManager.localPlayerName + "'s Lobby";
            int maxPlayers = 5;
            CreateLobbyOptions options = new CreateLobbyOptions();
            options.IsPrivate = false;
            options.Data = new Dictionary<string, DataObject>()
            {
                {
                    "GameVersion", new DataObject(
                        visibility: DataObject.VisibilityOptions.Public,
                        value: Application.version,
                        index: DataObject.IndexOptions.S1)
                },
                {
                    "CardsVersion", new DataObject(
                        visibility: DataObject.VisibilityOptions.Public,
                        value: CardLoader.instance.dataVersionObject.cardsFileVersion.ToString(),
                        index: DataObject.IndexOptions.N1)
                },
                {
                    "Code", new DataObject(
                        visibility: DataObject.VisibilityOptions.Public,
                        value: "abcd", //roomCodeInputField.text,
                        index: DataObject.IndexOptions.S2)
                },
                {
                    "Host", new DataObject(
                        visibility: DataObject.VisibilityOptions.Public,
                        value: AuthenticationService.Instance.PlayerId,
                        index: DataObject.IndexOptions.S3)
                }
            };

            try
            {
                hostedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);

                var callbacks = new LobbyEventCallbacks();
                callbacks.LobbyChanged += OnLobbyChanged;
                callbacks.PlayerJoined += OnPlayerJoined;
                callbacks.PlayerLeft += OnPlayerLeft;
                callbacks.KickedFromLobby += OnKickedFromLobby;
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
            QueryLobbies();
        }
    }

    public async void StartLeechingAsync(RoomResult room)
    {
        if (multiplayerState == MultiplayerState.none)
        {
            ChangeMultiplayerState(MultiplayerState.blocked);
            AssignLocalPlayerData();
            GameManager.singlePlayer = false;

            try
            {
                /*
                JoinLobbyByIdOptions options = new JoinLobbyByIdOptions();
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
                        value: "100")
                    }
                };
                */
                Lobby targetLobby = await LobbyService.Instance.JoinLobbyByIdAsync(room.lobby.Id);
                var callbacks = new LobbyEventCallbacks();
                callbacks.LobbyChanged += OnLobbyChanged;
                callbacks.PlayerLeft += OnPlayerLeft;
                callbacks.PlayerDataChanged += OnPlayerDataChanged;
                callbacks.KickedFromLobby += OnKickedFromLobby;
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
            QueryLobbies();
        }
    }

    public void StopHostingAndLeeching()
    {
        StopHostingAsync();
        StopLeechingAsync();
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
            room.SetInteractable(multiplayerState == MultiplayerState.leeching);
            room.gameObject.SetActive(nameMatch && codeMatch);
        }
    }

    // === LOBBY SUBSCRIPTION EVENTS === //

    private void OnLobbyChanged(ILobbyChanges changes)
    {
        if (changes.LobbyDeleted)
        {
            if (hostedLobby != null)
            {
                connectionStatusText.text = "Room was closed or kicked.";
            }
            StopHostingAndLeeching();
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
                        connectionStatusText.text = "Player joined! " + player.Player.Id;
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
        if (hostedLobby != null)
        {

        }
    }

    private void OnKickedFromLobby()
    {
        connectionStatusText.text = "Room was closed or kicked.";
        hostedLobby = null;
        leechedLobby = null;
        ChangeMultiplayerState(MultiplayerState.none);
    }

    private void OnPlayerDataChanged(Dictionary<int, Dictionary<string, ChangedOrRemovedLobbyValue<PlayerDataObject>>> dictionary)
    {
        
    }

    private void OnLobbyEventConnectionStateChanged(LobbyEventConnectionState state)
    {
        switch (state)
        {
            case LobbyEventConnectionState.Unsubscribed:
                StopHostingAndLeeching();
                connectionStatusText.text = "Disconnected from room.";
                break;
            case LobbyEventConnectionState.Subscribing:
                connectionStatusText.text = "Connecting to room...";
                break;
            case LobbyEventConnectionState.Subscribed:
                connectionStatusText.text = "Connected to room.";
                break;
            case LobbyEventConnectionState.Unsynced:
                StopHostingAndLeeching();
                connectionStatusText.text = "Disconnected from room.";
                break;
            case LobbyEventConnectionState.Error:
                StopHostingAndLeeching();
                connectionStatusText.text = "Disconnected from room.";
                break;
            default: return;
        }
    }

}
