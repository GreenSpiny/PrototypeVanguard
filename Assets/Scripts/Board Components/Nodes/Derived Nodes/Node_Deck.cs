using System.Collections.Generic;
using UnityEngine;

public class Node_Deck : Node_Stack
{
    public override NodeType Type => NodeType.deck;

    public override void CardAutoAction(Player player, Card clickedCard)
    {
        if (GameManager.singlePlayer || this.player == player)
        {
            GameManager.instance.RequestReceiveCardRpc(player.hand.nodeID, clickedCard.cardID, string.Empty);
        }
    }

    public override void NodeAutoAction()
    {
        base.NodeAutoAction();
    }

    public override List<CardInfo.ActionFlag> GenerateDefaultCardActions()
    {
        List<CardInfo.ActionFlag> toReturn = new List<CardInfo.ActionFlag>()
        {
            CardInfo.ActionFlag.search,
            CardInfo.ActionFlag.viewx,
            CardInfo.ActionFlag.revealx,
            CardInfo.ActionFlag.soul,
            CardInfo.ActionFlag.shuffle
        };
        return toReturn;
    }

}
