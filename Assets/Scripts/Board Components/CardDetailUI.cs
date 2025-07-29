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
        float targetHeight = cardImage.rectTransform.rect.width / Card.cardWidth;
        cardImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetHeight);
        Material targetMaterial;

        if (show)
        {
            cardImage.rectTransform.anchoredPosition = Vector2.zero;

            targetMaterial = CardLoader.GetCardImage(card.cardInfo.index);
            cardNameText.text = card.cardInfo.name;
            cardInfoText.text = GenerateCardInfoString(card);
            cardDescriptionText.text = card.cardInfo.effect;
        }
        else
        {
            cardImage.rectTransform.anchoredPosition = new Vector2(cardImage.rectTransform.anchoredPosition.x, parentHeight / 2f);

            targetMaterial = CardLoader.GetDefaultCardBack();
            cardNameText.text = defaultName;
            cardInfoText.text = defaultInfo;
            cardDescriptionText.text = string.Empty;
        }

        Texture2D targetTexture = targetMaterial.mainTexture as Texture2D;
        cardImage.sprite = Sprite.Create(targetTexture, new Rect(0, 0, targetTexture.width, targetTexture.height), Vector2.zero);
        LayoutRebuilder.ForceRebuildLayoutImmediate(cardDescriptionText.rectTransform);
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
