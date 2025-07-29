using UnityEngine;
using UnityEngine.UI;

public class DB_Card : MonoBehaviour
{
    Image cardImage;
    RectTransform rectTransform;
    public Vector3 moveTarget = Vector3.zero;

    private void Awake()
    {
        cardImage = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
    }

    public void SetWidth(float width)
    {
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
    }
    public void SetHeight(float height)
    {
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
    }
}
