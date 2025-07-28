using UnityEngine;
using UnityEngine.UI;

public class ToggleButton : MonoBehaviour
{
    [SerializeField] private GameObject[] toggleObjects;
    [SerializeField] private bool on;

    private void Awake()
    {
        Refresh();
    }

    public void Toggle()
    {
        on = !on;
        Refresh();
    }

    private void Refresh()
    {
        foreach (GameObject obj in  toggleObjects)
        {
            obj.SetActive(on);
        }
    }
}
