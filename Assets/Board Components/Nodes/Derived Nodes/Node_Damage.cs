using System.Collections.Generic;
using UnityEngine;

public class Node_Damage : Node_Fan
{
    public override NodeType Type => NodeType.damage;

    public override void CardAutoAction(Card clickedCard)
    {
        GameManager.instance.RequestSetOrientationRpc(clickedCard.cardID, !clickedCard.flip, clickedCard.rest);
    }

    public override void NodeAutoAction()
    {
        base.NodeAutoAction();
    }

    public override IEnumerable<CardInfo.ActionFlag> GetDefaultActions()
    {
        return new CardInfo.ActionFlag[]
        {
            CardInfo.ActionFlag.reveal
        };
    }

    public override IEnumerable<CardInfo.ActionFlag> GetSpecialActions()
    {
        return new CardInfo.ActionFlag[]
        {
            
        };
    }

}
