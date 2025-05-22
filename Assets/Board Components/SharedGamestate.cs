using UnityEngine;
using System.Collections.Generic;
public class SharedGamestate : MonoBehaviour
{
    public static List<Card> allCards = new List<Card>();
    public static List<Node> allNodes = new List<Node>();

    /*
    // Transfer Card is the essential function to maintain and progress gamestate.
    // 90% of gamestate-changing actions are the transfer of cards from one node to another.
    public static void TransferCard(Card card, Node destination, IEnumerable<string> parameters)
    {
        Node.NodeType sourceType = card.node.GetNodeType();
        switch (sourceType)
        {
            case Node.NodeType.hand:
                TransferCardFromHand(card, destination, parameters);
                break;



            default:
                break;
        }

    }

    protected static void TransferCardFromHand(Card card, Node destination, IEnumerable<string> parameters)
    {

    }
    */

}

