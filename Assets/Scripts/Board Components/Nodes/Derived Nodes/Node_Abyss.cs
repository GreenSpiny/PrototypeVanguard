using System.Collections.Generic;
using UnityEngine;

public class Node_Abyss : Node_Stack
{
    public override NodeType Type => NodeType.abyss;

    public override void CardAutoAction(Player player, Card clickedCard)
    {
        base.CardAutoAction(player, clickedCard);
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
