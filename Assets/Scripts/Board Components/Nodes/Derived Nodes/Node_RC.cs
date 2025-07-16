using System.Collections.Generic;
using UnityEngine;

public class Node_RC : Node_Stack
{
    public override NodeType Type => NodeType.RC;

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

    public override void RecieveCard(Card card, string parameters)
    {
        bool drag = parameters.Contains("drag");
        bool toSoulRC = parameters.Contains("bottom");
        bool isFromRC = card.node.Type == NodeType.RC || (card.node.Type == NodeType.drag && card.node.PreviousNode.Type == NodeType.RC);
        bool noRetire = parameters.Contains("noRetire");

        if (toSoulRC)
        {
            base.RecieveCard(card, parameters);
        }
        if (isFromRC && drag)
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
                targetNode.RecieveCard(c, "noRetire");
            }
            foreach (Card c in newCards)
            {
                RecieveCard(c, "noRetire");
            }
        }
        else
        {
            if (!noRetire)
            {
                RetireCards();
            }
            base.RecieveCard(card, parameters);
        }
    }

    public override void NodeAutoAction()
    {
        base.NodeAutoAction();
    }

    public override IEnumerable<CardInfo.ActionFlag> GetDefaultActions()
    {
        return new CardInfo.ActionFlag[]
        {
            CardInfo.ActionFlag.power,
            CardInfo.ActionFlag.soul,
            CardInfo.ActionFlag.botdeck
        };
    }

    public override IEnumerable<CardInfo.ActionFlag> GetSpecialActions()
    {
        return new CardInfo.ActionFlag[]
        {
            CardInfo.ActionFlag.bindFD,
            CardInfo.ActionFlag.gaugeZone,
            CardInfo.ActionFlag.locking,
            CardInfo.ActionFlag.overdress,
            CardInfo.ActionFlag.prison,
            CardInfo.ActionFlag.soulRC
        };
    }

}
