using System.Collections.Generic;
using UnityEngine;

public class Node_GC : Node_Fan
{
    public override NodeType Type => NodeType.GC;

    public override void CardAutoAction(Card clickedCard)
    {
        GameManager.instance.RequestRetireCardsRpc(clickedCard.node.nodeID, string.Empty);
    }

    public override void NodeAutoAction()
    {
        base.NodeAutoAction();
    }

    public override List<CardInfo.ActionFlag> GenerateDefaultCardActions()
    {
        List<CardInfo.ActionFlag> toReturn = new List<CardInfo.ActionFlag>()
        {
            CardInfo.ActionFlag.soul
        };
        return toReturn;
    }

}
