using System.Collections.Generic;
using UnityEngine;

public class Node_Deck : Node_Stack
{
    public override NodeType Type => NodeType.deck;

    public override void CardAutoAction(Card clickedCard)
    {
        GameManager.instance.RequestRecieveCardRpc(clickedCard.player.hand.nodeID, clickedCard.cardID, string.Empty);
    }

    public override void NodeAutoAction()
    {
        base.NodeAutoAction();
    }

    protected override List<CardInfo.ActionFlag> GenerateDefaultCardActions()
    {
        List<CardInfo.ActionFlag> toReturn = new List<CardInfo.ActionFlag>()
        {
            CardInfo.ActionFlag.search,
            CardInfo.ActionFlag.viewx,
            CardInfo.ActionFlag.revealx,
            CardInfo.ActionFlag.soul
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
