using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static CardInfo;

public class DeckBuilder : MonoBehaviour
{
    public static DeckBuilder instance;
    [NonSerialized] public CardInfo.DeckList currentDeckList;

    // Prefabs
    [SerializeField] private DB_Card cardPrefab;
    [SerializeField] public CardDetailUI cardDetailUI;

    // Linkages
    [SerializeField] TMP_Dropdown deckDropdown;
    [SerializeField] TMP_InputField deckInputField;
    [SerializeField] Button saveButton;
    [SerializeField] Button saveAsButton;
    [SerializeField] Button renameButton;
    [SerializeField] CanvasGroup pasteFeedbackGroup;
    private TextMeshProUGUI pasteFeedbackText;

    [SerializeField] DB_CardReciever rideReceiver;
    [SerializeField] DB_CardReciever mainReceiver;
    [SerializeField] DB_CardReciever strideReceiver;
    [SerializeField] DB_CardReciever toolboxReceiver;

    [SerializeField] TMP_Dropdown giftDropdown;
    [SerializeField] TMP_Dropdown gradeDropdown;
    [SerializeField] TMP_Dropdown groupDropdown;
    [SerializeField] TMP_Dropdown nationDropdown;
    [SerializeField] TMP_Dropdown raceDropdown;
    [SerializeField] TMP_Dropdown unitTypeDropdown;
    [SerializeField] TMP_InputField queryInputField;
    [SerializeField] Button resetFiltersButton;

    [SerializeField] VerticalLayoutGroup searchResultsArea;
    [SerializeField] TextMeshProUGUI deckValidText;
    [SerializeField] Color deckValidColor;
    [SerializeField] Color deckWarningColor;
    [SerializeField] Color deckErrorColor;

    [SerializeField] CanvasGroup mainCanvasGroup;
    [SerializeField] TextMeshProUGUI nationAssignment;
    [SerializeField] SceneLoadCanvas sceneLoadCanvas;

    [NonSerialized] public bool deckValid = false;
    private bool needsRefresh = false;
    private bool needsSearch = false;
    private bool initialLoadComplete = false;
    private const string blankDeckName = "blank deck";

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
        pasteFeedbackText = pasteFeedbackGroup.GetComponent<TextMeshProUGUI>();
        pasteFeedbackGroup.alpha = 0f;
    }

    void Start()
    {
        sceneLoadCanvas.Hide();
        cardDetailUI.DisableActionLog();
        cardDetailUI.DisableChat();
        StartCoroutine(LoadInitialDeck());
    }

    private void Update()
    {
        if (needsRefresh)
        {
            needsRefresh = false;
            RefreshInfo();
        }
        if (needsSearch)
        {
            needsSearch = false;
            ClearSearchResults();
            if (searchCoroutine != null)
            {
                StopCoroutine(searchCoroutine);
            }
            searchCoroutine = StartCoroutine(Search());
        }
        if (initialLoadComplete)
        {
            sceneLoadCanvas.TransitionIn();
        }
        if (pasteFeedbackGroup.alpha > 0)
        {
            pasteFeedbackGroup.alpha = Mathf.Clamp(pasteFeedbackGroup.alpha - Time.deltaTime, 0f, 1f);
        }
    }

    private void StatusAlert(string message)
    {
        pasteFeedbackGroup.alpha = 1f;
        pasteFeedbackText.text = message;
    }

    private IEnumerator LoadInitialDeck()
    {
        while (!CardLoader.CardsLoaded)
        {
            yield return null;
        }

        // Populate search dropdown options
        foreach (string option in CardLoader.instance.allCardGifts)
        {
            giftDropdown.options.Add(new TMP_Dropdown.OptionData(option, null, Color.white));
        }
        foreach (int option in CardLoader.instance.allCardGrades)
        {
            gradeDropdown.options.Add(new TMP_Dropdown.OptionData(option.ToString(), null, Color.white));
        }
        foreach (string option in CardLoader.instance.allCardGroups)
        {
            groupDropdown.options.Add(new TMP_Dropdown.OptionData(option, null, Color.white));
        }
        foreach (string option in CardLoader.instance.allCardNations)
        {
            nationDropdown.options.Add(new TMP_Dropdown.OptionData(option, null, Color.white));
        }
        foreach (string option in CardLoader.instance.allCardRaces)
        {
            raceDropdown.options.Add(new TMP_Dropdown.OptionData(option, null, Color.white));
        }
        List<string> improvedUnitTypes = new List<string>(CardLoader.instance.allCardUnitTypes);
        improvedUnitTypes.Add("Sentinel");
        improvedUnitTypes.Add("Regalis");
        improvedUnitTypes.Sort();
        foreach (string option in improvedUnitTypes)
        {
            unitTypeDropdown.options.Add(new TMP_Dropdown.OptionData(option, null, Color.white));
        }

        // Populate deck dropdown options
        List<string> deckOptions = SaveDataManager.GetAvailableDecks();
        foreach (string deckOption in deckOptions)
        {
            deckDropdown.options.Add(new TMP_Dropdown.OptionData(deckOption));
        }
        if (deckDropdown.options.Count > 0)
        {
            int targetDeckIndex = 0;
            string lastViewedDecklist = PlayerPrefs.GetString(SaveDataManager.player1DecklistKey);
            if (!string.IsNullOrWhiteSpace(lastViewedDecklist))
            {
                for (int i = 0; i < deckDropdown.options.Count; i++)
                {
                    if (deckDropdown.options[i].text == lastViewedDecklist)
                    {
                        targetDeckIndex = i;
                        break;
                    }
                }
            }
            string targetDeck = deckDropdown.options[targetDeckIndex].text;
            deckDropdown.value = targetDeckIndex;
            deckDropdown.RefreshShownValue();
            currentDeckList = SaveDataManager.LoadDeck(targetDeck);
            LoadDeck(currentDeckList, true);
        }
        else
        {
            currentDeckList = SaveDataManager.GenerateExampleDeck();
            deckDropdown.options.Add(new TMP_Dropdown.OptionData(currentDeckList.deckName));
            deckDropdown.value = 0;
            deckDropdown.RefreshShownValue();
            LoadDeck(currentDeckList, true);
        }

        // Inspect default card
        yield return new WaitForEndOfFrame();
        cardDetailUI.InspectCard(null);

        // Refresh info
        initialLoadComplete = true;
        mainCanvasGroup.blocksRaycasts = true;
        OnDeckInputFieldChanged();
        RefreshInfo();
    }

    private void LoadDeck(CardInfo.DeckList deckList, bool adjustSelection)
    {
        if (adjustSelection)
        {
            PlayerPrefs.SetString(SaveDataManager.player1DecklistKey, deckList.deckName);
            deckInputField.text = deckList.deckName;
        }

        rideReceiver.RemoveAllCards();
        for (int i = 0; i < Mathf.Min(deckList.rideDeck.Length, CardInfo.DeckList.maxRide); i++)
        {
            DB_Card card = Instantiate<DB_Card>(cardPrefab, rideReceiver.transform);
            card.Load(deckList.rideDeck[i]);
            rideReceiver.ReceiveCard(card);
        }
        rideReceiver.AlignCards(true);

        mainReceiver.RemoveAllCards();
        for (int i = 0; i < Mathf.Min(deckList.mainDeck.Length, CardInfo.DeckList.maxMain); i++)
        {
            DB_Card card = Instantiate<DB_Card>(cardPrefab, mainReceiver.transform);
            card.Load(deckList.mainDeck[i]);
            mainReceiver.ReceiveCard(card);
        }
        mainReceiver.AlignCards(true);

        strideReceiver.RemoveAllCards();
        for (int i = 0; i < Mathf.Min(deckList.strideDeck.Length, CardInfo.DeckList.maxStride); i++)
        {
            DB_Card card = Instantiate<DB_Card>(cardPrefab, strideReceiver.transform);
            card.Load(deckList.strideDeck[i]);
            strideReceiver.ReceiveCard(card);
        }
        strideReceiver.AlignCards(true);

        toolboxReceiver.RemoveAllCards();
        for (int i = 0; i < Mathf.Min(deckList.toolbox.Length, CardInfo.DeckList.maxToolbox); i++)
        {
            DB_Card card = Instantiate<DB_Card>(cardPrefab, toolboxReceiver.transform);
            card.Load(deckList.toolbox[i]);
            toolboxReceiver.ReceiveCard(card);
        }
        toolboxReceiver.AlignCards(true);
        currentDeckList = deckList;
        OnDeckInputFieldChanged();
    }

    public void CopyDeck()
    {
        currentDeckList = CreateDeck(currentDeckList.deckName);
        string outputstring = currentDeckList.ToJSON();
        outputstring += "\n\nRide Deck:";
        foreach (int cardIndex in currentDeckList.rideDeck)
        {
            outputstring += "\n- " + CardLoader.GetCardInfo(cardIndex).name;
        }
        outputstring += "\n\nMain Deck:";
        outputstring += StackCardsCopyUtility(currentDeckList.mainDeck);

        if (currentDeckList.strideDeck.Length > 0)
        {
            outputstring += "\n\nStride Deck:";
            outputstring += StackCardsCopyUtility(currentDeckList.strideDeck);
        }

        GUIUtility.systemCopyBuffer = outputstring;
        StatusAlert("Copied deck to clipboard!");
    }

    private string StackCardsCopyUtility(int[] cards)
    {
        string output = string.Empty;
        int currentCard = int.MinValue;
        int currentCount = 0;
        foreach (int cardIndex in cards)
        {
            if (cardIndex != currentCard)
            {
                if (currentCard >= 0)
                {
                    output += "\n" + currentCount.ToString() + "x " + CardLoader.GetCardInfo(currentCard).name;
                }
                currentCount = 1;
                currentCard = cardIndex;
            }
            else
            {
                currentCount++;
            }
        }
        if (currentCount > 0)
        {
            output += "\n" + currentCount.ToString() + "x " + CardLoader.GetCardInfo(currentCard).name;
        }
        return output;
    }

    public void PasteDeck()
    {
        pasteFeedbackText.text = "Pasted deck from clipboard.";
        pasteFeedbackGroup.alpha = 1f;
        try
        {
            string input = GUIUtility.systemCopyBuffer;
            string selection = input.Substring(input.IndexOf('{'), input.IndexOf('}') + 1);
            CardInfo.DeckList newList = CardInfo.DeckList.FromJSON(selection);
            LoadDeck(newList, false);
        }
        catch (Exception e)
        {
            pasteFeedbackText.text = "Unable to parse clipboard input.";
        }
    }

    public void ResetFilters()
    {
        nationDropdown.value = 0;
        unitTypeDropdown.value = 0;
        gradeDropdown.value = 0;
        raceDropdown.value = 0;
        groupDropdown.value = 0;
        giftDropdown.value = 0;
        queryInputField.text = string.Empty;
        OnFilterChanged();
    }

    public void OnFilterChanged()
    {
        string type = unitTypeDropdown.options[unitTypeDropdown.value].text;
        bool searchUnits = unitTypeDropdown.value == 0 || type.Contains("Unit", StringComparison.InvariantCultureIgnoreCase);
        if (!searchUnits)
        {
            raceDropdown.value = 0;
            groupDropdown.value = 0;
        }
        raceDropdown.interactable = searchUnits;
        groupDropdown.interactable = searchUnits;
        bool searchTriggers = unitTypeDropdown.value == 0 || type.Contains("Trigger", StringComparison.InvariantCultureIgnoreCase);
        if (!searchTriggers)
        {
            giftDropdown.value = 0;
        }
        giftDropdown.interactable = searchTriggers;

        needsSearch = true;
    }

    private Coroutine searchCoroutine;
    private List<DB_Card> searchCardObjects = new List<DB_Card>();

    private void ClearSearchResults()
    {
        for (int i = 0; i < searchCardObjects.Count; i++)
        {
            searchCardObjects[i].gameObject.SetActive(false);
        }
    }

    private IEnumerator Search()
    {
        // Get search parameters
        string nation = nationDropdown.options[nationDropdown.value].text;
        bool searchNation = nationDropdown.value != 0;

        string type = unitTypeDropdown.options[unitTypeDropdown.value].text;
        bool searchType = unitTypeDropdown.value != 0;

        int grade = 0;
        bool searchGrade = gradeDropdown.value != 0;
        if (searchGrade)
        {
            grade = Convert.ToInt32(gradeDropdown.options[gradeDropdown.value].text);
        }

        string race = raceDropdown.options[raceDropdown.value].text;
        bool searchRace = raceDropdown.value != 0;

        string group = groupDropdown.options[groupDropdown.value].text;
        bool searchGroup = groupDropdown.value != 0;

        string gift = giftDropdown.options[giftDropdown.value].text;
        bool searchGift = giftDropdown.value != 0;

        string query = GameManager.SimplifyString(queryInputField.text);
        bool searchQuery = query.Length > 0;

        bool shouldSearch = searchNation || searchType || searchGrade || searchRace || searchGroup || searchGift || searchQuery;
        if (!shouldSearch)
        {
            yield break;
        }

        // Dig through the cards data.
        float targetWidth = searchResultsArea.GetComponent<RectTransform>().rect.width - searchResultsArea.padding.left - searchResultsArea.padding.right;
        List<CardInfo> allCardsDataSorted = CardLoader.instance.allCardsDataSorted;
        int resultCount = 0;
        if (searchNation || searchType || searchGrade || searchRace || searchGroup || searchGift || searchQuery)
        {
            foreach (CardInfo cardInfo in allCardsDataSorted)
            {
                if (string.IsNullOrEmpty(cardInfo.regulation))
                {
                    continue;
                }
                if (searchQuery)
                {
                    bool nameMatch = cardInfo.strippedName.Contains(query, StringComparison.InvariantCultureIgnoreCase);
                    bool effectMatch = cardInfo.strippedEffect.Contains(query, StringComparison.InvariantCultureIgnoreCase);
                    if (!(nameMatch || effectMatch))
                    {
                        continue;
                    }
                }
                if (searchNation && !cardInfo.nation.Contains(nation))
                {
                    continue;
                }
                if (searchType && cardInfo.unitType != type)
                {
                    continue;
                }
                if (searchGrade && cardInfo.grade != grade)
                {
                    continue;
                }
                if (searchRace && !cardInfo.race.Contains(race))
                {
                    continue;
                }
                if (searchGroup && cardInfo.group != group)
                {
                    continue;
                }
                if (searchGift && cardInfo.gift != gift)
                {
                    continue;
                }

                // Add matching result to active objects
                CardInfo successfulResult = cardInfo;
                if (resultCount >= searchCardObjects.Count)
                {
                    DB_Card newCardObject = Instantiate<DB_Card>(cardPrefab, searchResultsArea.transform);
                    searchCardObjects.Add(newCardObject);
                }
                DB_Card targetCard = searchCardObjects[resultCount];
                targetCard.Load(cardInfo.index);
                targetCard.SetWidth(targetWidth);
                targetCard.gameObject.SetActive(true);
                resultCount++;
                yield return null;
            }
        }

        for (int i = resultCount; i < searchCardObjects.Count; i++)
        {
            searchCardObjects[i].gameObject.SetActive(false);
        }

        // End coroutine
        searchCoroutine = null;
    }

    public void SetDirty()
    {
        needsRefresh = true;
    }

    private void RefreshInfo()
    {
        rideReceiver.label.text = rideReceiver.templateLabelText.Replace("[x]", rideReceiver.cards.Count.ToString());
        mainReceiver.label.text = mainReceiver.templateLabelText.Replace("[x]", mainReceiver.cards.Count.ToString());
        strideReceiver.label.text = strideReceiver.templateLabelText.Replace("[x]", strideReceiver.cards.Count.ToString());
        toolboxReceiver.label.text = toolboxReceiver.templateLabelText.Replace("[x]", toolboxReceiver.cards.Count.ToString());

        currentDeckList = CreateDeck(currentDeckList.deckName);
        string errorText = string.Empty;
        deckValid = currentDeckList.IsValid(out errorText);
        nationAssignment.text = currentDeckList.nation;
        if (deckValid)
        {
            deckValidText.text = "Deck is Valid.";
            deckValidText.color = deckValidColor;
            if (currentDeckList.nation == "Touken Ranbu")
            {
                deckValidText.text += "\nIt appears you are playing the Touken Ranbu nation, which breaks all conventional deckbuilding rules. Rideline validation is not currently supported.";
                deckValidText.color = deckWarningColor;
            }
            /* Energy Generator check
            if (!currentDeckList.toolbox.Contains(1259))
            {
                deckValidText.text += "\nWarning: there is no 'Energy Generator' in the Toolbox.";
                deckValidText.color = deckWarningColor;
            }
            */
        }
        else
        {
            deckValidText.text = "Deck is invalid.\n" + errorText;
            deckValidText.color = deckErrorColor;
        }
        rideReceiver.AlignCards(false);
        mainReceiver.AlignCards(false);
        strideReceiver.AlignCards(false);
        toolboxReceiver.AlignCards(false);
    }

    private CardInfo.DeckList CreateDeck(string deckName)
    {
        CardInfo.DeckList deck = new CardInfo.DeckList();
        deck.deckName = deckName;
        deck.nation = nationAssignment.text;
        deck.rideDeck = new int[rideReceiver.cards.Count];
        for (int i = 0; i < rideReceiver.cards.Count; i++)
        {
            deck.rideDeck[i] = rideReceiver.cards[i].cardInfo.index;
        }
        deck.mainDeck = new int[mainReceiver.cards.Count];
        for (int i = 0; i < mainReceiver.cards.Count; i++)
        {
            deck.mainDeck[i] = mainReceiver.cards[i].cardInfo.index;
        }
        deck.strideDeck = new int[strideReceiver.cards.Count];
        for (int i = 0; i < strideReceiver.cards.Count; i++)
        {
            deck.strideDeck[i] = strideReceiver.cards[i].cardInfo.index;
        }
        deck.toolbox = new int[toolboxReceiver.cards.Count];
        for (int i = 0; i < toolboxReceiver.cards.Count; i++)
        {
            deck.toolbox[i] = toolboxReceiver.cards[i].cardInfo.index;
        }
        return deck;
    }

    public void SaveDeck()
    {
        currentDeckList = CreateDeck(currentDeckList.deckName);
        SaveDataManager.SaveDeck(currentDeckList);
        StatusAlert("Saved deck.");
    }

    public void SaveDeckAs()
    {
        currentDeckList = CreateDeck(deckInputField.text);
        SaveDataManager.SaveDeck(currentDeckList);

        bool found = false;
        for (int i = 0; i < deckDropdown.options.Count; i++)
        {
            var currentOption = deckDropdown.options[i];
            if (currentOption.text == currentDeckList.deckName)
            {
                deckDropdown.value = i;
                found = true;
                break;
            }
        }
        if (!found)
        {
            deckDropdown.options.Add(new TMP_Dropdown.OptionData(currentDeckList.deckName));
            deckDropdown.value = deckDropdown.options.Count - 1;
        }

        deckDropdown.RefreshShownValue();
        StatusAlert("Saved deck.");
    }

    public void SwapDecks(int deckIndex)
    {
        if (initialLoadComplete)
        {
            if (deckIndex >= 0 && deckIndex < deckDropdown.options.Count)
            {
                string targetDeck = deckDropdown.options[deckIndex].text;
                deckDropdown.value = deckIndex;
                deckDropdown.RefreshShownValue();
                currentDeckList = SaveDataManager.LoadDeck(targetDeck);
                LoadDeck(currentDeckList, true);
            }
        }
    }

    public void DeleteDeck()
    {
        int currentValue = deckDropdown.value;
        SaveDataManager.DeleteDeck(deckDropdown.options[currentValue].text);
        deckDropdown.options.RemoveAt(currentValue);

        if (deckDropdown.options.Count == 0)
        {
            currentDeckList = new CardInfo.DeckList();
            currentDeckList.deckName = blankDeckName;
            deckDropdown.options.Add(new TMP_Dropdown.OptionData(currentDeckList.deckName));
            deckDropdown.RefreshShownValue();
            SaveDataManager.SaveDeck(currentDeckList);
            ResetDeck();
        }
        else
        {
            if (currentValue >= deckDropdown.options.Count)
            {
                currentValue = deckDropdown.options.Count - 1;
                deckDropdown.value = currentValue;
                deckDropdown.RefreshShownValue();
            }
            else
            {
                SwapDecks(currentValue);
            }
            
        }
        StatusAlert("Deleted deck.");
    }

    public void RenameDeck()
    {
        int currentValue = deckDropdown.value;
        if (deckDropdown.options[currentValue].text != deckInputField.text)
        {
            SaveDataManager.DeleteDeck(deckDropdown.options[currentValue].text);
            deckDropdown.options.RemoveAt(currentValue);
            SaveDeckAs();
        }
        StatusAlert("Renamed deck.");
    }

    public void ResetDeck()
    {
        CardInfo.DeckList blankList = new CardInfo.DeckList();
        blankList.deckName = currentDeckList.deckName;
        LoadDeck(blankList, true);
    }

    public void OnDeckInputFieldChanged()
    {
        string currentDropdownName = deckDropdown.options[deckDropdown.value].text;
        string currentInputText = deckInputField.text;

        saveButton.interactable = string.IsNullOrWhiteSpace(deckInputField.text) || currentInputText == currentDropdownName;
        saveAsButton.interactable = !saveButton.interactable;
        renameButton.interactable = !saveButton.interactable;
    }
}
