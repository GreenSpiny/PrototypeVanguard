using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.Rendering.GPUSort;

public class ActionButton : ContextButton
{
    [SerializeField] public CardInfo.ActionFlag actionFlag;
    protected override void ButtonAction()
    {
        Player activePlayer = DragManager.instance.controllingPlayer;
        Card selectedCard = DragManager.instance.SelectedCard;
        Node selectedNode = DragManager.instance.SelectedNode;
        if (selectedNode == null)
        {
            selectedNode = selectedCard.node;
        }

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
                GameManager.instance.RequestRevealCardRpc(selectedCard.cardID, 1f);
                DragManager.instance.ClearSelections();
                break;
            case CardInfo.ActionFlag.search:
                GameManager.instance.RequestDisplayCardsRpc(activePlayer.playerIndex, selectedCard.node.nodeID, selectedCard.node.cards.Count, false, true);
                DragManager.instance.ClearSelections();
                break;
            case CardInfo.ActionFlag.view:
                DragManager.instance.OpenDisplay(activePlayer.playerIndex, selectedCard.node, selectedCard.node.cards.Count, false, true);
                DragManager.instance.ClearSelections();
                break;
            case CardInfo.ActionFlag.viewx:
                DragManager.instance.viewContext.DisplayButtons(Input.mousePosition, new CardInfo.ActionFlag[] { CardInfo.ActionFlag.view });                
                break;
            case CardInfo.ActionFlag.revealx:
                DragManager.instance.viewContext.DisplayButtons(Input.mousePosition, new CardInfo.ActionFlag[] { CardInfo.ActionFlag.reveal });
                break;
            case CardInfo.ActionFlag.ride:
                GameManager.instance.RequestRecieveCardRpc(selectedCard.player.VC.nodeID, selectedCard.cardID, string.Empty);
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
