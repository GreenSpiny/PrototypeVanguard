using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static DragManager;

public class DB_CardDragger : MonoBehaviour
{
    public static DB_CardDragger instance;
    public DB_CardReciever[] receivers;

    public DB_Card hoveredCard;
    public DB_CardReciever hoveredReceiver;
    public DB_Card draggedCard;

    [SerializeField] private Transform searchContainer;
    private ScrollRect scrollRect;

    private float clickTime;          // The time of the most recent mouse press
    private float lastClickTime;      // The time of the previous mouse press, for double click detection
    private Vector3 clickLocation;    // The location of the most recent mouse press, for drag detection
    protected float DragThreshold { get { return 5f; } }
    protected float DoubleClickThreshold { get { return 0.25f; } }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        lastClickTime = float.MinValue;
        scrollRect = searchContainer.GetComponentInParent<ScrollRect>();
    }

    private void Update()
    {
        Vector3 mousePosition = Input.mousePosition;
        if (Input.GetMouseButtonDown(0))
        {
            lastClickTime = clickTime;
            clickTime = Time.time;
        }
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            clickLocation = mousePosition;
        }

        bool dragDistanceMet = Vector3.Distance(clickLocation, mousePosition) > DragThreshold;
        bool doubleClick = (clickTime - lastClickTime < DoubleClickThreshold) && !dragDistanceMet;

        // Begin dragging
        if (Input.GetMouseButton(0) && draggedCard == null && hoveredCard != null && dragDistanceMet)
        {
            // Drag out of search area
            if (hoveredCard.transform.parent == searchContainer)
            {
                DB_Card cloneCard = Instantiate(hoveredCard, transform);
                cloneCard.Load(hoveredCard.cardInfo.index);
                cloneCard.SetWidth(DB_CardReciever.receiverCardWidth);
                draggedCard = cloneCard;
            }
            else if (hoveredCard.reciever != null)
            {
                DB_CardReciever originalReceiver = hoveredCard.reciever;
                hoveredCard.reciever.RemoveCard(hoveredCard, false);
                draggedCard = hoveredCard;
                draggedCard.transform.SetParent(transform, true);
                draggedCard.draggedFromReceiver = originalReceiver;
            }
            foreach (DB_CardReciever receiver in receivers)
            {
                receiver.accepting = receiver.CanAcceptCard(draggedCard);
                if (receiver.accepting)
                {
                    receiver.targetColor = receiver.availableColor;
                }
            }
        }
        // End dragging
        else if (draggedCard != null && !Input.GetMouseButton(0))
        {
            if (hoveredReceiver != null && hoveredReceiver.CanAcceptCard(draggedCard))
            {
                hoveredReceiver.ReceiveCard(draggedCard);
            }
            else
            {
                StartCoroutine(draggedCard.DestroySelf(transform.position));
            }
            draggedCard = null;
        }

        if (draggedCard == null)
        {
            // Double click or middle mouse click
            if ((doubleClick || Input.GetMouseButtonDown(2)) && hoveredCard != null)
            {
                clickTime = 0f;
                lastClickTime = float.MinValue;
                if (hoveredCard.transform.parent == searchContainer)
                {
                    foreach (DB_CardReciever receiver in receivers)
                    {
                        if (receiver.areaType != DB_CardReciever.AreaType.ride && receiver.CanAcceptCard(hoveredCard))
                        {
                            DB_Card cloneCard = Instantiate(hoveredCard, receiver.transform);
                            cloneCard.Load(hoveredCard.cardInfo.index);
                            cloneCard.SetWidth();
                            receiver.ReceiveCard(cloneCard);
                            cloneCard.transform.position = mousePosition;
                            ApplyCardOffset(cloneCard);
                            break;
                        }
                    }
                }
                else
                {
                    DB_CardReciever parentReceiver = hoveredCard.transform.parent.GetComponent<DB_CardReciever>();
                    if (parentReceiver != null && parentReceiver.CanAcceptCard(hoveredCard))
                    {
                        DB_Card cloneCard = Instantiate(hoveredCard, parentReceiver.transform);
                        cloneCard.Load(hoveredCard.cardInfo.index);
                        cloneCard.SetWidth();
                        parentReceiver.ReceiveCard(cloneCard);
                        DB_Card lastCopy = parentReceiver.GetLastCopy(cloneCard.cardInfo);
                        if (lastCopy != null)
                        {
                            cloneCard.transform.position = lastCopy.transform.position;
                        }
                    }
                }
            }
            // Right click
            if (Input.GetMouseButtonDown(1) && hoveredCard != null)
            {
                if (hoveredCard.reciever != null)
                {
                    hoveredCard.reciever.RemoveCard(hoveredCard, true);
                    hoveredCard = null;
                }
            }
        }

        
        scrollRect.enabled = draggedCard == null;
        transform.position = Input.mousePosition;
        if (draggedCard != null)
        {
            draggedCard.transform.localPosition = Vector3.zero;
            ApplyCardOffset(draggedCard);
        }
    }

    private void ApplyCardOffset(DB_Card card)
    {
        float dragOffset = DB_CardReciever.receiverCardWidth * 0.5f;
        card.transform.localPosition += new Vector3(-dragOffset, dragOffset / Card.cardWidth, 0f);
    }

}
