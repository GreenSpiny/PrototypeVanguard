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

public class MultiplayerManager : MonoBehaviour
{
    public static MultiplayerManager instance;
    [SerializeField] SceneLoadCanvas sceneLoadCanvas;
    bool initialized = false;

    [SerializeField] TMP_InputField playerNameInput;
    [SerializeField] Toggle goFirstToggle;
    [SerializeField] Button singlePlayerStartButton;

    [SerializeField] DeckSelectContainer p1DeckContainer;
    [SerializeField] DeckSelectContainer p2DeckContainer;
    DeckSelectContainer[] deckSelectContainers;
    bool uiDirty = false;

    [SerializeField] TextMeshProUGUI gameVersionLabel;
    [SerializeField] TextMeshProUGUI cardsVersionLabel;
    [SerializeField] TextMeshProUGUI onlineStatusLabel;
    [SerializeField] Button hostMultiplayerButton;
    [SerializeField] TextMeshProUGUI hostMultiplayerButtonText;
    [SerializeField] TextMeshProUGUI multiplayerInfoText;

    [SerializeField] TMP_InputField roomNameFilter;
    [SerializeField] TMP_InputField roomCodeFilter;

    [SerializeField] RoomResult roomPrefab;
    [SerializeField] PlayerResult playerPrefab;
    [SerializeField] RectTransform roomsContainer;
    [SerializeField] RectTransform playersContainer;
    private List<RoomResult> roomResults = new List<RoomResult>();
    private List<PlayerResult> playerResults = new List<PlayerResult>();

    private Lobby hostedLobby = null;
    private Lobby leechedLobby = null;

    [System.Serializable]
    public class DeckSelectContainer
    {
        public TMP_Dropdown deckSelect;
        public TMP_Text deckValidation;
        [NonSerialized] public CardInfo.DeckList deckList;
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            DestroyImmediate(gameObject);
            return;
        }
        deckSelectContainers = new DeckSelectContainer[2] { p1DeckContainer, p2DeckContainer };
    }

    void Start()
    {
        sceneLoadCanvas.Hide();
        string lastProfileName = PlayerPrefs.GetString(SaveDataManager.lastProfileNameKey);
        if (!string.IsNullOrWhiteSpace(lastProfileName))
        {
            playerNameInput.text = lastProfileName;
        }
        multiplayerInfoText.text = string.Empty;
        StartCoroutine(LoadInitialDisplay());
    }

    private void Update()
    {
        if (uiDirty)
        {
            RefreshUI();
        }
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            UnityGameServices.instance.SwitchProfileAsync();
        }
    }

    private IEnumerator LoadInitialDisplay()
    {
        while (CardLoader.instance == null || !CardLoader.instance.CardsLoaded)
        {
            yield return null;
        }
        sceneLoadCanvas.TransitionIn();

        // Populate deck dropdown options
        p1DeckContainer.deckSelect.ClearOptions();
        p2DeckContainer.deckSelect.ClearOptions();

        List<string> deckOptions = SaveDataManager.GetAvailableDecks();
        if (deckOptions.Count == 0)
        {
            CardInfo.DeckList exampleDeck = SaveDataManager.GenerateExampleDeck();
            deckOptions.Add(exampleDeck.deckName);
        }
        foreach (string deckOption in deckOptions)
        {
            p1DeckContainer.deckSelect.options.Add(new TMP_Dropdown.OptionData(deckOption));
            p2DeckContainer.deckSelect.options.Add(new TMP_Dropdown.OptionData(deckOption));
        }
        if (p1DeckContainer.deckSelect.options.Count > 0)
        {
            int targetDeckIndex = 0;
            string lastViewedDecklist = PlayerPrefs.GetString(SaveDataManager.lastViewedDecklistKey);
            if (!string.IsNullOrEmpty(lastViewedDecklist))
            {
                for (int i = 0; i < p1DeckContainer.deckSelect.options.Count; i++)
                {
                    if (p1DeckContainer.deckSelect.options[i].text == lastViewedDecklist)
                    {
                        targetDeckIndex = i;
                        break;
                    }
                }
            }
            string targetDeck = p1DeckContainer.deckSelect.options[targetDeckIndex].text;
            p1DeckContainer.deckSelect.value = targetDeckIndex;
            p2DeckContainer.deckSelect.value = targetDeckIndex;
            p1DeckContainer.deckSelect.RefreshShownValue();
            p2DeckContainer.deckSelect.RefreshShownValue();
            p1DeckContainer.deckList = SaveDataManager.LoadDeck(targetDeck);
            p2DeckContainer.deckList = SaveDataManager.LoadDeck(targetDeck);
        }
        uiDirty = true;
        initialized = true;
        QueryLobbies();
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

            if (deckIndex >= 0 && deckIndex < targetContainer.deckSelect.options.Count)
            {
                string targetDeck = targetContainer.deckSelect.options[deckIndex].text;
                targetContainer.deckSelect.value = deckIndex;
                targetContainer.deckSelect.RefreshShownValue();
                targetContainer.deckList = SaveDataManager.LoadDeck(targetDeck);
                uiDirty = true;
            }
        }
    }

    public void OnPlayerNameChanged(string playerName)
    {
        if (!string.IsNullOrWhiteSpace(playerName))
        {
            PlayerPrefs.SetString(SaveDataManager.lastProfileNameKey, playerName);
        }
    }

    private void RefreshUI()
    {
        uiDirty = false;
        string error;
        bool[] decksValid = new bool[2] { false, false };
        bool anyDeckFound = true;

        for (int i = 0; i < deckSelectContainers.Length; i++)
        {
            DeckSelectContainer targetContainer = deckSelectContainers[i];
            if (targetContainer.deckSelect.options.Count == 0)
            {
                targetContainer.deckValidation.text = "No decks found.";
                targetContainer.deckSelect.interactable = false;
                anyDeckFound = false;
            }
            else if (!targetContainer.deckList.IsValid(out error))
            {
                targetContainer.deckValidation.text = "Deck is invalid.";
            }
            else
            {
                targetContainer.deckValidation.text = "Deck is valid.";
                decksValid[i] = true;
            }
        }

        if (hostedLobby != null || leechedLobby != null)
        {
            ToggleOffButtons();
        }
        else
        {
            goFirstToggle.interactable = anyDeckFound;
            deckSelectContainers[0].deckSelect.interactable = true;
            deckSelectContainers[1].deckSelect.interactable = true;
            singlePlayerStartButton.interactable = decksValid[0] && decksValid[1];
        }
        hostMultiplayerButton.interactable = leechedLobby == null && decksValid[0];

        gameVersionLabel.text = Application.version;
        cardsVersionLabel.text = CardLoader.instance.dataVersionObject.cardsFileVersion.ToString();
        onlineStatusLabel.text = string.Empty;
    }

    public void StartSingleplayerGame()
    {
        GameManager.singlePlayer = true;
        GameManager.localPlayerDecklist1 = p1DeckContainer.deckList;
        GameManager.localPlayerDecklist2 = p2DeckContainer.deckList;
        AssignLocalPlayerName();
        SceneManager.LoadScene("FightScene");
    }

    private void AssignLocalPlayerName()
    {
        if (!string.IsNullOrWhiteSpace(playerNameInput.text))
        {
            string sanitizedName = GameManager.SanitizeString(playerNameInput.text);
            if (sanitizedName.Length > playerNameInput.characterLimit)
            {
                sanitizedName = sanitizedName.Substring(0, playerNameInput.characterLimit);
            }
            GameManager.localPlayerName = sanitizedName;
        }
        else
        {
            GameManager.localPlayerName = "Player";
        }
    }

    public void HostMultiplayerButton()
    {
        if (hostedLobby == null)
        {
            HostMultiplayerRoom();
        }
        else
        {
            CancelMultiplayerRoom();
        }
    }

    private async void HostMultiplayerRoom()
    {
        GameManager.singlePlayer = false;
        GameManager.localPlayerDecklist1 = p1DeckContainer.deckList;
        ToggleOffButtons();
        AssignLocalPlayerName();
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
                    value: "abcd",
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

            hostMultiplayerButtonText.text = "hostMultiplayerButtonText.text = \"Host multiplayer room\";";
            multiplayerInfoText.text = "Waiting for players to join...";
        }
        catch (LobbyServiceException e)
        {
            multiplayerInfoText.text = e.Message;
        }
        uiDirty = true;
        QueryLobbies();
    }

    public async void CancelMultiplayerRoom()
    {
        if (hostedLobby != null)
        {
            hostMultiplayerButton.interactable = false;
            try
            {
                await LobbyService.Instance.DeleteLobbyAsync(hostedLobby.Id);
            }
            catch (LobbyServiceException e)
            {
                multiplayerInfoText.text = e.Message;
            }
            hostedLobby = null;
            uiDirty = true;
            QueryLobbies();
        }
        hostMultiplayerButtonText.text = "Host multiplayer room";
        ClearPlayers();
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

    public async void QueryLobbies()
    {
        ClearRooms();
        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions();
            options.Count = 25;

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
                    RoomResult roomResult = Instantiate(roomPrefab, roomsContainer.transform);
                    roomResult.Initialize(result);
                    roomResults.Add(roomResult);
                }
            }
        }
        catch (LobbyServiceException e)
        {
           multiplayerInfoText.text = e.Message;
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
            room.SetInteractable(hostedLobby == null);
            room.gameObject.SetActive(nameMatch && codeMatch);
        }
    }

    public async void StartLeeching(RoomResult room)
    {
        AssignLocalPlayerName();
        CancelMultiplayerRoom();
        ToggleOffButtons();
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
            callbacks.KickedFromLobby += OnKickedFromLobby;
            callbacks.LobbyEventConnectionStateChanged += OnLobbyEventConnectionStateChanged;
            var events = await LobbyService.Instance.SubscribeToLobbyEventsAsync(targetLobby.Id, callbacks);

            leechedLobby = targetLobby;
        }
        catch (LobbyServiceException e)
        {
            multiplayerInfoText.text = e.Message;
        }
        uiDirty = true;
    }

    public void StopLeeching()
    {
        if (leechedLobby != null)
        {
            string targetLobbyId = leechedLobby.Id;
            leechedLobby = null;
            try
            {
                string playerId = AuthenticationService.Instance.PlayerId;
                LobbyService.Instance.RemovePlayerAsync(targetLobbyId, playerId);
            }
            catch (AuthenticationException e)
            {
                multiplayerInfoText.text = e.Message;
            }
            catch (LobbyServiceException e)
            {
                multiplayerInfoText.text = e.Message;
            }
            QueryLobbies();
            uiDirty = true;
        }
    }

    private void ToggleOffButtons()
    {
        goFirstToggle.interactable = false;
        deckSelectContainers[0].deckSelect.interactable = false;
        deckSelectContainers[1].deckSelect.interactable = false;
        singlePlayerStartButton.interactable = false;
        hostMultiplayerButton.interactable = false;
    }

    private void OnApplicationQuit()
    {
        CancelMultiplayerRoom();
        StopLeeching();
    }

    // === LOBBY SUBSCRIPTION EVENTS === //

    private void OnLobbyChanged(ILobbyChanges changes)
    {
        if (changes.LobbyDeleted)
        {
            if (hostedLobby != null)
            {
                multiplayerInfoText.text = "Room was closed or kicked.";
            }
            StopLeeching();
        }
    }

    private void OnPlayerJoined(List<LobbyPlayerJoined> joined)
    {
        multiplayerInfoText.text = "Player joined! 1";
        if (hostedLobby != null)
        {
            try
            {
                foreach (var player in joined)
                {
                    multiplayerInfoText.text = "Player joined! 2";
                    if (player.Player.Id != AuthenticationService.Instance.PlayerId)
                    {
                        PlayerResult playerResult = Instantiate(playerPrefab, playersContainer.transform);
                        playerResult.Initialize(player.Player.Id);
                        playerResults.Add(playerResult);
                        //playerResult.SetData(player.Player.Data["Name"].Value, player.Player.Data["Avatar"].Value);
                    }
                }
            }
            catch (AuthenticationException e)
            {
                multiplayerInfoText.text = e.Message;
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
        multiplayerInfoText.text = "Room was closed or kicked.";
        StopLeeching();
    }

    private void OnLobbyEventConnectionStateChanged(LobbyEventConnectionState state)
    {
        switch (state)
        {
            case LobbyEventConnectionState.Unsubscribed:
                multiplayerInfoText.text = "Unsubscribed from host's room.";
                StopLeeching();
                break;
            case LobbyEventConnectionState.Subscribing:
                multiplayerInfoText.text = "Subscribing to room...";
                break;
            case LobbyEventConnectionState.Subscribed:
                multiplayerInfoText.text = "Subscribed to room...";
                break;
            case LobbyEventConnectionState.Unsynced:
                // multiplayerInfoText.text = "There was a connection issue.";
                StopLeeching();
                break;
            case LobbyEventConnectionState.Error:
                // multiplayerInfoText.text = "There was a connection issue.";
                StopLeeching();
                break;
            default: return;
        }
    }

}
