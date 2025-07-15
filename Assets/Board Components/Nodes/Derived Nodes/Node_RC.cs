using System.Collections.Generic;
using UnityEngine;

public class Node_RC : Node_Stack
{
    public override NodeType Type => NodeType.RC;

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

    public override void RecieveCard(Card card, string parameters)
    {
        bool isFromDrag = card.node.Type == NodeType.drag;
        bool isFromRC = card.node.Type == NodeType.RC || (isFromDrag && card.node.PreviousNode.Type == NodeType.RC);
        bool toSoulRC = parameters.Contains("bottom");
        if (isFromRC)
        {
            if (isFromDrag)
            {
                SwapAllCards(card.node, parameters + ",drag");
            }
            else
            {
                SwapAllCards(card.node, parameters);
            }
        }
        else if (!toSoulRC)
        {
            RetireCards();
            base.RecieveCard(card, parameters);
        }
        else
        {
            base.RecieveCard(card, parameters);
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
            CardInfo.ActionFlag.power,
            CardInfo.ActionFlag.soul,
            CardInfo.ActionFlag.botdeck
        };
    }

    public override IEnumerable<CardInfo.ActionFlag> GetSpecialActions()
    {
        return new CardInfo.ActionFlag[]
        {
            CardInfo.ActionFlag.bindFD,
            CardInfo.ActionFlag.gaugeZone,
            CardInfo.ActionFlag.locking,
            CardInfo.ActionFlag.overdress,
            CardInfo.ActionFlag.prison,
            CardInfo.ActionFlag.soulRC
        };
    }

}
