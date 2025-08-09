using System.Collections;
using UnityEngine;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using Newtonsoft.Json.Linq;
using Unity.VisualScripting;

// CARDINFO contains detailed card information. It is purely for data storage.
public class CardInfo : IComparable<CardInfo>
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
    public readonly int index;
    public readonly string name;
    public readonly string[] nation;
    public readonly bool placeholder; // If true, the card does not yet have an image
    public readonly int basePower;
    public readonly string race;
    public readonly string regulation;
    public readonly bool rotate;
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

    public readonly bool isTrigger;
    public readonly bool isOrder;
    public readonly bool isSentinel;
    public readonly bool isRegalis;

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
        gaugeZone,      // the owner obtains a Gauge Zone - - - REMOVE THIS
        locking,        // both players can access LOCK
        overdress,      // the owner can access OV DRESS
        prison,         // the owner gains a Prison Zone and the opponent can access PRISON
        soulRC,         // the owner's RC have SOUL access (i.e. Noblesse Gauge)

        // MORE
        search,
        token, // remove
        marker, // remove
        ticket, // remove
        crest, // remove
        shuffle,
        viewsoul
    }

    public CardInfo(int count, int baseCrit, int baseDrive, string effect, string gift, int grade, string group, string id, int index, string name, string[] nation, bool placeholder, int basePower, string race, string regulation, bool rotate, int baseShield, string[] skills, string unitType, int version)
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
        this.placeholder = placeholder;
        this.basePower = basePower;
        this.race = race;
        this.regulation = regulation;
        this.rotate = rotate;
        this.baseShield = baseShield;
        this.skills = skills;
        this.unitType = unitType;
        this.version = version;

        isTrigger = unitType.Contains("Trigger", StringComparison.InvariantCultureIgnoreCase);
        isOrder = unitType.Contains("Order", StringComparison.InvariantCultureIgnoreCase);
        isSentinel = skills.Contains("Sentinel");
        isRegalis = skills.Contains("Regalis Piece");
    }

    public static CardInfo GenerateDefaultCardInfo()
    {
        return new CardInfo(4, 1, 1, "effect", "", 1, "", "default", 0, "default", new string[] { "Dark States" }, false, 8000, "Human", "Standard", false, 5000, new string[0], "Normal Unit", 0);
    }

    public static CardInfo FromDictionary(Dictionary<string, object> dictionary)
    {
        JArray skillJArray = (JArray)dictionary["skill"];
        string[] skillArray = new string[skillJArray.Count];
        for (int i = 0; i < skillArray.Count(); i++)
        {
            skillArray[i] = skillJArray[i].ToObject<string>();
        }
        JArray nationJArray = (JArray)dictionary["nation"];
        string[] nationArray = new string[nationJArray.Count];
        for (int i = 0; i < nationArray.Count(); i++)
        {
            nationArray[i] = nationJArray[i].ToObject<string>();
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
            nationArray,
            Convert.ToBoolean(dictionary["placeholder"]),
            Convert.ToInt32(dictionary["power"]),
            Convert.ToString(dictionary["race"]),
            Convert.ToString(dictionary["regulation"]),
            Convert.ToBoolean(dictionary["rotate"]),
            Convert.ToInt32(dictionary["shield"]),
            skillArray,
            Convert.ToString(dictionary["type"]),
            Convert.ToInt32(dictionary["version"])
            );
    }

    [System.Serializable]
    public class DeckList
    {
        public const int maxMain = 50;
        public const int maxRide = 6;
        public const int maxStride = 16;
        public const int maxToolbox = 34;

        public string deckName;
        public string nation;

        public int[] mainDeck;      // MAIN DECK.
        public int[] rideDeck;      // RIDE DECK.
        public int[] strideDeck;    // STRIDE DECK.
        public int[] toolbox;       // TOKENS, TICKETS, MARKERS.

        public DeckList()
        {

        }
        public DeckList(string deckName, string nation, int[] mainDeck, int[] rideDeck, int[] strideDeck, int[] toolbox)
        {
            this.deckName = deckName;
            this.nation = nation;
            this.mainDeck = mainDeck;
            this.rideDeck = rideDeck;
            this.strideDeck = strideDeck;
            this.toolbox = toolbox;
        }

        public int CardCount(int cardIndex)
        {
            int count = 0;
            foreach (int i in mainDeck) { if  (i == cardIndex) count++; }
            foreach (int i in rideDeck) { if (i == cardIndex) count++; }
            foreach (int i in strideDeck) { if (i == cardIndex) count++; }
            return count;
        }

        public int SentinelCount()
        {
            int count = 0;
            foreach (int i in mainDeck) { if (CardLoader.GetCardInfo(i).isSentinel) count++; }
            foreach (int i in rideDeck) { if (CardLoader.GetCardInfo(i).isSentinel) count++; }
            return count;
        }

        public int RegalisCount()
        {
            int count = 0;
            foreach (int i in mainDeck) { if (CardLoader.GetCardInfo(i).isRegalis) count++; }
            foreach (int i in rideDeck) { if (CardLoader.GetCardInfo(i).isRegalis) count++; }
            return count;
        }

        public bool IsValid(out string error)
        {
            error = string.Empty;
            if (CardLoader.instance != null && CardLoader.instance.CardsLoaded)
            {
                error = "Card Loader is not initialized.";
                return true;
            }
            else
            {
                // This can be optimized to use fewer runthroughs, but it would lack readability for a small increase in speed.
                HashSet<int> cardSet = new HashSet<int>();
                foreach (int i in mainDeck) { cardSet.Add(i); }
                foreach (int i in rideDeck) { cardSet.Add(i); }
                foreach (int i in strideDeck) { cardSet.Add(i); }
                foreach (int cardIndex in cardSet)
                {
                    CardInfo info = CardLoader.GetCardInfo(cardIndex);
                    if (info.nation[0] != "Nationless" && !info.nation.Contains(nation))
                    {
                        error = "Mixing nations is disallowed.";
                        return true;
                    }
                    if (CardCount(cardIndex) > info.count)
                    {
                        error = "'" + info.name + "' exceeds the maximum number of copies.";
                        return true;
                    }
                }
                if (rideDeck.Length > 0)
                {
                    if (rideDeck.Length > 4 && (rideDeck[0] != 1676 || rideDeck[0] != 3080 || nation != "Touken Ranbu"))
                    {
                        error = "Only 'Griphosid' rideline, 'Sephirosid' rideline, and the 'Touken Ranbu' nation allow more than 4 cards in the Ride Deck.";
                    }
                    else if (rideDeck[0] != 1676)
                    {
                        // Griphosid exception
                    }
                    else if (rideDeck[0] != 3080)
                    {
                        // Seriphosid exception
                    }
                    else if (nation == "Touken Ranbu")
                    {
                        // Touken Ranbu exception
                    }
                }
                if (strideDeck.Length > 0)
                {
                    foreach (int i in strideDeck)
                    {
                        if (CardLoader.GetCardInfo(i).unitType != "G Unit")
                        {
                            error = "Only G Units are allowed in the Stride Deck.";
                            return true;
                        }
                    }
                }
                if (rideDeck.Length < 4 || rideDeck.Length > maxRide)
                {
                    error = "Invalid number of cards in the Ride Deck. Typically four are required.";
                }
                else if (mainDeck.Length != maxMain)
                {
                    error = "Invalid number of cards in the Main Deck. 50 are required.";
                }
                else if (strideDeck.Length > maxStride)
                {
                    error = "Invalid number of cards in the Stride Deck. 16 is the maximum.";
                }
                else if (toolbox.Length > maxToolbox)
                {
                    error = "Invalid number of cards in the Toolbox. 34 is the maximum.";
                }
                return string.IsNullOrEmpty(error);
            }
        }

        public static DeckList FromJSON(string JSON)
        {
            return JsonConvert.DeserializeObject<DeckList>(JSON);
        }

        public string ToJSON()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public static DeckList CreateRandomDeck()
    {
        DeckList deck = new DeckList();
        deck.deckName = "random deck";
        deck.nation = "Dark States";
        deck.mainDeck = new int[DeckList.maxMain];
        deck.rideDeck = new int[DeckList.maxRide];
        deck.strideDeck = new int[DeckList.maxStride];
        deck.toolbox = new int[DeckList.maxToolbox];
        int cycle = UnityEngine.Random.Range(0, 90);
        for (int i = 0; i < deck.mainDeck.Count(); i++)
        {
            deck.mainDeck[i] = cycle;
            cycle++;
            if (cycle > 5)
            {
                cycle = 0;
            }
        }
        for (int i = 0; i < deck.rideDeck.Count(); i++)
        {
            deck.rideDeck[i] = UnityEngine.Random.Range(0, 100);
        }
        for (int i = 0; i < deck.strideDeck.Count(); i++)
        {
            deck.strideDeck[i] = UnityEngine.Random.Range(0, 100);
        }
        for (int i = 0; i < deck.toolbox.Count(); i++)
        {
            deck.toolbox[i] = UnityEngine.Random.Range(0, 100);
        }
        return deck;
    }

    public int CompareTo(CardInfo other)
    {
        int adjustedGrade = grade;
        if (isSentinel) { adjustedGrade += 90; }
        if (isTrigger) { adjustedGrade += 100; }

        int otherAdjustedGrade = grade;
        if (other.isSentinel) { otherAdjustedGrade += 90; }
        if (other.isTrigger) { otherAdjustedGrade += 100; }

        if (adjustedGrade != otherAdjustedGrade) { return adjustedGrade.CompareTo(otherAdjustedGrade); }
        if (unitType != other.unitType) { return unitType.CompareTo(other.unitType); }
        if (name != other.name) { return name.CompareTo(other.name); }
        return index.CompareTo(other.index);
    }

}
