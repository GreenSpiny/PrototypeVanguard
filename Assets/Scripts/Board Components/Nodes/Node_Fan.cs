using System.Collections.Generic;
using UnityEngine;

// Fan nodes contain a "fan" of cards, such as the Hand or Damage.
public class Node_Fan : Node
{
    protected enum FanDirection { vertical, horizontal }
    protected enum FanOrigin { center, edge }

    [SerializeField] protected FanDirection fanDirection;
    [SerializeField] protected FanOrigin fanOrigin;
    [SerializeField] protected bool reverse;
    [SerializeField] protected bool descend;
    [SerializeField] protected int maxCards;
    [SerializeField] protected float maxWidth;
    [SerializeField] protected float defaultSpacing;
    [SerializeField] protected bool lookAtCamera;

    public override void RecieveCard(Card card, string parameters)
    {
        cards.Add(card);
        base.RecieveCard(card, parameters);
    }

    public override void AlignCards(bool instant)
    {
        if (HasCard)
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
                if (descend)
                {
                    yOffset *= -1f;
                }
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
                if (fanDirection == FanDirection.vertical)
                {
                    if (reverse)
                    {
                        card.anchoredPosition = new Vector3(0f, i * yOffset, origin + spacing * i);
                    }
                    else
                    {
                        card.anchoredPosition = new Vector3(0f, i * yOffset, -origin - spacing * i);
                    }
                }
                else
                {
                    if (reverse)
                    {
                        card.anchoredPosition = new Vector3(-origin - spacing * i, i * yOffset, 0f);
                    }
                    else
                    {
                        card.anchoredPosition = new Vector3(origin + spacing * i, i * yOffset, 0f);
                    }
                }
                if (lookAtCamera)
                {
                    card.LookAt(cameraTransform);
                }
                else
                {
                    card.LookAt(null);
                }
                card.ToggleColliders(true);
            }
        }
        base.AlignCards(instant);
    }

    protected override List<CardInfo.ActionFlag> GenerateDefaultCardActions()
    {
        return new List<CardInfo.ActionFlag>();
    }

    protected override List<CardInfo.ActionFlag> GenerateDefaultNodeActions()
    {
        return new List<CardInfo.ActionFlag>();
    }
}
