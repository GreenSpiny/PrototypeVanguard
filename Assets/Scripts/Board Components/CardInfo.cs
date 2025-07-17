using UnityEngine;

// CARDINFO contains detailed card information. It is purely for data storage.
public class CardInfo
{
    // Standard card elements --- o
    public enum UnitType { normalUnit, triggerUnit, gUnit, normalOrder, blitzOrder, setOrder, crest };
    public enum TriggerType { none, critical, heal, draw, front, over };

    public readonly UnitType unitType;          // unit type
    public readonly TriggerType triggerType;    // trigger type; 'none' if not a trigger
    public readonly int grade;                  // grade of the unit
    public readonly string nation;              // nation, i.e. Keter Sanctuary
    public readonly string race;                // race, i.e. Human
    public readonly string group;               // group, i.e. Shadow Paladin
    public readonly bool persona;               // if the unit has Persona Ride
    public readonly int basePower;              // base power of the unit
    public readonly int baseShield;             // base shield of the unit
    public readonly int baseDrive;              // base drive of the unit
    public readonly int baseCrit;               // base critical of the unit
    public readonly int maxCount;               // maxmimum number of this card allowed in a deck; usually four

    public readonly string name;
    public readonly string text;

    //public readonly string nameLocalizationKey;
    //public readonly string textLocalizationKey;

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

    public CardInfo(UnitType unitType, TriggerType triggerType, int grade, string nation, string race, string group, bool persona, int basePower, int baseShield, int baseDrive, int baseCrit, int maxCount, string name, string text)
    {
        this.unitType = unitType;
        this.triggerType = triggerType;
        this.grade = grade;
        this.persona = persona;
        this.basePower = basePower;
        this.baseShield = baseShield;
        this.baseDrive = baseDrive;
        this.baseCrit = baseCrit;
        this.maxCount = maxCount;
        this.name = name;
        this.text = text;
    }

    public static CardInfo GenerateDefaultCardInfo()
    {
        return new CardInfo
            (UnitType.normalUnit,
            TriggerType.none,
            1,
            "Stoicheia",
            "Dryad",
            "none",
            false,
            8000,
            5000,
            1,
            1,
            4,
            "Burrow Mushrooms [default]",
            "[ACT](RC):[COST][Put this unit into soul], call up to two Plant tokens to (RC), if you have a grade 3 or greater vanguard with \"Granfia\" in its card name, choose one of your rear-guards, and it gets [Power] +5000 until end of turn. (Plant tokens are grade 0/[Power] 5000/[Critical] 1 and have boost)");
    }

}
