using System.Collections.Generic;
using UnityEngine;

public class Node_Damage : Node_Fan
{
    public override NodeType Type => NodeType.damage;

    public override void CardAutoAction(Player player, Card clickedCard)
    {
        if (GameManager.singlePlayer || this.player == player)
        {
            GameManager.instance.RequestSetOrientationRpc(clickedCard.cardID, !clickedCard.flip, clickedCard.rest);
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
