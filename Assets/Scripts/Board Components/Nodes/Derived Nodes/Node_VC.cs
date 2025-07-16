using System.Collections.Generic;
using UnityEngine;

public class Node_VC : Node_Stack
{
    public override NodeType Type => NodeType.VC;

    public override void CardAutoAction(Card clickedCard)
    {
        if (clickedCard.flip)
        {
            GameManager.instance.RequestSetOrientationRpc(clickedCard.cardID, !clickedCard.flip, clickedCard.rest);
        }
        else
        {
            GameManager.instance.RequestSetOrientationRpc(clickedCard.cardID, clickedCard.flip, !clickedCard.rest);
        }
    }

    public override void NodeAutoAction()
    {
        base.NodeAutoAction();
    }

    public override IEnumerable<CardInfo.ActionFlag> GetDefaultActions()
    {
        return new CardInfo.ActionFlag[]
        {
            CardInfo.ActionFlag.power
        };
    }

    public override IEnumerable<CardInfo.ActionFlag> GetSpecialActions()
    {
        return new CardInfo.ActionFlag[]
        {

        };
    }

}
