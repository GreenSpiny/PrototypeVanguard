using System.Collections.Generic;
using UnityEngine;

public class Node_Bind : Node
{
    public override NodeType GetNodeType()
    {
        return NodeType.bind;
    }

    public override bool CanDragTo() { return true; }
    public override bool CanSelectRaw() { return true; }

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
            card.anchoredPosition = Vector3.zero;
            card.anchoredPositionOffset = Vector3.zero;
            card.LookAt(null);
            card.ToggleColliders(false);
            base.AlignCards(instant);
        }
    }
}
