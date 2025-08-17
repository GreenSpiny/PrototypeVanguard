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

    public override List<CardInfo.ActionFlag> GenerateDefaultCardActions()
    {
        List<CardInfo.ActionFlag> toReturn = new List<CardInfo.ActionFlag>()
        {
            
        };
        return toReturn;
    }

}
