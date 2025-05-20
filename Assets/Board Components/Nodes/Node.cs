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
    public bool HasCard { get { return cards.Count > 0; } }

    [SerializeField] protected List<Card> cards;
    [SerializeField] protected Vector3 nudgeDistance;
    [NonSerialized] public Node PreviousNode = null;

    protected virtual void Start()
    {
        AlignCards(true);
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

}
