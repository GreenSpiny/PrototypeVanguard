using NUnit.Framework;
using System;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    // All entities owned by this player
    List<Card> cards = new List<Card>();
    List<Node> nodes = new List<Node>();

    // Specific entities owned by this player
    Node hand;
    Node deck;
    Node drop;
    Node VC;
    List<Node> RC;

    private void Awake()
    {
        foreach (Node node in GetComponentsInChildren<Node>())
        {
            nodes.Add(node);
            node.player = this;
            switch(node.GetNodeType())
            {
                case Node.NodeType.hand: hand = node; break;
                case Node.NodeType.deck: deck = node; break;
                case Node.NodeType.drop: drop = node; break;
                case Node.NodeType.VC: VC = node; break;
                case Node.NodeType.RC: RC.Add(node); break;
            }
        }
        foreach (Card card in GetComponentsInChildren<Card>())
        {
            cards.Add(card);
            card.player = this;
        }
    }

}
