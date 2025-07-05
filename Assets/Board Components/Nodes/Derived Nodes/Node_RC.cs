using System.Collections.Generic;
using UnityEngine;

public class Node_RC : Node_Stack
{
    public override NodeType Type => NodeType.RC;

    public override void CardAutoAction(Card clickedCard)
    {
        clickedCard.rest = !clickedCard.rest;
        AlignCards(false);
    }

    public override void NodeAutoAction()
    {
        base.NodeAutoAction();
    }

    public override IEnumerable<CardInfo.ActionFlag> GetDefaultActions()
    {
        return new CardInfo.ActionFlag[]
        {
            CardInfo.ActionFlag.power,
            CardInfo.ActionFlag.soul,
            CardInfo.ActionFlag.botdeck
        };
    }

    public override IEnumerable<CardInfo.ActionFlag> GetSpecialActions()
    {
        return new CardInfo.ActionFlag[]
        {
            CardInfo.ActionFlag.bindFD,
            CardInfo.ActionFlag.gaugeZone,
            CardInfo.ActionFlag.locking,
            CardInfo.ActionFlag.overdress,
            CardInfo.ActionFlag.prison,
            CardInfo.ActionFlag.soulRC
        };
    }

}
