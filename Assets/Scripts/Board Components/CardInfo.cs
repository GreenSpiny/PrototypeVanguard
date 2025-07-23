using System.Collections;
using UnityEngine;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using Newtonsoft.Json.Linq;
using Unity.VisualScripting;

// CARDINFO contains detailed card information. It is purely for data storage.
public class CardInfo
{
    // Standard card elements --- o
    public readonly int count;
    public readonly int baseCrit;
    public readonly int baseDrive;
    public readonly string effect;
    public readonly string gift;
    public readonly int grade;
    public readonly string group;
    public readonly string id;
    // public readonly string image;
    public readonly int index;
    public readonly string name;
    public readonly string nation;
    public readonly int basePower;
    public readonly string race;
    // public readonly string regulation;
    public readonly int baseShield;
    public readonly string[] skills;
    public readonly string unitType;
    // public readonly string url;
    public readonly int version;

    public int powerModifier;
    public int shieldModifier;
    public int driveModifier;
    public int critModifier;
    public int power { get { return basePower + powerModifier; } set { powerModifier = value - basePower; } }
    public int shield { get { return baseShield + shieldModifier; } set { shieldModifier = value - baseShield; } }
    public int drive { get { return baseDrive + driveModifier; } set { driveModifier = value - baseDrive; } }
    public int crit { get { return baseCrit + critModifier; } set { critModifier = value - baseCrit; } }

    // Unique card elements --- o
    // Some cards have properties that necessitate additional actions be offered to either player.
    // These properties are treated as flags to keep offerings to a minimum.
    public readonly ActionFlag[] actionFlags;
    public enum ActionFlag
    {
        none,

        // DEFAULT ACTIONS
        power,          // POWER     (card)
        soul,           // TO SOUL   (card)
        botdeck,        // BOT DECK  (card)
        reveal,         // REVEAL    (card)
        view,           // SEARCH    (node)
        viewx,          // VIEW X    (node)
        revealx,        // REVEAL X  (node)
        ride,           // RIDE      (card)

        // SPECIAL ACTIONS
        armLeft,        // this card offers ARM LEFT
        armRight,       // this card offers ARM RIGHT
        bindFD,         // the owner can access BIND FD
        bindFDFoe,      // the opponent can access BIND FD (i.e. Blangdmire)
        gaugeZone,      // the owner obtains a Gauge Zone
        locking,        // both players can access LOCK
        overdress,      // the owner can access OV DRESS
        prison,         // the owner gains a Prison Zone and the opponent can access PRISON
        soulRC          // the owner's RC have SOUL access (i.e. Noblesse Gauge)
    }

    public CardInfo(int count, int baseCrit, int baseDrive, string effect, string gift, int grade, string group, string id, int index, string name, string nation, int basePower, string race, int baseShield, string[] skills, string unitType, int version)
    {
        this.count = count;
        this.baseCrit = baseCrit;
        this.baseDrive = baseDrive;
        this.effect = effect;
        this.gift = gift;
        this.grade = grade;
        this.group = group;
        this.id = id;
        this.index = index;
        this.name = name;
        this.nation = nation;
        this.basePower = basePower;
        this.race = race;
        this.baseShield = baseShield;
        this.skills = skills;
        this.unitType = unitType;
        this.version = version;
    }

    public static CardInfo GenerateDefaultCardInfo()
    {
        return new CardInfo(4, 1, 1, "effect", "", 1, "", "default", 0, "default", "Dark States", 8000, "Human", 5000, new string[0], "Normal Unit", 0);
    }

    public static CardInfo FromDictionary(Dictionary<string, object> dictionary)
    {
        JArray skillJArray = (JArray)dictionary["skill"];
        string[] skillArray = new string[skillJArray.Count];
        for (int i = 0; i < skillArray.Count(); i++)
        {
            skillArray[i] = skillJArray[i].ToObject<string>();
        }
        return new CardInfo(
            Convert.ToInt32(dictionary["count"]),
            Convert.ToInt32(dictionary["critical"]),
            Convert.ToInt32(dictionary["drive"]),
            Convert.ToString(dictionary["effect"]),
            Convert.ToString(dictionary["gift"]),
            Convert.ToInt32(dictionary["grade"]),
            Convert.ToString(dictionary["group"]),
            Convert.ToString(dictionary["id"]),
            Convert.ToInt32(dictionary["index"]),
            Convert.ToString(dictionary["name"]),
            Convert.ToString(dictionary["nation"]),
            Convert.ToInt32(dictionary["power"]),
            Convert.ToString(dictionary["race"]),
            Convert.ToInt32(dictionary["shield"]),
            skillArray,
            Convert.ToString(dictionary["type"]),
            Convert.ToInt32(dictionary["version"])
            );
    }

    /*
    public static string GetUnitTypeName(UnitType type)
    {
        switch (type)
        {
            case UnitType.normalUnit: return "Normal Unit";
            case UnitType.triggerUnit: return "Trigger Unit";
            case UnitType.gUnit: return "G Unit";
            case UnitType.normalOrder: return "Normal Order";
            case UnitType.setOrder: return "Set Order";
            case UnitType.blitzOrder: return "Blitz Order";
            case UnitType.crest: return "Crest";
            case UnitType.token: return "Token";
            case UnitType.ticket: return "Ticket";
            default: return "[missing]";
        }
    }
    */

    [System.Serializable]
    public class DeckList
    {
        public string deckName;     // Name of the deck.
        public int cardSleeves;     // ID linking to the card sleeves.

        public int[] mainDeck;      // MAIN DECK.                   50 cards max
        public int[] rideDeck;      // RIDE DECK.                   5  cards max
        public int[] strideDeck;    // STRIDE DECK.                 16 cards max
        public int[] toolbox;       // TOKENS, TICKETS, MARKERS.    30 cards max
                                    //                            = 101 total
        public DeckList()
        {

        }
        public DeckList(string deckName, int cardSleeves, int[] mainDeck, int[] rideDeck, int[] strideDeck, int[] toolbox)
        {
            this.deckName = deckName;
            this.cardSleeves = cardSleeves;
            this.mainDeck = mainDeck;
            this.rideDeck = rideDeck;
            this.strideDeck = strideDeck;
            this.toolbox = toolbox;
        }
    }

    public static DeckList CreateDeck(string JSON)
    {
       return JsonConvert.DeserializeObject<DeckList>(JSON);
    }

    public static DeckList CreateRandomDeck()
    {
        DeckList deck = new DeckList();
        deck.deckName = "random deck";
        deck.cardSleeves = 0;
        deck.mainDeck = new int[50];
        deck.rideDeck = new int[5];
        deck.strideDeck = new int[16];
        deck.toolbox = new int[30];
        for (int i = 0; i < deck.mainDeck.Count(); i++)
        {
            deck.mainDeck[i] = UnityEngine.Random.Range(0, 4000);
        }
        for (int i = 0; i < deck.rideDeck.Count(); i++)
        {
            deck.rideDeck[i] = UnityEngine.Random.Range(0, 4000);
        }
        for (int i = 0; i < deck.strideDeck.Count(); i++)
        {
            deck.strideDeck[i] = UnityEngine.Random.Range(0, 4000);
        }
        for (int i = 0; i < deck.toolbox.Count(); i++)
        {
            deck.toolbox[i] = UnityEngine.Random.Range(0, 4000);
        }
        return deck;
    }

}
