using UnityEngine;
using System;
using System.IO;
using System.Globalization;

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

    public static void SaveDeck(CardInfo.DeckList deck)
    {
        string directory = DeckSaveLocation;
        string filePath = Path.Join(DeckSaveLocation, deck.deckName + ".json");
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        File.WriteAllText(filePath, deck.ToJSON());
        Debug.Log("Saving deck to: " + filePath);
    }


}
