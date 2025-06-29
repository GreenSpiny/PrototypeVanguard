using System.Collections.Generic;
using UnityEngine;

public class Node_Deck : Node
{
    public override NodeType GetNodeType()
    {
        return NodeType.deck;
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
            card.flipRotation = true;
            card.LookAt(null);
            card.ToggleColliders(i == cards.Count - 1);
            base.AlignCards(instant);
        }
    }

    public override void AutoAction(Card clickedCard)
    {
        Node hand = clickedCard.player.hand;
        hand.RecieveCard(clickedCard, new string[0]);
    }
}
