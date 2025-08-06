using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Button button;
    private MenuManager manager;
    [SerializeField] private Color color;
    [SerializeField] private Sprite sprite;

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
        manager.SetCharacterSprite(color, sprite);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        
    }
}
