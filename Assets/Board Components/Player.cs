using UnityEngine;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    public bool isActivePlayer;

    // All entities owned by this player
    public Camera playerCamera;
    public List<Card> cards = new List<Card>();
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
    public Node VC;
    public Node GC;     // Special because shared - for now, assign in Inspector
    public List<Node> RC;

    private void Awake()
    {
        foreach (Node node in GetComponentsInChildren<Node>())
        {
            nodes.Add(node);
            switch(node.Type)
            {
                case Node.NodeType.hand: hand = node; break;
                case Node.NodeType.deck: deck = node; break;
                case Node.NodeType.drop: drop = node; break;
                case Node.NodeType.bind: bind = node; break;
                case Node.NodeType.remove: remove = node; break;
                case Node.NodeType.damage: damage = node; break;
                case Node.NodeType.order: order = node; break;
                case Node.NodeType.gzone: gzone = node; break;
                case Node.NodeType.VC: VC = node; break;
                case Node.NodeType.RC: RC.Add(node); break;
            }
        }
        foreach (Card card in GetComponentsInChildren<Card>())
        {
            cards.Add(card);
        }
    }

}
