using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.GPUSort;

public class Node_Drag : Node
{
    public override NodeType GetNodeType()
    {
        return NodeType.drag;
    }
    private void Update()
    {
        if (HasCard)
        {
            AlignCards(false);
        }
    }
    public override bool DefaultSelectable()
    {
        return false;
    }
    public override void RecieveCard(Card card, IEnumerable<string> parameters)
    {
        foreach (Card c in cards)
        {
            RemoveCard(c);
        }
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
        if (HasCard)
        {
            Card card = cards[0];
            card.node = this;
            card.anchoredPosition = Vector3.zero;
            card.anchoredPositionOffset = Vector3.zero;
            card.lookTarget = null;
            card.ToggleColliders(true);
            if (instant)
            {
                card.transform.position = transform.position + card.anchoredPosition + card.anchoredPositionOffset;
            }
        }
    }
}
