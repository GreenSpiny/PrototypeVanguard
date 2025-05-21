using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.GPUSort;

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
            card.anchoredPosition = new Vector3(0f, (i * Card.cardDepth) + (Card.cardDepth / 2f), 0f);
            card.anchoredPositionOffset = Vector3.zero;
            card.lookTarget = null;
            card.ToggleColliders(i == cards.Count - 1);
            if (instant)
            {
                card.transform.position = transform.position + card.anchoredPosition + card.anchoredPositionOffset;
            }
        }
    }
}
