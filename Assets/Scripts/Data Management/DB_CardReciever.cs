using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

public class DB_CardReciever : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
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
    private List<DB_Card> cards = new List<DB_Card>();
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

    public int CardCount {  get { return cards.Count; } }

    public List<DB_Card> GetCards()
    {
        return cards;
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
        card.cardImage.raycastTarget = true;
        builder.needsRefresh = true;
        receiverImage.color = baseColor;
    }

    public void RemoveCard(DB_Card card, bool destroy)
    {
        cards.Remove(card);
        if (destroy)
        {
            StartCoroutine(card.DestroySelf(transform.position));
        }
        builder.needsRefresh = true;
    }

    public void RemoveAllCards()
    {
        for (int i = cards.Count - 1; i >= 0; i--)
        {
            Destroy(cards[i].gameObject);
        }
        cards.Clear();
        builder.needsRefresh = true;
    }

    public void AlignCards(bool instant)
    {
        cardPositions.Clear();
        cards.Sort();

        float cardSlotWidth = (rectTransform.rect.width - padding) / cardsPerRow;
        float cardWidth = cardSlotWidth - padding;

        float cardHeight = cardWidth / Card.cardWidth;
        float cardSlotHeight = cardHeight + padding;

        int currentRow = 0;
        int currentColumn = 0;
        for (int i = 0; i < cards.Count; i++)
        {
            Vector3 newPosition = new Vector3(currentColumn * cardSlotWidth + padding, currentRow * -cardSlotHeight - padding, 0);
            cards[i].moveTarget = newPosition;
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
        return true;
    }

    public void ToggleCardRaycasting(bool toggle)
    {
        foreach (DB_Card card in cards)
        {
            card.cardImage.raycastTarget = toggle;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (DB_CardDragger.instance.draggedCard != null)
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
