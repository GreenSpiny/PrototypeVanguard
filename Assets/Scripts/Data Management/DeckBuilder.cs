using System.Collections;
using UnityEngine;

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
