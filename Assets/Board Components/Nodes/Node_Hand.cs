using System.Collections.Generic;
using UnityEngine;

public class Node_Hand : Node
{
    public override NodeType GetNodeType()
    {
        return NodeType.hand;
    }

    public override bool CanDragTo() { return true; }
    public override bool CanSelectRaw() { return false; }

    public override void RecieveCard(Card card, IEnumerable<string> parameters)
    {
        base.RecieveCard(card, parameters);
        cards.Add(card);
        AlignCards(false);
    }

    public override void AlignCards(bool instant)
    {
        int maxCards = 8;

        float spacing = 0f;
        float totalWidth = 0f;
        float originX = 0f;
        float yOffset = 0f;

        if (cards.Count >= maxCards)
        {
            totalWidth = Card.cardWidth * maxCards;
            spacing = (totalWidth - Card.cardWidth) / (cards.Count - 1);
            originX = -totalWidth / 2f + Card.cardWidth / 2f;
            if (cards.Count > maxCards)
            {
                yOffset = -Card.cardDepth;
            }
        }
        else
        {
            totalWidth = Card.cardWidth * cards.Count + 0.1f * (cards.Count - 1);
            spacing = Card.cardWidth + Card.cardWidth * 0.1f;
            originX = -totalWidth / 2f + Card.cardWidth / 2f + Card.cardWidth * 0.05f;
        }

        for (int i = 0; i < cards.Count; i++)
        {
            Card card = cards[i];
            card.node = this;
            card.anchoredPosition = new Vector3(originX + spacing * i, i * yOffset, 0f);
            card.flipRotation = false;
            card.LookAt(card.player.playerCamera.transform);
            card.ToggleColliders(true);
            if (instant)
            {
                card.transform.position = cardAnchor.transform.position + card.anchoredPosition + card.anchoredPositionOffset;
            }
        }
    }
}
