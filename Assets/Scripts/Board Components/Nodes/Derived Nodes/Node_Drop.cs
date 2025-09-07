using System.Collections.Generic;
using UnityEngine;

public class Node_Drop : Node_Stack
{
    public override NodeType Type => NodeType.drop;

    public override void CardAutoAction(Card clickedCard)
    {
        DragManager.instance.OpenDisplay(player.playerIndex, this, 0, cards.Count, false, true);
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
