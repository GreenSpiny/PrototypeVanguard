
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

// Nodes are locations on the board that cards are anchored to. They recieve and arrange cards by their own devices.
// Examples include the hand, deck, all zones, and all unit circles.
public abstract class Node : MonoBehaviour
{
    public enum NodeType { drag, hand, deck, drop, trigger, damage, order, gauge, VC, RC, GC }
    public abstract NodeType GetNodeType();
    public abstract bool CanDragTo();       // If true, this node can have cards dragged to it.
    public abstract bool CanSelectRaw();    // If true, this node can be context clicked in an open gamestate
    public bool HasCard { get { return cards.Count > 0; } }     // True if the node has at least one card

    [SerializeField] protected List<Card> cards;        // The cards attached to this node
    [SerializeField] public Transform cardAnchor;    // The position and rotation cards begin to accrue on this node
    [SerializeField] protected Vector3 nudgeDistance;   // If and how far cards on this node "nudge" when hovered, as feedback
    [NonSerialized] public Node PreviousNode = null;    // The previous Node of the most recently attached card

    // The player who owns this node
    [NonSerialized] public Player player;

    // Animation properties
    [SerializeField] protected NodeAnimationInfo animInfo;

    public enum NodeUIState { normal, available, hovered, selected };
    protected NodeUIState state;

    private void Awake()
    {
        SharedGamestate.allNodes.Add(this);
        if (cardAnchor == null)
        {
            cardAnchor = transform;
        }
        animInfo.Initialize();
        foreach (var child in GetComponentsInChildren<Card>())
        {
            cards.Add(child);
            child.transform.SetParent(transform.parent, true);
            child.transform.localScale = Vector3.one;
            child.transform.localRotation = Quaternion.identity;
        }
        AlignCards(true);
    }

    private void Update()
    {
        animInfo.Animate();
    }

    public virtual void RecieveCard(Card card, IEnumerable<string> parameters)
    {
        if (card.node != this)
        {
            PreviousNode = card.node;
            PreviousNode.RemoveCard(card);
            card.node = this;
        }
    }

    public virtual void SwapAllCards(Node otherNode, IEnumerable<string> parameters)
    {
        bool drag = parameters.Contains("drag");

        List<Card> selfCards = new List<Card>();
        foreach (Card card in cards)
        {
            selfCards.Add(card);
        }
        cards.Clear();

        List<Card> otherCards = new List<Card>();
        foreach (Card c in otherNode.cards)
        {
            otherCards.Add(c);
        }
        otherNode.cards.Clear();

        if (drag)
        {
            foreach (Card c in otherNode.PreviousNode.cards)
            {
                otherCards.Add(c);
            }
            otherNode.PreviousNode.cards.Clear();
        } 
        
        foreach (Card c in selfCards)
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

        foreach (Card c in otherCards)
        {
            cards.Add(c);
        }

        AlignCards(false);
        otherNode.AlignCards(false);
        otherNode.PreviousNode.AlignCards(false);
    }

    // RemoveCard is not to be overridden. External classes should call either RecieveCard or SwapAllCards.
    private void RemoveCard(Card card)
    {
        if (cards.Contains(card))
        {
            cards.Remove(card);
            AlignCards(false);
        }
    }
    public abstract void AlignCards(bool instant);

    // "Auto Action" is the default action when a card on this node is double clicked.
    public virtual void AutoAction(Card clickedCard) { }

    public Vector3 NudgeDistance { get { return nudgeDistance; } }

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
                animInfo.shouldFlash = false;
                animInfo.arrowsScale = 0f;
                animInfo.arrowsColor = Color.red;
                animInfo.flashColor = Color.white;
                animInfo.arrowsColor.a = 0f;
                animInfo.flashColor.a = 0f;
                animInfo.instantColor = false;
            }
            else if (state == NodeUIState.available)
            {
                animInfo.shouldFlash = true;
                animInfo.arrowsScale = 1f;
                animInfo.arrowsColor = Color.red;
                animInfo.flashColor = Color.red;
                animInfo.arrowsColor.a = 0.5f;
                animInfo.flashColor.a = 0.5f;
                animInfo.instantColor = false;
            }
            else if (state == NodeUIState.hovered)
            {
                animInfo.shouldFlash = false;
                animInfo.arrowsScale = 1f;
                animInfo.arrowsColor = Color.yellow;
                animInfo.flashColor = Color.yellow;
                animInfo.arrowsColor.a = 0.5f;
                animInfo.flashColor.a = 0.5f;
                animInfo.instantColor = true;
            }
        }
    }

    [System.Serializable]
    public class NodeAnimationInfo
    {
        [SerializeField] public SpriteRenderer flashRenderer;
        [SerializeField] public SpriteRenderer arrowsRenderer;

        [NonSerialized] public bool shouldFlash;
        [NonSerialized] public float arrowsScale;
        [NonSerialized] public Color flashColor = new Color(1f, 1f, 1f, 0f);
        [NonSerialized] public Color arrowsColor = new Color(1f, 1f, 1f, 0f);
        [NonSerialized] public bool instantColor = false;

        static float transitionSpeed = 10f;
        static float spinSpeed = 64f;

        public bool CanAnimate { get { return flashRenderer != null && arrowsRenderer != null; } }

        public void Initialize()
        {
            if (CanAnimate)
            {
                arrowsRenderer.transform.localScale = new Vector3(0f, 0f, 1f);
                flashRenderer.color = flashColor;
            }
        }

        public void Animate()
        {
            if (CanAnimate)
            {
                float targetScale = Mathf.Lerp(arrowsRenderer.transform.localScale.x, arrowsScale, Time.deltaTime * transitionSpeed);
                arrowsRenderer.transform.localScale = new Vector3(targetScale, targetScale, 1f);
                arrowsRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, (Time.time * spinSpeed) % 360);

                if (targetScale > 0.001f)
                {
                    if (instantColor)
                    {
                        flashRenderer.color = flashColor;
                    }
                    else
                    {
                        flashRenderer.color = Color.Lerp(flashRenderer.color, flashColor, Time.deltaTime * transitionSpeed);
                    }
                    arrowsRenderer.color = new Color(arrowsColor.r, arrowsColor.g, arrowsColor.b, Mathf.Clamp(arrowsRenderer.transform.localScale.x * 0.5f, 0f, 0.5f));
                }
            }
        }
    }

}
