using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    bool singlePlayerDirty = false;

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
        if (singlePlayerDirty)
        {
            RefreshSingleplayer();
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
        singlePlayerDirty = true;
        initialized = true;
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
                singlePlayerDirty = true;
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

    private void RefreshSingleplayer()
    {
        singlePlayerDirty = false;
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
        
        goFirstToggle.interactable = anyDeckFound;
        singlePlayerStartButton.interactable = decksValid[0] && decksValid[1];
    }

    public void StartSingleplayerGame()
    {
        GameManager.singlePlayer = true;
        GameManager.localPlayerDecklist1 = p1DeckContainer.deckList;
        GameManager.localPlayerDecklist2 = p2DeckContainer.deckList;
        if (!string.IsNullOrWhiteSpace(playerNameInput.text))
        {
            string sanitizedName = playerNameInput.text.Trim();
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
}
