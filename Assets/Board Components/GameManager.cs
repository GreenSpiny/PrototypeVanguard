using UnityEngine;
using System.Collections.Generic;
using System;
using Unity.Netcode;

public class GameManager : NetworkBehaviour
{
    public static GameManager instance;

    [SerializeField] public NetworkManager networkManager;
    [SerializeField] public DragManager dragManager;

    [NonSerialized] public Dictionary<int, Node> allNodes = new Dictionary<int, Node>();
    [NonSerialized] public HashSet<Card> allCards = new HashSet<Card>();

    [SerializeField] public Player[] players;
    [SerializeField] private Camera infoCamera;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            DestroyImmediate(gameObject);
            return;
        }

        dragManager.Init();

        int nodeCount = 0;
        foreach (var node in GetComponentsInChildren<Node>(true))
        {
            allNodes.Add(nodeCount, node);
            node.Init(nodeCount);
            nodeCount++;
        }

        int cardCount = 0;
        foreach (var card in GetComponentsInChildren<Card>(true))
        {
            allCards.Add(card);
            cardCount++;
        }

        infoCamera.gameObject.SetActive(true);
    }

}
