using System.Collections.Generic;
using UnityEngine;

public class Node_Remove : Node_Stack
{
    public override NodeType Type => NodeType.remove;

    public override void CardAutoAction(Card clickedCard)
    {
        base.CardAutoAction(clickedCard);
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
