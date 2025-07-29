using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

public class DB_CardReciever : MonoBehaviour
{
    public enum AreaType { ride, main, stride, toolbox }
    [SerializeField] public AreaType areaType;

    [SerializeField] private int maxCards;
    [SerializeField] private int cardsPerRow;
    [SerializeField] private float cardHeight;
    [SerializeField] private float alignSpeed;
    [SerializeField] private float padding;
    
    private RectTransform rectTransform;
    private LayoutElement layoutElement;
    private List<DB_Card> cards = new List<DB_Card>();
    private List<Vector3> cardPositions = new List<Vector3>();

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        layoutElement = GetComponent<LayoutElement>();
    }

    private void Start()
    {
        foreach (DB_Card card in GetComponentsInChildren<DB_Card>())
        {
            cards.Add(card);
        }
        AlignCards(true);
    }
    public void ReceiveCard(DB_Card card)
    {
        cards.Add(card);
    }

    public void RemoveCard(DB_Card card)
    {
        cards.Remove(card);
        Destroy(card);
    }

    public void RemoveAllCards()
    {
        for (int i = cards.Count - 1; i >= 0; i--)
        {
            Destroy(cards[i]);
        }
        cards.Clear();
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


}
