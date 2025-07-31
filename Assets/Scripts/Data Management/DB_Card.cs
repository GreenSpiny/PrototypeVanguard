using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DB_Card : MonoBehaviour, IComparable<DB_Card>, IPointerEnterHandler, IPointerExitHandler
{
    public Image cardImage;
    public RectTransform rectTransform;
    public Vector3 moveTarget = Vector3.zero;
    public CardInfo cardInfo;
    public DB_CardReciever reciever;

    private void Awake()
    {
        cardImage = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        cardImage.color = Color.white;
    }

    public void SetWidth(float width)
    {
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, width / Card.cardWidth);
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
            return comparator;
        }
        return GetInstanceID().CompareTo(other.GetInstanceID());
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
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        transform.position = position;
        cardImage.raycastTarget = false;
        float shrinkSpeed = 4f * Time.deltaTime;
        while (transform.localScale.x > 0.00001)
        {
            transform.localScale = new Vector3(Mathf.Max(transform.localScale.x - shrinkSpeed, 0f), Mathf.Max(transform.localScale.y - shrinkSpeed, 0f), 0f);
            yield return null;
        }
        Destroy(gameObject);
    }
}
