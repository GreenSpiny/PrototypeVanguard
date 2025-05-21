using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Globalization;
using System;

// Nodes are locations on the board that cards are anchored to. They recieve and arrange cards by their own devices.
// Examples include the hand, deck, all zones, and all unit circles.
public abstract class Node : MonoBehaviour
{
    public enum NodeType { drag, hand, deck, drop, trigger, damage, order, gauge, VC, RC, GC }
    public abstract NodeType GetNodeType();
    public abstract bool DefaultSelectable();   // If true, this node can be hovered and selected outside of targeting mode.
    public bool HasCard { get { return cards.Count > 0; } }     // True if the node has at least one card

    [SerializeField] protected List<Card> cards;        // The cards attached to this node
    [SerializeField] protected Transform cardAnchor;    // The position and rotation cards begin to accrue on this node
    [SerializeField] protected Collider nodeColldier;   // The physics collider associated with this node.
    [SerializeField] protected Vector3 nudgeDistance;   // If and how far cards on this node "nudge" when hovered, as feedback
    [NonSerialized] public Node PreviousNode = null;    // The previous Node of the most recently attached card

    protected virtual void Awake()
    {
        SharedGamestate.allNodes.Add(this);
        if (cardAnchor == null)
        {
            cardAnchor = transform;
        }
        if (nodeColldier == null)
        {
            nodeColldier = GetComponent<Collider>();
        }
    }

    protected virtual void Start()
    {
        AlignCards(true);
    }

    public void ToggleColliders(bool toggle)
    {
        if (nodeColldier != null)
        {
            nodeColldier.enabled = toggle;
        }
    }
    public virtual void RecieveCard(Card card, IEnumerable<string> parameters)
    {
        if (card.Node != this)
        {
            PreviousNode = card.Node;
            PreviousNode.RemoveCard(card);
            card.Node = this;
            card.transform.SetParent(transform, true);
        }
    }
    protected virtual void RemoveCard(Card card)
    {
        if (cards.Contains(card))
        {
            cards.Remove(card);
            AlignCards(false);
        }
    }
    public abstract void AlignCards(bool instant);

    public Vector3 NudgeDistance { get { return nudgeDistance; } }

    public void OnMouseEnter()
    {
        DragManager.instance.OnNodeHoverEnter(this);
    }

    public void OnMouseExit()
    {
        DragManager.instance.OnNodeHoverExit(this);
    }

}
