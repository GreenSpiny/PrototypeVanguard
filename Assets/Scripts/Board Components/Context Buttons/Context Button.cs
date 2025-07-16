using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ContextButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] public CardInfo.ActionFlag actionFlag;
    public LayoutElement layoutElement;
    private Button actionButton;

    private void Awake()
    {
        layoutElement = GetComponent<LayoutElement>();
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

    void ButtonAction()
    {
        Card selectedCard = DragManager.instance.SelectedCard;
        Card hoveredCard = DragManager.instance.HoveredCard;
        Node selectedNode = DragManager.instance.SelectedNode;
        Node hoveredNode = DragManager.instance.HoveredNode;

        switch(actionFlag)
        {
            case CardInfo.ActionFlag.power:
                DragManager.instance.powerContext.DisplayButtons(Input.mousePosition, null);
                break;
            case CardInfo.ActionFlag.soul:
                GameManager.instance.RequestRecieveCardRpc(selectedCard.player.VC.nodeID, selectedCard.cardID, "bottom");
                DragManager.instance.ClearSelections();
                break;
            case CardInfo.ActionFlag.botdeck:
                GameManager.instance.RequestRecieveCardRpc(selectedCard.player.deck.nodeID, selectedCard.cardID, "bottom");
                DragManager.instance.ClearSelections();
                break;
            case CardInfo.ActionFlag.reveal:
                DragManager.instance.ClearSelections();
                break;
            case CardInfo.ActionFlag.view:
                DragManager.instance.ClearSelections();
                break;
            case CardInfo.ActionFlag.viewx:
                DragManager.instance.ClearSelections();
                break;
            case CardInfo.ActionFlag.revealx:
                DragManager.instance.ClearSelections();
                break;
            case CardInfo.ActionFlag.armLeft:
                DragManager.instance.ClearSelections();
                break;
            case CardInfo.ActionFlag.armRight:
                DragManager.instance.ClearSelections();
                break;
            case CardInfo.ActionFlag.bindFD:
                DragManager.instance.ClearSelections();
                break;
            case CardInfo.ActionFlag.locking:
                DragManager.instance.ClearSelections();
                break;
            case CardInfo.ActionFlag.overdress:
                DragManager.instance.ClearSelections();
                break;
            case CardInfo.ActionFlag.prison:
                DragManager.instance.ClearSelections();
                break;
            case CardInfo.ActionFlag.soulRC:
                DragManager.instance.ClearSelections();
                break;
            default:
                DragManager.instance.ClearSelections();
                break;
        }

    }
}
