using System.Collections.Generic;
using UnityEngine;

public class Node_Abyss : Node_Stack
{
    public override NodeType Type => NodeType.abyss;

    public override void CardAutoAction(Card clickedCard)
    {
        base.CardAutoAction(clickedCard);
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
            
        };
        return toReturn;
    }

}
