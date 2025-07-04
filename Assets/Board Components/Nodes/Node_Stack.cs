using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Stack nodes contain a "stack" of cards, such as the Deck or Drop.
public class Node_Stack : Node
{
    [SerializeField] protected bool compressCards;
    public override void RecieveCard(Card card, IEnumerable<string> parameters)
    {
        base.RecieveCard(card, parameters);
        bool toBottom = parameters.Contains("bottom");
        bool facedown = parameters.Contains("facedown");
        bool faceup = parameters.Contains("faceup");
        if (toBottom)
        {
            cards.Insert(0, card);
        }
        else
        {
            cards.Add(card);
        }
        if (facedown)
        {
            card.flipRotation = true;
        }
        else if (faceup)
        {
            card.flipRotation = false;
        }
        AlignCards(false);
    }

    public override void AlignCards(bool instant)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            Card card = cards[i];
            card.node = this;
            if (compressCards)
            {
                card.anchoredPosition = Vector3.zero;
            }
            else
            {
                card.anchoredPosition = new Vector3(0f, (Card.cardDepth / 2f) + (i * Card.cardDepth), 0f);
            }
            card.LookAt(null);
            card.ToggleColliders(i == cards.Count - 1 && !compressCards);
        }
        base.AlignCards(instant);
    }

    public override IEnumerable<CardInfo.ActionFlag> GetDefaultActions()
    {
        throw new System.NotImplementedException();
    }

    public override IEnumerable<CardInfo.ActionFlag> GetSpecialActions()
    {
        throw new System.NotImplementedException();
    }
}
