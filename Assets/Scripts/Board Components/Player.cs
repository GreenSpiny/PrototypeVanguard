using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
public class Player : MonoBehaviour
{
    [SerializeField] public int playerIndex;
    [NonSerialized] public CardInfo.DeckList deckList = null;

    // All entities owned by this player
    public Camera playerCamera;
    public List<Node> nodes = new List<Node>();

    // Specific entities owned by this player
    public Node hand;
    public Node deck;
    public Node drop;
    public Node bind;
    public Node remove;
    public Node damage;
    public Node order;
    public Node gzone;  // Also serves as Gauge
    public Node ride;
    public Node VC;
    public Node_Display display;
    public Node GC;                 // Special because shared - for now, assign in inspector
    public List<Node> RC;

    public void AssignDeck(CardInfo.DeckList deckList)
    {
        if (this.deckList == null)
        {
            this.deckList = deckList;

            for (int i = 0; i < deck.cards.Count(); i++)
            {
                CardInfo c = CardLoader.GetCardInfo(deckList.mainDeck[i]);
                deck.cards[i].cardInfo = c;
                deck.cards[i].SetTexture(CardLoader.GetCardImage(c.index), true);
            }

            for (int i = 0; i < VC.cards.Count(); i++)
            {
                CardInfo c = CardLoader.GetCardInfo(deckList.rideDeck[i]);
                VC.cards[i].cardInfo = c;
                VC.cards[i].SetTexture(CardLoader.GetCardImage(c.index), true);
            }

            for (int i = 0; i < ride.cards.Count(); i++)
            {
                CardInfo c = CardLoader.GetCardInfo(deckList.rideDeck[i]);
                ride.cards[i].cardInfo = c;
                ride.cards[i].SetTexture(CardLoader.GetCardImage(c.index), true);
            }

            for (int i = 0; i < gzone.cards.Count(); i++)
            {
                CardInfo c = CardLoader.GetCardInfo(deckList.strideDeck[i]);
                gzone.cards[i].cardInfo = c;
                gzone.cards[i].SetTexture(CardLoader.GetCardImage(c.index), true);
            }
        }
    }

}
