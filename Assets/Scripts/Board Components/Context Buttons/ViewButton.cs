using System.Collections.Generic;
using UnityEngine;

public class ViewButton : ContextButton
{
    [SerializeField] int amount;
    protected override void ButtonAction()
    {
        Player activePlayer = DragManager.instance.controllingPlayer;
        Card selectedCard = DragManager.instance.SelectedCard;
        Node selectedNode = DragManager.instance.SelectedNode;
        if (selectedNode == null)
        {
            selectedNode = selectedCard.node;
        }

        if (selectedNode.Type == Node.NodeType.deck)
        {
            GameManager.instance.RequestDisplayCardsRpc(activePlayer.playerIndex, selectedNode.nodeID, amount);
        }
        else
        {
            DragManager.instance.OpenDisplay(activePlayer.playerIndex, selectedNode, amount);
        }

        DragManager.instance.ClearSelections();
    }

    public override bool ShowByActionFlag(IEnumerable<CardInfo.ActionFlag> flags)
    {
        return true;
    }
}
