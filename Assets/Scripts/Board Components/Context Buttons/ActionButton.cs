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
                GameManager.instance.RequestReceiveCardRpc(selectedCard.player.VC.nodeID, selectedCard.cardID, Node.par_bottom);
                DragManager.instance.ClearSelections();
                break;
            case CardInfo.ActionFlag.botdeck:
                GameManager.instance.RequestReceiveCardRpc(selectedCard.player.deck.nodeID, selectedCard.cardID, Node.par_bottom);
                DragManager.instance.ClearSelections();
                break;
            case CardInfo.ActionFlag.reveal:
                if (selectedNode.Type == Node.NodeType.hand)
                    GameManager.instance.RequestRevealCardRpc(selectedCard.cardID, 1f);
                else
                    GameManager.instance.RequestRevealCardRpc(selectedCard.cardID, float.MaxValue);
                DragManager.instance.ClearSelections();
                break;
            case CardInfo.ActionFlag.view:
                DragManager.instance.OpenDisplay(activePlayer.playerIndex, selectedCard.node, 0, selectedCard.node.cards.Count, false, true);
                DragManager.instance.ClearSelections();
                break;
            case CardInfo.ActionFlag.viewx:
                DragManager.instance.viewContext.DisplayButtons(Input.mousePosition, new CardInfo.ActionFlag[] { CardInfo.ActionFlag.view });                
                break;
            case CardInfo.ActionFlag.revealx:
                DragManager.instance.viewContext.DisplayButtons(Input.mousePosition, new CardInfo.ActionFlag[] { CardInfo.ActionFlag.reveal });
                break;
            case CardInfo.ActionFlag.search:
                GameManager.instance.RequestDisplayCardsRpc(activePlayer.playerIndex, selectedCard.node.nodeID, 0, selectedCard.node.cards.Count, false, true);
                DragManager.instance.ClearSelections();
                break;
            case CardInfo.ActionFlag.shuffle:
                GameManager.instance.RequestShuffleCardsRpc(selectedCard.node.nodeID, RandomUtility.RandInt());
                DragManager.instance.ClearSelections();
                break;
            case CardInfo.ActionFlag.viewsoul:
                DragManager.instance.OpenDisplay(activePlayer.playerIndex, selectedCard.node, 1, selectedCard.node.cards.Count - 1, false, true);
                DragManager.instance.ClearSelections();
                break;
            case CardInfo.ActionFlag.armLeft:
                GameManager.instance.RequestArmRpc(activePlayer.VC.nodeID, selectedCard.cardID, 1);
                DragManager.instance.ClearSelections();
                break;
            case CardInfo.ActionFlag.armRight:
                GameManager.instance.RequestArmRpc(activePlayer.VC.nodeID, selectedCard.cardID, 0);
                DragManager.instance.ClearSelections();
                break;
            case CardInfo.ActionFlag.bindFD:
                GameManager.instance.RequestReceiveCardRpc(selectedCard.player.bind.nodeID, selectedCard.cardID, Node.par_facedown);
                DragManager.instance.ClearSelections();
                break;
            case CardInfo.ActionFlag.locking:
                GameManager.instance.RequestSetOrientationRpc(selectedCard.cardID, true, false);
                DragManager.instance.ClearSelections();
                break;
            case CardInfo.ActionFlag.rideRC:
                DragManager.instance.ClearSelections();
                break;
            case CardInfo.ActionFlag.soulRC:
                DragManager.instance.ClearSelections();
                break;
            case CardInfo.ActionFlag.prison:
                GameManager.instance.RequestReceiveCardRpc(GameManager.instance.NextPlayer(activePlayer).order.nodeID, selectedCard.cardID, string.Empty);
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
