using System.Collections.Generic;
using UnityEngine;

public class Node_Bind : Node_Stack
{
    public override NodeType Type => NodeType.bind;

    public override void CardAutoAction(Player player, Card clickedCard)
    {
        DragManager.instance.OpenDisplay(DragManager.instance.controllingPlayer.playerIndex, this, 0, cards.Count, false, true);
    }

    public override void NodeAutoAction()
    {
        base.NodeAutoAction();
    }

    public override List<CardInfo.ActionFlag> GenerateDefaultCardActions()
    {
        List<CardInfo.ActionFlag> toReturn = new List<CardInfo.ActionFlag>()
        {
            CardInfo.ActionFlag.view
        };
        return toReturn;
    }

}
