using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

public class SaveDataManager : MonoBehaviour
{
    private static SaveDataManager instance;
    public static string DeckSaveLocation { get { return Path.Join(Application.persistentDataPath, "Decks"); } }

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
