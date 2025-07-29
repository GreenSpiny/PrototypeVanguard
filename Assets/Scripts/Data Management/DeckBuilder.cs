using System.Collections;
using UnityEngine;

public class DeckBuilder : MonoBehaviour
{
    string activeDeckName;
    CardInfo.DeckList activeDeckList;

    // Prefabs
    [SerializeField] private GameObject cardPrefab;

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
    }
}
