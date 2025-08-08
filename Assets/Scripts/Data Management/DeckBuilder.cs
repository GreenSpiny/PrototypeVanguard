using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    [SerializeField] Button deleteButton;
    [SerializeField] Button resetButton;

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
    [SerializeField] TMP_Dropdown nationAssignmentDropdown;

    [NonSerialized] public bool deckValid = false;
    private bool needsRefresh = false;
    private bool initialLoadComplete = false;
    private bool transitioningOut = false;
    private float transitionSpeed = 2f;

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
    }

    void Start()
    {
        mainCanvasGroup.alpha = 0f;
        mainCanvasGroup.blocksRaycasts = false;
        StartCoroutine(LoadInitialDeck());
    }

    private void Update()
    {
        if (needsRefresh)
        {
            RefreshInfo();
        }
        if (!transitioningOut && initialLoadComplete && mainCanvasGroup.alpha < 1f)
        {
            mainCanvasGroup.alpha = Mathf.Clamp(mainCanvasGroup.alpha + Time.deltaTime * transitionSpeed, 0f, 1f);
        }
        if (Input.GetKeyDown(KeyCode.Escape) && !transitioningOut)
        {
            transitioningOut = true;
        }
        if (transitioningOut)
        {
            mainCanvasGroup.alpha = Mathf.Clamp(mainCanvasGroup.alpha - Time.deltaTime * transitionSpeed, 0f, 1f);
            mainCanvasGroup.blocksRaycasts = false;
            if (mainCanvasGroup.alpha <= 0f)
            {
                SceneManager.LoadScene("MenuScene");
            }
        }
    }

    private IEnumerator LoadInitialDeck()
    {
        while (CardLoader.instance == null || !CardLoader.instance.CardsLoaded)
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
            if (option != "Nationless")
            {
                nationAssignmentDropdown.options.Add(new TMP_Dropdown.OptionData(option, null, Color.white));
            }
        }
        foreach (string option in CardLoader.instance.allCardRaces)
        {
            raceDropdown.options.Add(new TMP_Dropdown.OptionData(option, null, Color.white));
        }
        foreach (string option in CardLoader.instance.allCardUnitTypes)
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
            string targetDeck = deckDropdown.options[0].text;
            deckDropdown.value = 0;
            deckDropdown.RefreshShownValue();
            deckInputField.text = targetDeck;
            currentDeckList = SaveDataManager.LoadDeck(targetDeck);
            LoadDeck(currentDeckList);
        }
        else
        {
            currentDeckList = new CardInfo.DeckList();
            currentDeckList.deckName = "blank deck";
            deckDropdown.options.Add(new TMP_Dropdown.OptionData(currentDeckList.deckName));
            deckDropdown.RefreshShownValue();
            deckInputField.text = currentDeckList.deckName;
            LoadDeck(currentDeckList);
        }

        // Enable interaction
        deckDropdown.interactable = true;
        deckInputField.interactable = true;
        saveButton.interactable = true;
        saveAsButton.interactable = true;
        deleteButton.interactable = true;
        resetButton.interactable = true;
        giftDropdown.interactable = true;
        gradeDropdown.interactable = true;
        groupDropdown.interactable = true;
        nationDropdown.interactable = true;
        raceDropdown.interactable = true;
        unitTypeDropdown.interactable = true;
        queryInputField.interactable = true;
        resetFiltersButton.interactable = true;

        // Inspect default card
        yield return new WaitForEndOfFrame();
        cardDetailUI.InspectCard(null);

        // Refresh info
        RefreshInfo();
        initialLoadComplete = true;
        mainCanvasGroup.blocksRaycasts = true;
    }

    private void LoadDeck(CardInfo.DeckList deckList)
    {
        Debug.Log("Loading deck: " + deckList.deckName);

        nationAssignmentDropdown.value = 0;
        for (int i = 0; i < nationAssignmentDropdown.options.Count; i++)
        {
            if (nationAssignmentDropdown.options[i].text == deckList.nation)
            {
                nationAssignmentDropdown.value = i;
                break;
            }
        }
        nationAssignmentDropdown.RefreshShownValue();

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

        // Initiate search automatically on parameters changed
        if (searchCoroutine != null)
        {
            StopCoroutine(searchCoroutine);
        }
        searchCoroutine = StartCoroutine(Search());
    }

    private Coroutine searchCoroutine;
    private const int actionsPerFrame = 25;
    private List<CardInfo> searchResults = new List<CardInfo>();
    private List<DB_Card> searchCardObjects = new List<DB_Card>();
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

        string query = queryInputField.text.Trim();
        bool searchQuery = query.Length > 2;

        // Dig through the cards data. Do not do it all in one frame!
        searchResults.Clear();
        List<CardInfo> allCardsDataSorted = CardLoader.instance.allCardsDataSorted;
        int currentStep = 0;

        if (searchNation || searchType || searchGrade || searchRace || searchGroup || searchGift || searchQuery)
        {
            foreach (CardInfo cardInfo in allCardsDataSorted)
            {
                currentStep++;
                if (currentStep > actionsPerFrame)
                {
                    currentStep = 0;
                    yield return null;
                }

                if (searchQuery)
                {
                    if (!cardInfo.name.Contains(query, StringComparison.InvariantCultureIgnoreCase) && !cardInfo.effect.Contains(query, StringComparison.InvariantCultureIgnoreCase))
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
                if (searchRace && cardInfo.race != race)
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
                searchResults.Add(cardInfo);
            }
        }

        currentStep = 0;

        // Display results
        float targetWidth = searchResultsArea.GetComponent<RectTransform>().rect.width - searchResultsArea.padding.left - searchResultsArea.padding.right;
        while (searchCardObjects.Count < searchResults.Count)
        {
            currentStep++;
            if (currentStep > actionsPerFrame)
            {
                currentStep = 0;
                yield return null;
            }
            DB_Card newCardObject = Instantiate<DB_Card>(cardPrefab, searchResultsArea.transform);
            searchCardObjects.Add(newCardObject);
            newCardObject.SetWidth(targetWidth);
        }
        while (searchCardObjects.Count > searchResults.Count)
        {
            currentStep++;
            if (currentStep > actionsPerFrame)
            {
                currentStep = 0;
                yield return null;
            }
            DB_Card existingCard = searchCardObjects[searchCardObjects.Count - 1];
            searchCardObjects.RemoveAt(searchCardObjects.Count - 1);
            Destroy(existingCard.gameObject);
        }
        for (int i = 0; i < searchResults.Count; i++)
        {
            currentStep++;
            if (currentStep > actionsPerFrame)
            {
                currentStep = 0;
                yield return null;
            }
            CardInfo currentCardInfo = searchResults[i];
            searchCardObjects[i].Load(currentCardInfo.index);
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
        needsRefresh = false;
        rideReceiver.label.text = rideReceiver.templateLabelText.Replace("[x]", rideReceiver.cards.Count.ToString());
        mainReceiver.label.text = mainReceiver.templateLabelText.Replace("[x]", mainReceiver.cards.Count.ToString());
        strideReceiver.label.text = strideReceiver.templateLabelText.Replace("[x]", strideReceiver.cards.Count.ToString());
        toolboxReceiver.label.text = toolboxReceiver.templateLabelText.Replace("[x]", toolboxReceiver.cards.Count.ToString());

        currentDeckList = CreateDeck();
        string errorText = string.Empty;
        deckValid = currentDeckList.IsValid(out errorText);
        if (deckValid)
        {
            deckValidText.text = "Deck is Valid.";
            deckValidText.color = deckValidColor;
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

    private CardInfo.DeckList CreateDeck()
    {
        CardInfo.DeckList deck = new CardInfo.DeckList();
        deck.deckName = deckDropdown.options[deckDropdown.value].text;
        deck.nation = nationAssignmentDropdown.options[nationAssignmentDropdown.value].text;
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
        currentDeckList = CreateDeck();
        SaveDataManager.SaveDeck(currentDeckList);
    }

    public void SaveDeckAs()
    {
        currentDeckList = CreateDeck();
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
    }

    public void SwapDecks(int deckIndex)
    {
        if (deckIndex >= 0 && deckIndex < deckDropdown.options.Count)
        {
            string targetDeck = deckDropdown.options[deckIndex].text;
            deckDropdown.value = deckIndex;
            deckDropdown.RefreshShownValue();
            deckInputField.text = targetDeck;
            currentDeckList = SaveDataManager.LoadDeck(targetDeck);
            LoadDeck(currentDeckList);
        }
    }

    public void DeleteDeck()
    {
        int currentValue = deckDropdown.value;
        SaveDataManager.DeleteDeck(deckDropdown.options[currentValue].text);
        deckDropdown.options.RemoveAt(currentValue);
        if (currentValue != 0)
        {
            string targetDeck = deckDropdown.options[currentValue - 1].text;
            deckDropdown.value = currentValue - 1;
            deckDropdown.RefreshShownValue();
            deckInputField.text = targetDeck;
            currentDeckList = SaveDataManager.LoadDeck(targetDeck);
        }
        else
        {
            currentDeckList = new CardInfo.DeckList();
            currentDeckList.deckName = "blank deck";
            deckDropdown.options.Add(new TMP_Dropdown.OptionData(currentDeckList.deckName));
            deckDropdown.RefreshShownValue();
            deckInputField.text = currentDeckList.deckName;
            LoadDeck(currentDeckList);
        }

    }

    public void ResetDeck()
    {
        rideReceiver.RemoveAllCards();
        mainReceiver.RemoveAllCards();
        strideReceiver.RemoveAllCards();
        toolboxReceiver.RemoveAllCards();
        RefreshInfo();
    }
}
