using System.Collections.Generic;
using UnityEngine;

public class Node_Toolbox : Node_Stack
{
    public override NodeType Type => NodeType.toolbox;

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

        };
        return toReturn;
    }

}
