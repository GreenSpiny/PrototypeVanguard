using System.Collections.Generic;
using UnityEngine;

// Stack nodes contain a "stack" of cards, such as the Deck or Drop.
public class Node_Stack : Node
{
    [SerializeField] protected bool compressCards;
    public override void RecieveCard(Card card, string parameters)
    {
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
        
        base.RecieveCard(card, parameters);

        if (cards.Contains(card))
        {
            if (facedown)
            {
                card.SetOrientation(true, false);
            }
            else if (faceup)
            {
                card.SetOrientation(false, false);
            }
        }
    }

    public override void AlignCards(bool instant)
    {
        if (HasCard)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                Card card = cards[i];
                if (compressCards)
                {
                    card.anchoredPosition = Vector3.zero;
                }
                else
                {
                    card.anchoredPosition = new Vector3(0f, (Card.cardDepth / 2f) + (i * Card.cardDepth), 0f);
                }
                card.SetOrientation(card.flip, TopCard.rest);
                card.LookAt(null);
                card.ToggleColliders(i == cards.Count - 1 && !compressCards);
                if (i == cards.Count - 1)
                {
                    verticalOffsetUI = (i + 1) * Card.cardDepth;
                }
            }
        }
        base.AlignCards(instant);
    }

    public override List<CardInfo.ActionFlag> GenerateDefaultCardActions()
    {
        return new List<CardInfo.ActionFlag>();
    }

}
