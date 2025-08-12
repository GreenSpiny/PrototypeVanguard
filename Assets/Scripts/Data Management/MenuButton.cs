using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Button button;
    private MenuManager manager;
    public Color color;
    public Sprite sprite;
    public float height;

    private void Awake()
    {
        button = GetComponent<Button>();
    }
    public void Init(MenuManager manager)
    {
        this.manager = manager;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        manager.SetCharacterSprite(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {

    }
}
