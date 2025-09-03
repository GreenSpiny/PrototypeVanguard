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
        BlockUIByState();

        string lastProfileName = PlayerPrefs.GetString(SaveDataManager.lastProfileNameKey);
        if (!string.IsNullOrWhiteSpace(lastProfileName))
        {
            displayNameInputField.text = lastProfileName;
        }

        StartCoroutine(LoadInitialDisplay());
    }

    private IEnumerator LoadInitialDisplay()
    {
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
        //uiDirty = true;
        initialized = true;
        //QueryLobbies();
    }

    private void BlockUIByState()
    {
        switch(multiplayerState)
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


    private async void StartHostingAsync()
    {
        multiplayerState = MultiplayerState.hosting;
        BlockUIByState();
    }

    private async void StopHostingAsync()
    {
        multiplayerState = MultiplayerState.none;
        BlockUIByState();
    }

    private async void StartLeechingAsync()
    {
        multiplayerState = MultiplayerState.leeching;
        BlockUIByState();
    }

    private async void StopLeechingAsync()
    {
        multiplayerState = MultiplayerState.none;
        BlockUIByState();
    }



}
