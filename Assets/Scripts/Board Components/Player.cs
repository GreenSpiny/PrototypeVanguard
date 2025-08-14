using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using Unity.VisualScripting;
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
    public Node GC;             // shared
    public Node crest;
    public Node toolbox;
    public Node abyss;          // shared
    public List<Node> RC;

    public void AssignDeck(CardInfo.DeckList deckList)
    {
        if (this.deckList == null)
        {
            this.deckList = deckList;

            for (int i = deck.cards.Count() - 1; i >= 0; i--)
            {
                if (i < deckList.mainDeck.Count())
                {
                    CardInfo c = CardLoader.GetCardInfo(deckList.mainDeck[i]);
                    deck.cards[i].cardInfo = c;
                    deck.cards[i].SetTexture(CardLoader.GetCardImage(c.index), true);
                    deck.cards[i].SetMesh(c.rotate);
                    deck.cards[i].gameObject.SetActive(true);
                }
                else
                {
                    abyss.RecieveCard(deck.cards[i], string.Empty);
                }
            }
            deck.AlignCards(true);

            CardInfo vanguard = CardLoader.GetCardInfo(deckList.rideDeck[0]);
            VC.cards[0].cardInfo = vanguard;
            VC.cards[0].SetTexture(CardLoader.GetCardImage(vanguard.index), true);
            VC.cards[0].SetMesh(vanguard.rotate);
            VC.cards[0].gameObject.SetActive(true);

            for (int i = ride.cards.Count() - 1; i >= 0; i--)
            {
                if (i < deckList.rideDeck.Count())
                {
                    CardInfo c = CardLoader.GetCardInfo(deckList.rideDeck[i]);
                    ride.cards[i].cardInfo = c;
                    ride.cards[i].SetTexture(CardLoader.GetCardImage(c.index), true);
                    ride.cards[i].SetMesh(c.rotate);
                    ride.cards[i].gameObject.SetActive(true);
                }
                else
                {
                    abyss.RecieveCard(ride.cards[i], string.Empty);
                }
            }
            ride.AlignCards(true);

            for (int i = gzone.cards.Count() - 1; i >= 0; i--)
            {
                if (i < deckList.strideDeck.Count())
                {
                    CardInfo c = CardLoader.GetCardInfo(deckList.strideDeck[i]);
                    gzone.cards[i].cardInfo = c;
                    gzone.cards[i].SetTexture(CardLoader.GetCardImage(c.index), true);
                    gzone.cards[i].SetMesh(c.rotate);
                    gzone.cards[i].gameObject.SetActive(true);
                }
                else
                {
                    abyss.RecieveCard(gzone.cards[i], string.Empty);
                }
            }
            gzone.AlignCards(true);

            for (int i = toolbox.cards.Count() - 1; i >= 0; i--)
            {
                if (i < deckList.toolbox.Count())
                {
                    CardInfo c = CardLoader.GetCardInfo(deckList.toolbox[i]);
                    toolbox.cards[i].cardInfo = c;
                    toolbox.cards[i].SetTexture(CardLoader.GetCardImage(c.index), true);
                    toolbox.cards[i].SetMesh(c.rotate);
                    toolbox.cards[i].gameObject.SetActive(true);
                }
                else
                {
                    abyss.RecieveCard(toolbox.cards[i], string.Empty);
                }
            }
            toolbox.AlignCards(true);

            abyss.AlignCards(true);
        }
    }

}
