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

    public override void ReceiveCard(Card card, string parameters)
    {
        base.ReceiveCard(card, parameters);
    }

    public override void NodeAutoAction()
    {
        base.NodeAutoAction();
    }

    public override List<CardInfo.ActionFlag> GenerateDefaultCardActions()
    {
        List<CardInfo.ActionFlag> toReturn = new List<CardInfo.ActionFlag>()
        {
            CardInfo.ActionFlag.power
        };
        if (cards.Count > 1)
        {
            toReturn.Add(CardInfo.ActionFlag.viewsoul);
        }
        return toReturn;
    }

}
