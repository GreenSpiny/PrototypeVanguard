using UnityEngine;
using static DragManager;

public class DB_CardDragger : MonoBehaviour
{
    public static DB_CardDragger instance;
    public DB_CardReciever[] receivers;

    public DB_Card hoveredCard;
    public DB_CardReciever hoveredReceiver;
    public DB_Card draggedCard;

    [SerializeField] private Transform searchContainer;

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
                cloneCard.SetWidth(cloneCard.rectTransform.rect.height * Card.cardWidth);
                cloneCard.transform.localPosition = new Vector3(cloneCard.rectTransform.rect.width / -2f, cloneCard.rectTransform.rect.height / 2f, 0f);
                cloneCard.cardImage.raycastTarget = false;
                draggedCard = cloneCard;
                ToggleCardRaycasting(false);
            }
        }
        else if (!Input.GetMouseButton(0))
        {
            if (draggedCard != null)
            {
                if (hoveredReceiver != null && hoveredReceiver.CanAcceptCard(draggedCard))
                {
                    hoveredReceiver.ReceiveCard(draggedCard);
                }
                else
                {
                    StartCoroutine(draggedCard.DestroySelf(transform.position));
                }
                ToggleCardRaycasting(true);
                draggedCard = null;
            }
        }

        transform.position = mousePosition;
    }

    private void ToggleCardRaycasting(bool toggle)
    {
        foreach (DB_CardReciever reciever in receivers)
        {
            reciever.ToggleCardRaycasting(toggle);
        }
    }


}
