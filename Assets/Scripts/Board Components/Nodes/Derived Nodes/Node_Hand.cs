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

    public override List<CardInfo.ActionFlag> GenerateDefaultCardActions()
    {
        List<CardInfo.ActionFlag> toReturn = new List<CardInfo.ActionFlag>()
        {
            CardInfo.ActionFlag.reveal,
            CardInfo.ActionFlag.soul,
            CardInfo.ActionFlag.botdeck
        };
        return toReturn;
    }

}
