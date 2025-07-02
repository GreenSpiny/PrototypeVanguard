using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Fan nodes contain a "fan" of cards, such as the Hand or Damage.
public class Node_Fan : Node
{
    protected enum FanDirection { vertical, horizontal }
    protected enum FanOrigin { center, edge }

    [SerializeField] protected FanDirection fanDirection;
    [SerializeField] protected FanOrigin fanOrigin;
    [SerializeField] protected int maxCards;
    [SerializeField] protected float maxWidth;
    [SerializeField] protected float defaultSpacing;
    [SerializeField] Transform lookTarget;
    public override void RecieveCard(Card card, IEnumerable<string> parameters)
    {
        base.RecieveCard(card, parameters);
        cards.Add(card);
        AlignCards(false);
    }

    public override void AlignCards(bool instant)
    {
        float totalWidth = 0f;
        float spacing = 0f;
        float origin = 0f;
        float yOffset = 0f;

        float scaledCardWidth = Card.cardWidth * cardScale.x;
        float scaledSpacingFactor = Card.cardWidth * defaultSpacing * cardScale.x;

        if (cards.Count >= maxCards)
        {
            if (maxWidth < 0.1f)
            {
                totalWidth = scaledCardWidth * maxCards;
            }
            else
            {
                totalWidth = maxWidth;
            }
            spacing = (totalWidth - scaledCardWidth) / (cards.Count - 1);
        }
        else
        {
            totalWidth = scaledCardWidth + scaledSpacingFactor * (cards.Count - 1);
            spacing = scaledSpacingFactor;
        }

        if (cards.Count > maxCards || defaultSpacing < 1f)
        {
            yOffset = Card.cardDepth;
        }

        if (fanOrigin == FanOrigin.edge)
        {
            origin = scaledCardWidth / 2f;
        }
        else
        {
            origin = -totalWidth / 2f + scaledCardWidth / 2f;
        }

        for (int i = 0; i < cards.Count; i++)
        {
            Card card = cards[i];
            card.node = this;
            if (fanDirection == FanDirection.vertical)
            {
                card.anchoredPosition = new Vector3(0f, i * yOffset, -origin - spacing * i);
            }
            else
            {
                card.anchoredPosition = new Vector3(origin + spacing * i, i * yOffset, 0f);
            }
            card.LookAt(lookTarget);
            card.ToggleColliders(true);
        }

        base.AlignCards(instant);
    }
}
