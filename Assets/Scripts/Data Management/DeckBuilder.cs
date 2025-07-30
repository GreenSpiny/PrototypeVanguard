using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(LoadInitialDeck());
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
}
