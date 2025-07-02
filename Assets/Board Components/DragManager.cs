using UnityEngine;
using System.Collections.Generic;

public class DragManager : MonoBehaviour
{
    // DragManager controls not only dragging, but also the appearance and disappearance of on-board UI elements.
    // Nothing performed by DragManager should impact the gamestate synced across players.

    public static DragManager instance;

    [SerializeField] Player activePlayer;
    [SerializeField] Camera activeCamera;
    [SerializeField] Node_Drag dragNode;

    protected Card hoveredCard;     // The card currently being hovered
    protected Card draggedCard;     // The card currently being dragged about
    protected Card selectedCard;    // The card currently chosen for a context action
    protected Node hoveredNode;     // The node currently being hovered
    protected Node targetedNode;    // The node currently being hovered to recieve a card or context action
    protected Node selectedNode;    // The node currently being chosen for a context action

    protected float clickTime;          // The time of the most recent mouse press
    protected float lastClickTime;      // The time of the previous mouse press, for double click detection
    protected Vector3 clickLocation;    // The location of the most recent mouse press, for drag detection

    // Layer Masks
    public static LayerMask cardMask;
    public static LayerMask nodeMask;
    public static LayerMask dragMask;

    public enum DMstate
    {
        open,       // Open game state
        dragging,   // The user is dragging a card
        targeting,  // The user has chosen a context option that requires targeting a node
        menu        // The user has chosen a context option that requires interacting with a menu
    }
    protected DMstate dmstate;

    protected float AnimationSpeed { get { return 10f * Time.deltaTime; } }
    protected float DragThreshold { get { return 5f; } }
    protected float DoubleClickThreshold { get { return 0.25f; } }

    // TEMP VALUE STORAGE
    Card[] allCards;
    Node[] allNodes;

    protected void Awake()
    {
        if (instance != null)
        {
            gameObject.SetActive(false);
        }
        instance = this;
        cardMask = LayerMask.GetMask("Card Layer");
        nodeMask = LayerMask.GetMask("Node Layer");
        dragMask = LayerMask.GetMask("Board Drag Layer");
        lastClickTime = float.MinValue;

        allCards = FindObjectsByType<Card>(FindObjectsSortMode.None);
        allNodes = FindObjectsByType<Node>(FindObjectsSortMode.None);
    }

    protected void Update()
    {
        // First, raycast all nodes and cards.
        float raycastDistance = 10f;
        Vector3 raycastOffset = (activeCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, raycastDistance)) - activeCamera.transform.position).normalized;
        Debug.DrawRay(activeCamera.transform.position, raycastOffset * 10f, Color.yellow);

        RaycastHit cardHit;
        Card hitCard = null;
        if (Physics.Raycast(activeCamera.transform.position, raycastOffset, out cardHit, 10f, cardMask))
        {
            hitCard = cardHit.transform.GetComponent<Card>();
        }
        foreach (Card card in allCards)
        {
            if (card == hitCard)
            {
                OnCardHoverEnter(card);
            }
            else
            {
                OnCardHoverExit(card);
            }
        }

        RaycastHit nodeHit;
        Node hitNode = null;
        if (Physics.Raycast(activeCamera.transform.position, raycastOffset, out nodeHit, raycastDistance, nodeMask))
        {
            hitNode = nodeHit.transform.GetComponent<Node>();
        }
        foreach (Node node in allNodes)
        {
            if (node == hitNode)
            {
                OnNodeHoverEnter(node);
            }
            else
            {
                OnNodeHoverExit(node);
            }
        }

        // Second, handle input and state changes.
        Vector3 mousePosition = Input.mousePosition;
        if (Input.GetMouseButtonDown(0))
        {
            lastClickTime = clickTime;
            clickTime = Time.time;
            clickLocation = mousePosition;
        }
        bool dragDistanceMet = Vector3.Distance(clickLocation, mousePosition) > DragThreshold;
        bool doubleClick = (clickTime - lastClickTime < DoubleClickThreshold) && !dragDistanceMet;

        if (doubleClick && dmstate == DMstate.open && hoveredCard != null)
        {
            CardAutoAction(hoveredCard);
            lastClickTime = float.MinValue;
        }
        else if (Input.GetMouseButton(0) && dmstate == DMstate.open && hoveredCard != null && dragDistanceMet)
        {
            ChangeDMstate(DMstate.dragging);
        }
        else if (!Input.GetMouseButton(0) && dmstate == DMstate.dragging)
        {
            ChangeDMstate(DMstate.open);
        }

        if (dmstate == DMstate.dragging)
        {
            RaycastHit dragHit;
            if (Physics.Raycast(activeCamera.transform.position, raycastOffset, out dragHit, raycastDistance, dragMask))
            {
                dragNode.transform.position = dragHit.point;
            }
        }
    }

    protected void ChangeDMstate(DMstate state)
    {
        switch (state)
        {
            case DMstate.open:
                dmstate = DMstate.open;
                Debug.Log("DMstate -> open");
                if (draggedCard != null && hoveredNode == null)
                {
                    dragNode.PreviousNode.RecieveCard(draggedCard, new string[0]);
                }
                else if (draggedCard != null && hoveredNode != null)
                {
                    if (hoveredNode.type == Node.NodeType.RC)
                    {
                        if (dragNode.PreviousNode.type == Node.NodeType.RC)
                        {
                            hoveredNode.SwapAllCards(dragNode, new string[1] { "drag" });
                        }
                        else
                        {
                            hoveredNode.RetireCards();
                            hoveredNode.RecieveCard(draggedCard, new string[0]);
                        }
                    }
                    else
                    {
                        hoveredNode.RecieveCard(draggedCard, new string[0]);
                    }
                }

                foreach (Node node in allNodes)
                {
                    node.UIState = Node.NodeUIState.normal;
                }

                draggedCard = null;
                selectedCard = null;
                selectedNode = null;
                break;

            case DMstate.dragging:
                dmstate = DMstate.dragging;
                Debug.Log("DMstate -> dragging");
                draggedCard = hoveredCard;
                draggedCard.UIState = Card.CardUIState.normal;
                dragNode.RecieveCard(draggedCard, null);

                foreach (Node node in allNodes)
                {
                    // TODO: need exception for Prison
                    if (node.canDragTo && draggedCard.player == node.player && draggedCard.node.PreviousNode != node)
                    {
                        node.UIState = Node.NodeUIState.available;
                    }
                }

                selectedCard = null;
                selectedNode = null;
                break;

            case DMstate.targeting:
                dmstate = DMstate.targeting;
                Debug.Log("DMstate -> targeting");

                draggedCard = null;
                break;

            case DMstate.menu:
                dmstate = DMstate.menu;
                Debug.Log("DMstate -> menu");

                draggedCard = null;
                break;

            default:
                break;
        }
    }

    public void OnCardHoverEnter(Card card)
    {
        if (dmstate == DMstate.open)
        {
            card.UIState = Card.CardUIState.hovered;
            hoveredCard = card;
        }
    }

    public void OnCardHoverExit(Card card)
    {
        if (hoveredCard == card)
        {
            hoveredCard = null;
        }
        if (card.UIState == Card.CardUIState.hovered)
        {
            card.UIState = Card.CardUIState.normal;
        }
    }

    public void OnCardContextClick(Card card)
    {

    }

    public void OnNodeHoverEnter(Node node)
    {
        if (dmstate == DMstate.open && node.canSelectRaw)
        {
            node.UIState = Node.NodeUIState.hovered;
            hoveredNode = node;
        }
        else if (dmstate == DMstate.dragging && node.UIState == Node.NodeUIState.available)
        {
            node.UIState = Node.NodeUIState.hovered;
            hoveredNode = node;
        }
    }

    public void OnNodeHoverExit(Node node)
    {
        if (hoveredNode == node)
        {
            hoveredNode = null;
        }
        if (node.UIState == Node.NodeUIState.hovered)
        {
            if (dmstate == DMstate.dragging)
            {
                node.UIState = Node.NodeUIState.available;
            }
            else
            {
                node.UIState = Node.NodeUIState.normal;
            }
        }
    }

    public void OnNodeContextClick(Node node)
    {

    }

    // === double click "Auto Actions" === //
    protected void CardAutoAction(Card clickedCard)
    {
        switch (clickedCard.node.type)
        {
            case Node.NodeType.RC:
                clickedCard.rest = !clickedCard.rest;
                clickedCard.node.AlignCards(false);
                break;
            case Node.NodeType.VC:
                clickedCard.rest = !clickedCard.rest;
                clickedCard.node.AlignCards(false);
                break;
            case Node.NodeType.GC:
                clickedCard.node.RetireCards();
                break;
            case Node.NodeType.order:
                clickedCard.rest = !clickedCard.rest;
                clickedCard.node.AlignCards(false);
                break;
            case Node.NodeType.deck:
                clickedCard.node.player.hand.RecieveCard(clickedCard, new string[0]);
                break;
            case Node.NodeType.damage:
                clickedCard.flipRotation = !clickedCard.flipRotation;
                clickedCard.node.AlignCards(true);
                break;
        }
    }

    protected void NodeAutoAction(Node clickedNode)
    {
        switch(clickedNode.type)
        {

        }
    }
}
