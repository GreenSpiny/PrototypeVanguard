using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class ContextRoot : MonoBehaviour
{
    [SerializeField] private float buttonHeight = 90f;
    private List<ContextButton> ContextButtons = new List<ContextButton>();

    private void Awake()
    {
        foreach (var button in GetComponentsInChildren<ContextButton>(true))
        {
            ContextButtons.Add(button);
        }
    }

    public void DisplayButtons(Vector3 position, IEnumerable<CardInfo.ActionFlag> flags)
    {
        int activeCount = 0;
        foreach (var button in ContextButtons)
        {
            bool active = flags.Contains(button.actionFlag);
            button.gameObject.SetActive(active);
            if (active)
            {
                activeCount++;
            }
        }
        transform.position = new Vector2(position.x, position.y);
        transform.localPosition += new Vector3(0f, activeCount * buttonHeight - buttonHeight / 2f, 0f);
    }

    public void HideAllButtons()
    {
        foreach (var button in ContextButtons)
        {
            button.gameObject.SetActive(false);
        }
    }
}
