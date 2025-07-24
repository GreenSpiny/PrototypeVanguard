using System.Collections.Generic;
using UnityEngine;

public class Node_Drop : Node_Stack
{
    public override NodeType Type => NodeType.drop;

    public override void CardAutoAction(Card clickedCard)
    {
        DragManager.instance.OpenDisplay(player.playerIndex, this, cards.Count, false);
    }

    public override void NodeAutoAction()
    {
        base.NodeAutoAction();
    }

    protected override List<CardInfo.ActionFlag> GenerateDefaultCardActions()
    {
        List<CardInfo.ActionFlag> toReturn = new List<CardInfo.ActionFlag>()
        {
            CardInfo.ActionFlag.view
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
