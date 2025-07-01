using System.Collections.Generic;
using UnityEngine;

public class Node_Drag : Node
{
    public override void RecieveCard(Card card, IEnumerable<string> parameters)
    {
        cardRotation = card.node.cardRotation;
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
            base.AlignCards(instant);
        }
    }
}
