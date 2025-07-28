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
    private int lastCardIndex = -1;
    private float currentAlpha;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void DisplayCardInfo(Card card)
    {
        if (lastCardIndex != card.cardInfo.index || lastCardRevealed != card.WasRevealed)
        {
            lastCardIndex = card.cardInfo.index;
            lastCardRevealed = card.WasRevealed;
            if (card.IsPublic(DragManager.instance.controllingPlayer))
            {
                cardDetailUI.InspectCard(card, true);
                if (card.node.previewText)
                {
                    currentAlpha = alphaCeiling;
                    text.text = CardLoader.GetCardInfo(card.cardInfo.index).name;
                }
            }
            else
            {;
                cardDetailUI.InspectCard(card, false);
                currentAlpha = 0;
            }
        }
        canvasGroup.alpha = Mathf.Clamp(currentAlpha, 0, 1);
    }

    private void Update()
    {
        transform.position = new Vector3(Input.mousePosition.x, Input.mousePosition.y + yOffset, 0f);
        Card c = DragManager.instance.HoveredCard;
        Card d = DragManager.instance.DraggedCard;
        Card s = DragManager.instance.SelectedCard;
        if (c != null && d == null && s == null && c.PositionDistance < displayThreshold)
        {
            DisplayCardInfo(c);
            currentAlpha -= fadeSpeed * Time.deltaTime;
        }
        else
        {
            Hide();
        }
    }

    public void Hide()
    {
        currentAlpha = 0;
        canvasGroup.alpha = 0;
        lastCardRevealed = false;
        lastCardIndex = -1;
    }
}
