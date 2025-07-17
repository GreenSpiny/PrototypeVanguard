using System.Collections.Generic;
using UnityEngine;

public class Node_Display : Node
{
    public override NodeType Type => NodeType.display;
    private Node lastAcceptedNode;

    public override void RecieveCard(Card card, string parameters)
    {
        cards.Add(card);
        base.RecieveCard(card, parameters);
    }

    public override void AlignCards(bool instant)
    {
        int row = 0;
        int column = 0;
        int maxCardsPerRow = 16;

        float cardWidth = Card.cardWidth * cardScale.x;
        float totalWidth = cardWidth * maxCardsPerRow; ;
        float origin = totalWidth / 2f - cardWidth * 0.5f;

        float xSpacing = cardWidth;
        float ySpacing = 0.33f;

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

    public override IEnumerable<CardInfo.ActionFlag> GetDefaultActions()
    {
        throw new System.NotImplementedException();
    }

    public override IEnumerable<CardInfo.ActionFlag> GetSpecialActions()
    {
        throw new System.NotImplementedException();
    }

    public void OpenDisplay(Node node, int cardCount)
    {
        CloseDisplay();
        lastAcceptedNode = node;
        Card c = node.cards[node.cards.Count - 1];
        for (int i = node.cards.Count - 1; i >= node.cards.Count - cardCount;)
        {
            RecieveCard(c, string.Empty);
            i--;
            if (i >= 0)
            {
                c = node.cards[i];
            }
        }
    }

    public void CloseDisplay()
    {
        for (int i = cards.Count - 1; i >= 0; i--)
        {
            lastAcceptedNode.RecieveCard(cards[i], "cancel");
            // This could put a card in the deck face up if revealed? Be careful. TODO
        }
    }
}
