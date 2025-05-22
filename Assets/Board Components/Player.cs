using UnityEngine;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    // All entities owned by this player
    public Camera playerCamera;
    public List<Card> cards = new List<Card>();
    public List<Node> nodes = new List<Node>();

    // Specific entities owned by this player
    public Node hand;
    public Node deck;
    public Node drop;
    public Node VC;
    public List<Node> RC;

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
