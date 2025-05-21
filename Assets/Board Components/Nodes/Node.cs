using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Globalization;
using System;
using static Card;
using NUnit.Framework.Constraints;

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
        if (nodeColldier == null)
        {
            nodeColldier = GetComponent<Collider>();
        }
    }

    private void OnDestroy()
    {
        SharedGamestate.allNodes.Remove(this);
    }

    protected virtual void Start()
    {
        AlignCards(true);
    }

    private void Update()
    {
        if (animInfo.Initialized)
        {
            animInfo.Animate();
        }
    }

    public virtual void RecieveCard(Card card, IEnumerable<string> parameters)
    {
        if (card.Node != this)
        {
            PreviousNode = card.Node;
            PreviousNode.RemoveCard(card);
            card.Node = this;
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

    public NodeUIState UIState
    {
        get
        {
            return state;
        }
        set
        {
            state = value;
            if (animInfo.Initialized)
            {
                if (state == NodeUIState.normal)
                {
                    animInfo.shouldFlash = false;
                    animInfo.arrowsScale = 0f;
                    animInfo.ArrowsColor = Color.red;
                }
                else if (state == NodeUIState.available)
                {
                    animInfo.shouldFlash = true;
                    animInfo.arrowsScale = 1f;
                    animInfo.ArrowsColor = Color.red;
                }
                else if (state == NodeUIState.hovered)
                {
                    animInfo.shouldFlash = true;
                    animInfo.arrowsScale = 1f;
                    animInfo.ArrowsColor = Color.yellow;
                }
            }
        }
    }

    [System.Serializable]
    public class NodeAnimationInfo
    {
        [SerializeField] public GameObject nodeFlashObject;
        [SerializeField] public GameObject nodeArrowsObject;

        [NonSerialized] public bool shouldFlash;
        [NonSerialized] public float arrowsScale;
        [NonSerialized] protected Color arrowsColor;

        public Color ArrowsColor { get { return arrowsColor; } set { arrowsColor = value; nodeArrowsObject.GetComponent<SpriteRenderer>().color = arrowsColor; } }

        static float transitionSpeed = 8f;
        static float spinSpeed = 64f;

        public bool Initialized { get { return nodeFlashObject != null && nodeArrowsObject != null; } }
        public void Animate()
        {
            float targetScale = Mathf.Lerp(nodeArrowsObject.transform.localScale.x, arrowsScale, Time.deltaTime * transitionSpeed);
            nodeArrowsObject.transform.localScale = new Vector3(targetScale, targetScale, 1f);
            nodeArrowsObject.transform.localRotation = Quaternion.Euler(0f, 0f, (Time.time * spinSpeed) % 360);
        }
    }

}
