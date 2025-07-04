using System.Collections.Generic;
using UnityEngine;

public class Node_Damage : Node_Fan
{
    public override NodeType Type => NodeType.damage;

    public override void CardAutoAction(Card clickedCard)
    {
        clickedCard.flipRotation = !clickedCard.flipRotation;
        AlignCards(false);
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
