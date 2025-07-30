using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using UnityEditor.Rendering;

public class DeckBuilder : MonoBehaviour
{
    string activeDeckName;
    CardInfo.DeckList activeDeckList;

    // Prefabs
    [SerializeField] private DB_Card cardPrefab;

    // Linkages
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

    [SerializeField] VerticalLayoutGroup searchResultsArea;
    [SerializeField] TextMeshProUGUI deckValidText;
    [SerializeField] TextMeshProUGUI deckErrorText;
    [SerializeField] Color deckValidColor;
    [SerializeField] Color deckWarningColor;
    [SerializeField] Color deckErrorColor;

    [NonSerialized] public bool deckValid = false;
    [NonSerialized] public bool needsRefresh = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(LoadInitialDeck());
    }

    private void Update()
    {
        if (needsRefresh)
        {
            RefreshInfo();
            needsRefresh = false;
        }
    }

    private IEnumerator LoadInitialDeck()
    {
        while (CardLoader.instance != null && !CardLoader.instance.JSONLoaded)
        {
            yield return null;
        }
        activeDeckList = CardInfo.CreateRandomDeck();
        LoadDeck(activeDeckList);

        // Populate dropdown options
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
        foreach (string option in CardLoader.instance.allCardUnitTypes)
        {
            unitTypeDropdown.options.Add(new TMP_Dropdown.OptionData(option, null, Color.white));
        }
        RefreshInfo();
    }

    private void LoadDeck(CardInfo.DeckList deckList)
    {
        Debug.Log("Loading deck: " + deckList.deckName);

        rideReceiver.RemoveAllCards();
        for (int i = 0; i < CardInfo.DeckList.maxRide; i++)
        {
            DB_Card card = Instantiate<DB_Card>(cardPrefab, rideReceiver.transform);
            card.Load(deckList.rideDeck[i]);
            rideReceiver.ReceiveCard(card);
        }
        rideReceiver.AlignCards(true);

        mainReceiver.RemoveAllCards();
        for (int i = 0; i < CardInfo.DeckList.maxMain; i++)
        {
            DB_Card card = Instantiate<DB_Card>(cardPrefab, mainReceiver.transform);
            card.Load(deckList.mainDeck[i]);
            mainReceiver.ReceiveCard(card);
        }
        mainReceiver.AlignCards(true);

        strideReceiver.RemoveAllCards();
        for (int i = 0; i < CardInfo.DeckList.maxStride; i++)
        {
            DB_Card card = Instantiate<DB_Card>(cardPrefab, strideReceiver.transform);
            card.Load(deckList.strideDeck[i]);
            strideReceiver.ReceiveCard(card);
        }
        strideReceiver.AlignCards(true);

        toolboxReceiver.RemoveAllCards();
        for (int i = 0; i < CardInfo.DeckList.maxToolbox; i++)
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
        Dictionary<int, CardInfo> allCardsData = CardLoader.instance.allCardsData;
        int currentStep = 0;

        if (searchNation || searchType || searchGrade || searchRace || searchGroup || searchGift || searchQuery)
        {
            foreach (CardInfo cardInfo in allCardsData.Values)
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
                if (searchNation && cardInfo.nation != nation)
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

    private void RefreshInfo()
    {
        rideReceiver.label.text = rideReceiver.templateLabelText.Replace("[x]", rideReceiver.CardCount.ToString());
        mainReceiver.label.text = mainReceiver.templateLabelText.Replace("[x]", mainReceiver.CardCount.ToString());
        strideReceiver.label.text = strideReceiver.templateLabelText.Replace("[x]", strideReceiver.CardCount.ToString());
        toolboxReceiver.label.text = toolboxReceiver.templateLabelText.Replace("[x]", toolboxReceiver.CardCount.ToString());
        deckValid = CheckDeckValidity();
        if (deckValid)
        {
            deckValidText.text = "Deck is Valid.";
            deckValidText.color = deckValidColor;
        }
        else
        {
            deckValidText.text = "Deck is invalid.";
            deckValidText.color = deckErrorColor;
        }
        rideReceiver.AlignCards(false);
        mainReceiver.AlignCards(false);
        strideReceiver.AlignCards(false);
        toolboxReceiver.AlignCards(false);
    }

    private bool CheckDeckValidity()
    {
        if (rideReceiver.CardCount < 4  || rideReceiver.CardCount > 5)
        {
            deckErrorText.text = "Invalid number of cards in the Ride Deck.";
            deckErrorText.color = deckErrorColor;
            return false;
        }
        if (rideReceiver.CardCount == 5 && rideReceiver.GetCards()[0].cardInfo.index != 1676)
        {
            deckErrorText.text = "Only 'Griphosid' rideline may have 5 cards in the Ride Deck.";
            deckErrorText.color = deckErrorColor;
            return false;
        }
        if (mainReceiver.CardCount != 50)
        {
            deckErrorText.text = "Invalid number of cards in the Main Deck.";
            deckErrorText.color = deckErrorColor;
            return false;
        }
        deckErrorText.text = string.Empty;
        return true;
    }
}
