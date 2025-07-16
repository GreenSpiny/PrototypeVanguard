using System.Collections.Generic;
using UnityEngine;

public class Node_Hand : Node_Fan
{
    public override NodeType Type => NodeType.hand;

    public override void CardAutoAction(Card clickedCard)
    {
        base.CardAutoAction(clickedCard);
    }

    public override void NodeAutoAction()
    {
        base.NodeAutoAction();
    }

    public override IEnumerable<CardInfo.ActionFlag> GetDefaultActions()
    {
        return new CardInfo.ActionFlag[]
        {
            CardInfo.ActionFlag.reveal,
            CardInfo.ActionFlag.soul,
            CardInfo.ActionFlag.botdeck
        };
    }

    public override IEnumerable<CardInfo.ActionFlag> GetSpecialActions()
    {
        return new CardInfo.ActionFlag[]
        {
            CardInfo.ActionFlag.armLeft,
            CardInfo.ActionFlag.armRight,
            CardInfo.ActionFlag.prison,
            CardInfo.ActionFlag.overdress,
            CardInfo.ActionFlag.soulRC
        };
    }

}
