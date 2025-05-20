using System.Collections.Generic;
using UnityEngine;

public class Node_Hand : Node
{
    public override NodeType GetNodeType()
    {
        return NodeType.hand;
    }

    public override void RecieveCard(Card card, IEnumerable<string> parameters)
    {
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
            card.Node = this;
            card.anchoredPosition = new Vector3(transform.position.x + (i * Card.cardWidth * 1.1f), transform.position.y, transform.position.z);
            card.lookTarget = Camera.main.transform;
            card.ToggleColliders(true);
            if (instant)
            {
                card.transform.position = card.anchoredPosition;
            }
        }
    }
}
