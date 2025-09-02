using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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

    [SerializeField] TMP_InputField roomNameFilter;
    [SerializeField] TMP_InputField roomCodeFilter;

    [SerializeField] RoomResult roomPrefab;
    [SerializeField] RectTransform RoomsContainer;
    private List<RoomResult> roomResults = new List<RoomResult>();

    private Lobby lobby = null;

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
        StartCoroutine(LoadInitialDisplay());
    }

    private void Update()
    {
        if (uiDirty)
        {
            RefreshUI();
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
        
        goFirstToggle.interactable = lobby == null && anyDeckFound;
        deckSelectContainers[0].deckSelect.interactable = lobby == null;
        deckSelectContainers[1].deckSelect.interactable = lobby == null;
        singlePlayerStartButton.interactable = lobby == null && decksValid[0] && decksValid[1];
        hostMultiplayerButton.interactable = decksValid[0];

        gameVersionLabel.text = Application.version;
        cardsVersionLabel.text = CardLoader.instance.dataVersionObject.cardsFileVersion.ToString();
        onlineStatusLabel.text = string.Empty;
    }

    public void StartSingleplayerGame()
    {
        GameManager.singlePlayer = true;
        GameManager.localPlayerDecklist1 = p1DeckContainer.deckList;
        GameManager.localPlayerDecklist2 = p2DeckContainer.deckList;
        if (!string.IsNullOrWhiteSpace(playerNameInput.text))
        {
            string sanitizedName = GameManager.SanitizeString(playerNameInput.text);
            if (sanitizedName.Length > playerNameInput.characterLimit)
            {
                sanitizedName = sanitizedName.Substring(0, playerNameInput.characterLimit);
            }
            if (sanitizedName.Length > 13 && !sanitizedName.Contains(' '))
            {
                sanitizedName = sanitizedName.Insert(Mathf.CeilToInt(sanitizedName.Length / 2f), " ");
            }
            GameManager.localPlayerName = sanitizedName;
        }
        else
        {
            GameManager.localPlayerName = "Player";
        }
        SceneManager.LoadScene("FightScene");
    }

    public void HostMultiplayerButton()
    {
        if (lobby == null)
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

        goFirstToggle.interactable = false;
        deckSelectContainers[0].deckSelect.interactable = false;
        deckSelectContainers[1].deckSelect.interactable = false;
        singlePlayerStartButton.interactable = false;
        hostMultiplayerButton.interactable = false;

        string sanitizedName = GameManager.SanitizeString(playerNameInput.text);
        string lobbyName = sanitizedName + "'s Lobby";
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
        };

        lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);

        hostMultiplayerButtonText.text = "Hosting. Press to cancel room.";
        uiDirty = true;
        QueryLobbies();
    }

    private async void CancelMultiplayerRoom()
    {
        if (lobby != null)
        {
            hostMultiplayerButton.interactable = false;
            await LobbyService.Instance.DeleteLobbyAsync(lobby.Id);
            lobby = null;
        }
        hostMultiplayerButtonText.text = "Host multiplayer room";
        uiDirty = true;
        QueryLobbies();
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
                    RoomResult roomResult = Instantiate(roomPrefab, RoomsContainer.transform);
                    roomResult.Initialize(result);
                    roomResults.Add(roomResult);
                }
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
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
            if (!string.IsNullOrWhiteSpace(codeFilter) && room.lobby.LobbyCode != codeFilter)
            {
                codeMatch = false;
            }
            //room.SetInteractable(lobby == null);
            room.gameObject.SetActive(nameMatch && codeMatch);
        }
    }

}
