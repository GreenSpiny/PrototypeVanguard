using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;
using Newtonsoft.Json.Linq;

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
            allCardsVersion = Convert.ToInt32(parsedJSON["_version"]);
            JObject token = parsedJSON["cards"] as JObject;
            Dictionary<string, JObject> parsedCards = token.ToObject<Dictionary<string, JObject>>();
            foreach (JObject card in parsedCards.Values)
            {
                Dictionary<string, object> cardData = card.ToObject<Dictionary<string, object>>();
                CardInfo newEntry = CardInfo.FromDictionary(cardData);
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
