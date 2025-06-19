using System.Collections.Generic;
using UnityEngine;

public class Node_Hand : Node
{
    public override NodeType GetNodeType()
    {
        return NodeType.hand;
    }

    public override bool DefaultSelectable()
    {
        return false;
    }

    public override void RecieveCard(Card card, IEnumerable<string> parameters)
    {
        base.RecieveCard(card, parameters);
        cards.Add(card);
        AlignCards(false);
    }

    public override void AlignCards(bool instant)
    {
        float spacing = 0f;
        float totalWidth = 0f;
        float originX = 0f;

        if (cards.Count >= 5)
        {
            totalWidth = Card.cardWidth * 5f;
            spacing = (totalWidth - Card.cardWidth) / (cards.Count - 1);
            originX = -totalWidth / 2f + Card.cardWidth / 2f;
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
            card.anchoredPosition = new Vector3(originX + spacing * i, 0f, 0f);
            card.flipRotation = false;
            card.LookAt(card.player.playerCamera.transform);
            card.ToggleColliders(true);
            if (instant)
            {
                card.transform.position = transform.position + card.anchoredPosition + card.anchoredPositionOffset;
            }
        }
    }
}
