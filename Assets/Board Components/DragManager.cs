using NUnit.Framework;
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
        context,    // The user has opened a context menu and must select an option
        targeting   // The user has chosen a context option that requires targeting a node
    }
    protected DMstate dmstate;

    protected float AnimationSpeed { get { return 10f * Time.deltaTime; } }
    protected float DragThreshold { get { return 5f; } }
    protected float DoubleClickThreshold { get { return 0.25f; } }

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
    }

    protected void Update()
    {
        Vector3 mousePosition = Input.mousePosition;
        if (Input.GetMouseButtonDown(0))
        {
            lastClickTime = clickTime;
            clickTime = Time.time;
            clickLocation = mousePosition;
        }
        bool doubleClick = clickTime - lastClickTime < DoubleClickThreshold;

        if (Input.GetMouseButton(0) && dmstate == DMstate.open && hoveredCard != null && Vector3.Distance(clickLocation, mousePosition) > DragThreshold)
        {
            ChangeDMstate(DMstate.dragging);
        }
        if (!Input.GetMouseButton(0) && dmstate == DMstate.dragging)
        {
            ChangeDMstate(DMstate.open);
        }
        if (dmstate == DMstate.dragging)
        {
            RaycastHit hit;
            Vector3 offset = (activeCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f)) - activeCamera.transform.position).normalized;
            // Debug.DrawRay(activeCamera.transform.position, offset * 10f, Color.yellow);
            if (Physics.Raycast(activeCamera.transform.position, offset, out hit, 10f, dragMask.value))
            {
                dragNode.transform.position = hit.point;
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
                if (draggedCard != null && targetedNode == null)
                {
                    dragNode.PreviousNode.RecieveCard(draggedCard, null);
                }

                draggedCard = null;
                selectedCard = null;
                targetedNode = null;
                return;

            case DMstate.dragging:
                dmstate = DMstate.dragging;
                Debug.Log("DMstate -> dragging");
                draggedCard = hoveredCard;
                dragNode.RecieveCard(draggedCard, null);

                selectedCard = null;
                return;

            case DMstate.context:
                dmstate = DMstate.context;
                Debug.Log("DMstate -> context");

                draggedCard = null;
                targetedNode = null;
                return;

            case DMstate.targeting:
                dmstate = DMstate.targeting;
                Debug.Log("DMstate -> targeting");

                draggedCard = null;
                return;

            default:
                return;
        }
    }

    public void OnCardHoverEnter(Card card)
    {
        hoveredCard = card;
        if (dmstate == DMstate.open)
        {
            card.UIState = Card.CardUIState.hovered;
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
        hoveredNode = node;
    }

    public void OnNodeHoverExit(Node node)
    {
        if (hoveredNode == node)
        {
            hoveredNode = null;
        }
    }

    public void OnNodeContextClick(Node node)
    {

    }
}
