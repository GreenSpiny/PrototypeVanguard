using System.Collections.Generic;
using UnityEngine;

public class Node_Drop : Node
{
    public override NodeType GetNodeType()
    {
        return NodeType.drop;
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
        for (int i = 0; i < cards.Count; i++)
        {
            Card card = cards[i];
            card.node = this;
            card.anchoredPosition = new Vector3(0f, (i * Card.cardDepth) + (Card.cardDepth / 2f), 0f);
            card.anchoredPositionOffset = Vector3.zero;
            card.flipRotation = false;
            card.LookAt(null);
            card.ToggleColliders(i == cards.Count - 1);
            if (instant)
            {
                card.transform.position = transform.position + card.anchoredPosition + card.anchoredPositionOffset;
            }
        }
    }
}
