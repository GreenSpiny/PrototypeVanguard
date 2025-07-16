using UnityEngine;
using System.Collections.Generic;

public class DragManager : MonoBehaviour
{
    // DragManager controls not only dragging, but also the appearance and disappearance of on-board UI elements.
    // Nothing performed by DragManager should impact the gamestate synced across players.

    public static DragManager instance;

    [SerializeField] public Player controllingPlayer;
    [SerializeField] Node_Drag dragNode;

    public Card HoveredCard { get; private set; }       // The card currently being hovered
    public Card DraggedCard { get; private set; }       // The card currently being dragged about
    public Card SelectedCard { get; private set; }      // The card currently chosen for a context action
    public Node HoveredNode { get; private set; }       // The node currently being hovered
    public Node TargetedNode { get; private set; }      // The node currently being hovered to recieve a card or context action
    public Node SelectedNode { get; private set; }      // The node currently being chosen for a context action
    public ContextButton HoveredButton { get; set; }    // The button currently being hovered during a context action

    private float clickTime;          // The time of the most recent mouse press
    private float lastClickTime;      // The time of the previous mouse press, for double click detection
    private Vector3 clickLocation;    // The location of the most recent mouse press, for drag detection

    [SerializeField] public ContextRoot standardContext;
    [SerializeField] public ContextRoot powerContext;

    // Layer Masks
    private static LayerMask cardMask;
    private static LayerMask nodeMask;
    private static LayerMask dragMask;

    public enum DMstate
    {
        open,       // Open game state
        dragging,   // The user is dragging a card
        targeting   // The user has chosen a context option that requires targeting a node
    }
    protected DMstate dmstate;

    protected float DragThreshold { get { return 5f; } }
    protected float DoubleClickThreshold { get { return 0.25f; } }

    public void Init()
    {
        dmstate = DMstate.open;
        instance = this;
        cardMask = LayerMask.GetMask("Card Layer");
        nodeMask = LayerMask.GetMask("Node Layer");
        dragMask = LayerMask.GetMask("Board Drag Layer");
        lastClickTime = float.MinValue;
    }

    public void ClearSelections()
    {
        standardContext.HideAllButtons();
        powerContext.HideAllButtons();
        HoveredButton = null;
        if (SelectedCard != null)
        {
            SelectedCard.UIState = Card.CardUIState.normal;
            SelectedCard = null;
        }
        if (SelectedNode != null)
        {
            SelectedNode.UIState = Node.NodeUIState.normal;
            SelectedNode = null;
        }
    }

    protected void Update()
    {
        if (controllingPlayer == null)
        {
            return;
        }

        // First, raycast all nodes and cards.
        float raycastDistance = 20f;
        Ray cameraRay = controllingPlayer.playerCamera.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(controllingPlayer.playerCamera.transform.position, cameraRay.direction * raycastDistance, Color.yellow);

        RaycastHit cardHit;
        Card hitCard = null;
        if (Physics.Raycast(cameraRay, out cardHit, raycastDistance, cardMask))
        {
            hitCard = cardHit.transform.GetComponent<Card>();
        }
        foreach (Card card in GameManager.instance.allCards.Values)
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
        if (Physics.Raycast(cameraRay, out nodeHit, raycastDistance, nodeMask))
        {
            hitNode = nodeHit.transform.GetComponent<Node>();
        }
        foreach (Node node in GameManager.instance.allNodes.Values)
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
        if (Input.GetMouseButtonDown(0) && HoveredButton == null)
        {
            lastClickTime = clickTime;
            clickTime = Time.time;
        }
        if (Input.GetMouseButtonDown(0) ||  Input.GetMouseButtonDown(1))
        {
            clickLocation = mousePosition;
        }

        if ((Input.GetMouseButtonDown(0) && HoveredButton == null) || Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
        {
            ClearSelections();
        }

        bool dragDistanceMet = Vector3.Distance(clickLocation, mousePosition) > DragThreshold;
        bool doubleClick = (clickTime - lastClickTime < DoubleClickThreshold) && !dragDistanceMet;

        if (doubleClick && dmstate == DMstate.open && HoveredCard != null)
        {
            HoveredCard.node.CardAutoAction(HoveredCard);
            clickTime = 0f;
            lastClickTime = float.MinValue;
        }
        else if (Input.GetMouseButton(0) && dmstate == DMstate.open && HoveredCard != null && dragDistanceMet)
        {
            ChangeDMstate(DMstate.dragging);
        }
        else if (!Input.GetMouseButton(0) && dmstate == DMstate.dragging)
        {
            ChangeDMstate(DMstate.open);
        }
        else if (Input.GetMouseButtonDown(1) && dmstate == DMstate.open)
        {
            if (HoveredCard != null)
            {
                SelectedCard = HoveredCard;
                SelectedCard.UIState = Card.CardUIState.selected;
                standardContext.DisplayButtons(clickLocation, HoveredCard.node.GetDefaultActions()); // temp
            }
            else if (HoveredNode != null)
            {
                SelectedNode = HoveredNode;
                SelectedNode.UIState = Node.NodeUIState.selected;
                standardContext.DisplayButtons(clickLocation, HoveredNode.GetDefaultActions());
            }
        }

        if (dmstate == DMstate.dragging)
        {
            RaycastHit dragHit;
            if (Physics.Raycast(cameraRay, out dragHit, raycastDistance, dragMask))
            {
                dragNode.transform.position = dragHit.point;
            }
        }
    }

    public void ChangeDMstate(DMstate state)
    {
        switch (state)
        {
            case DMstate.open:
                dmstate = DMstate.open;
                Debug.Log("DMstate -> open");
                if (DraggedCard != null && HoveredNode == null)
                {
                    // DragNode is intentionally not synced across clients
                    dragNode.PreviousNode.RecieveCard(DraggedCard, "cancel");
                }
                else if (DraggedCard != null && HoveredNode != null)
                {
                    GameManager.instance.RequestRecieveCardRpc(HoveredNode.nodeID, DraggedCard.cardID, "drag");
                }

                foreach (Node node in GameManager.instance.allNodes.Values)
                {
                    node.UIState = Node.NodeUIState.normal;
                }

                DraggedCard = null;
                ClearSelections();
                break;

            case DMstate.dragging:
                dmstate = DMstate.dragging;
                Debug.Log("DMstate -> dragging");
                DraggedCard = HoveredCard;
                DraggedCard.UIState = Card.CardUIState.normal;

                // DragNode is intentionally not synced across clients
                dragNode.RecieveCard(DraggedCard, null);

                foreach (Node node in GameManager.instance.allNodes.Values)
                {
                    // TODO: need exception for Prison
                    if (node.canDragTo && (DraggedCard.player == node.player || node.Type == Node.NodeType.GC) && DraggedCard.node.PreviousNode != node)
                    {
                        node.UIState = Node.NodeUIState.available;
                    }
                }

                ClearSelections();
                break;

            case DMstate.targeting:
                dmstate = DMstate.targeting;
                Debug.Log("DMstate -> targeting");

                DraggedCard = null;
                break;

            default:
                break;
        }
    }

    public void OnCardHoverEnter(Card card)
    {
        if (dmstate == DMstate.open)
        {
            if (card.UIState == Card.CardUIState.normal)
            {
                card.UIState = Card.CardUIState.hovered;
            }
            HoveredCard = card;
        }
    }

    public void OnCardHoverExit(Card card)
    {
        if (HoveredCard == card)
        {
            HoveredCard = null;
        }
        if (card.UIState == Card.CardUIState.hovered)
        {
            card.UIState = Card.CardUIState.normal;
        }
    }

    public void OnNodeHoverEnter(Node node)
    {
        if (dmstate == DMstate.open && node.canSelectRaw)
        {
            if (node.UIState == Node.NodeUIState.normal)
            {
                node.UIState = Node.NodeUIState.hovered;
            }
            HoveredNode = node;
        }
        else if (dmstate == DMstate.dragging && node.UIState == Node.NodeUIState.available)
        {
            node.UIState = Node.NodeUIState.hovered;
            HoveredNode = node;
        }
    }

    public void OnNodeHoverExit(Node node)
    {
        if (HoveredNode == node)
        {
            HoveredNode = null;
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

}
