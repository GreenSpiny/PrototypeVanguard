
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

// Nodes are locations on the board that cards are anchored to. They recieve and arrange cards by their own devices.
// Examples include the hand, deck, all zones, and all unit circles.
public abstract class Node : MonoBehaviour
{
    public enum NodeType { none, drag, hand, deck, drop, bind, remove, trigger, damage, order, gzone, ride, VC, RC, GC }
    public bool HasCard { get { return cards.Count > 0; } }     // True if the node contains at least one card

    [NonSerialized] protected List<Card> cards = new List<Card>();  // The cards attached to this node
    [NonSerialized] public Node PreviousNode;                       // The previous Node of the most recently attached card
    [NonSerialized] public Player player;                           // The player who owns this node

    public virtual NodeType Type { get { return NodeType.none; } }
    [SerializeField] public bool canDragTo;             // If true, this node can accept dragged cards
    [SerializeField] public bool canSelectRaw;          // If true, this node can be selected when empty
    [SerializeField] public bool preserveRest;          // If true, preserve the REST state of cards moved to this node
    [SerializeField] public bool preserveFlip;          // If true, preserve the FLIP state of cards moved to this node

    [SerializeField] public Transform cardAnchor;       // The default position and rotation of cards attached to this node
    [SerializeField] public Vector3 cardRotation;       // The default euler offset of cards attached to this node
    [SerializeField] public Vector3 cardScale;          // The default scale offset of cards attached to this node
    [SerializeField] public Vector3 nudgeDistance;      // If and how far cards on this node "nudge" when hovered, as feedback

    public int nodeID { get; private set; }     // Unique node identifier for networking purposes

    // Dirty nodes are realigned on the next update cycle.
    protected bool isDirty = false;
    public void SetDirty() { isDirty = true; }

    // Animation properties
    [SerializeField] protected NodeAnimationInfo animInfo;

    public enum NodeUIState { normal, available, hovered, selected };
    protected NodeUIState state;

    public void Init(int nodeID)
    {
        this.nodeID = nodeID;
        player = GetComponentInParent<Player>();
        animInfo.Initialize();
        if (cardAnchor == null)
        {
            cardAnchor = transform;
        }
        foreach (var child in GetComponentsInChildren<Card>())
        {
            cards.Add(child);
            child.Init(this);
        }
        AlignCards(true);
    }

    private void Update()
    {
        animInfo.Animate();
        if (isDirty)
        {
            AlignCards(false);
        }
    }

    public virtual void RecieveCard(Card card, IEnumerable<string> parameters)
    {
        if (true) // TODO: make recording logic for Drag, etc
        {
            card.player.RecordMoveAction(card, this, parameters);
        }
        if (card.node != this)
        {
            if (!preserveRest)
            {
                card.rest = false;
            }
            if (!preserveFlip)
            {
                card.flip = false;
            }
            PreviousNode = card.node;
            PreviousNode.RemoveCard(card);
            card.node = this;
        }
    }

    public virtual void RetireCards()
    {
        if (Type != NodeType.drop)
        {
            for (int i = cards.Count - 1; i >= 0; i--)
            {
                cards[i].player.drop.RecieveCard(cards[i], new string[0]);
            }
        }
    }

    public virtual void SwapAllCards(Node otherNode, IEnumerable<string> parameters)
    {
        // When swapping cards, intermediary nodes (i.e. Drag) must be accounted for
        bool drag = parameters.Contains("drag");

        // Create shallow copies of the card data, then clear the original data
        List<Card> selfCardsShallowCopy = new List<Card>();
        foreach (Card card in cards)
        {
            selfCardsShallowCopy.Add(card);
        }
        cards.Clear();

        List<Card> otherCardsShallowCopy = new List<Card>();
        foreach (Card c in otherNode.cards)
        {
            otherCardsShallowCopy.Add(c);
        }
        otherNode.cards.Clear();

        if (drag)
        {
            foreach (Card c in otherNode.PreviousNode.cards)
            {
                otherCardsShallowCopy.Add(c);
            }
            otherNode.PreviousNode.cards.Clear();
        }

        // Assign the cards to their new nodes
        foreach (Card c in selfCardsShallowCopy)
        {
            if (drag)
            {
                otherNode.PreviousNode.cards.Add(c);
            }
            else
            {
                otherNode.cards.Add(c);
            }
        }

        foreach (Card c in otherCardsShallowCopy)
        {
            cards.Add(c);
        }

        // Flag all nodes potentially involved as dirty
        SetDirty();
        otherNode.SetDirty();
        otherNode.PreviousNode.SetDirty();
    }

    private void RemoveCard(Card card)
    {
        if (cards.Contains(card))
        {
            cards.Remove(card);
            SetDirty();
        }
    }

    public virtual void AlignCards(bool instant)
    {
        isDirty = false;
        if (instant)
        {
            foreach (Card card in cards)
            {
                card.transform.position = transform.position + card.anchoredPosition + card.anchoredPositionOffset;
                card.transform.rotation = Quaternion.Euler(card.targetEuler);
                card.transform.localScale = cardScale;
            }
        }
        foreach (Card card in cards)
        {
            card.anchoredPositionOffset = Vector3.zero;
        }
    }

    public virtual void CardAutoAction(Card clickedCard) { }
    public virtual void NodeAutoAction() { }
    public abstract IEnumerable<CardInfo.ActionFlag> GetDefaultActions();
    public abstract IEnumerable<CardInfo.ActionFlag> GetSpecialActions();
    public IEnumerable<CardInfo.ActionFlag> GetActions()
    {
        List<CardInfo.ActionFlag> actions = new List<CardInfo.ActionFlag>();
        foreach (var action in GetDefaultActions())
        {
            actions.Add(action);
        }
        foreach (var action in GetSpecialActions())
        {
            actions.Add(action);
        }
        return actions;
    }

    // ===== ANIMATION SECTION ===== //
    public NodeUIState UIState
    {
        get
        {
            return state;
        }
        set
        {
            state = value;
            if (state == NodeUIState.normal)
            {
                animInfo.flashColor = Color.white;
                animInfo.flashColor.a = 0f;
                animInfo.instantColor = false;
            }
            else if (state == NodeUIState.available)
            {
                animInfo.flashColor = Color.red;
                animInfo.flashColor.a = 0.2f;
                animInfo.instantColor = false;
            }
            else if (state == NodeUIState.hovered)
            {
                animInfo.flashColor = Color.yellow;
                animInfo.flashColor.a = 0.3f;
                animInfo.instantColor = true;
            }
            else if (state == NodeUIState.selected)
            {
                animInfo.flashColor = Color.blue;
                animInfo.flashColor.a = 0.4f;
                animInfo.instantColor = true;
            }
        }
    }

    [System.Serializable]
    public class NodeAnimationInfo
    {
        [SerializeField] public SpriteRenderer flashRenderer;
        [NonSerialized] public Color flashColor = new Color(1f, 1f, 1f, 0f);
        [NonSerialized] public bool instantColor = false;

        static float transitionSpeed = 10f;

        public bool CanAnimate { get { return flashRenderer != null; } }

        public void Initialize()
        {
            if (CanAnimate)
            {
                flashRenderer.color = flashColor;
            }
        }

        public void Animate()
        {
            if (CanAnimate)
            {
                if (instantColor)
                {
                    flashRenderer.color = flashColor;
                }
                else
                {
                    flashRenderer.color = Color.Lerp(flashRenderer.color, flashColor, Time.deltaTime * transitionSpeed);
                }
            }
        }
    }

}
