using System.Collections.Generic;
using UnityEngine;

public class PowerButton : ContextButton
{
    [SerializeField] bool resetPower;
    [SerializeField] int powerModifier;
    [SerializeField] int critModifier;
    [SerializeField] int driveModifier;

    protected override void ButtonAction()
    {
        if (resetPower)
        {
            GameManager.instance.RequestResetPowerRpc(DragManager.instance.SelectedCard.cardID);
        }
        else
        {
            GameManager.instance.RequestEditPowerRpc(DragManager.instance.SelectedCard.cardID, powerModifier, critModifier, driveModifier);
        }
        DragManager.instance.ClearSelections();
    }

    public override bool ShowByActionFlag(IEnumerable<CardInfo.ActionFlag> flags)
    {
        return true;
    }
}
