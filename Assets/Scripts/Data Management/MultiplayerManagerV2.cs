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
    public enum MultiplayerState { none, hosting, leeching, gaming };
    private MultiplayerState multiplayerState;
    private bool initialized = false;
    [SerializeField] private SceneLoadCanvas sceneLoadCanvas;

    // User fields
    [SerializeField] private Image userAvatar;
    [SerializeField] private TMP_InputField displayNameInputField;
    [SerializeField] private Toggle goFirstToggle;
    [SerializeField] private DeckSelectContainer player1DeckContainer;
    [SerializeField] private DeckSelectContainer player2DeckContainer;
    [SerializeField] private TMP_InputField roomCodeInputField;

    // User status displays
    [SerializeField] private TextMeshProUGUI gameVersionText;
    [SerializeField] private TextMeshProUGUI cardsVersionText;
    [SerializeField] private TextMeshProUGUI onlineStatusText;
    [SerializeField] private TextMeshProUGUI connectionStatusText;

    // Blocking areas
    [SerializeField] private CanvasGroup optionsArea;
    [SerializeField] private CanvasGroup userLobbyArea;
    [SerializeField] private CanvasGroup browseLobbiesArea;
    [SerializeField] private Image grayOutImage;

    // Major buttons
    [SerializeField] private Button startSingleplayerButton;
    [SerializeField] private Button startMultiplayerButton;
    [SerializeField] private Button stopMultiplayerButton;
    [SerializeField] private Button refreshLobbiesButton;

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
        if (instance != null)
        {
            instance = this;
        }
        else
        {
            DestroyImmediate(gameObject);
        }
    }

    private void Start()
    {
        sceneLoadCanvas.Hide();
        connectionStatusText.text = string.Empty;
        multiplayerState = MultiplayerState.none;
        ResetAllHosting();

        string lastProfileName = PlayerPrefs.GetString(SaveDataManager.lastProfileNameKey);
        if (!string.IsNullOrWhiteSpace(lastProfileName))
        {
            displayNameInputField.text = lastProfileName;
        }

        StartCoroutine(LoadInitialDisplay());
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

    private IEnumerator LoadInitialDisplay()
    {
        // Manually transition the scene in
        while (CardLoader.instance == null || !CardLoader.instance.CardsLoaded)
        {
            yield return null;
        }
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
            player1DeckContainer.deckSelectDropdown.options.Add(new TMP_Dropdown.OptionData(deckOption));
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
                userLobbyArea.interactable = false;
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

            case MultiplayerState.gaming:
                optionsArea.interactable = false;
                userLobbyArea.interactable = false;
                browseLobbiesArea.interactable = false;
                break;
        }
    }

    private async void ResetAllHosting()
    {
        if (hostedLobby != null)
        {
            try
            {
                await LobbyService.Instance.DeleteLobbyAsync(hostedLobby.Id);
            }
            catch (LobbyServiceException e)
            {
                // do not catch this message
            }
            hostedLobby = null;
        }
        if (leechedLobby != null)
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(leechedLobby.Id, AuthenticationService.Instance.PlayerId);
            }
            catch (LobbyServiceException e)
            {
                // do not catch this message
            }
            catch (AuthenticationException e)
            {
                // do not catch this message
            }
            leechedLobby = null;
        }
        ChangeMultiplayerState(MultiplayerState.none);

        // TODO: query for all connected rooms first?
    }

    private async void StartHostingAsync()
    {
        if (multiplayerState == MultiplayerState.none)
        {
            ChangeMultiplayerState(MultiplayerState.hosting);
            AssignLocalPlayerData();

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
                        value: roomCodeInputField.text,
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
            }
            catch (LobbyServiceException e)
            {
                connectionStatusText.text = "Lobby error: " + e.Message;
            }
        }
    }

    private async void StopHostingAsync()
    {
        if (hostedLobby != null)
        {
            try
            {
                await LobbyService.Instance.DeleteLobbyAsync(hostedLobby.Id);
            }
            catch (LobbyServiceException e)
            {
                connectionStatusText.text = "Lobby error: " + e.Message;
            }
            hostedLobby = null;
        }
        if (multiplayerState == MultiplayerState.hosting)
        {
            ChangeMultiplayerState(MultiplayerState.none);
        }
    }

    private async void StartLeechingAsync(RoomResult room)
    {
        if (multiplayerState == MultiplayerState.none)
        {
            ChangeMultiplayerState(MultiplayerState.leeching);
            AssignLocalPlayerData();

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
            }
            catch (LobbyServiceException e)
            {
                connectionStatusText.text = e.Message;
            }
        }
    }

    private async void StopLeechingAsync()
    {
        if (leechedLobby != null)
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(leechedLobby.Id, AuthenticationService.Instance.PlayerId);
            }
            catch (LobbyServiceException e)
            {
                connectionStatusText.text = "Lobby error: " + e.Message;
            }
            catch (AuthenticationException e)
            {
                connectionStatusText.text = "Authentication error: " + e.Message;
            }
            leechedLobby = null;
        }
        if (multiplayerState == MultiplayerState.leeching)
        {
            ChangeMultiplayerState(MultiplayerState.none);
        }
    }

    // === LOBBY SUBSCRIPTION EVENTS === //

    private void OnLobbyChanged(ILobbyChanges changes)
    {
        if (multiplayerState == MultiplayerState.hosting || multiplayerState == MultiplayerState.leeching)
        {
            if (changes.LobbyDeleted)
            {
                if (hostedLobby != null)
                {
                    connectionStatusText.text = "Room was closed or kicked.";
                }
                StopLeechingAsync();
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
                ResetAllHosting();
                connectionStatusText.text = "Disconnected from room.";
                break;
            case LobbyEventConnectionState.Subscribing:
                connectionStatusText.text = "Connecting to room...";
                break;
            case LobbyEventConnectionState.Subscribed:
                connectionStatusText.text = "Connected to room.";
                break;
            case LobbyEventConnectionState.Unsynced:
                ResetAllHosting();
                connectionStatusText.text = "Disconnected from room.";
                break;
            case LobbyEventConnectionState.Error:
                ResetAllHosting();
                connectionStatusText.text = "Disconnected from room.";
                break;
            default: return;
        }
    }

}
