using System.Collections.Generic;
using UnityEngine;

public class Node_Trigger : Node_Fan
{
    public override NodeType Type => NodeType.trigger;

    public override void CardAutoAction(Card clickedCard)
    {
        GameManager.instance.RequestRecieveCardRpc(clickedCard.player.hand.nodeID, clickedCard.cardID, string.Empty);
    }

    public override void NodeAutoAction()
    {
        base.NodeAutoAction();
    }

    public override IEnumerable<CardInfo.ActionFlag> GetDefaultActions()
    {
        return new CardInfo.ActionFlag[]
        {

        };
    }

    public override IEnumerable<CardInfo.ActionFlag> GetSpecialActions()
    {
        return new CardInfo.ActionFlag[]
        {

        };
    }

}
