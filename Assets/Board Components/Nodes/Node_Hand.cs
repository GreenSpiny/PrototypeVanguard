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

        if (cards.Count >= 7)
        {
            totalWidth = Card.cardWidth * 7f;
            spacing = (totalWidth - Card.cardWidth) / (cards.Count - 1);
        }
        else
        {
            spacing = Card.cardWidth + Card.cardWidth * 0.1f;
            totalWidth = Card.cardWidth * cards.Count + 0.1f * (cards.Count - 1);
        }
        
        float originX = -totalWidth / 2f + Card.cardWidth / 2f;

        for (int i = 0; i < cards.Count; i++)
        {
            Card card = cards[i];
            card.node = this;
            card.anchoredPosition = new Vector3(originX + spacing * i, 0f, 0f);
            card.lookTarget = card.player.playerCamera.transform;
            card.flipRotation = false;
            card.ToggleColliders(true);
            if (instant)
            {
                card.transform.position = transform.position + card.anchoredPosition + card.anchoredPositionOffset;
            }
        }
    }
}
