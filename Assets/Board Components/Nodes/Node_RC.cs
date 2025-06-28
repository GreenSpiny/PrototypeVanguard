using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Node_RC : Node
{
    public override NodeType GetNodeType()
    {
        return NodeType.RC;
    }

    public override bool CanDragTo() { return true; }
    public override bool CanSelectRaw() { return false; }

    public override void RecieveCard(Card card, IEnumerable<string> parameters)
    {
        bool otherNodeRC = card.node.GetNodeType() == NodeType.RC;
        bool otherNodeRCDrag = card.node.GetNodeType() == NodeType.drag && card.node.PreviousNode.GetNodeType() == NodeType.RC;

        if (otherNodeRC)
        {
            SwapAllCards(card.node, new string[0]);
        }
        if (otherNodeRCDrag)
        {
            SwapAllCards(card.node, new string[1] {"drag"});
        }
        else
        {
            for (int i = cards.Count - 1; i >= 0; i--)
            {
                Card existingCard = cards[i];
                existingCard.player.drop.RecieveCard(existingCard, new string[0]);
            }
            base.RecieveCard(card, parameters);
            cards.Add(card);
            AlignCards(false);
        }
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
                card.transform.position = cardAnchor.transform.position + card.anchoredPosition + card.anchoredPositionOffset;
            }
        }
    }
}
