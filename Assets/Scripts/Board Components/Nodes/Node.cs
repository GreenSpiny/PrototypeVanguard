using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Mathematics;

// Nodes are locations on the board that cards are anchored to. They recieve and arrange cards by their own devices.
// Examples include the hand, deck, all zones, and all unit circles.
public abstract class Node : MonoBehaviour
{
    public static Transform cameraTransform;
    public enum NodeType { none, drag, hand, deck, drop, bind, remove, trigger, damage, order, gzone, ride, VC, RC, GC, display, toolbox, crest, abyss }
    public bool HasCard { get { return cards.Count > 0; } }         // True if the node contains at least one card
    public Card BottomCard {  get { return cards[0]; } }
    public Card TopCard { get { return cards[cards.Count - 1]; } }

    [NonSerialized] public List<Card> cards = new List<Card>();     // The cards attached to this node
    public Node PreviousNode { get; protected set; }                // The previous Node of the most recently attached card
    [NonSerialized] public Player player;                           // The player who owns this node

    public virtual NodeType Type { get { return NodeType.none; } }
    [SerializeField] public bool canDragTo;             // If true, this node can accept dragged cards
    [SerializeField] public bool canSelectRaw;          // If true, this node can be selected when empty
    [SerializeField] public bool preserveRest;          // If true, preserve the REST state of cards moved to this node
    [SerializeField] public bool preserveFlip;          // If true, preserve the FLIP state of cards moved to this node
    [SerializeField] public bool preservePower;         // If true, preserves the power / crit / drive of cards moved to this node
    [SerializeField] public bool initialFlip;           // If true, cards are initially flipped on this node
    [SerializeField] public bool privateKnowledge;      // If true, cards are flipped in the opponent's eyes only
    [SerializeField] public bool acceptToolbox;         // If true, this node accepts Toolbox cards rather than returning them to the Toolbox.
    [SerializeField] public bool previewText;

    [SerializeField] public Transform cardAnchor;       // The default position and rotation of cards attached to this node
    [SerializeField] public Vector3 cardRotation;       // The default euler offset of cards attached to this node
    [SerializeField] public Vector3 cardScale;          // The default scale offset of cards attached to this node
    [SerializeField] public Vector3 nudgeDistance;      // If and how far cards on this node "nudge" when hovered, as feedback

    [SerializeField] public NodeUI NodeUI;
    protected float verticalOffsetUI;

    // Const parameters
    public const string par_facedown = "facedown";
    public const string par_faceup = "faceup";
    public const string par_cancel = "cancel";
    public const string par_bottom = "bottom";

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
        if (NodeUI != null)
        {
            NodeUI.Init(this);
        }
        AlignCards(true);
    }

    private void Update()
    {
        animInfo.Animate();
        if (NodeUI != null)
        {
            NodeUI.Animate();
        }
        if (isDirty)
        {
            AlignCards(false);
        }
    }

    public virtual void RecieveCard(Card card, string parameters)
    {

        // Redirect incompatible cards
        if (Type != NodeType.toolbox && card.isToolboxCard && !acceptToolbox)
        {
            if (cards.Contains(card))
            {
                cards.Remove(card);
            }
            card.player.toolbox.RecieveCard(card, string.Empty);
            card.player.toolbox.transform.position = transform.position;
            SetDirty();
            return;
        }

        // Otherwise, manage node ownership normally
        bool cancel = parameters.Contains(par_cancel);

        bool shouldFlip = card.flip;
        bool shouldRest = card.rest;

        if (!cancel)
        {
            if (!preserveFlip)
            {
                shouldFlip = false;
            }
            if (!preserveRest)
            {
                shouldRest = false;
            }
            if (!preservePower)
            {
                card.ResetPower();
            }
        }
            
        PreviousNode = card.node;
        PreviousNode.RemoveCard(card);
        card.node = this;
        card.SetOrientation(shouldFlip, shouldRest);
        ResetRevealed();
        SetDirty();
    }

    public virtual void RetireCards()
    {
        if (Type != NodeType.drop)
        {
            for (int i = cards.Count - 1; i >= 0; i--)
            {
                cards[i].player.drop.RecieveCard(cards[i], string.Empty);
            }
        }
    }

    private void RemoveCard(Card card)
    {
        if (cards.Contains(card))
        {
            cards.Remove(card);
            SetDirty();
        }
    }

    private void ResetRevealed()
    {
        foreach (Card card in cards)
        {
            card.SetRevealed(false, 0f);
        }
    }

    public void Shuffle(int randomSeed, bool instant)
    {
        RandomUtility.Shuffle(RandomUtility.GenerateRandomBySeed(randomSeed), cards);
        AlignCards(true);
        if (!instant)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                float xOffset = 0;
                if (i != cards.Count - 1)
                {
                    xOffset = ((i % 2) * 2 - 1) * 0.2f;
                }
                float yOffset = i * 0.05f;
                Card currentCard = cards[i];
                currentCard.transform.localPosition += new Vector3(Card.cardWidth * xOffset, i * Card.cardDepth * yOffset, 0f);
            }
        }
    }

    public virtual void AlignCards(bool instant)
    {
        isDirty = false;
        if (HasCard)
        {
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
        if (NodeUI != null)
        {
            NodeUI.Refresh(verticalOffsetUI);
        }
    }

    public virtual void CardAutoAction(Card clickedCard) { }
    public virtual void NodeAutoAction() { }
    public abstract List<CardInfo.ActionFlag> GenerateDefaultCardActions();

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

        static float transitionSpeed = 20f;

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
