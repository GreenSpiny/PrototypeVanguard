using System.Collections.Generic;
using UnityEngine;

public class Node_Marker : Node_Fan
{
    public override NodeType Type => NodeType.marker;

    public override void CardAutoAction(Player player, Card clickedCard)
    {
        if (GameManager.singlePlayer || this.player == player)
        {
            GameManager.instance.RequestReceiveCardRpc(clickedCard.player.drop.nodeID, clickedCard.cardID, string.Empty);
        }
    }

    public override void NodeAutoAction()
    {
        base.NodeAutoAction();
    }

    public override List<CardInfo.ActionFlag> GenerateDefaultCardActions()
    {
        List<CardInfo.ActionFlag> toReturn = new List<CardInfo.ActionFlag>()
        {

        };
        return toReturn;
    }

    public override void AlignCards(bool instant)
    {
        base.AlignCards(instant);
        if (DragManager.instance != null && DragManager.instance.controllingPlayer != null)
        {
            Player player = DragManager.instance.controllingPlayer;
            transform.localRotation = Quaternion.Euler(0f, 180f * player.playerIndex, 0f);
            reverse = player.playerIndex == 1;
        }
    }
}
