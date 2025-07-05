using UnityEngine;

// CARDINFO contains detailed card information. It is purely for data storage.
public class CardInfo
{
    // Standard card elements --- o
    public enum UnitType { normalUnit, triggerUnit, gUnit, normalOrder, blitzOrder, setOrder };
    public enum TriggerType { none, critical, heal, draw, front, over };

    public readonly UnitType unitType;          // unit type
    public readonly TriggerType triggerType;    // trigger type; 'none' if not a trigger
    public readonly int grade;                  // grade of the unit
    public readonly bool persona;               // if the unit has Persona Ride
    public readonly int basePower;              // base power of the unit
    public readonly int baseShield;             // base shield of the unit
    public readonly int baseDrive;              // base drive of the unit
    public readonly int baseCrit;               // base critical of the unit
    public readonly int maxCount;               // maxmimum number of this card allowed in a deck; usually four

    public readonly string name;
    public readonly string text;

    public readonly string nameLocalizationKey;
    public readonly string textLocalizationKey;

    public int powerModifier;
    public int shieldModifier;
    public int driveModifier;
    public int critModifier;
    public int power { get { return basePower + powerModifier; } set { powerModifier = value - basePower; } }
    public int shield { get { return baseShield + shieldModifier; } set { shieldModifier = value - baseShield; } }
    public int drive { get { return baseDrive + driveModifier; } set { driveModifier = value - baseDrive; } }
    public int crit { get { return baseCrit + critModifier; } set { critModifier = value - baseCrit; } }

    public enum CardState { stand, rest, facedown }
    public CardState cardState;

    // Unique card elements --- o
    // Some cards have properties that necessitate additional actions be offered to either player.
    // These properties are treated as flags to keep offerings to a minimum.
    public readonly ActionFlag[] actionFlags;
    public enum ActionFlag
    {
        // DEFAULT ACTIONS
        power,          // POWER     (card)
        soul,           // TO SOUL   (card)
        botdeck,        // BOT DECK  (card)
        reveal,         // REVEAL    (card)
        view,           // VIEW      (node)
        viewx,          // VIEW X    (node)
        revealx,        // REVEAL X  (node)

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

}
