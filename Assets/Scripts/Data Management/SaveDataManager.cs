using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

public class SaveDataManager : MonoBehaviour
{
    private static SaveDataManager instance;
    private static string DeckSaveLocation { get { return Path.Join(Application.persistentDataPath, "Decks"); } }
    private static string VersionJSONSaveLocation { get { return Path.Join(Application.dataPath, "dataVersion.json"); } }
    private static string CardsJSONSaveLocation { get { return Path.Join(Application.dataPath, "allCardsSingleton.json"); } }

    private void Awake()
    {
        if (instance == null)
        { 
            instance = this;
        }
        else
        {
            enabled = false;
            DestroyImmediate(gameObject);
        }
    }

    // === UTILITY === ///

    public static void SaveVersionJSON(CardLoader.DataVersionObject data)
    {
        if (!Directory.Exists(Application.dataPath))
        {
            Directory.CreateDirectory(Application.dataPath);
        }
        string fileText = data.ToJSON();
        File.WriteAllText(VersionJSONSaveLocation, fileText);
    }

    public static CardLoader.DataVersionObject LoadVersionJSON()
    {
        string filePath = VersionJSONSaveLocation;
        if (File.Exists(filePath))
        {
            string fileText = File.ReadAllText(filePath);
            return CardLoader.DataVersionObject.FromJSON(fileText);
        }
        else
        {
            return new CardLoader.DataVersionObject();
        }
    }

    public static void SaveCardsJSON(string cardsJSON)
    {
        if (!Directory.Exists(Application.dataPath))
        {
            Directory.CreateDirectory(Application.dataPath);
        }
        File.WriteAllText(CardsJSONSaveLocation, cardsJSON);
    }

    public static string LoadCardsJSON()
    {
        string filePath = VersionJSONSaveLocation;
        if (File.Exists(filePath))
        {
            return File.ReadAllText(filePath);
        }
        else
        {
            return "{}";
        }
    }

    // === DECKS === //

    public static List<string> GetAvailableDecks()
    {
        List<string> result = new List<string>();
        if (Directory.Exists(DeckSaveLocation))
        {
            DirectoryInfo dInfo = new DirectoryInfo(DeckSaveLocation);
            var fInfos = dInfo.GetFiles();
            foreach (var fInfo in fInfos)
            {
                if (fInfo.Extension == ".json")
                {
                    result.Add(fInfo.Name.Substring(0, fInfo.Name.Length - 5));
                }
            }
        }
        return result;
    }

    public static CardInfo.DeckList LoadDeck(string deckName)
    {
        string filePath = Path.Join(DeckSaveLocation, deckName + ".json");
        if (File.Exists(filePath))
        {
            string fileText = File.ReadAllText(filePath);
            return CardInfo.DeckList.FromJSON(fileText);
        }
        return null;
    }

    public static void SaveDeck(CardInfo.DeckList deck)
    {
        string filePath = Path.Join(DeckSaveLocation, deck.deckName + ".json");
        if (!Directory.Exists(DeckSaveLocation))
        {
            Directory.CreateDirectory(DeckSaveLocation);
        }
        File.WriteAllText(filePath, deck.ToJSON());
        Debug.Log("Saving deck to: " + filePath);
    }

    public static void DeleteDeck(string deckName)
    {
        string filePath = Path.Join(DeckSaveLocation, deckName + ".json");
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        Debug.Log("Deleting deck at: " + filePath);
    }

    public static void RenameDeck(CardInfo.DeckList deck, string oldDeckName)
    {
        SaveDeck(deck);
        DeleteDeck(oldDeckName);
    }


}
