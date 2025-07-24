using System.Collections.Generic;
using UnityEngine;

public class Node_Order : Node_Fan
{
    public override NodeType Type => NodeType.order;

    public override void CardAutoAction(Card clickedCard)
    {
        GameManager.instance.RequestSetOrientationRpc(clickedCard.cardID, clickedCard.flip, !clickedCard.rest);
    }

    public override void NodeAutoAction()
    {
        base.NodeAutoAction();
    }

    protected override List<CardInfo.ActionFlag> GenerateDefaultCardActions()
    {
        List<CardInfo.ActionFlag> toReturn = new List<CardInfo.ActionFlag>()
        {

        };
        return toReturn;
    }

    protected override List<CardInfo.ActionFlag> GenerateDefaultNodeActions()
    {
        List<CardInfo.ActionFlag> toReturn = new List<CardInfo.ActionFlag>()
        {
            CardInfo.ActionFlag.token
        };
        return toReturn;
    }

}
