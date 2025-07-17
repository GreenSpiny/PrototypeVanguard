using UnityEngine;
using UnityEngine.UI;

public class CardDetailUI : MonoBehaviour
{
    [SerializeField] public Card inspectedCard;

    // UI components
    [SerializeField] private RectTransform imageContainer;
    [SerializeField] private Image cardImage;

    private float parentHeight;
    private void Awake()
    {
        parentHeight = imageContainer.rect.height;
    }
    public void InspectCard(Card card)
    {
        if (inspectedCard != card)
        {
            inspectedCard = card;
            
            // Set the card image
            Texture2D targetTexture = card.GetTexture() as Texture2D;
            float targetHeight = cardImage.rectTransform.rect.width / Card.cardWidth;
            cardImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetHeight);
            cardImage.sprite = Sprite.Create(targetTexture, new Rect(0, 0, targetTexture.width, targetTexture.height), Vector2.zero);

            // Move the image upward to be centered in the frame
            float squarePictureHeight = targetHeight * Card.cardWidth;
            float calculatedPosition = squarePictureHeight * 0.5f - parentHeight * 0.5f;
            cardImage.rectTransform.anchoredPosition = new Vector2(cardImage.rectTransform.anchoredPosition.x, calculatedPosition);


            
        }
    }



}
