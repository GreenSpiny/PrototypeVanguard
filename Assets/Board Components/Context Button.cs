using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ContextButton : MonoBehaviour
{
    [SerializeField] public CardInfo.ActionFlag actionFlag;
    [SerializeField] private Button actionButton;

    private void Awake()
    {
        actionButton = GetComponent<Button>();
    }
}
