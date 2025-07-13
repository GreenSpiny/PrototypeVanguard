using System.Collections.Generic;
using UnityEngine;

public class Node_Order : Node_Fan
{
    public override NodeType Type => NodeType.order;

    public override void CardAutoAction(Card clickedCard)
    {
        clickedCard.rest = !clickedCard.rest;
        SetDirty();
    }

    public override void NodeAutoAction()
    {
        base.NodeAutoAction();
    }

    public override IEnumerable<CardInfo.ActionFlag> GetDefaultActions()
    {
        return new CardInfo.ActionFlag[]
        {

        };
    }

    public override IEnumerable<CardInfo.ActionFlag> GetSpecialActions()
    {
        return new CardInfo.ActionFlag[]
        {

        };
    }

}
