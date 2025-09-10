using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Node_Display : Node
{
    public override NodeType Type => NodeType.display;
    private Node lastAcceptedNode;
    private string lastAcceptedParams = string.Empty;

    public bool LastNodeWasDeck { get { return lastAcceptedNode != null && lastAcceptedNode.Type == NodeType.deck; } }
    public override void ReceiveCard(Card card, string parameters)
    {
        cards.Add(card);
        lastAcceptedParams = parameters;
        base.ReceiveCard(card, parameters);
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

    public void OpenDisplay(Node node, int startIndex, int cardCount, bool revealCards, bool sortCards)
    {
        if (node == this)
        {
            return;
        }
        CloseDisplay();
        lastAcceptedNode = node;
        if (node.Type == NodeType.toolbox)
        {
            node.transform.localPosition = Vector3.zero;
            node.AlignCards(true);
        }
        int initialCount = node.cards.Count;
        for (int i = initialCount - startIndex - 1; i >= initialCount - startIndex - cardCount; i--)
        {
            Card c = node.cards[node.cards.Count - startIndex - 1];
            ReceiveCard(c, string.Empty);
        }
        if (sortCards)
        {
            cards.Sort(new Card.CardComparer());
        }
        if (revealCards)
        {
            foreach (Card card in cards)
            {
                card.SetRevealed(true, float.MaxValue);
            }
        }
    }

    public void CloseDisplay()
    {
        for (int i = cards.Count - 1; i >= 0; i--)
        {
            string paramaters = lastAcceptedParams;
            if (cards[i].flip)
            {
                paramaters += par_facedown;
            }
            if (lastAcceptedNode.Type == NodeType.RC || lastAcceptedNode.Type == NodeType.VC)
            {
                paramaters += par_bottom;
            }
            if (lastAcceptedNode.Type == NodeType.toolbox)
            {
                lastAcceptedNode.transform.localPosition = Vector3.zero;
            }
            lastAcceptedNode.ReceiveCard(cards[i], paramaters);
        }
    }
}
