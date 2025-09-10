using System;
using System.Collections.Generic;
using UnityEngine;

public class Node_RC : Node_Stack
{
    public override NodeType Type => NodeType.RC;
    [SerializeField] public bool isBackRC;

    public override void CardAutoAction(Card clickedCard)
    {
        if (clickedCard.flip)
        {
            GameManager.instance.RequestSetOrientationRpc(clickedCard.cardID, false, clickedCard.rest);
        }
        else
        {
            GameManager.instance.RequestSetOrientationRpc(clickedCard.cardID, clickedCard.flip, !clickedCard.rest);
        }
    }

    public override void ReceiveCard(Card card, string parameters)
    {
        bool drag = parameters.Contains("drag");
        bool toSoulRC = parameters.Contains("bottom");
        bool isFromRC = card.node.Type == NodeType.RC || (card.node.Type == NodeType.drag && card.node.PreviousNode.Type == NodeType.RC);
        bool noRetire = parameters.Contains("noRetire");

        if (toSoulRC)
        {
            base.ReceiveCard(card, parameters);
        }
        else if (isFromRC && drag)
        {
            // Account for local Drag Node
            Node targetNode = card.node;
            if (targetNode.Type == NodeType.drag && card.player == player)
            {
                targetNode = targetNode.PreviousNode;
            }

            // Create shallow copies
            HashSet<Card> originalCards = new HashSet<Card>();
            foreach (Card c in cards)
            {
                originalCards.Add(c);
            }
            HashSet<Card> newCards = new HashSet<Card>();
            foreach (Card c in targetNode.cards)
            {
                newCards.Add(c);
            }
            newCards.Add(card);

            // Initiate the swap
            foreach (Card c in originalCards)
            {
                targetNode.ReceiveCard(c, "noRetire");
            }
            foreach (Card c in newCards)
            {
                ReceiveCard(c, "noRetire");
            }
        }
        else
        {
            if (!noRetire)
            {
                for (int i = cards.Count - 1; i >= 0; i--)
                {
                    cards[i].player.drop.ReceiveCard(cards[i], string.Empty);
                }
            }
            base.ReceiveCard(card, parameters);
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
            CardInfo.ActionFlag.power,
            CardInfo.ActionFlag.soul,
            CardInfo.ActionFlag.botdeck
        };
        if (cards.Count > 1)
        {
            toReturn.Add(CardInfo.ActionFlag.viewsoul);
        }
        return toReturn;
    }

}
