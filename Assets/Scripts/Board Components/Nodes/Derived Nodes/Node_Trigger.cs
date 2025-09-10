using System.Collections.Generic;
using UnityEngine;

public class Node_Trigger : Node_Fan
{
    public override NodeType Type => NodeType.trigger;

    public override void CardAutoAction(Card clickedCard)
    {
        if (GameManager.instance.turnPlayer == player.playerIndex)
        {
            GameManager.instance.RequestReceiveAllCardsRpc(nodeID, "hand", string.Empty);
        }
        else
        {
            GameManager.instance.RequestReceiveAllCardsRpc(nodeID, "damage", string.Empty);
        }
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
