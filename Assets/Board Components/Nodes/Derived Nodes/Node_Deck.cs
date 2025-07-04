using System.Collections.Generic;
using UnityEngine;

public class Node_Deck : Node_Stack
{
    public override NodeType Type => NodeType.deck;

    public override void CardAutoAction(Card clickedCard)
    {
        clickedCard.player.hand.RecieveCard(clickedCard, new string[0]);
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
