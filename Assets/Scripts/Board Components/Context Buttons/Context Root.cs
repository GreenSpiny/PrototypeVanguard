using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class ContextRoot : MonoBehaviour
{
    [SerializeField] private float buttonHeight = 90f;
    [SerializeField] ContextRoot[] otherContexts;
    [SerializeField] bool fixedHeight;
    private List<ContextButton> ContextButtons = new List<ContextButton>();

    private void Awake()
    {
        foreach (var button in GetComponentsInChildren<ContextButton>(true))
        {
            ContextButtons.Add(button);
        }
    }

    // need to fix this up to more eloquently account for the two types of context menu
    public void DisplayButtons(Vector3 position, IEnumerable<CardInfo.ActionFlag> flags)
    {
        bool showAll = flags == null;
        foreach (var other in otherContexts)
        {
            other.HideAllButtons();
        }
        int activeCount = 0;
        foreach (var button in ContextButtons)
        {
            if (showAll)
            {
                button.gameObject.SetActive(true);
            }
            else
            {
                bool active = button.ShowByActionFlag(flags);
                button.gameObject.SetActive(active);
                if (active)
                {
                    activeCount++;
                }
            }
        }

        transform.position = new Vector2(position.x, position.y);
        if (!fixedHeight)
        {
            float top = transform.position.y;
            float bottom = transform.position.y - activeCount * buttonHeight;
            transform.localPosition += new Vector3(0f, activeCount * buttonHeight - buttonHeight / 2f, 0f);
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
