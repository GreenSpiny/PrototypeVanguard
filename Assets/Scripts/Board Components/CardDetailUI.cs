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
    [SerializeField] private TextMeshProUGUI cardInfoText1;
    [SerializeField] private TextMeshProUGUI cardInfoText2;
    [SerializeField] private TextMeshProUGUI cardInfoText3;
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
            GenerateCardInfoStrings(cardInfo);
            cardDescriptionText.text = cardInfo.effect;
        }
        else
        {
            cardImage.rectTransform.anchoredPosition = new Vector2(cardImage.rectTransform.anchoredPosition.x, (Mathf.Abs(imageContainerRect.rect.height - targetHeight) - imageContainer.padding.w) / 2f);
            targetMaterial = CardLoader.GetDefaultCardBack();
            cardNameText.text = defaultName;
            cardInfoText1.text = "-";
            cardInfoText2.text = "-";
            cardInfoText3.text = "-";
            cardDescriptionText.text = string.Empty;
        }

        Texture2D targetTexture = targetMaterial.mainTexture as Texture2D;
        cardImage.sprite = Sprite.Create(targetTexture, new Rect(0, 0, targetTexture.width, targetTexture.height), Vector2.zero);
        LayoutRebuilder.ForceRebuildLayoutImmediate(cardDescriptionText.rectTransform);
    }

    private void GenerateCardInfoStrings(CardInfo cardInfo)
    {
        string cardInfoString1 = string.Empty;
        cardInfoString1 += "G" + cardInfo.grade.ToString();
        if (!cardInfo.isOrder)
        {
            cardInfoString1 += " / " + cardInfo.basePower.ToString();
            cardInfoString1 += " / " + cardInfo.baseShield.ToString();
            cardInfoString1 += " / " + cardInfo.baseCrit.ToString() + "C";
        }
        cardInfoText1.text = cardInfoString1;

        string cardInfoString2 = string.Empty;
        cardInfoString2 += cardInfo.unitType;
        if (cardInfo.skills != null && cardInfo.skills.Count() > 0)
        {
            foreach (string skill in cardInfo.skills)
            {
                if (!string.IsNullOrWhiteSpace(skill))
                {
                    cardInfoString2 += " / " + skill;
                }
            }
        }
        cardInfoText2.text = cardInfoString2;

        string cardInfoString3 = string.Empty;
        for (int i = 0; i < cardInfo.nation.Length; i++)
        {
            if (i != 0)
            {
                cardInfoString3 += " | ";
            }
            cardInfoString3 += cardInfo.nation[i];
        }
        foreach (string race in cardInfo.race)
        {
            cardInfoString3 += " / " + race;
        }
        if (!string.IsNullOrWhiteSpace(cardInfo.group))
        {
            cardInfoString3 += " / " + cardInfo.group;
        }
        cardInfoText3.text = cardInfoString3;
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
