using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ContextRoot : MonoBehaviour
{
    List<ContextButton> ContextButtons = new List<ContextButton>();

    private void Awake()
    {
        foreach (var button in GetComponentsInChildren<ContextButton>(true))
        {
            ContextButtons.Add(button);
        }
    }

    public void DisplayButtons(IEnumerable<CardInfo.ActionFlag> flags)
    {
        foreach (var button in ContextButtons)
        {
            button.gameObject.SetActive(flags.Contains(button.actionFlag));
        }
    }

    public void HideAllButtons()
    {
        foreach (var button in ContextButtons)
        {
            button.gameObject.SetActive(false);
        }
    }
}
