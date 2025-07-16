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
            DragManager.instance.SelectedCard.ResetPower();
        }
        else
        {
            DragManager.instance.SelectedCard.EditPower(powerModifier, critModifier, driveModifier);
        }
        DragManager.instance.ClearSelections();
    }

    public override bool ShowByActionFlag(IEnumerable<CardInfo.ActionFlag> flags)
    {
        return true;
    }
}
