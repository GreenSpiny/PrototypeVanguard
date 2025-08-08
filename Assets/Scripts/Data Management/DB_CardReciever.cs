using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

public class DB_CardReciever : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public static float receiverCardWidth;
    public enum AreaType { ride, main, stride, toolbox }
    [SerializeField] public AreaType areaType;

    [SerializeField] private int maxCards;
    [SerializeField] private int cardsPerRow;
    [SerializeField] private float cardHeight;
    [SerializeField] private float alignSpeed;
    [SerializeField] private float padding;

    [SerializeField] public DeckBuilder builder;
    [SerializeField] public TextMeshProUGUI label;
    [NonSerialized] public string templateLabelText;
    
    private RectTransform rectTransform;
    private LayoutElement layoutElement;
    [NonSerialized] public List<DB_Card> cards = new List<DB_Card>();
    private List<Vector3> cardPositions = new List<Vector3>();
    private Image receiverImage;
    private Color baseColor;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        layoutElement = GetComponent<LayoutElement>();
        receiverImage = GetComponent<Image>();
        baseColor = receiverImage.color;
        templateLabelText = label.text;
    }

    private void Start()
    {
        foreach (DB_Card card in GetComponentsInChildren<DB_Card>())
        {
            cards.Add(card);
        }
        if (cards.Count > 0)
        {
            AlignCards(true);
        }
    }
    public void ReceiveCard(DB_Card card)
    {
        cards.Add(card);
        card.transform.SetParent(transform, true);
        //card.cardImage.raycastTarget = true;
        card.reciever = this;
        builder.SetDirty();
        receiverImage.color = baseColor;
    }

    public void RemoveCard(DB_Card card, bool destroy)
    {
        cards.Remove(card);
        card.reciever = null;
        if (destroy)
        {
            StartCoroutine(card.DestroySelf(card.transform.position));
        }
        builder.SetDirty();
    }

    public void RemoveAllCards()
    {
        for (int i = cards.Count - 1; i >= 0; i--)
        {
            cards[i].reciever = null;
            Destroy(cards[i].gameObject);
        }
        cards.Clear();
        builder.SetDirty();
    }

    public void AlignCards(bool instant)
    {
        cardPositions.Clear();
        cards.Sort();

        float cardSlotWidth = (rectTransform.rect.width - padding) / cardsPerRow;
        float cardWidth = cardSlotWidth - padding;
        receiverCardWidth = cardWidth;

        float cardHeight = cardWidth / Card.cardWidth;
        float cardSlotHeight = cardHeight + padding;

        int currentRow = 0;
        int currentColumn = 0;
        for (int i = 0; i < cards.Count; i++)
        {
            Vector3 newPosition = new Vector3(currentColumn * cardSlotWidth + padding, currentRow * -cardSlotHeight - padding, 0);
            cards[i].moveTarget = newPosition;
            if (instant)
            {
                cards[i].transform.localPosition = newPosition;
            }
            cards[i].SetWidth(cardWidth);
            currentColumn++;
            if (currentColumn >= cardsPerRow)
            {
                currentColumn = 0;
                currentRow++;
            }
        }
        int totalRows = Mathf.CeilToInt((float)maxCards / cardsPerRow);
        layoutElement.minHeight = cardSlotHeight * totalRows + padding;
    }

    private void Update()
    {
        foreach (DB_Card card in cards)
        {
            card.transform.localPosition = Vector3.Lerp(card.transform.localPosition, card.moveTarget, alignSpeed * Time.deltaTime);
        }
    }

    public bool CanAcceptCard(DB_Card card)
    {
        bool isGUnit = card.cardInfo.unitType == "G Unit";
        bool isToolboxCard = card.cardInfo.unitType == "Token" || card.cardInfo.unitType == "Crest";
        bool isOrder = card.cardInfo.unitType.Contains("Order", StringComparison.InvariantCultureIgnoreCase);

        if (cards.Count >= maxCards)
        {
            return false;
        }
        if (DeckBuilder.instance.currentDeckList.CardCount(card.cardInfo.index) > card.cardInfo.count)
        {
            return false;
        }
        if (areaType == AreaType.stride)
        {
            return isGUnit;
        }
        if (areaType == AreaType.toolbox)
        {
            return isToolboxCard;
        }
        if (areaType == AreaType.ride)
        {
            return !isGUnit && !isToolboxCard && !isOrder;
        }
        if (areaType == AreaType.main)
        {
            return !isGUnit && !isToolboxCard;
        }
        return true;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (DB_CardDragger.instance.draggedCard != null && CanAcceptCard(DB_CardDragger.instance.draggedCard))
        {
            DB_CardDragger.instance.hoveredReceiver = this;
            receiverImage.color = new Color(0.5f, 0.5f, 0.25f);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (DB_CardDragger.instance.hoveredReceiver == this)
        {
            DB_CardDragger.instance.hoveredReceiver = null;
        }
        receiverImage.color = baseColor;
    }
}
