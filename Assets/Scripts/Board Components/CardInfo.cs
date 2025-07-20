using System.Collections;
using UnityEngine;

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

    public static CardInfo FromIDictionary(IDictionary dictionary)
    {
        return new CardInfo(
            (int) dictionary["count"],
            (int) dictionary["crit"],
            (int) dictionary["drive"],
            (string) dictionary["effect"],
            (string) dictionary["gift"],
            (int) dictionary["grade"],
            (string) dictionary["group"],
            (string) dictionary["id"],
            (int) dictionary["index"],
            (string) dictionary["name"],
            (string) dictionary["nation"],
            (int) dictionary["power"],
            (string) dictionary["race"],
            (int) dictionary["shield"],
            (string[]) dictionary["skills"],
            (string) dictionary["type"],
            (int) dictionary["version"]
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

}
