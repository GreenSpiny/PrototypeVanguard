using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.GPUSort;

public class Node_Deck : Node
{
    public override NodeType GetNodeType()
    {
        return NodeType.deck;
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
            card.anchoredPosition = new Vector3(transform.position.x, transform.position.y + (i * Card.cardDepth) + (Card.cardDepth / 2f), transform.position.z);
            card.lookTarget = null;
            card.ToggleColliders(i == cards.Count - 1);
            if (instant)
            {
                card.transform.position = card.anchoredPosition;
            }
        }
    }
}
