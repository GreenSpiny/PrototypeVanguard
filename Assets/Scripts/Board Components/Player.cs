using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [SerializeField] public int playerIndex;
    [NonSerialized] public CardInfo.DeckList deckList = null;
    private int energy = 0;
    [SerializeField] private Animator energyAnimator;

    // All entities owned by this player
    public Camera playerCamera;
    public List<Node> nodes = new List<Node>();
    [NonSerialized] public List<Card> cards = new List<Card>();

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
    public Node GC;             // shared between players
    public Node crest;
    public Node toolbox;
    public Node abyss;          // shared between players
    public List<Node> RC;

    [SerializeField] private TextMeshProUGUI energyCountText;

    // Global auto actions granted to this player
    public HashSet<CardInfo.ActionFlag> playerActionFlags = new HashSet<CardInfo.ActionFlag>();

    // Board UI elements visible to this player
    public GameObject[] ownedUIRoots;
    public RectTransform[] reversedUIRoots;
    [SerializeField] private Button previousPhaseButton;
    [SerializeField] private Button nextPhaseButton;
    [SerializeField] TextMeshProUGUI phaseText;

    public void AssignDeck(CardInfo.DeckList deckList)
    {
        if (this.deckList == null)
        {
            this.deckList = deckList;

            for (int i = deck.cards.Count() - 1; i >= 0; i--)
            {
                cards.Add(deck.cards[i]);
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
                    abyss.ReceiveCard(deck.cards[i], string.Empty);
                }
            }

            cards.Add(VC.cards[0]);
            CardInfo vanguard = CardLoader.GetCardInfo(deckList.rideDeck[0]);
            VC.cards[0].cardInfo = vanguard;
            VC.cards[0].SetTexture(CardLoader.GetCardImage(vanguard.index), true);
            VC.cards[0].SetMesh(vanguard.rotate);
            VC.cards[0].gameObject.SetActive(true);

            cards.Add(crest.cards[0]);
            int generatorIndex = 1259;
            CardInfo energyGenerator = CardLoader.GetCardInfo(generatorIndex);
            crest.cards[0].cardInfo = energyGenerator;
            crest.cards[0].SetTexture(CardLoader.GetCardImage(generatorIndex), true);
            crest.cards[0].SetMesh(energyGenerator.rotate);
            crest.cards[0].gameObject.SetActive(true);

            for (int i = ride.cards.Count() - 1; i >= 0; i--)
            {
                cards.Add(ride.cards[i]);
                if (i != 0 && i < deckList.rideDeck.Count())
                {
                    CardInfo c = CardLoader.GetCardInfo(deckList.rideDeck[i]);
                    ride.cards[i].cardInfo = c;
                    ride.cards[i].SetTexture(CardLoader.GetCardImage(c.index), true);
                    ride.cards[i].SetMesh(c.rotate);
                    ride.cards[i].gameObject.SetActive(true);
                }
                else
                {
                    abyss.ReceiveCard(ride.cards[i], string.Empty);
                }
            }
            ride.cards.Reverse();
            ride.AlignCards(true);

            for (int i = gzone.cards.Count() - 1; i >= 0; i--)
            {
                cards.Add(gzone.cards[i]);
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
                    abyss.ReceiveCard(gzone.cards[i], string.Empty);
                }
            }
            gzone.AlignCards(true);

            for (int i = toolbox.cards.Count() - 1; i >= 0; i--)
            {
                cards.Add(toolbox.cards[i]);
                if (i < deckList.toolbox.Count())
                {
                    CardInfo c = CardLoader.GetCardInfo(deckList.toolbox[i]);
                    toolbox.cards[i].cardInfo = c;
                    toolbox.cards[i].SetTexture(CardLoader.GetCardImage(c.index), true);
                    toolbox.cards[i].SetMesh(c.rotate);
                    toolbox.cards[i].gameObject.SetActive(true);
                    toolbox.cards[i].isToolboxCard = true;
                }
                else
                {
                    abyss.ReceiveCard(toolbox.cards[i], string.Empty);
                }
            }
            toolbox.AlignCards(true);
            abyss.AlignCards(true);
        }
    }

    public void OnPhaseChanged()
    {
        if (GameManager.instance != null)
        {
            bool isTurnPlayer = GameManager.instance.turnPlayer == playerIndex;
            GameManager.Phase phase = GameManager.instance.phase;

            phaseText.text = GameManager.phaseNames[(int)phase];
            if (isTurnPlayer || GameManager.singlePlayer)
            {
                bool canGoBack = (phase == GameManager.Phase.ride && GameManager.instance.turnCount == 0) || (phase > GameManager.Phase.ride);
                previousPhaseButton.interactable = canGoBack;
                nextPhaseButton.interactable = true;
            }
            else
            {
                previousPhaseButton.interactable = false;
                nextPhaseButton.interactable = false;
            }
        }
    }
    public void RequestIncrementEnergy(int amount)
    {
        int originalEnergy = energy;
        int newEnergy = Mathf.Clamp(energy + amount, 0, 10);
        if (newEnergy != originalEnergy)
        {
            GameManager.instance.RequestSetEnergyRpc(playerIndex, newEnergy);
        }
    }

    public void IncrementEnergy(int amount)
    {
        int originalEnergy = energy;
        int newEnergy = Mathf.Clamp(energy + amount, 0, 10);
        if (newEnergy != originalEnergy)
        {
            SetEnergy(newEnergy);
        }
    }

    public void SetEnergy(int amount)
    {
        energy = amount;
        energyCountText.text = energy.ToString();
        energyAnimator.Play("energy pulse", 0, 0f);
    }

}
