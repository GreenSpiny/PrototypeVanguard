using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ContextButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] public CardInfo.ActionFlag actionFlag;
    private Button actionButton;
    public LayoutElement layoutElement;

    private void Awake()
    {
        actionButton = GetComponent<Button>();
        layoutElement = GetComponent<LayoutElement>();
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        if (DragManager.instance != null)
        {
            DragManager.instance.HoveredButton = this;
        }
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        if (DragManager.instance != null && DragManager.instance.HoveredButton == this)
        {
            DragManager.instance.HoveredButton = null;
        }
    }
}
