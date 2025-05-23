using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Node_RC : Node
{
    public override NodeType GetNodeType()
    {
        return NodeType.RC;
    }

    public override bool DefaultSelectable()
    {
        return false;
    }

    public override void RecieveCard(Card card, IEnumerable<string> parameters)
    {
        for (int i = cards.Count - 1; i >= 0; i--)
        {
            Card existingCard = cards[i];
            existingCard.player.drop.RecieveCard(existingCard, new string[0]);
        }

        // base resolution
        base.RecieveCard(card, parameters);
        cards.Add(card);
        AlignCards(false);
    }

    protected override void RemoveCard(Card card)
    {
        base.RemoveCard(card);
    }

    public override void AlignCards(bool instant)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            Card card = cards[i];
            card.node = this;
            card.anchoredPosition = new Vector3(0f, (i * Card.cardDepth) + (Card.cardDepth / 2f), 0f);
            card.anchoredPositionOffset = Vector3.zero;
            card.lookTarget = null;
            card.flipRotation = false;
            card.ToggleColliders(i == cards.Count - 1);
            if (instant)
            {
                card.transform.position = transform.position + card.anchoredPosition + card.anchoredPositionOffset;
            }
        }
    }
}
