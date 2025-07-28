using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardDetailUI : MonoBehaviour
{
    [SerializeField] public Card inspectedCard;

    // UI components
    [SerializeField] private RectTransform imageContainer;
    [SerializeField] private Image cardImage;
    [SerializeField] private Image imageArea;
    [SerializeField] private TextMeshProUGUI cardNameText;
    [SerializeField] private TextMeshProUGUI cardInfoText;
    [SerializeField] private TextMeshProUGUI cardDescriptionText;

    private string defaultName;
    private string defaultInfo;

    private float parentHeight;
    private void Awake()
    {
        parentHeight = imageContainer.rect.height;
        defaultName = cardNameText.text;
        defaultInfo = cardInfoText.text;
    }
    public void InspectCard(Card card, bool show)
    {
        if (show)
        {
            // Set the card image
            Texture2D targetTexture = card.GetTexture() as Texture2D;
            float targetHeight = cardImage.rectTransform.rect.width / Card.cardWidth;
            cardImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetHeight);
            cardImage.color = Color.white;
            cardImage.sprite = Sprite.Create(targetTexture, new Rect(0, 0, targetTexture.width, targetTexture.height), Vector2.zero);

            // Move the image upward to be centered in the frame
            /*
            float squarePictureHeight = targetHeight * Card.cardWidth;
            float calculatedPosition = squarePictureHeight * 0.5f - parentHeight * 0.5f;
            cardImage.rectTransform.anchoredPosition = new Vector2(cardImage.rectTransform.anchoredPosition.x, calculatedPosition);
            */

            // Set the card info and text
            cardNameText.text = card.cardInfo.name;
            cardInfoText.text = GenerateCardInfoString(card);
            cardDescriptionText.text = card.cardInfo.effect;
            LayoutRebuilder.ForceRebuildLayoutImmediate(cardDescriptionText.rectTransform);
        }
        else
        {
            cardImage.color = imageArea.color;
            cardImage.sprite = null;
            cardNameText.text = defaultName;
            cardInfoText.text = defaultInfo;
            cardDescriptionText.text = string.Empty;
            LayoutRebuilder.ForceRebuildLayoutImmediate(cardDescriptionText.rectTransform);
        }
    }

    private string GenerateCardInfoString(Card card)
    {
        string cardInfoString = string.Empty;
        cardInfoString += "G" + card.cardInfo.grade.ToString() + " / ";
        cardInfoString += card.cardInfo.basePower.ToString() + " / ";
        cardInfoString += card.cardInfo.baseShield.ToString() + " / ";
        cardInfoString += card.cardInfo.baseCrit.ToString() + "C";
        cardInfoString += "\n" + card.cardInfo.unitType;
        if (card.cardInfo.skills != null && card.cardInfo.skills.Count() > 0)
        {
            foreach (string skill in card.cardInfo.skills)
            {
                if (!string.IsNullOrEmpty(skill))
                {
                    cardInfoString += " / " + skill;
                }
            }
        }
        cardInfoString += "\n" + card.cardInfo.nation + " / ";
        cardInfoString += card.cardInfo.race;
        if (!string.IsNullOrEmpty(card.cardInfo.group))
        {
            cardInfoString += " / " + card.cardInfo.group;
        }
        return cardInfoString;
    }


}
