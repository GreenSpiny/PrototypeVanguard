using System.Collections.Generic;
using UnityEngine;

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
    public override bool CanDragTo() { return false; }
    public override bool CanSelectRaw() { return false; }
    public override void RecieveCard(Card card, IEnumerable<string> parameters)
    {
        base.RecieveCard(card, parameters);
        cards.Add(card);
        AlignCards(false);
    }

    public override void AlignCards(bool instant)
    {
        if (HasCard)
        {
            Card card = cards[0];
            card.node = this;
            card.anchoredPosition = Vector3.zero;
            card.anchoredPositionOffset = Vector3.zero;
            card.LookAt(null);
            card.ToggleColliders(true);
            if (instant)
            {
                card.transform.position = transform.position + card.anchoredPosition + card.anchoredPositionOffset;
            }
        }
    }
}
