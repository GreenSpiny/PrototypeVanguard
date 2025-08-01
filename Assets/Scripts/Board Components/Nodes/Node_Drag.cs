using System.Collections.Generic;
using UnityEngine;

public class Node_Drag : Node
{
    public override NodeType Type => NodeType.drag;
    public override void RecieveCard(Card card, string parameters)
    {
        cardRotation = card.node.cardRotation;
        cards.Add(card);
        base.RecieveCard(card, parameters);
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

    protected override List<CardInfo.ActionFlag> GenerateDefaultCardActions()
    {
        return new List<CardInfo.ActionFlag>();
    }

    protected override List<CardInfo.ActionFlag> GenerateDefaultNodeActions()
    {
        return new List<CardInfo.ActionFlag>();
    }
}
