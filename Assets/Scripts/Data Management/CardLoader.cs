using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;
using Newtonsoft.Json.Linq;

public static class CardLoader
{
    private const string cardDataPath = "JSON/allCardsSingleton";
    private const string cardMaterialPath = "Materials/DefaultCardMaterial";
    private const string cardImagePath = "Card_Images/";

    // Material Loading
    private static Material defaultCardMaterial;

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
            allCardsJSON = Resources.Load<TextAsset>(cardDataPath);
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

            defaultCardMaterial = Resources.Load<Material>(cardMaterialPath);
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
    public static Dictionary<int, Material> allImagesData = new Dictionary<int, Material>();

    public static Material GetCardImage(int cardIndex)
    {
        if (allImagesData.ContainsKey(cardIndex))
        {
            return allImagesData[cardIndex];
        }
        else
        {
            int folderIndex = Mathf.FloorToInt(cardIndex / 100f);
            string texturePath = cardImagePath + folderIndex.ToString() + '/' + cardIndex.ToString();
            Texture cardTexture = Resources.Load<Texture>(texturePath);
            if (cardTexture != null)
            {
                Material newMaterial = new Material(defaultCardMaterial);
                newMaterial.mainTexture = cardTexture;
                allImagesData[cardIndex] = newMaterial;
                return newMaterial;
            }
            else
            {
                Debug.LogError("Failed to load card image with index: " + cardIndex.ToString());
                return null;
            }
        }
    }

}
