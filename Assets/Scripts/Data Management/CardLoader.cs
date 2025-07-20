using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;

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
            allCardsJSON = Resources.Load<TextAsset>("JSON/allCardsSingleton");
            var parsedJSON = JsonConvert.DeserializeObject<IDictionary<string, object>>(allCardsJSON.text);

            Debug.Log(parsedJSON.Count);
            allCardsVersion = Convert.ToInt16(parsedJSON["_version"]);
            Debug.Log(parsedJSON["cards"].GetType());
            IDictionary<string, object> parsedCards = parsedJSON["cards"] as IDictionary<string, object>;
            foreach (object card in parsedCards.Values)
            {
                IDictionary<string, object> cardData = card as IDictionary<string, object>;
                CardInfo newEntry = CardInfo.FromIDictionary((System.Collections.IDictionary)cardData);
                allCardsData[newEntry.index] = newEntry;
            }
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
