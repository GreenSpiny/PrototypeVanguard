using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DB_Card : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image cardImage;
    [NonSerialized] private RectTransform rectTransform;
    [NonSerialized] public Vector3 moveTarget = Vector3.zero;
    [NonSerialized] public CardInfo cardInfo;
    [NonSerialized] public DB_CardReciever reciever;
    [NonSerialized] public DB_CardReciever draggedFromReceiver;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        cardImage.color = Color.white;
    }

    public void SetWidth()
    {
        float width = rectTransform.rect.height * Card.cardWidth;
        SetWidth(width);
    }

    public void SetWidth(float width)
    {
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, width / Card.cardWidth);
        if (cardInfo != null && cardInfo.rotate)
        {
            cardImage.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            cardImage.transform.localScale = new Vector3(1f / Card.cardWidth, Card.cardWidth, 1f);
        }
        else
        {
            cardImage.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            cardImage.transform.localScale = Vector3.one;
        }
    }

    public void Load(int cardIndex)
    {
        cardInfo = CardLoader.GetCardInfo(cardIndex);
        name = cardInfo.index.ToString();
        Material targetMaterial = CardLoader.GetCardImage(cardInfo.index);
        Texture2D targetTexture = targetMaterial.mainTexture as Texture2D;

        cardImage.sprite = Sprite.Create(targetTexture, new Rect(0, 0, targetTexture.width, targetTexture.height), Vector2.zero);
    }
    public int CompareTo(DB_Card other)
    {
        if (cardInfo == null || other.cardInfo == null)
        {
            return 0;
        }
        int comparator = cardInfo.CompareTo(other.cardInfo);
        if (comparator != 0)
        {
            Debug.Log(comparator);
            return comparator;
        }
        return GetInstanceID() - other.GetInstanceID();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (DB_CardDragger.instance.draggedCard != this)
        {
            DB_CardDragger.instance.hoveredCard = this;
            cardImage.color = new Color(1f, 1f, 0.5f);
        }
        DeckBuilder.instance.cardDetailUI.InspectCard(cardInfo);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (DB_CardDragger.instance.hoveredCard == this)
        {
            DB_CardDragger.instance.hoveredCard = null;
        }
        cardImage.color = Color.white;
    }

    public IEnumerator DestroySelf(Vector3 position)
    {
        cardImage.raycastTarget = false;
        float shrinkSpeed = 4f * Time.deltaTime;
        while (cardImage.rectTransform.localScale.x > 0.00001)
        {
            cardImage.rectTransform.localScale = new Vector3(Mathf.Max(cardImage.rectTransform.localScale.x - shrinkSpeed, 0f), Mathf.Max(cardImage.rectTransform.localScale.y - shrinkSpeed, 0f), 0f);
            yield return null;
        }
        Destroy(gameObject);
    }

    public class DBCardComparer : Comparer<DB_Card>
    {
        public override int Compare(DB_Card x, DB_Card y)
        {
            int initialResult = x.cardInfo.CompareTo(y.cardInfo);
            if (initialResult != 0) { return  initialResult; }
            return x.GetInstanceID().CompareTo(y.GetInstanceID());
        }
    }
}
