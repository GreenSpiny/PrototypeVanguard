using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
public class Player : MonoBehaviour
{
    [SerializeField] public int playerIndex;
    [NonSerialized] public CardInfo.DeckList deckList;

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
        this.deckList = deckList;
        for (int i = 0; i < deck.cards.Count(); i++)
        {
            deck.cards[i].cardInfo = CardLoader.GetCardInfo(deckList.mainDeck[i]);
        }
        VC.cards[0].cardInfo = CardLoader.GetCardInfo(deckList.rideDeck[0]);
        for (int i = 0; i < ride.cards.Count(); i++)
        {
            ride.cards[i].cardInfo = CardLoader.GetCardInfo(deckList.rideDeck[i+1]);
        }
        for (int i = 0; i < gzone.cards.Count(); i++)
        {
            gzone.cards[i].cardInfo = CardLoader.GetCardInfo(deckList.strideDeck[i]);
        }
    }

}
