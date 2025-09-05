using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.VolumeComponent;

// CARDINFO contains detailed card information. It is purely for data storage.
public class CardInfo : IComparable<CardInfo>
{
    // Standard card elements --- o
    public readonly int alias;
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
    public readonly string[] race;
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
    public readonly bool isElementaria;
    public readonly bool isRegalis;

    public readonly string strippedName;
    public readonly string strippedEffect;

    public bool hasAlias { get { return alias >= 0; } }
    public int GetUniqueIndex()
    {
        if (hasAlias)
        {
            return alias;
        }
        return index;
    }

    public const string invalidRegulation = "Invalid";

    // Unique card elements --- o
    // Some cards have properties that necessitate additional actions be offered to either player.
    // These properties are treated as flags to keep offerings to a minimum.

    public readonly List<ActionFlag> cardActionFlags = new List<ActionFlag>();      // grants actions to its card
    public readonly List<ActionFlag> playerActionFlags = new List<ActionFlag>();    // grants actions to its player
    public readonly List<ActionFlag> globalActionFlags = new List<ActionFlag>();    // grants actions to both players
    public enum ActionFlag
    {
        none = 0,

        // DEFAULT ACTIONS
        power = 1,
        soul = 2,
        botdeck = 3,
        reveal = 4,
        view = 5,
        viewx = 6,
        revealx = 7,
        search = 8,
        shuffle = 9,
        viewsoul = 10,

        // SPECIAL ACTIONS
        armLeft = 11,
        armRight = 12,
        bindFD = 13,
        bindFDFoe = 14,
        locking = 15,
        rideRC = 16,
        soulRC = 17
    }

    public CardInfo()
    {
        nation = new string[0];
        race = new string[0];
        skills = new string[0];
        placeholder = true;
        regulation = invalidRegulation;
    }
    public CardInfo(int alias, int count, int baseCrit, int baseDrive, string effect, string gift, int grade, string group, string id, int index, string name, string[] nation, bool placeholder, int basePower, string[] race, string regulation, bool rotate, int baseShield, string[] skills, string unitType, int version, ActionFlag[] actionFlags)
    {
        this.alias = alias;
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

        foreach (ActionFlag flag in actionFlags)
        {
            switch (flag)
            {
                case ActionFlag.armLeft: cardActionFlags.Add(flag); break;
                case ActionFlag.armRight: cardActionFlags.Add(flag); break;
                case ActionFlag.bindFD: playerActionFlags.Add(flag); break;
                case ActionFlag.bindFDFoe: globalActionFlags.Add(flag); break;
                case ActionFlag.locking: globalActionFlags.Add(flag); break;
                case ActionFlag.rideRC: playerActionFlags.Add(flag); break;
                case ActionFlag.soulRC: playerActionFlags.Add(flag); break;
            }
        }

        isTrigger = unitType.Contains("Trigger", StringComparison.InvariantCultureIgnoreCase);
        isOrder = unitType.Contains("Order", StringComparison.InvariantCultureIgnoreCase);
        isSentinel = skills.Contains("Sentinel");
        isElementaria = skills.Contains("Elementaria");
        isRegalis = skills.Contains("Regalis Piece");

        strippedName = GameManager.SimplifyString(name);
        strippedEffect = GameManager.SimplifyString(effect);
    }

    public static CardInfo GenerateDefaultCardInfo()
    {
        return new CardInfo(-1, 4, 1, 1, "effect", "", 1, "", "default", 0, "default", new string[] { "Dark States" }, false, 8000, new string[] { "Human" }, "Standard", false, 5000, new string[0], "Normal Unit", 0, new ActionFlag[0]);
    }

    public static CardInfo FromDictionary(Dictionary<string, object> dictionary)
    {
        JArray skillJArray = (JArray)dictionary["skill"];
        string[] skillArray = new string[skillJArray.Count];
        for (int i = 0; i < skillArray.Count(); i++)
        {
            skillArray[i] = skillJArray[i].ToObject<string>();
        }

        JArray raceJArray = (JArray)dictionary["race"];
        string[] raceArray = new string[raceJArray.Count];
        for (int i = 0; i < raceArray.Count(); i++)
        {
            raceArray[i] = raceJArray[i].ToObject<string>();
        }

        JArray nationJArray = (JArray)dictionary["nation"];
        string[] nationArray = new string[nationJArray.Count];
        for (int i = 0; i < nationArray.Count(); i++)
        {
            nationArray[i] = nationJArray[i].ToObject<string>();
        }

        JArray actionFlagJArray = (JArray)dictionary["actionflags"];
        ActionFlag[] actionFlagArray = new ActionFlag[actionFlagJArray.Count];
        for (int i = 0; i < actionFlagArray.Count(); i++)
        {
            ActionFlag flag = (ActionFlag) actionFlagJArray[i].ToObject<int>();
        }

        return new CardInfo(
            Convert.ToInt32(dictionary["alias"]),
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
            raceArray,
            Convert.ToString(dictionary["regulation"]),
            Convert.ToBoolean(dictionary["rotate"]),
            Convert.ToInt32(dictionary["shield"]),
            skillArray,
            Convert.ToString(dictionary["type"]),
            Convert.ToInt32(dictionary["version"]),
            actionFlagArray
            );
    }

    [System.Serializable]
    public class DeckList
    {
        public const int maxMain = 50;
        public const int maxRide = 5;
        public const int maxStride = 16;
        public const int maxToolbox = 30;

        public string deckName;
        public string nation;

        public int[] mainDeck;      // MAIN DECK.
        public int[] rideDeck;      // RIDE DECK.
        public int[] strideDeck;    // STRIDE DECK.
        public int[] toolbox;       // TOKENS, TICKETS, MARKERS.

        public DeckList()
        {
            mainDeck = new int[0];
            rideDeck = new int[0];
            strideDeck = new int[0];
            toolbox = new int[0];
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

        public void ShuffleMainDeck()
        {
            RandomUtility.Shuffle(RandomUtility.GenerateRandom(), mainDeck);
        }

        public int CardCount(int cardIndex)
        {
            int uniqueIndex = CardLoader.GetCardInfo(cardIndex).GetUniqueIndex();
            int count = 0;
            foreach (int i in mainDeck)
            {
                if (CardLoader.GetCardInfo(i).GetUniqueIndex() == uniqueIndex) count++;
            }
            foreach (int i in rideDeck)
            {
                if (CardLoader.GetCardInfo(i).GetUniqueIndex() == uniqueIndex) count++;
            }
            foreach (int i in strideDeck)
            {
                if (CardLoader.GetCardInfo(i).GetUniqueIndex() == uniqueIndex) count++;
            }
            return count;
        }

        public int SentinelCount()
        {
            int count = 0;
            foreach (int i in mainDeck) { if (CardLoader.GetCardInfo(i).isSentinel) count++; }
            foreach (int i in rideDeck) { if (CardLoader.GetCardInfo(i).isSentinel) count++; }
            return count;
        }

        public int ElementariaCount()
        {
            int count = 0;
            foreach (int i in mainDeck) { if (CardLoader.GetCardInfo(i).isElementaria) count++; }
            foreach (int i in rideDeck) { if (CardLoader.GetCardInfo(i).isElementaria) count++; }
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
            if (CardLoader.instance == null || !CardLoader.instance.CardsLoaded)
            {
                error = "Card Loader is not initialized.";
                return false;
            }
            else
            {
                // Wave 1 checks
                // This can be optimized to use fewer runthroughs, but it would lack readability for a small increase in speed.
                int triggerCount = 0;
                int healCount = 0;
                int critCount = 0;
                int frontCount = 0;
                int drawCount = 0;
                int overCount = 0;

                HashSet<int> cardSet = new HashSet<int>();
                List<int> cardList = new List<int>();
                foreach (int i in mainDeck)
                {
                    CardInfo info = CardLoader.GetCardInfo(i);
                    if (info.regulation == invalidRegulation)
                    {
                        error = "Deck contains a redacted card.";
                        return false;
                    }
                    cardSet.Add(i);
                    cardList.Add(i);
                }
                foreach (int i in rideDeck)
                {
                    CardInfo info = CardLoader.GetCardInfo(i);
                    if (info.regulation == invalidRegulation)
                    {
                        error = "Deck contains a redacted card.";
                        return false;
                    }
                    cardSet.Add(i);
                    cardList.Add(i);
                }
                foreach (int i in strideDeck)
                {
                    CardInfo info = CardLoader.GetCardInfo(i);
                    if (info.regulation == invalidRegulation)
                    {
                        error = "Deck contains a redacted card.";
                        return false;
                    }
                    cardSet.Add(i);
                    cardList.Add(i);
                }
                foreach (int cardIndex in cardSet)
                {
                    CardInfo info = CardLoader.GetCardInfo(cardIndex);
                    if (info.nation[0] != "Nationless" && !info.nation.Contains(nation))
                    {
                        error = "Mixing nations is disallowed.";
                        return false;
                    }
                    if (CardCount(cardIndex) > info.count)
                    {
                        error = "'" + info.name + "' exceeds the maximum number of copies.";
                        return false;
                    }
                }
                foreach (int cardIndex in cardList)
                {
                    CardInfo info = CardLoader.GetCardInfo(cardIndex);
                    if (info.isTrigger)
                    {
                        triggerCount++;
                        if (info.gift == "Heal") { healCount++; }
                        else if (info.gift == "Critical") { critCount++; }
                        else if (info.gift == "Front") { frontCount++; }
                        else if (info.gift == "Draw") { drawCount++; }
                        else if (info.gift == "Over") { overCount++; }
                    }
                }
                if (triggerCount > 16)
                {
                    error = "A maximum of 16 triggers are allowed in a deck.";
                }
                else if (healCount > 4)
                {
                    error = "A maximum of 4 heal triggers are allowed in a deck.";
                }
                else if (critCount > 8)
                {
                    error = "A maximum of 8 critical triggers are allowed in a deck.";
                }
                else if (frontCount > 8)
                {
                    error = "A maximum of 8 front triggers are allowed in a deck.";
                }
                else if (drawCount > 8)
                {
                    error = "A maximum of 8 draw triggers are allowed in a deck.";
                }
                else if (overCount > 1)
                {
                    error = "A maximum of 1 over trigger is allowed in a deck.";
                }
                else if (SentinelCount() > 4)
                {
                    error = "A maximum of 4 sentinels are allowed in a deck.";
                }
                else if (ElementariaCount() > 4)
                {
                    error = "A maximum of 1 'Elementaria Sanctitude' is allowed a deck.";
                }
                else if (RegalisCount() > 1)
                {
                    error = "A maximum of 1 Regalis Piece is allowed in a deck.";
                }

                // Wave 2 checks
                if (string.IsNullOrEmpty(error))
                {
                    if (rideDeck.Length > 0)
                    {
                        if (rideDeck.Length > 4 && (rideDeck[0] != 1676 && nation != "Touken Ranbu"))
                        {
                            error = "Only 'Griphosid' rideline and the 'Touken Ranbu' nation allow more than 4 cards in the Ride Deck.";
                        }
                        else
                        {
                            HashSet<int> requiredRides = new HashSet<int>() { 0, 1, 2, 3 };
                            bool overTrigger = false;
                            bool calamityOnly = true;
                            bool blessingOnly = true;
                            foreach (int ride in rideDeck)
                            {
                                CardInfo rideInfo = CardLoader.GetCardInfo(ride);
                                if (requiredRides.Contains(rideInfo.grade))
                                {
                                    requiredRides.Remove(rideInfo.grade);
                                }
                                if (rideInfo.grade > 0 && !rideInfo.race.Contains("Calamity"))
                                {
                                    calamityOnly = false;
                                }
                                if (rideInfo.grade > 0 && !rideInfo.race.Contains("Blessing"))
                                {
                                    blessingOnly = false;
                                }
                                if (rideInfo.gift == "Over")
                                {
                                    overTrigger = true;
                                }
                            }
                            if (rideDeck[0] == 1676)
                            {
                                if (requiredRides.Count != 0 || !calamityOnly || !overTrigger)
                                {
                                    error = "'Griphosid' rideline requires four Calamity of different grades including 'Griphosid', plus an Over Trigger.";
                                }
                            }
                            else if (rideDeck[0] == 3080)
                            {
                                if (requiredRides.Count != 0 || !blessingOnly)
                                {
                                    error = "'Sephirosid' rideline requires four Blessing cards of different grades including 'Seriphosid'.";
                                }
                            }
                            else if (requiredRides.Count != 0)
                            {
                                error = "A typical ride deck must contain four cards of different grades.";
                            }
                        }
                    }
                }

                // Wave 3 checks
                if (string.IsNullOrEmpty(error))
                {
                    if (strideDeck.Length > 0)
                    {
                        foreach (int i in strideDeck)
                        {
                            if (CardLoader.GetCardInfo(i).unitType != "G Unit")
                            {
                                error = "Only G Units are allowed in the Stride Deck.";
                                return false;
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
        if (isTrigger != other.isTrigger) { return isTrigger.CompareTo(other.isTrigger); }
        if (isSentinel != other.isSentinel) { return isSentinel.CompareTo(other.isSentinel); }
        if (isOrder != other.isOrder) { return isOrder.CompareTo(other.isOrder); }
        if (grade != other.grade) { return grade.CompareTo(other.grade); }
        if (gift != other.gift) { return gift.CompareTo(other.gift); }
        if (unitType != other.unitType) { return unitType.CompareTo(other.unitType); }
        if (name != other.name) { return name.CompareTo(other.name); }
        return index.CompareTo(other.index);
    }

}
