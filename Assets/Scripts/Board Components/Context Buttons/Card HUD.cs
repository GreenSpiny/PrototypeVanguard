using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

public class CardHUD : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    [SerializeField] private float alpha;
    [SerializeField] private float yOffset;
    [SerializeField] private float alphaCeiling;
    [SerializeField] private float fadeSpeed;
    [SerializeField] private float displayThreshold;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private CardDetailUI cardDetailUI;
    private bool lastCardRevealed = false;
    private Card lastCardViewed = null;
    private float currentAlpha;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0;
    }

    public void DisplayCardInfo(Card card)
    {
        if (lastCardViewed != card || lastCardRevealed != card.WasRevealed)
        {
            lastCardViewed = card;
            lastCardRevealed = card.WasRevealed;
            if (card.IsPublic(DragManager.instance.controllingPlayer))
            {
                cardDetailUI.InspectCard(card.cardInfo);
                if (card.node.previewText)
                {
                    currentAlpha = alphaCeiling;
                    text.text = CardLoader.GetCardInfo(card.cardInfo.index).name;
                }
            }
            else
            {
                cardDetailUI.InspectCard(null);
                currentAlpha = 0;
            }
        }
    }

    private void Update()
    {
        transform.position = new Vector3(Input.mousePosition.x, Input.mousePosition.y + yOffset, 0f);
        Card h = DragManager.instance.HoveredCard;
        Card d = DragManager.instance.DraggedCard;
        if (d != null)
        {
            DisplayCardInfo(d);
            currentAlpha -= fadeSpeed * Time.deltaTime;
        }
        else if (h != null)
        {
            DisplayCardInfo(h);
            currentAlpha -= fadeSpeed * Time.deltaTime;
        }
        else
        {
            currentAlpha = 0;
            lastCardRevealed = false;
            lastCardViewed = null;
        }
        canvasGroup.alpha = Mathf.Clamp(currentAlpha, 0, 1);
    }

}
