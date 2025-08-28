using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ViewButton : ContextButton
{
    [SerializeField] int amount;
    [SerializeField] CardInfo.ActionFlag actionFlag;
    protected override void ButtonAction()
    {
        Player activePlayer = DragManager.instance.controllingPlayer;
        Card selectedCard = DragManager.instance.SelectedCard;
        Node selectedNode = DragManager.instance.SelectedNode;
        if (selectedNode == null)
        {
            selectedNode = selectedCard.node;
        }

        if (selectedNode.HasCard)
        {
            if (selectedNode.Type == Node.NodeType.deck)
            {
                GameManager.instance.RequestDisplayCardsRpc(activePlayer.playerIndex, selectedNode.nodeID, amount, actionFlag == CardInfo.ActionFlag.reveal, false);
            }
            else
            {
                DragManager.instance.OpenDisplay(activePlayer.playerIndex, selectedNode, amount, actionFlag == CardInfo.ActionFlag.reveal, false);
            }
        }

        DragManager.instance.ClearSelections();
    }

    public override bool ShowByActionFlag(IEnumerable<CardInfo.ActionFlag> flags)
    {
        return flags.Contains(actionFlag);
    }
}
