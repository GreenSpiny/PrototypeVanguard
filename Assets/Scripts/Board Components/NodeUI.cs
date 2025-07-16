using System;
using TMPro;
using UnityEngine;

public class NodeUI : MonoBehaviour
{
    [SerializeField] private bool displayPower;
    [SerializeField] private bool displayCount;
    [SerializeField] private bool displayName;

    [SerializeField] RectTransform rootTranform;    // Transform governing all text
    [SerializeField] TextMeshProUGUI powerText;     // Unit power
    [SerializeField] TextMeshProUGUI criticalText;  // Unit critical
    [SerializeField] TextMeshProUGUI driveText;     // Unit drive
    [SerializeField] TextMeshProUGUI countText;     // Number of cards in the stack / fan
    [SerializeField] TextMeshProUGUI nameText;      // Name of the node, if applicable

    private CanvasGroup canvasGroup;
    private int targetAlpha;

    private int currentPower;
    private int targetPower;

    private float PowerAnimationSpeed { get { return 100f * Time.deltaTime; } }
    private float FadeAnimationSpeed { get { return 10f * Time.deltaTime; } }
    private float PulseTime { get { return 0.5f; } }

    private Node node;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0;
        targetAlpha = 0;
    }
    public void Init(Node node)
    {
        this.node = node;
    }

    public void Refresh(float verticalOffset)
    {
        // Set vertical offset
        rootTranform.localPosition = new Vector3(rootTranform.localPosition.x, rootTranform.localPosition.y, -verticalOffset);

        // Set power
        powerText.gameObject.SetActive(displayPower);
        if (displayPower)
        {
            if (node.HasCard)
            {
                CardInfo cardInfo = node.cards[0].cardInfo;
                if (cardInfo != null)
                {
                    powerText.text = Convert.ToString(cardInfo.power);
                    criticalText.text = Convert.ToString(cardInfo.crit);
                    driveText.text = Convert.ToString(cardInfo.drive);
                }
                targetAlpha = 1;
            }
            else
            {
                targetAlpha = 0;
            }
        }

        // Set count
        countText.gameObject.SetActive(displayCount);
        if (displayCount)
        {
            countText.text = Convert.ToString(node.cards.Count);
        }

        // Set name
        nameText.gameObject.SetActive(displayName && !node.HasCard);
    }
    public void Animate()
    {
        if (targetAlpha == 0 && canvasGroup.alpha > 0)
        {
            canvasGroup.alpha = Math.Clamp(canvasGroup.alpha - FadeAnimationSpeed, 0f, 1f);
        }
        else if (targetAlpha == 1 && canvasGroup.alpha < 1)
        {
            canvasGroup.alpha = Math.Clamp(canvasGroup.alpha + FadeAnimationSpeed, 0f, 1f);
        }
    }

}
