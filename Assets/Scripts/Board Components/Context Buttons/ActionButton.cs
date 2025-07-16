using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ActionButton : ContextButton
{
    [SerializeField] public CardInfo.ActionFlag actionFlag;
    protected override void ButtonAction()
    {
        Card selectedCard = DragManager.instance.SelectedCard;
        Card hoveredCard = DragManager.instance.HoveredCard;
        Node selectedNode = DragManager.instance.SelectedNode;
        Node hoveredNode = DragManager.instance.HoveredNode;

        switch (actionFlag)
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

    public override bool ShowByActionFlag(IEnumerable<CardInfo.ActionFlag> flags)
    {
        return flags.Contains(actionFlag);
    }

}
