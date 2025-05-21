using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
public class SharedGamestate : MonoBehaviour
{
    public static List<Card> allCards = new List<Card>();
    public static List<Node> allNodes = new List<Node>();
}

public static class SharedGameStateFunctions
{ 

}