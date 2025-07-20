using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public static class CardLoader
{
    // Card Loading
    private static TextAsset allCardsJSON;
    private static Dictionary<int, CardInfo> allCardsData = new Dictionary<int, CardInfo>();
    private static int allCardsVersion;
    private static bool allCardsLoaded;

    public static void Initialize()
    {
        if (!allCardsLoaded)
        {
            allCardsData.Clear();
            allCardsJSON = Resources.Load("JSON/allCardsSingleton.json") as TextAsset;
            var parsedJSON = JsonConvert.DeserializeObject<IDictionary<string, object>>(allCardsJSON.text);

            allCardsVersion = (int) parsedJSON["version"];

            IDictionary<string, object> parsedCards = parsedJSON["cards"] as IDictionary<string, object>;
            foreach (object card in parsedCards.Values)
            {
                IDictionary<string, object> cardData = card as IDictionary<string, object>;
                CardInfo newEntry = CardInfo.FromIDictionary((System.Collections.IDictionary)cardData);
                allCardsData[newEntry.index] = newEntry;
            }
            Debug.Log(allCardsData.Count);
            allCardsLoaded = true;
        }
    }

    public static CardInfo GetCardInfo(int cardIndex)
    {
        if (!allCardsData.ContainsKey(cardIndex))
        {
            return null;
        }
        return allCardsData[cardIndex];
    }

    // Image Loading

}
