using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class ContextButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Button actionButton;

    private void Awake()
    {
        actionButton = GetComponent<Button>();
        actionButton.onClick.AddListener(ButtonAction);
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

    protected abstract void ButtonAction();

    public abstract bool ShowByActionFlag(IEnumerable<CardInfo.ActionFlag> flags);

}
