using UnityEngine;
using System.Collections.Generic;
using System;

public class DB_CardReciever : MonoBehaviour
{
    [SerializeField] private int maxCards;
    [SerializeField] private int cardsPerRow;
    [SerializeField] private float cardHeight;
    [SerializeField] private float alignSpeed;
    
    private RectTransform rectTransform;
    private List<DB_Card> cards = new List<DB_Card>();
    private List<Vector3> cardPositions = new List<Vector3>();

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        foreach (DB_Card card in GetComponentsInChildren<DB_Card>())
        {
            cards.Add(card);
        }
        AlignCards();
    }
    public void ReceiveCard(DB_Card card)
    {
        cards.Add(card);
    }

    public void RemoveCard(DB_Card card)
    {
        cards.Remove(card);
        //Destroy(card);
    }

    public void RemoveAllCards()
    {
        for (int i = cards.Count - 1; i >= 0; i--)
        {
            //Destroy(cards[i]);
        }
        cards.Clear();
    }

    private void AlignCards()
    {
        Canvas.ForceUpdateCanvases(); // temp
        cardPositions.Clear();
        int currentRow = 0;
        int currentColumn = 0;

        float cardWidth = rectTransform.rect.width / cardsPerRow;
        float cardHeight = cardWidth * (1f / Card.cardWidth);

        for (int i = 0; i < cards.Count; i++)
        {
            Vector3 newPosition = new Vector3(currentColumn * cardWidth, currentRow * -cardHeight, 0);
            cards[i].moveTarget = newPosition;
            cards[i].SetWidth(cardWidth);
            currentColumn++;
            if (currentColumn >= cardsPerRow)
            {
                currentColumn = 0;
                currentRow++;
            }
        }
    }
    private void Update()
    {
        foreach (DB_Card card in cards)
        {
            card.transform.localPosition = Vector3.Lerp(card.transform.localPosition, card.moveTarget, alignSpeed * Time.deltaTime);
        }
    }


}
