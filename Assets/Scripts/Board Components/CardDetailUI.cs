using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardDetailUI : MonoBehaviour
{
    [SerializeField] public Card inspectedCard;

    // UI components
    [SerializeField] private RectMask2D imageContainer;
    [SerializeField] private Image cardImage;
    [SerializeField] private Image imageArea;
    [SerializeField] private TextMeshProUGUI cardNameText;
    [SerializeField] private TextMeshProUGUI cardInfoText;
    [SerializeField] private TextMeshProUGUI cardDescriptionText;

    [SerializeField] private GameObject actionLogLabel;
    [SerializeField] private GameObject[] actionLogElements;

    [SerializeField] private GameObject chatLogLabel;
    [SerializeField] private GameObject[] chatLogElements;

    private string defaultName;
    private string defaultInfo;
    private RectTransform imageContainerRect;

    private void Awake()
    {
        imageContainerRect = imageContainer.GetComponent<RectTransform>();
        defaultName = cardNameText.text;
        defaultInfo = cardInfoText.text;
    }
    public void InspectCard(CardInfo cardInfo)
    {
        float targetHeight;
        if (cardInfo == null || !cardInfo.rotate)
        {
            targetHeight = cardImage.rectTransform.rect.width / Card.cardWidth;
        }
        else
        {
            targetHeight = cardImage.rectTransform.rect.width * Card.cardWidth;
        }
        cardImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetHeight);
        Material targetMaterial;

        if (cardInfo != null)
        {
            cardImage.rectTransform.anchoredPosition = Vector2.zero;

            targetMaterial = CardLoader.GetCardImage(cardInfo.index);
            cardNameText.text = cardInfo.name;
            cardInfoText.text = GenerateCardInfoString(cardInfo);
            cardDescriptionText.text = cardInfo.effect;
        }
        else
        {
            cardImage.rectTransform.anchoredPosition = new Vector2(cardImage.rectTransform.anchoredPosition.x, (Mathf.Abs(imageContainerRect.rect.height - targetHeight) - imageContainer.padding.w) / 2f);
            targetMaterial = CardLoader.GetDefaultCardBack();
            cardNameText.text = defaultName;
            cardInfoText.text = defaultInfo;
            cardDescriptionText.text = string.Empty;
        }

        Texture2D targetTexture = targetMaterial.mainTexture as Texture2D;
        cardImage.sprite = Sprite.Create(targetTexture, new Rect(0, 0, targetTexture.width, targetTexture.height), Vector2.zero);
        LayoutRebuilder.ForceRebuildLayoutImmediate(cardDescriptionText.rectTransform);
    }

    private string GenerateCardInfoString(CardInfo cardInfo)
    {
        string cardInfoString = string.Empty;
        cardInfoString += "G" + cardInfo.grade.ToString() + " / ";
        cardInfoString += cardInfo.basePower.ToString() + " / ";
        cardInfoString += cardInfo.baseShield.ToString() + " / ";
        cardInfoString += cardInfo.baseCrit.ToString() + "C";
        cardInfoString += "\n" + cardInfo.unitType;
        if (cardInfo.skills != null && cardInfo.skills.Count() > 0)
        {
            foreach (string skill in cardInfo.skills)
            {
                if (!string.IsNullOrEmpty(skill))
                {
                    cardInfoString += " / " + skill;
                }
            }
        }
        cardInfoString += "\n";
        for (int i = 0; i < cardInfo.nation.Length; i++)
        {
            if (i != 0)
            {
                cardInfoString += " | ";
            }
            cardInfoString += cardInfo.nation[i];
        }
        foreach (string race in cardInfo.race)
        {
            cardInfoString += " / " + race;
        }
        if (!string.IsNullOrEmpty(cardInfo.group))
        {
            cardInfoString += " / " + cardInfo.group;
        }
        return cardInfoString;
    }

    public void DisableActionLog()
    {
        actionLogLabel.gameObject.SetActive(false);
        ToggleActionLog(false);
    }

    public void ToggleActionLog(bool toggle)
    {
        foreach (GameObject actionObject in actionLogElements)
        {
            actionObject.gameObject.SetActive(toggle);
        }
    }

    public void DisableChat()
    {
        chatLogLabel.gameObject.SetActive(false);
        ToggleChat(false);
    }

    public void ToggleChat(bool toggle)
    {
        foreach (GameObject chatObject in chatLogElements)
        {
            chatObject.gameObject.SetActive(toggle);
        }
    }


}
