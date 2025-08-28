using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Node_Display : Node
{
    public override NodeType Type => NodeType.display;
    private Node lastAcceptedNode;

    public bool LastNodeWasDeck { get { return lastAcceptedNode != null && lastAcceptedNode.Type == NodeType.deck; } }
    public override void RecieveCard(Card card, string parameters)
    {
        cards.Add(card);
        base.RecieveCard(card, parameters);
    }

    public override void AlignCards(bool instant)
    {
        // If the number of cards is greater than the max cards per row, align left. Otherwise, align center.

        int row = 0;
        int column = 0;
        int maxCardsPerRow = 16;

        float cardWidth = Card.cardWidth * cardScale.x;
        float xSpacing = cardWidth;
        float ySpacing = 0.33f;

        bool leftAlign = cards.Count > maxCardsPerRow;

        float totalWidth = cardWidth * cards.Count; ;
        if (leftAlign)
        {
            totalWidth = cardWidth * maxCardsPerRow;
        }
        float origin = totalWidth / 2f - cardWidth * 0.5f;

        for (int i = 0; i < cards.Count; i++)
        {
            Card card = cards[i];
            card.anchoredPosition = new Vector3(-origin + column * xSpacing, 0f, row * ySpacing);

            column++;
            if (column >= maxCardsPerRow)
            {
                row++;
                column = 0;
            }
            card.LookAt(cameraTransform);
            card.ToggleColliders(true);
        }

        base.AlignCards(instant);
    }

    public override List<CardInfo.ActionFlag> GenerateDefaultCardActions()
    {
        List<CardInfo.ActionFlag> toReturn = new List<CardInfo.ActionFlag>()
        {
            CardInfo.ActionFlag.reveal,
            CardInfo.ActionFlag.soul,
            CardInfo.ActionFlag.botdeck
        };
        return toReturn;
    }

    public void OpenDisplay(Node node, int cardCount, bool revealCards, bool sortCards)
    {
        CloseDisplay();
        lastAcceptedNode = node;
        if (node.Type == NodeType.toolbox)
        {
            node.transform.localPosition = Vector3.zero;
            node.AlignCards(true);
        }
        int initialCount = node.cards.Count;
        Card c = node.TopCard;
        for (int i = initialCount - 1; i >= initialCount - cardCount;)
        {
            RecieveCard(c, string.Empty);
            i--;
            if (i >= 0)
            {
                c = node.cards[i];
            }
        }
        if (sortCards)
        {
            cards.Sort(new Card.CardComparer());
        }
        if (revealCards)
        {
            foreach (Card card in cards)
            {
                c.SetRevealed(true, float.MaxValue);
            }
        }
    }

    public void CloseDisplay()
    {
        for (int i = cards.Count - 1; i >= 0; i--)
        {
            string paramaters = string.Empty;
            if (cards[i].flip)
            {
                paramaters += "facedown";
            }
            if (lastAcceptedNode.Type == NodeType.toolbox)
            {
                lastAcceptedNode.transform.localPosition = Vector3.zero;
            }
            lastAcceptedNode.RecieveCard(cards[i], paramaters);
        }
    }
}
