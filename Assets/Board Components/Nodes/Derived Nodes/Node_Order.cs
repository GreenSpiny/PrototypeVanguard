using System.Collections.Generic;
using UnityEngine;

public class Node_Order : Node
{
    public override void RecieveCard(Card card, IEnumerable<string> parameters)
    {
        base.RecieveCard(card, parameters);
        cards.Add(card);
        AlignCards(false);
    }

    public override void AlignCards(bool instant)
    {
        int maxCards = 4;

        float spacing = 0f;
        float totalWidth = 0f;
        float originX = Card.cardWidth * 0.5f * cardScale.x;
        float yOffset = Card.cardDepth;

        if (cards.Count >= maxCards)
        {
            totalWidth = Card.cardHeight;
            spacing = (totalWidth - Card.cardWidth * cardScale.x) / (cards.Count - 1);
        }
        else
        {
            spacing = Card.cardWidth * 0.5f * cardScale.x;
            totalWidth = Card.cardWidth * cardScale.x + (Card.cardWidth * (cards.Count - 1f) * spacing);
        }

        for (int i = 0; i < cards.Count; i++)
        {
            Card card = cards[i];
            card.node = this;
            card.anchoredPosition = new Vector3(originX + spacing * i, i * yOffset, 0f);
            card.LookAt(null);
            card.ToggleColliders(true);
            base.AlignCards(instant);
        }
    }
}
